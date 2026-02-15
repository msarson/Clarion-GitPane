using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Sda;

namespace ICSharpCode.SharpDevelop;

public class SharpDevelopMain
{
	private const int SW_RESTORE = 9;

	private static string[] commandLineArgs = null;

	private static string configurationFolder;

	private static Mutex mutex;

	public static string[] CommandLineArgs => commandLineArgs;

	[STAThread]
	public static void Main(string[] args)
	{
		try
		{
			Run(args);
		}
		catch (Exception ex)
		{
			try
			{
				HandleMainException(ex);
			}
			catch (Exception ex2)
			{
				MessageBox.Show(ex2.ToString(), "Critical error (Logging service defect?)");
			}
		}
	}

	private static void HandleMainException(Exception ex)
	{
		LoggingService.Fatal(ex);
		try
		{
			Application.Run(new ExceptionBox(ex, "Unhandled exception terminated Clarion", mustTerminate: true));
		}
		catch
		{
			MessageBox.Show(ex.ToString(), "Critical error (cannot use ExceptionBox)");
		}
	}

	private static void Run(string[] args)
	{
		commandLineArgs = args;
		bool flag = false;
		bool flag2 = false;
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		SplashScreenForm.SetCommandLineArgs(args);
		string[] parameterList = SplashScreenForm.GetParameterList();
		foreach (string text in parameterList)
		{
			if (text.Equals("nologo", StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
			}
			else if (!flag2 && text.StartsWith("ConfigDir=", StringComparison.OrdinalIgnoreCase) && text.Length > 10)
			{
				configurationFolder = text.Substring(10);
				flag2 = true;
			}
			else if (!flag2 && text.Equals("ConfigDir", StringComparison.OrdinalIgnoreCase))
			{
				configurationFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Settings");
				flag2 = true;
			}
		}
		if (!flag)
		{
			SplashScreenForm.ShowSplashScreen();
		}
		try
		{
			RunApplication();
		}
		finally
		{
			if (SplashScreenForm.SplashScreen != null)
			{
				SplashScreenForm.SplashScreen.Dispose();
			}
		}
	}

	private static void RunApplication()
	{
		LoggingService.Info("Starting Clarion...");
		try
		{
			VersionService.Version = IDEVersion.Enterprise;
			StartupSettings startupSettings = new StartupSettings();
			Assembly assembly = typeof(SharpDevelopMain).Assembly;
			startupSettings.ApplicationRootPath = Directory.GetParent(Path.GetDirectoryName(assembly.Location)).FullName;
			startupSettings.ApplicationName = Path.GetFileNameWithoutExtension(assembly.Location);
			startupSettings.ResourceAssemblyName = startupSettings.ApplicationName;
			if (startupSettings.ApplicationRootPath.EndsWith("bin", ignoreCase: true, CultureInfo.CurrentCulture))
			{
				startupSettings.ApplicationRootPath = Directory.GetParent(startupSettings.ApplicationRootPath).FullName;
			}
			startupSettings.AllowUserAddIns = true;
			if (configurationFolder == null)
			{
				configurationFolder = FileUtility.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoftVelocity", "Clarion", "11.0") + Path.DirectorySeparatorChar;
			}
			startupSettings.ConfigDirectory = configurationFolder;
			startupSettings.AddAddInsFromDirectory(FileUtility.Combine(startupSettings.ApplicationRootPath, Path.GetDirectoryName(assembly.Location), "AddIns"));
			startupSettings.AddAddInsFromDirectory(FileUtility.Combine(startupSettings.ApplicationRootPath, "Accessory", "AddIns"));
			SharpDevelopHost sharpDevelopHost = new SharpDevelopHost(AppDomain.CurrentDomain, startupSettings);
			if (!sharpDevelopHost.SupportMultipleInstances && IsAlreadyRunning())
			{
				SwitchToCurrentInstance();
				return;
			}
			string[] requestedFileList = SplashScreenForm.GetRequestedFileList();
			if (requestedFileList.Length > 0 && LoadFilesInPreviousInstance(requestedFileList))
			{
				LoggingService.Info("Aborting startup, arguments will be handled by previous instance");
				return;
			}
			sharpDevelopHost.BeforeRunWorkbench += delegate
			{
				if (SplashScreenForm.SplashScreen != null)
				{
					SplashScreenForm.SplashScreen.BeginInvoke(new MethodInvoker(SplashScreenForm.SplashScreen.Dispose));
					SplashScreenForm.SplashScreen = null;
				}
			};
			WorkbenchSettings workbenchSettings = new WorkbenchSettings();
			workbenchSettings.RunOnNewThread = false;
			workbenchSettings.UseTipOfTheDay = true;
			for (int num = 0; num < requestedFileList.Length; num++)
			{
				workbenchSettings.InitialFileList.Add(requestedFileList[num]);
			}
			if (ClarionLic.IsValid(askForSerial: true))
			{
				sharpDevelopHost.RunWorkbench(workbenchSettings);
			}
		}
		finally
		{
			try
			{
				List<ICommand> list = AddInTree.BuildItems<ICommand>("/Workspace/Terminate", null, throwOnNotFound: false);
				for (int num2 = list.Count; num2 > 0; num2--)
				{
					ICommand command = list[num2 - 1];
					command.Run();
				}
			}
			catch (Exception message)
			{
				LoggingService.Error(message);
			}
			LoggingService.Info("Leaving RunApplication()");
		}
	}

	private static bool LoadFilesInPreviousInstance(string[] fileList)
	{
		try
		{
			foreach (string fileName in fileList)
			{
				if (ProjectService.HasProjectLoader(fileName))
				{
					return false;
				}
			}
			return DefaultWorkbench.SingleInstanceHelper.OpenFilesInPreviousInstance(fileList);
		}
		catch (Exception message)
		{
			LoggingService.Error(message);
			return false;
		}
	}

	[DllImport("user32.dll")]
	private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	private static extern int SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern int IsIconic(IntPtr hWnd);

	private static IntPtr GetCurrentInstanceWindowHandle()
	{
		IntPtr result = IntPtr.Zero;
		Process currentProcess = Process.GetCurrentProcess();
		Process[] processesByName = Process.GetProcessesByName(currentProcess.ProcessName);
		Process[] array = processesByName;
		foreach (Process process in array)
		{
			if (process.Id != currentProcess.Id && process.MainModule.FileName == currentProcess.MainModule.FileName && process.MainWindowHandle != IntPtr.Zero)
			{
				result = process.MainWindowHandle;
				break;
			}
		}
		return result;
	}

	private static void SwitchToCurrentInstance()
	{
		IntPtr currentInstanceWindowHandle = GetCurrentInstanceWindowHandle();
		if (currentInstanceWindowHandle != IntPtr.Zero)
		{
			if (IsIconic(currentInstanceWindowHandle) != 0)
			{
				ShowWindow(currentInstanceWindowHandle, 9);
			}
			SetForegroundWindow(currentInstanceWindowHandle);
		}
	}

	private static bool IsAlreadyRunning()
	{
		string location = Assembly.GetExecutingAssembly().Location;
		FileSystemInfo fileSystemInfo = new FileInfo(location);
		string fullName = fileSystemInfo.FullName;
		fullName = fullName.Replace('\\', '_');
		fullName = fullName.Replace('.', '_');
		fullName = fullName.Replace(':', '_');
		mutex = new Mutex(initiallyOwned: true, "Global\\" + fullName, out var createdNew);
		if (createdNew)
		{
			mutex.ReleaseMutex();
		}
		return !createdNew;
	}
}

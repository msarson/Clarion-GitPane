using System;
using System.Collections.Generic;
using System.IO;

namespace ICSharpCode.Core;

public class CoreStartup
{
	private List<string> addInFiles = new List<string>();

	private List<string> disabledAddIns = new List<string>();

	private string propertiesName;

	private string configDirectory;

	private string dataDirectory;

	private string applicationName;

	public string PropertiesName
	{
		get
		{
			return propertiesName;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				throw new ArgumentNullException("value");
			}
			propertiesName = value;
		}
	}

	public string ConfigDirectory
	{
		get
		{
			return configDirectory;
		}
		set
		{
			configDirectory = value;
		}
	}

	public string DataDirectory
	{
		get
		{
			return dataDirectory;
		}
		set
		{
			dataDirectory = value;
		}
	}

	public CoreStartup(string applicationName)
	{
		if (applicationName == null)
		{
			throw new ArgumentNullException("applicationName");
		}
		this.applicationName = applicationName;
		propertiesName = "ClarionProperties";
		MessageService.DefaultMessageBoxTitle = applicationName;
		MessageService.ProductName = applicationName;
	}

	public void AddAddInsFromDirectory(string addInDir)
	{
		if (addInDir == null)
		{
			throw new ArgumentNullException("addInDir");
		}
		try
		{
			addInFiles.AddRange(FileUtility.SearchDirectory(addInDir, "*.addin"));
		}
		catch (DirectoryNotFoundException)
		{
		}
	}

	public void AddAddInFile(string addInFile)
	{
		if (addInFile == null)
		{
			throw new ArgumentNullException("addInFile");
		}
		addInFiles.Add(addInFile);
	}

	public void ConfigureExternalAddIns(string addInConfigurationFile)
	{
		AddInManager.ConfigurationFileName = addInConfigurationFile;
		AddInManager.LoadAddInConfiguration(addInFiles, disabledAddIns);
	}

	public void ConfigureUserAddIns(string addInInstallTemp, string userAddInPath)
	{
		AddInManager.AddInInstallTemp = addInInstallTemp;
		AddInManager.UserAddInPath = userAddInPath;
		if (Directory.Exists(addInInstallTemp))
		{
			AddInManager.InstallAddIns(disabledAddIns);
		}
		if (Directory.Exists(userAddInPath))
		{
			AddAddInsFromDirectory(userAddInPath);
		}
	}

	public void RunInitialization()
	{
		string text = "";
		try
		{
			AddInTree.Load(addInFiles, disabledAddIns);
			LoggingService.Info("Running autostart commands...");
			foreach (ICommand item in AddInTree.BuildItems<ICommand>("/Workspace/Autostart", null, throwOnNotFound: false))
			{
				text = "\r\nCommand:" + item.GetType().ToString() + "\r\n";
				item.Run();
			}
		}
		catch (FileNotFoundException ex)
		{
			MessageService.WriteLog(ex, "Application Addins Error", "Error running autostart commands for addins.\r\ncommandMessage=" + text + "\r\n");
			MessageService.ShowMessage("Error running autostart commands for addins.\r\nException Type: " + ex.GetType().ToString() + "\r\n\r\n" + ex.Message + "\r\n\r\nVerify that all the required files are properly installed.\r\nThere was an error trying to load the addin.\r\nFile not found name:" + ex.FileName + text, "Application Addins Error");
			throw ex;
		}
		catch (FileLoadException ex2)
		{
			MessageService.WriteLog(ex2, "Application Addins Error", "Error running autostart commands for addins.\r\ncommandMessage=" + text + "\r\n");
			MessageService.ShowMessage("Error running autostart commands for addins.\r\nException Type: " + ex2.GetType().ToString() + "\r\n\r\n" + ex2.Message + "\r\n\r\nVerify that all the required files are properly installed.\r\nThere was an error trying to load the addin some component of the addin was not found.\r\nFile that cound't load name:" + ex2.FileName + text, "Application Addins Error");
			throw ex2;
		}
		catch (Exception ex3)
		{
			if (ex3.InnerException != null)
			{
				MessageService.WriteLog(ex3, "Application Addins Error", "Error running autostart commands for addins.\r\ncommandMessage=" + text + "\r\n");
				if (typeof(FileNotFoundException).IsAssignableFrom(ex3.InnerException.GetType()))
				{
					FileNotFoundException ex4 = (FileNotFoundException)ex3.InnerException;
					MessageService.ShowMessage("Error running autostart commands for addins.\r\nException Type: " + ex3.GetType().ToString() + "\r\n\r\n" + ex3.Message + "\r\n\r\nVerify that all the required files are properly installed.\r\nThere was an error trying to load the addin.\r\nFile not found name:" + ex4.FileName + text, "Application Addins Error");
				}
				else if (typeof(FileLoadException).IsAssignableFrom(ex3.InnerException.GetType()))
				{
					FileLoadException ex5 = (FileLoadException)ex3.InnerException;
					MessageService.ShowMessage("Error running autostart commands for addins.\r\nException Type: " + ex3.GetType().ToString() + "\r\n\r\n" + ex3.Message + "\r\n\r\nVerify that all the required files are properly installed.\r\nThere was an error trying to load the addin some component of the addin was not found.\r\nFile that cound't load name:" + ex5.FileName + text, "Application Addins Error");
				}
			}
			else
			{
				MessageService.ShowMessage(ex3, "Error running autostart commands for addins.\r\nException Type: " + ex3.GetType().ToString() + "\r\n\r\nClarion IDE encountered an error and cannot continue.\r\nVerify that all the required files are properly installed." + text + ex3.Message + "\r\n\r\n", "Application Addins Error");
			}
			throw ex3;
		}
		MenuShortcutService.Save();
		MenuShortcutService.RestoreAutosave();
	}

	public void StartCoreServices()
	{
		if (configDirectory == null)
		{
			configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);
		}
		PropertyService.InitializeService(configDirectory, dataDirectory ?? Path.Combine(FileUtility.ApplicationRootPath, "data"), propertiesName);
		PropertyService.Load();
		MenuShortcutService.Initialize();
		MenuShortcutService.SuspendAutosave();
		MenuShortcutService.Load();
		ResourceService.InitializeService(FileUtility.Combine(PropertyService.DataDirectory, "resources"));
		StringParser.Properties["AppName"] = applicationName;
	}
}

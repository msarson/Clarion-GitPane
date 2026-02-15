using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class FileService
{
	private class LoadFileWrapper
	{
		private IDisplayBinding binding;

		public LoadFileWrapper(IDisplayBinding binding)
		{
			this.binding = binding;
		}

		public void Invoke(string fileName)
		{
			IViewContent viewContent = binding.CreateContentForFile(fileName);
			if (viewContent != null)
			{
				DisplayBindingService.AttachSubWindows(viewContent, isReattaching: false);
				WorkbenchSingleton.Workbench.ShowView(viewContent);
			}
		}
	}

	[Flags]
	public enum FileOperationFlags : ushort
	{
		FOF_SILENT = 4,
		FOF_NOCONFIRMATION = 0x10,
		FOF_ALLOWUNDO = 0x40,
		FOF_SIMPLEPROGRESS = 0x100,
		FOF_NOERRORUI = 0x400,
		FOF_WANTNUKEWARNING = 0x4000
	}

	public enum FileOperationType : uint
	{
		FO_MOVE = 1u,
		FO_COPY,
		FO_DELETE,
		FO_RENAME
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
	private struct SHFILEOPSTRUCT_x86
	{
		public IntPtr hwnd;

		[MarshalAs(UnmanagedType.U4)]
		public FileOperationType wFunc;

		public string pFrom;

		public string pTo;

		public FileOperationFlags fFlags;

		[MarshalAs(UnmanagedType.Bool)]
		public bool fAnyOperationsAborted;

		public IntPtr hNameMappings;

		public string lpszProgressTitle;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct SHFILEOPSTRUCT_x64
	{
		public IntPtr hwnd;

		[MarshalAs(UnmanagedType.U4)]
		public FileOperationType wFunc;

		public string pFrom;

		public string pTo;

		public FileOperationFlags fFlags;

		[MarshalAs(UnmanagedType.Bool)]
		public bool fAnyOperationsAborted;

		public IntPtr hNameMappings;

		public string lpszProgressTitle;
	}

	private const string xmlFile = "RecentOpen.xml";

	private static RecentOpen recentOpen;

	private static string propFile;

	private static FileSystemWatcher propertiesFileWatcher;

	private static bool sendToRecycleBin;

	private static string PropertyFile
	{
		get
		{
			if (propFile == null)
			{
				propFile = Path.Combine(PropertyService.ConfigDirectory, "RecentOpen.xml");
			}
			return propFile;
		}
	}

	public static RecentOpen RecentOpen
	{
		get
		{
			if (recentOpen == null)
			{
				try
				{
					Properties properties = Properties.Load(PropertyFile);
					if (properties == null || properties.Count == 0)
					{
						properties = PropertyService.Get("RecentOpen", new Properties());
					}
					recentOpen = RecentOpen.FromXmlElement(properties);
				}
				catch
				{
					recentOpen = new RecentOpen();
				}
			}
			return recentOpen;
		}
	}

	public static string CurrentDirectory
	{
		get
		{
			IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
			if (activeWorkbenchWindow != null)
			{
				IViewContent viewContent = activeWorkbenchWindow.ViewContent;
				if (!viewContent.IsUntitled && !string.IsNullOrEmpty(viewContent.FileName))
				{
					return GetDirectory(viewContent.FileName);
				}
			}
			IProject currentProject = ProjectService.CurrentProject;
			if (currentProject != null)
			{
				return currentProject.Directory;
			}
			Solution openSolution = ProjectService.OpenSolution;
			if (openSolution != null && !string.IsNullOrEmpty(openSolution.FileName))
			{
				return GetDirectory(openSolution.FileName);
			}
			return Environment.CurrentDirectory;
		}
	}

	public static event EventHandler<FileEventArgs> FileCreated;

	public static event EventHandler<FileRenamingEventArgs> FileRenaming;

	public static event EventHandler<FileRenameEventArgs> FileRenamed;

	public static event EventHandler<FileCancelEventArgs> FileRemoving;

	public static event EventHandler<FileEventArgs> FileRemoved;

	public static event EventHandler<FileCancelEventArgs> FileReplacing;

	public static event EventHandler<FileEventArgs> FileReplaced;

	public static void Unload()
	{
		RecentOpen.ToProperties().Save(PropertyFile);
	}

	static FileService()
	{
		recentOpen = null;
		sendToRecycleBin = true;
		ProjectService.SolutionLoaded += ProjectServiceSolutionLoaded;
		propertiesFileWatcher = new FileSystemWatcher(PropertyService.ConfigDirectory, "RecentOpen.xml");
		propertiesFileWatcher.Changed += PropFileChanged;
		propertiesFileWatcher.EnableRaisingEvents = true;
		RecentOpen.RecentChanged += RecentChanged;
		sendToRecycleBin = PropertyService.Get("DeleteWillSendToRecycleBin", defaultValue: true);
	}

	private static void RecentChanged(object sender, RecentOpenEventArgs e)
	{
		propertiesFileWatcher.EnableRaisingEvents = false;
		Unload();
		propertiesFileWatcher.EnableRaisingEvents = true;
	}

	private static void PropFileChanged(object sender, FileSystemEventArgs e)
	{
		recentOpen = null;
	}

	private static void ProjectServiceSolutionLoaded(object sender, SolutionEventArgs e)
	{
		if (WorkbenchSingleton.MainForm != null)
		{
			RecentOpen.AddLastItem(ProjectService.GetProjectFileCategory(e.Solution.FileName), FileUtility.NormalizePath(e.Solution.FileName), null);
		}
	}

	public static bool CheckFileName(string fileName)
	{
		if (FileUtility.IsValidFileName(fileName))
		{
			return true;
		}
		MessageService.ShowMessage(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.SaveFile.InvalidFileNameError}", new string[1, 2] { { "FileName", fileName } }));
		return false;
	}

	public static bool CheckDirectoryName(string name)
	{
		if (FileUtility.IsValidDirectoryName(name))
		{
			return true;
		}
		MessageService.ShowMessage(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.SaveFile.InvalidFileNameError}", new string[1, 2] { { "FileName", name } }));
		return false;
	}

	public static bool IsOpen(string fileName)
	{
		return GetOpenFile(fileName) != null;
	}

	public static IWorkbenchWindow OpenFile(string fileName)
	{
		LoggingService.Info("Open file " + fileName);
		IWorkbenchWindow workbenchWindow = null;
		try
		{
			workbenchWindow = GetOpenFile(fileName);
			if (workbenchWindow != null)
			{
				workbenchWindow.SelectWindow();
				return workbenchWindow;
			}
			Cursor.Current = Cursors.WaitCursor;
			if (!FileUtility.OnFileLoading(fileName))
			{
				return null;
			}
			IDisplayBinding bindingPerFileName = DisplayBindingService.GetBindingPerFileName(fileName);
			if (bindingPerFileName == null)
			{
				Cursor.Current = Cursors.Default;
				MessageService.ShowMessage("No display codon found for the file type.\r\nThe IDE could not find an Editor for the file.\r\nCheck if the file type is supported and or the installation of the IDE is correct.\r\nCan't open the file.\r\nFile Name:" + fileName, "Error loading the file");
				return null;
			}
			if (FileUtility.ObservedLoad(new LoadFileWrapper(bindingPerFileName).Invoke, fileName) == FileOperationResult.OK)
			{
				RecentOpen.AddLastItem(GetFileCategory(fileName), FileUtility.NormalizePath(fileName), null);
			}
			workbenchWindow = GetOpenFile(fileName);
		}
		catch (ApplicationException ex)
		{
			if (ex is TargetInvocationException || ex is TargetException || ex is TargetParameterCountException)
			{
				MessageService.ShowError(ex, "Error loading the file.\r\nFile Name:" + fileName);
			}
			else
			{
				MessageService.ShowMessage(ex, ex.Message + "\r\nFile Name:" + fileName, "Error loading the file");
			}
		}
		catch (ArgumentException ex2)
		{
			MessageService.ShowMessage(ex2, "The file name is invalid.\r\nFile Name:" + fileName + "\r\n" + ex2.Message, "Error loading the file");
		}
		finally
		{
			Cursor.Current = Cursors.Default;
		}
		return workbenchWindow;
	}

	public static IWorkbenchWindow NewFile(string defaultName, string language, string content, bool openFile)
	{
		IDisplayBinding bindingPerLanguageName = DisplayBindingService.GetBindingPerLanguageName(language);
		if (bindingPerLanguageName != null)
		{
			IViewContent viewContent = bindingPerLanguageName.CreateContentForLanguage(language, content);
			if (viewContent == null)
			{
				LoggingService.Warn(string.Format("Created view content was null{3}DefaultName:{0}{3}Language:{1}{3}Content:{2}", defaultName, language, content, Environment.NewLine));
				return null;
			}
			viewContent.UntitledName = viewContent.GetHashCode() + "/" + defaultName;
			DisplayBindingService.AttachSubWindows(viewContent, isReattaching: false);
			if (openFile)
			{
				WorkbenchSingleton.Workbench.ShowView(viewContent);
			}
			else
			{
				((DefaultWorkbench)WorkbenchSingleton.Workbench).CreateView(viewContent);
			}
			return viewContent.WorkbenchWindow;
		}
		throw new ApplicationException("Can't create display binding for language " + language);
	}

	public static IWorkbenchWindow NewFile(string defaultName, string language, string content)
	{
		return NewFile(defaultName, language, content, openFile: true);
	}

	public static IList<string> GetOpenFiles()
	{
		List<string> list = new List<string>();
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			string text = (item.IsUntitled ? item.UntitledName : item.FileName);
			if (text != null)
			{
				list.Add(text);
			}
		}
		return list;
	}

	public static IViewContent GetOpenFileViewContent(string fileName)
	{
		if (fileName != null && fileName.Length > 0)
		{
			foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				string text = (item.IsUntitled ? item.UntitledName : item.FileName);
				if (text != null && FileUtility.IsEqualFileName(fileName, text))
				{
					return item;
				}
			}
		}
		return null;
	}

	public static IWorkbenchWindow GetOpenFile(string fileName)
	{
		if (fileName != null && fileName.Length > 0)
		{
			foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				string text = (item.IsUntitled ? item.UntitledName : item.FileName);
				if (text != null && FileUtility.IsEqualFileName(fileName, text))
				{
					return item.WorkbenchWindow;
				}
			}
		}
		return null;
	}

	private static string GetDirectory(string fName)
	{
		fName = Path.GetFullPath(fName);
		return Path.Combine(Path.GetPathRoot(fName), Path.GetDirectoryName(fName));
	}

	public static void RemoveFile(string fileName, bool isDirectory)
	{
		FileCancelEventArgs e = new FileCancelEventArgs(fileName, isDirectory);
		OnFileRemoving(e);
		if (e.Cancel)
		{
			return;
		}
		if (!e.OperationAlreadyDone)
		{
			if (isDirectory)
			{
				try
				{
					if (Directory.Exists(fileName))
					{
						if (sendToRecycleBin)
						{
							SendToRecybleBinSilent(fileName);
						}
						else
						{
							Directory.Delete(fileName, recursive: true);
						}
					}
				}
				catch (Exception ex)
				{
					MessageService.ShowError(ex, "Can't remove directory " + fileName);
				}
			}
			else
			{
				try
				{
					if (File.Exists(fileName))
					{
						if (sendToRecycleBin)
						{
							SendToRecybleBinSilent(fileName);
						}
						else
						{
							File.Delete(fileName);
						}
					}
				}
				catch (Exception ex2)
				{
					MessageService.ShowError(ex2, "Can't remove file " + fileName);
				}
			}
		}
		OnFileRemoved(new FileEventArgs(fileName, isDirectory));
	}

	public static bool RenameFile(string oldName, string newName, bool isDirectory)
	{
		if (FileUtility.IsEqualFileName(oldName, newName))
		{
			return false;
		}
		FileRenamingEventArgs e = new FileRenamingEventArgs(oldName, newName, isDirectory);
		OnFileRenaming(e);
		if (e.Cancel)
		{
			return false;
		}
		if (!e.OperationAlreadyDone)
		{
			try
			{
				if (isDirectory && Directory.Exists(oldName))
				{
					if (Directory.Exists(newName))
					{
						MessageService.ShowMessage(StringParser.Parse("${res:Gui.ProjectBrowser.FileInUseError}"));
						return false;
					}
					Directory.Move(oldName, newName);
				}
				else if (File.Exists(oldName))
				{
					if (File.Exists(newName))
					{
						MessageService.ShowMessage(StringParser.Parse("${res:Gui.ProjectBrowser.FileInUseError}"));
						return false;
					}
					File.Move(oldName, newName);
				}
			}
			catch (Exception ex)
			{
				if (isDirectory)
				{
					MessageService.ShowError(ex, "Can't rename directory " + oldName);
				}
				else
				{
					MessageService.ShowError(ex, "Can't rename file " + oldName);
				}
				return false;
			}
		}
		OnFileRenamed(new FileRenameEventArgs(oldName, newName, isDirectory));
		return true;
	}

	public static IViewContent JumpToFilePosition(string fileName, int line, int column)
	{
		if (fileName == null || fileName.Length == 0)
		{
			return null;
		}
		IWorkbenchWindow workbenchWindow = OpenFile(fileName);
		if (workbenchWindow == null)
		{
			return null;
		}
		IBaseViewContent activeViewContent = workbenchWindow.ActiveViewContent;
		IViewContent viewContent = workbenchWindow.ViewContent;
		IPositionable positionable = (activeViewContent as IPositionable) ?? (viewContent as IPositionable);
		if (positionable != null)
		{
			if (positionable == viewContent && viewContent != activeViewContent)
			{
				workbenchWindow.SelectWindow();
			}
			else
			{
				positionable.JumpTo(Math.Max(0, line), Math.Max(0, column));
			}
		}
		NavigationService.Log(viewContent.BuildNavPoint());
		return viewContent;
	}

	public static FolderBrowserDialog CreateFolderBrowserDialog(string description, string selectedPath)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.Description = StringParser.Parse(description);
		if (selectedPath != null && selectedPath.Length > 0 && Directory.Exists(selectedPath))
		{
			folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
			folderBrowserDialog.SelectedPath = selectedPath;
		}
		return folderBrowserDialog;
	}

	public static FolderBrowserDialog CreateFolderBrowserDialog(string description)
	{
		return CreateFolderBrowserDialog(description, null);
	}

	private static void OnFileRemoved(FileEventArgs e)
	{
		if (FileService.FileRemoved != null)
		{
			FileService.FileRemoved(null, e);
		}
	}

	private static void OnFileRemoving(FileCancelEventArgs e)
	{
		if (FileService.FileRemoving != null)
		{
			FileService.FileRemoving(null, e);
		}
	}

	private static void OnFileRenamed(FileRenameEventArgs e)
	{
		if (FileService.FileRenamed != null)
		{
			FileService.FileRenamed(null, e);
		}
	}

	private static void OnFileRenaming(FileRenamingEventArgs e)
	{
		if (FileService.FileRenaming != null)
		{
			FileService.FileRenaming(null, e);
		}
	}

	public static bool FireFileReplacing(string fileName, bool isDirectory)
	{
		FileCancelEventArgs e = new FileCancelEventArgs(fileName, isDirectory);
		if (FileService.FileReplacing != null)
		{
			FileService.FileReplacing(null, e);
		}
		return !e.Cancel;
	}

	public static void FireFileReplaced(string fileName, bool isDirectory)
	{
		if (FileService.FileReplaced != null)
		{
			FileService.FileReplaced(null, new FileEventArgs(fileName, isDirectory));
		}
	}

	public static void FireFileCreated(string fileName)
	{
		if (FileService.FileCreated != null)
		{
			FileService.FileCreated(null, new FileEventArgs(fileName, isDirectory: false));
		}
	}

	public static string GetFileCategory(string fileName)
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/Workbench/FileFilter");
		foreach (Codon codon in treeNode.Codons)
		{
			string pattern = codon.Properties.Get("extensions", "");
			if (FileUtility.MatchesPattern(fileName, pattern) && codon.Properties.Contains("category"))
			{
				return codon.Properties.Get("category", RecentOpen.defaultTypeFiles);
			}
		}
		return RecentOpen.defaultTypeFiles;
	}

	[DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "SHFileOperation")]
	private static extern int SHFileOperation_x86(ref SHFILEOPSTRUCT_x86 FileOp);

	[DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "SHFileOperation")]
	private static extern int SHFileOperation_x64(ref SHFILEOPSTRUCT_x64 FileOp);

	private static bool IsWOW64Process()
	{
		return IntPtr.Size == 8;
	}

	public static bool SendToRecybleBin(string path, FileOperationFlags flags)
	{
		try
		{
			if (IsWOW64Process())
			{
				SHFILEOPSTRUCT_x64 FileOp = new SHFILEOPSTRUCT_x64
				{
					wFunc = FileOperationType.FO_DELETE,
					pFrom = path + '\0' + '\0',
					fFlags = (FileOperationFlags.FOF_ALLOWUNDO | flags)
				};
				SHFileOperation_x64(ref FileOp);
			}
			else
			{
				SHFILEOPSTRUCT_x86 FileOp2 = new SHFILEOPSTRUCT_x86
				{
					wFunc = FileOperationType.FO_DELETE,
					pFrom = path + '\0' + '\0',
					fFlags = (FileOperationFlags.FOF_ALLOWUNDO | flags)
				};
				SHFileOperation_x86(ref FileOp2);
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool SendToRecybleBin(string path)
	{
		return SendToRecybleBin(path, FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING);
	}

	public static bool SendToRecybleBinSilent(string path)
	{
		return SendToRecybleBin(path, FileOperationFlags.FOF_SILENT | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI);
	}
}

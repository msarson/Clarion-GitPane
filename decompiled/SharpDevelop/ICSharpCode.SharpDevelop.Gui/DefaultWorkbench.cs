using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.CustomizableStrips;
using ICSharpCode.SharpDevelop.Gui.StartPage;
using ICSharpCode.SharpDevelop.Project;
using WeifenLuo.WinFormsUI;

namespace ICSharpCode.SharpDevelop.Gui;

public class DefaultWorkbench : Form, IWorkbench, IMementoCapable
{
	public static class SingleInstanceHelper
	{
		private const int CUSTOM_MESSAGE = 1026;

		private const int RESULT_FILES_HANDLED = 2;

		private const int RESULT_PROJECT_IS_OPEN = 3;

		public static bool OpenFilesInPreviousInstance(string[] fileList)
		{
			LoggingService.Info("Trying to pass arguments to previous instance...");
			int id = Process.GetCurrentProcess().Id;
			string location = Assembly.GetEntryAssembly().Location;
			int num = new Random().Next();
			string path = Path.Combine(Path.GetTempPath(), "sd" + num + ".tmp");
			try
			{
				File.WriteAllLines(path, fileList);
				List<IntPtr> list = new List<IntPtr>();
				Process[] processesByName = Process.GetProcessesByName("Clarion");
				foreach (Process process in processesByName)
				{
					if (process.Id == id || !FileUtility.IsEqualFileName(location, process.MainModule.FileName))
					{
						continue;
					}
					IntPtr mainWindowHandle = process.MainWindowHandle;
					if (mainWindowHandle != IntPtr.Zero)
					{
						switch (NativeMethods.SendMessage(mainWindowHandle, 1026, new IntPtr(num), IntPtr.Zero).ToInt64())
						{
						case 2L:
							return true;
						case 3L:
							list.Add(mainWindowHandle);
							break;
						}
					}
				}
				foreach (IntPtr item in list)
				{
					if (NativeMethods.SendMessage(item, 1026, new IntPtr(num), new IntPtr(1)).ToInt64() == 2)
					{
						return true;
					}
				}
				return false;
			}
			finally
			{
				File.Delete(path);
			}
		}

		internal static bool PreFilterMessage(ref Message m)
		{
			if (m.Msg != 1026)
			{
				return false;
			}
			long num = m.WParam.ToInt64();
			long num2 = m.LParam.ToInt64();
			LoggingService.Info("Receiving custom message...");
			if (num2 == 0 && ProjectService.OpenSolution != null)
			{
				m.Result = new IntPtr(3);
			}
			else
			{
				m.Result = new IntPtr(2);
				try
				{
					WorkbenchSingleton.SafeThreadAsyncCall(delegate
					{
						NativeMethods.SetForegroundWindow(WorkbenchSingleton.MainForm.Handle);
					});
					string[] array = File.ReadAllLines(Path.Combine(Path.GetTempPath(), "sd" + num + ".tmp"));
					foreach (string arg in array)
					{
						WorkbenchSingleton.SafeThreadAsyncCall(delegate(string openFileName)
						{
							FileService.OpenFile(openFileName);
						}, arg);
					}
				}
				catch (Exception message)
				{
					LoggingService.Warn(message);
				}
			}
			return true;
		}
	}

	private const int WM_CHAR = 258;

	private const int WM_SYSCHAR = 262;

	private static readonly string mainMenuPath = "/SharpDevelop/Workbench/MainMenu";

	private static readonly string viewContentPath = "/SharpDevelop/Workbench/Pads";

	private List<PadDescriptor> viewContentCollection = new List<PadDescriptor>();

	private List<IViewContent> workbenchContentCollection = new List<IViewContent>();

	private bool isActiveWindow;

	private bool closeAll;

	private bool fullscreen;

	private FormWindowState defaultWindowState;

	private Rectangle normalBounds = new Rectangle(0, 0, 640, 480);

	private IWorkbenchLayout layout;

	private Timer toolbarUpdateTimer;

	private AppearanceControl AppearanceControl;

	private string claVer;

	private string claNetVer;

	private string defaultClaVer;

	private string defaultClaNetVer;

	public MenuStrip TopMenu;

	public ToolStrip[] ToolBars;

	private Message win32Message;

	public bool FullScreen
	{
		get
		{
			return fullscreen;
		}
		set
		{
			if (fullscreen != value)
			{
				fullscreen = value;
				if (fullscreen)
				{
					defaultWindowState = base.WindowState;
					base.Visible = false;
					base.FormBorderStyle = FormBorderStyle.None;
					base.WindowState = FormWindowState.Maximized;
					base.Visible = true;
				}
				else
				{
					base.FormBorderStyle = FormBorderStyle.Sizable;
					base.Bounds = normalBounds;
					base.WindowState = defaultWindowState;
				}
				RedrawAllComponents();
			}
		}
	}

	public string Title
	{
		get
		{
			return Text;
		}
		set
		{
			SetText(value);
		}
	}

	public bool IsActiveWindow => isActiveWindow;

	public IWorkbenchLayout WorkbenchLayout
	{
		get
		{
			return layout;
		}
		set
		{
			if (layout != null)
			{
				layout.ActiveWorkbenchWindowChanged -= OnActiveWindowChanged;
				layout.Detach();
			}
			value.Attach(this);
			layout = value;
			layout.ActiveWorkbenchWindowChanged += OnActiveWindowChanged;
		}
	}

	public List<PadDescriptor> PadContentCollection => viewContentCollection;

	public List<IViewContent> ViewContentCollection => workbenchContentCollection;

	public IWorkbenchWindow ActiveWorkbenchWindow
	{
		get
		{
			if (layout == null)
			{
				return null;
			}
			return layout.ActiveWorkbenchwindow;
		}
	}

	public object ActiveContent
	{
		get
		{
			if (layout == null)
			{
				return null;
			}
			return layout.ActiveContent;
		}
	}

	public Form MainForm => this;

	public bool IsAltGRPressed
	{
		get
		{
			if (NativeMethods.IsKeyPressed(Keys.RMenu))
			{
				return (Control.ModifierKeys & Keys.Control) == Keys.Control;
			}
			return false;
		}
	}

	public event ViewContentEventHandler ViewOpened;

	public event ViewContentEventHandler ViewClosed;

	public event EventHandler ActiveWorkbenchWindowChanged;

	private void SetText(string value)
	{
		if (base.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(SetText, value);
		}
		else
		{
			Text = value;
		}
	}

	public DefaultWorkbench()
	{
		Text = ResourceService.GetString("MainWindow.DialogName");
		base.Icon = ResourceService.GetIcon("Icons.SharpDevelopIcon");
		base.StartPosition = FormStartPosition.Manual;
		AllowDrop = true;
	}

	protected override void WndProc(ref Message m)
	{
		if (!SingleInstanceHelper.PreFilterMessage(ref m))
		{
			base.WndProc(ref m);
		}
	}

	public void InitializeWorkspace()
	{
		AppearanceControl = new AppearanceControl();
		UpdateRenderer();
		base.MenuComplete += SetStandardStatusBar;
		SetStandardStatusBar(null, null);
		ProjectService.CurrentProjectChanged += SetProjectTitle;
		ProjectService.SolutionConfigurationChanged += SetConfigurationTitle;
		FileService.FileRemoved += CheckRemovedOrReplacedFile;
		FileService.FileReplaced += CheckRemovedOrReplacedFile;
		FileService.FileRenamed += CheckRenamedFile;
		FileService.FileRemoved += FileService.RecentOpen.FileRemoved;
		FileService.FileRenamed += FileService.RecentOpen.FileRenamed;
		ProjectService.SolutionLoaded += ProjectService_SolutionLoaded;
		ProjectService.SolutionClosed += ProjectService_SolutionClosed;
		try
		{
			ArrayList arrayList = AddInTree.GetTreeNode(viewContentPath).BuildChildItems(this);
			foreach (PadDescriptor item in arrayList)
			{
				if (item != null)
				{
					ShowPad(item);
				}
			}
		}
		catch (TreePathNotFoundException)
		{
		}
		CreateMainMenu();
		CreateToolBars();
		toolbarUpdateTimer = new Timer();
		toolbarUpdateTimer.Tick += UpdateMenu;
		toolbarUpdateTimer.Interval = 500;
		toolbarUpdateTimer.Start();
		RightToLeftConverter.Convert(this);
	}

	private void ProjectService_SolutionClosed(object sender, EventArgs e)
	{
		SetProjectTitle(null);
	}

	private void ProjectService_SolutionLoaded(object sender, SolutionEventArgs e)
	{
		SetProjectTitle(null);
	}

	public void CloseContent(IViewContent content)
	{
		if (PropertyService.Get("SharpDevelop.LoadDocumentProperties", defaultValue: true) && content is IMementoCapable)
		{
			StoreMemento(content);
		}
		if (ViewContentCollection.Contains(content))
		{
			ViewContentCollection.Remove(content);
		}
		OnViewClosed(new ViewContentEventArgs(content));
		content.Dispose();
		content = null;
	}

	public void CloseAllViews()
	{
		try
		{
			closeAll = true;
			List<IViewContent> list = new List<IViewContent>(workbenchContentCollection);
			foreach (IViewContent item in list)
			{
				item.WorkbenchWindow?.CloseWindow(force: false);
			}
		}
		finally
		{
			closeAll = false;
			OnActiveWindowChanged(this, EventArgs.Empty);
		}
	}

	public bool CloseAllSolutionViews()
	{
		try
		{
			closeAll = true;
			bool flag = PropertyService.Get("SharpDevelop.TreatUnrelatedFilesAsSolutions", defaultValue: false);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			CountSolutionFiles(dictionary, ProjectService.OpenSolution);
			List<IViewContent> list = new List<IViewContent>(workbenchContentCollection);
			foreach (IViewContent item in list)
			{
				IWorkbenchWindow workbenchWindow = item.WorkbenchWindow;
				if (workbenchWindow == null || string.IsNullOrEmpty(item.FileName))
				{
					continue;
				}
				if (dictionary.ContainsKey(Path.GetFullPath(item.FileName).ToUpper()))
				{
					if (!workbenchWindow.CloseWindow(force: false))
					{
						return false;
					}
				}
				else if (flag)
				{
					workbenchWindow.CloseWindow(force: false);
				}
			}
		}
		finally
		{
			closeAll = false;
			OnActiveWindowChanged(this, EventArgs.Empty);
		}
		return true;
	}

	private static void CountSolutionFiles(Dictionary<string, string> allFiles, Solution solution)
	{
		if (solution == null)
		{
			return;
		}
		foreach (IProject project in solution.Projects)
		{
			string key = Path.GetFullPath(project.FileName).ToUpper();
			if (!allFiles.ContainsKey(key))
			{
				allFiles.Add(key, string.Empty);
			}
			foreach (ProjectItem item in project.Items)
			{
				if (item is FileProjectItem)
				{
					key = Path.GetFullPath(item.FileName).ToUpper();
					if (item is FileProjectItem && item.FileName != null && !allFiles.ContainsKey(key))
					{
						allFiles.Add(key, string.Empty);
					}
				}
			}
		}
	}

	private void CreateViewBase(IViewContent content)
	{
		if (!PropertyService.Get("SharpDevelop.LoadDocumentProperties", defaultValue: true) || !(content is IMementoCapable))
		{
			return;
		}
		try
		{
			Properties storedMemento = GetStoredMemento(content);
			if (storedMemento != null)
			{
				((IMementoCapable)content).SetMemento(storedMemento);
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex, "Can't get/set memento");
		}
	}

	public virtual void CreateView(IViewContent content)
	{
		CreateViewBase(content);
		layout.CreateWorkbenchWindow(content);
	}

	public virtual void ShowView(IViewContent content)
	{
		ViewContentCollection.Add(content);
		CreateViewBase(content);
		layout.ShowView(content);
		content.WorkbenchWindow.SelectWindow();
		OnViewOpened(new ViewContentEventArgs(content));
	}

	public virtual void ShowPad(PadDescriptor content)
	{
		PadContentCollection.Add(content);
		if (layout != null)
		{
			layout.ShowPad(content);
		}
	}

	public virtual void ShowAndDockPad(PadDescriptor content)
	{
		PadContentCollection.Add(content);
		if (layout != null)
		{
			layout.ShowAndDockPad(content);
		}
	}

	public void UnloadPad(PadDescriptor content)
	{
		PadContentCollection.Remove(content);
		if (layout != null)
		{
			layout.UnloadPad(content);
		}
		content.Dispose();
	}

	public void UpdateRenderer()
	{
		StartpageBrowserPane.ColorSchemeChanged = true;
		if (PropertyService.Get("ICSharpCode.SharpDevelop.Gui.UseProfessionalRenderer", defaultValue: true))
		{
			string toolStripManagerRendererTheme = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.ProfessionalRendererColorTableStyles", "Win10Blue");
			ColorThemesListService.SetToolStripManagerRendererTheme(toolStripManagerRendererTheme);
			DockPanelColorTable.Instance.UseProfessionalColorTable = true;
		}
		else
		{
			ToolStripManager.Renderer = new ToolStripProfessionalRenderer();
			DockPanelColorTable.Instance.UseProfessionalColorTable = false;
		}
	}

	public void RedrawAllComponents()
	{
		ToolbarService.SetSize();
		if (ToolBars != null)
		{
			ToolbarService.SetToolStripSize(this, ToolBars);
		}
		RightToLeftConverter.ConvertRecursive(this);
		foreach (ToolStripItem item in TopMenu.Items)
		{
			if (item is IStatusUpdate)
			{
				((IStatusUpdate)item).UpdateText();
			}
		}
		foreach (IViewContent item2 in workbenchContentCollection)
		{
			item2.RedrawContent();
			if (item2.WorkbenchWindow != null)
			{
				item2.WorkbenchWindow.RedrawContent();
			}
		}
		foreach (PadDescriptor item3 in viewContentCollection)
		{
			item3.RedrawContent();
		}
		if (layout != null)
		{
			layout.RedrawAllComponents();
		}
		StatusBarService.RedrawStatusbar();
	}

	private string GetMementoFileName(string contentName, string baseDir)
	{
		if (FileUtility.IsValidFileName(baseDir))
		{
			string text = Path.Combine(baseDir, "temp");
			string relativePath = FileUtility.GetRelativePath(text, contentName);
			return Path.Combine(text, Path.GetFileName(contentName) + "." + relativePath.ToLowerInvariant().GetHashCode().ToString("x") + ".xml");
		}
		string path = Path.Combine(PropertyService.ConfigDirectory, "temp");
		return Path.Combine(path, Path.GetFileName(contentName) + "." + contentName.ToLowerInvariant().GetHashCode().ToString("x") + ".xml");
	}

	public Properties GetStoredMemento(IViewContent content)
	{
		if (content != null && content.FileName != null)
		{
			string baseDir = null;
			if (PropertyService.Get("SharpDevelop.PreferenceInSolutionFolder", defaultValue: false) && ProjectService.OpenSolution != null)
			{
				IProject project = ProjectService.OpenSolution.FindProjectContainingFile(content.FileName);
				if (project != null)
				{
					baseDir = Path.GetDirectoryName(project.FileName);
				}
			}
			string mementoFileName = GetMementoFileName(content.FileName, baseDir);
			if (FileUtility.IsValidFileName(mementoFileName) && File.Exists(mementoFileName))
			{
				return Properties.Load(mementoFileName);
			}
		}
		return null;
	}

	public void StoreMemento(IViewContent content)
	{
		if (content.FileName == null)
		{
			return;
		}
		string baseDir = null;
		if (PropertyService.Get("SharpDevelop.PreferenceInSolutionFolder", defaultValue: false) && ProjectService.OpenSolution != null)
		{
			IProject project = ProjectService.OpenSolution.FindProjectContainingFile(content.FileName);
			if (project != null)
			{
				baseDir = Path.GetDirectoryName(project.FileName);
			}
		}
		Properties properties = ((IMementoCapable)content).CreateMemento();
		string mementoFileName = GetMementoFileName(content.FileName, baseDir);
		if (FileUtility.IsValidFileName(mementoFileName))
		{
			if (!Directory.Exists(Path.GetDirectoryName(mementoFileName)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(mementoFileName));
			}
			FileUtility.ObservedSave(properties.Save, mementoFileName, FileErrorPolicy.Inform);
		}
	}

	public Properties CreateMemento()
	{
		Properties properties = new Properties();
		properties["bounds"] = normalBounds.X.ToString(NumberFormatInfo.InvariantInfo) + "," + normalBounds.Y.ToString(NumberFormatInfo.InvariantInfo) + "," + normalBounds.Width.ToString(NumberFormatInfo.InvariantInfo) + "," + normalBounds.Height.ToString(NumberFormatInfo.InvariantInfo);
		if (FullScreen || base.WindowState == FormWindowState.Minimized)
		{
			properties["windowstate"] = defaultWindowState.ToString();
		}
		else
		{
			properties["windowstate"] = base.WindowState.ToString();
		}
		properties["defaultstate"] = defaultWindowState.ToString();
		return properties;
	}

	public void SetMemento(Properties properties)
	{
		if (properties != null && properties.Contains("bounds"))
		{
			string[] array = properties["bounds"].Split(',');
			if (array.Length == 4)
			{
				base.Bounds = (normalBounds = new Rectangle(int.Parse(array[0], NumberFormatInfo.InvariantInfo), int.Parse(array[1], NumberFormatInfo.InvariantInfo), int.Parse(array[2], NumberFormatInfo.InvariantInfo), int.Parse(array[3], NumberFormatInfo.InvariantInfo)));
			}
			defaultWindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), properties["defaultstate"]);
			FullScreen = properties.Get("fullscreen", defaultValue: false);
			base.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), properties["windowstate"]);
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (!FullScreen && base.WindowState != FormWindowState.Minimized)
		{
			defaultWindowState = base.WindowState;
			if (base.WindowState == FormWindowState.Normal)
			{
				normalBounds = base.Bounds;
			}
		}
	}

	protected override void OnLocationChanged(EventArgs e)
	{
		base.OnLocationChanged(e);
		if (base.WindowState == FormWindowState.Normal)
		{
			normalBounds = base.Bounds;
		}
	}

	private void CheckRemovedOrReplacedFile(object sender, FileEventArgs e)
	{
		int num = 0;
		while (num < ViewContentCollection.Count)
		{
			if (FileUtility.IsBaseDirectory(e.FileName, ViewContentCollection[num].FileName))
			{
				ViewContentCollection[num].WorkbenchWindow.CloseWindow(force: true);
			}
			else
			{
				num++;
			}
		}
	}

	private void CheckRenamedFile(object sender, FileRenameEventArgs e)
	{
		if (e.IsDirectory)
		{
			foreach (IViewContent item in ViewContentCollection)
			{
				if (item.FileName != null && FileUtility.IsBaseDirectory(e.SourceFile, item.FileName))
				{
					item.FileName = FileUtility.RenameBaseDirectory(item.FileName, e.SourceFile, e.TargetFile);
				}
			}
			return;
		}
		foreach (IViewContent item2 in ViewContentCollection)
		{
			if (item2.FileName != null && FileUtility.IsEqualFileName(item2.FileName, e.SourceFile))
			{
				item2.FileName = e.TargetFile;
				item2.TitleName = Path.GetFileName(e.TargetFile);
				break;
			}
		}
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
		if (ProjectService.IsBuilding)
		{
			MessageService.ShowMessage(StringParser.Parse("${res:MainWindow.CannotCloseWithBuildInProgressMessage}"));
			e.Cancel = true;
			return;
		}
		ProjectService.SaveSolutionPreferences();
		while (WorkbenchSingleton.Workbench.ViewContentCollection.Count > 0)
		{
			IViewContent viewContent = WorkbenchSingleton.Workbench.ViewContentCollection[0];
			if (viewContent.WorkbenchWindow == null)
			{
				LoggingService.Warn("Content with empty WorkbenchWindow found");
				WorkbenchSingleton.Workbench.ViewContentCollection.RemoveAt(0);
				continue;
			}
			viewContent.WorkbenchWindow.CloseWindow(force: false);
			if (WorkbenchSingleton.Workbench.ViewContentCollection.IndexOf(viewContent) < 0)
			{
				continue;
			}
			e.Cancel = true;
			return;
		}
		if (!ProjectService.CloseSolution())
		{
			e.Cancel = true;
			return;
		}
		closeAll = true;
		ParserService.StopParserThread();
		layout.Detach();
		foreach (PadDescriptor item in PadContentCollection)
		{
			item.Dispose();
		}
	}

	protected override void OnClosed(EventArgs e)
	{
		base.OnClosed(e);
	}

	public void SetProjectTitle(IProject p)
	{
		if (base.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(SetProjectTitle, p);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = p != null && ProjectService.OpenSolution != null;
		if (flag)
		{
			stringBuilder.Append(p.TitleName);
			stringBuilder.Append(" - ");
		}
		stringBuilder.Append(ResourceService.GetString("MainWindow.DialogName"));
		stringBuilder.Append(" [");
		if (flag && !string.IsNullOrEmpty(p.VersionName) && p.VersionName != "Current")
		{
			stringBuilder.Append(p.VersionName);
		}
		else
		{
			if (!string.IsNullOrEmpty(defaultClaVer))
			{
				if (string.IsNullOrEmpty(claVer))
				{
					stringBuilder.Append(defaultClaVer);
				}
				else
				{
					stringBuilder.Append(claVer);
				}
				if (!string.IsNullOrEmpty(defaultClaNetVer))
				{
					stringBuilder.Append(", ");
				}
			}
			if (!string.IsNullOrEmpty(claNetVer))
			{
				stringBuilder.Append(claNetVer);
			}
			else if (!string.IsNullOrEmpty(defaultClaNetVer))
			{
				stringBuilder.Append(defaultClaNetVer);
			}
		}
		stringBuilder.Append("]");
		if (flag)
		{
			stringBuilder.Append(" (");
			stringBuilder.Append(ProjectService.OpenSolution.Preferences.ActiveConfiguration);
			stringBuilder.Append(")");
		}
		if (PropertyService.Get("Workbench.ShowFullPathOnTitle", defaultValue: true))
		{
			if (flag)
			{
				stringBuilder.Append(" - [");
				stringBuilder.Append(Path.GetDirectoryName(p.FileName));
				stringBuilder.Append("]");
			}
			else if (ProjectService.OpenSolution != null)
			{
				stringBuilder.Append(" - [");
				stringBuilder.Append(ProjectService.OpenSolution.Directory);
				stringBuilder.Append("]");
			}
		}
		Title = stringBuilder.ToString();
	}

	public void SetClarionVersion(string version, bool forWindows)
	{
		string text = ((version == "Current") ? null : version);
		if (forWindows)
		{
			claVer = text;
		}
		else
		{
			claNetVer = text;
		}
	}

	public void SetDefaultClarionVersion(string version, bool forWindows)
	{
		if (forWindows)
		{
			defaultClaVer = version;
		}
		else
		{
			defaultClaNetVer = version;
		}
	}

	private void SetProjectTitle(object sender, ProjectEventArgs e)
	{
		SetProjectTitle(e.Project);
	}

	private void SetConfigurationTitle(object sender, SolutionConfigurationEventArgs e)
	{
		SetProjectTitle(ProjectService.CurrentProject);
	}

	private void SetStandardStatusBar(object sender, EventArgs e)
	{
		StatusBarService.SetMessage("${res:MainWindow.StatusBar.ReadyMessage}");
	}

	private void OnActiveWindowChanged(object sender, EventArgs e)
	{
		if (!closeAll && this.ActiveWorkbenchWindowChanged != null)
		{
			this.ActiveWorkbenchWindowChanged(this, e);
		}
	}

	public PadDescriptor GetPad(Type type)
	{
		foreach (PadDescriptor item in PadContentCollection)
		{
			if (item.Class == type.FullName)
			{
				return item;
			}
		}
		return null;
	}

	private void CreateMainMenu()
	{
		TopMenu = new MenuStrip();
		TopMenu.Items.Clear();
		try
		{
			ToolStripItem[] toolStripItems = (ToolStripItem[])AddInTree.GetTreeNode(mainMenuPath).BuildChildItems(this).ToArray(typeof(ToolStripItem));
			TopMenu.Items.AddRange(toolStripItems);
			UpdateMenus();
		}
		catch (TreePathNotFoundException)
		{
		}
	}

	private void UpdateMenu(object sender, EventArgs e)
	{
		UpdateMenus();
		UpdateToolbars();
	}

	private void UpdateMenus()
	{
		foreach (object item in TopMenu.Items)
		{
			if (item is IStatusUpdate)
			{
				((IStatusUpdate)item).UpdateStatus();
			}
		}
	}

	private void UpdateToolbars()
	{
		if (ToolBars != null)
		{
			ToolStrip[] toolBars = ToolBars;
			foreach (ToolStrip toolStrip in toolBars)
			{
				ToolbarService.UpdateToolbar(toolStrip);
			}
		}
	}

	private void CreateToolBars()
	{
		if (ToolBars == null)
		{
			ToolBars = ToolbarService.CreateToolbars(this, "/SharpDevelop/Workbench/ToolBar");
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (IsAltGRPressed)
		{
			return false;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	protected override void OnDragEnter(DragEventArgs e)
	{
		base.OnDragEnter(e);
		if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
			string[] array2 = array;
			foreach (string path in array2)
			{
				if (File.Exists(path))
				{
					e.Effect = DragDropEffects.Copy;
					return;
				}
			}
		}
		e.Effect = DragDropEffects.None;
	}

	protected override void OnDragDrop(DragEventArgs e)
	{
		base.OnDragDrop(e);
		if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			return;
		}
		string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (File.Exists(text))
			{
				IProjectLoader projectLoader = ProjectService.GetProjectLoader(text);
				if (projectLoader != null)
				{
					FileUtility.ObservedLoad(projectLoader.Load, text);
				}
				else
				{
					FileService.OpenFile(text);
				}
			}
		}
	}

	protected virtual void OnViewOpened(ViewContentEventArgs e)
	{
		if (this.ViewOpened != null)
		{
			this.ViewOpened(this, e);
		}
	}

	protected virtual void OnViewClosed(ViewContentEventArgs e)
	{
		if (this.ViewClosed != null)
		{
			this.ViewClosed(this, e);
		}
	}

	protected override void OnActivated(EventArgs e)
	{
		isActiveWindow = true;
		base.OnActivated(e);
	}

	protected override void OnDeactivate(EventArgs e)
	{
		isActiveWindow = false;
		base.OnDeactivate(e);
	}

	private bool ThreadedDoWin32Event()
	{
		if (base.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction(ThreadedDoWin32Event);
		}
		switch ((Keys)(int)win32Message.WParam)
		{
		case Keys.Tab:
		case Keys.Left:
		case Keys.Up:
		case Keys.Right:
		case Keys.Down:
			return false;
		default:
		{
			bool flag = PreProcessMessage(ref win32Message);
			if (flag)
			{
				if (win32Message.Msg == 262)
				{
					TopMenu.Focus();
				}
				else if (win32Message.Msg == 258)
				{
					return false;
				}
			}
			return flag;
		}
		}
	}

	internal bool DoWin32Event(ref Message msg)
	{
		win32Message = msg;
		bool result = ThreadedDoWin32Event();
		msg = win32Message;
		return result;
	}
}

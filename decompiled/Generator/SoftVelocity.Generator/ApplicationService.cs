using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Clarion.Core.Options;
using Clarion.GEN;
using Clarion.PRJ;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Project.Commands;
using ICSharpCode.SharpDevelop.Project.Dialogs;
using Microsoft.Build.Framework;
using SoftVelocity.BinaryToText;
using SoftVelocity.Common;
using SoftVelocity.DataDictionary.FileSchemaEditor;
using SoftVelocity.Generator.Commands;
using SoftVelocity.Generator.Properties;
using SoftVelocity.Generator.UI;

namespace SoftVelocity.Generator;

public sealed class ApplicationService : ITreeModel
{
	public enum ApplicationsSort
	{
		Unknown,
		ByName,
		ByDependency,
		ByModificationDate
	}

	private class ProjectDependencyAppComparer : IComparer<Application>, IComparer<IProject>
	{
		private bool solServiceAttached;

		private List<IProject> sortedProjects = new List<IProject>();

		private bool HasReferences(IProject p)
		{
			for (int i = 0; i < p.Items.Count; i++)
			{
				ProjectItem val = p.Items[i];
				if (val is ProjectReferenceProjectItem)
				{
					return true;
				}
			}
			return false;
		}

		private bool HasDependencies(IProject p)
		{
			if (p.ProjectSections != null)
			{
				for (int i = 0; i < p.ProjectSections.Count; i++)
				{
					ProjectSection val = p.ProjectSections[i];
					if (val.Name.Equals("ProjectDependencies", StringComparison.OrdinalIgnoreCase) && val.Items.Count > 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void Refresh()
		{
			DateTime now = DateTime.Now;
			SetTextDebug(ResourceService.GetString("Clarion.Generator.SortingApplicationsListBegin"));
			sortedProjects.Clear();
			Solution openSolution = ProjectService.OpenSolution;
			if (openSolution != null)
			{
				if (!solServiceAttached)
				{
					ProjectService.SolutionClosing += OnSolutionClosing;
				}
				foreach (IProject project in openSolution.Projects)
				{
					if (!HasReferences(project) && !HasDependencies(project))
					{
						sortedProjects.Add(project);
					}
				}
				foreach (IProject project2 in openSolution.Projects)
				{
					AddProject(project2, sortedProjects, new Stack<IProject>());
				}
			}
			DateTime now2 = DateTime.Now;
			TimeSpan timeSpan = now2 - now;
			SetTextDebug(string.Format(ResourceService.GetString("Clarion.Generator.SortingApplicationsListFinish"), timeSpan.ToString()));
		}

		private void OnSolutionClosing(object sender, SolutionCancelEventArgs e)
		{
			sortedProjects.Clear();
		}

		private void AddProject(IProject proj, List<IProject> sortedProjects, Stack<IProject> refStack)
		{
			if (proj == null || sortedProjects.Contains(proj))
			{
				return;
			}
			refStack.Push(proj);
			foreach (ProjectItem item in proj.Items)
			{
				if (!(item is ProjectReferenceProjectItem))
				{
					continue;
				}
				ProjectReferenceProjectItem val = (ProjectReferenceProjectItem)(object)((item is ProjectReferenceProjectItem) ? item : null);
				if (val != null)
				{
					IProject referencedProject = val.ReferencedProject;
					if (referencedProject == null)
					{
						SetText(string.Format(ResourceService.GetString("Clarion.Generator.Error.InvalidPrjReference"), ((ISolutionFolder)proj).Name, val.ProjectName));
					}
					else if (!refStack.Contains(referencedProject))
					{
						AddProject(referencedProject, sortedProjects, refStack);
					}
				}
			}
			Solution openSolution = ProjectService.OpenSolution;
			if (openSolution != null && proj.ProjectSections != null)
			{
				foreach (ProjectSection projectSection in proj.ProjectSections)
				{
					if (!projectSection.Name.Equals("ProjectDependencies", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					foreach (SolutionItem item2 in projectSection.Items)
					{
						foreach (IProject project in openSolution.Projects)
						{
							if (((ISolutionFolder)project).IdGuid.Equals(item2.Name, StringComparison.OrdinalIgnoreCase))
							{
								if (!refStack.Contains(project))
								{
									AddProject(project, sortedProjects, refStack);
								}
								break;
							}
						}
					}
				}
			}
			refStack.Pop();
			sortedProjects.Add(proj);
		}

		public int Compare(Application x, Application y)
		{
			if (x == null || y == null)
			{
				return 0;
			}
			IProject project;
			IProject project2;
			try
			{
				project = ProjectService.GetProject(x.ProjectFileName);
				project2 = ProjectService.GetProject(y.ProjectFileName);
			}
			catch
			{
				return 0;
			}
			if (project == null || project2 == null)
			{
				return 0;
			}
			return Compare(project, project2);
		}

		public int Compare(IProject x, IProject y)
		{
			if (x == null || y == null)
			{
				return 0;
			}
			if (sortedProjects.Count == 0 && ApplicationsList.Count > 0)
			{
				Refresh();
			}
			int num;
			int value;
			try
			{
				num = sortedProjects.IndexOf(x);
				value = sortedProjects.IndexOf(y);
			}
			catch
			{
				return 0;
			}
			return num.CompareTo(value);
		}
	}

	public const string ApplicationFileExtension = ".app";

	private static bool needToReloadSolution;

	private static bool thereIsAMainWindow;

	private static bool inLoad;

	private static ApplicationService _Instance;

	private static GeneratorRequestor theGeneratorRequestor;

	private static LoggerVerbosity verbosity;

	private static MessageViewCategory generatorMessages;

	private static bool inAppendTextMode;

	private static Win32GeneratorInstance Gen;

	private ProjectDependencyAppComparer ByDependencyComparer;

	private static ApplicationsSort _ApplicationListCurrentSort;

	private static List<Application> _Applications;

	private static bool _Closing;

	private static bool OpeningApplication;

	private static bool OpeningDictionary;

	private static bool _CanOpenEditor;

	private static bool _InsideEditingDefaultApplication;

	private static List<string> _PushedAplicationNames;

	private static bool _PushedAplicationNameWithSettings;

	private static bool _PushedAplicationIsDebug;

	private static bool suspendRefreshApplicationList;

	private static bool solutionDirty;

	private static bool activeWindowAttached;

	private static object prevActiveContent;

	public static EventHandler<BuildErrorEventArgs> ErrorOccured;

	private static bool insideSaveAsApplication;

	internal static bool _IsTemplateRegistryOpen;

	private static TemplateRegistryControl_ViewContent TemplateRegistryVC;

	private static bool _IsGeneatorOptionsOpen;

	private static bool _Generating;

	private static Thread _BatchProcessorThread;

	private static bool unloadAppAfterMerge;

	private static volatile bool _BatchProcessorThreadHaveToWaitForCmd;

	private static bool _BatchProcessorThreadStoped;

	private static Application batchProcessSingleApp;

	private static IProject batchProcessSingleIPrj;

	private static int batchProcessTotalApps;

	private static Queue<Application> batchProcessApps;

	private static List<Application> batchProcessAppsOk;

	private static List<Application> batchProcessAppsError;

	private static bool batchProcessGenerate;

	private static bool batchProcessCancelled;

	private static bool batchProcessPosGenActionIsBatch;

	private static PosGenerationAction batchProcessPosGenAction;

	private static GenerationMode batchProcessConditionalGeneration;

	private static GenerationMode batchProcessDebugTraceGeneration;

	private static DateTime batchProcessStartTime;

	private static DateTime batchProcessStartTimeEach;

	private static Application batchProcessCurrentApplication;

	private static List<Task> preservedTasksList;

	private static List<IProject> reparsingDelayed;

	public static bool ThereIsAMainWindow
	{
		get
		{
			return thereIsAMainWindow;
		}
		private set
		{
			thereIsAMainWindow = value;
		}
	}

	internal static ApplicationService Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = new ApplicationService();
			}
			return _Instance;
		}
	}

	private static string ClarionVersion
	{
		get
		{
			string activeVersion = Versions.GetActiveVersion(ClarionAddins.Win32Present);
			if (string.IsNullOrEmpty(activeVersion))
			{
				return Versions.CurrentVersionName(ClarionAddins.Win32Present);
			}
			return activeVersion;
		}
	}

	public static LoggerVerbosity Verbosity => verbosity;

	private static MessageViewCategory GeneratorCategory
	{
		get
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			if (generatorMessages == null)
			{
				generatorMessages = CompilerMessageView.Instance.GetCategory("Generator");
				if (generatorMessages == null)
				{
					generatorMessages = new MessageViewCategory("Generator");
					CompilerMessageView instance = CompilerMessageView.Instance;
					instance.AddCategory(generatorMessages);
				}
			}
			return generatorMessages;
		}
	}

	public static string ApplicationInportFileExtension => "." + ((BinaryFileWatcher)AppWatcher.Instance).WatcherDetails.DefaultTextExtension;

	public static string ApplicationAutoImportExportLocalFile => ((BinaryFileWatcher)AppWatcher.Instance).AutoImportExportLocalFileExtension;

	internal static string AppExtension => ".app";

	internal Application DefaultApp
	{
		get
		{
			ApplicationListCurrentSort = ApplicationsSort.ByDependency;
			if (ApplicationsList.Count > 0)
			{
				return ApplicationsList[ApplicationsList.Count - 1];
			}
			return null;
		}
	}

	internal static ApplicationsSort ApplicationListCurrentSort
	{
		get
		{
			return _ApplicationListCurrentSort;
		}
		set
		{
			if (ApplicationsList != null)
			{
				if (value == _ApplicationListCurrentSort)
				{
					return;
				}
				_ApplicationListCurrentSort = value;
				if (ApplicationsList.Count <= 1)
				{
					return;
				}
				ApplicationServiceSettings.DefaultApplicationsListSort = _ApplicationListCurrentSort;
				switch (value)
				{
				case ApplicationsSort.ByName:
					ApplicationsList.Sort((Application a1, Application a2) => a1.Name.CompareTo(a2.Name));
					Instance.TreeModelDataChanged();
					break;
				case ApplicationsSort.ByModificationDate:
					ApplicationsList.Sort((Application a1, Application a2) => a2.ModificationDate.CompareTo(a1.ModificationDate));
					Instance.TreeModelDataChanged();
					break;
				case ApplicationsSort.ByDependency:
					if (Instance.ByDependencyComparer == null)
					{
						Instance.ByDependencyComparer = new ProjectDependencyAppComparer();
					}
					Instance.ByDependencyComparer.Refresh();
					ApplicationsList.Sort(Instance.ByDependencyComparer);
					Instance.TreeModelDataChanged();
					break;
				case ApplicationsSort.Unknown:
					break;
				}
				return;
			}
			throw new NullReferenceException("You can not set the Sorting before having a list of apps.");
		}
	}

	internal static List<Application> ApplicationsList => _Applications;

	internal static IEnumerable<Application> Applications
	{
		get
		{
			for (int idx = 0; idx < ApplicationsList.Count; idx++)
			{
				yield return _Applications[idx];
			}
		}
	}

	internal static bool AreApplicationOnEditFromSolution
	{
		get
		{
			foreach (Application application in Applications)
			{
				if (application.InEdit && application.IsOnSolution)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal static bool AreApplicationOnEdit
	{
		get
		{
			foreach (Application application in Applications)
			{
				if (application.InEdit)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal static List<Application> ApplicationsLoaded
	{
		get
		{
			List<Application> list = new List<Application>();
			foreach (Application application in Applications)
			{
				if (application.IsLoaded)
				{
					list.Add(application);
				}
			}
			return list;
		}
	}

	internal static bool Closing => _Closing;

	public static bool CanOpenEditor
	{
		get
		{
			return _CanOpenEditor;
		}
		internal set
		{
			_CanOpenEditor = value;
		}
	}

	private static IEnumerable<string> PushedAplicationNames
	{
		get
		{
			for (int idx = 0; idx < _PushedAplicationNames.Count; idx++)
			{
				yield return _PushedAplicationNames[idx];
			}
		}
	}

	private static int PushedAplicationNamesCount => _PushedAplicationNames.Count;

	private static string FirstPushedAplicationName
	{
		get
		{
			if (PushedAplicationNamesCount > 0)
			{
				return _PushedAplicationNames[0];
			}
			return string.Empty;
		}
	}

	private static bool SuspendRefreshApplicationList
	{
		get
		{
			return suspendRefreshApplicationList;
		}
		set
		{
			suspendRefreshApplicationList = value;
		}
	}

	public static bool IsTemplateRegistryOpen => _IsTemplateRegistryOpen;

	internal static string TemplateRegistryName => Gen.TemplateRegistryName;

	public static bool IsGeneatorOptionsOpen => _IsGeneatorOptionsOpen;

	public bool Generating => _Generating;

	public static bool IsGenerating => _Generating;

	public static event EventHandler<ApplicationLoadingEventArgs> ApplicationLoading;

	public static event EventHandler<ApplicationEventArgs> RemovingFromList;

	public event EventHandler<TreeModelEventArgs> NodesChanged;

	public event EventHandler<TreeModelEventArgs> NodesInserted;

	public event EventHandler<TreeModelEventArgs> NodesRemoved;

	public event EventHandler<TreePathEventArgs> StructureChanged;

	public static event EventHandler<GenerationStartEventArgs> GenerationStarting;

	public static event EventHandler<GenerationEndEventArgs> GenerationEnded;

	public static event EventHandler<ApplicationGeneratingEventArgs> ApplicationGenerationStarting;

	public static event EventHandler<ApplicationGeneratingEventArgs> ApplicationBatchGenerationStarting;

	public static event EventHandler<ApplicationGeneratingEventArgs> ApplicationBatchCompilationStarting;

	public static event EventHandler<GenerationStartEventArgs> ApplicationBatchBuildingStarting;

	public static event EventHandler ApplicationBatchBuildingEnded;

	public static event EventHandler ApplicationBatchBuildingCancelled;

	public static event EventHandler<ApplicationGeneratedEventArgs> ApplicationGenerationEnded;

	public static string GetApplicationsSortText(ApplicationsSort sort)
	{
		return sort switch
		{
			ApplicationsSort.ByName => ResourceService.GetString("Clarion.Generator.Pad.Buttons.sortByName"), 
			ApplicationsSort.ByDependency => ResourceService.GetString("Clarion.Generator.Pad.Buttons.sortByPosition"), 
			ApplicationsSort.ByModificationDate => ResourceService.GetString("Clarion.Generator.Pad.Buttons.sortByDate"), 
			_ => "Unknown", 
		};
	}

	public static Bitmap GetApplicationsSortImage(ApplicationsSort sort)
	{
		return sort switch
		{
			ApplicationsSort.ByName => Resources.SortAlphaDes, 
			ApplicationsSort.ByDependency => Resources.SortNumDes, 
			ApplicationsSort.ByModificationDate => Resources.SortByDate, 
			_ => Resources.GenCancel, 
		};
	}

	private ApplicationService()
	{
	}

	static ApplicationService()
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected O, but got Unknown
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Expected O, but got Unknown
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Expected O, but got Unknown
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Expected O, but got Unknown
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Expected O, but got Unknown
		verbosity = LoggerVerbosity.Normal;
		inAppendTextMode = false;
		_ApplicationListCurrentSort = ApplicationsSort.Unknown;
		_Closing = false;
		OpeningApplication = false;
		OpeningDictionary = false;
		_CanOpenEditor = true;
		_InsideEditingDefaultApplication = false;
		_PushedAplicationNames = new List<string>();
		_PushedAplicationNameWithSettings = false;
		_PushedAplicationIsDebug = false;
		suspendRefreshApplicationList = false;
		solutionDirty = false;
		activeWindowAttached = false;
		prevActiveContent = null;
		insideSaveAsApplication = false;
		_IsTemplateRegistryOpen = false;
		TemplateRegistryVC = null;
		_IsGeneatorOptionsOpen = false;
		_Generating = false;
		unloadAppAfterMerge = false;
		_BatchProcessorThreadHaveToWaitForCmd = false;
		_BatchProcessorThreadStoped = false;
		batchProcessGenerate = true;
		batchProcessCancelled = true;
		batchProcessPosGenActionIsBatch = false;
		batchProcessPosGenAction = PosGenerationAction.None;
		batchProcessConditionalGeneration = GenerationMode.GlobalOption;
		batchProcessDebugTraceGeneration = GenerationMode.GlobalOption;
		batchProcessStartTime = DateTime.Now;
		batchProcessStartTimeEach = DateTime.Now;
		batchProcessCurrentApplication = null;
		reparsingDelayed = new List<IProject>();
		_Applications = new List<Application>();
		Versions.VersionChanging = (VersionChangingDelegate)Delegate.Combine((Delegate)(object)Versions.VersionChanging, (Delegate)new VersionChangingDelegate(VersionChanging));
		Versions.VersionChanged = (VersionChangingDelegate)Delegate.Combine((Delegate)(object)Versions.VersionChanged, (Delegate)new VersionChangingDelegate(VersionChanged));
		Gen = Win32Generator.CreateGenerator(ClarionVersion, ClarionAddins.Win32Present);
		theGeneratorRequestor = new GeneratorRequestor();
		GeneratorRequestor generatorRequestor = theGeneratorRequestor;
		generatorRequestor.ErrorOccured = (EventHandler<BuildErrorEventArgs>)Delegate.Combine(generatorRequestor.ErrorOccured, new EventHandler<BuildErrorEventArgs>(GeneratorErrorOccured));
		Win32Generator.SetRequestor(theGeneratorRequestor);
		RefreshVerbosity();
		ProjectService.SolutionClosing += ProjectService_SolutionClosing;
		ProjectService.SolutionLoaded += ProjectService_SolutionLoaded;
		ProjectService.SolutionClosed += ProjectService_SolutionClosed;
		ProjectService.SolutionSaved += ProjectService_SolutionSaved;
		ProjectService.SolutionFolderRemoved += new SolutionFolderEventHandler(ProjectService_SolutionFolderRemoved);
		ProjectService.ProjectAdded += new ProjectEventHandler(ProjectService_ProjectAdded);
		ProjectService.StartBuild += ProjectService_StartBuild;
		ProjectService.SolutionConfigurationChanged += new SolutionConfigurationEventHandler(SolutionConfigurationChanged);
		FileService.FileRemoved += FileService_FileRemoved;
		FileService.FileRenamed += FileService_FileRenamed;
		FileService.FileRenaming += FileService_FileRenaming;
		CancelGeneration.GenerationCancelled += OnGenerationCancelledCommandRun;
		if (ProjectService.OpenSolution != null)
		{
			RefreshApplicationList();
		}
		if (WorkbenchSingleton.Workbench == null)
		{
			WorkbenchSingleton.WorkbenchCreated += OnWorkbenchCreated;
			return;
		}
		ThereIsAMainWindow = true;
		WorkbenchSingleton.Workbench.ViewOpened += new ViewContentEventHandler(OnWorkbenchViewOpened);
		WorkbenchSingleton.MainForm.Closed += OnMainFormClosed;
	}

	private static void OnMainFormClosed(object sender, EventArgs e)
	{
		WorkbenchSingleton.MainForm.Closed -= OnMainFormClosed;
		ThereIsAMainWindow = false;
	}

	private static void RefreshVerbosity()
	{
		verbosity = (LoggerVerbosity)Enum.Parse(typeof(LoggerVerbosity), PropertyService.Get<string>("SharpDevelop.LoggerVerbosity", LoggerVerbosity.Normal.ToString()));
	}

	private static void OnWorkbenchCreated(object sender, EventArgs e)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		ThereIsAMainWindow = true;
		WorkbenchSingleton.MainForm.Closed += OnMainFormClosed;
		WorkbenchSingleton.WorkbenchCreated -= OnWorkbenchCreated;
		WorkbenchSingleton.Workbench.ViewOpened += new ViewContentEventHandler(OnWorkbenchViewOpened);
	}

	private static void OnWorkbenchViewOpened(object sender, ViewContentEventArgs e)
	{
		if (e.Content is ProjectOptionsView && GetAppFromIProject(e.Content.FileName) != null)
		{
			((ICanBeDirty)e.Content).DirtyChanged += OnProjectContentSaved;
			((IBaseViewContent)e.Content).WorkbenchWindow.CloseEvent += OnWorkbenchWindowCloseEvent;
		}
	}

	private static void OnProjectContentSaved(object sender, EventArgs e)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Instance.Generating || !(sender is ProjectOptionsView))
		{
			return;
		}
		ProjectOptionsView val = (ProjectOptionsView)sender;
		if (!((AbstractViewContent)val).IsDirty)
		{
			IProject project = val.Project;
			if (project != null)
			{
				GetAppFromIProject(project)?.StoreProjectSettings(project);
			}
		}
	}

	private static void OnWorkbenchWindowCloseEvent(object sender, EventArgs e)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (sender is IWorkbenchWindow)
		{
			IWorkbenchWindow val = (IWorkbenchWindow)sender;
			val.CloseEvent -= OnWorkbenchWindowCloseEvent;
			if (val.ActiveViewContent is ProjectOptionsView)
			{
				((AbstractViewContent)(ProjectOptionsView)val.ActiveViewContent).DirtyChanged -= OnProjectContentSaved;
			}
		}
	}

	internal static void AllowClearErrors(bool value)
	{
		theGeneratorRequestor.AllowClearErrors = value;
	}

	internal static void SetTextDebug(string text)
	{
		if (Verbosity == LoggerVerbosity.Detailed || Verbosity == LoggerVerbosity.Diagnostic)
		{
			DoSetText("** " + DateTime.Now.ToString("t") + " - " + text);
		}
	}

	internal static void AppendTextDebug(string text)
	{
		if (Verbosity == LoggerVerbosity.Detailed || Verbosity == LoggerVerbosity.Diagnostic)
		{
			DoAppendText(text);
		}
	}

	internal static void SetText(string text)
	{
		DoSetText(text);
	}

	private static void DoSetText(string text)
	{
		if (Win32Generator.CommandLineLogger != null)
		{
			Win32Generator.CommandLineLogger.Message(text);
			return;
		}
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<string>((Action<string>)DoSetText, text);
			return;
		}
		if (ThereIsAMainWindow)
		{
			GeneratorCategory.AppendLine(text);
		}
		inAppendTextMode = false;
	}

	internal static void AppendText(string text)
	{
		DoAppendText(text);
	}

	private static void DoAppendText(string text)
	{
		if (Win32Generator.CommandLineLogger != null)
		{
			Win32Generator.CommandLineLogger.Message(text);
			return;
		}
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<string>((Action<string>)DoAppendText, text);
			return;
		}
		if (ThereIsAMainWindow)
		{
			GeneratorCategory.AppendText(text);
		}
		inAppendTextMode = true;
	}

	internal static void SetTextNewLine(string text)
	{
		if (inAppendTextMode)
		{
			SetText("");
		}
		SetText(text);
	}

	internal static void SetText(GeneratorError err)
	{
		if (err != GeneratorError.NoError)
		{
			if (Win32Generator.CommandLineLogger != null)
			{
				Win32Generator.CommandLineLogger.Error($"GEN2{(int)err:D2}", ResourceService.GetString("Clarion.Generator.Error." + err));
			}
			else
			{
				SetTextNewLine(ResourceService.GetString("Clarion.Generator.Error." + err));
			}
		}
	}

	internal static void ShowError(GeneratorError err)
	{
		if (err != GeneratorError.NoError)
		{
			MessageService.ShowMessage(ResourceService.GetString("Clarion.Generator.Error." + err), ResourceService.GetString("Clarion.Generator"));
		}
	}

	internal static void ThrowError(GeneratorError err)
	{
		if (err != GeneratorError.NoError)
		{
			throw new ApplicationServiceException("", ResourceService.GetString("Clarion.Generator.Error." + err));
		}
	}

	internal static void ThrowError(string applicationName, GeneratorError err)
	{
		if (err != GeneratorError.NoError)
		{
			throw new ApplicationServiceException(applicationName, err.ToString());
		}
	}

	private static void AddApplication(Application app)
	{
		ApplicationsList.Add(app);
		if (Instance.NodesInserted != null)
		{
			Instance.NodesInserted(Instance, new TreeModelEventArgs(app));
		}
		app.LoadedChanged += OnAppLoadedChanged;
		app.Closed += OnAppClosed;
		app.SavedAs += OnAppSavedAs;
	}

	private static void OnAppSavedAs(object sender, ApplicationRenamedEventArgs e)
	{
		RefreshApplicationList();
	}

	private static void OnAppClosed(object sender, ApplicationEventArgs e)
	{
		if (e == null || e.Application == null)
		{
			return;
		}
		e.Application.LoadedChanged -= OnAppLoadedChanged;
		e.Application.Closed -= OnAppClosed;
		e.Application.SavedAs -= OnAppSavedAs;
		if (!_Closing)
		{
			if (!e.Application.IsOnSolution && ApplicationsList.Contains(e.Application))
			{
				ApplicationsList.Remove(e.Application);
			}
			RefreshApplicationList();
		}
	}

	private static void OnAppLoadedChanged(object sender, ApplicationEventArgs e)
	{
		if (Instance.NodesChanged != null)
		{
			Instance.NodesChanged(Instance, new TreeModelEventArgs(e.Application));
		}
	}

	internal void CloseDown()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		try
		{
			Win32Generator.SetRequestor(null);
			Win32Generator.CloseDown();
			WorkbenchSingleton.Workbench.ViewOpened -= new ViewContentEventHandler(OnWorkbenchViewOpened);
			CancelGeneration.GenerationCancelled -= OnGenerationCancelledCommandRun;
		}
		catch
		{
		}
	}

	internal static void OpenApplication(Application app)
	{
		if (!OpeningApplication)
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				WorkbenchSingleton.SafeThreadCall<Application>((Action<Application>)OpenApplication, app);
			}
			else
			{
				app.LazyLoad(OpenApplication(app.FileName, app.QuietConvert));
			}
		}
	}

	private static Win32App OpenApplication(string fileName, bool autoConvert)
	{
		if (OpeningApplication)
		{
			return null;
		}
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<string, bool, Win32App>((Func<string, bool, Win32App>)OpenApplication, fileName, autoConvert);
		}
		try
		{
			OpeningApplication = true;
			LoggingService.Info((object)$"Opening Application {fileName}");
			DateTime now = DateTime.Now;
			Win32App win32App = Gen.OpenApplication(fileName, autoConvert);
			DateTime now2 = DateTime.Now;
			SetTextDebug($"Finish opening Application {fileName} elapsed time: {(now - now2).ToString()}");
			LoggingService.Info((object)$"Finish opening Application {fileName} elapsed time: {(now - now2).ToString()}");
			if (win32App == null)
			{
				if (Win32Generator.CommandLineLogger != null)
				{
					Win32Generator.CommandLineLogger.Error("0", "The application " + fileName + " could not be open.");
				}
				else
				{
					ErrorListPad.ShowIfErrors();
					MessageService.ShowError(ResourceService.GetString("Clarion.Generator.Error.AppLoadFailed") + "\r\nFile Name:\r\n" + fileName + "\r\n\r\nCheck for errors in the errors pad.\r\nLast Error:\r\n-" + TaskService.GetLastErrorDescription());
					ErrorListPad.ShowIfErrors();
				}
			}
			else
			{
				ProjectService.CurrentProject = ProjectService.GetProject(ProjectFileName(fileName, win32App.Language));
			}
			return win32App;
		}
		finally
		{
			OpeningApplication = false;
		}
	}

	public static void ApplicationFrameOpened()
	{
		FileSchemaPad.Pad.Resume();
	}

	public static void ApplicationFrameClosed()
	{
	}

	internal static bool IsValidFileName(string fileName)
	{
		if (FileUtility.IsValidFileName(fileName))
		{
			return Path.GetExtension(fileName).ToLowerInvariant() == ".app";
		}
		return false;
	}

	internal static IProject LoadProject(IMSBuildEngineProvider engineProvider, string fileName, string projectName)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		if (IsTemplateRegistryOpen)
		{
			MessageService.ShowMessage(ResourceService.GetString("Clarion.Generator.Error.RegistryInEdit"), ResourceService.GetString("Clarion.Generator"));
			return null;
		}
		if (IsValidApplicationFile(fileName))
		{
			PRJFile appProject = null;
			string appLanguage = string.Empty;
			GetApplicationProjectFile(fileName, out appProject, out appLanguage);
			LanguageBindingDescriptor codonPerLanguageName = LanguageBindingService.GetCodonPerLanguageName(appLanguage);
			if (codonPerLanguageName != null)
			{
				string text = Path.ChangeExtension(fileName, codonPerLanguageName.ProjectFileExtension);
				string text2 = projectName;
				if (text2 == string.Empty || text2 == null)
				{
					text2 = Path.GetFileNameWithoutExtension(fileName);
				}
				IProject val = ProjectService.GetProject(text);
				if (val == null)
				{
					if (!File.Exists(text))
					{
						ProjectCreateInformation val2 = new ProjectCreateInformation();
						val2.SolutionPath = Path.GetDirectoryName(fileName);
						val2.ProjectBasePath = Path.GetDirectoryName(fileName);
						val2.ProjectName = text2;
						val2.Solution = (Solution)engineProvider;
						val2.OutputProjectFileName = text;
						val = codonPerLanguageName.Binding.CreateProject(val2);
						ProjectsMerger.Merge(appProject, appProject, val, initial: true, appLanguage, saveTargetIprj: true);
					}
					else
					{
						val = codonPerLanguageName.Binding.LoadProject(engineProvider, text, text2);
					}
				}
				PushApplication(fileName);
				return val;
			}
		}
		return null;
	}

	public static void LoadApplication(string fileName)
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		if (IsTemplateRegistryOpen)
		{
			MessageService.ShowMessage(ResourceService.GetString("Clarion.Generator.Error.RegistryInEdit"), ResourceService.GetString("Clarion.Generator"));
		}
		else if (IsValidApplicationFile(fileName))
		{
			StatusBarService.SetMessage($"Loading: {fileName}");
			try
			{
				PRJFile appProject = null;
				string appLanguage = string.Empty;
				if (!GetApplicationProjectFile(fileName, out appProject, out appLanguage))
				{
					throw new ApplicationServiceException(fileName, $"The application {fileName} does not have a valid. The language is empty.");
				}
				LanguageBindingDescriptor codonPerLanguageName = LanguageBindingService.GetCodonPerLanguageName(appLanguage);
				if (codonPerLanguageName == null)
				{
					throw new ApplicationServiceException(fileName, $"There are no LanguageBindingDescriptor for the application {fileName} using the language ({appLanguage})");
				}
				string text = Path.ChangeExtension(fileName, codonPerLanguageName.ProjectFileExtension);
				if (!File.Exists(text))
				{
					if (!SuspendRefreshApplicationList && ApplicationsList.Count == 1 && fileName.Equals(ApplicationsList[0].FileName))
					{
						PushApplicationSettings(fileName, ApplicationsList[0].BuildingDebug);
					}
					if (ProjectService.OpenSolution != null)
					{
						bool flag = SuspendRefreshApplicationList;
						SuspendRefreshApplicationList = true;
						ProjectService.SaveSolution();
						SuspendRefreshApplicationList = flag;
						ProjectService.CloseSolution();
					}
					ProjectCreateInformation val = new ProjectCreateInformation();
					val.SolutionPath = Path.GetDirectoryName(fileName);
					val.ProjectBasePath = Path.GetDirectoryName(fileName);
					val.ProjectName = Path.GetFileNameWithoutExtension(fileName);
					val.OutputProjectFileName = Path.GetFullPath(text);
					val.Solution = new Solution();
					ILanguageBinding binding = codonPerLanguageName.Binding;
					IProject targetIprj = binding.CreateProject(val);
					ProjectsMerger.Merge(appProject, appProject, targetIprj, initial: true, appLanguage, saveTargetIprj: true);
					if (ProjectService.OpenSolution != null)
					{
						bool flag2 = SuspendRefreshApplicationList;
						SuspendRefreshApplicationList = true;
						ProjectServiceOpenSolutionSave();
						SuspendRefreshApplicationList = flag2;
						ProjectService.CloseSolution();
					}
				}
				if (!SuspendRefreshApplicationList)
				{
					PushApplication(fileName);
					if (ProjectService.GetProject(text) == null)
					{
						ProjectService.LoadProject(text);
					}
				}
				else if (ProjectService.GetProject(text) == null)
				{
					AbstractProjectBrowserTreeNode solutionNode = ProjectBrowserPad.Instance.SolutionNode;
					ISolutionFolderNode val2 = (ISolutionFolderNode)(object)((solutionNode is ISolutionFolderNode) ? solutionNode : null);
					if (val2 != null)
					{
						IProject val3 = codonPerLanguageName.Binding.LoadProject((IMSBuildEngineProvider)(object)ProjectService.OpenSolution, text, Path.GetFileNameWithoutExtension(text));
						if (val3 != null)
						{
							((ISolutionFolder)val3).Location = FileUtility.GetRelativePath(val2.Solution.Directory, text);
							ProjectService.AddProject(val2, val3);
							NodeBuilders.AddProjectNode((TreeNode)(object)val2, val3).EnsureVisible();
							val2.Solution.ApplySolutionConfigurationAndPlatformToProjects();
							needToReloadSolution = true;
						}
					}
				}
			}
			catch (ApplicationServiceException ex)
			{
				if (Gen.ErrorCode != GeneratorError.AppLoadCanceled)
				{
					MessageService.ShowMessage($"The application {ex.ApplicationName} couldn't be loaded. Check for missing templates or invalid app file.{ex.Message}", ResourceService.GetString("Clarion.Generator"));
				}
				return;
			}
			inLoad = true;
			ProjectService.SaveSolution();
			inLoad = false;
			if (!ProjectsMerger.Merging && ApplicationsList.Count == 1 && ApplicationServiceSettings.OnSolutionLoadedEditApp)
			{
				EditDefaultApplication();
			}
		}
		else
		{
			MessageService.ShowMessage($"The file name {fileName} is not valid for an application", ResourceService.GetString("Clarion.Generator"));
		}
	}

	public static Application FindApplication(string fileName)
	{
		foreach (Application application in Applications)
		{
			if (FileUtility.IsEqualFileName(fileName, application.FileName))
			{
				return application;
			}
		}
		return null;
	}

	internal static Application FetchApplication(string fileName)
	{
		if (IsTemplateRegistryOpen)
		{
			return null;
		}
		Application result = null;
		if (IsValidApplicationFile(fileName))
		{
			Application application = FindApplication(fileName);
			if (application == null && OnApplicationLoading(fileName))
			{
				application = new Application(fileName, null);
				if (!string.IsNullOrEmpty(application.Name))
				{
					AddApplication(application);
				}
			}
			result = application;
		}
		return result;
	}

	internal static void OpenDictionary(Application app)
	{
		if (OpeningDictionary)
		{
			return;
		}
		OpeningDictionary = true;
		try
		{
			if (app == null || IsTemplateRegistryOpen || !CanOpenEditor)
			{
				return;
			}
			string dictionaryFileName = app.DictionaryFileName;
			if (string.IsNullOrEmpty(dictionaryFileName))
			{
				return;
			}
			IWorkbenchWindow openFile = FileService.GetOpenFile(dictionaryFileName);
			if (openFile != null)
			{
				openFile.SelectWindow();
				return;
			}
			openFile = FileService.OpenFile(dictionaryFileName);
			if (openFile != null)
			{
				openFile.SelectWindow();
			}
		}
		finally
		{
			OpeningDictionary = false;
		}
	}

	public static bool GetCanOpenEditor(string fileName)
	{
		if (CanOpenEditor && !IsTemplateRegistryOpen)
		{
			Application application = FindApplication(fileName);
			if (application != null)
			{
				return application.IsOnSolution;
			}
		}
		return false;
	}

	internal static void EditApplication(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName) && !IsTemplateRegistryOpen && CanOpenEditor)
		{
			EditApplication(FindApplication(fileName));
		}
	}

	internal static void EditApplication(Application app)
	{
		if (app != null && !IsTemplateRegistryOpen && CanOpenEditor)
		{
			System.Windows.Forms.Application.DoEvents();
			WaitForParser();
			if (WorkbenchSingleton.MainForm != null)
			{
				WorkbenchSingleton.MainForm.Focus();
			}
			IWorkbenchWindow openFile = FileService.GetOpenFile(app.FileName);
			if (openFile != null)
			{
				openFile.SelectWindow();
			}
			else
			{
				FileService.OpenFile(app.FileName);
			}
			SelectApplicationProject(app);
		}
	}

	internal static void EditDefaultApplication()
	{
		if (!_InsideEditingDefaultApplication)
		{
			_InsideEditingDefaultApplication = true;
			if (WorkbenchSingleton.MainForm != null)
			{
				EditApplication(Instance.DefaultApp);
			}
			_InsideEditingDefaultApplication = false;
		}
	}

	internal static void PushApplication(string fileName)
	{
		if (!_PushedAplicationNames.Contains(fileName))
		{
			_PushedAplicationNames.Add(fileName);
		}
	}

	private static void RemovePushedAplication(string appName)
	{
		_PushedAplicationNames.Remove(appName);
	}

	private static void ClearPushedApplications()
	{
		if (_PushedAplicationNames != null)
		{
			_PushedAplicationNames.Clear();
		}
	}

	internal static void PushApplicationSettings(string fileName, bool isDebug)
	{
		if (ApplicationsList.Count == 1 && !_PushedAplicationNameWithSettings)
		{
			_PushedAplicationNameWithSettings = true;
			_PushedAplicationIsDebug = isDebug;
		}
	}

	internal static void WaitForParser()
	{
	}

	internal static void MergeApplicationSetting()
	{
		if (_PushedAplicationNameWithSettings)
		{
			_PushedAplicationNameWithSettings = false;
			StatusBarService.SetMessage("Setting Project Configuration");
			if (_PushedAplicationIsDebug)
			{
				ProjectService.SetConfiguration("Debug");
			}
			else
			{
				ProjectService.SetConfiguration("Release");
			}
			WaitForParser();
			StatusBarService.ClearMessage();
		}
	}

	private static void RefreshApplicationList()
	{
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Expected O, but got Unknown
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Expected O, but got Unknown
		if (SuspendRefreshApplicationList)
		{
			return;
		}
		SetTextDebug("Start Refresh Application List");
		solutionDirty = false;
		Solution openSolution = ProjectService.OpenSolution;
		if (openSolution != null)
		{
			List<SolutionItem> list = new List<SolutionItem>();
			ISolutionFolder[] array = ((SolutionFolder)openSolution).Folders.ToArray();
			foreach (object obj in array)
			{
				if (obj is SolutionFolder)
				{
					list.AddRange(GetFolderApps((SolutionFolder)((obj is SolutionFolder) ? obj : null)));
				}
			}
			if (IsTemplateRegistryOpen && list.Count > 0)
			{
				MessageService.ShowMessage(ResourceService.GetString("Clarion.Generator.Error.RegistryInEdit"), ResourceService.GetString("Clarion.Generator"));
				throw new ApplicationServiceException(list[0].Name, ResourceService.GetString("Clarion.Generator.Error.RegistryInEdit"));
			}
			for (int num = ApplicationsList.Count - 1; num >= 0; num--)
			{
				bool flag = false;
				Application application = ApplicationsList[num];
				foreach (SolutionItem item in list)
				{
					string fullPath = Path.GetFullPath(Path.Combine(openSolution.Directory, item.Location));
					if (application.FileName == fullPath)
					{
						flag = true;
						if (!application.IsOnSolution)
						{
							application.LinkToSolution(item);
							solutionDirty = true;
						}
						break;
					}
				}
				if (!flag && application.IsOnSolution)
				{
					_Closing = true;
					IProject project = ProjectService.GetProject(ProjectFileName(Path.GetFullPath(application.FileName), application.Language));
					OnRemovingFromList(new ApplicationEventArgs(application));
					application.Close(forceClose: false);
					application.Dispose();
					ApplicationsList.RemoveAt(num);
					if (project != null)
					{
						ProjectService.RemoveSolutionFolder(((ISolutionFolder)project).IdGuid);
						solutionDirty = true;
					}
					_Closing = false;
				}
			}
			foreach (SolutionItem item2 in list)
			{
				string fullPath = Path.GetFullPath(Path.Combine(openSolution.Directory, item2.Location));
				if (IsApplicationOnService(fullPath) || !OnApplicationLoading(fullPath))
				{
					continue;
				}
				if (!File.Exists(fullPath))
				{
					MessageService.ShowMessage("The application file was not found." + Environment.NewLine + "File Name:" + fullPath, "File Error");
					continue;
				}
				IProject projectWithOutTypeHint = ProjectService.GetProjectWithOutTypeHint(fullPath);
				if (projectWithOutTypeHint != null)
				{
					if (projectWithOutTypeHint is MissingProject)
					{
						PRJFile appProject = null;
						string appLanguage = string.Empty;
						if (GetApplicationProjectFile(fullPath, out appProject, out appLanguage))
						{
							string fileName = projectWithOutTypeHint.FileName;
							ProjectService.RemoveSolutionFolder(((ISolutionFolder)projectWithOutTypeHint).IdGuid);
							ProjectCreateInformation val = new ProjectCreateInformation();
							val.SolutionPath = Path.GetDirectoryName(fileName);
							val.ProjectBasePath = Path.GetDirectoryName(fileName);
							val.ProjectName = Path.GetFileNameWithoutExtension(fileName);
							val.OutputProjectFileName = Path.GetFullPath(fileName);
							val.Solution = ProjectService.OpenSolution;
							LanguageBindingDescriptor codonPerLanguageName = LanguageBindingService.GetCodonPerLanguageName(appLanguage);
							ILanguageBinding binding = codonPerLanguageName.Binding;
							projectWithOutTypeHint = binding.CreateProject(val);
							ProjectsMerger.Merge(appProject, appProject, projectWithOutTypeHint, initial: true, appLanguage, saveTargetIprj: true);
							AddExitingProjectToSolution.AddProject((ISolutionFolderNode)ProjectBrowserPad.Instance.SolutionNode, fileName);
							ProjectService.SaveSolution();
						}
					}
					Application application2 = null;
					application2 = FindApplication(fullPath);
					if (application2 == null)
					{
						application2 = new Application(fullPath, item2);
						if (!string.IsNullOrEmpty(application2.Name))
						{
							AddApplication(application2);
						}
					}
					else if (!application2.IsOnSolution)
					{
						application2.LinkToSolution(item2);
					}
					solutionDirty = true;
				}
				else
				{
					SuspendRefreshApplicationList = true;
					LoadApplication(fullPath);
					solutionDirty = true;
					SuspendRefreshApplicationList = false;
				}
			}
		}
		else if (ApplicationsList != null && ApplicationsList.Count > 0)
		{
			_Closing = true;
			for (int num2 = ApplicationsList.Count - 1; num2 >= 0; num2--)
			{
				Application application3 = ApplicationsList[num2];
				if (application3.IsOnSolution)
				{
					OnRemovingFromList(new ApplicationEventArgs(application3));
					application3.Close(forceClose: true);
					application3.Dispose();
					ApplicationsList.RemoveAt(num2);
					solutionDirty = true;
				}
			}
			_Closing = false;
		}
		Instance.TreeModelDataChanged();
		if (solutionDirty)
		{
			solutionDirty = false;
			if (ProjectService.OpenSolution != null)
			{
				SuspendRefreshApplicationList = true;
				ProjectServiceOpenSolutionSave();
				SuspendRefreshApplicationList = false;
			}
		}
		SetTextDebug("End Refresh Application List");
	}

	internal static bool OnApplicationLoading(string fileName)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<string, bool>((Func<string, bool>)OnApplicationLoading, fileName);
		}
		if (ApplicationService.ApplicationLoading != null)
		{
			ApplicationLoadingEventArgs e = new ApplicationLoadingEventArgs(fileName);
			ApplicationService.ApplicationLoading(null, e);
			return !e.Cancel;
		}
		return true;
	}

	private static void OnRemovingFromList(ApplicationEventArgs e)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<ApplicationEventArgs>((Action<ApplicationEventArgs>)OnRemovingFromList, e);
		}
		else if (ApplicationService.RemovingFromList != null)
		{
			ApplicationService.RemovingFromList(null, e);
		}
	}

	private static List<SolutionItem> GetFolderApps(SolutionFolder folder)
	{
		List<SolutionItem> list = new List<SolutionItem>();
		if (folder != null)
		{
			for (int i = 0; i < folder.Sections.Count; i++)
			{
				ProjectSection val = folder.Sections[i];
				for (int j = 0; j < val.Items.Count; j++)
				{
					SolutionItem val2 = val.Items[j];
					if (string.Equals(Path.GetExtension(val2.Name), ".app", StringComparison.InvariantCultureIgnoreCase))
					{
						list.Add(val2);
					}
				}
			}
			if (folder.Folders != null)
			{
				for (int k = 0; k < folder.Folders.Count; k++)
				{
					ISolutionFolder val3 = folder.Folders[k];
					if (val3 is SolutionFolder)
					{
						list.AddRange(GetFolderApps((SolutionFolder)(object)((val3 is SolutionFolder) ? val3 : null)));
					}
				}
			}
		}
		return list;
	}

	private static void RefreshSolutionPad()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(new Action(RefreshSolutionPad));
		}
		else if (((AbstractPadContent)ProjectBrowserPad.Instance).IsVisible)
		{
			if (ApplicationServiceSettings.RestoreSolutionViewState)
			{
				ProjectBrowserPad.Instance.ProjectBrowserControl.RefreshView();
			}
			else
			{
				ProjectBrowserPad.Instance.ProjectBrowserControl.RefreshViewOpenSolution();
			}
		}
		else
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.RefreshViewOpenSolution();
		}
	}

	internal static void ReLink(string fileName, SolutionItem si)
	{
		if (!insideSaveAsApplication)
		{
			return;
		}
		Win32App win32App = OpenApplication(fileName, autoConvert: false);
		if (win32App != null)
		{
			Application application = new Application(fileName, win32App, si);
			if (!string.IsNullOrEmpty(application.Name))
			{
				AddApplication(application);
			}
		}
	}

	internal static bool GetApplicationProjectFile(string applicationFileName, out PRJFile appProject, out string appLanguage)
	{
		Application application = FetchApplication(applicationFileName);
		if (application != null)
		{
			if (!application.AppPrjInited && application.CheckLazyLoad())
			{
				application.Unload(11L);
			}
			appProject = application.AppPrj;
			appLanguage = application.Language;
			return true;
		}
		appProject = null;
		appLanguage = "";
		return false;
	}

	internal static string GetApplicationLanguage(string applicationFileName)
	{
		string text = "Clarion";
		foreach (Application application in Applications)
		{
			if (application.FileName.Equals(applicationFileName, StringComparison.InvariantCultureIgnoreCase))
			{
				text = application.Language;
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
		}
		return "Clarion";
	}

	internal static bool ApplicationFileExists(string applicationFileName)
	{
		if (!File.Exists(applicationFileName))
		{
			if (File.Exists(Path.ChangeExtension(applicationFileName, ApplicationInportFileExtension)))
			{
				return File.Exists(ApplicationAutoImportExportLocalFile);
			}
			return false;
		}
		return true;
	}

	internal static bool IsValidApplicationFile(string applicationFileName)
	{
		if (!IsValidFileName(applicationFileName))
		{
			if (Win32Generator.CommandLineLogger != null)
			{
				Win32Generator.CommandLineLogger.Warning("GENW002", string.Format(ResourceService.GetString("Clarion.Generator.Application.InvalidName"), applicationFileName));
			}
			return false;
		}
		if (!ApplicationFileExists(applicationFileName))
		{
			if (Win32Generator.CommandLineLogger != null)
			{
				Win32Generator.CommandLineLogger.Error("GENE100", string.Format(ResourceService.GetString("Clarion.Generator.Application.NotFound"), applicationFileName));
			}
			return false;
		}
		return true;
	}

	internal static string ProjectFileName(string applicationFileName)
	{
		if (GetApplicationProjectFile(applicationFileName, out var _, out var appLanguage))
		{
			return ProjectFileName(applicationFileName, appLanguage);
		}
		return ProjectFileName(applicationFileName, GetApplicationLanguage(applicationFileName));
	}

	internal static string ProjectFileName(string applicationFileName, string language)
	{
		return ProjectFileName(applicationFileName, LanguageBindingService.GetCodonPerLanguageName(language));
	}

	internal static string ProjectFileName(string applicationFileName, LanguageBindingDescriptor languageBinding)
	{
		if (IsValidFileName(applicationFileName))
		{
			if (languageBinding != null)
			{
				return Path.Combine(Path.GetDirectoryName(applicationFileName), Path.GetFileNameWithoutExtension(applicationFileName) + languageBinding.ProjectFileExtension);
			}
			throw new ApplicationServiceException(applicationFileName, string.Format(ResourceService.GetString("Clarion.Generator.ApplicationService.Exception.NoBinding"), applicationFileName, "null"));
		}
		return string.Empty;
	}

	internal static void SelectApplicationProject(Application app)
	{
		if (!app.IsOnSolution)
		{
			return;
		}
		IProject projectServiceProject = app.GetProjectServiceProject();
		if (projectServiceProject == null || ProjectBrowserPad.Instance.SolutionNode == null)
		{
			return;
		}
		foreach (TreeNode node in ((TreeNode)(object)ProjectBrowserPad.Instance.SolutionNode).Nodes)
		{
			ProjectNode val = (ProjectNode)(object)((node is ProjectNode) ? node : null);
			if (val != null)
			{
				object tag = ((TreeNode)(object)val).Tag;
				IProject val2 = (IProject)((tag is IProject) ? tag : null);
				if (val2 != null && val2.FileName == projectServiceProject.FileName)
				{
					((TreeView)(object)ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView).SelectedNode = node;
					break;
				}
			}
		}
	}

	internal static bool RemoveSolutionItem(SolutionItem si)
	{
		bool flag = false;
		foreach (TreeNode node in ((TreeNode)(object)ProjectBrowserPad.Instance.SolutionNode).Nodes)
		{
			SolutionFolderNode val = (SolutionFolderNode)(object)((node is SolutionFolderNode) ? node : null);
			if (val != null)
			{
				flag = RemoveSolutionItem(val, si);
				if (flag)
				{
					ProjectServiceOpenSolutionSave();
					ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.ClearCutNodes();
					break;
				}
			}
		}
		return flag;
	}

	private static bool RemoveSolutionItem(SolutionFolderNode folder, SolutionItem si)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		bool flag = false;
		foreach (TreeNode node in ((TreeNode)(object)folder).Nodes)
		{
			if (node is SolutionFolderNode)
			{
				flag = RemoveSolutionItem((SolutionFolderNode)node, si);
				if (flag)
				{
					break;
				}
			}
			else if (node is SolutionItemNode)
			{
				SolutionItemNode val = (SolutionItemNode)node;
				if (val.SolutionItem == si)
				{
					((ExtTreeNode)val).Delete();
					flag = true;
					break;
				}
			}
		}
		return flag;
	}

	internal static bool IsApplicationOnService(string fileName)
	{
		bool result = false;
		foreach (Application application in Applications)
		{
			if (FileUtility.IsEqualFileName(fileName, application.FileName))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	internal static bool IsApplicationOnService(Application application)
	{
		bool result = false;
		foreach (Application application2 in Applications)
		{
			if (application2 == application)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private static void VersionChanging(string version, bool forWindows)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		if (IsTemplateRegistryOpen && ApplicationsList.Count != 0)
		{
			flag = false;
		}
		if (ApplicationsList.Count != 0 && flag)
		{
			foreach (Application application in Applications)
			{
				if (application.InEdit)
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			MessageBox.Show(ResourceService.GetString("Clarion.Generator.NoVersionChange"), ResourceService.GetString("Clarion.Generator"), MessageBoxButtons.OK);
			throw new VersionChangedNotAllowedException();
		}
	}

	private static void VersionChanged(string version, bool forWindows)
	{
		if (!forWindows)
		{
			return;
		}
		ClearCachedABCFilesMenuCommand.DoRun();
		Gen = Win32Generator.CreateGenerator(version, forWindows);
		try
		{
			foreach (Application application in Applications)
			{
				if ((forWindows && application.Language == "Clarion") || (!forWindows && application.Language != "Clarion"))
				{
					application.GeneratorVersionChanged(Gen);
				}
			}
		}
		catch (ApplicationServiceException)
		{
			ProjectService.CloseSolution();
		}
	}

	private static void OnSolutionClosed()
	{
		Task.NewTaskEvent = (EventHandler<NewTaskEventArgs>)Delegate.Remove(Task.NewTaskEvent, new EventHandler<NewTaskEventArgs>(NewBuildTask));
		RefreshApplicationList();
		Win32Generator.SetSolutionOpen(isOpen: false);
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	private static void OnSolutionSaved()
	{
		if (!inLoad)
		{
			RefreshApplicationList();
		}
	}

	private static void OnSolutionLoaded()
	{
		Win32Generator.SetSolutionOpen(isOpen: true);
		StatusBarService.ClearMessage();
		Task.NewTaskEvent = (EventHandler<NewTaskEventArgs>)Delegate.Combine(Task.NewTaskEvent, new EventHandler<NewTaskEventArgs>(NewBuildTask));
		MergeApplicationSetting();
		Versions.SetActiveVersionFromSolution();
		WaitForParser();
		if (PushedAplicationNamesCount > 0)
		{
			if (IsTemplateRegistryOpen)
			{
				ClearPushedApplications();
				MessageService.ShowMessage(ResourceService.GetString("Clarion.Generator.Error.RegistryInEdit"), ResourceService.GetString("Clarion.Generator"));
				throw new ApplicationServiceException(FirstPushedAplicationName, ResourceService.GetString("Clarion.Generator.Error.RegistryInEdit"));
			}
			while (PushedAplicationNamesCount > 0)
			{
				string firstPushedAplicationName = FirstPushedAplicationName;
				AddAppToSolution(firstPushedAplicationName);
				RemovePushedAplication(firstPushedAplicationName);
			}
		}
		RefreshApplicationList();
		WaitForParser();
		if (!ProjectsMerger.Merging && ApplicationsList.Count == 1 && ApplicationServiceSettings.OnSolutionLoadedEditApp)
		{
			EditDefaultApplication();
		}
		ApplicationListCurrentSort = ApplicationServiceSettings.DefaultApplicationsListSort;
	}

	private static void NewBuildTask(object sender, NewTaskEventArgs e)
	{
		if (e.Task != null)
		{
			return;
		}
		IProject val = ProjectService.OpenSolution.FindProjectContainingFile(e.BuildError.FileName);
		if (val != null)
		{
			Application application = FindApplication(Path.GetFileNameWithoutExtension(val.FileName) + ".app");
			if (application != null)
			{
				e.Task = (Task)(object)new ApplicationBuildErrorTask(application, e.BuildError);
				application.AddError(e.BuildError);
			}
		}
	}

	private static void ProjectServiceOpenSolutionSave()
	{
		if (!string.IsNullOrEmpty(ProjectService.OpenSolution.FileName))
		{
			FileAttributes fileAttributes = FileAttributes.Normal;
			if (File.Exists(ProjectService.OpenSolution.FileName))
			{
				fileAttributes = File.GetAttributes(ProjectService.OpenSolution.FileName);
			}
			if ((fileAttributes & FileAttributes.ReadOnly) != 0)
			{
				SetText("The solution is read only can not be saved.");
			}
			else
			{
				ProjectService.OpenSolution.Save();
			}
		}
	}

	private static void ProjectService_SolutionSaved(object sender, SolutionEventArgs e)
	{
		OnSolutionSaved();
	}

	private static void ProjectService_SolutionClosing(object sender, SolutionCancelEventArgs e)
	{
		foreach (Application application in Applications)
		{
			if (application.InEdit)
			{
				((CancelEventArgs)(object)e).Cancel = true;
				return;
			}
		}
		ProjectServiceOpenSolutionSave();
	}

	private static void ProjectService_SolutionClosed(object sender, EventArgs e)
	{
		OnSolutionClosed();
	}

	private static void ProjectService_SolutionLoaded(object sender, SolutionEventArgs e)
	{
		needToReloadSolution = false;
		if (!activeWindowAttached && WorkbenchSingleton.Workbench != null)
		{
			WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += ActiveWindowChanged;
			activeWindowAttached = true;
		}
		OnSolutionLoaded();
		if (needToReloadSolution)
		{
			ProjectBrowserPad.Instance.RefreshSolution();
		}
		ProjectBrowserPad.Instance.RefreshSolution();
	}

	private static void AddAppToSolution(string appFileName)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValidFileName(appFileName) || !ApplicationFileExists(appFileName))
		{
			return;
		}
		Solution openSolution = ProjectService.OpenSolution;
		if (openSolution == null)
		{
			return;
		}
		SolutionFolder val = (SolutionFolder)(object)openSolution;
		if (val == null)
		{
			return;
		}
		string relativePath = FileUtility.GetRelativePath(openSolution.Directory, appFileName);
		bool flag = false;
		foreach (SolutionItem item2 in val.SolutionItems.Items)
		{
			if (item2.Name.Equals(relativePath, StringComparison.OrdinalIgnoreCase) && item2.Location.Equals(relativePath, StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		SolutionItem item = new SolutionItem(relativePath, relativePath);
		val.SolutionItems.Items.Add(item);
		string text = ProjectFileName(appFileName);
		IProject val2 = ProjectService.GetProject(text);
		if (val2 == null)
		{
			if (File.Exists(text))
			{
				ILanguageBinding bindingPerProjectFile = LanguageBindingService.GetBindingPerProjectFile(text);
				if (bindingPerProjectFile != null)
				{
					val2 = LanguageBindingService.LoadProject((IMSBuildEngineProvider)(object)openSolution, text, Path.GetFileNameWithoutExtension(text));
				}
			}
			else
			{
				val2 = LoadProject((IMSBuildEngineProvider)(object)openSolution, text, Path.GetFileNameWithoutExtension(text));
			}
			if (val2 != null)
			{
				if (val2 is UnknownProject && !((UnknownProject)val2).WarningDisplayedToUser)
				{
					((UnknownProject)val2).ShowWarningMessageBox();
				}
				((SolutionFolder)openSolution).AddFolder((ISolutionFolder)(object)val2);
				foreach (ProjectItem item3 in val2.Items)
				{
					ProjectReferenceProjectItem val3 = (ProjectReferenceProjectItem)(object)((item3 is ProjectReferenceProjectItem) ? item3 : null);
					if (val3 != null)
					{
						string text2 = Path.ChangeExtension(((ReferenceProjectItem)val3).Name, ".app");
						if (File.Exists(text2))
						{
							PushApplication(text2);
						}
					}
				}
			}
		}
		ProjectServiceOpenSolutionSave();
		needToReloadSolution = true;
	}

	private static void OnProjectAdded(string projectFileName)
	{
		if (PushedAplicationNamesCount <= 0)
		{
			return;
		}
		foreach (string pushedAplicationName in PushedAplicationNames)
		{
			if (string.Equals(Path.ChangeExtension(projectFileName, ".APP"), Path.ChangeExtension(pushedAplicationName, ".APP")))
			{
				string text = pushedAplicationName;
				if (!SuspendRefreshApplicationList)
				{
					AddAppToSolution(text);
				}
				RemovePushedAplication(text);
				break;
			}
		}
		RefreshSolutionPad();
	}

	private static void SolutionConfigurationChanged(object sender, SolutionConfigurationEventArgs e)
	{
		foreach (Application application in Applications)
		{
			application.SetDebugState();
		}
	}

	private static void ProjectService_StartBuild(object sender, EventArgs args)
	{
	}

	private static void ProjectService_ProjectAdded(object sender, ProjectEventArgs e)
	{
		OnProjectAdded(e.Project.FileName);
	}

	private static void ProjectService_SolutionFolderRemoved(object sender, SolutionFolderEventArgs e)
	{
	}

	private static Application GetAppFromIProject(string iPrjName)
	{
		if (!string.IsNullOrEmpty(iPrjName))
		{
			foreach (Application application in Applications)
			{
				if (application.IsOnSolution && application.ProjectFileName.Equals(iPrjName, StringComparison.InvariantCultureIgnoreCase))
				{
					return application;
				}
			}
		}
		return null;
	}

	public static bool IsIProjectOfApplication(IProject iPrj)
	{
		Application appFromIProject = GetAppFromIProject(iPrj);
		if (appFromIProject != null)
		{
			return true;
		}
		return false;
	}

	public static bool IsIProjectOfEditedApplication(IProject iPrj)
	{
		return GetAppFromIProject(iPrj)?.InEdit ?? false;
	}

	public static Application GetAppFromIProject(IProject iPrj)
	{
		if (iPrj != null)
		{
			for (int i = 0; i < ApplicationsList.Count; i++)
			{
				Application application = ApplicationsList[i];
				if (application.IsOnSolution && application.ProjectFileName.Equals(iPrj.FileName, StringComparison.InvariantCultureIgnoreCase))
				{
					return application;
				}
			}
		}
		return null;
	}

	public static bool IsMatchingApplicationInSolution(IProject iPrj)
	{
		if (IsIProjectOfApplication(iPrj))
		{
			return true;
		}
		Solution openSolution = ProjectService.OpenSolution;
		if (openSolution != null)
		{
			List<SolutionItem> list = new List<SolutionItem>();
			for (int i = 0; i < ((SolutionFolder)openSolution).Folders.Count; i++)
			{
				object obj = ((SolutionFolder)openSolution).Folders[i];
				if (obj is SolutionFolder)
				{
					list.AddRange(GetFolderApps((SolutionFolder)((obj is SolutionFolder) ? obj : null)));
				}
			}
			if (list.Count > 0)
			{
				string text = Path.Combine(Path.GetDirectoryName(iPrj.FileName), Path.GetFileNameWithoutExtension(iPrj.FileName) + ".app");
				string text2 = null;
				foreach (SolutionItem item in list)
				{
					if (!string.IsNullOrEmpty(item.Location))
					{
						text2 = Path.GetFullPath(Path.Combine(openSolution.Directory, item.Location));
						if (text.Equals(text2, StringComparison.InvariantCultureIgnoreCase))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private static void FileService_FileRemoved(object sender, FileEventArgs e)
	{
		if (IsApplicationOnService(e.FileName))
		{
			RefreshApplicationList();
			SuspendRefreshApplicationList = true;
			ProjectServiceOpenSolutionSave();
			SuspendRefreshApplicationList = false;
		}
	}

	private static void FileService_FileRenaming(object sender, FileRenamingEventArgs e)
	{
		if (string.Equals(Path.GetExtension(((FileRenameEventArgs)e).SourceFile), ".app", StringComparison.InvariantCultureIgnoreCase) && !string.Equals(Path.GetExtension(((FileRenameEventArgs)e).TargetFile), ".app", StringComparison.InvariantCultureIgnoreCase))
		{
			e.Cancel = true;
			e.OperationAlreadyDone = true;
			MessageBox.Show("The extension for an app must be .app");
		}
	}

	private static void FileService_FileRenamed(object sender, FileRenameEventArgs e)
	{
		string.Equals(Path.GetExtension(e.SourceFile), ".app", StringComparison.InvariantCultureIgnoreCase);
	}

	private static void ActiveWindowChanged(object sender, EventArgs e)
	{
		object activeContent = WorkbenchSingleton.Workbench.ActiveContent;
		if (prevActiveContent == activeContent)
		{
			return;
		}
		prevActiveContent = activeContent;
		if (activeContent is ApplicationMainWindowControl_ViewContent { App: { } app })
		{
			IProject projectServiceProject = app.GetProjectServiceProject();
			if (projectServiceProject != null)
			{
				ProjectService.CurrentProject = projectServiceProject;
			}
		}
	}

	private static void GeneratorErrorOccured(object sender, BuildErrorEventArgs args)
	{
		if (ErrorOccured != null)
		{
			ErrorOccured(sender, args);
		}
	}

	internal static ITreeModel ApplicationBrowserTreeModel()
	{
		return Instance;
	}

	private void TreeModelDataChanged()
	{
		if (this.StructureChanged != null)
		{
			this.StructureChanged(this, new TreePathEventArgs());
		}
	}

	public IEnumerable GetChildren(TreePath treePath)
	{
		if (treePath.IsEmpty())
		{
			return Applications;
		}
		return null;
	}

	public bool IsLeaf(TreePath treePath)
	{
		return true;
	}

	internal static bool RenameApplication(string oldFileName, string newFileName)
	{
		if (IsValidFileName(oldFileName) && File.Exists(oldFileName) && IsValidFileName(newFileName) && !File.Exists(newFileName))
		{
			Application application = FindApplication(oldFileName);
			if (application != null)
			{
				return application.Rename(newFileName);
			}
		}
		return false;
	}

	internal static bool SaveApplication(string fileName)
	{
		return FindApplication(fileName)?.Save() ?? false;
	}

	internal static bool SaveAsApplication(string oldFileName, string newFileName)
	{
		if (IsValidFileName(newFileName))
		{
			Application application = FindApplication(oldFileName);
			if (application != null)
			{
				insideSaveAsApplication = true;
				bool result = application.SaveAs(newFileName);
				insideSaveAsApplication = false;
				Instance.TreeModelDataChanged();
				return result;
			}
		}
		return false;
	}

	internal static void EditTemplateRegistry(bool forWindows, string directory)
	{
		if (!AreApplicationOnEdit)
		{
			if (!_IsTemplateRegistryOpen)
			{
				foreach (Application item in ApplicationsLoaded)
				{
					item.Unload(12L);
				}
				TemplateRegistryVC = null;
				if (Win32Generator.EditTemplateRegistry(Versions.GetActiveVersion(forWindows), directory))
				{
					TemplateRegistryVC = new TemplateRegistryControl_ViewContent(Win32Generator.TemplateRegistryInstance);
					WorkbenchSingleton.Workbench.ShowView((IViewContent)(object)TemplateRegistryVC);
					TemplateRegistryVC.OnAllControlsClosedBefore += OnTemplateRegistryVC_OnAllControlsClosedBefore;
					_IsTemplateRegistryOpen = true;
					Win32Generator.TemplateRegistryEditorClosed += OnTemplateRegistryEditorClosed;
				}
				else
				{
					SetText(GeneratorError.RegistryBusy);
					TemplateRegistryVC = null;
				}
			}
			else
			{
				((AbstractBaseViewContent)TemplateRegistryVC).WorkbenchWindow.SelectWindow();
			}
		}
		else
		{
			SetText(GeneratorError.AppIsNotClosed);
		}
	}

	private static void OnTemplateRegistryVC_OnAllControlsClosedBefore(object sender, EventArgs e)
	{
		if (TemplateRegistryVC != null)
		{
			TemplateRegistryVC.OnAllControlsClosedBefore -= OnTemplateRegistryVC_OnAllControlsClosedBefore;
		}
		StatusBarService.ProgressMonitor.ShowNotification("TemplateRegistryClose", "Template Registry Closing...");
	}

	private static void OnTemplateRegistryEditorClosed()
	{
		Win32Generator.TemplateRegistryEditorClosed -= OnTemplateRegistryEditorClosed;
		if (_IsTemplateRegistryOpen && TemplateRegistryVC != null)
		{
			TemplateRegistryVC = null;
			_IsTemplateRegistryOpen = false;
			Gen = Win32Generator.CreateGenerator(ClarionVersion, ClarionAddins.Win32Present);
			Win32Generator.SetRequestor(theGeneratorRequestor);
			StatusBarService.ProgressMonitor.Done("TemplateRegistryClose");
		}
	}

	internal static void EditGeneratorOptions()
	{
		if (!IsGeneatorOptionsOpen)
		{
			CanOpenEditor = false;
			_IsGeneatorOptionsOpen = true;
			try
			{
				Win32Generator.EditGeneratorOptions();
				PropertyService.Save();
			}
			finally
			{
				CanOpenEditor = true;
				_IsGeneatorOptionsOpen = false;
			}
			Win32Generator.FinishEditGeneratorOptions();
			Gen = Win32Generator.CreateGenerator(ClarionVersion, ClarionAddins.Win32Present);
			Win32Generator.SetRequestor(theGeneratorRequestor);
		}
	}

	public static Win32App NewAppFromTxa(string appName, string txaName)
	{
		return Gen.NewApplication(appName, txaName);
	}

	internal static Win32App NewApp(string fileName, string language)
	{
		return Gen.NewApplication(fileName);
	}

	private static void OnGenerationStart(GenerationStartEventArgs e)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<GenerationStartEventArgs>((Action<GenerationStartEventArgs>)OnGenerationStart, e);
		}
		else if (ApplicationService.GenerationStarting != null)
		{
			ApplicationService.GenerationStarting(null, e);
		}
	}

	private static void OnGenerationEnd(GenerationEndEventArgs e)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<GenerationEndEventArgs>((Action<GenerationEndEventArgs>)OnGenerationEnd, e);
		}
		else if (ApplicationService.GenerationEnded != null)
		{
			ApplicationService.GenerationEnded(null, e);
		}
	}

	private static void OnAppGenerationStart(ApplicationGeneratingEventArgs e)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<ApplicationGeneratingEventArgs>((Action<ApplicationGeneratingEventArgs>)OnAppGenerationStart, e);
		}
		else if (ApplicationService.ApplicationGenerationStarting != null)
		{
			ApplicationService.ApplicationGenerationStarting(null, e);
		}
	}

	private static void OnAppBatchGenerationStart(ApplicationGeneratingEventArgs e)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<ApplicationGeneratingEventArgs>((Action<ApplicationGeneratingEventArgs>)OnAppBatchGenerationStart, e);
		}
		else if (ApplicationService.ApplicationBatchGenerationStarting != null)
		{
			ApplicationService.ApplicationBatchGenerationStarting(null, e);
		}
	}

	private static void OnAppBatchCompilationStart(ApplicationGeneratingEventArgs e)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<ApplicationGeneratingEventArgs>((Action<ApplicationGeneratingEventArgs>)OnAppBatchCompilationStart, e);
		}
		else if (ApplicationService.ApplicationBatchCompilationStarting != null)
		{
			ApplicationService.ApplicationBatchCompilationStarting(null, e);
		}
	}

	private static void OnAppBatchBuildingStart(GenerationStartEventArgs e)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<GenerationStartEventArgs>((Action<GenerationStartEventArgs>)OnAppBatchBuildingStart, e);
		}
		else if (ApplicationService.ApplicationBatchBuildingStarting != null)
		{
			ApplicationService.ApplicationBatchBuildingStarting(null, e);
		}
	}

	private static void OnAppBatchBuildingEnd()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(new Action(OnAppBatchBuildingEnd));
		}
		else if (ApplicationService.ApplicationBatchBuildingEnded != null)
		{
			ApplicationService.ApplicationBatchBuildingEnded(null, null);
		}
	}

	private static void OnAppBatchBuildingCancelled()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(new Action(OnAppBatchBuildingCancelled));
		}
		else if (ApplicationService.ApplicationBatchBuildingCancelled != null)
		{
			ApplicationService.ApplicationBatchBuildingCancelled(null, null);
		}
	}

	private static void OnAppGenerationEnd(ApplicationGeneratedEventArgs e)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<ApplicationGeneratedEventArgs>((Action<ApplicationGeneratedEventArgs>)OnAppGenerationEnd, e);
		}
		else if (ApplicationService.ApplicationGenerationEnded != null)
		{
			ApplicationService.ApplicationGenerationEnded(null, e);
		}
	}

	private static void BatchProcessorStart()
	{
		if (Win32Generator.CommandLineLogger != null)
		{
			BatchProcessorWork();
		}
		else
		{
			BatchProcessorThreadStart();
		}
	}

	private static void BatchProcessorThreadStart()
	{
		if (_BatchProcessorThread == null)
		{
			_BatchProcessorThread = new Thread(BatchProcessorWork);
			_BatchProcessorThread.Name = "BatchProcessor";
			_BatchProcessorThread.Priority = ThreadPriority.Highest;
			_BatchProcessorThread.IsBackground = true;
			_BatchProcessorThreadStoped = false;
			SetTextDebug("Before calling BatchProcessorThreadWork to start the app processing");
			_BatchProcessorThread.Start();
		}
	}

	private static void BatchProcessorWork()
	{
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Expected O, but got Unknown
		try
		{
			bool flag = false;
			IProject val = null;
			Application application = null;
			ApplicationGeneratingEventArgs e = null;
			int num = 0;
			batchProcessTotalApps = batchProcessApps.Count;
			while (batchProcessApps.Count > 0)
			{
				WorkbenchSingleton.DoEvents();
				if (_BatchProcessorThreadStoped)
				{
					_BatchProcessorThreadStoped = false;
					break;
				}
				num = batchProcessTotalApps - batchProcessApps.Count + 1;
				batchProcessCurrentApplication = batchProcessApps.Dequeue();
				application = batchProcessCurrentApplication;
				LoggingService.Info((object)$"START BatchProcessing {application.Name}");
				e = new ApplicationGeneratingEventArgs(application, batchProcessPosGenAction);
				batchProcessStartTimeEach = DateTime.Now;
				OnAppBatchGenerationStart(e);
				if (e.Cancel)
				{
					batchProcessCancelled = true;
				}
				else
				{
					if (application.IsOnSolution)
					{
						val = application.GetProjectServiceProject();
						if (val is MissingProject || val is UnknownProject)
						{
							SetText($"The project for the application \"{application.Name}\" does not exist on the the same Directory. Project: {val.FileName}");
							MessageService.ShowMessage($"The project for the application \"{application.Name}\" does not exist on the the same Directory. Project: {val.FileName}", "Error on the Solution");
							val = null;
						}
					}
					else
					{
						val = null;
					}
					SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchEachTitle"), application.Name) + " (" + num + "/" + batchProcessTotalApps + ")");
					flag = true;
					if (batchProcessGenerate)
					{
						AppendText(string.Format(ResourceService.GetString("Clarion.Generator.BatchGenerationStart"), batchProcessStartTimeEach.ToString("t")));
						try
						{
							LoggingService.Info((object)$"Generation started {application.Name}");
							flag = BatchProcessThreadedGenerate(application, val);
							WorkbenchSingleton.DoEvents();
						}
						catch (IOException)
						{
							flag = false;
							try
							{
								application.Unload(22L);
							}
							catch
							{
							}
							DoFinishGenenerateAndCompileApplicationsBatch(batchProcessCancelled);
						}
						catch (Exception ex2)
						{
							flag = false;
							try
							{
								application.Unload(22L);
							}
							catch
							{
							}
							unloadAppAfterMerge = false;
							batchProcessCancelled = true;
							SetText("##########################################");
							SetText("##########################################");
							SetText("An error occurred during code generation. The batch operation can not continue.");
							SetText("The .App that failed to generate is: " + application.Name + ".app");
							SetText("If a .xlog file was generated please send it to support@softvelocity.com.");
							SetText("##########################################");
							SetText("##########################################");
							DoFinishGenenerateAndCompileApplicationsBatch(batchProcessCancelled);
							throw ex2;
						}
						StatusBarService.SetMessage($"{application.Name} Generation finish");
						LoggingService.Info((object)$"Generation finish {application.Name}");
						WorkbenchSingleton.DoEvents();
						if (flag)
						{
							if (val != null && application.IsOnSolution && application.AppPrj != null)
							{
								try
								{
									StatusBarService.SetMessage($"Updating project {application.Name}");
									WorkbenchSingleton.DoEvents();
									BatchProcessThreadedProjectsMerge(application, val);
									WorkbenchSingleton.DoEvents();
								}
								catch (Exception ex3)
								{
									flag = false;
									batchProcessCancelled = true;
									SetText("##########################################");
									SetText("##########################################");
									SetText("An error happen after generating and merging the project. The process can not continue. The app where it fail is." + application.Name);
									SetText("If an xlog file was generated please get in contact with support.");
									SetText("##########################################");
									SetText("##########################################");
									DoFinishGenenerateAndCompileApplicationsBatch(batchProcessCancelled);
									throw ex3;
								}
								application.OldAppPrj = application.AppPrj;
								reparsingDelayed.Add(val);
							}
							if (unloadAppAfterMerge)
							{
								unloadAppAfterMerge = false;
								application.Save();
								application.Unload(25L);
							}
						}
						StatusBarService.ClearMessage();
						OnAppGenerationEnd(new ApplicationGeneratedEventArgs(application, batchProcessPosGenAction, flag));
						DateTime now = DateTime.Now;
						SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchGenerationEnd"), now.ToString("t"), (now - batchProcessStartTimeEach).ToString()));
						LoggingService.Info((object)("SubTotal elapsed time: " + (now - batchProcessStartTime).ToString()));
						SetTextDebug("SubTotal elapsed time: " + (now - batchProcessStartTime).ToString());
						SetText("");
					}
					if (!flag || CancelGeneration.IsGenerationProcessCancelled)
					{
						batchProcessCancelled = true;
						break;
					}
					if (batchProcessTotalApps > 1)
					{
						CleanUpGC();
					}
					if (!batchProcessCancelled && ProjectService.OpenSolution != null && batchProcessPosGenAction != PosGenerationAction.None)
					{
						batchProcessStartTimeEach = DateTime.Now;
						if (batchProcessPosGenActionIsBatch)
						{
							AppendText(string.Format(ResourceService.GetString("Clarion.Generator.BatchBuildStart"), Enum.GetName(typeof(PosGenerationAction), batchProcessPosGenAction), batchProcessStartTimeEach.ToString("t")));
							if (val != null)
							{
								e = new ApplicationGeneratingEventArgs(application, batchProcessPosGenAction);
								OnAppBatchCompilationStart(e);
								if (e.Cancel)
								{
									SetTextDebug("Cancelled at OnAppBatchCompilationStart");
									batchProcessCancelled = true;
								}
								else
								{
									BuildProject val2 = new BuildProject(val);
									val2.AdditionalProperties.Add("NoDependency", "true");
									ProjectService.BuildFinished += OnApplicationBatchBuildingBuildFinished;
									WorkbenchSingleton.DoEvents();
									DoRunCommandInMainThread((AbstractCommand)(object)val2);
									BatchProcessorThreadWaitForCmd();
								}
							}
						}
						else if (batchProcessSingleApp == null && batchProcessTotalApps == 1)
						{
							batchProcessSingleApp = application;
							batchProcessSingleIPrj = val;
						}
					}
				}
				LoggingService.Info((object)$"FINISH BatchProcessing {application.Name}");
			}
		}
		catch (Exception ex4)
		{
			SetTextDebug("BatchProcessorThreadWork Exception " + ex4.ToString());
			throw;
		}
		finally
		{
			DoFinishGenenerateAndCompileApplicationsBatch(batchProcessCancelled);
			_BatchProcessorThread = null;
			SetTextDebug("BatchProcessorThreadWork FINISH");
		}
	}

	private static void DoRunCommandInMainThread(AbstractCommand cmd)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<AbstractCommand>((Action<AbstractCommand>)DoRunCommandInMainThread, cmd);
			return;
		}
		TaskService.InUpdateChanged += OnTaskServiceInUpdateChanged;
		LoggingService.Info((object)"Before cmd Run");
		try
		{
			cmd.Run();
		}
		catch (Exception ex)
		{
			if (cmd is BuildProject)
			{
				MessageService.ShowMessage(ex, "MSBuild has a problem building the project: " + ((AbstractProjectBuildMenuCommand)(BuildProject)cmd).ProjectFileName, "Error Executing the Build Project Command");
			}
			else
			{
				MessageService.ShowMessage(ex, "MSBuild has a problem with the command " + ((object)cmd).GetType().ToString() + "\n\rSolution: " + ((AbstractSolutionFolder)ProjectService.OpenSolution).Name, "Error Executing the Command");
			}
		}
	}

	private static bool BatchProcessThreadedGenerate(Application app, IProject iPrj)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<Application, IProject, bool>((Func<Application, IProject, bool>)BatchProcessThreadedGenerate, app, iPrj);
		}
		unloadAppAfterMerge = app.CheckLazyLoad();
		bool flag = false;
		if (app.IsLoaded)
		{
			flag = app.Generate(iPrj, batchProcessConditionalGeneration, batchProcessDebugTraceGeneration);
			if (!flag)
			{
				app.Save();
				app.Unload(24L);
				unloadAppAfterMerge = false;
			}
		}
		return flag;
	}

	private static void BatchProcessThreadedProjectsMerge(Application app, IProject iPrj)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<Application, IProject>((Action<Application, IProject>)BatchProcessThreadedProjectsMerge, app, iPrj);
			return;
		}
		ProjectsMerger.Merge(app.OldAppPrj, app.AppPrj, iPrj, initial: false, app.Language, saveTargetIprj: true);
		if (unloadAppAfterMerge)
		{
			unloadAppAfterMerge = false;
			app.Save();
			app.Unload(23L);
		}
	}

	private static void BatchProcessorThreadWaitForCmd()
	{
		_BatchProcessorThreadHaveToWaitForCmd = true;
		_BatchProcessorThread.Priority = ThreadPriority.Normal;
		while (_BatchProcessorThreadHaveToWaitForCmd)
		{
			Thread.Sleep(10);
		}
		if (_BatchProcessorThread != null)
		{
			_BatchProcessorThread.Priority = ThreadPriority.Highest;
		}
	}

	private static void BatchProcessorThreadResumeFromCmd()
	{
		_BatchProcessorThreadHaveToWaitForCmd = false;
	}

	private static void BatchProcessorThreadStop()
	{
		_BatchProcessorThreadStoped = true;
	}

	private static string GetPosGenerationActionMessage(bool alsoGenerate, PosGenerationAction posGenAction)
	{
		string result = "";
		switch (posGenAction)
		{
		case PosGenerationAction.None:
			result = "";
			if (alsoGenerate)
			{
				result = "Generate";
			}
			break;
		case PosGenerationAction.Compile:
			result = ((!alsoGenerate) ? "Compile" : "Generate and Compile");
			break;
		case PosGenerationAction.CompileAndRun:
			result = ((!alsoGenerate) ? "Compile and Run" : "Generate, Compile and Run");
			break;
		case PosGenerationAction.CompileAndRunDebug:
			result = ((!alsoGenerate) ? "Compile and Debug" : "Generate, Compile and Debug");
			break;
		case PosGenerationAction.BatchCompile:
			result = ((!alsoGenerate) ? "Compile in Batch" : "Generate and Compile in Batch");
			break;
		case PosGenerationAction.BatchCompileAndRun:
			result = ((!alsoGenerate) ? "Compile and Run in Batch" : "Generate, Compile and Run in Batch");
			break;
		case PosGenerationAction.BatchCompileAndRunDebug:
			result = ((!alsoGenerate) ? "Compile and Deubg in Batch" : "Generate, Compile and Deubg in Batch");
			break;
		}
		return result;
	}

	private static void OnGenerationCancelledCommandRun(object sender, EventArgs e)
	{
		SetTextDebug("Geneneration Cancelled - button clicked");
	}

	private static void GenenerateAndCompileApplicationsBatch(IEnumerable<Application> apps, bool generate, PosGenerationAction posGenAction, GenerationMode conditionalGeneration, GenerationMode debugTraceGeneration)
	{
		if (!CanOpenEditor)
		{
			SetTextDebug("GenenerateAndCompileApplicationsBatch could't start CanOpenEditor = FALSE");
			return;
		}
		if (_BatchProcessorThread != null)
		{
			SetTextDebug("The Batch Processing thread still running. The process didn't finish yet and can not be re started.");
			return;
		}
		SetTextDebug("GenenerateAndCompileApplicationsBatch START");
		SetTextDebug("generate: " + generate);
		SetTextDebug("posGenAction: " + posGenAction);
		SetTextDebug("conditionalGeneration: " + conditionalGeneration);
		SetTextDebug("debugTraceGeneration: " + debugTraceGeneration);
		WaitForParser();
		reparsingDelayed.Clear();
		ParserService.StopParserThread();
		CanOpenEditor = false;
		if (Win32Generator.CommandLineLogger == null && ThereIsAMainWindow)
		{
			GeneratorCategory.ClearText();
		}
		TaskService.Clear();
		foreach (Application application in Applications)
		{
			application.ClearErrorList();
		}
		AllowClearErrors(value: false);
		int num = 0;
		foreach (Application app in apps)
		{
			_ = app;
			num++;
		}
		if (num > 1)
		{
			ApplicationListCurrentSort = ApplicationsSort.Unknown;
			ApplicationListCurrentSort = ApplicationsSort.ByDependency;
		}
		_Generating = true;
		batchProcessPosGenAction = posGenAction;
		batchProcessPosGenActionIsBatch = batchProcessPosGenAction == PosGenerationAction.BatchCompile || batchProcessPosGenAction == PosGenerationAction.BatchCompileAndRun || batchProcessPosGenAction == PosGenerationAction.BatchCompileAndRunDebug;
		batchProcessTotalApps = num;
		batchProcessSingleApp = null;
		batchProcessSingleIPrj = null;
		batchProcessGenerate = generate;
		batchProcessConditionalGeneration = conditionalGeneration;
		batchProcessDebugTraceGeneration = debugTraceGeneration;
		batchProcessStartTime = DateTime.Now;
		DateTime now = DateTime.Now;
		SetText(ResourceService.GetString("Clarion.Generator.BatchSeparator"));
		if (posGenAction == PosGenerationAction.None)
		{
			SetText(string.Format(ResourceService.GetString("Clarion.Generator.GenerationStart"), now));
		}
		else if (generate)
		{
			_ = "Processing: " + GetPosGenerationActionMessage(generate, posGenAction);
			SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchTitleGenerate"), "Generation", Enum.GetName(typeof(PosGenerationAction), posGenAction), now));
		}
		else
		{
			_ = "Processing: " + GetPosGenerationActionMessage(generate, posGenAction);
			SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchTitle"), Enum.GetName(typeof(PosGenerationAction), posGenAction), now));
		}
		SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchSubTitle"), num));
		if (string.IsNullOrEmpty(Versions.GetActiveVersion(true)))
		{
			AppendText(Versions.CurrentVersionName(true));
		}
		else
		{
			AppendText(Versions.GetActiveVersion(true));
		}
		AppendText(" - ");
		if (string.IsNullOrEmpty(Versions.GetActiveVersion(false)))
		{
			SetText(Versions.CurrentVersionName(false));
		}
		else
		{
			SetText(Versions.GetActiveVersion(false));
		}
		SetText(ResourceService.GetString("Clarion.Generator.BatchSeparator"));
		GenerationStartEventArgs e = new GenerationStartEventArgs(apps, posGenAction);
		OnGenerationStart(e);
		if (e.Cancel)
		{
			SetTextDebug("Cancelled at OnGenerationStart");
			batchProcessCancelled = true;
			_Generating = false;
			OnGenerationEnd(new GenerationEndEventArgs(apps, posGenAction, noErrors: false));
			ProcessingEnded();
			return;
		}
		OnAppBatchBuildingStart(e);
		if (e.Cancel)
		{
			SetTextDebug("Cancelled at OnAppBatchBuildingStart");
			batchProcessCancelled = true;
			_Generating = false;
			OnGenerationEnd(new GenerationEndEventArgs(apps, posGenAction, noErrors: false));
			ProcessingEnded();
			return;
		}
		if (batchProcessApps != null)
		{
			batchProcessApps.Clear();
		}
		batchProcessApps = new Queue<Application>(num);
		batchProcessAppsOk = new List<Application>();
		batchProcessAppsError = new List<Application>();
		if (num > 0)
		{
			if (num > 1)
			{
				foreach (Application application2 in Applications)
				{
					foreach (Application app2 in apps)
					{
						if (app2 == application2)
						{
							batchProcessApps.Enqueue(application2);
							break;
						}
					}
				}
			}
			else
			{
				using IEnumerator<Application> enumerator5 = apps.GetEnumerator();
				if (enumerator5.MoveNext())
				{
					Application current4 = enumerator5.Current;
					batchProcessApps.Enqueue(current4);
				}
			}
		}
		batchProcessCancelled = false;
		BatchProcessorStart();
	}

	private static void PreserveTasks()
	{
		if (preservedTasksList != null)
		{
			preservedTasksList.Clear();
		}
		preservedTasksList = new List<Task>(TaskService.Tasks);
	}

	private static void RestoreTasks()
	{
		foreach (Task preservedTasks in preservedTasksList)
		{
			TaskService.Add(preservedTasks);
		}
		preservedTasksList.Clear();
		preservedTasksList = null;
	}

	private static void OnTaskServiceInUpdateChanged(object sender, EventArgs e)
	{
		DoTaskServicePreservation();
	}

	private static void DoTaskServicePreservation()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(new Action(DoTaskServicePreservation));
			return;
		}
		if (TaskService.InUpdate)
		{
			PreserveTasks();
			return;
		}
		TaskService.Clear();
		RestoreTasks();
		TaskService.InUpdateChanged -= OnTaskServiceInUpdateChanged;
	}

	private static void OnApplicationBatchBuildingBuildFinished(object sender, EventArgs e)
	{
		ProjectService.BuildFinished -= OnApplicationBatchBuildingBuildFinished;
		DateTime now = DateTime.Now;
		if (TaskService.GetCount((TaskType)0) > 0)
		{
			batchProcessAppsError.Add(batchProcessCurrentApplication);
		}
		else
		{
			batchProcessAppsOk.Add(batchProcessCurrentApplication);
		}
		SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchBuildEnd"), now.ToString("t"), (now - batchProcessStartTime).ToString()));
		SetText("");
		batchProcessCurrentApplication = null;
		BatchProcessorThreadResumeFromCmd();
	}

	private static void DoFinishGenenerateAndCompileApplicationsBatch(bool cancelled)
	{
		batchProcessCancelled = cancelled;
		DateTime now = DateTime.Now;
		if (cancelled)
		{
			SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchCancelled"), now));
			OnAppBatchBuildingCancelled();
		}
		else if (!batchProcessPosGenActionIsBatch && batchProcessPosGenAction != PosGenerationAction.None)
		{
			ExecutePosAction();
		}
		DoFinishGenenerateAndCompileApplicationsBatchEnd();
	}

	private static void DoFinishGenenerateAndCompileApplicationsBatchEnd()
	{
		if (!batchProcessCancelled)
		{
			DateTime now = DateTime.Now;
			SetText(ResourceService.GetString("Clarion.Generator.BatchSeparator"));
			SetText(ResourceService.GetString("Clarion.Generator.BatchSummaryTitle"));
			if (batchProcessPosGenAction == PosGenerationAction.None)
			{
				SetText(string.Format(ResourceService.GetString("Clarion.Generator.GenerationFinish"), now, (now - batchProcessStartTime).ToString()));
			}
			else if (batchProcessGenerate)
			{
				SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchSummarySubTitleGenerate"), Enum.GetName(typeof(PosGenerationAction), batchProcessPosGenAction), now, (now - batchProcessStartTime).ToString()));
			}
			else
			{
				SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchSummarySubTitle"), Enum.GetName(typeof(PosGenerationAction), batchProcessPosGenAction), now, (now - batchProcessStartTime).ToString()));
			}
			if (batchProcessPosGenActionIsBatch)
			{
				SetText("");
				SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchSummaryWithoutErrors"), batchProcessAppsOk.Count));
				foreach (Application item in batchProcessAppsOk)
				{
					SetText(item.Name);
				}
				if (batchProcessAppsOk.Count == 0)
				{
					SetText(ResourceService.GetString("Clarion.Generator.BatchSummaryNone"));
				}
				SetText("");
				SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchSummaryWithErrors"), batchProcessAppsError.Count));
				foreach (Application item2 in batchProcessAppsError)
				{
					SetText(item2.Name);
				}
				if (batchProcessAppsError.Count == 0)
				{
					SetText(ResourceService.GetString("Clarion.Generator.BatchSummaryNone"));
				}
			}
			else
			{
				SetText("");
				if (batchProcessAppsError.Count > 0)
				{
					SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchSummaryWithErrors"), "--"));
				}
				else
				{
					SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchSummaryWithoutErrors"), batchProcessTotalApps));
				}
			}
			OnAppBatchBuildingEnd();
		}
		OnGenerationEnd(new GenerationEndEventArgs(null, batchProcessPosGenAction, !batchProcessCancelled));
		ProcessingEnded();
	}

	private static void ExecutePosAction()
	{
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected O, but got Unknown
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Expected O, but got Unknown
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Expected O, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected O, but got Unknown
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected O, but got Unknown
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Expected O, but got Unknown
		SetTextDebug("ExecutePosAction preparing the command");
		AbstractCommand val = null;
		_Generating = false;
		if (batchProcessCancelled || batchProcessPosGenAction == PosGenerationAction.None || batchProcessPosGenActionIsBatch || ProjectService.OpenSolution == null)
		{
			return;
		}
		val = null;
		SetText("");
		SetText(ResourceService.GetString("Clarion.Generator.BatchSeparator"));
		SetText(string.Format(ResourceService.GetString("Clarion.Generator.GenerationPostAction"), Enum.GetName(typeof(PosGenerationAction), batchProcessPosGenAction)));
		if (batchProcessTotalApps == 1)
		{
			if (batchProcessSingleApp != null)
			{
				SetText(string.Format(ResourceService.GetString("Clarion.Generator.GenerationPostAppTitle"), batchProcessSingleApp.Name));
			}
			if (batchProcessSingleIPrj != null)
			{
				SetText(string.Format(ResourceService.GetString("Clarion.Generator.GenerationPostPrjTitle"), ((ISolutionFolder)batchProcessSingleIPrj).Name));
			}
		}
		SetText(ResourceService.GetString("Clarion.Generator.BatchSeparator"));
		batchProcessStartTimeEach = DateTime.Now;
		if (batchProcessTotalApps > 1 || (batchProcessTotalApps == 1 && batchProcessSingleIPrj != null))
		{
			AppendText(string.Format(ResourceService.GetString("Clarion.Generator.BatchBuildStart"), Enum.GetName(typeof(PosGenerationAction), batchProcessPosGenAction), batchProcessStartTimeEach.ToString("t")));
		}
		SetTextDebug("batchProcessPosGenAction: " + batchProcessPosGenAction);
		switch (batchProcessPosGenAction)
		{
		case PosGenerationAction.Compile:
			if (batchProcessTotalApps == 1)
			{
				if (batchProcessSingleIPrj != null)
				{
					BuildProject val2 = new BuildProject(batchProcessSingleIPrj);
					val2.AdditionalProperties.Add("NoDependency", "true");
					val = (AbstractCommand)(object)val2;
				}
			}
			else
			{
				val = (AbstractCommand)new Build();
			}
			break;
		case PosGenerationAction.CompileAndRun:
			if (batchProcessTotalApps == 1)
			{
				if (batchProcessSingleIPrj == null)
				{
					break;
				}
				BuildProject cmdBP2 = new BuildProject(batchProcessSingleIPrj);
				cmdBP2.AdditionalProperties.Add("NoDependency", "true");
				val = (AbstractCommand)(object)cmdBP2;
				((AbstractBuildMenuCommand)cmdBP2).BuildComplete += delegate
				{
					if (((AbstractBuildMenuCommand)cmdBP2).LastBuildResults.ErrorCount == 0)
					{
						AbstractRunProjectMenuCommand.RunCurrentProject(batchProcessSingleIPrj, useDebug: false, fallbackToStartUp: true);
					}
				};
			}
			else
			{
				val = (AbstractCommand)new ExecuteStartupProjectWithoutDebugger();
			}
			break;
		case PosGenerationAction.CompileAndRunDebug:
			if (batchProcessTotalApps == 1)
			{
				if (batchProcessSingleIPrj == null)
				{
					break;
				}
				BuildProject cmdBP = new BuildProject(batchProcessSingleIPrj);
				cmdBP.AdditionalProperties.Add("NoDependency", "true");
				val = (AbstractCommand)(object)cmdBP;
				((AbstractBuildMenuCommand)cmdBP).BuildComplete += delegate
				{
					if (((AbstractBuildMenuCommand)cmdBP).LastBuildResults.ErrorCount == 0)
					{
						AbstractRunProjectMenuCommand.RunCurrentProject(batchProcessSingleIPrj, useDebug: true, fallbackToStartUp: true);
					}
				};
			}
			else
			{
				val = (AbstractCommand)new ExecuteStartupProject();
			}
			break;
		}
		if (val != null)
		{
			SetTextDebug("Build Command Type: " + ((object)val).GetType().ToString());
			ProjectService.BuildFinished += OnExecutePosActionFinished;
			if (batchProcessTotalApps == 1)
			{
				if (batchProcessSingleApp != null)
				{
					SetTextDebug("Building " + batchProcessSingleIPrj.FileName);
				}
			}
			else
			{
				SetTextDebug("Building - Solution " + ((AbstractSolutionFolder)ProjectService.OpenSolution).Name);
			}
			SetTextDebug("Before calling post action command");
			DoRunCommandInMainThread(val);
			BatchProcessorThreadWaitForCmd();
		}
		else
		{
			string text = null;
			text = ((batchProcessSingleApp == null) ? "The post action couldn't be executed. The project was not found." : $"The post action couldn't be executed. The project for the application {batchProcessSingleApp.Name} was not found in the solution.");
			SetText(text);
			MessageService.ShowError(text);
		}
	}

	private static void OnExecutePosActionFinished(object sender, EventArgs e)
	{
		SetTextDebug("OnExecutePosActionFinished");
		ProjectService.BuildFinished -= OnExecutePosActionFinished;
		if (TaskService.GetCount((TaskType)0) > 0)
		{
			batchProcessAppsError.Add(batchProcessCurrentApplication);
		}
		else
		{
			batchProcessAppsOk.Add(batchProcessCurrentApplication);
		}
		DateTime now = DateTime.Now;
		SetText(string.Format(ResourceService.GetString("Clarion.Generator.BatchBuildEnd"), now.ToString("t"), (now - batchProcessStartTime).ToString()));
		SetText("");
		batchProcessCurrentApplication = null;
		SetText("");
		BatchProcessorThreadResumeFromCmd();
	}

	private static void ProcessingEnded()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(new Action(ProcessingEnded));
			return;
		}
		SetTextDebug("ProcessingEnded");
		if (batchProcessAppsError != null)
		{
			batchProcessAppsError.Clear();
			batchProcessAppsError = null;
		}
		if (batchProcessAppsOk != null)
		{
			batchProcessAppsOk.Clear();
			batchProcessAppsOk = null;
		}
		batchProcessSingleApp = null;
		batchProcessSingleIPrj = null;
		batchProcessTotalApps = 0;
		if (batchProcessApps != null)
		{
			batchProcessApps.Clear();
			batchProcessApps = new Queue<Application>();
		}
		if (WorkbenchSingleton.MainForm != null)
		{
			ParserService.StartParserThread();
		}
		if (reparsingDelayed.Count > 0 && WorkbenchSingleton.MainForm != null)
		{
			StatusBarService.SetMessage("Refreshing Solution Explorer.");
			RefreshSolutionPad();
			StatusBarService.ClearMessage();
			StatusBarService.SetMessage("Reparsing Project Files");
			bool flag = true;
			foreach (IProject item in reparsingDelayed)
			{
				flag = true;
				if (item is CommonClarionProject)
				{
					flag = ((CommonClarionProject)(object)item).ProjectParsingEnabled;
				}
				if (flag)
				{
					ParserService.Reparse(item, true, true);
				}
			}
			WaitForParser();
			StatusBarService.ClearMessage();
		}
		reparsingDelayed.Clear();
		_Generating = false;
		CancelGeneration.ResetGenerationProcessCancelled();
		CanOpenEditor = true;
		Instance.TreeModelDataChanged();
		CleanUpGC();
		AllowClearErrors(value: true);
		SetTextDebug("ProcessingEnded FINISHED");
	}

	private static void CleanUpGC()
	{
		StatusBarService.SetMessage("Cleaning...");
		GC.Collect();
		GC.WaitForPendingFinalizers();
		StatusBarService.ClearMessage();
	}

	private static void GenenerateAndCompileApplications(IEnumerable<Application> apps, PosGenerationAction posGenAction)
	{
		GenenerateAndCompileApplications(apps, generate: true, posGenAction, GenerationMode.GlobalOption, GenerationMode.GlobalOption);
	}

	private static void GenenerateAndCompileApplications(IEnumerable<Application> papps, bool generate, PosGenerationAction posGenAction, GenerationMode conditionalGeneration, GenerationMode debugTraceGeneration)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Invalid comparison between Unknown and I4
		CancelGeneration.ResetGenerationProcessCancelled();
		RefreshVerbosity();
		SetTextDebug("GenenerateAndCompileApplications START");
		SetTextDebug("debugTraceGeneration: " + debugTraceGeneration);
		WaitForParser();
		reparsingDelayed.Clear();
		if (!CanOpenEditor)
		{
			SetTextDebug("GenenerateAndCompileApplications could't start CanOpenEditor = FALSE");
			return;
		}
		if (generate)
		{
			foreach (Application papp in papps)
			{
				if (papp.IsBusy)
				{
					return;
				}
				if (papp.InEdit && !papp.CanGenerate)
				{
					MessageService.ShowMessage(string.Format(ResourceService.GetString("Clarion.Generator.Error.ReturnToApptree"), papp.Name), ResourceService.GetString("Clarion.Generator.Error.ReturnToApptree.Title"));
					return;
				}
			}
		}
		if ((int)VersionService.Version != 1)
		{
			switch (posGenAction)
			{
			case PosGenerationAction.BatchCompile:
				posGenAction = PosGenerationAction.Compile;
				break;
			case PosGenerationAction.BatchCompileAndRun:
				posGenAction = PosGenerationAction.CompileAndRun;
				break;
			case PosGenerationAction.BatchCompileAndRunDebug:
				posGenAction = PosGenerationAction.CompileAndRunDebug;
				break;
			}
		}
		GenenerateAndCompileApplicationsBatch(papps, generate, posGenAction, conditionalGeneration, debugTraceGeneration);
	}

	private static void GenAndCompileBuildFinished(object sender, EventArgs e)
	{
		ProjectService.BuildFinished -= GenAndCompileBuildFinished;
		ProcessingEnded();
	}

	internal static void GenerateApplication(Application app)
	{
		GenerateApplication(app, GenerationMode.GlobalOption, GenerationMode.GlobalOption);
	}

	internal static void GenerateApplication(Application app, GenerationMode conditionalGeneration, GenerationMode debugTraceGeneration)
	{
		if (app != null)
		{
			List<Application> list = new List<Application>();
			list.Add(app);
			GenerateApplication(list, conditionalGeneration, debugTraceGeneration);
		}
	}

	internal static void GenerateApplication(IEnumerable<Application> apps, GenerationMode conditionalGeneration, GenerationMode debugTraceGeneration)
	{
		GenenerateAndCompileApplications(apps, generate: true, PosGenerationAction.None, conditionalGeneration, debugTraceGeneration);
	}

	internal static void GenMakeApplications(GenMakeSelection appsSelection, PosGenerationAction posAction, GenerationMode conditionalGeneration, GenerationMode debugTraceGeneration)
	{
		GenMakeApplications(generate: true, appsSelection, posAction, conditionalGeneration, debugTraceGeneration);
	}

	internal static void GenMakeApplications(bool generate, GenMakeSelection appsSelection, PosGenerationAction posAction, GenerationMode conditionalGeneration, GenerationMode debugTraceGeneration)
	{
		List<Application> list = null;
		IEnumerable<Application> enumerable = null;
		switch (appsSelection)
		{
		case GenMakeSelection.All:
			enumerable = Applications;
			break;
		case GenMakeSelection.Selected:
			enumerable = ApplicationBrowserPad.Instance.SelectedApplications;
			break;
		case GenMakeSelection.Edited:
			enumerable = ApplicationsLoaded;
			break;
		case GenMakeSelection.Current:
		{
			Application application = null;
			if (application != null)
			{
				list = new List<Application>();
				list.Add(application);
				enumerable = list;
			}
			break;
		}
		}
		if (enumerable != null)
		{
			GenenerateAndCompileApplications(enumerable, generate, posAction, conditionalGeneration, debugTraceGeneration);
		}
	}

	internal static void GenerateSelectedApplications(GenerationMode conditionalGeneration, GenerationMode debugTraceGeneration)
	{
		List<Application> selectedApplications = ApplicationBrowserPad.Instance.SelectedApplications;
		GenenerateAndCompileApplications(selectedApplications, generate: true, PosGenerationAction.None, conditionalGeneration, debugTraceGeneration);
	}

	internal static void GenerateSelectedApplications()
	{
		GenerateSelectedApplications(GenerationMode.GlobalOption, GenerationMode.GlobalOption);
	}

	internal static void GenerateAllApplications()
	{
		GenenerateAndCompileApplications(ApplicationsList, PosGenerationAction.None);
	}

	internal static void GenerateAllEditedApplications()
	{
		GenenerateAndCompileApplications(ApplicationsLoaded, PosGenerationAction.None);
	}

	internal static void GenerateAndMakeApplication(Application app)
	{
		if (app != null)
		{
			List<Application> list = new List<Application>();
			list.Add(app);
			GenenerateAndCompileApplications(list, PosGenerationAction.Compile);
		}
	}

	internal static void MakeAndRunApplication(Application app)
	{
		if (app != null)
		{
			List<Application> list = new List<Application>();
			list.Add(app);
			GenenerateAndCompileApplications(list, PosGenerationAction.CompileAndRun);
		}
	}

	internal static void MakeSelectedApplications()
	{
		List<Application> selectedApplications = ApplicationBrowserPad.Instance.SelectedApplications;
		GenenerateAndCompileApplications(selectedApplications, PosGenerationAction.Compile);
	}

	internal static void MakeAndRunAllApplications()
	{
		GenenerateAndCompileApplications(Applications, PosGenerationAction.CompileAndRun);
	}

	internal static void MakeAndRunEditedApplications()
	{
		GenenerateAndCompileApplications(ApplicationsLoaded, PosGenerationAction.CompileAndRun);
	}

	internal static void MakeAndRunAllApplicationsDebugger()
	{
		GenenerateAndCompileApplications(Applications, PosGenerationAction.CompileAndRunDebug);
	}

	internal static void MakeAndRunEditedApplicationsDebugger()
	{
		GenenerateAndCompileApplications(ApplicationsLoaded, PosGenerationAction.CompileAndRunDebug);
	}
}

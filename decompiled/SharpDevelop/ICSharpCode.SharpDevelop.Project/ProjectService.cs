using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project.Commands;

namespace ICSharpCode.SharpDevelop.Project;

public static class ProjectService
{
	private static Solution openSolution;

	private static IProject currentProject;

	private static string invalidOpenBindings = "SoftVelocity.Generator.UI.TemplateRegistryControl_ViewContent, SoftVelocity.GeneratorX.Binding.TemplateRegistryViewContent, SoftVelocity.DataBrowser.BrowserView";

	private static bool initialized;

	private static bool building;

	private static bool isLoading = false;

	private static Semaphore sem;

	private static string semName;

	public static Solution OpenSolution
	{
		[DebuggerStepThrough]
		get
		{
			return openSolution;
		}
	}

	public static IProject CurrentProject
	{
		[DebuggerStepThrough]
		get
		{
			return currentProject;
		}
		set
		{
			if (currentProject != value)
			{
				LoggingService.Info("CurrentProject changed to " + ((value == null) ? "null" : value.Name));
				currentProject = value;
				OnCurrentProjectChanged(new ProjectEventArgs(currentProject));
			}
		}
	}

	public static bool IsBuilding => building;

	public static bool IsLoading => isLoading;

	public static Semaphore CancelSemaphore
	{
		get
		{
			Init();
			return sem;
		}
	}

	public static string SemaphoreName
	{
		get
		{
			Init();
			return semName;
		}
	}

	public static event ProjectEventHandler ProjectAdded;

	public static event SolutionFolderEventHandler SolutionFolderRemoved;

	public static event EventHandler StartBuild;

	public static event EventHandler EndBuild;

	public static event EventHandler BuildFinished;

	public static event ProjectConfigurationEventHandler ProjectConfigurationChanged;

	public static event SolutionConfigurationEventHandler SolutionConfigurationChanged;

	public static event EventHandler<SolutionEventArgs> SolutionLoadedFirstChanceCall;

	public static event EventHandler<SolutionEventArgs> SolutionLoaded;

	public static event EventHandler<SolutionEventArgs> SolutionSaved;

	public static event EventHandler<SolutionCancelEventArgs> SolutionClosing;

	public static event EventHandler SolutionClosed;

	public static event EventHandler<SolutionEventArgs> SolutionPreferencesSaving;

	public static event ProjectEventHandler CurrentProjectChanged;

	public static event EventHandler<ProjectItemEventArgs> ProjectItemAdded;

	public static event EventHandler<ProjectItemEventArgs> ProjectItemRemoved;

	public static IProject GetProjectWithOutTypeHint(string someFileName)
	{
		if (openSolution == null)
		{
			return null;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(someFileName);
		foreach (IProject project in openSolution.Projects)
		{
			string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(project.FileName);
			if (string.Equals(fileNameWithoutExtension, fileNameWithoutExtension2, StringComparison.OrdinalIgnoreCase))
			{
				return project;
			}
		}
		return null;
	}

	public static IProject GetProject(string projectFilename)
	{
		if (openSolution == null)
		{
			return null;
		}
		foreach (IProject project in openSolution.Projects)
		{
			if (FileUtility.IsEqualFileName(project.FileName, projectFilename))
			{
				return project;
			}
		}
		return null;
	}

	public static IProject GetProjectForFile(string fileName)
	{
		if (openSolution == null || string.IsNullOrEmpty(fileName))
		{
			return null;
		}
		foreach (IProject project in openSolution.Projects)
		{
			if (project.IsFileInProject(fileName))
			{
				return project;
			}
		}
		return null;
	}

	private static string[] FreeOpenFiles()
	{
		List<string> list = new List<string>();
		bool flag = false;
		OpenSolution.GetNoneProjectSolutionFolders();
		if (WorkbenchSingleton.MainForm != null)
		{
			IViewContent[] array = WorkbenchSingleton.Workbench.ViewContentCollection.ToArray();
			foreach (IViewContent viewContent in array)
			{
				if (viewContent.IsUntitled || viewContent.FileName == null || viewContent.FileName.ToUpper().Contains("TEMPLATEREGISTRY") || invalidOpenBindings.Contains(viewContent.GetType().ToString()))
				{
					continue;
				}
				string fileName = viewContent.FileName;
				flag = false;
				foreach (IProject project in openSolution.Projects)
				{
					if (FileUtility.IsEqualFileName(project.FileName, fileName))
					{
						flag = true;
						break;
					}
					if (project.IsFileInProject(fileName))
					{
						flag = true;
						break;
					}
				}
				if (!flag && File.Exists(fileName))
				{
					list.Add(fileName);
				}
			}
		}
		return list.ToArray();
	}

	public static void InitializeService()
	{
		if (!initialized)
		{
			initialized = true;
			Environment.SetEnvironmentVariable("MSBUILDENABLEALLPROPERTYFUNCTIONS", "1");
			WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += ActiveWindowChanged;
			FileService.FileRenamed += FileServiceFileRenamed;
			FileService.FileRemoved += FileServiceFileRemoved;
		}
	}

	public static bool HasProjectLoader(string fileName)
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/Workbench/Combine/FileFilter");
		foreach (Codon codon in treeNode.Codons)
		{
			string pattern = codon.Properties.Get("extensions", "");
			if (FileUtility.MatchesPattern(fileName, pattern) && codon.Properties.Contains("class"))
			{
				return true;
			}
		}
		return false;
	}

	public static IProjectLoader GetProjectLoader(string fileName)
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/Workbench/Combine/FileFilter");
		foreach (Codon codon in treeNode.Codons)
		{
			string pattern = codon.Properties.Get("extensions", "");
			if (FileUtility.MatchesPattern(fileName, pattern) && codon.Properties.Contains("class"))
			{
				object obj = codon.AddIn.CreateObject(codon.Properties["class"]);
				return obj as IProjectLoader;
			}
		}
		return null;
	}

	public static string GetProjectFileCategory(string fileName)
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/Workbench/Combine/FileFilter");
		foreach (Codon codon in treeNode.Codons)
		{
			string pattern = codon.Properties.Get("extensions", "");
			if (FileUtility.MatchesPattern(fileName, pattern) && codon.Properties.Contains("category"))
			{
				return codon.Properties.Get("category", RecentOpen.defaultTypeProjects);
			}
		}
		return RecentOpen.defaultTypeProjects;
	}

	public static void LoadSolutionOrProject(string fileName)
	{
		IProjectLoader projectLoader = GetProjectLoader(fileName);
		if (projectLoader != null)
		{
			projectLoader.Load(fileName);
			return;
		}
		MessageService.ShowError(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.OpenCombine.InvalidProjectOrCombine}", new string[1, 2] { { "FileName", fileName } }));
	}

	private static void FileServiceFileRenamed(object sender, FileRenameEventArgs e)
	{
		if (OpenSolution == null)
		{
			return;
		}
		string sourceFile = e.SourceFile;
		string targetFile = e.TargetFile;
		long num = 0L;
		foreach (ISolutionFolderContainer solutionFolderContainer in OpenSolution.SolutionFolderContainers)
		{
			foreach (SolutionItem item in solutionFolderContainer.SolutionItems.Items)
			{
				string text = Path.Combine(OpenSolution.Directory, item.Name);
				num++;
				if (FileUtility.IsBaseDirectory(sourceFile, text))
				{
					string absPath = FileUtility.RenameBaseDirectory(text, sourceFile, targetFile);
					string name = (item.Location = FileUtility.GetRelativePath(OpenSolution.Directory, absPath));
					item.Name = name;
				}
			}
		}
		long num2 = 0L;
		foreach (IProject project in OpenSolution.Projects)
		{
			if (!FileUtility.IsBaseDirectory(project.Directory, sourceFile))
			{
				continue;
			}
			foreach (ProjectItem item2 in project.Items)
			{
				num2++;
				if (FileUtility.IsBaseDirectory(sourceFile, item2.FileName))
				{
					OnProjectItemRemoved(new ProjectItemEventArgs(project, item2));
					item2.FileName = FileUtility.RenameBaseDirectory(item2.FileName, sourceFile, targetFile);
					OnProjectItemAdded(new ProjectItemEventArgs(project, item2));
				}
			}
		}
	}

	private static void FileServiceFileRemoved(object sender, FileEventArgs e)
	{
		if (OpenSolution == null)
		{
			return;
		}
		string fileName = e.FileName;
		foreach (ISolutionFolderContainer solutionFolderContainer in OpenSolution.SolutionFolderContainers)
		{
			int num = 0;
			while (num < solutionFolderContainer.SolutionItems.Items.Count)
			{
				SolutionItem solutionItem = solutionFolderContainer.SolutionItems.Items[num];
				if (FileUtility.IsBaseDirectory(fileName, Path.Combine(OpenSolution.Directory, solutionItem.Name)))
				{
					solutionFolderContainer.SolutionItems.Items.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
		foreach (IProject project in OpenSolution.Projects)
		{
			if (!FileUtility.IsBaseDirectory(project.Directory, fileName) || !(project is IProjectItemListProvider projectItemListProvider))
			{
				continue;
			}
			ProjectItem[] array = Linq.ToArray(projectItemListProvider.Items);
			foreach (ProjectItem projectItem in array)
			{
				if (FileUtility.IsBaseDirectory(fileName, projectItem.FileName))
				{
					projectItemListProvider.RemoveProjectItem(projectItem);
					OnProjectItemRemoved(new ProjectItemEventArgs(project, projectItem));
				}
			}
		}
	}

	private static void ActiveWindowChanged(object sender, EventArgs e)
	{
		object activeContent = WorkbenchSingleton.Workbench.ActiveContent;
		IViewContent viewContent = activeContent as IViewContent;
		if (viewContent == null && activeContent is ISecondaryViewContent)
		{
			IWorkbenchWindow workbenchWindow = ((ISecondaryViewContent)activeContent).WorkbenchWindow;
			if (workbenchWindow == null)
			{
				return;
			}
			viewContent = workbenchWindow.ViewContent;
		}
		if (OpenSolution != null && viewContent != null)
		{
			string fileName = viewContent.FileName;
			if (fileName != null)
			{
				CurrentProject = OpenSolution.FindProjectContainingFile(fileName) ?? CurrentProject;
			}
		}
	}

	public static bool AddProject(ISolutionFolderNode solutionFolderNode, IProject newProject)
	{
		if (Linq.Exists(solutionFolderNode.Solution.SolutionFolders, (ISolutionFolder folder) => string.Equals(folder.IdGuid, newProject.IdGuid, StringComparison.OrdinalIgnoreCase)))
		{
			LoggingService.Warn("ProjectService.AddProject: Duplicate IdGuid detected");
			IProject project = GetProject(newProject.FileName);
			if (project == newProject)
			{
				return false;
			}
			newProject.IdGuid = "{" + Guid.NewGuid().ToString().ToUpperInvariant() + "}";
		}
		solutionFolderNode.Container.AddFolder(newProject);
		ParserService.CreateProjectContentForAddedProject(newProject);
		solutionFolderNode.Solution.FixSolutionConfiguration(new IProject[1] { newProject });
		OnProjectAdded(new ProjectEventArgs(newProject));
		return true;
	}

	public static void AddProjectItem(IProject project, ProjectItem item)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (project is IProjectItemListProvider projectItemListProvider)
		{
			projectItemListProvider.AddProjectItem(item);
			OnProjectItemAdded(new ProjectItemEventArgs(project, item));
		}
	}

	public static void RemoveProjectItem(IProject project, ProjectItem item)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (project is IProjectItemListProvider projectItemListProvider && projectItemListProvider.RemoveProjectItem(item))
		{
			OnProjectItemRemoved(new ProjectItemEventArgs(project, item));
		}
	}

	private static bool BeforeLoadSolution()
	{
		if (openSolution != null)
		{
			SaveSolutionPreferences();
			if (WorkbenchSingleton.Workbench == null || WorkbenchSingleton.Workbench.CloseAllSolutionViews())
			{
				return CloseSolution();
			}
			return false;
		}
		return true;
	}

	public static void LoadSolution(string fileName)
	{
		if (!BeforeLoadSolution())
		{
			return;
		}
		try
		{
			isLoading = true;
			openSolution = Solution.Load(fileName);
			if (openSolution == null)
			{
				isLoading = false;
				return;
			}
		}
		catch (UnauthorizedAccessException ex)
		{
			isLoading = false;
			MessageService.ShowError(ex.Message);
			return;
		}
		AbstractProject.filesToOpenAfterSolutionLoad.Clear();
		try
		{
			string preferenceFileName = GetPreferenceFileName(openSolution.FileName, PropertyService.Get("SharpDevelop.PreferenceInSolutionFolder", defaultValue: false));
			if (FileUtility.IsValidFileName(preferenceFileName) && File.Exists(preferenceFileName))
			{
				((IMementoCapable)openSolution.Preferences).SetMemento(Properties.Load(preferenceFileName));
			}
			else
			{
				((IMementoCapable)openSolution.Preferences).SetMemento(new Properties());
			}
		}
		catch (XmlException)
		{
			isLoading = false;
			((IMementoCapable)openSolution.Preferences).SetMemento(new Properties());
		}
		catch (Exception ex3)
		{
			isLoading = false;
			MessageService.ShowError(ex3);
		}
		try
		{
			ApplyConfigurationAndReadPreferences();
		}
		catch (Exception ex4)
		{
			isLoading = false;
			MessageService.ShowError(ex4);
		}
		if (PropertyService.Get("SharpDevelop.UseLastSolutionFolderForDefault", defaultValue: true))
		{
			PropertyService.Set("ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.DefaultPath", Path.Combine(Path.GetPathRoot(fileName), Path.GetDirectoryName(fileName)));
		}
		OnSolutionLoaded(new SolutionEventArgs(openSolution));
		if (WorkbenchSingleton.MainForm != null)
		{
			ParserService.OnSolutionLoaded();
		}
		isLoading = false;
	}

	internal static void ParserServiceCreatedProjectContents()
	{
		if (WorkbenchSingleton.MainForm != null)
		{
			bool flag = false;
			if (OpenSolution.Preferences.HasFilesToOpenAfterSolutionLoad || AbstractProject.filesToOpenAfterSolutionLoad.Count > 0)
			{
				flag = true;
				StatusBarService.ProgressMonitor.ShowNotification("ParserServiceCreatedProjectContents", "Loading solution's files...");
			}
			foreach (string item in AbstractProject.filesToOpenAfterSolutionLoad)
			{
				if (File.Exists(item))
				{
					FileService.OpenFile(item);
				}
			}
			if (PropertyService.Get("SharpDevelop.TreatUnrelatedFilesAsSolutions", defaultValue: false))
			{
				string[] filesToOpenAfterSolutionLoad = OpenSolution.Preferences.FilesToOpenAfterSolutionLoad;
				foreach (string text in filesToOpenAfterSolutionLoad)
				{
					if (File.Exists(text))
					{
						FileService.OpenFile(text);
					}
				}
			}
			OpenSolution.Preferences.ClearFilesToOpenAfterSolutionLoad();
			if (flag)
			{
				StatusBarService.ProgressMonitor.Done("ParserServiceCreatedProjectContents");
			}
		}
		AbstractProject.filesToOpenAfterSolutionLoad.Clear();
	}

	private static void ApplyConfigurationAndReadPreferences()
	{
		openSolution.ApplySolutionConfigurationAndPlatformToProjects();
		bool useProjectSubfolder = PropertyService.Get("SharpDevelop.PreferenceInSolutionFolder", defaultValue: false);
		foreach (IProject project in openSolution.Projects)
		{
			string preferenceFileName = GetPreferenceFileName(project.FileName, useProjectSubfolder);
			if (FileUtility.IsValidFileName(preferenceFileName) && File.Exists(preferenceFileName))
			{
				project.SetMemento(Properties.Load(preferenceFileName));
			}
		}
	}

	public static void LoadProject(string fileName)
	{
		isLoading = true;
		string text = Path.ChangeExtension(fileName, ".sln");
		if (File.Exists(text))
		{
			LoadSolution(text);
			if (openSolution == null)
			{
				isLoading = false;
				return;
			}
			bool flag = false;
			foreach (IProject project2 in openSolution.Projects)
			{
				if (FileUtility.IsEqualFileName(fileName, project2.FileName))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				isLoading = false;
				return;
			}
			string[,] customTags = new string[2, 2]
			{
				{
					"SolutionName",
					Path.GetFileName(text)
				},
				{
					"ProjectName",
					Path.GetFileName(fileName)
				}
			};
			switch (MessageService.ShowCustomDialog(MessageService.ProductName, StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.OpenCombine.SolutionDoesNotContainProject}", customTags), 0, 2, StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.OpenCombine.SolutionDoesNotContainProject.AddProjectToSolution}", customTags), StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.OpenCombine.SolutionDoesNotContainProject.CreateNewSolution}", customTags), "${res:Global.IgnoreButtonText}"))
			{
			case 0:
				AddExitingProjectToSolution.AddProject((ISolutionFolderNode)ProjectBrowserPad.Instance.SolutionNode, fileName);
				SaveSolution();
				isLoading = false;
				return;
			case 1:
				break;
			default:
				isLoading = false;
				return;
			}
			CloseSolution();
			try
			{
				File.Copy(text, Path.ChangeExtension(text, ".old.sln"), overwrite: true);
			}
			catch (IOException)
			{
				isLoading = false;
			}
		}
		Solution solution = new Solution();
		solution.Name = Path.GetFileNameWithoutExtension(fileName);
		ILanguageBinding bindingPerProjectFile = LanguageBindingService.GetBindingPerProjectFile(fileName);
		if (bindingPerProjectFile != null)
		{
			IProject project = LanguageBindingService.LoadProject(solution, fileName, solution.Name);
			if (project is UnknownProject)
			{
				if (!((UnknownProject)project).WarningDisplayedToUser)
				{
					((UnknownProject)project).ShowWarningMessageBox();
				}
				isLoading = false;
				return;
			}
			solution.AddFolder(project);
			ProjectSection solutionConfigurationsSection = solution.GetSolutionConfigurationsSection();
			foreach (string configurationName in project.ConfigurationNames)
			{
				foreach (string platformName in project.PlatformNames)
				{
					string text2 = ((!(platformName == "AnyCPU")) ? (configurationName + "|" + platformName) : (configurationName + "|Any CPU"));
					solutionConfigurationsSection.Items.Add(new SolutionItem(text2, text2));
				}
			}
			solution.FixSolutionConfiguration(new IProject[1] { project });
			if (FileUtility.ObservedSave((NamedFileOperationDelegate)solution.Save, text) == FileOperationResult.OK)
			{
				LoadSolution(text);
			}
			isLoading = false;
		}
		else
		{
			MessageService.ShowError(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.OpenCombine.InvalidProjectOrCombine}", new string[1, 2] { { "FileName", fileName } }));
			isLoading = false;
		}
	}

	public static void SaveSolution()
	{
		if (openSolution == null)
		{
			return;
		}
		openSolution.Save(forceShowError: false);
		foreach (IProject project in openSolution.Projects)
		{
			try
			{
				project.Save();
			}
			catch (UnauthorizedAccessException)
			{
				if (!PropertyService.Get("SharpDevelop.SilentReadOnlyWarnings", defaultValue: false))
				{
					string text = $"The access to the project {project.Name} is restricted (probably it is Read Only) and can not be saved.";
					if (PropertyService.Get("SharpDevelop.ReadOnlyPrjWarning", defaultValue: true))
					{
						MessageService.ShowWarning(text);
					}
					TaskService.BuildMessageViewCategory.AppendLine(text);
					TaskService.Add(new Task(null, text, 0, 0, TaskType.Warning));
				}
			}
		}
		OnSolutionSaved(new SolutionEventArgs(openSolution));
	}

	public static string GetAllProjectsFilter(object caller)
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/Workbench/Combine/FileFilter");
		StringBuilder stringBuilder = new StringBuilder(StringParser.Parse("${res:SharpDevelop.Solution.AllKnownProjectFormats}|"));
		bool flag = true;
		foreach (Codon codon in treeNode.Codons)
		{
			string text = codon.Properties.Get("extensions", "");
			if (text != "*.*" && text.Length > 0)
			{
				if (!flag)
				{
					stringBuilder.Append(';');
				}
				else
				{
					flag = false;
				}
				stringBuilder.Append(text);
			}
		}
		ArrayList arrayList = treeNode.BuildChildItems(caller);
		arrayList.Sort(StringComparer.OrdinalIgnoreCase);
		foreach (string item in arrayList)
		{
			stringBuilder.Append('|');
			stringBuilder.Append(item);
		}
		return stringBuilder.ToString();
	}

	private static string GetPreferenceFileName(string projectFileName, bool useProjectSubfolder)
	{
		if (useProjectSubfolder)
		{
			string text = Path.Combine(Path.GetDirectoryName(projectFileName), "temp");
			string relativePath = FileUtility.GetRelativePath(text, projectFileName);
			return Path.Combine(text, Path.GetFileName(projectFileName) + "." + relativePath.ToLowerInvariant().GetHashCode().ToString("x") + ".xml");
		}
		string path = Path.Combine(PropertyService.ConfigDirectory, "preferences");
		return Path.Combine(path, Path.GetFileName(projectFileName) + "." + projectFileName.ToLowerInvariant().GetHashCode().ToString("x") + ".xml");
	}

	public static void SaveSolutionPreferences()
	{
		if (openSolution == null)
		{
			return;
		}
		bool useProjectSubfolder = PropertyService.Get("SharpDevelop.PreferenceInSolutionFolder", defaultValue: false);
		openSolution.Preferences.ClearFilesToOpenAfterSolutionLoad();
		if (PropertyService.Get("SharpDevelop.TreatUnrelatedFilesAsSolutions", defaultValue: false))
		{
			openSolution.Preferences.SetFilesToOpenAfterSolutionLoad(FreeOpenFiles());
		}
		if (ProjectService.SolutionPreferencesSaving != null)
		{
			ProjectService.SolutionPreferencesSaving(null, new SolutionEventArgs(openSolution));
		}
		Properties properties = ((IMementoCapable)openSolution.Preferences).CreateMemento();
		string preferenceFileName = GetPreferenceFileName(openSolution.FileName, useProjectSubfolder);
		if (FileUtility.IsValidFileName(preferenceFileName))
		{
			if (!Directory.Exists(Path.GetDirectoryName(preferenceFileName)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(preferenceFileName));
			}
			FileUtility.ObservedSave(properties.Save, preferenceFileName, FileErrorPolicy.Inform);
		}
		foreach (IProject project in OpenSolution.Projects)
		{
			properties = project.CreateMemento();
			if (properties == null)
			{
				continue;
			}
			preferenceFileName = GetPreferenceFileName(project.FileName, useProjectSubfolder);
			if (FileUtility.IsValidFileName(preferenceFileName))
			{
				if (!Directory.Exists(Path.GetDirectoryName(preferenceFileName)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(preferenceFileName));
				}
				FileUtility.ObservedSave(properties.Save, preferenceFileName, FileErrorPolicy.Inform);
			}
		}
	}

	public static bool CloseSolution()
	{
		if (openSolution != null)
		{
			foreach (IProject project in OpenSolution.Projects)
			{
				if (project.IsDirty)
				{
					switch (MessageBox.Show(ResourceService.GetString("MainWindow.SaveChangesMessage"), ResourceService.GetString("MainWindow.SaveChangesMessageHeader") + " " + project.Name + " ?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, RightToLeftConverter.IsRightToLeft ? (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) : ((MessageBoxOptions)0)))
					{
					case DialogResult.Cancel:
						return false;
					case DialogResult.Yes:
						project.Save();
						break;
					}
				}
			}
			CurrentProject = null;
			SolutionCancelEventArgs e = new SolutionCancelEventArgs(openSolution);
			OnSolutionClosing(e);
			if (e.Cancel)
			{
				return false;
			}
			openSolution.Dispose();
			openSolution = null;
			OnSolutionClosed(EventArgs.Empty);
		}
		return true;
	}

	public static void MarkFileDirty(string fileName)
	{
		if (OpenSolution == null)
		{
			return;
		}
		foreach (IProject project in OpenSolution.Projects)
		{
			if (project.IsFileInProject(fileName))
			{
				MarkProjectDirty(project);
			}
		}
	}

	public static void MarkProjectDirty(IProject project)
	{
		project.IsDirty = true;
	}

	private static void OnCurrentProjectChanged(ProjectEventArgs e)
	{
		if (ProjectService.CurrentProjectChanged != null)
		{
			ProjectService.CurrentProjectChanged(null, e);
		}
	}

	private static void OnSolutionClosed(EventArgs e)
	{
		if (ProjectService.SolutionClosed != null)
		{
			ProjectService.SolutionClosed(null, e);
		}
	}

	private static bool OnSolutionClosing(SolutionCancelEventArgs e)
	{
		if (ProjectService.SolutionClosing != null)
		{
			ProjectService.SolutionClosing(null, e);
			if (e.Cancel)
			{
				return false;
			}
		}
		return true;
	}

	private static void OnSolutionLoaded(SolutionEventArgs e)
	{
		if (ProjectService.SolutionLoadedFirstChanceCall != null)
		{
			ProjectService.SolutionLoadedFirstChanceCall(null, e);
		}
		if (ProjectService.SolutionLoaded != null)
		{
			ProjectService.SolutionLoaded(null, e);
		}
	}

	private static void OnSolutionSaved(SolutionEventArgs e)
	{
		if (ProjectService.SolutionSaved != null)
		{
			ProjectService.SolutionSaved(null, e);
		}
	}

	private static void OnProjectConfigurationChanged(ProjectConfigurationEventArgs e)
	{
		if (ProjectService.ProjectConfigurationChanged != null)
		{
			ProjectService.ProjectConfigurationChanged(null, e);
		}
	}

	private static void OnSolutionConfigurationChanged(SolutionConfigurationEventArgs e)
	{
		if (ProjectService.SolutionConfigurationChanged != null)
		{
			ProjectService.SolutionConfigurationChanged(null, e);
		}
	}

	public static void SetConfiguration(string value)
	{
		OpenSolution.Preferences.ActiveConfiguration = value;
		OpenSolution.ApplySolutionConfigurationAndPlatformToProjects();
		OnSolutionConfigurationChanged(new SolutionConfigurationEventArgs(value));
	}

	private static void Init()
	{
		if (string.IsNullOrEmpty(semName))
		{
			semName = "ClarionSem" + Guid.NewGuid().ToString();
		}
		if (sem == null)
		{
			sem = new Semaphore(0, 1, semName);
		}
	}

	public static void RaiseEventStartBuild(BuildOptions options)
	{
		Init();
		building = true;
		if (ProjectService.StartBuild != null)
		{
			ProjectService.StartBuild(options, EventArgs.Empty);
		}
	}

	public static void RaiseEventEndBuild()
	{
		building = false;
		if (ProjectService.EndBuild != null)
		{
			ProjectService.EndBuild(null, EventArgs.Empty);
		}
	}

	public static void RaiseEventBuildFinished()
	{
		building = false;
		if (ProjectService.BuildFinished != null)
		{
			ProjectService.BuildFinished(null, EventArgs.Empty);
		}
	}

	public static void RemoveSolutionFolder(string guid)
	{
		if (OpenSolution == null)
		{
			return;
		}
		foreach (ISolutionFolder solutionFolder in OpenSolution.SolutionFolders)
		{
			if (solutionFolder.IdGuid == guid)
			{
				solutionFolder.Parent.RemoveFolder(solutionFolder);
				OnSolutionFolderRemoved(new SolutionFolderEventArgs(solutionFolder));
				break;
			}
		}
	}

	public static void RaiseProjectItemAdded(IProject project, ProjectItem item)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		OnProjectItemAdded(new ProjectItemEventArgs(project, item));
	}

	private static void OnSolutionFolderRemoved(SolutionFolderEventArgs e)
	{
		if (ProjectService.SolutionFolderRemoved != null)
		{
			ProjectService.SolutionFolderRemoved(null, e);
		}
	}

	private static void OnProjectItemAdded(ProjectItemEventArgs e)
	{
		if (ProjectService.ProjectItemAdded != null)
		{
			ProjectService.ProjectItemAdded(null, e);
		}
	}

	private static void OnProjectItemRemoved(ProjectItemEventArgs e)
	{
		if (ProjectService.ProjectItemRemoved != null)
		{
			ProjectService.ProjectItemRemoved(null, e);
		}
	}

	private static void OnProjectAdded(ProjectEventArgs e)
	{
		if (ProjectService.ProjectAdded != null)
		{
			ProjectService.ProjectAdded(null, e);
		}
	}

	public static bool IsProcessRuning(string processFileName)
	{
		if (!string.IsNullOrEmpty(processFileName))
		{
			string extension = Path.GetExtension(processFileName);
			if (!string.IsNullOrEmpty(extension) && extension.Equals(".EXE", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					Process[] processesByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processFileName));
					if (processesByName.Length > 0)
					{
						Process[] array = processesByName;
						foreach (Process process in array)
						{
							try
							{
								if (process.MainModule.FileName.Equals(processFileName, StringComparison.InvariantCultureIgnoreCase))
								{
									return true;
								}
							}
							catch (Win32Exception)
							{
							}
						}
					}
				}
				catch (Exception ex2)
				{
					LoggingService.Info(string.Format(StringParser.Parse("${res:IsProcessRuningException}"), processFileName));
					MessageService.ShowMessage(ex2, string.Format(StringParser.Parse("${res:IsProcessRunningError}"), processFileName), "${res:IsProcessRunningErrorTitle}");
				}
			}
		}
		return false;
	}

	public static bool IsTargetRuning(IProject targetProject)
	{
		if (targetProject != null)
		{
			return IsProcessRuning(targetProject.OutputAssemblyFullPath);
		}
		return false;
	}

	public static void KillRunningTarget(IProject targetProject)
	{
		if (targetProject != null)
		{
			string outputAssemblyFullPath = targetProject.OutputAssemblyFullPath;
			if (KillRunningProcess(outputAssemblyFullPath))
			{
				LoggingService.Info("The " + outputAssemblyFullPath + " process was stopped.");
			}
		}
	}

	public static bool KillRunningProcess(string outputFile)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(outputFile))
		{
			string extension = Path.GetExtension(outputFile);
			if (!string.IsNullOrEmpty(extension) && extension.Equals(".EXE", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					Process[] processesByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(outputFile));
					if (processesByName.Length > 0)
					{
						Process[] array = processesByName;
						foreach (Process process in array)
						{
							if (process.MainModule.FileName.Equals(outputFile, StringComparison.InvariantCultureIgnoreCase))
							{
								try
								{
									process.Kill();
									result = true;
								}
								catch (Win32Exception ex)
								{
									LoggingService.Info(string.Format(StringParser.Parse("${res:KillRunningProcessException.Win32Exception}"), outputFile));
									MessageService.ShowMessage(ex, string.Format(StringParser.Parse("${res:KillRunningProcessException.Win32Exception}"), outputFile), "${res:IsProcessRunningErrorTitle}");
								}
								catch (InvalidOperationException ex2)
								{
									LoggingService.Info(string.Format(StringParser.Parse("${res:KillRunningProcessException.InvalidOperationException}"), outputFile));
									MessageService.ShowMessage(ex2, string.Format(StringParser.Parse("${res:KillRunningProcessException.InvalidOperationException}"), outputFile), "${res:IsProcessRunningErrorTitle}");
								}
								catch (NotSupportedException ex3)
								{
									LoggingService.Info(string.Format(StringParser.Parse("${res:KillRunningProcessException.NotSupportedException}"), outputFile));
									MessageService.ShowMessage(ex3, string.Format(StringParser.Parse("${res:KillRunningProcessException.NotSupportedException}"), outputFile), "${res:IsProcessRunningErrorTitle}");
								}
							}
						}
					}
				}
				catch (Exception ex4)
				{
					LoggingService.Info(string.Format(StringParser.Parse("${res:KillRunningProcessException}"), outputFile));
					MessageService.ShowMessage(ex4, string.Format(StringParser.Parse("${res:KillRunningProcessException}"), outputFile), "${res:IsProcessRunningErrorTitle}");
				}
			}
		}
		return result;
	}

	public static bool OpenSolutionHasTargetRunning()
	{
		if (OpenSolution != null)
		{
			foreach (IProject project in OpenSolution.Projects)
			{
				if (IsTargetRuning(project))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void KillSolutionRunningTargets()
	{
		if (OpenSolution == null)
		{
			return;
		}
		foreach (IProject project in OpenSolution.Projects)
		{
			KillRunningTarget(project);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TextEditor.Util;

namespace ICSharpCode.SharpDevelop;

public static class ParserService
{
	public const string ProgressId = "Parsing";

	private static IList<ParserDescriptor> parser;

	private static IList<ProjectContentRegistryDescriptor> registries;

	private static Dictionary<IProject, IProjectContent> projectContents = new Dictionary<IProject, IProjectContent>();

	private static Dictionary<string, ParseInformation> parsings = new Dictionary<string, ParseInformation>();

	private static ProjectContentRegistry defaultProjectContentRegistry = new ProjectContentRegistry();

	private static string domPersistencePath;

	private static bool initialized = false;

	private static Thread loadSolutionProjectsThread;

	private static bool abortLoadSolutionProjectsThread;

	private static Queue<ParseProjectContent> reParse1 = new Queue<ParseProjectContent>();

	private static Queue<ParseProjectContent> reParse2 = new Queue<ParseProjectContent>();

	private static Thread reParseThread;

	private static bool _IsParsing = false;

	private static Queue<KeyValuePair<string, string>> parseQueue = new Queue<KeyValuePair<string, string>>();

	private static volatile bool abortParserUpdateThread = false;

	private static Dictionary<string, int> lastUpdateHash = new Dictionary<string, int>();

	private static DefaultProjectContent defaultProjectContent;

	public static readonly string[] DefaultTaskListTokens = new string[4] { "HACK", "TODO", "UNDONE", "FIXME" };

	public static string DomPersistencePath
	{
		get
		{
			return domPersistencePath;
		}
		set
		{
			if (parser != null)
			{
				throw new InvalidOperationException("Cannot set DomPersistencePath after ParserService was initialized");
			}
			domPersistencePath = value;
		}
	}

	public static ProjectContentRegistry DefaultProjectContentRegistry => defaultProjectContentRegistry;

	public static IProjectContent CurrentProjectContent
	{
		[DebuggerStepThrough]
		get
		{
			if (ProjectService.CurrentProject == null || !projectContents.ContainsKey(ProjectService.CurrentProject))
			{
				return DefaultProjectContent;
			}
			return projectContents[ProjectService.CurrentProject];
		}
	}

	public static IEnumerable<IProjectContent> AllProjectContents => projectContents.Values;

	public static bool LoadSolutionProjectsThreadRunning => loadSolutionProjectsThread != null;

	public static bool IsParsing => _IsParsing;

	public static bool IsParserThreadRunning => !abortParserUpdateThread;

	public static IProjectContent DefaultProjectContent
	{
		get
		{
			if (defaultProjectContent == null)
			{
				lock (projectContents)
				{
					if (defaultProjectContent == null)
					{
						CreateDefaultProjectContent();
					}
				}
			}
			return defaultProjectContent;
		}
	}

	public static Encoding DefaultFileEncoding => SharpDevelopTextEditorProperties.Instance.Encoding;

	public static event ParserUpdateStepEventHandler ParserUpdateStepFinished;

	public static event ParseInformationEventHandler ParseInformationUpdated;

	public static event EventHandler LoadSolutionProjectsThreadEnded;

	internal static void InitializeParserService()
	{
		if (parser == null && !initialized)
		{
			initialized = true;
			parser = AddInTree.BuildItems<ParserDescriptor>("/Workspace/Parser", null, throwOnNotFound: false);
			registries = AddInTree.BuildItems<ProjectContentRegistryDescriptor>("/Workspace/ProjectContentRegistry", null, throwOnNotFound: false);
			domPersistencePath = Path.Combine(PropertyService.ConfigDirectory, "DomPersistence");
			Directory.CreateDirectory(domPersistencePath);
			defaultProjectContentRegistry.ActivatePersistence(domPersistencePath);
			ProjectService.SolutionClosed += ProjectServiceSolutionClosed;
			if (WorkbenchSingleton.Workbench != null)
			{
				WorkbenchSingleton.Workbench.ViewClosed += ViewClosed;
			}
			else
			{
				WorkbenchSingleton.WorkbenchCreated += WorkbenchCreated;
			}
		}
	}

	private static void WorkbenchCreated(object sender, EventArgs e)
	{
		WorkbenchSingleton.WorkbenchCreated -= WorkbenchCreated;
		WorkbenchSingleton.Workbench.ViewClosed += ViewClosed;
	}

	private static void ViewClosed(object sender, ViewContentEventArgs e)
	{
		string fileName = e.Content.FileName ?? e.Content.UntitledName;
		bool flag = true;
		if (ProjectService.OpenSolution != null && ProjectService.OpenSolution.FindProjectContainingFile(fileName) != null)
		{
			flag = false;
		}
		if (flag)
		{
			ClearParseInformation(fileName);
		}
	}

	private static void ProjectServiceSolutionClosed(object sender, EventArgs e)
	{
		abortLoadSolutionProjectsThread = true;
		lock (reParse1)
		{
			reParse1.Clear();
			reParse2.Clear();
		}
		lock (projectContents)
		{
			foreach (IProjectContent value in projectContents.Values)
			{
				value.Dispose();
			}
			projectContents.Clear();
		}
		lock (parsings)
		{
			parsings.Clear();
		}
		lock (parseQueue)
		{
			parseQueue.Clear();
		}
		lock (lastUpdateHash)
		{
			lastUpdateHash.Clear();
		}
	}

	internal static void OnSolutionLoaded()
	{
		if (loadSolutionProjectsThread != null)
		{
			if (!abortLoadSolutionProjectsThread)
			{
				throw new InvalidOperationException("Cannot open new solution without closing old solution!");
			}
			if (!loadSolutionProjectsThread.Join(50))
			{
				WorkbenchSingleton.SafeThreadAsyncCall(OnSolutionLoaded);
				return;
			}
		}
		loadSolutionProjectsThread = new Thread(LoadSolutionProjects);
		loadSolutionProjectsThread.SetApartmentState(ApartmentState.STA);
		loadSolutionProjectsThread.Name = "loadSolutionProjects";
		loadSolutionProjectsThread.Priority = ThreadPriority.BelowNormal;
		loadSolutionProjectsThread.IsBackground = true;
		loadSolutionProjectsThread.Start();
	}

	private static void LoadSolutionProjects()
	{
		try
		{
			abortLoadSolutionProjectsThread = false;
			LoggingService.Info("Start LoadSolutionProjects thread");
			LoadSolutionProjectsInternal();
		}
		finally
		{
			LoggingService.Info("LoadSolutionProjects thread ended");
			loadSolutionProjectsThread = null;
			OnLoadSolutionProjectsThreadEnded(EventArgs.Empty);
		}
	}

	private static void LoadSolutionProjectsInternal()
	{
		InitializeParserService();
		List<ParseProjectContent> list = new List<ParseProjectContent>();
		foreach (IProject project in ProjectService.OpenSolution.Projects)
		{
			try
			{
				ParseProjectContent parseProjectContent = project.CreateProjectContent();
				if (parseProjectContent != null)
				{
					lock (projectContents)
					{
						projectContents[project] = parseProjectContent;
					}
					list.Add(parseProjectContent);
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex, "Error while retrieving project contents from " + project);
			}
		}
		WorkbenchSingleton.SafeThreadAsyncCall(ProjectService.ParserServiceCreatedProjectContents);
		int num = 0;
		foreach (ParseProjectContent item in list)
		{
			if (abortLoadSolutionProjectsThread)
			{
				return;
			}
			try
			{
				item.Initialize1();
				num += item.GetInitializationWorkAmount();
			}
			catch (Exception ex2)
			{
				MessageService.ShowError(ex2, "Error while initializing project references:" + item);
			}
		}
		foreach (ParseProjectContent item2 in list)
		{
			if (!abortLoadSolutionProjectsThread)
			{
				try
				{
					item2.Initialize2();
				}
				catch (Exception ex3)
				{
					MessageService.ShowError(ex3, "Error while initializing project contents:" + item2);
				}
				continue;
			}
			break;
		}
	}

	private static void InitAddedProject(object state)
	{
		ParseProjectContent parseProjectContent = (ParseProjectContent)state;
		parseProjectContent.Initialize1();
		parseProjectContent.Initialize2();
	}

	private static void ReparseProjects()
	{
		_IsParsing = true;
		LoggingService.Info("reParse thread started");
		Thread.Sleep(100);
		try
		{
			ReparseProjectsInternal();
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
		finally
		{
			_IsParsing = false;
		}
	}

	private static void ReparseProjectsInternal()
	{
		bool flag = false;
		while (true)
		{
			ParseProjectContent parseProjectContent;
			lock (reParse1)
			{
				if (reParse1.Count > 0)
				{
					flag = false;
					parseProjectContent = reParse1.Dequeue();
				}
				else
				{
					if (reParse2.Count <= 0)
					{
						reParseThread = null;
						LoggingService.Info("reParse thread finished all jobs");
						break;
					}
					if (!flag)
					{
						int num = 0;
						foreach (ParseProjectContent item in reParse2)
						{
							num += item.GetInitializationWorkAmount();
						}
					}
					flag = true;
					parseProjectContent = reParse2.Dequeue();
				}
			}
			if (flag)
			{
				LoggingService.Info("reparsing code for " + parseProjectContent.Project);
				parseProjectContent.ReInitialize2();
			}
			else
			{
				LoggingService.Debug("reloading references for " + parseProjectContent.Project);
				parseProjectContent.ReInitialize1();
			}
		}
	}

	public static void Reparse(IProject project, bool initReferences, bool parseCode)
	{
		if (!(GetProjectContent(project) is ParseProjectContent item))
		{
			return;
		}
		lock (reParse1)
		{
			if (initReferences && !reParse1.Contains(item))
			{
				LoggingService.Debug("Enqueue for reinitializing references: " + project);
				reParse1.Enqueue(item);
			}
			if (parseCode && !reParse2.Contains(item))
			{
				LoggingService.Debug("Enqueue for reparsing code: " + project);
				reParse2.Enqueue(item);
			}
			if (reParseThread == null)
			{
				LoggingService.Info("Starting reParse thread");
				reParseThread = new Thread(ReparseProjects);
				reParseThread.SetApartmentState(ApartmentState.STA);
				reParseThread.Name = "reParse";
				reParseThread.Priority = ThreadPriority.BelowNormal;
				reParseThread.IsBackground = true;
				reParseThread.Start();
			}
		}
	}

	internal static IProjectContent CreateProjectContentForAddedProject(IProject project)
	{
		lock (projectContents)
		{
			ParseProjectContent parseProjectContent = project.CreateProjectContent();
			if (parseProjectContent != null)
			{
				projectContents[project] = parseProjectContent;
				ThreadPool.QueueUserWorkItem(InitAddedProject, parseProjectContent);
			}
			return parseProjectContent;
		}
	}

	internal static void RemoveProjectContentForRemovedProject(IProject project)
	{
		lock (projectContents)
		{
			projectContents.Remove(project);
		}
	}

	public static IProjectContent GetProjectContent(IProject project)
	{
		lock (projectContents)
		{
			if (projectContents.ContainsKey(project))
			{
				return projectContents[project];
			}
		}
		return null;
	}

	private static void ParseQueue()
	{
		while (true)
		{
			KeyValuePair<string, string> keyValuePair;
			lock (parseQueue)
			{
				if (parseQueue.Count == 0)
				{
					break;
				}
				keyValuePair = parseQueue.Dequeue();
			}
			ParseFile(keyValuePair.Key, keyValuePair.Value);
		}
	}

	public static void EnqueueForParsing(string fileName)
	{
		EnqueueForParsing(fileName, GetParseableFileContent(fileName));
	}

	public static void EnqueueForParsing(string fileName, string fileContent)
	{
		lock (parseQueue)
		{
			parseQueue.Enqueue(new KeyValuePair<string, string>(fileName, fileContent));
		}
	}

	public static void StartParserThread()
	{
		InitializeParserService();
		if (!IsParserThreadRunning)
		{
			abortParserUpdateThread = false;
			Thread thread = new Thread(ParserUpdateThread);
			thread.Name = "parser";
			thread.Priority = ThreadPriority.BelowNormal;
			thread.IsBackground = true;
			thread.Start();
		}
	}

	public static void StopParserThread()
	{
		InitializeParserService();
		abortParserUpdateThread = true;
	}

	private static void ParserUpdateThread()
	{
		LoggingService.Info("ParserUpdateThread started");
		Thread.Sleep(750);
		_ = defaultProjectContentRegistry.Mscorlib;
		while (!abortParserUpdateThread)
		{
			try
			{
				ParseQueue();
				ParserUpdateStep();
			}
			catch (Exception)
			{
				Thread.Sleep(10000);
			}
			Thread.Sleep(2000);
		}
		LoggingService.Info("ParserUpdateThread stopped");
	}

	private static object[] GetWorkbench()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null)
		{
			return null;
		}
		IBaseViewContent activeViewContent = activeWorkbenchWindow.ActiveViewContent;
		if (activeViewContent == null)
		{
			return null;
		}
		return new object[2] { activeViewContent, activeWorkbenchWindow.ViewContent };
	}

	public static void ParseCurrentViewContent()
	{
		ParserUpdateStep();
	}

	private static void ParserUpdateStep()
	{
		object[] array;
		try
		{
			array = WorkbenchSingleton.SafeThreadFunction(GetWorkbench);
		}
		catch (InvalidOperationException)
		{
			LoggingService.Warn("InvalidOperationException while trying to invoke GetWorkbench()");
			return;
		}
		if (array == null || !(array[0] is IEditable editable))
		{
			return;
		}
		string text = null;
		IViewContent viewContent = (IViewContent)array[1];
		IParseableContent parseableContent = array[0] as IParseableContent;
		string text2 = null;
		if (parseableContent != null)
		{
			text = parseableContent.ParseableContentName;
			text2 = parseableContent.ParseableText;
		}
		else
		{
			text = (viewContent.IsUntitled ? viewContent.UntitledName : viewContent.FileName);
		}
		if (text == null || text.Length == 0)
		{
			return;
		}
		ParseInformation parseInformation = null;
		bool flag = false;
		if (text2 == null)
		{
			text2 = editable.Text;
			if (text2 == null)
			{
				return;
			}
		}
		int hashCode = text2.GetHashCode();
		bool flag2;
		lock (lastUpdateHash)
		{
			flag2 = !lastUpdateHash.ContainsKey(text) || lastUpdateHash[text] != hashCode;
		}
		if (flag2)
		{
			parseInformation = ParseFile(text, text2, !viewContent.IsUntitled);
			lock (lastUpdateHash)
			{
				lastUpdateHash[text] = hashCode;
			}
			flag = true;
		}
		if (flag && parseInformation != null && editable is IParseInformationListener)
		{
			((IParseInformationListener)editable).ParseInformationUpdated(parseInformation);
		}
		OnParserUpdateStepFinished(new ParserUpdateStepEventArgs(text, text2, flag, parseInformation));
	}

	public static void ParseViewContent(IViewContent viewContent)
	{
		string text = ((IEditable)viewContent).Text;
		ParseInformation parseInformation = ParseFile(viewContent.IsUntitled ? viewContent.UntitledName : viewContent.FileName, text, !viewContent.IsUntitled);
		if (parseInformation != null && viewContent is IParseInformationListener)
		{
			((IParseInformationListener)viewContent).ParseInformationUpdated(parseInformation);
		}
	}

	public static void UpdateFileOnNextParserStep(string fileName)
	{
		lock (lastUpdateHash)
		{
			if (lastUpdateHash.ContainsKey(fileName))
			{
				lastUpdateHash.Remove(fileName);
			}
		}
	}

	private static void OnParserUpdateStepFinished(ParserUpdateStepEventArgs e)
	{
		if (ParserService.ParserUpdateStepFinished != null)
		{
			ParserService.ParserUpdateStepFinished(typeof(ParserService), e);
		}
	}

	public static ParseInformation ParseFile(string fileName)
	{
		return ParseFile(fileName, null);
	}

	public static ParseInformation ParseFile(string fileName, string fileContent)
	{
		return ParseFile(fileName, fileContent, updateCommentTags: true);
	}

	private static IProjectContent GetProjectContent(string fileName)
	{
		lock (projectContents)
		{
			foreach (KeyValuePair<IProject, IProjectContent> projectContent in projectContents)
			{
				if (projectContent.Key.IsFileInProject(fileName))
				{
					return projectContent.Value;
				}
			}
		}
		return null;
	}

	private static void CreateDefaultProjectContent()
	{
		LoggingService.Info("Creating default project content");
		defaultProjectContent = new DefaultProjectContent();
		defaultProjectContent.AddReferencedContent(defaultProjectContentRegistry.Mscorlib);
		CreateDefaultProjectContentReferences();
	}

	private static void CreateDefaultProjectContentReferences()
	{
		IList<string> list = AddInTree.BuildItems<string>("/SharpDevelop/Services/ParserService/SingleFileGacReferences", null, throwOnNotFound: false);
		foreach (string item2 in list)
		{
			ReferenceProjectItem item = new ReferenceProjectItem(null, item2);
			defaultProjectContent.AddReferencedContent(GetProjectContentForReference(item));
		}
		if (WorkbenchSingleton.Workbench == null)
		{
			return;
		}
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += delegate
		{
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
			{
				string text = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName ?? WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.UntitledName;
				if (text != null)
				{
					IParser parser = GetParser(text);
					if (parser != null && parser.Language != null)
					{
						defaultProjectContent.Language = parser.Language;
						defaultProjectContent.DefaultImports = parser.Language.CreateDefaultImports(defaultProjectContent);
					}
				}
			}
		};
	}

	public static ParseInformation ParseFile(string fileName, string fileContent, bool updateCommentTags)
	{
		return ParseFile(null, fileName, fileContent, updateCommentTags);
	}

	public static ParseInformation ParseFile(IProjectContent fileProjectContent, string fileName, string fileContent, bool updateCommentTags)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentNullException("fileName");
		}
		IParser parser = GetParser(fileName);
		if (parser == null)
		{
			return null;
		}
		ICompilationUnit compilationUnit = null;
		try
		{
			if (fileProjectContent == null)
			{
				fileProjectContent = GetProjectContent(fileName);
				if (fileProjectContent == null)
				{
					fileProjectContent = DefaultProjectContent;
				}
			}
			if (fileContent == null)
			{
				if (!File.Exists(fileName))
				{
					return null;
				}
				fileContent = GetParseableFileContent(fileName);
			}
			compilationUnit = parser.Parse(fileProjectContent, fileName, fileContent);
			if (parsings.ContainsKey(fileName))
			{
				ParseInformation parseInformation = parsings[fileName];
				fileProjectContent.UpdateCompilationUnit(parseInformation.MostRecentCompilationUnit, compilationUnit, fileName);
			}
			else
			{
				fileProjectContent.UpdateCompilationUnit(null, compilationUnit, fileName);
			}
			if (updateCommentTags)
			{
				TaskService.UpdateCommentTags(fileName, compilationUnit.TagComments);
			}
			return UpdateParseInformation(compilationUnit, fileName, updateCommentTags);
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static ParseInformation UpdateParseInformation(ICompilationUnit parserOutput, string fileName, bool updateCommentTags)
	{
		if (!parsings.ContainsKey(fileName))
		{
			parsings[fileName] = new ParseInformation();
		}
		ParseInformation parseInformation = parsings[fileName];
		try
		{
			OnParseInformationUpdated(new ParseInformationEventArgs(fileName, parseInformation, parserOutput));
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
		if (parserOutput.ErrorsDuringCompile)
		{
			parseInformation.DirtyCompilationUnit = parserOutput;
		}
		else
		{
			parseInformation.ValidCompilationUnit = parserOutput;
			parseInformation.DirtyCompilationUnit = null;
		}
		return parseInformation;
	}

	public static string GetParseableFileContent(string fileName)
	{
		IWorkbenchWindow openFile = FileService.GetOpenFile(fileName);
		if (openFile != null)
		{
			IViewContent viewContent = openFile.ViewContent;
			if (viewContent is IEditable editable)
			{
				return editable.Text;
			}
		}
		Encoding defaultFileEncoding = DefaultFileEncoding;
		return FileReader.ReadFileContent(fileName, defaultFileEncoding);
	}

	public static ParseInformation GetParseInformation(string fileName)
	{
		if (fileName == null || fileName.Length == 0)
		{
			return null;
		}
		if (!parsings.ContainsKey(fileName))
		{
			return ParseFile(fileName);
		}
		return parsings[fileName];
	}

	public static ParseInformation GetParseInformationIfExist(string fileName)
	{
		InitializeParserService();
		if (fileName == null || fileName.Length == 0)
		{
			return null;
		}
		if (!parsings.ContainsKey(fileName))
		{
			return null;
		}
		return parsings[fileName];
	}

	public static void ClearParseInformation(string fileName)
	{
		if (fileName == null || fileName.Length == 0)
		{
			return;
		}
		LoggingService.Info("ClearParseInformation: " + fileName);
		if (!parsings.ContainsKey(fileName))
		{
			return;
		}
		ParseInformation parseInformation = parsings[fileName];
		if (parseInformation != null && parseInformation.MostRecentCompilationUnit != null)
		{
			parseInformation.MostRecentCompilationUnit.ProjectContent.RemoveCompilationUnit(parseInformation.MostRecentCompilationUnit);
		}
		parsings.Remove(fileName);
		lock (lastUpdateHash)
		{
			if (lastUpdateHash.ContainsKey(fileName))
			{
				lastUpdateHash.Remove(fileName);
			}
		}
		OnParseInformationUpdated(new ParseInformationEventArgs(fileName, parseInformation, null));
	}

	public static IExpressionFinder GetExpressionFinder(string fileName)
	{
		return GetParser(fileName)?.CreateExpressionFinder(fileName);
	}

	public static IParser GetParser(string fileName)
	{
		InitializeParserService();
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		IParser parser = null;
		foreach (ParserDescriptor item in ParserService.parser)
		{
			if (item.CanParse(fileName))
			{
				parser = item.Parser;
				break;
			}
		}
		if (parser != null)
		{
			parser.LexerTags = PropertyService.Get("SharpDevelop.TaskListTokens", DefaultTaskListTokens);
		}
		return parser;
	}

	public static ArrayList CtrlSpace(int caretLine, int caretColumn, string fileName, string fileContent, ExpressionContext context)
	{
		return CreateResolver(fileName)?.CtrlSpace(caretLine, caretColumn, fileName, fileContent, context);
	}

	public static IResolver CreateResolver(string fileName)
	{
		return GetParser(fileName)?.CreateResolver();
	}

	public static ResolveResult Resolve(ExpressionResult expressionResult, int caretLineNumber, int caretColumn, string fileName, string fileContent)
	{
		return CreateResolver(fileName)?.Resolve(expressionResult, caretLineNumber, caretColumn, fileName, fileContent);
	}

	private static void OnParseInformationUpdated(ParseInformationEventArgs e)
	{
		if (ParserService.ParseInformationUpdated != null)
		{
			ParserService.ParseInformationUpdated(null, e);
		}
	}

	private static void OnLoadSolutionProjectsThreadEnded(EventArgs e)
	{
		if (ParserService.LoadSolutionProjectsThreadEnded != null)
		{
			ParserService.LoadSolutionProjectsThreadEnded(null, e);
		}
	}

	public static ProjectContentRegistry GetRegistryForReference(ReferenceProjectItem item)
	{
		if (item is ProjectReferenceProjectItem || item.Project == null)
		{
			return defaultProjectContentRegistry;
		}
		foreach (ProjectContentRegistryDescriptor registry2 in registries)
		{
			if (registry2.UseRegistryForProject(item.Project))
			{
				ProjectContentRegistry registry = registry2.Registry;
				if (registry != null)
				{
					return registry;
				}
				return defaultProjectContentRegistry;
			}
		}
		return defaultProjectContentRegistry;
	}

	public static IProjectContent GetExistingProjectContentForReference(ReferenceProjectItem item)
	{
		if (item is ProjectReferenceProjectItem)
		{
			if (((ProjectReferenceProjectItem)item).ReferencedProject == null)
			{
				return null;
			}
			return GetProjectContent(((ProjectReferenceProjectItem)item).ReferencedProject);
		}
		return GetRegistryForReference(item).GetExistingProjectContent(item.Include, item.FileName);
	}

	public static IProjectContent GetProjectContentForReference(ReferenceProjectItem item)
	{
		if (item is ProjectReferenceProjectItem)
		{
			if (((ProjectReferenceProjectItem)item).ReferencedProject == null)
			{
				return null;
			}
			return GetProjectContent(((ProjectReferenceProjectItem)item).ReferencedProject);
		}
		return GetRegistryForReference(item).GetProjectContentForReference(item.Include, item.FileName, item.EvaluatedReferencePath);
	}

	public static void RefreshProjectContentForReference(ReferenceProjectItem item)
	{
		if (item is ProjectReferenceProjectItem)
		{
			return;
		}
		ProjectContentRegistry registry = GetRegistryForReference(item);
		registry.RunLocked(delegate
		{
			IProjectContent existingProjectContentForReference = GetExistingProjectContentForReference(item);
			if (existingProjectContentForReference == null)
			{
				LoggingService.Debug("RefreshProjectContentForReference: not refreshing (rpc==null) " + item.FileName);
			}
			else if (existingProjectContentForReference.IsUpToDate)
			{
				LoggingService.Debug("RefreshProjectContentForReference: not refreshing (rpc.IsUpToDate) " + item.FileName);
			}
			else
			{
				LoggingService.Debug("RefreshProjectContentForReference " + item.FileName);
				HashSet<IProject> hashSet = new HashSet<IProject>();
				HashSet<IProjectContent> unloadedReferenceContents = new HashSet<IProjectContent>();
				UnloadReferencedContent(hashSet, unloadedReferenceContents, registry, existingProjectContentForReference);
				if (!ProjectService.IsLoading)
				{
					foreach (IProject item2 in hashSet)
					{
						Reparse(item2, initReferences: true, parseCode: false);
					}
				}
			}
		});
	}

	private static void UnloadReferencedContent(HashSet<IProject> projectsToRefresh, HashSet<IProjectContent> unloadedReferenceContents, ProjectContentRegistry referencedContentRegistry, IProjectContent referencedContent)
	{
		LoggingService.Debug("Unload referenced content " + referencedContent);
		List<KeyValuePair<ProjectContentRegistry, IProjectContent>> list = new List<KeyValuePair<ProjectContentRegistry, IProjectContent>>();
		foreach (ProjectContentRegistryDescriptor registry in registries)
		{
			if (!registry.IsRegistryLoaded)
			{
				continue;
			}
			foreach (IProjectContent loadedProjectContent in registry.Registry.GetLoadedProjectContents())
			{
				if (loadedProjectContent.ReferencedContents.Contains(referencedContent) && unloadedReferenceContents.Add(loadedProjectContent))
				{
					LoggingService.Debug("Mark dependent content for unloading " + loadedProjectContent);
					list.Add(new KeyValuePair<ProjectContentRegistry, IProjectContent>(registry.Registry, loadedProjectContent));
				}
			}
		}
		foreach (IProjectContent allProjectContent in AllProjectContents)
		{
			IProject project = (IProject)allProjectContent.Project;
			if (!projectsToRefresh.Contains(project) && allProjectContent.ReferencedContents.Remove(referencedContent))
			{
				LoggingService.Debug("UnloadReferencedContent: Mark project for reparsing " + project.Name);
				projectsToRefresh.Add(project);
			}
		}
		foreach (KeyValuePair<ProjectContentRegistry, IProjectContent> item in list)
		{
			UnloadReferencedContent(projectsToRefresh, unloadedReferenceContents, item.Key, item.Value);
		}
		referencedContentRegistry.UnloadProjectContent(referencedContent);
	}
}

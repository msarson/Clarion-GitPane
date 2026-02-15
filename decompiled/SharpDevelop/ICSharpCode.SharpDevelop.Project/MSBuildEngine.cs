using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class MSBuildEngine
{
	internal sealed class BuildRun
	{
		private class DependencyCycleException : Exception
		{
		}

		private Solution solution;

		private IProject project;

		private BuildOptions options;

		private MSBuildEngine parentEngine;

		internal BuildResults currentResults = new BuildResults();

		private List<ProjectToBuild> projectsToBuild = new List<ProjectToBuild>();

		private int workerCount;

		private int maxWorkerCount;

		private Queue<MSBuildEngineWorker> unusedWorkers = new Queue<MSBuildEngineWorker>();

		private int lastUniqueWorkerID;

		private Queue<string> queuedOutputText = new Queue<string>();

		private volatile bool outputLockIsAquired;

		private Dictionary<IProject, ProjectToBuild> parseMSBuildProjectProjectsToBuildDict = new Dictionary<IProject, ProjectToBuild>();

		internal Solution Solution => solution;

		internal BuildOptions BuildOptions => options;

		public BuildRun(Solution solution, IProject project, BuildOptions options, MSBuildEngine parentEngine)
		{
			this.solution = solution;
			this.project = project;
			this.options = options;
			this.parentEngine = parentEngine;
		}

		[STAThread]
		internal void RunMainBuild()
		{
			try
			{
				PrepareBuild();
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
			StartWorkerBuild();
		}

		private void Finish()
		{
			LoggingService.Debug("MSBuild finished");
			isRunning = false;
			if (currentResults.Result == BuildResultCode.None)
			{
				currentResults.Result = BuildResultCode.Success;
			}
			if (ProjectService.CancelSemaphore.WaitOne(1, exitContext: false))
			{
				ProjectService.CancelSemaphore.Release();
			}
			else if (currentResults.Result == BuildResultCode.Success)
			{
				parentEngine.MessageView.AppendLine("${res:MainWindow.CompilerMessages.BuildFinished}");
				StatusBarService.SetMessage("${res:MainWindow.CompilerMessages.BuildFinished}");
			}
			else
			{
				parentEngine.MessageView.AppendLine("${res:MainWindow.CompilerMessages.BuildFailed}");
				StatusBarService.SetMessage("${res:MainWindow.CompilerMessages.BuildFailed}");
			}
			if (options.Callback != null)
			{
				WorkbenchSingleton.MainForm.BeginInvoke(options.Callback, currentResults);
			}
		}

		private void PrepareBuild()
		{
			parentEngine.MessageView.AppendLine("${res:MainWindow.CompilerMessages.BuildStarted}");
			if (this.project == null)
			{
				LoggingService.Debug("Parsing solution file " + solution.FileName);
				Engine engine = CreateEngine();
				if (parentEngine.Configuration != null)
				{
					engine.GlobalProperties.SetProperty("Configuration", parentEngine.Configuration);
				}
				if (parentEngine.Platform != null)
				{
					engine.GlobalProperties.SetProperty("Platform", parentEngine.Platform);
				}
				Microsoft.Build.BuildEngine.Project project = LoadProject(engine, solution.FileName);
				if (project == null)
				{
					Finish();
					return;
				}
				if (!ParseSolution(project))
				{
					Finish();
					return;
				}
			}
			else if (ParseMSBuildProject(this.project) == null)
			{
				Finish();
				return;
			}
			SortProjectsToBuild();
		}

		private void StartWorkerBuild()
		{
			workerCount = 1;
			maxWorkerCount = ((!PropertyService.Get("SharpDevelop.BuildAsProcess", defaultValue: true)) ? 1 : PropertyService.Get("SharpDevelop.BuildProcesses", 1));
			RunWorkerBuild();
		}

		[STAThread]
		private void RunWorkerBuild()
		{
			LoggingService.Debug("Build Worker thread started");
			MSBuildEngineWorker mSBuildEngineWorker = null;
			try
			{
				lock (projectsToBuild)
				{
					if (unusedWorkers.Count > 0)
					{
						mSBuildEngineWorker = unusedWorkers.Dequeue();
					}
				}
				if (mSBuildEngineWorker == null)
				{
					mSBuildEngineWorker = new MSBuildEngineWorker(parentEngine, this);
				}
				while (RunWorkerInternal(mSBuildEngineWorker))
				{
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
			finally
			{
				bool flag;
				lock (projectsToBuild)
				{
					workerCount--;
					flag = workerCount == 0;
					if (mSBuildEngineWorker != null)
					{
						unusedWorkers.Enqueue(mSBuildEngineWorker);
					}
				}
				LoggingService.Debug("Build Worker thread finished");
				if (flag)
				{
					Finish();
				}
			}
		}

		private bool RunWorkerInternal(MSBuildEngineWorker worker)
		{
			ProjectToBuild projectToBuild = null;
			lock (projectsToBuild)
			{
				foreach (ProjectToBuild item in projectsToBuild)
				{
					if (!item.buildStarted && item.DependenciesSatisfied())
					{
						if (projectToBuild != null)
						{
							LoggingService.Debug("Starting a new worker");
							workerCount++;
							Thread thread = new Thread(RunWorkerBuild);
							thread.Name = "MSBuildEngine worker " + ++lastUniqueWorkerID;
							thread.SetApartmentState(ApartmentState.STA);
							thread.Start();
							break;
						}
						projectToBuild = item;
						if (workerCount == maxWorkerCount)
						{
							break;
						}
					}
				}
				if (projectToBuild == null)
				{
					return false;
				}
				projectToBuild.buildStarted = true;
			}
			StatusBarService.SetMessage("${res:MainWindow.CompilerMessages.BuildVerb} " + Path.GetFileNameWithoutExtension(projectToBuild.file) + "...");
			if (worker.Build(projectToBuild))
			{
				lock (projectsToBuild)
				{
					projectToBuild.buildFinished = true;
				}
			}
			return true;
		}

		internal bool TryAquireOutputLock()
		{
			lock (queuedOutputText)
			{
				if (outputLockIsAquired)
				{
					return false;
				}
				outputLockIsAquired = true;
				return true;
			}
		}

		internal void ReleaseOutputLock()
		{
			lock (queuedOutputText)
			{
				outputLockIsAquired = false;
				while (queuedOutputText.Count > 0)
				{
					parentEngine.MessageView.AppendText(queuedOutputText.Dequeue());
				}
			}
		}

		internal void EnqueueTextForAppendWhenOutputLockIsReleased(string text)
		{
			lock (queuedOutputText)
			{
				if (outputLockIsAquired)
				{
					queuedOutputText.Enqueue(text);
				}
				else
				{
					parentEngine.MessageView.AppendText(text);
				}
			}
		}

		internal Engine CreateEngine()
		{
			Engine engine = MSBuildInternals.CreateEngine();
			MSBuildBasedProject.InitializeMSBuildProjectProperties(engine.GlobalProperties);
			if (options.AdditionalProperties != null)
			{
				foreach (KeyValuePair<string, string> additionalProperty in options.AdditionalProperties)
				{
					engine.GlobalProperties.SetProperty(additionalProperty.Key, additionalProperty.Value);
				}
			}
			engine.GlobalProperties.SetProperty("SolutionDir", EnsureBackslash(solution.Directory));
			engine.GlobalProperties.SetProperty("SolutionExt", ".sln");
			engine.GlobalProperties.SetProperty("SolutionFileName", Path.GetFileName(solution.FileName));
			engine.GlobalProperties.SetProperty("SolutionPath", solution.FileName);
			return engine;
		}

		private static string EnsureBackslash(string path)
		{
			if (path.EndsWith("\\"))
			{
				return path;
			}
			return path + "\\";
		}

		internal Microsoft.Build.BuildEngine.Project LoadProject(Engine engine, string fileName)
		{
			Microsoft.Build.BuildEngine.Project project = engine.CreateNewProject();
			try
			{
				project.Load(fileName);
				project.Targets.AddNewTarget("_ComputeNonExistentFileProperty");
				return project;
			}
			catch (ArgumentException ex)
			{
				currentResults.Result = BuildResultCode.BuildFileError;
				currentResults.Add(new BuildError("", ex.Message));
			}
			catch (InvalidProjectFileException ex2)
			{
				currentResults.Result = BuildResultCode.BuildFileError;
				currentResults.Add(new BuildError(ex2.ProjectFile, ex2.LineNumber, ex2.ColumnNumber, ex2.ErrorCode, ex2.Message));
			}
			return null;
		}

		private bool ParseSolution(Microsoft.Build.BuildEngine.Project solution)
		{
			List<Target> list = new List<Target>();
			string targetName = options.Target.TargetName;
			if (targetName == "Build")
			{
				foreach (Target target3 in solution.Targets)
				{
					string name = target3.Name;
					if (name.Contains(":") || !(name != "Clean") || !(name != "Rebuild") || !(name != "Publish") || !(name != "Build"))
					{
						continue;
					}
					List<BuildTask> list2 = Linq.ToList(Linq.CastTo<BuildTask>(target3));
					foreach (BuildTask item in list2)
					{
						if (item.Name == "MSBuild")
						{
							list.Add(target3);
							break;
						}
					}
				}
			}
			else
			{
				foreach (Target target4 in solution.Targets)
				{
					if (target4.Name.EndsWith(":" + targetName))
					{
						list.Add(target4);
					}
				}
			}
			Dictionary<string, ProjectToBuild> dictionary = new Dictionary<string, ProjectToBuild>();
			foreach (Target item2 in list)
			{
				List<BuildTask> list3 = Linq.ToList(Linq.CastTo<BuildTask>(item2));
				BuildTask buildTask = null;
				if (list3.Count != 0)
				{
					foreach (BuildTask item3 in list3)
					{
						if (item3.Name == "MSBuild" && MSBuildInternals.EvaluateCondition(solution, item3.Condition))
						{
							buildTask = item3;
						}
					}
					if (buildTask == null)
					{
						LoggingService.Warn("No matching condition for solution target " + item2.Name);
					}
				}
				if (buildTask == null)
				{
					continue;
				}
				string path = Path.Combine(this.solution.Directory, buildTask.GetParameterValue("Projects"));
				ProjectToBuild projectToBuild = new ProjectToBuild(Path.GetFullPath(path), buildTask.GetParameterValue("Targets"));
				string parameterValue = buildTask.GetParameterValue("Properties");
				Match match = Regex.Match(parameterValue, "\\bConfiguration=([^;]+);");
				if (match.Success)
				{
					projectToBuild.configuration = match.Groups[1].Value;
				}
				else
				{
					projectToBuild.configuration = parentEngine.Configuration;
				}
				match = Regex.Match(parameterValue, "\\bPlatform=([^;]+);");
				if (match.Success)
				{
					projectToBuild.platform = match.Groups[1].Value;
				}
				else
				{
					projectToBuild.platform = parentEngine.Platform;
					if (projectToBuild.platform == "Any CPU")
					{
						projectToBuild.platform = "AnyCPU";
					}
				}
				projectsToBuild.Add(projectToBuild);
				dictionary[item2.Name] = projectToBuild;
			}
			foreach (Target item4 in list)
			{
				if (!dictionary.TryGetValue(item4.Name, out var value))
				{
					continue;
				}
				string[] array = item4.DependsOnTargets.Split(';');
				foreach (string key in array)
				{
					if (dictionary.TryGetValue(key, out var value2))
					{
						value.dependencies.Add(value2);
					}
				}
			}
			return true;
		}

		private bool InvalidTarget(Target target)
		{
			currentResults.Result = BuildResultCode.BuildFileError;
			currentResults.Add(new BuildError(solution.FileName, "Solution target '" + target.Name + "' is invalid."));
			return false;
		}

		private ProjectToBuild ParseMSBuildProject(IProject project)
		{
			if (parseMSBuildProjectProjectsToBuildDict.TryGetValue(project, out var value))
			{
				return value;
			}
			value = new ProjectToBuild(project.FileName, options.Target.TargetName);
			value.configuration = parentEngine.Configuration;
			value.platform = parentEngine.Platform;
			projectsToBuild.Add(value);
			parseMSBuildProjectProjectsToBuildDict[project] = value;
			bool flag = true;
			if (options != null && options.AdditionalProperties != null && options.AdditionalProperties.ContainsKey("NoDependency") && !string.IsNullOrEmpty(options.AdditionalProperties["NoDependency"]))
			{
				flag = false;
			}
			if (flag)
			{
				foreach (ProjectItem item in project.GetItemsOfType(ItemType.ProjectReference))
				{
					if (item is ProjectReferenceProjectItem { ReferencedProject: not null } projectReferenceProjectItem)
					{
						ProjectToBuild projectToBuild = ParseMSBuildProject(projectReferenceProjectItem.ReferencedProject);
						if (projectToBuild == null)
						{
							return null;
						}
						value.dependencies.Add(projectToBuild);
					}
				}
			}
			return value;
		}

		private void SortProjectsToBuild()
		{
			try
			{
				foreach (ProjectToBuild item in projectsToBuild)
				{
					projectsToBuild.ForEach(delegate(ProjectToBuild p)
					{
						p.visitFlag = 0;
					});
					item.dependencies.ForEach(IncrementRequiredByCount);
				}
			}
			catch (DependencyCycleException)
			{
				currentResults.Add(new BuildError(null, "Dependency cycle detected, cannot build!"));
				return;
			}
			projectsToBuild.Sort((ProjectToBuild a, ProjectToBuild b) => -a.requiredByCount.CompareTo(b.requiredByCount));
		}

		private static void IncrementRequiredByCount(ProjectToBuild ptb)
		{
			if (ptb.visitFlag != 1)
			{
				if (ptb.visitFlag == -1)
				{
					throw new DependencyCycleException();
				}
				ptb.visitFlag = -1;
				ptb.requiredByCount++;
				ptb.dependencies.ForEach(IncrementRequiredByCount);
				ptb.visitFlag = 1;
			}
		}
	}

	internal class ProjectToBuild
	{
		internal string file;

		internal string targets;

		internal string configuration;

		internal string platform;

		internal List<ProjectToBuild> dependencies = new List<ProjectToBuild>();

		internal int requiredByCount;

		internal int visitFlag;

		internal bool buildStarted;

		internal bool buildFinished;

		internal bool DependenciesSatisfied()
		{
			return dependencies.TrueForAll((ProjectToBuild p) => p.buildFinished);
		}

		public ProjectToBuild(string file, string targets)
		{
			this.file = file;
			this.targets = targets;
		}
	}

	private const string CompileTaskNamesPath = "/SharpDevelop/MSBuildEngine/CompileTaskNames";

	private const string AdditionalTargetFilesPath = "/SharpDevelop/MSBuildEngine/AdditionalTargetFiles";

	private const string AdditionalLoggersPath = "/SharpDevelop/MSBuildEngine/AdditionalLoggers";

	internal const string AdditionalPropertiesPath = "/SharpDevelop/MSBuildEngine/AdditionalProperties";

	public static readonly ICollection<string> CompileTaskNames;

	public static readonly SortedList<string, string> MSBuildProperties;

	public static readonly List<string> AdditionalTargetFiles;

	public static readonly List<IMSBuildAdditionalLogger> AdditionalMSBuildLoggers;

	private MessageViewCategory messageView;

	private string configuration;

	private string platform;

	private static volatile bool isRunning;

	public MessageViewCategory MessageView
	{
		get
		{
			return messageView;
		}
		set
		{
			messageView = value;
		}
	}

	public string Configuration
	{
		get
		{
			return configuration;
		}
		set
		{
			configuration = value;
		}
	}

	public string Platform
	{
		get
		{
			return platform;
		}
		set
		{
			platform = value;
		}
	}

	static MSBuildEngine()
	{
		isRunning = false;
		CompileTaskNames = new Set<string>(AddInTree.BuildItems<string>("/SharpDevelop/MSBuildEngine/CompileTaskNames", null, throwOnNotFound: false), StringComparer.OrdinalIgnoreCase);
		AdditionalTargetFiles = AddInTree.BuildItems<string>("/SharpDevelop/MSBuildEngine/AdditionalTargetFiles", null, throwOnNotFound: false);
		AdditionalMSBuildLoggers = AddInTree.BuildItems<IMSBuildAdditionalLogger>("/SharpDevelop/MSBuildEngine/AdditionalLoggers", null, throwOnNotFound: false);
		MSBuildProperties = new SortedList<string, string>();
		MSBuildProperties.Add("SharpDevelopBinPath", Path.GetDirectoryName(typeof(MSBuildEngine).Assembly.Location));
		MSBuildProperties.Add("BuildingInsideVisualStudio", "true");
	}

	public void Run(Solution solution, IProject project, BuildOptions options)
	{
		if (isRunning)
		{
			BuildResults buildResults = new BuildResults();
			buildResults.Result = BuildResultCode.MSBuildAlreadyRunning;
			buildResults.Add(new BuildError(null, ResourceService.GetString("MainWindow.CompilerMessages.MSBuildAlreadyRunning")));
			if (options.Callback != null)
			{
				options.Callback(buildResults);
			}
		}
		else
		{
			isRunning = true;
			Thread thread = new Thread(new BuildRun(solution, project, options, this).RunMainBuild);
			thread.Name = "MSBuildEngine main worker";
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}
	}
}

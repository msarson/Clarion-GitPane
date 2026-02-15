using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Commands;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public abstract class AbstractBuildMenuCommand : AbstractMenuCommand
{
	private BuildResults lastBuildResults;

	private bool _KillRunningTarget;

	private bool _QueryBeforeKillRunningTarget;

	public virtual bool CanRunBuild => ProjectService.OpenSolution != null;

	public BuildResults LastBuildResults => lastBuildResults;

	public bool KillRunningTarget
	{
		get
		{
			return _KillRunningTarget;
		}
		internal set
		{
			_KillRunningTarget = value;
		}
	}

	public bool QueryBeforeKillRunningTarget
	{
		get
		{
			return _QueryBeforeKillRunningTarget;
		}
		internal set
		{
			_QueryBeforeKillRunningTarget = value;
		}
	}

	protected bool supportKillRunningTarget
	{
		get
		{
			if (VersionService.Version != IDEVersion.Enterprise)
			{
				return VersionService.Version == IDEVersion.Standard;
			}
			return true;
		}
	}

	public event EventHandler BuildComplete;

	public event EventHandler BuildFinish;

	public virtual void BeforeBuild()
	{
		TaskService.BuildMessageViewCategory.ClearText();
		TaskService.InUpdate = true;
		TaskService.ClearExceptCommentTasks();
		TaskService.InUpdate = false;
		if (supportKillRunningTarget)
		{
			KillRunningTarget = PropertyService.Get("SharpDevelop.KillProcessBeforeBuild", defaultValue: false);
			QueryBeforeKillRunningTarget = PropertyService.Get("SharpDevelop.QueryBeforeKillRunningTarget", defaultValue: true);
		}
		else
		{
			KillRunningTarget = false;
			QueryBeforeKillRunningTarget = false;
		}
		if (PropertyService.Get("SharpDevelop.KillCCCEXEBeforeBuild", defaultValue: true) && ProjectService.IsProcessRuning("ccc.exe") && MessageService.AskQuestion("${res:AbstractBuildMenuCommand.CccEXE.AskQuestion}", "${res:AbstractBuildMenuCommand.CccEXE.AskQuestionTitle}", defaultToYes: true) && ProjectService.KillRunningProcess("ccc.exe"))
		{
			LoggingService.Info("The Catalyst Control Center Utility Process (ccc.exe) was stopped.");
		}
		SaveAllFiles.SaveAll();
		ProjectService.SaveSolution();
	}

	public virtual void AfterBuild()
	{
	}

	public virtual void BuildFinished()
	{
		ProjectService.RaiseEventBuildFinished();
	}

	public override void Run()
	{
		if (CanRunBuild)
		{
			if (DebuggerService.IsDebuggerLoaded && DebuggerService.CurrentDebugger.IsDebugging)
			{
				if (!MessageService.AskQuestion("${res:XML.MainMenu.RunMenu.Compile.StopDebuggingQuestion}", "${res:XML.MainMenu.RunMenu.Compile.StopDebuggingTitle}"))
				{
					return;
				}
				DebuggerService.CurrentDebugger.Stop();
			}
			BeforeBuild();
			StartBuild();
		}
		else
		{
			AddNoSingleFileCompilationError();
		}
	}

	protected void CallbackMethod(BuildResults results)
	{
		lastBuildResults = results;
		ShowResults(results);
		AfterBuild();
		if (this.BuildComplete != null)
		{
			this.BuildComplete(this, EventArgs.Empty);
		}
		BuildFinished();
		if (this.BuildFinish != null)
		{
			this.BuildFinish(this, EventArgs.Empty);
		}
	}

	protected void StartBuild(BuildTarget target)
	{
		BuildOptions options = new BuildOptions(target, CallbackMethod);
		ProjectService.RaiseEventStartBuild(options);
		if (supportKillRunningTarget && KillRunningTarget && ProjectService.OpenSolutionHasTargetRunning() && (!QueryBeforeKillRunningTarget || (QueryBeforeKillRunningTarget && MessageService.AskQuestion("${res:AbstractBuildMenuCommand.KillSolutionRunningTargets.AskQuestion}", "${res:AbstractBuildMenuCommand.KillSolutionRunningTargets.AskQuestionTitle}", defaultToYes: true))))
		{
			ProjectService.KillSolutionRunningTargets();
		}
		ProjectService.OpenSolution.StartBuild(options);
	}

	public abstract void StartBuild();

	public static void ShowResults(BuildResults results)
	{
		if (results == null)
		{
			return;
		}
		TaskService.InUpdate = true;
		bool flag = PropertyService.Get("SharpDevelop.SilentReadOnlyWarnings", defaultValue: false);
		foreach (BuildError error in results.Errors)
		{
			if ((!error.ErrorText.Contains("System.UnauthorizedAccessException") && !error.ErrorText.Contains("was not copied to the target directory because Access to the path")) || !flag)
			{
				TaskService.Add(Task.NewTask(error));
			}
		}
		TaskService.InUpdate = false;
		if (results.Errors.Count > 0)
		{
			ErrorListPad.ShowAfterBuild();
		}
	}

	public static void AddNoSingleFileCompilationError()
	{
		TaskService.Add(new Task(null, StringParser.Parse("${res:BackendBindings.ExecutionManager.NoSingleFileCompilation}"), 0, 0, TaskType.Error));
		WorkbenchSingleton.Workbench.GetPad(typeof(ErrorListPad)).BringPadToFront();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			lastBuildResults = null;
		}
		base.Dispose(disposing);
	}
}

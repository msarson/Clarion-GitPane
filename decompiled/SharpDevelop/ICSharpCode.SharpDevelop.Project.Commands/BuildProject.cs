using System.Collections.Generic;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class BuildProject : AbstractProjectBuildMenuCommand
{
	private IDictionary<string, string> additionalProperties = new SortedList<string, string>();

	public IDictionary<string, string> AdditionalProperties => additionalProperties;

	public BuildProject()
	{
	}

	public BuildProject(IProject targetProject)
	{
		base.targetProject = targetProject;
	}

	protected new void StartBuild(BuildTarget target)
	{
		if (base.supportKillRunningTarget && base.KillRunningTarget && ProjectService.OpenSolutionHasTargetRunning() && (!base.QueryBeforeKillRunningTarget || (base.QueryBeforeKillRunningTarget && MessageService.AskQuestion("${res:AbstractBuildMenuCommand.KillSolutionRunningTargets.AskQuestion}", "${res:AbstractBuildMenuCommand.KillSolutionRunningTargets.AskQuestionTitle}", defaultToYes: true))))
		{
			ProjectService.KillRunningTarget(base.ProjectToBuild);
		}
		BuildOptions options = new BuildOptions(target, base.CallbackMethod, AdditionalProperties);
		ProjectService.RaiseEventStartBuild(options);
		base.ProjectToBuild.StartBuild(options);
	}

	public override void StartBuild()
	{
		StartBuild(BuildTarget.Build);
	}

	public override void AfterBuild()
	{
		ProjectService.RaiseEventEndBuild();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && additionalProperties != null)
		{
			additionalProperties.Clear();
			additionalProperties = null;
		}
		base.Dispose(disposing);
	}
}

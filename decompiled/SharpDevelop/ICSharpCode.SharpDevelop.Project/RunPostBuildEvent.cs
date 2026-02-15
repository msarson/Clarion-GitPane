using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.Project;

public enum RunPostBuildEvent
{
	[Description("${res:Dialog.ProjectOptions.RunPostBuildEvent.OnSuccessfulBuild}")]
	OnBuildSuccess,
	[Description("${res:Dialog.ProjectOptions.RunPostBuildEvent.Always}")]
	Always,
	[Description("${res:Dialog.ProjectOptions.RunPostBuildEvent.OnOutputUpdated}")]
	OnOutputUpdated
}

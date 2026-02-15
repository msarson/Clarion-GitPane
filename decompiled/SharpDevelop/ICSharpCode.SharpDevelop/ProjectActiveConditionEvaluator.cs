using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class ProjectActiveConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		string text = condition.Properties["activeproject"];
		IProject project = (caller as IProject) ?? ProjectService.CurrentProject;
		if (text == "*")
		{
			if (project != null)
			{
				return project.ProjectType == "DotNET";
			}
			return false;
		}
		if (project != null)
		{
			return project.Language == text;
		}
		return false;
	}
}

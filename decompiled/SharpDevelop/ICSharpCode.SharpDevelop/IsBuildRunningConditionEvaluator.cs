using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class IsBuildRunningConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		string value = condition.Properties.Get("isbuildrunning", bool.TrueString);
		bool isBuilding = ProjectService.IsBuilding;
		return isBuilding == bool.Parse(value);
	}
}

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class SolutionOpenConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		return ProjectService.OpenSolution != null;
	}
}

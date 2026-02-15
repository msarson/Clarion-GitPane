using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class CanNavigateForwardConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		return NavigationService.CanNavigateForwards;
	}
}

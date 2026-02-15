using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class CanNavigateBackConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (!NavigationService.CanNavigateBack)
		{
			return NavigationService.CanNavigateForwards;
		}
		return true;
	}
}

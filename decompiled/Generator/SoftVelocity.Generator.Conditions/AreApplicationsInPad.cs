using ICSharpCode.Core;

namespace SoftVelocity.Generator.Conditions;

public class AreApplicationsInPad : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (ApplicationService.ApplicationsList.Count > 0)
		{
			return true;
		}
		return false;
	}
}

using ICSharpCode.Core;

namespace SoftVelocity.Generator.Conditions;

public class CanOpenTemplateRegistry : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (!ApplicationService.IsTemplateRegistryOpen && !ApplicationService.AreApplicationOnEdit)
		{
			return true;
		}
		return false;
	}
}

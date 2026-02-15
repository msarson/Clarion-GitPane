using ICSharpCode.Core;

namespace SoftVelocity.Generator.Conditions;

public class IsTemplateRegistryOpen : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		return ApplicationService.IsTemplateRegistryOpen;
	}
}

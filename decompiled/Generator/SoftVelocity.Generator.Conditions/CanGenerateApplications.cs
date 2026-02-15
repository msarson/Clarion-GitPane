using ICSharpCode.Core;

namespace SoftVelocity.Generator.Conditions;

public class CanGenerateApplications : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (!ApplicationService.Instance.Generating && !ApplicationService.IsTemplateRegistryOpen && ApplicationService.ApplicationsList.Count > 0)
		{
			return true;
		}
		return false;
	}
}

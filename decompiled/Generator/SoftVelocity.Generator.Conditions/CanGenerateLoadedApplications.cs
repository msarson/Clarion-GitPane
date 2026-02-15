using ICSharpCode.Core;

namespace SoftVelocity.Generator.Conditions;

public class CanGenerateLoadedApplications : IConditionEvaluator
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

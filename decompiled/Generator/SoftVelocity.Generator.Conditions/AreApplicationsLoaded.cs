using ICSharpCode.Core;

namespace SoftVelocity.Generator.Conditions;

public class AreApplicationsLoaded : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (ApplicationService.ApplicationsLoaded.Count > 0)
		{
			if (ApplicationServiceSettings.CacheApplicationAfterEdit && !ApplicationService.AreApplicationOnEdit)
			{
				return false;
			}
			return true;
		}
		return false;
	}
}

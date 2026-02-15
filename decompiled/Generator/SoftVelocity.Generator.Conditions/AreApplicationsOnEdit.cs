using ICSharpCode.Core;

namespace SoftVelocity.Generator.Conditions;

public class AreApplicationsOnEdit : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		return ApplicationService.AreApplicationOnEdit;
	}
}

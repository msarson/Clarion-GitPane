using ICSharpCode.Core;

namespace SoftVelocity.Generator.Conditions;

public class GeneratingApplication : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		string value = condition.Properties.Get<string>("isgenerationrunning", bool.TrueString);
		bool generating = ApplicationService.Instance.Generating;
		return generating == bool.Parse(value);
	}
}

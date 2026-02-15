using System;

namespace ICSharpCode.Core;

public class CompareConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		string value = condition.Properties["comparisonType"];
		return string.Equals(comparisonType: (!string.IsNullOrEmpty(value)) ? ((StringComparison)Enum.Parse(typeof(StringComparison), value)) : StringComparison.InvariantCultureIgnoreCase, a: StringParser.Parse(condition.Properties["string"]), b: StringParser.Parse(condition.Properties["equals"]));
	}
}

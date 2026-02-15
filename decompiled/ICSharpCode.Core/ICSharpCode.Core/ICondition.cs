namespace ICSharpCode.Core;

public interface ICondition
{
	string Name { get; }

	ConditionFailedAction Action { get; set; }

	bool IsValid(object caller);
}

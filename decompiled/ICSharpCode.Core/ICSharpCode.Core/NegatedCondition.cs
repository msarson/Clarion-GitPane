using System.Xml;

namespace ICSharpCode.Core;

public class NegatedCondition : ICondition
{
	private ICondition condition;

	private ConditionFailedAction action = ConditionFailedAction.Exclude;

	public string Name => "Not " + condition.Name;

	public ConditionFailedAction Action
	{
		get
		{
			return action;
		}
		set
		{
			action = value;
		}
	}

	public NegatedCondition(ICondition condition)
	{
		this.condition = condition;
	}

	public bool IsValid(object owner)
	{
		return !condition.IsValid(owner);
	}

	public static ICondition Read(XmlReader reader)
	{
		return new NegatedCondition(Condition.ReadConditionList(reader, "Not")[0]);
	}
}

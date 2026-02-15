using System.Text;
using System.Xml;

namespace ICSharpCode.Core;

public class OrCondition : ICondition
{
	private ICondition[] conditions;

	private ConditionFailedAction action = ConditionFailedAction.Exclude;

	public string Name
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < conditions.Length; i++)
			{
				stringBuilder.Append(conditions[i].Name);
				if (i + 1 < conditions.Length)
				{
					stringBuilder.Append(" Or ");
				}
			}
			return stringBuilder.ToString();
		}
	}

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

	public OrCondition(ICondition[] conditions)
	{
		this.conditions = conditions;
	}

	public bool IsValid(object owner)
	{
		ICondition[] array = conditions;
		foreach (ICondition condition in array)
		{
			if (condition.IsValid(owner))
			{
				return true;
			}
		}
		return false;
	}

	public static ICondition Read(XmlReader reader)
	{
		return new OrCondition(Condition.ReadConditionList(reader, "Or"));
	}
}

using System.Collections.Generic;
using System.Xml;

namespace ICSharpCode.Core;

public class Condition : ICondition
{
	private string name;

	private Properties properties;

	private ConditionFailedAction action;

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

	public string Name => name;

	public string this[string key] => properties[key];

	public Properties Properties => properties;

	public Condition(string name, Properties properties)
	{
		this.name = name;
		this.properties = properties;
		action = properties.Get("action", ConditionFailedAction.Exclude);
	}

	public bool IsValid(object caller)
	{
		try
		{
			return AddInTree.ConditionEvaluators[name].IsValid(caller, this);
		}
		catch (KeyNotFoundException)
		{
			throw new CoreException("Condition evaluator " + name + " not found!");
		}
	}

	public static ICondition Read(XmlReader reader)
	{
		Properties properties = Properties.ReadFromAttributes(reader);
		string text = properties["name"];
		return new Condition(text, properties);
	}

	public static ICondition ReadComplexCondition(XmlReader reader)
	{
		Properties properties = Properties.ReadFromAttributes(reader);
		reader.Read();
		ICondition condition = null;
		while (reader.Read())
		{
			XmlNodeType nodeType = reader.NodeType;
			if (nodeType == XmlNodeType.Element)
			{
				condition = reader.LocalName switch
				{
					"And" => AndCondition.Read(reader), 
					"Or" => OrCondition.Read(reader), 
					"Not" => NegatedCondition.Read(reader), 
					_ => throw new AddInLoadException("Invalid element name '" + reader.LocalName + "', the first entry in a ComplexCondition must be <And>, <Or> or <Not>"), 
				};
				break;
			}
		}
		if (condition != null)
		{
			ConditionFailedAction conditionFailedAction = properties.Get("action", ConditionFailedAction.Exclude);
			condition.Action = conditionFailedAction;
		}
		return condition;
	}

	public static ICondition[] ReadConditionList(XmlReader reader, string endElement)
	{
		List<ICondition> list = new List<ICondition>();
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.EndElement:
				if (reader.LocalName == endElement)
				{
					return list.ToArray();
				}
				break;
			case XmlNodeType.Element:
				switch (reader.LocalName)
				{
				case "And":
					list.Add(AndCondition.Read(reader));
					break;
				case "Or":
					list.Add(OrCondition.Read(reader));
					break;
				case "Not":
					list.Add(NegatedCondition.Read(reader));
					break;
				case "Condition":
					list.Add(Read(reader));
					break;
				default:
					throw new AddInLoadException("Invalid element name '" + reader.LocalName + "', entries in a <" + endElement + "> must be <And>, <Or>, <Not> or <Condition>");
				}
				break;
			}
		}
		return list.ToArray();
	}

	public static ConditionFailedAction GetFailedAction(IEnumerable<ICondition> conditionList, object caller)
	{
		ConditionFailedAction result = ConditionFailedAction.Nothing;
		foreach (ICondition condition in conditionList)
		{
			if (!condition.IsValid(caller))
			{
				if (condition.Action != ConditionFailedAction.Disable)
				{
					return ConditionFailedAction.Exclude;
				}
				result = ConditionFailedAction.Disable;
			}
		}
		return result;
	}
}

using System.Collections;

namespace ICSharpCode.Core;

public class Codon
{
	private AddIn addIn;

	private string name;

	private string parentName;

	private Properties properties;

	private ICondition[] conditions;

	public string Name => name;

	public string ParentName => parentName;

	public string UniqueId => parentName + '/' + Id;

	public string ShortcutId
	{
		get
		{
			if (MenuShortcutService.UseFullName)
			{
				return parentName + '/' + Id;
			}
			return Id.Trim();
		}
	}

	public AddIn AddIn => addIn;

	public string Id => properties["id"];

	public string InsertAfter
	{
		get
		{
			if (!properties.Contains("insertafter"))
			{
				return "";
			}
			return properties["insertafter"];
		}
		set
		{
			properties["insertafter"] = value;
		}
	}

	public string InsertBefore
	{
		get
		{
			if (!properties.Contains("insertbefore"))
			{
				return "";
			}
			return properties["insertbefore"];
		}
		set
		{
			properties["insertbefore"] = value;
		}
	}

	public string this[string key] => properties[key];

	public Properties Properties => properties;

	public ICondition[] Conditions => conditions;

	public Codon(AddIn addIn, string parentName, string name, Properties properties, ICondition[] conditions)
	{
		this.addIn = addIn;
		this.name = name;
		this.parentName = parentName;
		this.properties = properties;
		this.conditions = conditions;
	}

	public ConditionFailedAction GetFailedAction(object caller)
	{
		return Condition.GetFailedAction(conditions, caller);
	}

	public object BuildItem(object owner, ArrayList subItems)
	{
		if (!AddInTree.Doozers.TryGetValue(Name, out var value))
		{
			throw new CoreException("Doozer " + Name + " not found!");
		}
		if (!value.HandleConditions && conditions.Length > 0 && GetFailedAction(owner) != ConditionFailedAction.Nothing)
		{
			return null;
		}
		return value.BuildItem(owner, this, subItems);
	}

	public override string ToString()
	{
		return $"[Codon: name = {name}, addIn={addIn.FileName}]";
	}
}

using System;
using System.Reflection;

namespace SoftVelocity.Common;

public class HistorySetValueCommand : HistoryCommand
{
	private string _PropertyName;

	private object _Value;

	private object _OldValue;

	public string PropertyName
	{
		get
		{
			return _PropertyName;
		}
		set
		{
			_PropertyName = value;
		}
	}

	public object Value
	{
		get
		{
			return _Value;
		}
		set
		{
			_Value = value;
		}
	}

	public HistorySetValueCommand(object target, string propName, object newValue)
		: base(target)
	{
		_PropertyName = propName;
		_Value = newValue;
		Type type = base.Target.GetType();
		PropertyInfo property = type.GetProperty(PropertyName);
		_OldValue = property.GetValue(base.Target, null);
	}

	public HistorySetValueCommand(object target, string propName, object oldValue, object newValue)
		: this(target, propName, newValue)
	{
		_OldValue = oldValue;
	}

	protected override void Cleaning()
	{
		_PropertyName = null;
		_Value = null;
		_OldValue = null;
		base.Cleaning();
	}

	protected override void DoExecute()
	{
		Type type = base.Target.GetType();
		PropertyInfo property = type.GetProperty(PropertyName);
		_OldValue = property.GetValue(base.Target, null);
		property.SetValue(base.Target, _Value, null);
	}

	protected override void DoUnExecute()
	{
		Type type = base.Target.GetType();
		PropertyInfo property = type.GetProperty(PropertyName);
		object value = property.GetValue(base.Target, null);
		property.SetValue(base.Target, _OldValue, null);
		_OldValue = value;
	}
}

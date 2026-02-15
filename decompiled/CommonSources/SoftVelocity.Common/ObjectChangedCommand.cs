using System.Collections.Generic;
using System.Reflection;

namespace SoftVelocity.Common;

public class ObjectChangedCommand : HistoryCommand
{
	private List<HistorySetValueCommand> changedValues = new List<HistorySetValueCommand>();

	public ObjectChangedCommand(object target, object original)
		: this(target, target, original)
	{
	}

	public ObjectChangedCommand(object target, object oldValues, object newValues)
		: base(target)
	{
		PropertyInfo[] properties = target.GetType().GetProperties();
		PropertyInfo[] array = properties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!propertyInfo.CanWrite || !propertyInfo.CanRead)
			{
				continue;
			}
			try
			{
				object value = propertyInfo.GetValue(oldValues, null);
				object value2 = propertyInfo.GetValue(newValues, null);
				if ((value != null && value2 == null) || (value2 != null && value == null) || value2.ToString() != value.ToString())
				{
					changedValues.Add(new HistorySetValueCommand(target, propertyInfo.Name, value, value2));
				}
			}
			catch
			{
			}
		}
	}

	protected override void Cleaning()
	{
		foreach (HistorySetValueCommand changedValue in changedValues)
		{
			changedValue.Clean();
		}
		changedValues.Clear();
		changedValues = null;
		base.Cleaning();
	}

	protected override void DoExecute()
	{
		foreach (HistorySetValueCommand changedValue in changedValues)
		{
			changedValue.UnExecute();
		}
	}

	protected override void DoUnExecute()
	{
		foreach (HistorySetValueCommand changedValue in changedValues)
		{
			changedValue.Execute();
		}
	}
}

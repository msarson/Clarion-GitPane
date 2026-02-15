using System;

namespace ICSharpCode.Core;

public class PropertyChangedEventArgs : EventArgs
{
	private Properties properties;

	private string key;

	private object newValue;

	private object oldValue;

	public Properties Properties => properties;

	public string Key => key;

	public object NewValue => newValue;

	public object OldValue => oldValue;

	public PropertyChangedEventArgs(Properties properties, string key, object oldValue, object newValue)
	{
		this.properties = properties;
		this.key = key;
		this.oldValue = oldValue;
		this.newValue = newValue;
	}
}

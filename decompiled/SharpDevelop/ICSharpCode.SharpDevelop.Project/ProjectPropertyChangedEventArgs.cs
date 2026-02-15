using System;

namespace ICSharpCode.SharpDevelop.Project;

public class ProjectPropertyChangedEventArgs : EventArgs
{
	private string propertyName;

	private string configuration;

	private string platform;

	private string oldValue;

	private string newValue;

	private PropertyStorageLocations newLocation;

	private PropertyStorageLocations oldLocation;

	public string PropertyName => propertyName;

	public string Configuration
	{
		get
		{
			return configuration;
		}
		set
		{
			configuration = value;
		}
	}

	public string Platform
	{
		get
		{
			return platform;
		}
		set
		{
			platform = value;
		}
	}

	public string OldValue
	{
		get
		{
			return oldValue;
		}
		set
		{
			oldValue = value;
		}
	}

	public string NewValue
	{
		get
		{
			return newValue;
		}
		set
		{
			newValue = value;
		}
	}

	public PropertyStorageLocations NewLocation
	{
		get
		{
			return newLocation;
		}
		set
		{
			newLocation = value;
		}
	}

	public PropertyStorageLocations OldLocation
	{
		get
		{
			return oldLocation;
		}
		set
		{
			oldLocation = value;
		}
	}

	public ProjectPropertyChangedEventArgs(string propertyName)
	{
		if (string.IsNullOrEmpty(propertyName))
		{
			throw new ArgumentNullException("propertyName");
		}
		this.propertyName = propertyName;
	}
}

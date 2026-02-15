using System;

namespace ICSharpCode.SharpDevelop.Gui;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class LocalizedPropertyAttribute : Attribute
{
	private string name = string.Empty;

	private string description = string.Empty;

	private string category = string.Empty;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public string Description
	{
		get
		{
			return description;
		}
		set
		{
			description = value;
		}
	}

	public string Category
	{
		get
		{
			return category;
		}
		set
		{
			category = value;
		}
	}

	public LocalizedPropertyAttribute(string name)
	{
		this.name = name;
	}
}

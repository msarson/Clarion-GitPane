using System;
using System.Drawing;

namespace ICSharpCode.Core;

public class ComponentFont : ICloneable
{
	private Font _Font;

	private string _FontString;

	private string _Component;

	private string _Description;

	public string FontString
	{
		get
		{
			return _FontString;
		}
		set
		{
			_FontString = value;
			_Font = FontService.StringToFont(value);
		}
	}

	public Font Font
	{
		get
		{
			return _Font;
		}
		set
		{
			_Font = value;
			_FontString = FontService.FontToString(value);
		}
	}

	public string Component
	{
		get
		{
			return _Component;
		}
		set
		{
			_Component = value.Replace('|', '_');
			if (!string.IsNullOrEmpty(_Component))
			{
				_Component = _Component.Replace('|', '_');
			}
		}
	}

	public string Description
	{
		get
		{
			return _Description;
		}
		set
		{
			_Description = value;
			if (!string.IsNullOrEmpty(_Description))
			{
				_Description = _Description.Replace('|', '_');
			}
		}
	}

	public ComponentFont()
	{
	}

	public ComponentFont(ComponentFont cloned)
		: this()
	{
		_Component = cloned.Component;
		_Description = cloned.Description;
		_Font = cloned.Font;
	}

	public ComponentFont(string component, string font)
		: this(component, string.Empty, font)
	{
	}

	public ComponentFont(string component, string description, Font font)
		: this()
	{
		Font = font;
		Component = component;
		Description = description;
	}

	public ComponentFont(string component, string description, string font)
		: this()
	{
		FontString = font;
		Component = component;
		Description = description;
	}

	public static ComponentFont FromString(string componentFont)
	{
		ComponentFont componentFont2 = new ComponentFont();
		try
		{
			string[] array = componentFont.Split(new char[1] { '|' }, 3);
			componentFont2.Component = array[0];
			componentFont2.Description = array[1];
			componentFont2.FontString = array[2];
		}
		catch
		{
		}
		return componentFont2;
	}

	public string ToStringSerialize()
	{
		if (string.IsNullOrEmpty(Description))
		{
			return Component + "||" + FontString;
		}
		return Component + "|" + Description + "|" + FontString;
	}

	public override string ToString()
	{
		if (string.IsNullOrEmpty(Description))
		{
			return Component;
		}
		return Description;
	}

	public object Clone()
	{
		return new ComponentFont(this);
	}
}

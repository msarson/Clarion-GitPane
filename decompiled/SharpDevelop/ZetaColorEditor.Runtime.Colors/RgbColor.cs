using System.Drawing;

namespace ZetaColorEditor.Runtime.Colors;

public sealed class RgbColor
{
	private int _Red;

	private int _Green;

	private int _Blue;

	public int Red
	{
		get
		{
			return _Red;
		}
		set
		{
			_Red = value;
		}
	}

	public int Green
	{
		get
		{
			return _Green;
		}
		set
		{
			_Green = value;
		}
	}

	public int Blue
	{
		get
		{
			return _Blue;
		}
		set
		{
			_Blue = value;
		}
	}

	public static RgbColor FromColor(Color color)
	{
		return ColorConverting.ColorToRgb(color);
	}

	public static RgbColor FromRgbColor(RgbColor color)
	{
		return new RgbColor(color.Red, color.Green, color.Blue);
	}

	public static RgbColor FromHsbColor(HsbColor color)
	{
		return color.ToRgbColor();
	}

	public static RgbColor FromHslColor(HslColor color)
	{
		return color.ToRgbColor();
	}

	public RgbColor(int red, int green, int blue)
	{
		Red = red;
		Green = green;
		Blue = blue;
	}

	public override string ToString()
	{
		return $"Red: {Red}; green: {Green}; blue: {Blue}";
	}

	public Color ToColor()
	{
		return ColorConverting.RgbToColor(this);
	}

	public RgbColor ToRgbColor()
	{
		return this;
	}

	public HsbColor ToHsbColor()
	{
		return ColorConverting.RgbToHsb(this);
	}

	public HslColor ToHslColor()
	{
		return ColorConverting.RgbToHsl(this);
	}

	public override bool Equals(object obj)
	{
		bool result = false;
		if (obj is RgbColor)
		{
			RgbColor rgbColor = (RgbColor)obj;
			if (Red == rgbColor.Red && Blue == rgbColor.Blue && Green == rgbColor.Green)
			{
				result = true;
			}
		}
		return result;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}

using System.Drawing;

namespace ZetaColorEditor.Runtime.Colors;

public sealed class HsbColor
{
	private int _Hue;

	private int _Saturation;

	private int _Brightness;

	public int Hue
	{
		get
		{
			return _Hue;
		}
		set
		{
			_Hue = value;
		}
	}

	public int Saturation
	{
		get
		{
			return _Saturation;
		}
		set
		{
			_Saturation = value;
		}
	}

	public int Brightness
	{
		get
		{
			return _Brightness;
		}
		set
		{
			_Brightness = value;
		}
	}

	public static HsbColor FromColor(Color color)
	{
		return ColorConverting.ColorToRgb(color).ToHsbColor();
	}

	public static HsbColor FromRgbColor(RgbColor color)
	{
		return color.ToHsbColor();
	}

	public static HsbColor FromHsbColor(HsbColor color)
	{
		return new HsbColor(color.Hue, color.Saturation, color.Brightness);
	}

	public static HsbColor FromHslColor(HslColor color)
	{
		return FromRgbColor(color.ToRgbColor());
	}

	public HsbColor(int hue, int saturation, int brightness)
	{
		Hue = hue;
		Saturation = saturation;
		Brightness = brightness;
	}

	public override string ToString()
	{
		return $"Hue: {Hue}; saturation: {Saturation}; brightness: {Brightness}.";
	}

	public Color ToColor()
	{
		return ColorConverting.HsbToRgb(this).ToColor();
	}

	public RgbColor ToRgbColor()
	{
		return ColorConverting.HsbToRgb(this);
	}

	public HsbColor ToHsbColor()
	{
		return this;
	}

	public HslColor ToHslColor()
	{
		return ColorConverting.RgbToHsl(ToRgbColor());
	}

	public override bool Equals(object obj)
	{
		bool result = false;
		if (obj is HsbColor)
		{
			HsbColor hsbColor = (HsbColor)obj;
			if (Hue == hsbColor.Hue && Saturation == hsbColor.Saturation && Brightness == hsbColor.Brightness)
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

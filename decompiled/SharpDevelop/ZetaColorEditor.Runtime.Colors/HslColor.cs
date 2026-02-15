using System.Drawing;

namespace ZetaColorEditor.Runtime.Colors;

public sealed class HslColor
{
	private double _hue;

	private double _saturation;

	private double _light;

	public int Hue
	{
		get
		{
			return (int)_hue;
		}
		set
		{
			_hue = value;
		}
	}

	public double PreciseHue
	{
		get
		{
			return _hue;
		}
		set
		{
			_hue = value;
		}
	}

	public int Saturation
	{
		get
		{
			return (int)_saturation;
		}
		set
		{
			_saturation = value;
		}
	}

	public double PreciseSaturation
	{
		get
		{
			return _saturation;
		}
		set
		{
			_saturation = value;
		}
	}

	public int Light
	{
		get
		{
			return (int)_light;
		}
		set
		{
			_light = value;
		}
	}

	public double PreciseLight
	{
		get
		{
			return _light;
		}
		set
		{
			_light = value;
		}
	}

	public static HslColor FromColor(Color color)
	{
		return ColorConverting.RgbToHsl(ColorConverting.ColorToRgb(color));
	}

	public static HslColor FromRgbColor(RgbColor color)
	{
		return color.ToHslColor();
	}

	public static HslColor FromHslColor(HslColor color)
	{
		return new HslColor(color.PreciseHue, color.PreciseSaturation, color.PreciseLight);
	}

	public static HslColor FromHsbColor(HsbColor color)
	{
		return FromRgbColor(color.ToRgbColor());
	}

	public HslColor(double hue, double saturation, double light)
	{
		_hue = hue;
		_saturation = saturation;
		_light = light;
	}

	public HslColor(int hue, int saturation, int light)
	{
		_hue = hue;
		_saturation = saturation;
		_light = light;
	}

	public override string ToString()
	{
		return $"Hue: {Hue}; saturation: {Saturation}; light: {Light}.";
	}

	public Color ToColor()
	{
		return ColorConverting.HslToRgb(this).ToColor();
	}

	public RgbColor ToRgbColor()
	{
		return ColorConverting.HslToRgb(this);
	}

	public HslColor ToHslColor()
	{
		return this;
	}

	public HsbColor ToHsbColor()
	{
		return ColorConverting.RgbToHsb(ToRgbColor());
	}

	public override bool Equals(object obj)
	{
		bool result = false;
		if (obj is HslColor)
		{
			HslColor hslColor = (HslColor)obj;
			if ((double)Hue == hslColor.PreciseHue && (double)Saturation == hslColor.PreciseSaturation && (double)Light == hslColor.PreciseLight)
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

using System;
using System.Drawing;

namespace ZetaColorEditor.Runtime.Colors;

internal static class ColorConverting
{
	public static RgbColor ColorToRgb(Color color)
	{
		return new RgbColor(color.R, color.G, color.B);
	}

	public static Color RgbToColor(RgbColor rgb)
	{
		return Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);
	}

	public static HsbColor RgbToHsb(RgbColor rgb)
	{
		double num = (double)rgb.Red / 255.0;
		double num2 = (double)rgb.Green / 255.0;
		double num3 = (double)rgb.Blue / 255.0;
		double minimumValue = getMinimumValue(num, num2, num3);
		double maximumValue = getMaximumValue(num, num2, num3);
		double num4 = maximumValue - minimumValue;
		double num5 = 0.0;
		double a = maximumValue * 100.0;
		double a2;
		if (maximumValue == 0.0 || num4 == 0.0)
		{
			num5 = 0.0;
			a2 = 0.0;
		}
		else
		{
			a2 = ((minimumValue != 0.0) ? (num4 / maximumValue * 100.0) : 100.0);
			if (Math.Abs(num - maximumValue) < double.Epsilon)
			{
				num5 = (num2 - num3) / num4;
			}
			else if (Math.Abs(num2 - maximumValue) < double.Epsilon)
			{
				num5 = 2.0 + (num3 - num) / num4;
			}
			else if (Math.Abs(num3 - maximumValue) < double.Epsilon)
			{
				num5 = 4.0 + (num - num2) / num4;
			}
		}
		num5 *= 60.0;
		if (num5 < 0.0)
		{
			num5 += 360.0;
		}
		return new HsbColor((int)Math.Round(num5), (int)Math.Round(a2), (int)Math.Round(a));
	}

	public static HslColor RgbToHsl(RgbColor rgb)
	{
		double num = (double)rgb.Red / 255.0;
		double num2 = (double)rgb.Green / 255.0;
		double num3 = (double)rgb.Blue / 255.0;
		double minimumValue = getMinimumValue(num, num2, num3);
		double maximumValue = getMaximumValue(num, num2, num3);
		double num4 = maximumValue - minimumValue;
		double num5 = (maximumValue + minimumValue) / 2.0;
		double num6;
		double num7;
		if (num4 == 0.0)
		{
			num6 = 0.0;
			num7 = 1.0;
		}
		else
		{
			num7 = ((!(num5 < 0.5)) ? (num4 / (2.0 - maximumValue - minimumValue)) : (num4 / (maximumValue + minimumValue)));
			double num8 = ((maximumValue - num) / 6.0 + num4 / 2.0) / num4;
			double num9 = ((maximumValue - num2) / 6.0 + num4 / 2.0) / num4;
			double num10 = ((maximumValue - num3) / 6.0 + num4 / 2.0) / num4;
			num6 = ((num == maximumValue) ? (num10 - num9) : ((num2 == maximumValue) ? (1.0 / 3.0 + num8 - num10) : ((num3 != maximumValue) ? 0.0 : (2.0 / 3.0 + num9 - num8))));
			if (num6 < 0.0)
			{
				num6 += 1.0;
			}
			if (num6 > 1.0)
			{
				num6 -= 1.0;
			}
		}
		return new HslColor(num6 * 360.0, num7 * 100.0, num5 * 100.0);
	}

	public static RgbColor HsbToRgb(HsbColor hsb)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = hsb.Hue;
		double num5 = (double)hsb.Saturation / 100.0;
		double num6 = (double)hsb.Brightness / 100.0;
		if (num5 == 0.0)
		{
			num = num6;
			num2 = num6;
			num3 = num6;
		}
		else
		{
			double num7 = num4 / 60.0;
			int num8 = (int)Math.Floor(num7);
			double num9 = num7 - (double)num8;
			double num10 = num6 * (1.0 - num5);
			double num11 = num6 * (1.0 - num5 * num9);
			double num12 = num6 * (1.0 - num5 * (1.0 - num9));
			switch (num8)
			{
			case 0:
				num = num6;
				num2 = num12;
				num3 = num10;
				break;
			case 1:
				num = num11;
				num2 = num6;
				num3 = num10;
				break;
			case 2:
				num = num10;
				num2 = num6;
				num3 = num12;
				break;
			case 3:
				num = num10;
				num2 = num11;
				num3 = num6;
				break;
			case 4:
				num = num12;
				num2 = num10;
				num3 = num6;
				break;
			case 5:
				num = num6;
				num2 = num10;
				num3 = num11;
				break;
			}
		}
		int red = (int)Math.Round(num * 255.0);
		int green = (int)Math.Round(num2 * 255.0);
		int blue = (int)Math.Round(num3 * 255.0);
		return new RgbColor(red, green, blue);
	}

	public static RgbColor HslToRgb(HslColor hsl)
	{
		double num = hsl.PreciseHue / 360.0;
		double num2 = hsl.PreciseSaturation / 100.0;
		double num3 = hsl.PreciseLight / 100.0;
		double num4;
		double num5;
		double num6;
		if (num2 == 0.0)
		{
			num4 = num3;
			num5 = num3;
			num6 = num3;
		}
		else
		{
			double num7 = ((!(num3 < 0.5)) ? (num3 + num2 - num2 * num3) : (num3 * (1.0 + num2)));
			double v = 2.0 * num3 - num7;
			num4 = hue_2_RGB(v, num7, num + 1.0 / 3.0);
			num5 = hue_2_RGB(v, num7, num);
			num6 = hue_2_RGB(v, num7, num - 1.0 / 3.0);
		}
		int red = (int)Math.Round(num4 * 255.0);
		int green = (int)Math.Round(num5 * 255.0);
		int blue = (int)Math.Round(num6 * 255.0);
		return new RgbColor(red, green, blue);
	}

	private static double hue_2_RGB(double v1, double v2, double vH)
	{
		if (vH < 0.0)
		{
			vH += 1.0;
		}
		if (vH > 1.0)
		{
			vH -= 1.0;
		}
		if (6.0 * vH < 1.0)
		{
			return v1 + (v2 - v1) * 6.0 * vH;
		}
		if (2.0 * vH < 1.0)
		{
			return v2;
		}
		if (3.0 * vH < 2.0)
		{
			return v1 + (v2 - v1) * (2.0 / 3.0 - vH) * 6.0;
		}
		return v1;
	}

	private static double getMaximumValue(params double[] values)
	{
		double num = values[0];
		if (values.Length >= 2)
		{
			for (int i = 1; i < values.Length; i++)
			{
				double val = values[i];
				num = Math.Max(num, val);
			}
		}
		return num;
	}

	private static double getMinimumValue(params double[] values)
	{
		double num = values[0];
		if (values.Length >= 2)
		{
			for (int i = 1; i < values.Length; i++)
			{
				double val = values[i];
				num = Math.Min(num, val);
			}
		}
		return num;
	}
}

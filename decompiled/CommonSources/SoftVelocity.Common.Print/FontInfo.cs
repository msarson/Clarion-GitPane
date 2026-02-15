using System;
using System.Drawing;

namespace SoftVelocity.Common.Print;

public class FontInfo : IDisposable
{
	private Font font;

	private string typeface;

	private float fontsize;

	private FontStyle style;

	private byte charset;

	public virtual string TypeFace
	{
		get
		{
			return typeface;
		}
		set
		{
			typeface = value;
			font = null;
		}
	}

	public virtual float FontSize
	{
		get
		{
			return fontsize;
		}
		set
		{
			fontsize = value;
			font = null;
		}
	}

	public virtual FontStyle Style
	{
		get
		{
			return style;
		}
		set
		{
			style = value;
			font = null;
		}
	}

	public virtual byte CharSet
	{
		get
		{
			return charset;
		}
		set
		{
			charset = value;
			font = null;
		}
	}

	public FontInfo()
	{
		font = null;
		TypeFace = "Courier New";
		FontSize = 10f;
		Style = FontStyle.Regular;
		CharSet = 1;
	}

	public void Dispose()
	{
		if (font != null)
		{
			font.Dispose();
		}
	}

	public Font MakeFont()
	{
		if (font == null)
		{
			font = new Font(TypeFace, FontSize, Style, GraphicsUnit.Point, CharSet);
		}
		return font;
	}
}

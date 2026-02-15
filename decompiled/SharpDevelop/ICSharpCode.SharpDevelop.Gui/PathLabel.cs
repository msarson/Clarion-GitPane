using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

public class PathLabel : Label
{
	private string initialText;

	public override string Text
	{
		get
		{
			return base.Text;
		}
		set
		{
			initialText = value;
			base.Text = GetTrimmedText();
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		base.Text = GetTrimmedText();
	}

	private string GetTrimmedText()
	{
		if (string.IsNullOrEmpty(initialText))
		{
			return initialText;
		}
		using Graphics graphics = CreateGraphics();
		Rectangle clientRectangle = base.ClientRectangle;
		clientRectangle.Width -= base.Padding.Horizontal;
		StringFormat stringFormat = new StringFormat();
		stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
		SizeF sizeF = graphics.MeasureString(initialText, Font, new SizeF(0f, 0f), stringFormat);
		if (sizeF.Width <= (float)clientRectangle.Width)
		{
			return initialText;
		}
		string text;
		try
		{
			text = ((!Path.IsPathRooted(initialText) || initialText[0] == '\\') ? "\\...\\" : (initialText[0] + ":\\...\\"));
		}
		catch (Exception)
		{
			return initialText;
		}
		SizeF sizeF2 = graphics.MeasureString(text, Font, new SizeF(0f, 0f), stringFormat);
		if (sizeF2.Width >= (float)clientRectangle.Width)
		{
			return initialText;
		}
		SizeF sizeF3 = new SizeF((float)clientRectangle.Width - sizeF2.Width, sizeF2.Height);
		int startIndex = initialText.Length - (int)(sizeF3.Width / (sizeF.Width / (float)initialText.Length));
		int num = initialText.LastIndexOf('\\', startIndex);
		if (num == -1)
		{
			return initialText;
		}
		string text2 = initialText.Substring(num + 1);
		SizeF sizeF4 = graphics.MeasureString(text2, Font, new SizeF(0f, 0f), stringFormat);
		while (sizeF4.Width + sizeF2.Width > (float)clientRectangle.Width)
		{
			num = text2.IndexOf("\\");
			if (num == -1)
			{
				return initialText;
			}
			text2 = text2.Substring(num + 1);
			sizeF4 = graphics.MeasureString(text2, Font, new SizeF(0f, 0f), stringFormat);
		}
		return text + text2;
	}
}

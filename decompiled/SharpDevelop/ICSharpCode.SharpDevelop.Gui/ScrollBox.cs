using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ScrollBox : UserControl
{
	private string[] text;

	private int[] textHeights;

	private Image image;

	private Timer timer;

	private int scroll = -220;

	private int curText;

	public int ScrollY
	{
		get
		{
			return scroll;
		}
		set
		{
			scroll = value;
		}
	}

	public Image Image
	{
		get
		{
			return image;
		}
		set
		{
			image = value;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			timer.Stop();
			foreach (Control control in base.Controls)
			{
				control.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	public ScrollBox()
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		Image = IconService.GetBitmap("Icons.AboutImage");
		Font = ResourceService.LoadFont("Tahoma", 10);
		text = new string[2]
		{
			"Licensed to: " + ClarionLic.Name,
			"Visit http://www.clarionSharp.com/blog for the latest news"
		};
		timer = new Timer();
		timer.Interval = 10;
		timer.Tick += ScrollDown;
		timer.Start();
	}

	private void ScrollDown(object sender, EventArgs e)
	{
		scroll++;
		Refresh();
	}

	protected override void OnPaintBackground(PaintEventArgs pe)
	{
		if (image != null)
		{
			pe.Graphics.DrawImage(image, 0, 0, base.Width, base.Height);
		}
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		Graphics graphics = pe.Graphics;
		if (textHeights == null)
		{
			textHeights = new int[text.Length];
			for (int i = 0; i < text.Length; i++)
			{
				textHeights[i] = (int)graphics.MeasureString(text[i], Font, new SizeF(base.Width / 2, base.Height * 2)).Height;
			}
		}
		graphics.DrawString(text[curText], Font, Brushes.Black, new Rectangle(base.Width / 2, -scroll, base.Width / 2, base.Height * 2));
		if (scroll > textHeights[curText])
		{
			curText = (curText + 1) % text.Length;
			scroll = -textHeights[curText] - base.Height;
		}
	}
}

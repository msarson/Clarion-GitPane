using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ZetaColorEditor.Runtime.Colors;

namespace ZetaColorEditor.Runtime.InternalControls.CustomColors;

public class ColorAreaUserControl : UserControl
{
	private double _h;

	private double _s;

	private Bitmap _colorBitmap;

	private readonly Brush _blackBrush = new SolidBrush(Color.Black);

	private readonly Brush _whiteBrush = new SolidBrush(Color.White);

	private IContainer components;

	public event EventHandler HueSaturationChanged;

	public event EventHandler ValueChangedByUser;

	public event EventHandler ColorSelected;

	public ColorAreaUserControl()
	{
		InitializeComponent();
		SetStyle(ControlStyles.UserPaint | ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, value: true);
	}

	public void SetHueSaturation(double h, double s)
	{
		_h = h;
		_s = s;
		Invalidate();
		notifyHueSaturationChanged();
	}

	public void GetHueSaturation(out double h, out double s)
	{
		h = _h;
		s = _s;
	}

	private void translateCaretPositionToHueSaturation(Point caretPosition, out double h, out double s)
	{
		double num = 360.0 / (double)base.ClientSize.Width;
		double num2 = 100.0 / (double)base.ClientSize.Height;
		Point point = caretPosition;
		point.X = Math.Max(0, point.X);
		point.X = Math.Min(base.ClientSize.Width - 1, point.X);
		point.Y = Math.Max(0, point.Y);
		point.Y = Math.Min(base.ClientSize.Height - 1, point.Y);
		point.Y = base.ClientSize.Height - point.Y;
		h = (double)point.X * num;
		s = (double)point.Y * num2;
		h = Math.Max(0.0, h);
		h = Math.Min(360.0, h);
		s = Math.Max(0.0, s);
		s = Math.Min(100.0, s);
	}

	private void translateHueSaturationToCaretPosition(out Point caretPosition, double h, double s)
	{
		double num = 360.0 / (double)base.ClientSize.Width;
		double num2 = 100.0 / (double)base.ClientSize.Height;
		h = Math.Max(0.0, h);
		h = Math.Min(360.0, h);
		s = Math.Max(0.0, s);
		s = Math.Min(100.0, s);
		double val = h / num;
		double val2 = s / num2;
		val = Math.Max(0.0, val);
		val = Math.Min(base.ClientSize.Width - 1, val);
		val2 = Math.Max(0.0, val2);
		val2 = Math.Min(base.ClientSize.Height - 1, val2);
		val2 = (double)base.ClientSize.Height - val2;
		caretPosition = new Point((int)val, (int)val2);
	}

	private void colorAreaUserControl_Paint(object sender, PaintEventArgs e)
	{
		if (_colorBitmap == null)
		{
			_colorBitmap = drawColorBitmap();
		}
		double num = (double)_colorBitmap.Width / (double)base.ClientSize.Width;
		double num2 = (double)_colorBitmap.Height / (double)base.ClientSize.Height;
		Rectangle clipRectangle = e.ClipRectangle;
		Rectangle srcRect = new Rectangle((int)(num * (double)clipRectangle.Left), (int)(num2 * (double)clipRectangle.Top), (int)(num * (double)clipRectangle.Width), (int)(num2 * (double)clipRectangle.Height));
		e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
		e.Graphics.DrawImage(_colorBitmap, clipRectangle, srcRect, GraphicsUnit.Pixel);
		drawCaret(e.Graphics);
	}

	private void drawCaret()
	{
		Invalidate();
	}

	private void drawCaret(Graphics g)
	{
		Brush brush;
		Brush brush2;
		if (Focused)
		{
			brush = _blackBrush;
			brush2 = _whiteBrush;
		}
		else
		{
			brush = _whiteBrush;
			brush2 = _blackBrush;
		}
		translateHueSaturationToCaretPosition(out var caretPosition, _h, _s);
		int num = caretPosition.X;
		int num2 = caretPosition.Y;
		g.FillRectangle(brush2, num - 2, num2 - 11, 5, 8);
		g.FillRectangle(brush2, num - 2, num2 + 3 + 1, 5, 8);
		g.FillRectangle(brush2, num - 11, num2 - 2, 8, 5);
		g.FillRectangle(brush2, num + 3 + 1, num2 - 2, 8, 5);
		g.FillRectangle(brush, num - 1, num2 - 10, 3, 6);
		g.FillRectangle(brush, num - 1, num2 + 4 + 1, 3, 6);
		g.FillRectangle(brush, num - 10, num2 - 1, 6, 3);
		g.FillRectangle(brush, num + 4 + 1, num2 - 1, 6, 3);
	}

	private static Bitmap drawColorBitmap()
	{
		Bitmap bitmap = new Bitmap(361, 101);
		for (int i = 0; i < 101; i++)
		{
			for (int j = 0; j < 361; j++)
			{
				double hue = j;
				double saturation = 100 - i;
				double light = 100 - i;
				Color color = new HslColor(hue, saturation, light).ToColor();
				bitmap.SetPixel(j, i, color);
			}
		}
		return bitmap;
	}

	private void notifyHueSaturationChanged()
	{
		if (this.HueSaturationChanged != null)
		{
			this.HueSaturationChanged(this, EventArgs.Empty);
		}
	}

	private void notifyValueChangedByUser()
	{
		if (this.ValueChangedByUser != null)
		{
			this.ValueChangedByUser(this, EventArgs.Empty);
		}
	}

	private void colorAreaUserControl_Enter(object sender, EventArgs e)
	{
		drawCaret();
	}

	private void colorAreaUserControl_Leave(object sender, EventArgs e)
	{
		drawCaret();
	}

	private void colorAreaUserControl_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			translateCaretPositionToHueSaturation(e.Location, out _h, out _s);
			drawCaret();
			notifyValueChangedByUser();
			notifyHueSaturationChanged();
		}
	}

	private void colorAreaUserControl_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			translateCaretPositionToHueSaturation(e.Location, out _h, out _s);
			drawCaret();
			notifyValueChangedByUser();
			notifyHueSaturationChanged();
		}
	}

	private void colorAreaUserControl_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			translateCaretPositionToHueSaturation(e.Location, out _h, out _s);
			drawCaret();
			notifyValueChangedByUser();
			notifyHueSaturationChanged();
		}
	}

	private void DoColorSelected()
	{
		if (this.ColorSelected != null)
		{
			this.ColorSelected(this, EventArgs.Empty);
		}
	}

	private void ColorAreaUserControl_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		DoColorSelected();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.White;
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.Name = "ColorAreaUserControl";
		base.Size = new System.Drawing.Size(200, 185);
		base.Paint += new System.Windows.Forms.PaintEventHandler(colorAreaUserControl_Paint);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(colorAreaUserControl_MouseMove);
		base.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(ColorAreaUserControl_MouseDoubleClick);
		base.Leave += new System.EventHandler(colorAreaUserControl_Leave);
		base.MouseClick += new System.Windows.Forms.MouseEventHandler(colorAreaUserControl_MouseClick);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(colorAreaUserControl_MouseDown);
		base.Enter += new System.EventHandler(colorAreaUserControl_Enter);
		base.ResumeLayout(false);
	}
}

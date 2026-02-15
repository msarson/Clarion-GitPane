using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ZetaColorEditor.Runtime.Colors;

namespace ZetaColorEditor.Runtime.InternalControls.CustomColors;

public class ColorSliderPanel : Panel
{
	private Bitmap _colorBitmap;

	private double _h;

	private double _s;

	private double _l;

	private IContainer components;

	public event EventHandler ValueChangedByUser;

	public event EventHandler ValueChanged;

	public ColorSliderPanel()
	{
		InitializeComponent();
		SetStyle(ControlStyles.UserPaint | ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, value: true);
	}

	public void SetHueSaturation(double h, double s)
	{
		_h = h;
		_s = s;
		_colorBitmap = drawColorBitmap();
		Invalidate();
		notifyValueChanged();
	}

	private void notifyValueChanged()
	{
		if (this.ValueChanged != null)
		{
			this.ValueChanged(this, EventArgs.Empty);
		}
	}

	private Bitmap drawColorBitmap()
	{
		double h = _h;
		double s = _s;
		Bitmap bitmap = new Bitmap(5, 100);
		for (int i = 0; i < 100; i++)
		{
			double light = 100 - i;
			Color color = new HslColor(h, s, light).ToColor();
			for (int j = 0; j < 5; j++)
			{
				bitmap.SetPixel(j, i, color);
			}
		}
		return bitmap;
	}

	private void colorSliderPanel_Paint(object sender, PaintEventArgs e)
	{
		if (_colorBitmap == null)
		{
			_colorBitmap = drawColorBitmap();
		}
		double num = (double)_colorBitmap.Height / (double)base.ClientSize.Height;
		Rectangle clipRectangle = e.ClipRectangle;
		Rectangle srcRect = new Rectangle(0, (int)(num * (double)clipRectangle.Top), 1, (int)(num * (double)clipRectangle.Height));
		e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
		e.Graphics.DrawImage(_colorBitmap, clipRectangle, srcRect, GraphicsUnit.Pixel);
	}

	public Color GetColorAtY(int y)
	{
		return new HslColor(_h, _s, _l).ToColor();
	}

	internal void TranslateCaretPositionYToLight(int caretPositionY, out double l)
	{
		double num = 100.0 / (double)base.ClientSize.Height;
		double val = caretPositionY;
		val = Math.Max(0.0, val);
		val = Math.Min(base.ClientSize.Height - 1, val);
		val = (double)base.ClientSize.Height - val;
		l = val * num;
		l = Math.Max(0.0, l);
		l = Math.Min(100.0, l);
	}

	internal void TranslateLightToCaretPositionY(out int caretPositionY, double l)
	{
		double num = 100.0 / (double)base.ClientSize.Height;
		l = Math.Max(0.0, l);
		l = Math.Min(100.0, l);
		double val = l / num;
		val = Math.Max(0.0, val);
		val = Math.Min(base.ClientSize.Height - 1, val);
		val = (double)base.ClientSize.Height - val;
		caretPositionY = toParentPositionY((int)val);
	}

	private int toParentPositionY(int y)
	{
		Point p = new Point(0, y);
		p = PointToScreen(p);
		return base.Parent.PointToClient(p).Y;
	}

	public void SetLight(double l)
	{
		_l = l;
		notifyValueChanged();
	}

	public double GetLight()
	{
		return _l;
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
		base.Paint += new System.Windows.Forms.PaintEventHandler(colorSliderPanel_Paint);
		base.ResumeLayout(false);
	}
}

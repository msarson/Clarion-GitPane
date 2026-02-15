using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZetaColorEditor.Runtime.Colors;

namespace ZetaColorEditor.Runtime.InternalControls.CustomColors;

public class ColorAreaAndSliderUserControl : UserControl
{
	private IContainer components;

	private ColorSliderUserControl colorSliderControl;

	private ColorAreaUserControl colorAreaControl;

	public Color SelectedColor
	{
		get
		{
			return colorSliderControl.GetSelectedColor();
		}
		set
		{
			HslColor hslColor = HslColor.FromColor(value);
			colorAreaControl.SetHueSaturation(hslColor.PreciseHue, hslColor.PreciseSaturation);
			colorSliderControl.SetHueSaturation(hslColor.PreciseHue, hslColor.PreciseSaturation);
			colorSliderControl.SetLight(hslColor.PreciseLight);
			notifyColorChanged();
		}
	}

	public event EventHandler ColorChanged;

	public event EventHandler ValueChangedByUser;

	public event EventHandler ColorSelected;

	public ColorAreaAndSliderUserControl()
	{
		InitializeComponent();
	}

	private void colorAreaControl_HueSaturationChanged(object sender, EventArgs e)
	{
		colorAreaControl.GetHueSaturation(out var h, out var s);
		colorSliderControl.SetHueSaturation(h, s);
		notifyColorChanged();
	}

	private void notifyColorChanged()
	{
		if (this.ColorChanged != null)
		{
			this.ColorChanged(this, EventArgs.Empty);
		}
	}

	private void notifyValueChangedByUser()
	{
		if (this.ValueChangedByUser != null)
		{
			this.ValueChangedByUser(this, EventArgs.Empty);
		}
	}

	private void colorSliderControl_BrightnessChanged(object sender, EventArgs e)
	{
		notifyColorChanged();
	}

	private void colorAreaControl_ValueChangedByUser(object sender, EventArgs e)
	{
		notifyColorChanged();
		notifyValueChangedByUser();
	}

	private void colorSliderControl_ValueChangedByUser(object sender, EventArgs e)
	{
		notifyColorChanged();
		notifyValueChangedByUser();
	}

	private void DoColorSelected()
	{
		if (this.ColorSelected != null)
		{
			this.ColorSelected(this, EventArgs.Empty);
		}
	}

	private void ColorAreaAndSliderUserControl_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		DoColorSelected();
	}

	private void colorSliderControl_ColorSelected(object sender, EventArgs e)
	{
		DoColorSelected();
	}

	private void colorAreaControl_ColorSelected(object sender, EventArgs e)
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
		this.colorSliderControl = new ZetaColorEditor.Runtime.InternalControls.CustomColors.ColorSliderUserControl();
		this.colorAreaControl = new ZetaColorEditor.Runtime.InternalControls.CustomColors.ColorAreaUserControl();
		base.SuspendLayout();
		this.colorSliderControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.colorSliderControl.Dock = System.Windows.Forms.DockStyle.Right;
		this.colorSliderControl.Location = new System.Drawing.Point(290, 0);
		this.colorSliderControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.colorSliderControl.Name = "colorSliderControl";
		this.colorSliderControl.Size = new System.Drawing.Size(45, 328);
		this.colorSliderControl.TabIndex = 3;
		this.colorSliderControl.ColorSelected += new System.EventHandler(colorSliderControl_ColorSelected);
		this.colorSliderControl.ValueChangedByUser += new System.EventHandler(colorSliderControl_ValueChangedByUser);
		this.colorSliderControl.LightChanged += new System.EventHandler(colorSliderControl_BrightnessChanged);
		this.colorAreaControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.colorAreaControl.BackColor = System.Drawing.SystemColors.ButtonShadow;
		this.colorAreaControl.Location = new System.Drawing.Point(0, 0);
		this.colorAreaControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.colorAreaControl.Name = "colorAreaControl";
		this.colorAreaControl.Size = new System.Drawing.Size(282, 328);
		this.colorAreaControl.TabIndex = 2;
		this.colorAreaControl.ColorSelected += new System.EventHandler(colorAreaControl_ColorSelected);
		this.colorAreaControl.HueSaturationChanged += new System.EventHandler(colorAreaControl_HueSaturationChanged);
		this.colorAreaControl.ValueChangedByUser += new System.EventHandler(colorAreaControl_ValueChangedByUser);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		base.Controls.Add(this.colorSliderControl);
		base.Controls.Add(this.colorAreaControl);
		this.Font = new System.Drawing.Font("Segoe UI", 7.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "ColorAreaAndSliderUserControl";
		base.Size = new System.Drawing.Size(335, 328);
		base.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(ColorAreaAndSliderUserControl_MouseDoubleClick);
		base.ResumeLayout(false);
	}
}

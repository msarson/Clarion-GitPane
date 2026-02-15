using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ZetaColorEditor.Runtime.InternalControls.CustomColors;

public class ColorSliderUserControl : UserControl
{
	private IContainer components;

	private PictureBox arrowControl;

	private ColorSliderPanel colorPanel;

	public event EventHandler LightChanged;

	public event EventHandler ValueChangedByUser;

	public event EventHandler ColorSelected;

	public ColorSliderUserControl()
	{
		InitializeComponent();
		SetStyle(ControlStyles.Selectable, value: true);
	}

	private void notifyLightChanged()
	{
		if (this.LightChanged != null)
		{
			this.LightChanged(this, EventArgs.Empty);
		}
	}

	private void notifyValueChangedByUser()
	{
		if (this.ValueChangedByUser != null)
		{
			this.ValueChangedByUser(this, EventArgs.Empty);
		}
	}

	private void DoColorSelected()
	{
		if (this.ColorSelected != null)
		{
			this.ColorSelected(this, EventArgs.Empty);
		}
	}

	public Color GetSelectedColor()
	{
		return colorPanel.GetColorAtY(arrowControl.Location.Y);
	}

	public void SetHueSaturation(double h, double s)
	{
		colorPanel.SetHueSaturation(h, s);
		notifyLightChanged();
	}

	public void SetLight(double l)
	{
		colorPanel.SetLight(l);
		colorPanel.TranslateLightToCaretPositionY(out var caretPositionY, l);
		repositionArrow(caretPositionY);
		notifyLightChanged();
	}

	private void repositionArrow(int offsetY)
	{
		offsetY = Math.Max(0, offsetY);
		offsetY = Math.Min(base.ClientSize.Height - 1, offsetY);
		arrowControl.Location = new Point(arrowControl.Location.X, offsetY - arrowControl.Height / 2);
		colorPanel.TranslateCaretPositionYToLight(offsetY, out var l);
		colorPanel.SetLight(l);
	}

	private void colorSliderUserControl_MouseClick(object sender, MouseEventArgs e)
	{
		repositionArrow(e.Location.Y);
		notifyValueChangedByUser();
	}

	private void arrowControl_MouseClick(object sender, MouseEventArgs e)
	{
		repositionArrow(PointToClient(arrowControl.PointToScreen(e.Location)).Y);
		notifyValueChangedByUser();
	}

	private void colorPanel_MouseClick(object sender, MouseEventArgs e)
	{
		repositionArrow(PointToClient(colorPanel.PointToScreen(e.Location)).Y);
		notifyValueChangedByUser();
	}

	private void colorSliderUserControl_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			repositionArrow(e.Location.Y);
			notifyValueChangedByUser();
		}
	}

	private void arrowControl_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			repositionArrow(PointToClient(arrowControl.PointToScreen(e.Location)).Y);
			notifyValueChangedByUser();
		}
	}

	private void colorPanel_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			repositionArrow(PointToClient(colorPanel.PointToScreen(e.Location)).Y);
			notifyValueChangedByUser();
		}
	}

	private void colorSliderUserControl_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			repositionArrow(e.Location.Y);
			notifyValueChangedByUser();
		}
	}

	private void colorPanel_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			repositionArrow(PointToClient(colorPanel.PointToScreen(e.Location)).Y);
			notifyValueChangedByUser();
		}
	}

	private void arrowControl_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			repositionArrow(PointToClient(arrowControl.PointToScreen(e.Location)).Y);
			notifyValueChangedByUser();
		}
	}

	private void colorPanel_ValueChangedByUser(object sender, EventArgs e)
	{
		notifyValueChangedByUser();
	}

	private void colorPanel_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			DoColorSelected();
		}
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZetaColorEditor.Runtime.InternalControls.CustomColors.ColorSliderUserControl));
		this.arrowControl = new System.Windows.Forms.PictureBox();
		this.colorPanel = new ZetaColorEditor.Runtime.InternalControls.CustomColors.ColorSliderPanel();
		((System.ComponentModel.ISupportInitialize)this.arrowControl).BeginInit();
		base.SuspendLayout();
		this.arrowControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.arrowControl.Image = (System.Drawing.Image)resources.GetObject("arrowControl.Image");
		this.arrowControl.Location = new System.Drawing.Point(31, 0);
		this.arrowControl.Margin = new System.Windows.Forms.Padding(4);
		this.arrowControl.Name = "arrowControl";
		this.arrowControl.Size = new System.Drawing.Size(15, 14);
		this.arrowControl.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
		this.arrowControl.TabIndex = 1;
		this.arrowControl.TabStop = false;
		this.arrowControl.MouseMove += new System.Windows.Forms.MouseEventHandler(arrowControl_MouseMove);
		this.arrowControl.MouseClick += new System.Windows.Forms.MouseEventHandler(arrowControl_MouseClick);
		this.arrowControl.MouseDown += new System.Windows.Forms.MouseEventHandler(arrowControl_MouseDown);
		this.colorPanel.Dock = System.Windows.Forms.DockStyle.Left;
		this.colorPanel.Location = new System.Drawing.Point(0, 0);
		this.colorPanel.Margin = new System.Windows.Forms.Padding(4);
		this.colorPanel.Name = "colorPanel";
		this.colorPanel.Size = new System.Drawing.Size(23, 434);
		this.colorPanel.TabIndex = 0;
		this.colorPanel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(colorPanel_MouseDoubleClick);
		this.colorPanel.ValueChangedByUser += new System.EventHandler(colorPanel_ValueChangedByUser);
		this.colorPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(colorPanel_MouseClick);
		this.colorPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(colorPanel_MouseDown);
		this.colorPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(colorPanel_MouseMove);
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.Controls.Add(this.colorPanel);
		base.Controls.Add(this.arrowControl);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "ColorSliderUserControl";
		base.Size = new System.Drawing.Size(53, 434);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(colorSliderUserControl_MouseMove);
		base.MouseClick += new System.Windows.Forms.MouseEventHandler(colorSliderUserControl_MouseClick);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(colorSliderUserControl_MouseDown);
		((System.ComponentModel.ISupportInitialize)this.arrowControl).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

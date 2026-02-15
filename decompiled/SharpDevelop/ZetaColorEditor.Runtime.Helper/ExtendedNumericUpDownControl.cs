using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ZetaColorEditor.Runtime.Helper;

public class ExtendedNumericUpDownControl : NumericUpDown
{
	private IContainer components;

	private Timer changeTimer;

	public ExtendedNumericUpDownControl()
	{
		InitializeComponent();
		_ = base.DesignMode;
	}

	private void changeTimer_Tick(object sender, EventArgs e)
	{
	}

	private void extendedNumericUpDownControl_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
		{
			OnValueChanged(EventArgs.Empty);
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
		this.components = new System.ComponentModel.Container();
		this.changeTimer = new System.Windows.Forms.Timer(this.components);
		((System.ComponentModel.ISupportInitialize)this).BeginInit();
		base.SuspendLayout();
		this.changeTimer.Interval = 300;
		this.changeTimer.Tick += new System.EventHandler(changeTimer_Tick);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(extendedNumericUpDownControl_KeyDown);
		((System.ComponentModel.ISupportInitialize)this).EndInit();
		base.ResumeLayout(false);
	}
}

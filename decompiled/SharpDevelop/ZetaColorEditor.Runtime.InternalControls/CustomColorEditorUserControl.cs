using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZetaColorEditor.Runtime.Colors;
using ZetaColorEditor.Runtime.Helper;
using ZetaColorEditor.Runtime.InternalControls.CustomColors;

namespace ZetaColorEditor.Runtime.InternalControls;

public class CustomColorEditorUserControl : UserControl
{
	private enum LeadingInputElement
	{
		Unknown,
		ColorAreaAndSlider,
		RgbInput,
		HslInput,
		HtmlInput,
		ClarionInput
	}

	private bool _ignoreTextFieldChange;

	private bool _ignoreColorChangeEvents;

	private Control _changingControl;

	private LeadingInputElement _currentLeadingInputElement;

	private IContainer components;

	private GroupBox groupBox1;

	private Label label3;

	private Label label2;

	private Label label1;

	private ExtendedNumericUpDownControl rControl;

	private ExtendedNumericUpDownControl bControl;

	private ExtendedNumericUpDownControl gControl;

	private GroupBox groupBox2;

	private ExtendedNumericUpDownControl lControl;

	private ExtendedNumericUpDownControl sControl;

	private ExtendedNumericUpDownControl hControl;

	private Label label4;

	private Label label5;

	private Label label6;

	private TextBox htmlTextBox;

	private GroupBox groupBox3;

	private Panel currentColorPanel;

	private ColorAreaAndSliderUserControl colorControl;

	private GroupBox groupBox4;

	private Label label9;

	private Label label8;

	private Label label7;

	private Label label10;

	private TextBox clarionTextBox;

	private GroupBox groupBox5;

	[Browsable(false)]
	public Color SelectedColor
	{
		get
		{
			if (base.DesignMode)
			{
				return Color.Empty;
			}
			switch (_currentLeadingInputElement)
			{
			case LeadingInputElement.ClarionInput:
				if (!string.IsNullOrEmpty(clarionTextBox.Text))
				{
					return ColorEditorUserControl.FromClarion(clarionTextBox.Text);
				}
				return colorControl.SelectedColor;
			case LeadingInputElement.HtmlInput:
			{
				string text = htmlTextBox.Text.Trim('#', ' ');
				if (text.Length == 6)
				{
					return RgbColor.FromColor(ColorTranslator.FromHtml("#" + text)).ToColor();
				}
				return colorControl.SelectedColor;
			}
			case LeadingInputElement.ColorAreaAndSlider:
				return colorControl.SelectedColor;
			case LeadingInputElement.HslInput:
				return new HslColor((double)hControl.Value, (double)sControl.Value, (double)lControl.Value).ToColor();
			default:
				return new RgbColor((int)rControl.Value, (int)gControl.Value, (int)bControl.Value).ToColor();
			}
		}
		set
		{
			if (!base.DesignMode)
			{
				_ignoreTextFieldChange = true;
				_ignoreColorChangeEvents = true;
				try
				{
					HslColor hslColor = HslColor.FromColor(value);
					rControl.Value = value.R;
					gControl.Value = value.G;
					bControl.Value = value.B;
					hControl.Value = (decimal)hslColor.PreciseHue;
					sControl.Value = (decimal)hslColor.PreciseSaturation;
					lControl.Value = (decimal)hslColor.PreciseLight;
					htmlTextBox.Text = toHtml(value);
					currentColorPanel.BackColor = value;
					clarionTextBox.Text = ColorEditorUserControl.ToClarion(value).ToString();
					setColor(RgbColor.FromColor(value));
				}
				finally
				{
					_ignoreColorChangeEvents = false;
					_ignoreTextFieldChange = false;
				}
				if (this.NeedUpdateUI != null)
				{
					this.NeedUpdateUI(this, EventArgs.Empty);
				}
			}
		}
	}

	public event EventHandler NeedUpdateUI;

	public event EventHandler ColorSelected;

	public CustomColorEditorUserControl()
	{
		InitializeComponent();
	}

	private void customColorEditorUserControl_Load(object sender, EventArgs e)
	{
	}

	private void colorControl_ColorChanged(object sender, EventArgs e)
	{
		if (!_ignoreColorChangeEvents)
		{
			updateTextFields();
			if (this.NeedUpdateUI != null)
			{
				this.NeedUpdateUI(this, EventArgs.Empty);
			}
		}
	}

	private void DoColorSelected()
	{
		if (this.ColorSelected != null)
		{
			this.ColorSelected(this, EventArgs.Empty);
		}
	}

	private void updateTextFields()
	{
		_ignoreTextFieldChange = true;
		try
		{
			currentColorPanel.BackColor = colorControl.SelectedColor;
			Color selectedColor = colorControl.SelectedColor;
			HslColor hslColor = HslColor.FromColor(selectedColor);
			if (_currentLeadingInputElement != LeadingInputElement.HtmlInput && _changingControl != htmlTextBox)
			{
				htmlTextBox.Text = toHtml(selectedColor);
			}
			if (_currentLeadingInputElement != LeadingInputElement.ClarionInput && _changingControl != clarionTextBox)
			{
				clarionTextBox.Text = ColorEditorUserControl.ToClarion(selectedColor).ToString();
			}
			if (_currentLeadingInputElement != LeadingInputElement.RgbInput)
			{
				if (_changingControl != rControl)
				{
					rControl.Value = selectedColor.R;
				}
				if (_changingControl != gControl)
				{
					gControl.Value = selectedColor.G;
				}
				if (_changingControl != bControl)
				{
					bControl.Value = selectedColor.B;
				}
			}
			if (_currentLeadingInputElement != LeadingInputElement.HslInput)
			{
				if (_changingControl != hControl)
				{
					hControl.Value = (decimal)hslColor.PreciseHue;
				}
				if (_changingControl != sControl)
				{
					sControl.Value = (decimal)hslColor.PreciseSaturation;
				}
				if (_changingControl != lControl)
				{
					lControl.Value = (decimal)hslColor.PreciseLight;
				}
			}
		}
		finally
		{
			_ignoreTextFieldChange = false;
		}
	}

	private static string toHtml(Color color)
	{
		return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
	}

	private void setColor(HslColor color)
	{
		setColor(color.ToRgbColor());
	}

	private void setColor(RgbColor color)
	{
		Color selectedColor = color.ToColor();
		colorControl.SelectedColor = selectedColor;
		if (this.NeedUpdateUI != null)
		{
			this.NeedUpdateUI(this, EventArgs.Empty);
		}
	}

	private void rControl_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignoreTextFieldChange && rControl.Value >= 0m && rControl.Value <= 255m)
		{
			_changingControl = (Control)sender;
			notifyValueChangedByUser(LeadingInputElement.RgbInput);
			setColor(new RgbColor((int)rControl.Value, (int)gControl.Value, (int)bControl.Value));
			_changingControl = null;
		}
	}

	private void gControl_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignoreTextFieldChange && gControl.Value >= 0m && gControl.Value <= 255m)
		{
			_changingControl = (Control)sender;
			notifyValueChangedByUser(LeadingInputElement.RgbInput);
			setColor(new RgbColor((int)rControl.Value, (int)gControl.Value, (int)bControl.Value));
			_changingControl = null;
		}
	}

	private void bControl_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignoreTextFieldChange && bControl.Value >= 0m && bControl.Value <= 255m)
		{
			_changingControl = (Control)sender;
			notifyValueChangedByUser(LeadingInputElement.RgbInput);
			setColor(new RgbColor((int)rControl.Value, (int)gControl.Value, (int)bControl.Value));
			_changingControl = null;
		}
	}

	private void hControl_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignoreTextFieldChange && hControl.Value >= 0m && hControl.Value <= 360m)
		{
			_changingControl = (Control)sender;
			notifyValueChangedByUser(LeadingInputElement.HslInput);
			setColor(new HslColor((double)hControl.Value, (double)sControl.Value, (double)lControl.Value));
			_changingControl = null;
		}
	}

	private void sControl_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignoreTextFieldChange && sControl.Value >= 0m && sControl.Value <= 100m)
		{
			_changingControl = (Control)sender;
			notifyValueChangedByUser(LeadingInputElement.HslInput);
			setColor(new HslColor((double)hControl.Value, (double)sControl.Value, (double)lControl.Value));
			_changingControl = null;
		}
	}

	private void lControl_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignoreTextFieldChange && lControl.Value >= 0m && lControl.Value <= 100m)
		{
			_changingControl = (Control)sender;
			notifyValueChangedByUser(LeadingInputElement.HslInput);
			setColor(new HslColor((double)hControl.Value, (double)sControl.Value, (double)lControl.Value));
			_changingControl = null;
		}
	}

	private void htmlTextBox_TextChanged(object sender, EventArgs e)
	{
		if (!_ignoreTextFieldChange)
		{
			string text = htmlTextBox.Text.Trim('#', ' ');
			if (text.Length == 6)
			{
				_changingControl = (Control)sender;
				notifyValueChangedByUser(LeadingInputElement.HtmlInput);
				setColor(RgbColor.FromColor(ColorTranslator.FromHtml("#" + text)));
				_changingControl = null;
			}
		}
	}

	private void clarionTextBox_TextChanged(object sender, EventArgs e)
	{
		if (!_ignoreTextFieldChange && !string.IsNullOrEmpty(clarionTextBox.Text))
		{
			Color color = ColorEditorUserControl.FromClarion(clarionTextBox.Text);
			_changingControl = (Control)sender;
			notifyValueChangedByUser(LeadingInputElement.ClarionInput);
			setColor(new RgbColor(color.R, color.G, color.B));
			_changingControl = null;
		}
	}

	private void colorControl_ValueChangedByUser(object sender, EventArgs e)
	{
		notifyValueChangedByUser(LeadingInputElement.ColorAreaAndSlider);
	}

	private void notifyValueChangedByUser(LeadingInputElement inputElement)
	{
		_currentLeadingInputElement = inputElement;
	}

	private void currentColorPanel_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			DoColorSelected();
		}
	}

	private void colorControl_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			DoColorSelected();
		}
	}

	private void label10_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			DoColorSelected();
		}
	}

	private void colorControl_ColorSelected(object sender, EventArgs e)
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
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.bControl = new ZetaColorEditor.Runtime.Helper.ExtendedNumericUpDownControl();
		this.gControl = new ZetaColorEditor.Runtime.Helper.ExtendedNumericUpDownControl();
		this.rControl = new ZetaColorEditor.Runtime.Helper.ExtendedNumericUpDownControl();
		this.label3 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.lControl = new ZetaColorEditor.Runtime.Helper.ExtendedNumericUpDownControl();
		this.sControl = new ZetaColorEditor.Runtime.Helper.ExtendedNumericUpDownControl();
		this.hControl = new ZetaColorEditor.Runtime.Helper.ExtendedNumericUpDownControl();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.htmlTextBox = new System.Windows.Forms.TextBox();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.currentColorPanel = new System.Windows.Forms.Panel();
		this.label10 = new System.Windows.Forms.Label();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.clarionTextBox = new System.Windows.Forms.TextBox();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.colorControl = new ZetaColorEditor.Runtime.InternalControls.CustomColors.ColorAreaAndSliderUserControl();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.bControl).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gControl).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.rControl).BeginInit();
		this.groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lControl).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.sControl).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.hControl).BeginInit();
		this.groupBox3.SuspendLayout();
		this.currentColorPanel.SuspendLayout();
		this.groupBox4.SuspendLayout();
		this.groupBox5.SuspendLayout();
		base.SuspendLayout();
		this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox1.Controls.Add(this.bControl);
		this.groupBox1.Controls.Add(this.gControl);
		this.groupBox1.Controls.Add(this.rControl);
		this.groupBox1.Controls.Add(this.label3);
		this.groupBox1.Controls.Add(this.label2);
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Location = new System.Drawing.Point(290, 3);
		this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox1.Size = new System.Drawing.Size(95, 106);
		this.groupBox1.TabIndex = 1;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "RGB";
		this.bControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.bControl.Location = new System.Drawing.Point(28, 77);
		this.bControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.bControl.Maximum = new decimal(new int[4] { 255, 0, 0, 0 });
		this.bControl.Name = "bControl";
		this.bControl.Size = new System.Drawing.Size(55, 25);
		this.bControl.TabIndex = 5;
		this.bControl.Value = new decimal(new int[4] { 255, 0, 0, 0 });
		this.bControl.ValueChanged += new System.EventHandler(bControl_ValueChanged);
		this.gControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.gControl.Location = new System.Drawing.Point(28, 48);
		this.gControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.gControl.Maximum = new decimal(new int[4] { 255, 0, 0, 0 });
		this.gControl.Name = "gControl";
		this.gControl.Size = new System.Drawing.Size(55, 25);
		this.gControl.TabIndex = 3;
		this.gControl.ValueChanged += new System.EventHandler(gControl_ValueChanged);
		this.rControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.rControl.Location = new System.Drawing.Point(28, 20);
		this.rControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.rControl.Maximum = new decimal(new int[4] { 255, 0, 0, 0 });
		this.rControl.Name = "rControl";
		this.rControl.Size = new System.Drawing.Size(55, 25);
		this.rControl.TabIndex = 1;
		this.rControl.ValueChanged += new System.EventHandler(rControl_ValueChanged);
		this.label3.AutoSize = true;
		this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label3.Location = new System.Drawing.Point(7, 80);
		this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(20, 19);
		this.label3.TabIndex = 4;
		this.label3.Text = "&B:";
		this.label2.AutoSize = true;
		this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label2.Location = new System.Drawing.Point(7, 51);
		this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(22, 19);
		this.label2.TabIndex = 2;
		this.label2.Text = "&G:";
		this.label1.AutoSize = true;
		this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label1.Location = new System.Drawing.Point(7, 24);
		this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(20, 19);
		this.label1.TabIndex = 0;
		this.label1.Text = "&R:";
		this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox2.Controls.Add(this.lControl);
		this.groupBox2.Controls.Add(this.sControl);
		this.groupBox2.Controls.Add(this.hControl);
		this.groupBox2.Controls.Add(this.label4);
		this.groupBox2.Controls.Add(this.label5);
		this.groupBox2.Controls.Add(this.label9);
		this.groupBox2.Controls.Add(this.label8);
		this.groupBox2.Controls.Add(this.label7);
		this.groupBox2.Controls.Add(this.label6);
		this.groupBox2.Location = new System.Drawing.Point(290, 112);
		this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox2.Size = new System.Drawing.Size(95, 109);
		this.groupBox2.TabIndex = 2;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "HSL";
		this.lControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lControl.Location = new System.Drawing.Point(26, 77);
		this.lControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.lControl.Name = "lControl";
		this.lControl.Size = new System.Drawing.Size(46, 25);
		this.lControl.TabIndex = 7;
		this.lControl.ValueChanged += new System.EventHandler(lControl_ValueChanged);
		this.sControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.sControl.Location = new System.Drawing.Point(26, 48);
		this.sControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.sControl.Name = "sControl";
		this.sControl.Size = new System.Drawing.Size(46, 25);
		this.sControl.TabIndex = 4;
		this.sControl.ValueChanged += new System.EventHandler(sControl_ValueChanged);
		this.hControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.hControl.Location = new System.Drawing.Point(26, 20);
		this.hControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.hControl.Maximum = new decimal(new int[4] { 360, 0, 0, 0 });
		this.hControl.Name = "hControl";
		this.hControl.Size = new System.Drawing.Size(46, 25);
		this.hControl.TabIndex = 1;
		this.hControl.ValueChanged += new System.EventHandler(hControl_ValueChanged);
		this.label4.AutoSize = true;
		this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label4.Location = new System.Drawing.Point(7, 80);
		this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(19, 19);
		this.label4.TabIndex = 6;
		this.label4.Text = "&L:";
		this.label5.AutoSize = true;
		this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label5.Location = new System.Drawing.Point(7, 51);
		this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(19, 19);
		this.label5.TabIndex = 3;
		this.label5.Text = "&S:";
		this.label9.AutoSize = true;
		this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label9.Location = new System.Drawing.Point(75, 81);
		this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(20, 19);
		this.label9.TabIndex = 8;
		this.label9.Text = "%";
		this.label8.AutoSize = true;
		this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label8.Location = new System.Drawing.Point(75, 52);
		this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(20, 19);
		this.label8.TabIndex = 5;
		this.label8.Text = "%";
		this.label7.AutoSize = true;
		this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label7.Location = new System.Drawing.Point(75, 24);
		this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(14, 19);
		this.label7.TabIndex = 2;
		this.label7.Text = "°";
		this.label6.AutoSize = true;
		this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label6.Location = new System.Drawing.Point(7, 22);
		this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(22, 19);
		this.label6.TabIndex = 0;
		this.label6.Text = "&H:";
		this.htmlTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.htmlTextBox.Location = new System.Drawing.Point(7, 21);
		this.htmlTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.htmlTextBox.Name = "htmlTextBox";
		this.htmlTextBox.Size = new System.Drawing.Size(81, 25);
		this.htmlTextBox.TabIndex = 0;
		this.htmlTextBox.Text = "#0066CC";
		this.htmlTextBox.TextChanged += new System.EventHandler(htmlTextBox_TextChanged);
		this.groupBox3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox3.Controls.Add(this.htmlTextBox);
		this.groupBox3.Location = new System.Drawing.Point(290, 225);
		this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox3.Size = new System.Drawing.Size(95, 50);
		this.groupBox3.TabIndex = 3;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "HTML code";
		this.currentColorPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.currentColorPanel.Controls.Add(this.label10);
		this.currentColorPanel.Location = new System.Drawing.Point(7, 21);
		this.currentColorPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.currentColorPanel.Name = "currentColorPanel";
		this.currentColorPanel.Size = new System.Drawing.Size(81, 44);
		this.currentColorPanel.TabIndex = 4;
		this.currentColorPanel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(currentColorPanel_MouseDoubleClick);
		this.label10.AutoSize = true;
		this.label10.BackColor = System.Drawing.Color.Transparent;
		this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.label10.Location = new System.Drawing.Point(-1, 3);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(85, 38);
		this.label10.TabIndex = 0;
		this.label10.Text = "Double Click\r\nto Close";
		this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label10.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(label10_MouseDoubleClick);
		this.groupBox4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox4.Controls.Add(this.currentColorPanel);
		this.groupBox4.Location = new System.Drawing.Point(290, 332);
		this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox4.Size = new System.Drawing.Size(95, 72);
		this.groupBox4.TabIndex = 3;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "Color";
		this.clarionTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.clarionTextBox.Location = new System.Drawing.Point(7, 21);
		this.clarionTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.clarionTextBox.Name = "clarionTextBox";
		this.clarionTextBox.Size = new System.Drawing.Size(81, 25);
		this.clarionTextBox.TabIndex = 0;
		this.clarionTextBox.Text = "#0066CC";
		this.clarionTextBox.TextChanged += new System.EventHandler(clarionTextBox_TextChanged);
		this.groupBox5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox5.Controls.Add(this.clarionTextBox);
		this.groupBox5.Location = new System.Drawing.Point(290, 279);
		this.groupBox5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.groupBox5.Size = new System.Drawing.Size(95, 51);
		this.groupBox5.TabIndex = 4;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "Clarion code";
		this.colorControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.colorControl.Location = new System.Drawing.Point(0, 0);
		this.colorControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.colorControl.Name = "colorControl";
		this.colorControl.SelectedColor = System.Drawing.Color.FromArgb(20, 0, 0);
		this.colorControl.Size = new System.Drawing.Size(289, 411);
		this.colorControl.TabIndex = 0;
		this.colorControl.ColorChanged += new System.EventHandler(colorControl_ColorChanged);
		this.colorControl.ColorSelected += new System.EventHandler(colorControl_ColorSelected);
		this.colorControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(colorControl_MouseDoubleClick);
		this.colorControl.ValueChangedByUser += new System.EventHandler(colorControl_ValueChangedByUser);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		base.Controls.Add(this.groupBox5);
		base.Controls.Add(this.colorControl);
		base.Controls.Add(this.groupBox4);
		base.Controls.Add(this.groupBox3);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		this.Font = new System.Drawing.Font("Segoe UI", 7.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.MinimumSize = new System.Drawing.Size(379, 411);
		base.Name = "CustomColorEditorUserControl";
		base.Size = new System.Drawing.Size(390, 411);
		base.Load += new System.EventHandler(customColorEditorUserControl_Load);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.bControl).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gControl).EndInit();
		((System.ComponentModel.ISupportInitialize)this.rControl).EndInit();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.lControl).EndInit();
		((System.ComponentModel.ISupportInitialize)this.sControl).EndInit();
		((System.ComponentModel.ISupportInitialize)this.hControl).EndInit();
		this.groupBox3.ResumeLayout(false);
		this.groupBox3.PerformLayout();
		this.currentColorPanel.ResumeLayout(false);
		this.currentColorPanel.PerformLayout();
		this.groupBox4.ResumeLayout(false);
		this.groupBox5.ResumeLayout(false);
		this.groupBox5.PerformLayout();
		base.ResumeLayout(false);
	}
}

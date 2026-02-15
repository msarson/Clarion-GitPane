using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ZetaColorEditor;

public class ColorEditorForm : Form
{
	private Color _selectedColor;

	private IContainer components;

	private ColorEditorUserControl colorEditorControl;

	private Button buttonCancel;

	private Button buttonOK;

	private Button buttonNoColor;

	[Browsable(false)]
	public Color SelectedColor
	{
		get
		{
			return _selectedColor;
		}
		set
		{
			_selectedColor = value;
			if (!base.DesignMode)
			{
				colorEditorControl.SelectedColor = value;
			}
		}
	}

	public string SelectedClarionColor
	{
		get
		{
			return colorEditorControl.SelectedClarionColor;
		}
		set
		{
			colorEditorControl.SelectedClarionColor = value;
		}
	}

	[Browsable(false)]
	public Color Color
	{
		get
		{
			return SelectedColor;
		}
		set
		{
			SelectedColor = value;
		}
	}

	[Browsable(false)]
	public IExternalColorEditorInformationProvider ExternalColorEditorInformationProvider
	{
		get
		{
			return colorEditorControl.ExternalColorEditorInformationProvider;
		}
		set
		{
			colorEditorControl.ExternalColorEditorInformationProvider = value;
		}
	}

	internal string StoreID => $"{GetType().Name}.{base.Name}.{Text}";

	public ColorEditorForm()
	{
		InitializeComponent();
	}

	private void buttonOK_Click(object sender, EventArgs e)
	{
		_selectedColor = colorEditorControl.SelectedColor;
	}

	private void buttonNoColor_Click(object sender, EventArgs e)
	{
		_selectedColor = Color.Empty;
	}

	private void colorEditorForm_Load(object sender, EventArgs e)
	{
		if (ExternalColorEditorInformationProvider != null)
		{
			base.Width = Convert.ToInt32(ExternalColorEditorInformationProvider.RestorePerUserPerWorkstationValue(StoreID + ".Width", base.Width.ToString()));
			base.Height = Convert.ToInt32(ExternalColorEditorInformationProvider.RestorePerUserPerWorkstationValue(StoreID + ".Height", base.Height.ToString()));
		}
		CenterToParent();
		buttonNoColor.Visible = ExternalColorEditorInformationProvider == null || ExternalColorEditorInformationProvider.AllowNoColorSelectable;
	}

	private void colorEditorForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (ExternalColorEditorInformationProvider != null)
		{
			ExternalColorEditorInformationProvider.SavePerUserPerWorkstationValue(StoreID + ".Width", base.Width.ToString());
			ExternalColorEditorInformationProvider.SavePerUserPerWorkstationValue(StoreID + ".Height", base.Height.ToString());
		}
	}

	private void colorEditorUserControl1_NeedUpdateUI(object sender, EventArgs e)
	{
		updateUI();
	}

	private void updateUI()
	{
		buttonOK.Enabled = colorEditorControl.SelectedColor != Color.Empty;
	}

	private void colorEditorControl_ColorSelected(object sender, EventArgs e)
	{
		_selectedColor = colorEditorControl.SelectedColor;
		base.DialogResult = DialogResult.OK;
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
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonOK = new System.Windows.Forms.Button();
		this.buttonNoColor = new System.Windows.Forms.Button();
		this.colorEditorControl = new ZetaColorEditor.ColorEditorUserControl();
		base.SuspendLayout();
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(416, 477);
		this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 29);
		this.buttonCancel.TabIndex = 2;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOK.Location = new System.Drawing.Point(316, 477);
		this.buttonOK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.Size = new System.Drawing.Size(78, 29);
		this.buttonOK.TabIndex = 1;
		this.buttonOK.Text = "Ok";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.buttonOK.Click += new System.EventHandler(buttonOK_Click);
		this.buttonNoColor.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonNoColor.Location = new System.Drawing.Point(18, 477);
		this.buttonNoColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.buttonNoColor.Name = "buttonNoColor";
		this.buttonNoColor.Size = new System.Drawing.Size(98, 29);
		this.buttonNoColor.TabIndex = 0;
		this.buttonNoColor.Text = "No Color";
		this.buttonNoColor.UseVisualStyleBackColor = true;
		this.buttonNoColor.Click += new System.EventHandler(buttonNoColor_Click);
		this.colorEditorControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.colorEditorControl.ExternalColorEditorInformationProvider = null;
		this.colorEditorControl.Location = new System.Drawing.Point(6, 9);
		this.colorEditorControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.colorEditorControl.MaximumSize = new System.Drawing.Size(490, 574);
		this.colorEditorControl.MinimumSize = new System.Drawing.Size(490, 459);
		this.colorEditorControl.Name = "colorEditorControl";
		this.colorEditorControl.SelectedColor = System.Drawing.Color.Empty;
		this.colorEditorControl.Size = new System.Drawing.Size(490, 459);
		this.colorEditorControl.TabIndex = 3;
		this.colorEditorControl.ColorSelected += new System.EventHandler(colorEditorControl_ColorSelected);
		this.colorEditorControl.NeedUpdateUI += new System.EventHandler(colorEditorUserControl1_NeedUpdateUI);
		base.AcceptButton = this.buttonOK;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 20f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.buttonCancel;
		base.ClientSize = new System.Drawing.Size(503, 519);
		base.Controls.Add(this.buttonNoColor);
		base.Controls.Add(this.buttonOK);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.colorEditorControl);
		this.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		this.MaximumSize = new System.Drawing.Size(509, 547);
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(509, 547);
		base.Name = "ColorEditorForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		base.TopMost = true;
		this.Text = "Select a color";
		base.Load += new System.EventHandler(colorEditorForm_Load);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(colorEditorForm_FormClosing);
		base.ResumeLayout(false);
	}
}

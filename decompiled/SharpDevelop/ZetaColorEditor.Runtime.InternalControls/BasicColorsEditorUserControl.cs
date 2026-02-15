using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ZetaColorEditor.Runtime.InternalControls;

public class BasicColorsEditorUserControl : UserControl
{
	private const string CustomColorsPropertyName = "SharpDevelopColorDialog.CustomColors";

	private RadioButton[] userRadioButtons;

	private RadioButton colorToSaveRadioButton;

	private Color _SelectedColor = Color.Empty;

	private int[] _CustomColors;

	private IContainer components;

	private GroupBox basicColorsGroupBox;

	private RadioButton radioButton0;

	private RadioButton radioButton7;

	private RadioButton radioButton6;

	private RadioButton radioButton5;

	private RadioButton radioButton4;

	private RadioButton radioButton3;

	private RadioButton radioButton2;

	private RadioButton radioButton1;

	private RadioButton radioButton8;

	private RadioButton radioButton9;

	private RadioButton radioButton10;

	private RadioButton radioButton11;

	private RadioButton radioButton12;

	private RadioButton radioButton13;

	private RadioButton radioButton14;

	private RadioButton radioButton15;

	private RadioButton radioButton40;

	private RadioButton radioButton41;

	private RadioButton radioButton42;

	private RadioButton radioButton43;

	private RadioButton radioButton44;

	private RadioButton radioButton45;

	private RadioButton radioButton46;

	private RadioButton radioButton47;

	private RadioButton radioButton32;

	private RadioButton radioButton33;

	private RadioButton radioButton34;

	private RadioButton radioButton35;

	private RadioButton radioButton36;

	private RadioButton radioButton37;

	private RadioButton radioButton38;

	private RadioButton radioButton39;

	private RadioButton radioButton24;

	private RadioButton radioButton25;

	private RadioButton radioButton26;

	private RadioButton radioButton27;

	private RadioButton radioButton28;

	private RadioButton radioButton29;

	private RadioButton radioButton30;

	private RadioButton radioButton31;

	private RadioButton radioButton16;

	private RadioButton radioButton17;

	private RadioButton radioButton18;

	private RadioButton radioButton19;

	private RadioButton radioButton20;

	private RadioButton radioButton21;

	private RadioButton radioButton22;

	private RadioButton radioButton23;

	private RadioButton radioButton48;

	private RadioButton radioButton49;

	private RadioButton radioButton50;

	private RadioButton radioButton51;

	private RadioButton radioButton52;

	private RadioButton radioButton53;

	private RadioButton radioButton54;

	private RadioButton radioButton55;

	private RadioButton radioButton56;

	private RadioButton radioButton57;

	private RadioButton radioButton58;

	private RadioButton radioButton59;

	private RadioButton radioButton60;

	private RadioButton radioButton61;

	private RadioButton radioButton62;

	private RadioButton radioButton63;

	private Label label1;

	[Browsable(false)]
	public Color SelectedColor
	{
		get
		{
			if (base.DesignMode)
			{
				return Color.Empty;
			}
			return _SelectedColor;
		}
		set
		{
			if (base.DesignMode)
			{
				return;
			}
			_SelectedColor = Color.White;
			if (value == Color.White)
			{
				_SelectedColor = value;
				radioButton0.Select();
				return;
			}
			foreach (Control control in basicColorsGroupBox.Controls)
			{
				if (control is RadioButton && control.BackColor == value)
				{
					_SelectedColor = value;
					control.Select();
					break;
				}
			}
		}
	}

	public int[] CustomColors
	{
		get
		{
			return _CustomColors;
		}
		set
		{
			_CustomColors = value;
		}
	}

	public event EventHandler NeedUpdateUI;

	public event EventHandler UserColorRequested;

	public event EventHandler ColorSelected;

	public BasicColorsEditorUserControl()
	{
		InitializeComponent();
	}

	private void DoNeedUpdateUI()
	{
		if (this.NeedUpdateUI != null)
		{
			this.NeedUpdateUI(this, EventArgs.Empty);
		}
	}

	private void BasicColorsEditorUserControl_Load(object sender, EventArgs e)
	{
		userRadioButtons = new RadioButton[16]
		{
			radioButton48, radioButton49, radioButton50, radioButton51, radioButton52, radioButton53, radioButton54, radioButton55, radioButton56, radioButton57,
			radioButton58, radioButton59, radioButton60, radioButton61, radioButton62, radioButton63
		};
		LoadCustomColors();
		radioButton0.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton1.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton2.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton3.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton4.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton5.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton6.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton7.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton8.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton9.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton10.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton11.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton12.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton13.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton14.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton15.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton16.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton17.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton18.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton19.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton20.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton21.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton22.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton23.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton24.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton25.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton26.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton27.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton28.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton29.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton30.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton31.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton32.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton33.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton34.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton35.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton36.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton37.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton38.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton39.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton40.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton41.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton42.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton43.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton44.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton45.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton46.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton47.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton48.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton49.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton50.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton51.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton52.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton53.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton54.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton55.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton56.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton57.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton58.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton59.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton60.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton61.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton62.GotFocus += OnRadioButtonColor_GotFocus;
		radioButton63.GotFocus += OnRadioButtonColor_GotFocus;
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.Return)
		{
			_SelectedColor = GetSelectedControlColor();
			DoColorSelected();
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void OnRadioButtonColor_GotFocus(object sender, EventArgs e)
	{
		colorToSaveRadioButton = null;
		_SelectedColor = GetSelectedControlColor();
		DoNeedUpdateUI();
	}

	private void radioButtonColor_MouseClick(object sender, MouseEventArgs e)
	{
		colorToSaveRadioButton = null;
		if (e.Button == MouseButtons.Left)
		{
			RadioButton radioButton = (RadioButton)sender;
			if (radioButton != null)
			{
				_SelectedColor = radioButton.BackColor;
				DoColorSelected();
			}
		}
	}

	private void radioButtonUserColor_MouseUp(object sender, MouseEventArgs e)
	{
		colorToSaveRadioButton = null;
		RadioButton radioButton = (RadioButton)sender;
		if (radioButton != null && e.Button == MouseButtons.Right)
		{
			colorToSaveRadioButton = radioButton;
			DoUserColorRequested();
		}
	}

	public bool ContainsColor(Color value)
	{
		foreach (Control control in basicColorsGroupBox.Controls)
		{
			if (control is RadioButton && control.BackColor == value)
			{
				return true;
			}
		}
		return false;
	}

	private void radioButtonUserColor_MouseClick(object sender, MouseEventArgs e)
	{
		colorToSaveRadioButton = null;
		RadioButton radioButton = (RadioButton)sender;
		if (radioButton != null && e.Button == MouseButtons.Left)
		{
			_SelectedColor = radioButton.BackColor;
			DoColorSelected();
		}
	}

	public void SaveColor(Color colorToSave)
	{
		if (colorToSaveRadioButton != null)
		{
			colorToSaveRadioButton.BackColor = colorToSave;
			SaveCustomColors();
		}
	}

	private void DoUserColorRequested()
	{
		if (this.UserColorRequested != null)
		{
			this.UserColorRequested(this, EventArgs.Empty);
		}
	}

	private void DoColorSelected()
	{
		if (this.ColorSelected != null)
		{
			SaveCustomColors();
			this.ColorSelected(this, EventArgs.Empty);
		}
	}

	[Browsable(false)]
	public Color GetSelectedControlColor()
	{
		foreach (Control control in basicColorsGroupBox.Controls)
		{
			if (control is RadioButton && control.Focused)
			{
				return control.BackColor;
			}
		}
		return Color.Empty;
	}

	public static int[] CustomColorsFromString(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return null;
		}
		string[] array = s.Split('|');
		List<int> list = new List<int>();
		string[] array2 = array;
		foreach (string s2 in array2)
		{
			if (int.TryParse(s2, out var result))
			{
				list.Add(result);
			}
		}
		return list.ToArray();
	}

	public static string CustomColorsToString(int[] colors)
	{
		if (colors == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < colors.Length; i++)
		{
			if (i != 0)
			{
				stringBuilder.Append('|');
			}
			stringBuilder.Append(colors[i]);
		}
		return stringBuilder.ToString();
	}

	private void LoadCustomColors()
	{
		_CustomColors = CustomColorsFromString(PropertyService.Get("SharpDevelopColorDialog.CustomColors"));
		if (_CustomColors != null)
		{
			for (int i = 0; i < _CustomColors.Length; i++)
			{
				if (i < 16)
				{
					userRadioButtons[i].BackColor = Color.FromArgb(_CustomColors[i]);
				}
			}
			return;
		}
		for (int j = 0; j < 15; j++)
		{
			if (j < 16)
			{
				userRadioButtons[j].BackColor = Color.White;
			}
		}
	}

	private void SaveCustomColors()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < 16; i++)
		{
			if (userRadioButtons[i].BackColor != Color.White)
			{
				list.Add(userRadioButtons[i].BackColor.ToArgb());
			}
		}
		CustomColors = list.ToArray();
		PropertyService.Set("SharpDevelopColorDialog.CustomColors", CustomColorsToString(CustomColors));
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
		this.basicColorsGroupBox = new System.Windows.Forms.GroupBox();
		this.label1 = new System.Windows.Forms.Label();
		this.radioButton48 = new System.Windows.Forms.RadioButton();
		this.radioButton49 = new System.Windows.Forms.RadioButton();
		this.radioButton50 = new System.Windows.Forms.RadioButton();
		this.radioButton51 = new System.Windows.Forms.RadioButton();
		this.radioButton52 = new System.Windows.Forms.RadioButton();
		this.radioButton53 = new System.Windows.Forms.RadioButton();
		this.radioButton54 = new System.Windows.Forms.RadioButton();
		this.radioButton55 = new System.Windows.Forms.RadioButton();
		this.radioButton56 = new System.Windows.Forms.RadioButton();
		this.radioButton57 = new System.Windows.Forms.RadioButton();
		this.radioButton58 = new System.Windows.Forms.RadioButton();
		this.radioButton59 = new System.Windows.Forms.RadioButton();
		this.radioButton60 = new System.Windows.Forms.RadioButton();
		this.radioButton61 = new System.Windows.Forms.RadioButton();
		this.radioButton62 = new System.Windows.Forms.RadioButton();
		this.radioButton63 = new System.Windows.Forms.RadioButton();
		this.radioButton40 = new System.Windows.Forms.RadioButton();
		this.radioButton41 = new System.Windows.Forms.RadioButton();
		this.radioButton42 = new System.Windows.Forms.RadioButton();
		this.radioButton43 = new System.Windows.Forms.RadioButton();
		this.radioButton44 = new System.Windows.Forms.RadioButton();
		this.radioButton45 = new System.Windows.Forms.RadioButton();
		this.radioButton46 = new System.Windows.Forms.RadioButton();
		this.radioButton47 = new System.Windows.Forms.RadioButton();
		this.radioButton32 = new System.Windows.Forms.RadioButton();
		this.radioButton33 = new System.Windows.Forms.RadioButton();
		this.radioButton34 = new System.Windows.Forms.RadioButton();
		this.radioButton35 = new System.Windows.Forms.RadioButton();
		this.radioButton36 = new System.Windows.Forms.RadioButton();
		this.radioButton37 = new System.Windows.Forms.RadioButton();
		this.radioButton38 = new System.Windows.Forms.RadioButton();
		this.radioButton39 = new System.Windows.Forms.RadioButton();
		this.radioButton24 = new System.Windows.Forms.RadioButton();
		this.radioButton25 = new System.Windows.Forms.RadioButton();
		this.radioButton26 = new System.Windows.Forms.RadioButton();
		this.radioButton27 = new System.Windows.Forms.RadioButton();
		this.radioButton28 = new System.Windows.Forms.RadioButton();
		this.radioButton29 = new System.Windows.Forms.RadioButton();
		this.radioButton30 = new System.Windows.Forms.RadioButton();
		this.radioButton31 = new System.Windows.Forms.RadioButton();
		this.radioButton16 = new System.Windows.Forms.RadioButton();
		this.radioButton17 = new System.Windows.Forms.RadioButton();
		this.radioButton18 = new System.Windows.Forms.RadioButton();
		this.radioButton19 = new System.Windows.Forms.RadioButton();
		this.radioButton20 = new System.Windows.Forms.RadioButton();
		this.radioButton21 = new System.Windows.Forms.RadioButton();
		this.radioButton22 = new System.Windows.Forms.RadioButton();
		this.radioButton23 = new System.Windows.Forms.RadioButton();
		this.radioButton8 = new System.Windows.Forms.RadioButton();
		this.radioButton9 = new System.Windows.Forms.RadioButton();
		this.radioButton10 = new System.Windows.Forms.RadioButton();
		this.radioButton11 = new System.Windows.Forms.RadioButton();
		this.radioButton12 = new System.Windows.Forms.RadioButton();
		this.radioButton13 = new System.Windows.Forms.RadioButton();
		this.radioButton14 = new System.Windows.Forms.RadioButton();
		this.radioButton15 = new System.Windows.Forms.RadioButton();
		this.radioButton7 = new System.Windows.Forms.RadioButton();
		this.radioButton6 = new System.Windows.Forms.RadioButton();
		this.radioButton5 = new System.Windows.Forms.RadioButton();
		this.radioButton4 = new System.Windows.Forms.RadioButton();
		this.radioButton3 = new System.Windows.Forms.RadioButton();
		this.radioButton2 = new System.Windows.Forms.RadioButton();
		this.radioButton1 = new System.Windows.Forms.RadioButton();
		this.radioButton0 = new System.Windows.Forms.RadioButton();
		this.basicColorsGroupBox.SuspendLayout();
		base.SuspendLayout();
		this.basicColorsGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.basicColorsGroupBox.Controls.Add(this.label1);
		this.basicColorsGroupBox.Controls.Add(this.radioButton48);
		this.basicColorsGroupBox.Controls.Add(this.radioButton49);
		this.basicColorsGroupBox.Controls.Add(this.radioButton50);
		this.basicColorsGroupBox.Controls.Add(this.radioButton51);
		this.basicColorsGroupBox.Controls.Add(this.radioButton52);
		this.basicColorsGroupBox.Controls.Add(this.radioButton53);
		this.basicColorsGroupBox.Controls.Add(this.radioButton54);
		this.basicColorsGroupBox.Controls.Add(this.radioButton55);
		this.basicColorsGroupBox.Controls.Add(this.radioButton56);
		this.basicColorsGroupBox.Controls.Add(this.radioButton57);
		this.basicColorsGroupBox.Controls.Add(this.radioButton58);
		this.basicColorsGroupBox.Controls.Add(this.radioButton59);
		this.basicColorsGroupBox.Controls.Add(this.radioButton60);
		this.basicColorsGroupBox.Controls.Add(this.radioButton61);
		this.basicColorsGroupBox.Controls.Add(this.radioButton62);
		this.basicColorsGroupBox.Controls.Add(this.radioButton63);
		this.basicColorsGroupBox.Controls.Add(this.radioButton40);
		this.basicColorsGroupBox.Controls.Add(this.radioButton41);
		this.basicColorsGroupBox.Controls.Add(this.radioButton42);
		this.basicColorsGroupBox.Controls.Add(this.radioButton43);
		this.basicColorsGroupBox.Controls.Add(this.radioButton44);
		this.basicColorsGroupBox.Controls.Add(this.radioButton45);
		this.basicColorsGroupBox.Controls.Add(this.radioButton46);
		this.basicColorsGroupBox.Controls.Add(this.radioButton47);
		this.basicColorsGroupBox.Controls.Add(this.radioButton32);
		this.basicColorsGroupBox.Controls.Add(this.radioButton33);
		this.basicColorsGroupBox.Controls.Add(this.radioButton34);
		this.basicColorsGroupBox.Controls.Add(this.radioButton35);
		this.basicColorsGroupBox.Controls.Add(this.radioButton36);
		this.basicColorsGroupBox.Controls.Add(this.radioButton37);
		this.basicColorsGroupBox.Controls.Add(this.radioButton38);
		this.basicColorsGroupBox.Controls.Add(this.radioButton39);
		this.basicColorsGroupBox.Controls.Add(this.radioButton24);
		this.basicColorsGroupBox.Controls.Add(this.radioButton25);
		this.basicColorsGroupBox.Controls.Add(this.radioButton26);
		this.basicColorsGroupBox.Controls.Add(this.radioButton27);
		this.basicColorsGroupBox.Controls.Add(this.radioButton28);
		this.basicColorsGroupBox.Controls.Add(this.radioButton29);
		this.basicColorsGroupBox.Controls.Add(this.radioButton30);
		this.basicColorsGroupBox.Controls.Add(this.radioButton31);
		this.basicColorsGroupBox.Controls.Add(this.radioButton16);
		this.basicColorsGroupBox.Controls.Add(this.radioButton17);
		this.basicColorsGroupBox.Controls.Add(this.radioButton18);
		this.basicColorsGroupBox.Controls.Add(this.radioButton19);
		this.basicColorsGroupBox.Controls.Add(this.radioButton20);
		this.basicColorsGroupBox.Controls.Add(this.radioButton21);
		this.basicColorsGroupBox.Controls.Add(this.radioButton22);
		this.basicColorsGroupBox.Controls.Add(this.radioButton23);
		this.basicColorsGroupBox.Controls.Add(this.radioButton8);
		this.basicColorsGroupBox.Controls.Add(this.radioButton9);
		this.basicColorsGroupBox.Controls.Add(this.radioButton10);
		this.basicColorsGroupBox.Controls.Add(this.radioButton11);
		this.basicColorsGroupBox.Controls.Add(this.radioButton12);
		this.basicColorsGroupBox.Controls.Add(this.radioButton13);
		this.basicColorsGroupBox.Controls.Add(this.radioButton14);
		this.basicColorsGroupBox.Controls.Add(this.radioButton15);
		this.basicColorsGroupBox.Controls.Add(this.radioButton7);
		this.basicColorsGroupBox.Controls.Add(this.radioButton6);
		this.basicColorsGroupBox.Controls.Add(this.radioButton5);
		this.basicColorsGroupBox.Controls.Add(this.radioButton4);
		this.basicColorsGroupBox.Controls.Add(this.radioButton3);
		this.basicColorsGroupBox.Controls.Add(this.radioButton2);
		this.basicColorsGroupBox.Controls.Add(this.radioButton1);
		this.basicColorsGroupBox.Controls.Add(this.radioButton0);
		this.basicColorsGroupBox.Location = new System.Drawing.Point(3, 3);
		this.basicColorsGroupBox.MaximumSize = new System.Drawing.Size(375, 352);
		this.basicColorsGroupBox.MinimumSize = new System.Drawing.Size(375, 352);
		this.basicColorsGroupBox.Name = "basicColorsGroupBox";
		this.basicColorsGroupBox.Size = new System.Drawing.Size(375, 352);
		this.basicColorsGroupBox.TabIndex = 0;
		this.basicColorsGroupBox.TabStop = false;
		this.basicColorsGroupBox.Text = "Basic Colors";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(28, 252);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(91, 17);
		this.label1.TabIndex = 64;
		this.label1.Text = "User Defined";
		this.radioButton48.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton48.BackColor = System.Drawing.Color.White;
		this.radioButton48.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton48.Location = new System.Drawing.Point(31, 281);
		this.radioButton48.Name = "radioButton48";
		this.radioButton48.Size = new System.Drawing.Size(30, 28);
		this.radioButton48.TabIndex = 48;
		this.radioButton48.TabStop = true;
		this.radioButton48.UseVisualStyleBackColor = false;
		this.radioButton48.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton48.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton49.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton49.BackColor = System.Drawing.Color.White;
		this.radioButton49.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton49.Location = new System.Drawing.Point(71, 281);
		this.radioButton49.Name = "radioButton49";
		this.radioButton49.Size = new System.Drawing.Size(30, 28);
		this.radioButton49.TabIndex = 49;
		this.radioButton49.TabStop = true;
		this.radioButton49.UseVisualStyleBackColor = false;
		this.radioButton49.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton49.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton50.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton50.BackColor = System.Drawing.Color.White;
		this.radioButton50.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton50.Location = new System.Drawing.Point(112, 281);
		this.radioButton50.Name = "radioButton50";
		this.radioButton50.Size = new System.Drawing.Size(30, 28);
		this.radioButton50.TabIndex = 50;
		this.radioButton50.TabStop = true;
		this.radioButton50.UseVisualStyleBackColor = false;
		this.radioButton50.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton50.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton51.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton51.BackColor = System.Drawing.Color.White;
		this.radioButton51.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton51.Location = new System.Drawing.Point(153, 281);
		this.radioButton51.Name = "radioButton51";
		this.radioButton51.Size = new System.Drawing.Size(30, 28);
		this.radioButton51.TabIndex = 51;
		this.radioButton51.TabStop = true;
		this.radioButton51.UseVisualStyleBackColor = false;
		this.radioButton51.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton51.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton52.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton52.BackColor = System.Drawing.Color.White;
		this.radioButton52.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton52.Location = new System.Drawing.Point(194, 281);
		this.radioButton52.Name = "radioButton52";
		this.radioButton52.Size = new System.Drawing.Size(30, 28);
		this.radioButton52.TabIndex = 52;
		this.radioButton52.TabStop = true;
		this.radioButton52.UseVisualStyleBackColor = false;
		this.radioButton52.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton52.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton53.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton53.BackColor = System.Drawing.Color.White;
		this.radioButton53.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton53.Location = new System.Drawing.Point(235, 281);
		this.radioButton53.Name = "radioButton53";
		this.radioButton53.Size = new System.Drawing.Size(30, 28);
		this.radioButton53.TabIndex = 53;
		this.radioButton53.TabStop = true;
		this.radioButton53.UseVisualStyleBackColor = false;
		this.radioButton53.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton53.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton54.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton54.BackColor = System.Drawing.Color.White;
		this.radioButton54.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton54.Location = new System.Drawing.Point(276, 281);
		this.radioButton54.Name = "radioButton54";
		this.radioButton54.Size = new System.Drawing.Size(30, 28);
		this.radioButton54.TabIndex = 54;
		this.radioButton54.TabStop = true;
		this.radioButton54.UseVisualStyleBackColor = false;
		this.radioButton54.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton54.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton55.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton55.BackColor = System.Drawing.Color.White;
		this.radioButton55.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton55.Location = new System.Drawing.Point(317, 281);
		this.radioButton55.Name = "radioButton55";
		this.radioButton55.Size = new System.Drawing.Size(30, 28);
		this.radioButton55.TabIndex = 55;
		this.radioButton55.TabStop = true;
		this.radioButton55.UseVisualStyleBackColor = false;
		this.radioButton55.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton55.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton56.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton56.BackColor = System.Drawing.Color.White;
		this.radioButton56.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton56.Location = new System.Drawing.Point(31, 315);
		this.radioButton56.Name = "radioButton56";
		this.radioButton56.Size = new System.Drawing.Size(30, 28);
		this.radioButton56.TabIndex = 56;
		this.radioButton56.TabStop = true;
		this.radioButton56.UseVisualStyleBackColor = false;
		this.radioButton56.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton56.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton57.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton57.BackColor = System.Drawing.Color.White;
		this.radioButton57.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton57.Location = new System.Drawing.Point(71, 315);
		this.radioButton57.Name = "radioButton57";
		this.radioButton57.Size = new System.Drawing.Size(30, 28);
		this.radioButton57.TabIndex = 57;
		this.radioButton57.TabStop = true;
		this.radioButton57.UseVisualStyleBackColor = false;
		this.radioButton57.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton57.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton58.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton58.BackColor = System.Drawing.Color.White;
		this.radioButton58.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton58.Location = new System.Drawing.Point(112, 315);
		this.radioButton58.Name = "radioButton58";
		this.radioButton58.Size = new System.Drawing.Size(30, 28);
		this.radioButton58.TabIndex = 58;
		this.radioButton58.TabStop = true;
		this.radioButton58.UseVisualStyleBackColor = false;
		this.radioButton58.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton58.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton59.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton59.BackColor = System.Drawing.Color.White;
		this.radioButton59.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton59.Location = new System.Drawing.Point(153, 315);
		this.radioButton59.Name = "radioButton59";
		this.radioButton59.Size = new System.Drawing.Size(30, 28);
		this.radioButton59.TabIndex = 59;
		this.radioButton59.TabStop = true;
		this.radioButton59.UseVisualStyleBackColor = false;
		this.radioButton59.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton59.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton60.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton60.BackColor = System.Drawing.Color.White;
		this.radioButton60.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton60.Location = new System.Drawing.Point(194, 315);
		this.radioButton60.Name = "radioButton60";
		this.radioButton60.Size = new System.Drawing.Size(30, 28);
		this.radioButton60.TabIndex = 60;
		this.radioButton60.TabStop = true;
		this.radioButton60.UseVisualStyleBackColor = false;
		this.radioButton60.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton60.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton61.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton61.BackColor = System.Drawing.Color.White;
		this.radioButton61.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton61.Location = new System.Drawing.Point(235, 315);
		this.radioButton61.Name = "radioButton61";
		this.radioButton61.Size = new System.Drawing.Size(30, 28);
		this.radioButton61.TabIndex = 61;
		this.radioButton61.TabStop = true;
		this.radioButton61.UseVisualStyleBackColor = false;
		this.radioButton61.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton61.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton62.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton62.BackColor = System.Drawing.Color.White;
		this.radioButton62.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton62.Location = new System.Drawing.Point(276, 315);
		this.radioButton62.Name = "radioButton62";
		this.radioButton62.Size = new System.Drawing.Size(30, 28);
		this.radioButton62.TabIndex = 62;
		this.radioButton62.TabStop = true;
		this.radioButton62.UseVisualStyleBackColor = false;
		this.radioButton62.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton62.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton63.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton63.BackColor = System.Drawing.Color.White;
		this.radioButton63.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton63.Location = new System.Drawing.Point(317, 315);
		this.radioButton63.Name = "radioButton63";
		this.radioButton63.Size = new System.Drawing.Size(30, 28);
		this.radioButton63.TabIndex = 63;
		this.radioButton63.TabStop = true;
		this.radioButton63.UseVisualStyleBackColor = false;
		this.radioButton63.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseClick);
		this.radioButton63.MouseUp += new System.Windows.Forms.MouseEventHandler(radioButtonUserColor_MouseUp);
		this.radioButton40.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton40.BackColor = System.Drawing.Color.Black;
		this.radioButton40.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton40.Location = new System.Drawing.Point(31, 210);
		this.radioButton40.Name = "radioButton40";
		this.radioButton40.Size = new System.Drawing.Size(30, 28);
		this.radioButton40.TabIndex = 40;
		this.radioButton40.TabStop = true;
		this.radioButton40.UseVisualStyleBackColor = false;
		this.radioButton40.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton41.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton41.BackColor = System.Drawing.Color.FromArgb(64, 0, 0);
		this.radioButton41.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton41.Location = new System.Drawing.Point(71, 210);
		this.radioButton41.Name = "radioButton41";
		this.radioButton41.Size = new System.Drawing.Size(30, 28);
		this.radioButton41.TabIndex = 41;
		this.radioButton41.TabStop = true;
		this.radioButton41.UseVisualStyleBackColor = false;
		this.radioButton41.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton42.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton42.BackColor = System.Drawing.Color.FromArgb(128, 64, 64);
		this.radioButton42.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton42.Location = new System.Drawing.Point(112, 210);
		this.radioButton42.Name = "radioButton42";
		this.radioButton42.Size = new System.Drawing.Size(30, 28);
		this.radioButton42.TabIndex = 42;
		this.radioButton42.TabStop = true;
		this.radioButton42.UseVisualStyleBackColor = false;
		this.radioButton42.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton43.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton43.BackColor = System.Drawing.Color.FromArgb(64, 64, 0);
		this.radioButton43.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton43.Location = new System.Drawing.Point(153, 210);
		this.radioButton43.Name = "radioButton43";
		this.radioButton43.Size = new System.Drawing.Size(30, 28);
		this.radioButton43.TabIndex = 43;
		this.radioButton43.TabStop = true;
		this.radioButton43.UseVisualStyleBackColor = false;
		this.radioButton43.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton44.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton44.BackColor = System.Drawing.Color.FromArgb(0, 64, 0);
		this.radioButton44.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton44.Location = new System.Drawing.Point(194, 210);
		this.radioButton44.Name = "radioButton44";
		this.radioButton44.Size = new System.Drawing.Size(30, 28);
		this.radioButton44.TabIndex = 44;
		this.radioButton44.TabStop = true;
		this.radioButton44.UseVisualStyleBackColor = false;
		this.radioButton44.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton45.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton45.BackColor = System.Drawing.Color.FromArgb(0, 64, 64);
		this.radioButton45.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton45.Location = new System.Drawing.Point(235, 210);
		this.radioButton45.Name = "radioButton45";
		this.radioButton45.Size = new System.Drawing.Size(30, 28);
		this.radioButton45.TabIndex = 45;
		this.radioButton45.TabStop = true;
		this.radioButton45.UseVisualStyleBackColor = false;
		this.radioButton45.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton46.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton46.BackColor = System.Drawing.Color.FromArgb(0, 0, 64);
		this.radioButton46.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton46.Location = new System.Drawing.Point(276, 210);
		this.radioButton46.Name = "radioButton46";
		this.radioButton46.Size = new System.Drawing.Size(30, 28);
		this.radioButton46.TabIndex = 46;
		this.radioButton46.TabStop = true;
		this.radioButton46.UseVisualStyleBackColor = false;
		this.radioButton46.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton47.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton47.BackColor = System.Drawing.Color.FromArgb(64, 0, 64);
		this.radioButton47.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton47.Location = new System.Drawing.Point(317, 210);
		this.radioButton47.Name = "radioButton47";
		this.radioButton47.Size = new System.Drawing.Size(30, 28);
		this.radioButton47.TabIndex = 47;
		this.radioButton47.TabStop = true;
		this.radioButton47.UseVisualStyleBackColor = false;
		this.radioButton47.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton32.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton32.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.radioButton32.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton32.Location = new System.Drawing.Point(31, 176);
		this.radioButton32.Name = "radioButton32";
		this.radioButton32.Size = new System.Drawing.Size(30, 28);
		this.radioButton32.TabIndex = 32;
		this.radioButton32.TabStop = true;
		this.radioButton32.UseVisualStyleBackColor = false;
		this.radioButton32.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton33.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton33.BackColor = System.Drawing.Color.Maroon;
		this.radioButton33.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton33.Location = new System.Drawing.Point(71, 176);
		this.radioButton33.Name = "radioButton33";
		this.radioButton33.Size = new System.Drawing.Size(30, 28);
		this.radioButton33.TabIndex = 33;
		this.radioButton33.TabStop = true;
		this.radioButton33.UseVisualStyleBackColor = false;
		this.radioButton33.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton34.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton34.BackColor = System.Drawing.Color.FromArgb(128, 64, 0);
		this.radioButton34.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton34.Location = new System.Drawing.Point(112, 176);
		this.radioButton34.Name = "radioButton34";
		this.radioButton34.Size = new System.Drawing.Size(30, 28);
		this.radioButton34.TabIndex = 34;
		this.radioButton34.TabStop = true;
		this.radioButton34.UseVisualStyleBackColor = false;
		this.radioButton34.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton35.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton35.BackColor = System.Drawing.Color.Olive;
		this.radioButton35.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton35.Location = new System.Drawing.Point(153, 176);
		this.radioButton35.Name = "radioButton35";
		this.radioButton35.Size = new System.Drawing.Size(30, 28);
		this.radioButton35.TabIndex = 35;
		this.radioButton35.TabStop = true;
		this.radioButton35.UseVisualStyleBackColor = false;
		this.radioButton35.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton36.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton36.BackColor = System.Drawing.Color.Green;
		this.radioButton36.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton36.Location = new System.Drawing.Point(194, 176);
		this.radioButton36.Name = "radioButton36";
		this.radioButton36.Size = new System.Drawing.Size(30, 28);
		this.radioButton36.TabIndex = 36;
		this.radioButton36.TabStop = true;
		this.radioButton36.UseVisualStyleBackColor = false;
		this.radioButton36.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton37.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton37.BackColor = System.Drawing.Color.Teal;
		this.radioButton37.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton37.Location = new System.Drawing.Point(235, 176);
		this.radioButton37.Name = "radioButton37";
		this.radioButton37.Size = new System.Drawing.Size(30, 28);
		this.radioButton37.TabIndex = 37;
		this.radioButton37.TabStop = true;
		this.radioButton37.UseVisualStyleBackColor = false;
		this.radioButton37.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton38.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton38.BackColor = System.Drawing.Color.Navy;
		this.radioButton38.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton38.Location = new System.Drawing.Point(276, 176);
		this.radioButton38.Name = "radioButton38";
		this.radioButton38.Size = new System.Drawing.Size(30, 28);
		this.radioButton38.TabIndex = 38;
		this.radioButton38.TabStop = true;
		this.radioButton38.UseVisualStyleBackColor = false;
		this.radioButton38.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton39.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton39.BackColor = System.Drawing.Color.Purple;
		this.radioButton39.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton39.Location = new System.Drawing.Point(317, 176);
		this.radioButton39.Name = "radioButton39";
		this.radioButton39.Size = new System.Drawing.Size(30, 28);
		this.radioButton39.TabIndex = 39;
		this.radioButton39.TabStop = true;
		this.radioButton39.UseVisualStyleBackColor = false;
		this.radioButton39.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton24.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton24.BackColor = System.Drawing.Color.Gray;
		this.radioButton24.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton24.Location = new System.Drawing.Point(31, 142);
		this.radioButton24.Name = "radioButton24";
		this.radioButton24.Size = new System.Drawing.Size(30, 28);
		this.radioButton24.TabIndex = 24;
		this.radioButton24.TabStop = true;
		this.radioButton24.UseVisualStyleBackColor = false;
		this.radioButton24.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton25.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton25.BackColor = System.Drawing.Color.FromArgb(192, 0, 0);
		this.radioButton25.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton25.Location = new System.Drawing.Point(71, 142);
		this.radioButton25.Name = "radioButton25";
		this.radioButton25.Size = new System.Drawing.Size(30, 28);
		this.radioButton25.TabIndex = 25;
		this.radioButton25.TabStop = true;
		this.radioButton25.UseVisualStyleBackColor = false;
		this.radioButton25.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton26.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton26.BackColor = System.Drawing.Color.FromArgb(192, 64, 0);
		this.radioButton26.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton26.Location = new System.Drawing.Point(112, 142);
		this.radioButton26.Name = "radioButton26";
		this.radioButton26.Size = new System.Drawing.Size(30, 28);
		this.radioButton26.TabIndex = 26;
		this.radioButton26.TabStop = true;
		this.radioButton26.UseVisualStyleBackColor = false;
		this.radioButton26.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton27.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton27.BackColor = System.Drawing.Color.FromArgb(192, 192, 0);
		this.radioButton27.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton27.Location = new System.Drawing.Point(153, 142);
		this.radioButton27.Name = "radioButton27";
		this.radioButton27.Size = new System.Drawing.Size(30, 28);
		this.radioButton27.TabIndex = 27;
		this.radioButton27.TabStop = true;
		this.radioButton27.UseVisualStyleBackColor = false;
		this.radioButton27.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton28.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton28.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.radioButton28.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton28.Location = new System.Drawing.Point(194, 142);
		this.radioButton28.Name = "radioButton28";
		this.radioButton28.Size = new System.Drawing.Size(30, 28);
		this.radioButton28.TabIndex = 28;
		this.radioButton28.TabStop = true;
		this.radioButton28.UseVisualStyleBackColor = false;
		this.radioButton28.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton29.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton29.BackColor = System.Drawing.Color.FromArgb(0, 192, 192);
		this.radioButton29.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton29.Location = new System.Drawing.Point(235, 142);
		this.radioButton29.Name = "radioButton29";
		this.radioButton29.Size = new System.Drawing.Size(30, 28);
		this.radioButton29.TabIndex = 29;
		this.radioButton29.TabStop = true;
		this.radioButton29.UseVisualStyleBackColor = false;
		this.radioButton29.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton30.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton30.BackColor = System.Drawing.Color.FromArgb(0, 0, 192);
		this.radioButton30.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton30.Location = new System.Drawing.Point(276, 142);
		this.radioButton30.Name = "radioButton30";
		this.radioButton30.Size = new System.Drawing.Size(30, 28);
		this.radioButton30.TabIndex = 30;
		this.radioButton30.TabStop = true;
		this.radioButton30.UseVisualStyleBackColor = false;
		this.radioButton30.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton31.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton31.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.radioButton31.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton31.Location = new System.Drawing.Point(317, 142);
		this.radioButton31.Name = "radioButton31";
		this.radioButton31.Size = new System.Drawing.Size(30, 28);
		this.radioButton31.TabIndex = 31;
		this.radioButton31.TabStop = true;
		this.radioButton31.UseVisualStyleBackColor = false;
		this.radioButton31.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton16.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton16.BackColor = System.Drawing.Color.Silver;
		this.radioButton16.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton16.Location = new System.Drawing.Point(31, 108);
		this.radioButton16.Name = "radioButton16";
		this.radioButton16.Size = new System.Drawing.Size(30, 28);
		this.radioButton16.TabIndex = 16;
		this.radioButton16.TabStop = true;
		this.radioButton16.UseVisualStyleBackColor = false;
		this.radioButton16.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton17.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton17.BackColor = System.Drawing.Color.Red;
		this.radioButton17.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton17.Location = new System.Drawing.Point(71, 108);
		this.radioButton17.Name = "radioButton17";
		this.radioButton17.Size = new System.Drawing.Size(30, 28);
		this.radioButton17.TabIndex = 17;
		this.radioButton17.TabStop = true;
		this.radioButton17.UseVisualStyleBackColor = false;
		this.radioButton17.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton18.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton18.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.radioButton18.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton18.Location = new System.Drawing.Point(112, 108);
		this.radioButton18.Name = "radioButton18";
		this.radioButton18.Size = new System.Drawing.Size(30, 28);
		this.radioButton18.TabIndex = 18;
		this.radioButton18.TabStop = true;
		this.radioButton18.UseVisualStyleBackColor = false;
		this.radioButton18.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton19.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton19.BackColor = System.Drawing.Color.Yellow;
		this.radioButton19.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton19.Location = new System.Drawing.Point(153, 108);
		this.radioButton19.Name = "radioButton19";
		this.radioButton19.Size = new System.Drawing.Size(30, 28);
		this.radioButton19.TabIndex = 19;
		this.radioButton19.TabStop = true;
		this.radioButton19.UseVisualStyleBackColor = false;
		this.radioButton19.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton20.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton20.BackColor = System.Drawing.Color.Lime;
		this.radioButton20.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton20.Location = new System.Drawing.Point(194, 108);
		this.radioButton20.Name = "radioButton20";
		this.radioButton20.Size = new System.Drawing.Size(30, 28);
		this.radioButton20.TabIndex = 20;
		this.radioButton20.TabStop = true;
		this.radioButton20.UseVisualStyleBackColor = false;
		this.radioButton20.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton21.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton21.BackColor = System.Drawing.Color.Cyan;
		this.radioButton21.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton21.Location = new System.Drawing.Point(235, 108);
		this.radioButton21.Name = "radioButton21";
		this.radioButton21.Size = new System.Drawing.Size(30, 28);
		this.radioButton21.TabIndex = 21;
		this.radioButton21.TabStop = true;
		this.radioButton21.UseVisualStyleBackColor = false;
		this.radioButton21.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton22.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton22.BackColor = System.Drawing.Color.Blue;
		this.radioButton22.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton22.Location = new System.Drawing.Point(276, 108);
		this.radioButton22.Name = "radioButton22";
		this.radioButton22.Size = new System.Drawing.Size(30, 28);
		this.radioButton22.TabIndex = 22;
		this.radioButton22.TabStop = true;
		this.radioButton22.UseVisualStyleBackColor = false;
		this.radioButton22.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton23.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton23.BackColor = System.Drawing.Color.Fuchsia;
		this.radioButton23.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton23.Location = new System.Drawing.Point(317, 108);
		this.radioButton23.Name = "radioButton23";
		this.radioButton23.Size = new System.Drawing.Size(30, 28);
		this.radioButton23.TabIndex = 23;
		this.radioButton23.TabStop = true;
		this.radioButton23.UseVisualStyleBackColor = false;
		this.radioButton23.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton8.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton8.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.radioButton8.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton8.Location = new System.Drawing.Point(31, 74);
		this.radioButton8.Name = "radioButton8";
		this.radioButton8.Size = new System.Drawing.Size(30, 28);
		this.radioButton8.TabIndex = 8;
		this.radioButton8.TabStop = true;
		this.radioButton8.UseVisualStyleBackColor = false;
		this.radioButton8.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton9.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton9.BackColor = System.Drawing.Color.FromArgb(255, 128, 128);
		this.radioButton9.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton9.Location = new System.Drawing.Point(71, 74);
		this.radioButton9.Name = "radioButton9";
		this.radioButton9.Size = new System.Drawing.Size(30, 28);
		this.radioButton9.TabIndex = 9;
		this.radioButton9.TabStop = true;
		this.radioButton9.UseVisualStyleBackColor = false;
		this.radioButton9.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton10.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton10.BackColor = System.Drawing.Color.FromArgb(255, 192, 128);
		this.radioButton10.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton10.Location = new System.Drawing.Point(112, 74);
		this.radioButton10.Name = "radioButton10";
		this.radioButton10.Size = new System.Drawing.Size(30, 28);
		this.radioButton10.TabIndex = 10;
		this.radioButton10.TabStop = true;
		this.radioButton10.UseVisualStyleBackColor = false;
		this.radioButton10.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton11.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton11.BackColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.radioButton11.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton11.Location = new System.Drawing.Point(153, 74);
		this.radioButton11.Name = "radioButton11";
		this.radioButton11.Size = new System.Drawing.Size(30, 28);
		this.radioButton11.TabIndex = 11;
		this.radioButton11.TabStop = true;
		this.radioButton11.UseVisualStyleBackColor = false;
		this.radioButton11.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton12.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton12.BackColor = System.Drawing.Color.FromArgb(128, 255, 128);
		this.radioButton12.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton12.Location = new System.Drawing.Point(194, 74);
		this.radioButton12.Name = "radioButton12";
		this.radioButton12.Size = new System.Drawing.Size(30, 28);
		this.radioButton12.TabIndex = 12;
		this.radioButton12.TabStop = true;
		this.radioButton12.UseVisualStyleBackColor = false;
		this.radioButton12.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton13.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton13.BackColor = System.Drawing.Color.FromArgb(128, 255, 255);
		this.radioButton13.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton13.Location = new System.Drawing.Point(235, 74);
		this.radioButton13.Name = "radioButton13";
		this.radioButton13.Size = new System.Drawing.Size(30, 28);
		this.radioButton13.TabIndex = 13;
		this.radioButton13.TabStop = true;
		this.radioButton13.UseVisualStyleBackColor = false;
		this.radioButton13.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton14.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton14.BackColor = System.Drawing.Color.FromArgb(128, 128, 255);
		this.radioButton14.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton14.Location = new System.Drawing.Point(276, 74);
		this.radioButton14.Name = "radioButton14";
		this.radioButton14.Size = new System.Drawing.Size(30, 28);
		this.radioButton14.TabIndex = 14;
		this.radioButton14.TabStop = true;
		this.radioButton14.UseVisualStyleBackColor = false;
		this.radioButton14.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton15.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton15.BackColor = System.Drawing.Color.FromArgb(255, 128, 255);
		this.radioButton15.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton15.Location = new System.Drawing.Point(317, 74);
		this.radioButton15.Name = "radioButton15";
		this.radioButton15.Size = new System.Drawing.Size(30, 28);
		this.radioButton15.TabIndex = 15;
		this.radioButton15.TabStop = true;
		this.radioButton15.UseVisualStyleBackColor = false;
		this.radioButton15.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton7.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton7.BackColor = System.Drawing.Color.FromArgb(255, 192, 255);
		this.radioButton7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton7.Location = new System.Drawing.Point(317, 37);
		this.radioButton7.Name = "radioButton7";
		this.radioButton7.Size = new System.Drawing.Size(30, 28);
		this.radioButton7.TabIndex = 7;
		this.radioButton7.TabStop = true;
		this.radioButton7.UseVisualStyleBackColor = false;
		this.radioButton7.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton6.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton6.BackColor = System.Drawing.Color.FromArgb(192, 192, 255);
		this.radioButton6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton6.Location = new System.Drawing.Point(276, 37);
		this.radioButton6.Name = "radioButton6";
		this.radioButton6.Size = new System.Drawing.Size(30, 28);
		this.radioButton6.TabIndex = 6;
		this.radioButton6.TabStop = true;
		this.radioButton6.UseVisualStyleBackColor = false;
		this.radioButton6.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton5.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton5.BackColor = System.Drawing.Color.FromArgb(192, 255, 255);
		this.radioButton5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton5.Location = new System.Drawing.Point(235, 37);
		this.radioButton5.Name = "radioButton5";
		this.radioButton5.Size = new System.Drawing.Size(30, 28);
		this.radioButton5.TabIndex = 5;
		this.radioButton5.TabStop = true;
		this.radioButton5.UseVisualStyleBackColor = false;
		this.radioButton5.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton4.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton4.BackColor = System.Drawing.Color.FromArgb(192, 255, 192);
		this.radioButton4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton4.Location = new System.Drawing.Point(194, 37);
		this.radioButton4.Name = "radioButton4";
		this.radioButton4.Size = new System.Drawing.Size(30, 28);
		this.radioButton4.TabIndex = 4;
		this.radioButton4.TabStop = true;
		this.radioButton4.UseVisualStyleBackColor = false;
		this.radioButton4.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton3.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton3.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.radioButton3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton3.Location = new System.Drawing.Point(153, 37);
		this.radioButton3.Name = "radioButton3";
		this.radioButton3.Size = new System.Drawing.Size(30, 28);
		this.radioButton3.TabIndex = 3;
		this.radioButton3.TabStop = true;
		this.radioButton3.UseVisualStyleBackColor = false;
		this.radioButton3.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton2.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton2.BackColor = System.Drawing.Color.FromArgb(255, 224, 192);
		this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton2.Location = new System.Drawing.Point(112, 37);
		this.radioButton2.Name = "radioButton2";
		this.radioButton2.Size = new System.Drawing.Size(30, 28);
		this.radioButton2.TabIndex = 2;
		this.radioButton2.TabStop = true;
		this.radioButton2.UseVisualStyleBackColor = false;
		this.radioButton2.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton1.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton1.BackColor = System.Drawing.Color.FromArgb(255, 192, 192);
		this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton1.Location = new System.Drawing.Point(71, 37);
		this.radioButton1.Name = "radioButton1";
		this.radioButton1.Size = new System.Drawing.Size(30, 28);
		this.radioButton1.TabIndex = 1;
		this.radioButton1.TabStop = true;
		this.radioButton1.UseVisualStyleBackColor = false;
		this.radioButton1.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		this.radioButton0.Appearance = System.Windows.Forms.Appearance.Button;
		this.radioButton0.BackColor = System.Drawing.Color.White;
		this.radioButton0.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.radioButton0.Location = new System.Drawing.Point(31, 37);
		this.radioButton0.Name = "radioButton0";
		this.radioButton0.Size = new System.Drawing.Size(30, 28);
		this.radioButton0.TabIndex = 0;
		this.radioButton0.TabStop = true;
		this.radioButton0.UseVisualStyleBackColor = false;
		this.radioButton0.MouseClick += new System.Windows.Forms.MouseEventHandler(radioButtonColor_MouseClick);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		base.Controls.Add(this.basicColorsGroupBox);
		this.MaximumSize = new System.Drawing.Size(385, 361);
		this.MinimumSize = new System.Drawing.Size(385, 361);
		base.Name = "BasicColorsEditorUserControl";
		base.Size = new System.Drawing.Size(385, 361);
		base.Load += new System.EventHandler(BasicColorsEditorUserControl_Load);
		this.basicColorsGroupBox.ResumeLayout(false);
		this.basicColorsGroupBox.PerformLayout();
		base.ResumeLayout(false);
	}
}

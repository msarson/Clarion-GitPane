using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ZetaColorEditor.PropertyGridEditors;

public class ColorUI : ColorEditorUserControl
{
	private IWindowsFormsEditorService _edSvc;

	private object _value;

	private ColorTypeEditorDropDown _editor;

	private Color initialColor;

	private IContainer components;

	public object Value => _value;

	public ColorUI(ColorTypeEditorDropDown editor)
	{
		_editor = editor;
		InitializeComponent();
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		switch (keyData)
		{
		case Keys.Return:
			_value = base.SelectedColor;
			base.SelectedColor = (Color)_value;
			OnColorSelected(this, EventArgs.Empty);
			return true;
		case Keys.Escape:
			_value = initialColor;
			base.SelectedColor = initialColor;
			OnColorSelected(this, EventArgs.Empty);
			return true;
		default:
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}

	private void OnColorSelected(object sender, EventArgs e)
	{
		_value = base.SelectedColor;
		if (_edSvc != null)
		{
			_edSvc.CloseDropDown();
		}
	}

	private void adjustColorUIHeight()
	{
		base.Size = MinimumSize;
	}

	public void Start(IWindowsFormsEditorService edSvc, object value)
	{
		_edSvc = edSvc;
		_value = value;
		base.ColorSelected += OnColorSelected;
		adjustColorUIHeight();
		if (value != null)
		{
			base.SelectedColor = (initialColor = (Color)value);
		}
	}

	public void End()
	{
		base.ColorSelected -= OnColorSelected;
		_edSvc = null;
		_value = null;
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
	}
}

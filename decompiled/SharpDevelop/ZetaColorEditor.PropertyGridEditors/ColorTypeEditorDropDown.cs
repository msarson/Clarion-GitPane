using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace ZetaColorEditor.PropertyGridEditors;

public class ColorTypeEditorDropDown : UITypeEditor
{
	private ColorUI _colorUI;

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider != null)
		{
			IWindowsFormsEditorService windowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (windowsFormsEditorService == null)
			{
				return value;
			}
			if (_colorUI == null)
			{
				_colorUI = new ColorUI(this);
			}
			_colorUI.Start(windowsFormsEditorService, value);
			windowsFormsEditorService.DropDownControl(_colorUI);
			if (_colorUI.Value != null && (Color)_colorUI.Value != Color.Empty)
			{
				value = _colorUI.Value;
			}
			_colorUI.End();
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}

	public override bool GetPaintValueSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override void PaintValue(PaintValueEventArgs e)
	{
		if (e.Value is Color)
		{
			Color color = (Color)e.Value;
			SolidBrush solidBrush = new SolidBrush(color);
			e.Graphics.FillRectangle(solidBrush, e.Bounds);
			solidBrush.Dispose();
		}
	}
}

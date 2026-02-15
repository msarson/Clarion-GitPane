using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace ZetaColorEditor.PropertyGridEditors;

public class ColorTypeEditorDialog : UITypeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		Color selectedColor = ((value != null) ? ((Color)value) : Color.Empty);
		using (ColorEditorForm colorEditorForm = new ColorEditorForm())
		{
			colorEditorForm.SelectedColor = selectedColor;
			if (colorEditorForm.ShowDialog() == DialogResult.OK)
			{
				value = colorEditorForm.SelectedColor;
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
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

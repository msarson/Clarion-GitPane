using System.ComponentModel;
using System.Drawing;

namespace ZetaColorEditor.PropertyGridEditors;

public class ColorTypeDialogConverter : ColorConverter
{
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return false;
	}
}

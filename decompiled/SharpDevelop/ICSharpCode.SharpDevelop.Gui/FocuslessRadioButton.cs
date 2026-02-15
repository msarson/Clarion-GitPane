using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

public class FocuslessRadioButton : RadioButton
{
	protected override bool ShowFocusCues => false;
}

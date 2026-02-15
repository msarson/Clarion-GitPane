using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace ICSharpCode.SharpDevelop.Gui;

public class InputBox : BaseSharpDevelopForm
{
	private Label label;

	private TextBox textBox;

	public Label Label => label;

	public TextBox TextBox => textBox;

	public InputBox()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.InputBox.xfrm"));
		label = (Label)base.ControlDictionary["label"];
		textBox = (TextBox)base.ControlDictionary["textBox"];
	}
}

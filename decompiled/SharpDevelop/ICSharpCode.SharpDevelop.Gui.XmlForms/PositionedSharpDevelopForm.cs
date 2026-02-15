using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public class PositionedSharpDevelopForm : SharpDevelopFormWithHelp
{
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	protected virtual string DialogName => null;

	public PositionedSharpDevelopForm()
	{
		base.FormClosing += PositionedSharpDevelopForm_FormClosing;
	}

	private void PositionedSharpDevelopForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		PositionedForm.DoFormClosing(this, DialogName);
	}

	protected override void OnLoad(EventArgs e)
	{
		PositionedForm.DoLoad(this, DialogName);
		base.OnLoad(e);
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		PositionedForm.DoFormClosing(this, DialogName);
		base.OnFormClosing(e);
	}
}

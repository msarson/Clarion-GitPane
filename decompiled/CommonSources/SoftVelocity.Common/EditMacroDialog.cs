using System;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace SoftVelocity.Common;

public class EditMacroDialog : BaseSharpDevelopForm
{
	private string macro;

	private string value;

	public string Macro => macro;

	public string Value => value;

	public EditMacroDialog(string macro, string value)
	{
		this.macro = macro;
		this.value = value;
		InitializeComponents();
	}

	private void AcceptEvent(object sender, EventArgs e)
	{
		macro = ((XmlForm)this).ControlDictionary["macroTextBox"].Text;
		value = ((XmlForm)this).ControlDictionary["valueTextBox"].Text;
	}

	private void InitializeComponents()
	{
		((XmlForm)this).SetupFromXmlStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CommonSources.Resources.EditMacroDialog.xfrm"));
		((XmlForm)this).ControlDictionary["macroTextBox"].Text = macro;
		((XmlForm)this).ControlDictionary["valueTextBox"].Text = value;
		((XmlForm)this).ControlDictionary["okButton"].Click += AcceptEvent;
		((Form)this).Owner = (Form)(object)WorkbenchSingleton.Workbench;
		((Form)this).Icon = null;
	}
}

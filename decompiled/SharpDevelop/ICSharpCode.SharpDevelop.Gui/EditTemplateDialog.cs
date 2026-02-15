using System;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop.Gui;

public class EditTemplateDialog : BaseSharpDevelopForm
{
	private CodeTemplate codeTemplate;

	public CodeTemplate CodeTemplate => codeTemplate;

	public EditTemplateDialog(CodeTemplate codeTemplate)
	{
		this.codeTemplate = codeTemplate;
		InitializeComponents();
	}

	private void AcceptEvent(object sender, EventArgs e)
	{
		codeTemplate.Shortcut = base.ControlDictionary["templateTextBox"].Text;
		codeTemplate.Description = base.ControlDictionary["descriptionTextBox"].Text;
	}

	private void InitializeComponents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.EditTemplateDialog.xfrm"));
		base.ControlDictionary["templateTextBox"].Text = codeTemplate.Shortcut;
		base.ControlDictionary["descriptionTextBox"].Text = codeTemplate.Description;
		base.ControlDictionary["okButton"].Click += AcceptEvent;
		base.Owner = (Form)WorkbenchSingleton.Workbench;
		base.Icon = null;
	}
}

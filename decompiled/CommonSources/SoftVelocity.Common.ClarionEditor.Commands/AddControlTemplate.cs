using System.Windows.Forms;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Generator;
using SoftVelocity.Generator.Dialogs;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddControlTemplate : AbstractClarionGeneratorCommand
{
	public override void Run()
	{
		IFormatter formatterRequester = base.View.FormatterRequester;
		using ControlTemplateDlg controlTemplateDlg = new ControlTemplateDlg(formatterRequester);
		if (controlTemplateDlg.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		IControlTemplate returnIControlTemplate = controlTemplateDlg.ReturnIControlTemplate;
		if (returnIControlTemplate == null)
		{
			return;
		}
		base.View.TempUpdateStructure();
		IPopulatedTemplate populatedTemplate = formatterRequester.PopulateTemplate(returnIControlTemplate);
		if (populatedTemplate != null)
		{
			ControlContainer controlContainer = base.View.ParseControlString(populatedTemplate.Data);
			if (controlContainer != null)
			{
				base.View.SetTemplate(controlContainer, populatedTemplate.InstanceID, populatedTemplate);
			}
		}
	}
}

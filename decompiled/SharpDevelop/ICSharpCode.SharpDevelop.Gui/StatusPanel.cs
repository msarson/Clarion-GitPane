using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class StatusPanel : UserControl
{
	private WizardDialog wizard;

	private Font smallFont;

	private Font normalFont;

	private Font boldFont;

	public StatusPanel(WizardDialog wizard)
	{
		smallFont = ResourceService.LoadFont("Tahoma", 14, GraphicsUnit.World);
		normalFont = ResourceService.LoadFont("Tahoma", 14, GraphicsUnit.World);
		boldFont = ResourceService.LoadFont("Tahoma", 14, FontStyle.Bold, GraphicsUnit.World);
		this.wizard = wizard;
		BackgroundImage = ResourceService.GetBitmap("GeneralWizardBackground");
		base.Size = new Size(198, 400);
		base.ResizeRedraw = false;
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		Graphics graphics = pe.Graphics;
		graphics.DrawString(ResourceService.GetString("SharpDevelop.Gui.Dialogs.WizardDialog.StepsLabel"), smallFont, SystemBrushes.WindowText, 10f, 24 - smallFont.Height);
		graphics.DrawLine(SystemPens.WindowText, 10, 24, base.Width - 10, 24);
		int num = 0;
		for (int num2 = 0; num2 < wizard.WizardPanels.Count; num2 = wizard.GetSuccessorNumber(num2))
		{
			Font font = ((wizard.ActivePanelNumber == num2) ? boldFont : normalFont);
			IDialogPanelDescriptor dialogPanelDescriptor = (IDialogPanelDescriptor)wizard.WizardPanels[num2];
			graphics.DrawString(1 + num + ". " + dialogPanelDescriptor.Label, font, SystemBrushes.WindowText, 10f, 40 + num * font.Height);
			num++;
		}
	}
}

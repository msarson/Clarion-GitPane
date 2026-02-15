using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class CurrentPanelPanel : UserControl
{
	private WizardDialog wizard;

	private Font normalFont;

	public CurrentPanelPanel(WizardDialog wizard)
	{
		normalFont = ResourceService.LoadFont("SansSerif", 18, GraphicsUnit.World);
		this.wizard = wizard;
		base.Size = new Size(wizard.Width - 220, 30);
		base.ResizeRedraw = false;
		SetStyle(ControlStyles.UserPaint, value: true);
	}

	protected override void OnPaintBackground(PaintEventArgs pe)
	{
		Graphics graphics = pe.Graphics;
		using Brush brush = new LinearGradientBrush(new Point(0, 0), new Point(base.Width, base.Height), Color.White, SystemColors.Control);
		graphics.FillRectangle(brush, new Rectangle(0, 0, base.Width, base.Height));
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		Graphics graphics = pe.Graphics;
		graphics.DrawString(((IDialogPanelDescriptor)wizard.WizardPanels[wizard.ActivePanelNumber]).Label, normalFont, Brushes.Black, 10f, 24 - normalFont.Height, StringFormat.GenericTypographic);
		graphics.DrawLine(Pens.Black, 10, 24, base.Width - 10, 24);
	}
}

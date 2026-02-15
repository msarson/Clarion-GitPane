using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class GradientHeaderPanel : Label
{
	public GradientHeaderPanel(int fontSize)
		: this()
	{
		Font = ResourceService.LoadFont("Tahoma", fontSize);
	}

	public GradientHeaderPanel()
	{
		base.ResizeRedraw = true;
		Text = string.Empty;
	}

	protected override void OnPaintBackground(PaintEventArgs pe)
	{
		base.OnPaintBackground(pe);
		Graphics graphics = pe.Graphics;
		using Brush brush = new LinearGradientBrush(new Point(0, 0), new Point(base.Width, base.Height), SystemColors.Window, SystemColors.Control);
		graphics.FillRectangle(brush, new Rectangle(0, 0, base.Width, base.Height));
	}
}

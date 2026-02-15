using System.Drawing;
using System.Windows.Forms;
using SoftVelocity.Common.Controls;

namespace SoftVelocity.Generator.UI;

internal class LoadingAppMessagePanel : WaitPanel
{
	internal LoadingAppMessagePanel()
	{
		base.UseGradient = true;
		base.BackColorGradientEnd = SystemColors.Window;
		base.BackColorGradientBegin = SystemColors.Control;
		base.AlphaBlend = AlphaBlendType.None;
		base.BorderStyle = BorderStyle.Fixed3D;
		Dock = DockStyle.Fill;
		Font = new Font("Verdana", 14.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
		ForeColor = Color.Black;
		base.Location = new Point(0, 0);
		base.Name = "_LoadingAppMessagePanel";
		base.Padding = new Padding(5, 0, 0, 0);
		base.Size = new Size(615, 264);
		base.TabIndex = 2;
		base.Message = "Loading Application ...";
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			base.Visible = false;
			base.Parent.Controls.Remove(this);
			base.Parent = null;
		}
		base.Dispose(disposing);
	}
}

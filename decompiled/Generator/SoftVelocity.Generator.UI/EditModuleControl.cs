using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Generator.UI;

[ToolboxItem(true)]
[DesignTimeVisible(true)]
public sealed class EditModuleControl : CWControl_Host
{
	public EditModuleControl(CWControl_Container container)
		: base(container)
	{
	}

	internal override void InitializeView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.Name = "EditModuleControl";
		base.Location = new System.Drawing.Point(0, 0);
		base.Size = new System.Drawing.Size(745, 473);
		base.TabIndex = 2;
		base.Visible = false;
		base.ResumeLayout(false);
	}
}

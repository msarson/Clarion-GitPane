using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Generator.UI;

[DesignTimeVisible(true)]
[ToolboxItem(true)]
public sealed class EditProcedureControl : CWControl_Host
{
	public EditProcedureControl(CWControl_Container container)
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
		base.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.Name = "EditProcedureControl";
		base.Location = new System.Drawing.Point(0, 0);
		base.Size = new System.Drawing.Size(745, 473);
		base.TabIndex = 1;
		base.Visible = false;
		base.ResumeLayout(false);
	}
}

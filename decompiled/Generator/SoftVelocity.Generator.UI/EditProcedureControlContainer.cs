using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Generator.UI;

public class EditProcedureControlContainer : CWControl_Container
{
	public EditProcedureControlContainer(CWControl_ViewContent content)
		: base(content)
	{
		InitializeComponent();
		PerformLayout();
	}

	private void InitializeComponent()
	{
		base._ViewControl = new SoftVelocity.Generator.UI.EditProcedureControl(this);
		base._ViewControl.InitializeView();
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.Name = "EditProcedureControlContainer";
		base.Size = new System.Drawing.Size(612, 414);
		base.TabStop = false;
		base.Visible = false;
		base.Controls.Add(base._ViewControl);
		base.ResumeLayout(false);
	}
}

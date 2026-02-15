using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Generator.UI;

public class TemplateRegistryControl_Container : CWControl_Container
{
	public TemplateRegistryControl_Container(TemplateRegistryControl_ViewContent viewcontent)
		: base(viewcontent)
	{
		InitializeComponent();
	}

	internal override void ViewOpened()
	{
		if (_ViewControl != null)
		{
			_ViewControl.OnHostWindowResize();
		}
		base.ViewOpened();
	}

	private void InitializeComponent()
	{
		base._ViewControl = new SoftVelocity.Generator.UI.TemplateRegistryControl(this);
		base._ViewControl.InitializeView();
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.Name = "TemplateRegistryControlContainer";
		base.Size = new System.Drawing.Size(317, 263);
		base.TabStop = false;
		base.Visible = false;
		base.Controls.Add(base._ViewControl);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

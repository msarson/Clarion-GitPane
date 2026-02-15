using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Clarion.ASL;
using SoftVelocity.CWPInvoke;

namespace SoftVelocity.Generator.UI;

[ToolboxItem(true)]
[DesignTimeVisible(true)]
public sealed class TemplateRegistryControl : CWControl_Host
{
	public TemplateRegistryControl(TemplateRegistryControl_Container container)
		: base(container)
	{
		CWDialogService.Instance.CreateHost += Request_CreateHost;
	}

	static TemplateRegistryControl()
	{
	}

	private void Request_CreateHost(UINetBinding CWObj, UIBindingInterfaceKind kind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)kind == 7 && _Container._ViewContent.ValidObject(CWObj))
		{
			CWDialogService.Instance.CreateHost -= Request_CreateHost;
			OpenNewControl(CWObj);
		}
	}

	internal override void InitializeView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.Name = "TemplateRegistryControl";
		base.Location = new System.Drawing.Point(0, 25);
		base.Size = new System.Drawing.Size(317, 238);
		base.TabIndex = 1;
		base.Visible = false;
		base.ResumeLayout(false);
	}
}

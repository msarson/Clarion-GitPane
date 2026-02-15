using System;
using System.Drawing;
using System.Windows.Forms;
using Clarion.ASL;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.UI;

public class ApplicationContainer : CWControl_Container
{
	private bool titleAssigned;

	private ApplicationMainWindowControl_ViewContent ViewContent => (ApplicationMainWindowControl_ViewContent)_ViewContent;

	public event EventHandler ApplicationWindowOpened;

	public ApplicationContainer(ApplicationMainWindowControl_ViewContent content)
		: base(content)
	{
		InitializeComponent();
		base.GotFocus += ApplicationContainer_GotFocus;
		base.Enter += ApplicationContainer_Enter;
	}

	internal void OpenGeneratorWindow(UINetBinding CWObj, UIBindingInterfaceKind kind)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected I4, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		if (!_ViewContent.ValidObject(CWObj))
		{
			return;
		}
		_ = base._WorkbenchWindow;
		if (ViewContent != null)
		{
			if ((int)kind == 6)
			{
				ViewContent.App.SetIsOnApptree(value: true);
			}
			else
			{
				ViewContent.App.SetIsOnApptree(value: false);
			}
		}
		CWControl_Host cWControl_Host;
		switch (kind - 6)
		{
		default:
			return;
		case 0:
			if (_ViewControl != null)
			{
				return;
			}
			cWControl_Host = new ApplicationMainWindowControl(this);
			base.VisibleChanged += OnVisibleChanged;
			break;
		case 3:
			cWControl_Host = new EditModuleControl(this);
			break;
		case 4:
			cWControl_Host = new EditProcedureControl(this);
			break;
		case 1:
		case 2:
			return;
		}
		cWControl_Host.InitializeView();
		base.Controls.Add(cWControl_Host);
		cWControl_Host.AttachToParentFormCloseEvent();
		titleAssigned = false;
		cWControl_Host.OpenNewControl(CWObj);
	}

	private void OnVisibleChanged(object sender, EventArgs e)
	{
		if (base.Visible)
		{
			base.VisibleChanged -= OnVisibleChanged;
		}
	}

	internal override void SetCaptionText(string txt)
	{
		if (titleAssigned)
		{
			ViewContent.ReplaceHeaderTitle(txt);
			return;
		}
		titleAssigned = true;
		ViewContent.SetHeaderTitle(txt);
	}

	internal override void CloseView(CWControl_Host newctl)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		if (WorkbenchSingleton.MainForm.InvokeRequired)
		{
			throw new InvalidOperationException("Can not call the CloseView from a thread other than the Main Window Thread.");
		}
		if (newctl != null && !ForcedCancelMode)
		{
			ViewContent.RemoveCurrentHeaderTitle();
			titleAssigned = false;
		}
		base.CloseView(newctl);
		if (newctl == null)
		{
			ApplicationService.ApplicationFrameClosed();
		}
		else if (ViewContent != null && (int)newctl.UIKind == 6)
		{
			ViewContent.App.SetIsOnApptree(value: true);
			WorkbenchSingleton.MainForm.Activate();
			SetFocusOnChild();
		}
	}

	internal override void ViewOpened()
	{
		if (_ViewControl._PrevView == null && this.ApplicationWindowOpened != null)
		{
			this.ApplicationWindowOpened(null, null);
			this.ApplicationWindowOpened = null;
		}
	}

	internal void OnWorkbench_ViewClosed()
	{
		ForceCancel();
		Dispose();
	}

	private void ApplicationContainer_Enter(object sender, EventArgs e)
	{
		SetFocusOnChild();
	}

	private void ApplicationContainer_GotFocus(object sender, EventArgs e)
	{
		SetFocusOnChild();
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.Name = "ApplicationContainer";
		base.Size = new System.Drawing.Size(745, 473);
		base.TabIndex = 1;
		base.TabStop = false;
		base.Visible = false;
		base.ResumeLayout(false);
	}
}

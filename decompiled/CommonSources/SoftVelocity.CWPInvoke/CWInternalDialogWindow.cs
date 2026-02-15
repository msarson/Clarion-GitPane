using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.CWPInvoke;

[ToolboxItem(false)]
[DesignTimeVisible(true)]
public class CWInternalDialogWindow : CWWindow
{
	private IContainer components;

	protected CWDialogForm DlgForm;

	protected bool hostopened;

	public CWInternalDialogWindow(CWDialogForm f)
		: base(notdocked: true)
	{
		DlgForm = f;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
			base.Parent = null;
			DlgForm = null;
		}
		base.Dispose(disposing);
	}

	public void SetupWindow()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.Margin = new System.Windows.Forms.Padding(0);
		base.Name = "CWInternalDialogWindow";
		base.Size = new System.Drawing.Size(1352, 673);
		base.TabStop = false;
		base.Visible = false;
		base.ResumeLayout(false);
	}

	protected override void RegisterNetEvents()
	{
		base.RegisterNetEvents();
		base.WindowOpened += CWDialog_OnWindowOpened;
		base.CaptionChanged += CWDialog_OnWindowCaptionChanged;
		base.DisconnectFromHosted += CWDialog_OnDisconnectFromHosted;
		base.NotifyNewSize += CWDialog_OnWindowNotifyNewSize;
	}

	protected override void SetWindowVisible(bool on)
	{
		base.Visible = on;
	}

	private void CWDialog_OnWindowOpened(object sender)
	{
		base.WindowOpened -= CWDialog_OnWindowOpened;
		base.WindowClosing += CWDialog_OnWindowClosing;
		hostopened = true;
	}

	private void CWDialog_OnDisconnectFromHosted(object sender)
	{
		try
		{
			base.DisconnectFromHosted -= CWDialog_OnDisconnectFromHosted;
			CWDialog_PrepareToClose();
			CloseInnerWindow();
			DlgForm.OnCWDialogWindowClosed();
		}
		catch (Exception)
		{
		}
	}

	private void CWDialog_OnWindowClosing(object sender)
	{
		CWDialog_PrepareToClose();
	}

	private void CWDialog_OnWindowNotifyNewSize(object sender, Size size)
	{
		DlgForm.OnCWDialogWindowNotifyNewSize(size);
		SetFocusOnChild();
	}

	private void CWDialog_OnWindowCaptionChanged(object sender)
	{
		DlgForm.OnCWDialogWindowCaptionChanged(base.HostedWindowCaption);
	}

	internal void CWDialog_PrepareToClose()
	{
		if (hostopened)
		{
			hostopened = false;
			base.WindowClosing -= CWDialog_OnWindowClosing;
			base.CaptionChanged -= CWDialog_OnWindowCaptionChanged;
			base.NotifyNewSize -= CWDialog_OnWindowNotifyNewSize;
			base.Visible = false;
		}
	}

	internal void DisconnectFromParent()
	{
		OnParentClosed();
	}
}

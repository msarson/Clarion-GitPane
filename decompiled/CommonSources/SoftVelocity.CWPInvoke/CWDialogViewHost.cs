using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Clarion.ASL;

namespace SoftVelocity.CWPInvoke;

[ToolboxItem(false)]
[DesignTimeVisible(true)]
internal class CWDialogViewHost : CWWindow
{
	private IContainer components;

	private CWDialogViewContent _Content;

	protected bool hostopened;

	internal CWDialogViewHost(CWDialogViewContent content)
		: base(notdocked: false)
	{
		_Content = content;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
			_Content = null;
		}
		base.Dispose(disposing);
	}

	protected override void RegisterNetEvents()
	{
		base.RegisterNetEvents();
		base.WindowOpened += Host_WindowOpened;
		base.DisconnectFromHosted += Host_DisconnectFromHosted;
	}

	internal void OpenNewControl(UINetBinding CWObj)
	{
		InitializeComponent();
		AttachToParentFormCloseEvent();
		BindCWWindow(CWObj);
	}

	private void Host_WindowOpened(object sender)
	{
		base.WindowOpened -= Host_WindowOpened;
		base.WindowClosing += Host_WindowClosing;
		base.CaptionChanged += Host_CaptionChanged;
		_Content.OnWindowOpened();
		hostopened = true;
	}

	private void Host_CaptionChanged(object sender)
	{
		_Content.OnCaptionChanged(base.HostedWindowCaption);
	}

	protected virtual void Host_DisconnectFromHosted(object sender)
	{
		try
		{
			base.DisconnectFromHosted -= Host_DisconnectFromHosted;
			Host_PrepareToClose();
			CloseInnerWindow();
		}
		catch
		{
		}
	}

	protected virtual void Host_WindowClosing(object sender)
	{
		Host_PrepareToClose();
	}

	internal void Host_PrepareToClose()
	{
		if (hostopened)
		{
			hostopened = false;
			base.CaptionChanged -= Host_CaptionChanged;
			base.WindowClosing -= Host_WindowClosing;
			base.Visible = false;
			_Content.OnWindowClosed();
		}
	}

	internal void WorkbenchWindow_ClosingEvent(CancelEventArgs e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)102);
		if (val != null)
		{
			val.ValueOf = true;
		}
		e.Cancel = true;
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.Location = new System.Drawing.Point(0, 0);
		base.Margin = new System.Windows.Forms.Padding(0);
		base.Name = "CWDialogViewHost";
		base.Size = new System.Drawing.Size(343, 289);
		base.TabIndex = 0;
		base.Visible = false;
		base.ResumeLayout(false);
	}
}

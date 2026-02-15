using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Clarion.ASL;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.CWPInvoke;

namespace SoftVelocity.Generator.UI;

[DesignTimeVisible(false)]
[ToolboxItem(false)]
public class CWControl_Host : CWWindow
{
	protected CWControl_Container _Container;

	private CWControl_Host _prev;

	private IContainer components;

	protected bool hostopened;

	internal CWControl_Host _PrevView => _prev;

	public CWControl_Host(CWControl_Container container)
		: base(notdocked: false)
	{
		_Container = container;
		_prev = container._ViewControl;
		container._ViewControl = this;
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
			_Container = null;
			_prev = null;
		}
		base.Dispose(disposing);
	}

	protected override void RegisterNetEvents()
	{
		base.RegisterNetEvents();
		base.WindowOpened += Host_WindowOpened;
		base.CaptionChanged += Host_CaptionChanged;
		base.DisconnectFromHosted += Host_DisconnectFromHosted;
	}

	protected virtual void Host_WindowOpened(object sender)
	{
		base.WindowOpened -= Host_WindowOpened;
		base.WindowClosing += Host_WindowClosing;
		hostopened = true;
		if (_PrevView != null)
		{
			_PrevView.Visible = false;
		}
		if (_Container != null)
		{
			_Container.ViewOpened();
		}
	}

	protected virtual void Host_CaptionChanged(object sender)
	{
		if (_Container == null)
		{
			Text = base.HostedWindowCaption;
		}
		else
		{
			_Container.SetCaptionText(base.HostedWindowCaption);
		}
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
		if (_Container != null)
		{
			_Container.CloseView(_PrevView);
		}
	}

	protected virtual void Host_WindowClosing(object sender)
	{
		Host_PrepareToClose();
	}

	protected void Host_PrepareToClose()
	{
		if (hostopened)
		{
			hostopened = false;
			base.CaptionChanged -= Host_CaptionChanged;
			base.WindowClosing -= Host_WindowClosing;
			base.Visible = false;
			base.Parent.Controls.Remove(this);
		}
	}

	public virtual void OpenNewControl(UINetBinding CWObj)
	{
		BindCWWindow(CWObj);
	}

	internal virtual void InitializeView()
	{
	}

	public virtual void ForceCancel()
	{
		RequestClose();
	}

	public virtual void CommandInvoke(CommandID pCommandID)
	{
	}

	internal virtual void DoClosingEvent(CancelEventArgs e)
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

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.F1)
		{
			string parameter = GetType().FullName.Replace('.', '_') + ".htm";
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			FileInfo fileInfo = new FileInfo(entryAssembly.Location);
			string text = Path.Combine(fileInfo.DirectoryName, "ClarionHelp.chm");
			if (File.Exists(text))
			{
				Help.ShowHelp(WorkbenchSingleton.helpHost, text, HelpNavigator.Topic, parameter);
			}
			else
			{
				MessageService.ShowWarning("${res:MainWindow.Windows.HtmlHelp.NotFound} " + text);
			}
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}
}

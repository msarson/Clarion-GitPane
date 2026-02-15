using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.CWPInvoke;

namespace SoftVelocity.Generator.UI;

[ToolboxItem(false)]
[DesignTimeVisible(false)]
public class CWControl_Container : UserControl, ICWWindowContainer
{
	internal CWControl_ViewContent _ViewContent;

	internal CWControl_Host _ViewControl;

	private IContainer components;

	protected bool ForcedCancelMode;

	internal IWorkbenchWindow _WorkbenchWindow => ((AbstractBaseViewContent)_ViewContent).WorkbenchWindow;

	public CWControl_Container(CWControl_ViewContent content)
	{
		_ViewContent = content;
		base.ParentChanged += OnParentChanged;
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
			_ViewContent = null;
		}
		base.Dispose(disposing);
	}

	private void OnParentChanged(object sender, EventArgs e)
	{
		if (base.ParentForm != null)
		{
			base.ParentChanged -= OnParentChanged;
			base.ParentForm.FormClosed += OnParentForm_FormClosed;
			if (_ViewControl != null)
			{
				_ViewControl.AttachToParentFormCloseEvent();
			}
			base.ParentForm.SizeChanged += ParentForm_SizeChanged;
		}
	}

	private void ParentForm_SizeChanged(object sender, EventArgs e)
	{
		if (_ViewControl != null)
		{
			_ViewControl.OnHostWindowResize();
		}
	}

	private void OnParentForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		if (_ViewControl != null)
		{
			_ViewControl.OnParentClosed();
		}
		if (_ViewContent != null)
		{
			_ViewContent.ParentFormClosed();
		}
	}

	internal virtual void SetCaptionText(string txt)
	{
	}

	internal virtual void ViewOpened()
	{
	}

	internal virtual void CloseView(CWControl_Host newctl)
	{
		CWControl_Host viewControl = _ViewControl;
		if (viewControl != null)
		{
			_ViewControl = newctl;
			viewControl.Dispose();
			if (newctl != null)
			{
				if (!ForcedCancelMode)
				{
					newctl.Visible = true;
				}
				else
				{
					newctl.ForceCancel();
				}
				return;
			}
		}
		_ViewContent.CallAllControlsClosed();
	}

	internal virtual void ForceCancel()
	{
		CWControl_Host viewControl = _ViewControl;
		if (viewControl != null)
		{
			ForcedCancelMode = true;
			viewControl.ForceCancel();
		}
		else
		{
			_ViewContent.CallAllControlsClosed();
		}
	}

	internal void CommandInvoke(CommandID pCommandID)
	{
		if (_ViewControl != null)
		{
			_ViewControl.CommandInvoke(pCommandID);
		}
	}

	public void SetFocusOnChild()
	{
		if (_ViewControl != null)
		{
			_ViewControl.SetFocusOnChild();
		}
	}
}

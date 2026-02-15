using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Clarion.ASL;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.UI;

public class CWControl_ViewContent : AbstractViewContent
{
	internal CWControl_Container _Container;

	public override Control Control => _Container;

	protected virtual int InstID => 0;

	public override bool IsReadOnly => false;

	public override bool IsViewOnly => false;

	public override bool IsUntitled => false;

	public override string TabPageText => "${res:SoftVelocity.Generator.TabText}";

	public event EventHandler<EventArgs> OnAllControlsClosedBefore;

	public event EventHandler<EventArgs> OnAllControlsClosedAfter;

	public CWControl_ViewContent()
	{
		((AbstractBaseViewContent)this).WorkbenchWindowChanged += ViewContent_WorkbenchWindowChanged;
	}

	public override void Dispose()
	{
		if (((AbstractViewContent)this).SecondaryViewContents != null)
		{
			foreach (ISecondaryViewContent secondaryViewContent in ((AbstractViewContent)this).SecondaryViewContents)
			{
				if (secondaryViewContent != null && secondaryViewContent != null)
				{
					((IDisposable)secondaryViewContent)?.Dispose();
				}
			}
			((AbstractViewContent)this).SecondaryViewContents.Clear();
		}
		((AbstractViewContent)this).Dispose();
	}

	public bool ValidObject(UINetBinding CWObj)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		UIIntegerProperty val = (UIIntegerProperty)CWObj.Property((UIControlProperties)32);
		if (val == null)
		{
			return true;
		}
		return InstID == val.ValueOf;
	}

	private void ViewContent_WorkbenchWindowChanged(object sender, EventArgs e)
	{
		if (((AbstractBaseViewContent)this).WorkbenchWindow != null)
		{
			((AbstractBaseViewContent)this).WorkbenchWindowChanged -= ViewContent_WorkbenchWindowChanged;
			((AbstractBaseViewContent)this).WorkbenchWindow.ClosingEvent += WorkbenchWindow_ClosingEvent;
			((AbstractBaseViewContent)this).WorkbenchWindow.CloseEvent += WorkbenchWindow_CloseEvent;
			DoWorkbenchWindowChanged();
		}
	}

	protected virtual void DoWorkbenchWindowChanged()
	{
	}

	private void WorkbenchWindow_ClosingEvent(object sender, CancelEventArgs e)
	{
		if ((object)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent == this && _Container != null)
		{
			CWControl_Host viewControl = _Container._ViewControl;
			if (viewControl != null)
			{
				viewControl.DoClosingEvent(e);
				return;
			}
		}
		e.Cancel = true;
	}

	private void WorkbenchWindow_CloseEvent(object sender, EventArgs e)
	{
		((AbstractBaseViewContent)this).WorkbenchWindow.ClosingEvent -= WorkbenchWindow_ClosingEvent;
		((AbstractBaseViewContent)this).WorkbenchWindow.CloseEvent -= WorkbenchWindow_CloseEvent;
		DoCloseEvent();
	}

	protected virtual void DoCloseEvent()
	{
	}

	internal void CallAllControlsClosed()
	{
		if (this.OnAllControlsClosedBefore != null)
		{
			this.OnAllControlsClosedBefore(this, EventArgs.Empty);
		}
		AllControlsClosed();
		if (this.OnAllControlsClosedAfter != null)
		{
			this.OnAllControlsClosedAfter(this, EventArgs.Empty);
		}
	}

	internal virtual void AllControlsClosed()
	{
		if (_Container != null)
		{
			_Container.Dispose();
			_Container = null;
		}
		((AbstractBaseViewContent)this).WorkbenchWindow.CloseWindow(true);
	}

	internal virtual void ParentFormClosed()
	{
	}

	public void CommandInvoke(CommandID pCommandID)
	{
		if (_Container != null)
		{
			_Container.CommandInvoke(pCommandID);
		}
	}
}

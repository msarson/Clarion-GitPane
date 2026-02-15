using System;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

public abstract class AbstractBaseViewContent : IBaseViewContent, IDisposable
{
	private IWorkbenchWindow workbenchWindow;

	public abstract Control Control { get; }

	public virtual IWorkbenchWindow WorkbenchWindow
	{
		get
		{
			return workbenchWindow;
		}
		set
		{
			workbenchWindow = value;
			OnWorkbenchWindowChanged(EventArgs.Empty);
		}
	}

	public virtual string TabPageText => "Abstract Content";

	public event EventHandler WorkbenchWindowChanged;

	protected virtual void OnWorkbenchWindowChanged(EventArgs e)
	{
		if (this.WorkbenchWindowChanged != null)
		{
			this.WorkbenchWindowChanged(this, e);
		}
	}

	public virtual void SwitchedTo()
	{
	}

	public virtual void Selected()
	{
	}

	public virtual void Deselected()
	{
	}

	public virtual void Deselecting()
	{
	}

	public virtual void RedrawContent()
	{
	}

	public virtual void Dispose()
	{
		workbenchWindow = null;
	}
}

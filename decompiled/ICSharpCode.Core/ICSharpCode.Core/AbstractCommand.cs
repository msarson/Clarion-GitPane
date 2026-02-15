using System;

namespace ICSharpCode.Core;

public abstract class AbstractCommand : ICommand, IDisposable
{
	private object owner;

	private bool dispose;

	public virtual object Owner
	{
		get
		{
			return owner;
		}
		set
		{
			owner = value;
			OnOwnerChanged(EventArgs.Empty);
		}
	}

	public event EventHandler OwnerChanged;

	public abstract void Run();

	protected virtual void OnOwnerChanged(EventArgs e)
	{
		if (this.OwnerChanged != null)
		{
			this.OwnerChanged(this, e);
		}
	}

	public void Dispose()
	{
		if (!dispose)
		{
			dispose = true;
			Dispose(disposing: true);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		owner = null;
	}
}

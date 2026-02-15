using System;

namespace SoftVelocity.Common;

public abstract class HistoryCommand : IDisposable
{
	private object _target;

	private bool Cleaned;

	private bool disposed;

	public object Target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}

	protected HistoryCommand(object target)
	{
		_target = target;
	}

	public void Clean()
	{
		if (!Cleaned)
		{
			Cleaned = true;
			_target = null;
			Cleaning();
		}
	}

	protected virtual void Cleaning()
	{
	}

	public void Execute()
	{
		if (!Cleaned)
		{
			DoExecute();
		}
	}

	public void UnExecute()
	{
		if (!Cleaned)
		{
			DoUnExecute();
		}
	}

	protected abstract void DoExecute();

	protected abstract void DoUnExecute();

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
			Clean();
		}
	}
}

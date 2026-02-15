using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Clarion.ASL;

namespace SoftVelocity.CWPInvoke;

public abstract class EventInvokerBase : EventInvokerCPPBase
{
	private delegate void FireEvent(UIControlEvents ev);

	private delegate void FireEventLong(UIControlEvents ev, int v);

	private delegate void FireEventLong2(UIControlEvents ev, int v1, int v2);

	private delegate void FireEventString(UIControlEvents ev, string s);

	private delegate void FireEventString2(UIControlEvents ev, string s1, string s2);

	private delegate void FireEventLongString(UIControlEvents ev, int v, string s);

	private delegate void FireEventBinding(UIControlEvents ev, UINetBinding i);

	public EventInvokerBase(Control _wnd)
		: base(_wnd)
	{
	}

	public override void Fire([In] UIControlEvents ev)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!base.wnd.InvokeRequired)
			{
				Dispatch(ev);
				return;
			}
			WaitCompletion(base.wnd.BeginInvoke(new FireEvent(Dispatch), ev));
		}
		catch
		{
		}
	}

	public override void FireLong([In] UIControlEvents ev, [In] int v)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (base.wnd.InvokeRequired)
			{
				WaitCompletion(base.wnd.BeginInvoke(new FireEventLong(DispatchLong), ev, v));
			}
			else
			{
				DispatchLong(ev, v);
			}
		}
		catch
		{
		}
	}

	public override void FireLong2([In] UIControlEvents ev, [In] int v1, [In] int v2)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (base.wnd.InvokeRequired)
			{
				WaitCompletion(base.wnd.BeginInvoke(new FireEventLong2(DispatchLong2), ev, v1, v2));
			}
			else
			{
				DispatchLong2(ev, v1, v2);
			}
		}
		catch
		{
		}
	}

	public override void FireString([In] UIControlEvents ev, [In] string s)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (base.wnd.InvokeRequired)
			{
				WaitCompletion(base.wnd.BeginInvoke(new FireEventString(DispatchString), ev, s));
			}
			else
			{
				DispatchString(ev, s);
			}
		}
		catch
		{
		}
	}

	public override void FireString2([In] UIControlEvents ev, [In] string s1, [In] string s2)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (base.wnd.InvokeRequired)
			{
				WaitCompletion(base.wnd.BeginInvoke(new FireEventString2(DispatchString2), ev, s1, s2));
			}
			else
			{
				DispatchString2(ev, s1, s2);
			}
		}
		catch
		{
		}
	}

	public override void FireLongString([In] UIControlEvents ev, [In] int v, [In] string s)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (base.wnd.InvokeRequired)
			{
				WaitCompletion(base.wnd.BeginInvoke(new FireEventLongString(DispatchLongString), ev, v, s));
			}
			else
			{
				DispatchLongString(ev, v, s);
			}
		}
		catch
		{
		}
	}

	public override void FireBinding([In] UIControlEvents ev, [In] UINetBinding iface)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (base.wnd.InvokeRequired)
			{
				base.wnd.Invoke(new FireEventBinding(DispatchBinding), ev, iface);
			}
			else
			{
				DispatchBinding(ev, iface);
			}
		}
		catch (Exception)
		{
		}
	}

	protected virtual void Dispatch(UIControlEvents ev)
	{
		BadCall();
	}

	protected virtual void DispatchLong(UIControlEvents ev, int v)
	{
		BadCall();
	}

	protected virtual void DispatchLong2(UIControlEvents ev, int v1, int v2)
	{
		BadCall();
	}

	protected virtual void DispatchString(UIControlEvents ev, string s)
	{
		BadCall();
	}

	protected virtual void DispatchString2(UIControlEvents ev, string s1, string s2)
	{
		BadCall();
	}

	protected virtual void DispatchLongString(UIControlEvents ev, int v, string s)
	{
		BadCall();
	}

	protected virtual void DispatchBinding(UIControlEvents ev, UINetBinding i)
	{
		BadCall();
	}

	private void WaitCompletion(IAsyncResult async)
	{
		if (async != null)
		{
			WaitHandle asyncWaitHandle = async.AsyncWaitHandle;
			while (!asyncWaitHandle.WaitOne(50, exitContext: false))
			{
				Application.DoEvents();
			}
			if (base.wnd != null)
			{
				base.wnd.EndInvoke(async);
			}
		}
	}

	protected void BadCall()
	{
	}
}

using Clarion.ASL;

namespace SoftVelocity.CWPInvoke;

public sealed class CWEventInvoker : EventInvokerBase
{
	public CWEventInvoker(CWUserControl _wnd)
		: base(_wnd)
	{
	}

	protected override void Dispatch(UIControlEvents ev)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		CWUserControl cWUserControl = (CWUserControl)((EventInvokerCPPBase)this).wnd;
		cWUserControl.Dispatch(ev);
	}

	protected override void DispatchLong(UIControlEvents ev, int v)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		CWUserControl cWUserControl = (CWUserControl)((EventInvokerCPPBase)this).wnd;
		cWUserControl.DispatchLong(ev, v);
	}

	protected override void DispatchLong2(UIControlEvents ev, int v1, int v2)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		CWUserControl cWUserControl = (CWUserControl)((EventInvokerCPPBase)this).wnd;
		cWUserControl.DispatchLong2(ev, v1, v2);
	}

	protected override void DispatchString(UIControlEvents ev, string s)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		CWUserControl cWUserControl = (CWUserControl)((EventInvokerCPPBase)this).wnd;
		cWUserControl.DispatchString(ev, s);
	}

	protected override void DispatchString2(UIControlEvents ev, string s1, string s2)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		CWUserControl cWUserControl = (CWUserControl)((EventInvokerCPPBase)this).wnd;
		cWUserControl.DispatchString2(ev, s1, s2);
	}

	protected override void DispatchLongString(UIControlEvents ev, int v, string s)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		CWUserControl cWUserControl = (CWUserControl)((EventInvokerCPPBase)this).wnd;
		cWUserControl.DispatchLongString(ev, v, s);
	}

	protected override void DispatchBinding(UIControlEvents ev, UINetBinding b)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		CWUserControl cWUserControl = (CWUserControl)((EventInvokerCPPBase)this).wnd;
		cWUserControl.DispatchBinding(ev, b);
	}
}

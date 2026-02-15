using System.ComponentModel;
using System.Windows.Forms;
using Clarion.ASL;

namespace SoftVelocity.CWPInvoke;

[ToolboxItem(false)]
[DesignTimeVisible(false)]
public class CWUserControl : UserControl
{
	public virtual void Dispatch(UIControlEvents ev)
	{
	}

	public virtual void DispatchLong(UIControlEvents ev, int v)
	{
	}

	public virtual void DispatchLong2(UIControlEvents ev, int v1, int v2)
	{
	}

	public virtual void DispatchString(UIControlEvents ev, string s)
	{
	}

	public virtual void DispatchString2(UIControlEvents ev, string s1, string s2)
	{
	}

	public virtual void DispatchLongString(UIControlEvents ev, int v, string s)
	{
	}

	public virtual void DispatchBinding(UIControlEvents ev, UINetBinding i)
	{
	}
}

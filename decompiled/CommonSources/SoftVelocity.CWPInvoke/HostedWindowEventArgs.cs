using System;

namespace SoftVelocity.CWPInvoke;

public class HostedWindowEventArgs : EventArgs
{
	private string _IID;

	public string IID => _IID;

	internal HostedWindowEventArgs(string CWObjIID)
	{
		_IID = CWObjIID;
	}
}

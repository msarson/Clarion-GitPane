using System;

namespace SoftVelocity.CWPInvoke;

public class InvalidStateException : Exception
{
	private InvokeKind kind;

	private string name;

	public InvalidStateException(string name, InvokeKind kind)
	{
		this.name = name;
		this.kind = kind;
	}

	public override string ToString()
	{
		return kind switch
		{
			InvokeKind.MethodInvoke => "Invalid Method Invocation. Method name:" + name, 
			InvokeKind.PropertyGet => "Invalid Get Property. Property name:" + name, 
			InvokeKind.PropertySet => "Invalid Set Property. Property name:" + name, 
			_ => base.ToString(), 
		};
	}
}

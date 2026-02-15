using System;
using System.Runtime.Serialization;

namespace ICSharpCode.Core;

[Serializable]
public class TreePathNotFoundException : CoreException
{
	public TreePathNotFoundException(string path)
		: base("Treepath not found: " + path)
	{
	}

	public TreePathNotFoundException()
	{
	}

	public TreePathNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected TreePathNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

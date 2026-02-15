using System;
using System.Runtime.Serialization;

namespace ICSharpCode.Core;

[Serializable]
public class AddInLoadException : CoreException
{
	public AddInLoadException()
	{
	}

	public AddInLoadException(string message)
		: base(message)
	{
	}

	public AddInLoadException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected AddInLoadException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

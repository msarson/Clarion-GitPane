using System;
using System.Runtime.Serialization;

namespace ICSharpCode.Core;

[Serializable]
public class ResourceNotFoundException : CoreException
{
	public ResourceNotFoundException(string resource)
		: base("Resource not found : " + resource)
	{
	}

	public ResourceNotFoundException()
	{
	}

	public ResourceNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected ResourceNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

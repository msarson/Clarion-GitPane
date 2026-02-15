using System;
using System.Runtime.InteropServices;

namespace Clarion.Core.Redirection;

[Serializable]
[ComVisible(true)]
public class RedirectionException : Exception
{
	internal RedirectionException(string message)
		: base(message)
	{
	}
}

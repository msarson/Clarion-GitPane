using System;
using System.Runtime.InteropServices;

namespace Clarion.Core.Options;

[Serializable]
[ComVisible(true)]
public class OptionsException : ApplicationException
{
	internal OptionsException(string message)
		: base(message)
	{
	}

	internal OptionsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}

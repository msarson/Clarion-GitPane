using System;
using System.Runtime.InteropServices;

namespace Clarion.Core.Options;

[Serializable]
[ComVisible(true)]
public class InvalidFileDriverException : OptionsException
{
	public InvalidFileDriverException(string err)
		: base(err)
	{
	}
}

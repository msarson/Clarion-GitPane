using System;
using System.Runtime.InteropServices;
using Clarion.Core.Resources;

namespace Clarion.Core.Redirection;

[Serializable]
[ComVisible(true)]
internal class ParserException : RedirectionException
{
	private string _line;

	private string _file;

	public string Line => _line;

	public string File => _file;

	public override string Message => string.Format(IntenalResources.GetString("Redirection.Exception"), _line, _file, base.Message);

	public ParserException(string message)
		: base(message)
	{
		_file = "";
		_line = "";
	}

	public ParserException(string message, string file, string line)
		: base(message)
	{
		_file = file;
		_line = line;
	}

	public override string ToString()
	{
		return Message;
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Clarion.Core.Resources;

namespace Clarion.Core.Redirection;

[Serializable]
[ComVisible(true)]
internal class MacroException : ParserException
{
	private string macro;

	private Dictionary<string, string> macros;

	public string Macro => macro;

	public Dictionary<string, string> Macros => macros;

	public MacroException(string message, string macro, Dictionary<string, string> macros, string file, string line)
		: base(message, file, line)
	{
		this.macro = macro;
		this.macros = macros;
	}

	public MacroException(string message, string macro, string file, string line)
		: base(message, file, line)
	{
		this.macro = macro;
	}

	public override string ToString()
	{
		return string.Format(IntenalResources.GetString("Redirection.Macro.Exception"), Macro, base.File, base.Line, Message);
	}
}

using System;

namespace Clarion.Core;

public class ParsingException : Exception
{
	private string file;

	private int line;

	private int column;

	public string File => file;

	public int Line => line;

	public int Column => column;

	public ParsingException(string message, string file, int line, int column)
		: base(message)
	{
		this.file = file;
		this.line = line;
		this.column = column;
	}

	public ParsingException(string message, string file, int line)
		: this(message, file, line, 0)
	{
	}

	public ParsingException(string message, int line, int column)
		: this(message, null, line, column)
	{
	}

	public ParsingException(string message, int line)
		: this(message, null, line, 0)
	{
	}
}

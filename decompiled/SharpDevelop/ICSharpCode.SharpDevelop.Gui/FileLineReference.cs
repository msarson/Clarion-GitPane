namespace ICSharpCode.SharpDevelop.Gui;

public class FileLineReference
{
	private string fileName = string.Empty;

	private int line;

	private int column;

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}

	public int Line
	{
		get
		{
			return line;
		}
		set
		{
			line = value;
		}
	}

	public int Column
	{
		get
		{
			return column;
		}
		set
		{
			column = value;
		}
	}

	public FileLineReference(string fileName, int line, int column)
	{
		this.fileName = fileName;
		this.line = line;
		this.column = column;
	}

	public FileLineReference(string fileName, int line)
		: this(fileName, line, 0)
	{
	}

	public FileLineReference()
	{
	}
}

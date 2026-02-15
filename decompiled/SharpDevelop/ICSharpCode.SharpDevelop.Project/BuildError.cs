using System;
using System.Globalization;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

[Serializable]
public class BuildError
{
	private int column;

	private string errorCode;

	private string errorText;

	private string fileName;

	private int line;

	private bool warning;

	[NonSerialized]
	private object tag;

	private string contextMenuAddInTreeEntry;

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

	public string ErrorCode
	{
		get
		{
			return errorCode;
		}
		set
		{
			errorCode = value;
		}
	}

	public string ErrorText
	{
		get
		{
			return errorText;
		}
		set
		{
			errorText = value;
		}
	}

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

	public bool IsWarning
	{
		get
		{
			return warning;
		}
		set
		{
			warning = value;
		}
	}

	public object Tag
	{
		get
		{
			return tag;
		}
		set
		{
			tag = value;
		}
	}

	public string ContextMenuAddInTreeEntry
	{
		get
		{
			return contextMenuAddInTreeEntry;
		}
		set
		{
			contextMenuAddInTreeEntry = value;
		}
	}

	public BuildError()
	{
		line = -1;
		column = -1;
		errorCode = string.Empty;
		errorText = string.Empty;
		fileName = string.Empty;
	}

	public BuildError(string fileName, string errorText)
	{
		line = -1;
		column = -1;
		errorCode = string.Empty;
		this.errorText = errorText;
		this.fileName = fileName;
	}

	public BuildError(string fileName, int line, int column, string errorCode, string errorText)
	{
		this.line = line;
		this.column = column;
		this.errorCode = errorCode;
		this.errorText = errorText;
		this.fileName = fileName;
	}

	public override string ToString()
	{
		if (string.IsNullOrEmpty(FileName))
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} {1}: {2}", new object[3]
			{
				StringParser.Parse(IsWarning ? "${res:Global.WarningText}" : "${res:Global.ErrorText}"),
				ErrorCode,
				ErrorText
			});
		}
		return string.Format(CultureInfo.CurrentCulture, "{0}({1},{2}) : {3} {4}: {5}", FileName, Line, Column, StringParser.Parse(IsWarning ? "${res:Global.WarningText}" : "${res:Global.ErrorText}"), ErrorCode, ErrorText);
	}
}

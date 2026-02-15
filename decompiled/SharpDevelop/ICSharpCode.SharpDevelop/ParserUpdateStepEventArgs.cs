using System;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop;

public class ParserUpdateStepEventArgs : EventArgs
{
	private string fileName;

	private string content;

	private bool updated;

	private ParseInformation parseInformation;

	public string FileName => fileName;

	public string Content => content;

	public bool Updated => updated;

	public ParseInformation ParseInformation => parseInformation;

	public ParserUpdateStepEventArgs(string fileName, string content, bool updated, ParseInformation parseInformation)
	{
		this.fileName = fileName;
		this.content = content;
		this.updated = updated;
		this.parseInformation = parseInformation;
	}
}

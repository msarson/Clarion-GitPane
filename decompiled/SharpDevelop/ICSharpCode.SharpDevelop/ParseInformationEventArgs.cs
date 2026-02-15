using System;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop;

public class ParseInformationEventArgs : EventArgs
{
	private string fileName;

	private ParseInformation parseInformation;

	private ICompilationUnit compilationUnit;

	public string FileName => fileName;

	public ParseInformation ParseInformation => parseInformation;

	public ICompilationUnit CompilationUnit => compilationUnit;

	public ParseInformationEventArgs(string fileName, ParseInformation parseInformation, ICompilationUnit compilationUnit)
	{
		this.fileName = fileName;
		this.parseInformation = parseInformation;
		this.compilationUnit = compilationUnit;
	}
}

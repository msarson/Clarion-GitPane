using System;

namespace ICSharpCode.SharpDevelop;

public class FileRenameEventArgs : EventArgs
{
	private bool isDirectory;

	private string sourceFile;

	private string targetFile;

	public string SourceFile => sourceFile;

	public string TargetFile => targetFile;

	public bool IsDirectory => isDirectory;

	public FileRenameEventArgs(string sourceFile, string targetFile, bool isDirectory)
	{
		this.sourceFile = sourceFile;
		this.targetFile = targetFile;
		this.isDirectory = isDirectory;
	}
}

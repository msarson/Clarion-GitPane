using System;

namespace ICSharpCode.SharpDevelop;

public class FileEventArgs : EventArgs
{
	private string fileName;

	private bool isDirectory;

	public string FileName => fileName;

	public bool IsDirectory => isDirectory;

	public FileEventArgs(string fileName, bool isDirectory)
	{
		this.fileName = fileName;
		this.isDirectory = isDirectory;
	}
}

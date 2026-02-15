using System;

namespace ICSharpCode.Core;

public class FileNameEventArgs : EventArgs
{
	private string fileName;

	public string FileName => fileName;

	public FileNameEventArgs(string fileName)
	{
		this.fileName = fileName;
	}
}

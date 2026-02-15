namespace ICSharpCode.SharpDevelop;

public class FileRenamingEventArgs : FileRenameEventArgs
{
	private bool cancel;

	private bool operationAlreadyDone;

	public bool Cancel
	{
		get
		{
			return cancel;
		}
		set
		{
			cancel = value;
		}
	}

	public bool OperationAlreadyDone
	{
		get
		{
			return operationAlreadyDone;
		}
		set
		{
			operationAlreadyDone = value;
		}
	}

	public FileRenamingEventArgs(string sourceFile, string targetFile, bool isDirectory)
		: base(sourceFile, targetFile, isDirectory)
	{
	}
}

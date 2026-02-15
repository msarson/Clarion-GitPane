namespace ICSharpCode.SharpDevelop;

public class FileCancelEventArgs : FileEventArgs
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

	public FileCancelEventArgs(string fileName, bool isDirectory)
		: base(fileName, isDirectory)
	{
	}
}

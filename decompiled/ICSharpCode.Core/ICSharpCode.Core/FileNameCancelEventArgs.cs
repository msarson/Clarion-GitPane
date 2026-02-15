namespace ICSharpCode.Core;

public class FileNameCancelEventArgs : FileNameEventArgs
{
	private bool cancel;

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

	public FileNameCancelEventArgs(string fileName)
		: base(fileName)
	{
	}
}

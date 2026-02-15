using System.ComponentModel;

namespace SoftVelocity.Generator;

public class ApplicationLoadingEventArgs : CancelEventArgs
{
	private string fileName;

	public string FullPath => fileName;

	internal ApplicationLoadingEventArgs(string application)
	{
		fileName = application;
	}
}

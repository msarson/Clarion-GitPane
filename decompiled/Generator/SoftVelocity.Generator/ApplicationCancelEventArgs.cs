using System.ComponentModel;

namespace SoftVelocity.Generator;

public class ApplicationCancelEventArgs : CancelEventArgs
{
	private Application _Application;

	public Application Application => _Application;

	public ApplicationCancelEventArgs(Application application)
	{
		_Application = application;
	}
}

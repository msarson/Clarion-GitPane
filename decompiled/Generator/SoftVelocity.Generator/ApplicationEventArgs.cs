using System;

namespace SoftVelocity.Generator;

public class ApplicationEventArgs : EventArgs
{
	private Application _Application;

	public Application Application => _Application;

	public ApplicationEventArgs(Application application)
	{
		_Application = application;
	}
}

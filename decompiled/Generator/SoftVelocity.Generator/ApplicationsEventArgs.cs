using System;
using System.Collections.Generic;

namespace SoftVelocity.Generator;

public class ApplicationsEventArgs : EventArgs
{
	private IEnumerable<Application> _Applications;

	public IEnumerable<Application> Applications => _Applications;

	public ApplicationsEventArgs(IEnumerable<Application> applications)
	{
		_Applications = applications;
	}
}

using System.Collections.Generic;
using System.ComponentModel;

namespace SoftVelocity.Generator;

public class ApplicationsCancelEventArgs : CancelEventArgs
{
	private IEnumerable<Application> _Applications;

	public IEnumerable<Application> Applications => _Applications;

	public ApplicationsCancelEventArgs(IEnumerable<Application> applications)
	{
		_Applications = applications;
	}
}

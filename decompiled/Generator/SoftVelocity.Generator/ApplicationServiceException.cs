using System;

namespace SoftVelocity.Generator;

internal class ApplicationServiceException : ApplicationException
{
	private string _ApplicationName;

	public string ApplicationName => _ApplicationName;

	public ApplicationServiceException(string applicationName)
	{
		_ApplicationName = applicationName;
	}

	public ApplicationServiceException(string applicationName, string message)
		: base(message)
	{
		_ApplicationName = applicationName;
	}
}

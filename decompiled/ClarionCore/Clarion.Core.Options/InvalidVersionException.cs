using System;
using System.Runtime.InteropServices;
using Clarion.Core.Resources;

namespace Clarion.Core.Options;

[Serializable]
[ComVisible(true)]
public class InvalidVersionException : OptionsException
{
	private string _Version = string.Empty;

	public string Version => _Version;

	internal InvalidVersionException(string version)
		: base(string.Format(IntenalResources.GetString("Clarion.Options.InvalidVersion"), version))
	{
		if (!string.IsNullOrEmpty(version))
		{
			_Version = version.Trim();
		}
	}

	internal InvalidVersionException(string version, Exception innerException)
		: base(string.Format(IntenalResources.GetString("Clarion.Options.InvalidVersion"), version), innerException)
	{
		if (!string.IsNullOrEmpty(version))
		{
			_Version = version.Trim();
		}
	}
}

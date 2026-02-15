using System.Net;

namespace ICSharpCode.SharpDevelop.Gui;

public class DiscoveryNetworkCredential : NetworkCredential
{
	public const string DefaultAuthenticationType = "Default";

	private string authenticationType = string.Empty;

	public string AuthenticationType => authenticationType;

	public bool IsDefaultAuthenticationType => string.Compare(authenticationType, "Default", ignoreCase: true) == 0;

	public DiscoveryNetworkCredential(string userName, string password, string domain, string authenticationType)
		: base(userName, password, domain)
	{
		this.authenticationType = authenticationType;
	}

	public DiscoveryNetworkCredential(NetworkCredential credential, string authenticationType)
		: this(credential.UserName, credential.Password, credential.Domain, authenticationType)
	{
	}
}

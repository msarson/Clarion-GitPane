using System;

namespace ICSharpCode.SharpDevelop.Gui;

public class AsyncDiscoveryState
{
	private WebServiceDiscoveryClientProtocol protocol;

	private Uri uri;

	private DiscoveryNetworkCredential credential;

	public WebServiceDiscoveryClientProtocol Protocol => protocol;

	public Uri Uri => uri;

	public DiscoveryNetworkCredential Credential => credential;

	public AsyncDiscoveryState(WebServiceDiscoveryClientProtocol protocol, Uri uri, DiscoveryNetworkCredential credential)
	{
		this.protocol = protocol;
		this.uri = uri;
		this.credential = credential;
	}
}

using System.Net;
using System.Web.Services.Discovery;

namespace ICSharpCode.SharpDevelop.Gui;

public class WebServiceDiscoveryClientProtocol : DiscoveryClientProtocol
{
	private HttpWebResponse lastResponseReceived;

	public bool IsAuthenticationRequired
	{
		get
		{
			if (lastResponseReceived != null)
			{
				return lastResponseReceived.StatusCode == HttpStatusCode.Unauthorized;
			}
			return false;
		}
	}

	public HttpAuthenticationHeader GetAuthenticationHeader()
	{
		if (lastResponseReceived != null)
		{
			return new HttpAuthenticationHeader(lastResponseReceived.Headers);
		}
		return null;
	}

	protected override WebResponse GetWebResponse(WebRequest request)
	{
		WebResponse webResponse = base.GetWebResponse(request);
		lastResponseReceived = webResponse as HttpWebResponse;
		return webResponse;
	}
}

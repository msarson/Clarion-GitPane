using System.Net;
using System.Text;

namespace ICSharpCode.SharpDevelop.Gui;

public class HttpAuthenticationHeader
{
	private string[] authenticationSchemes;

	public string AuthenticationType
	{
		get
		{
			if (HasAuthenticationSchemes)
			{
				int num = 0;
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < authenticationSchemes.Length; i++)
				{
					string text = authenticationSchemes[i];
					int num2 = text.IndexOf(' ');
					if (num2 > 0)
					{
						text = text.Substring(0, num2);
					}
					if (num > 0)
					{
						stringBuilder.Append(",");
					}
					stringBuilder.Append(text);
					num++;
				}
				return stringBuilder.ToString();
			}
			return string.Empty;
		}
	}

	private bool HasAuthenticationSchemes
	{
		get
		{
			if (authenticationSchemes != null)
			{
				return authenticationSchemes.Length > 0;
			}
			return false;
		}
	}

	public HttpAuthenticationHeader(WebHeaderCollection headers)
	{
		authenticationSchemes = headers.GetValues("WWW-Authenticate");
	}

	public override string ToString()
	{
		if (HasAuthenticationSchemes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = authenticationSchemes;
			foreach (string value in array)
			{
				stringBuilder.Append("WWW-Authenticate: ");
				stringBuilder.Append(value);
				stringBuilder.Append("\r\n");
			}
			return stringBuilder.ToString();
		}
		return string.Empty;
	}
}

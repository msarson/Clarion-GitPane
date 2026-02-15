namespace ICSharpCode.SharpDevelop.Gui.StartPage;

public class MenuItem
{
	public string Caption;

	public string URL;

	public string Id;

	public MenuItem(string strCaption, string strUrl, string id)
	{
		Caption = strCaption;
		URL = strUrl;
		Id = id;
	}
}

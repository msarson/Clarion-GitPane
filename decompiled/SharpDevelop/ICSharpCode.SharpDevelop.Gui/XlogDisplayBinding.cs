namespace ICSharpCode.SharpDevelop.Gui;

public class XlogDisplayBinding : IDisplayBinding
{
	public bool CanCreateContentForFile(string fileName)
	{
		return fileName.ToLower().EndsWith(".xlog");
	}

	public IViewContent CreateContentForFile(string fileName)
	{
		XlogViewContent xlogViewContent = new XlogViewContent();
		xlogViewContent.Load(fileName);
		return xlogViewContent;
	}

	public bool CanCreateContentForLanguage(string languageName)
	{
		return false;
	}

	public IViewContent CreateContentForLanguage(string languageName, string content)
	{
		return null;
	}
}

using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class BrowserDisplayBinding : IDisplayBinding
{
	public bool CanCreateContentForFile(string fileName)
	{
		if (!fileName.StartsWith("http:") && !fileName.StartsWith("https:") && !fileName.StartsWith("ftp:"))
		{
			return fileName.StartsWith("browser://");
		}
		return true;
	}

	public bool CanCreateContentForLanguage(string language)
	{
		return false;
	}

	public IViewContent CreateContentForFile(string fileName)
	{
		BrowserPane browserPane = new BrowserPane();
		if (fileName.StartsWith("browser://"))
		{
			browserPane.Load(fileName.Substring("browser://".Length));
		}
		else
		{
			browserPane.Load(fileName);
		}
		return browserPane;
	}

	public IViewContent CreateContentForLanguage(string language, string content)
	{
		return null;
	}
}

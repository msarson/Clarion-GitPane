using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public interface IDisplayBinding
{
	bool CanCreateContentForFile(string fileName);

	IViewContent CreateContentForFile(string fileName);

	bool CanCreateContentForLanguage(string languageName);

	IViewContent CreateContentForLanguage(string languageName, string content);
}

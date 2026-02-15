using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.Gui;

public interface IParseInformationListener
{
	void ParseInformationUpdated(ParseInformation parseInfo);
}

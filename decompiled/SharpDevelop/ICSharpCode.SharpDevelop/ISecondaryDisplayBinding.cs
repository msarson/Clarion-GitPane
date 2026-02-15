using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public interface ISecondaryDisplayBinding
{
	bool ReattachWhenParserServiceIsReady { get; }

	bool CanAttachTo(IViewContent content);

	ISecondaryViewContent[] CreateSecondaryViewContent(IViewContent viewContent);
}

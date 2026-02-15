using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator;

namespace SoftVelocity.CWPInvoke;

internal class CWDialogDisplayBinding : ISecondaryDisplayBinding
{
	public bool ReattachWhenParserServiceIsReady => false;

	public bool CanAttachTo(IViewContent content)
	{
		return content is IAppViewContentEvents;
	}

	public ISecondaryViewContent[] CreateSecondaryViewContent(IViewContent viewContent)
	{
		if (viewContent.SecondaryViewContents.Exists((ISecondaryViewContent c) => ((object)c).GetType() == typeof(CWDialogViewContent)))
		{
			return (ISecondaryViewContent[])(object)new ISecondaryViewContent[0];
		}
		return (ISecondaryViewContent[])(object)new ISecondaryViewContent[1] { (ISecondaryViewContent)new CWDialogViewContent() };
	}
}

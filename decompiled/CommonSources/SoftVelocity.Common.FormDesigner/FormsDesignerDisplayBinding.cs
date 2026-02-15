using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.FormDesigner;

public abstract class FormsDesignerDisplayBinding : ISecondaryDisplayBinding
{
	public virtual bool ReattachWhenParserServiceIsReady => true;

	protected abstract bool _CanAttachTo(IViewContent viewContent);

	protected abstract ISecondaryViewContent[] _CreateSecondaryViewContent(IViewContent viewContent);

	public bool CanAttachTo(IViewContent viewContent)
	{
		return _CanAttachTo(viewContent);
	}

	public ISecondaryViewContent[] CreateSecondaryViewContent(IViewContent viewContent)
	{
		return _CreateSecondaryViewContent(viewContent);
	}
}

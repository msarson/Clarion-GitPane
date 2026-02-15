using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class WebReferenceNode : DirectoryNode
{
	public WebReferenceNode(WebReference webReference)
		: this(webReference.Directory)
	{
		base.ProjectItem = webReference.WebReferenceUrl;
	}

	public WebReferenceNode(string directory)
		: base(directory)
	{
		base.SpecialFolder = SpecialFolder.WebReference;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/WebReferenceNode";
	}
}

namespace ICSharpCode.SharpDevelop.Project;

public class WebReferencesFolderNode : DirectoryNode
{
	public WebReferencesFolderNode(WebReferencesProjectItem projectItem)
		: this(projectItem.Directory)
	{
		base.ProjectItem = projectItem;
	}

	public WebReferencesFolderNode(string directory)
		: base(directory)
	{
		sortOrder = 0;
		base.SpecialFolder = SpecialFolder.WebReferencesFolder;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/WebReferencesFolderNode";
	}
}

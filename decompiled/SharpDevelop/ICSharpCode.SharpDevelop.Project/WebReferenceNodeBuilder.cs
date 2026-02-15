using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class WebReferenceNodeBuilder
{
	private WebReferenceNodeBuilder()
	{
	}

	public static TreeNode AddWebReferencesFolderNode(ProjectNode projectNode, WebReference webReference)
	{
		if (webReference.WebReferencesProjectItem != null)
		{
			WebReferencesFolderNode webReferencesFolderNode = new WebReferencesFolderNode(webReference.WebReferencesProjectItem);
			webReferencesFolderNode.FileNodeStatus = FileNodeStatus.InProject;
			webReferencesFolderNode.AddTo(projectNode);
			return webReferencesFolderNode;
		}
		return null;
	}

	public static TreeNode AddWebReference(WebReferencesFolderNode webReferencesFolderNode, WebReference webReference)
	{
		WebReferenceNode webReferenceNode = new WebReferenceNode(webReference);
		webReferenceNode.FileNodeStatus = FileNodeStatus.InProject;
		webReferenceNode.AddTo(webReferencesFolderNode);
		return webReferenceNode;
	}
}

using System.Net;
using System.Web.Services.Discovery;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class RefreshWebReference : AbstractMenuCommand
{
	public override void Run()
	{
		if (!(Owner is WebReferenceNode { Project: not null, ProjectItem: not null } webReferenceNode))
		{
			return;
		}
		WebReferenceUrl webReferenceUrl = (WebReferenceUrl)webReferenceNode.ProjectItem;
		try
		{
			DiscoveryClientProtocol discoveryClientProtocol = DiscoverWebServices(webReferenceUrl.UpdateFromURL);
			if (discoveryClientProtocol == null)
			{
				return;
			}
			WebReference webReference = new WebReference(webReferenceUrl.Project, webReferenceUrl.UpdateFromURL, webReferenceNode.Text, webReferenceUrl.Namespace, discoveryClientProtocol);
			webReference.Save();
			WebReferenceChanges changes = webReference.GetChanges(webReferenceUrl.Project);
			if (changes.Changed)
			{
				foreach (ProjectItem item in changes.ItemsRemoved)
				{
					ProjectService.RemoveProjectItem(webReferenceUrl.Project, item);
					FileService.RemoveFile(item.FileName, isDirectory: false);
				}
				foreach (ProjectItem newItem in changes.NewItems)
				{
					ProjectService.AddProjectItem(webReferenceUrl.Project, newItem);
					FileNode fileNode = new FileNode(newItem.FileName, FileNodeStatus.InProject);
					fileNode.AddTo(webReferenceNode);
				}
				ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.Sort();
				webReferenceUrl.Project.Save();
			}
			ParserService.ParseFile(webReference.WebProxyFileName);
		}
		catch (WebException ex)
		{
			MessageService.ShowError(ex, string.Format(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ProjectBrowser.RefreshWebReference.ReadServiceDescriptionError}"), webReferenceUrl.UpdateFromURL));
		}
	}

	private DiscoveryClientProtocol DiscoverWebServices(string url)
	{
		WebServiceDiscoveryClientProtocol webServiceDiscoveryClientProtocol = new WebServiceDiscoveryClientProtocol();
		NetworkCredential credentials = CredentialCache.DefaultNetworkCredentials;
		bool flag = true;
		while (flag)
		{
			try
			{
				webServiceDiscoveryClientProtocol.Credentials = credentials;
				webServiceDiscoveryClientProtocol.DiscoverAny(url);
				webServiceDiscoveryClientProtocol.ResolveOneLevel();
				return webServiceDiscoveryClientProtocol;
			}
			catch (WebException ex)
			{
				if (webServiceDiscoveryClientProtocol.IsAuthenticationRequired)
				{
					using (UserCredentialsDialog userCredentialsDialog = new UserCredentialsDialog(url, webServiceDiscoveryClientProtocol.GetAuthenticationHeader().AuthenticationType))
					{
						if (userCredentialsDialog.ShowDialog() == DialogResult.OK)
						{
							credentials = userCredentialsDialog.Credential;
						}
						else
						{
							flag = false;
						}
					}
					continue;
				}
				throw ex;
			}
		}
		return null;
	}
}

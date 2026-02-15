using System.IO;
using Clarion.Core.Redirection;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Common.Redirection;

internal class CreateRedirectionFile : EditRedirectionFile
{
	protected override void AfterLoad()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		ProjectNode val = (ProjectNode)ProjectBrowserPad.Instance.SelectedNode;
		CommonClarionProject commonClarionProject = (CommonClarionProject)(object)((AbstractProjectBrowserTreeNode)val).Project;
		try
		{
			redFile = new RedirectionFile(((AbstractProject)commonClarionProject).Directory, commonClarionProject.Version, commonClarionProject.IsWin, false);
		}
		catch (FileNotFoundException)
		{
			if (commonClarionProject.Version == "")
			{
				redFile = RedirectionFile.Create(((AbstractProject)commonClarionProject).Directory, commonClarionProject.IsWin);
			}
			else
			{
				redFile = RedirectionFile.Create(((AbstractProject)commonClarionProject).Directory, commonClarionProject.Version);
			}
			redFile.ActiveSection = ((AbstractProject)commonClarionProject).ActiveConfiguration;
		}
	}
}

using System.Windows.Forms;
using Clarion.Core.Options;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Common;

public class SetVersionMenuBuilder : VersionBasedMenuBuilder
{
	public SetVersionMenuBuilder()
	{
		baseMenuText = "${res:MainMenu.BuildMenu.SetVersion}";
		limitByProjectType = true;
	}

	public override void MenuItemSelected(string version, bool forWindows)
	{
		try
		{
			Versions.SetActiveVersion(version, forWindows);
			WorkbenchSingleton.Workbench.SetProjectTitle(ProjectService.CurrentProject);
		}
		catch (VersionChangedNotAllowedException)
		{
		}
	}

	protected override bool TickThis(string version, bool forWindows)
	{
		string activeVersion = Versions.GetActiveVersion(forWindows);
		if (!(version == activeVersion))
		{
			if (string.IsNullOrEmpty(version))
			{
				return string.IsNullOrEmpty(activeVersion);
			}
			return false;
		}
		return true;
	}

	public override ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		return base.BuildSubmenu(codon, owner);
	}
}

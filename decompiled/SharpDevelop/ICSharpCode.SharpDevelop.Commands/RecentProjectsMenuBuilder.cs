using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Commands;

public class RecentProjectsMenuBuilder : ISubmenuBuilder
{
	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		RecentOpen recentOpen = FileService.RecentOpen;
		IList<RecentOpen.RecentOpenDescription> recentsFromCategory = recentOpen.GetRecentsFromCategory(RecentOpen.defaultTypeProjects);
		if (recentsFromCategory.Count > 0)
		{
			List<MenuCommand> list = new List<MenuCommand>();
			int maximumEntriesPerCategory = RecentOpen.MaximumEntriesPerCategory;
			for (int i = 0; i < recentsFromCategory.Count && i < maximumEntriesPerCategory; i++)
			{
				int num = i + 1;
				string text = ((num >= 10) ? ((num != 10) ? num.ToString() : "1&0") : ("&" + num));
				MenuCommand menuCommand = new MenuCommand(text + " " + recentsFromCategory[i].FileName, LoadRecentProject);
				menuCommand.Tag = recentsFromCategory[i].FileName;
				menuCommand.Description = StringParser.Parse(ResourceService.GetString("Dialog.Componnents.RichMenuItem.LoadProjectDescription"), new string[1, 2] { 
				{
					"PROJECT",
					recentsFromCategory[i].FileName
				} });
				list.Add(menuCommand);
			}
			return list.ToArray();
		}
		MenuCommand menuCommand2 = new MenuCommand("${res:Dialog.Componnents.RichMenuItem.NoRecentProjectsString}");
		menuCommand2.Enabled = false;
		return new MenuCommand[1] { menuCommand2 };
	}

	private void LoadRecentProject(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		string fileName = menuCommand.Tag.ToString();
		FileUtility.ObservedLoad(ProjectService.LoadSolution, fileName);
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class RecentFilesMenuBuilder : ISubmenuBuilder
{
	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		RecentOpen recentOpen = FileService.RecentOpen;
		IList<RecentOpen.RecentOpenDescription> recentsFromCategory = recentOpen.GetRecentsFromCategory(RecentOpen.defaultTypeFiles);
		if (recentsFromCategory.Count > 0)
		{
			List<MenuCommand> list = new List<MenuCommand>();
			int maximumEntriesPerCategory = RecentOpen.MaximumEntriesPerCategory;
			for (int i = 0; i < recentsFromCategory.Count && i < maximumEntriesPerCategory; i++)
			{
				int num = i + 1;
				string text = ((num >= 10) ? ((num != 10) ? num.ToString() : "1&0") : ("&" + num));
				MenuCommand menuCommand = new MenuCommand(text + " " + recentsFromCategory[i].FileName, LoadRecentFile);
				menuCommand.Tag = recentsFromCategory[i].FileName;
				menuCommand.Description = StringParser.Parse(ResourceService.GetString("Dialog.Componnents.RichMenuItem.LoadFileDescription"), new string[1, 2] { 
				{
					"FILE",
					recentsFromCategory[i].FileName
				} });
				list.Add(menuCommand);
			}
			return list.ToArray();
		}
		MenuCommand menuCommand2 = new MenuCommand("${res:Dialog.Componnents.RichMenuItem.NoRecentFilesString}");
		menuCommand2.Enabled = false;
		return new MenuCommand[1] { menuCommand2 };
	}

	private void LoadRecentFile(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		FileService.OpenFile(menuCommand.Tag.ToString());
	}
}

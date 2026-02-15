using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Project.Commands;

namespace ICSharpCode.SharpDevelop.Commands;

public class QuickOpenFileMenuBuilder : ISubmenuBuilder
{
	public class OpenProjectMenuItem : ToolStripMenuItem
	{
		private string fileFilterExtension;

		public OpenProjectMenuItem(string name, string extension)
		{
			Text = StringParser.Parse(name);
			fileFilterExtension = extension;
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if (!string.IsNullOrEmpty(fileFilterExtension))
			{
				using (ICSharpCode.SharpDevelop.Project.Commands.LoadSolution loadSolution = new ICSharpCode.SharpDevelop.Project.Commands.LoadSolution())
				{
					loadSolution.DefaultFileFilterExtension = fileFilterExtension;
					loadSolution.Run();
				}
			}
		}
	}

	public class OpenFileFilterMenuItem : ToolStripMenuItem
	{
		private string fileFilterExtension;

		public OpenFileFilterMenuItem(string name, string extension)
		{
			Text = StringParser.Parse(name);
			fileFilterExtension = extension;
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if (!string.IsNullOrEmpty(fileFilterExtension))
			{
				using (OpenFile openFile = new OpenFile())
				{
					openFile.DefaultFileFilterExtension = fileFilterExtension;
					openFile.Run();
				}
			}
		}
	}

	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		string allProjectsFilter = ProjectService.GetAllProjectsFilter(this);
		List<Codon> codons = AddInTree.GetTreeNode("/SharpDevelop/Workbench/QuickOpen/FileFilter").Codons;
		List<ToolStripItem> list = new List<ToolStripItem>();
		Dictionary<string, ToolStripMenuItem> dictionary = new Dictionary<string, ToolStripMenuItem>(StringComparer.OrdinalIgnoreCase);
		foreach (Codon item in codons)
		{
			if (item.Name == "FileFilter")
			{
				if (allProjectsFilter.IndexOf(item.Properties["extensions"]) > 0)
				{
					dictionary.Add(StringParser.Parse(item.Properties["name"]).Trim(), new OpenProjectMenuItem(item.Properties["name"], item.Properties["extensions"]));
				}
				else
				{
					dictionary.Add(StringParser.Parse(item.Properties["name"]).Trim(), new OpenFileFilterMenuItem(item.Properties["name"], item.Properties["extensions"]));
				}
			}
		}
		int num = 0;
		List<string> list2 = new List<string>();
		list2.AddRange(dictionary.Keys);
		list2.Sort(StringComparer.OrdinalIgnoreCase);
		foreach (string item2 in list2)
		{
			list.Add(dictionary[item2]);
			num++;
		}
		if (codons.Count > num)
		{
			list.Insert(0, new ToolStripSeparator());
			MenuCommand menuCommand = null;
			foreach (Codon item3 in codons)
			{
				if (item3.Name == "MenuItem")
				{
					menuCommand = new MenuCommand(item3, WorkbenchSingleton.Workbench, createCommand: true);
					switch (item3.GetFailedAction(owner))
					{
					case ConditionFailedAction.Nothing:
						list.Insert(0, menuCommand);
						break;
					case ConditionFailedAction.Disable:
						menuCommand.Enabled = false;
						list.Insert(0, menuCommand);
						break;
					}
				}
			}
		}
		return list.ToArray();
	}
}

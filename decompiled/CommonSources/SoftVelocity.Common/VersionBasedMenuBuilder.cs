using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Clarion.Core.Options;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Common;

public abstract class VersionBasedMenuBuilder : ISubmenuBuilder
{
	private delegate void DoClick(string version, bool forWindows);

	private class MenuComparer : IComparer<ToolStripItem>
	{
		public int Compare(ToolStripItem x, ToolStripItem y)
		{
			return x.Text.CompareTo(y.Text);
		}
	}

	private class VersionMenuItem
	{
		private string version;

		private bool forWindows;

		private DoClick click;

		public VersionMenuItem(string version, bool forWindows, DoClick cb)
		{
			this.version = version;
			this.forWindows = forWindows;
			click = cb;
		}

		public VersionMenuItem(bool forWindows, DoClick cb)
			: this(null, forWindows, cb)
		{
		}

		public void ProcessClick(object sender, EventArgs e)
		{
			click(version, forWindows);
		}
	}

	protected string baseMenuText;

	protected bool limitByProjectType;

	public VersionBasedMenuBuilder()
	{
		limitByProjectType = false;
	}

	public abstract void MenuItemSelected(string version, bool forWindows);

	protected virtual bool TickThis(string version, bool forWindows)
	{
		return false;
	}

	private ToolStripItem BuildSubmenu(Codon codon, bool forWindows, bool isBase)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		string[] array = Versions.VersionList(forWindows);
		int num = array.Length;
		string text = (isBase ? baseMenuText : ((!forWindows) ? "${res:MainMenu.ToolsMenu.ClarionVersion.DotNET}" : "${res:MainMenu.ToolsMenu.ClarionVersion.Windows}"));
		if (num != 1)
		{
			List<ToolStripItem> list = new List<ToolStripItem>(num + 1);
			MenuCommand val = new MenuCommand(StringParser.Parse("${res:MainMenu.ToolsMenu.ClarionVersion.CurrentVersion}"), (EventHandler)new VersionMenuItem(null, forWindows, MenuItemSelected).ProcessClick);
			((ToolStripMenuItem)(object)val).Checked = TickThis(null, forWindows);
			list.Add((ToolStripItem)(object)val);
			for (int i = 0; i < num; i++)
			{
				val = new MenuCommand(array[i], (EventHandler)new VersionMenuItem(array[i], forWindows, MenuItemSelected).ProcessClick);
				((ToolStripMenuItem)(object)val).Checked = TickThis(array[i], forWindows);
				list.Add((ToolStripItem)(object)val);
			}
			list.Sort(1, num, new MenuComparer());
			return (ToolStripItem)new Menu(text, list.ToArray());
		}
		return (ToolStripItem)new MenuCommand(text, (EventHandler)new VersionMenuItem(forWindows, MenuItemSelected).ProcessClick);
	}

	public virtual ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		ToolStripItem[] array = new ToolStripItem[1];
		_ = AddInTree.AddIns;
		bool flag = false;
		bool flag2 = false;
		if (limitByProjectType)
		{
			CommonClarionProject commonClarionProject = ProjectService.CurrentProject as CommonClarionProject;
			if (commonClarionProject == null)
			{
				Solution openSolution = ProjectService.OpenSolution;
				if (openSolution != null)
				{
					foreach (IProject project in openSolution.Projects)
					{
						commonClarionProject = project as CommonClarionProject;
						if (commonClarionProject != null)
						{
							break;
						}
					}
				}
			}
			if (commonClarionProject != null)
			{
				if (commonClarionProject.IsWin)
				{
					flag = true;
				}
				else
				{
					flag2 = true;
				}
			}
		}
		if (!limitByProjectType || (!flag && !flag2))
		{
			flag = ClarionAddins.Win32Present;
			flag2 = ClarionAddins.DotNetPresent;
		}
		if (flag)
		{
			if (flag2)
			{
				ToolStripItem[] array2 = new ToolStripItem[2]
				{
					BuildSubmenu(codon, forWindows: true, isBase: false),
					BuildSubmenu(codon, forWindows: false, isBase: false)
				};
				array[0] = (ToolStripItem)new Menu(baseMenuText, array2);
			}
			else
			{
				array[0] = BuildSubmenu(codon, forWindows: true, isBase: true);
			}
		}
		else if (flag2)
		{
			array[0] = BuildSubmenu(codon, forWindows: false, isBase: true);
		}
		else
		{
			array[0] = (ToolStripItem)new MenuSeparator();
		}
		return array;
	}
}

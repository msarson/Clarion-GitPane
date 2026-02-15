using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class SetConfigurationMenuBuilder : ISubmenuBuilder
{
	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		if (ProjectService.OpenSolution == null)
		{
			return new ToolStripItem[0];
		}
		IList<string> configurationNames = ProjectService.OpenSolution.GetConfigurationNames();
		string activeConfiguration = ProjectService.OpenSolution.Preferences.ActiveConfiguration;
		ToolStripMenuItem[] array = new ToolStripMenuItem[configurationNames.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new ToolStripMenuItem(configurationNames[i]);
			array[i].Click += SetConfigurationItemClick;
			array[i].Checked = activeConfiguration == configurationNames[i];
		}
		return array;
	}

	private void SetConfigurationItemClick(object sender, EventArgs e)
	{
		ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
		ProjectService.SetConfiguration(toolStripMenuItem.Text);
		ProjectBrowserPad.Instance.ProjectBrowserControl.RefreshView();
	}
}

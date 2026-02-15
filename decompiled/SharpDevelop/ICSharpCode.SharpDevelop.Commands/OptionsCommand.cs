using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class OptionsCommand : AbstractMenuCommand
{
	public static void ShowTabbedOptions(string dialogTitle, AddInTreeNode node)
	{
		using TabbedOptions tabbedOptions = new TabbedOptions(dialogTitle, PropertyService.Get("TextEditorSettings", new Properties()), node);
		tabbedOptions.ShowDialog(WorkbenchSingleton.MainForm);
		if (tabbedOptions.DialogResult == DialogResult.OK)
		{
			PropertyService.Save();
		}
	}

	public override void Run()
	{
		using TreeViewOptions treeViewOptions = new TreeViewOptions(PropertyService.Get("TextEditorSettings", new Properties()), AddInTree.GetTreeNode("/SharpDevelop/Dialogs/OptionsDialog"));
		treeViewOptions.Owner = (Form)WorkbenchSingleton.Workbench;
		treeViewOptions.ShowDialog(WorkbenchSingleton.MainForm);
		if (treeViewOptions.DialogResult == DialogResult.OK)
		{
			PropertyService.Save();
		}
	}
}

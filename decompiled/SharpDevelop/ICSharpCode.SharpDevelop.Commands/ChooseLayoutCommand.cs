using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class ChooseLayoutCommand : AbstractComboBoxCommand, ISubmenuBuilder
{
	private int editIndex = -1;

	private int resetIndex = -1;

	private int oldItem;

	private bool editingLayout;

	private bool clickingMenu;

	private static IEnumerable<string> CustomLayoutNames
	{
		get
		{
			foreach (LayoutConfiguration layout in LayoutConfiguration.Layouts)
			{
				if (layout.Custom)
				{
					yield return layout.Name;
				}
			}
		}
	}

	public ChooseLayoutCommand()
	{
		LayoutConfiguration.LayoutChanged += LayoutChanged;
		string[] defaultLayouts = LayoutConfiguration.DefaultLayouts;
		foreach (string text in defaultLayouts)
		{
			LayoutConfiguration layout = LayoutConfiguration.GetLayout(text);
			if (layout != null)
			{
				layout.DisplayName = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand." + text + "Item}");
			}
		}
	}

	public override void Run()
	{
		if (!editingLayout && !clickingMenu)
		{
			ComboBox comboBox = ((ToolBarComboBox)Owner).ComboBox;
			Path.Combine(PropertyService.DataDirectory, "resources" + Path.DirectorySeparatorChar + "layouts");
			string path = Path.Combine(PropertyService.ConfigDirectory, "layouts");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (oldItem != editIndex && oldItem != resetIndex)
			{
				WorkbenchSingleton.Workbench.WorkbenchLayout.StoreConfiguration();
			}
			if (comboBox.SelectedIndex == editIndex)
			{
				editingLayout = true;
				ShowLayoutEditor();
				OnOwnerChanged(EventArgs.Empty);
				editingLayout = false;
			}
			else if (comboBox.SelectedIndex == resetIndex)
			{
				ResetToDefaults();
			}
			else
			{
				LayoutConfiguration layoutConfiguration = LayoutConfiguration.Layouts[comboBox.SelectedIndex];
				LayoutConfiguration.CurrentLayoutName = layoutConfiguration.Name;
			}
			oldItem = comboBox.SelectedIndex;
		}
	}

	private void ShowLayoutEditor()
	{
		using Form form = new Form();
		form.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.EditLayouts.Title}");
		StringListEditor stringListEditor = new StringListEditor();
		stringListEditor.Dock = DockStyle.Fill;
		stringListEditor.ManualOrder = false;
		stringListEditor.BrowseForDirectory = false;
		stringListEditor.TitleText = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.EditLayouts.Label}");
		stringListEditor.AddButtonText = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.EditLayouts.AddLayout}");
		stringListEditor.LoadList(CustomLayoutNames);
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
		flowLayoutPanel.Dock = DockStyle.Bottom;
		flowLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
		Button button = new Button();
		flowLayoutPanel.Height = button.Height + 8;
		button.DialogResult = DialogResult.Cancel;
		button.Text = ResourceService.GetString("Global.CancelButtonText");
		form.CancelButton = button;
		flowLayoutPanel.Controls.Add(button);
		button = new Button();
		button.DialogResult = DialogResult.OK;
		button.Text = ResourceService.GetString("Global.OKButtonText");
		form.AcceptButton = button;
		flowLayoutPanel.Controls.Add(button);
		form.Controls.Add(stringListEditor);
		form.Controls.Add(flowLayoutPanel);
		form.FormBorderStyle = FormBorderStyle.FixedDialog;
		form.MaximizeBox = false;
		form.MinimizeBox = false;
		form.ClientSize = new Size(400, 300);
		form.StartPosition = FormStartPosition.CenterParent;
		if (form.ShowDialog(WorkbenchSingleton.MainForm) != DialogResult.OK)
		{
			return;
		}
		IList<string> list = new List<string>(CustomLayoutNames);
		IList<string> newNames = stringListEditor.GetList();
		foreach (string item in newNames)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
				LayoutConfiguration.CreateCustom(item);
			}
		}
		LayoutConfiguration.Layouts.RemoveAll((LayoutConfiguration lc) => lc.Custom && !newNames.Contains(lc.Name));
		LayoutConfiguration.SaveCustomLayoutConfiguration();
	}

	private void ResetToDefaults()
	{
		if (!MessageService.AskQuestion("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.ResetToDefaultsQuestion}"))
		{
			return;
		}
		foreach (LayoutConfiguration layout in LayoutConfiguration.Layouts)
		{
			string path = Path.Combine(PropertyService.ConfigDirectory, "layouts");
			string path2 = Path.Combine(PropertyService.DataDirectory, "resources" + Path.DirectorySeparatorChar + "layouts");
			if (File.Exists(Path.Combine(path2, layout.FileName)) && File.Exists(Path.Combine(path, layout.FileName)))
			{
				try
				{
					File.Delete(Path.Combine(path, layout.FileName));
				}
				catch (Exception)
				{
				}
			}
		}
		WorkbenchSingleton.Workbench.WorkbenchLayout.LoadConfiguration();
		LayoutChanged(null, null);
	}

	private void LayoutChanged(object sender, EventArgs e)
	{
		if (editingLayout || clickingMenu || Owner == null || !(Owner is ToolBarComboBox))
		{
			return;
		}
		ToolBarComboBox toolBarComboBox = (ToolBarComboBox)Owner;
		ComboBox comboBox = toolBarComboBox.ComboBox;
		for (int i = 0; i < comboBox.Items.Count; i++)
		{
			if (((LayoutConfiguration)comboBox.Items[i]).Name == LayoutConfiguration.CurrentLayoutName)
			{
				comboBox.SelectedIndex = i;
				break;
			}
		}
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		if (clickingMenu)
		{
			return;
		}
		ToolBarComboBox toolBarComboBox = (ToolBarComboBox)Owner;
		ComboBox comboBox = toolBarComboBox.ComboBox;
		comboBox.Items.Clear();
		int selectedIndex = 0;
		foreach (LayoutConfiguration layout in LayoutConfiguration.Layouts)
		{
			if (LayoutConfiguration.CurrentLayoutName == layout.Name)
			{
				selectedIndex = comboBox.Items.Count;
			}
			comboBox.Items.Add(layout);
		}
		editIndex = comboBox.Items.Count;
		comboBox.Items.Add(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.EditItem}"));
		resetIndex = comboBox.Items.Count;
		comboBox.Items.Add(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.ResetToDefaultItem}"));
		comboBox.SelectedIndex = selectedIndex;
	}

	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		Path.Combine(PropertyService.DataDirectory, "resources" + Path.DirectorySeparatorChar + "layouts");
		string path = Path.Combine(PropertyService.ConfigDirectory, "layouts");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		if (LayoutConfiguration.Layouts == null || LayoutConfiguration.Layouts.Count == 0)
		{
			return new ToolStripItem[0];
		}
		List<ToolStripMenuItem> list = new List<ToolStripMenuItem>();
		ToolStripMenuItem toolStripMenuItem = null;
		foreach (LayoutConfiguration layout in LayoutConfiguration.Layouts)
		{
			toolStripMenuItem = new ToolStripMenuItem(layout.Name);
			if (LayoutConfiguration.CurrentLayoutName == layout.Name)
			{
				toolStripMenuItem.Checked = true;
			}
			list.Add(toolStripMenuItem);
			toolStripMenuItem.Click += SetLayoutItemClick;
		}
		toolStripMenuItem = new ToolStripMenuItem(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.EditItem}"));
		list.Add(toolStripMenuItem);
		toolStripMenuItem.Click += SetLayoutItemClick;
		toolStripMenuItem = new ToolStripMenuItem(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.ResetToDefaultItem}"));
		list.Add(toolStripMenuItem);
		toolStripMenuItem.Click += SetLayoutItemClick;
		return list.ToArray();
	}

	private void SetLayoutItemClick(object sender, EventArgs e)
	{
		clickingMenu = true;
		ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
		if (toolStripMenuItem.Text == StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.EditItem}"))
		{
			editingLayout = true;
			ShowLayoutEditor();
			OnOwnerChanged(EventArgs.Empty);
			editingLayout = false;
		}
		else if (toolStripMenuItem.Text == StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ChooseLayoutCommand.ResetToDefaultItem}"))
		{
			ResetToDefaults();
		}
		else
		{
			LayoutConfiguration.CurrentLayoutName = toolStripMenuItem.Text;
			WorkbenchSingleton.Workbench.WorkbenchLayout.StoreConfiguration();
		}
		clickingMenu = false;
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Internal.ExternalTool;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class ExternalToolPane : AbstractOptionPanel
{
	private static string[,] argumentQuickInsertMenu = new string[21, 2]
	{
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.FullItemPath}", "${ItemPath}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.FullItemDirectory}", "${ItemDir}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.ItemFileName}", "${ItemFileName}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.ItemExtension}", "${ItemExt}" },
		{ "-", "" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.CurrentLine}", "${CurLine}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.CurrentColumn}", "${CurCol}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.CurrentText}", "${CurText}" },
		{ "-", "" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.FullTargetPath}", "${TargetPath}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.TargetDirectory}", "${TargetDir}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.TargetName}", "${TargetName}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.TargetExtension}", "${TargetExt}" },
		{ "-", "" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.ProjectDirectory}", "${ProjectDir}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.ProjectFileName}", "${ProjectFileName}" },
		{ "-", "" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.CombineDirectory}", "${CombineDir}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.CombineFileName}", "${CombineFileName}" },
		{ "-", "" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.SharpDevelopStartupPath}", "${StartupPath}" }
	};

	private static string[,] workingDirInsertMenu = new string[10, 2]
	{
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.FullItemDirectory}", "${ItemDir}" },
		{ "-", "" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.TargetDirectory}", "${TargetDir}" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.TargetName}", "${TargetName}" },
		{ "-", "" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.ProjectDirectory}", "${ProjectDir}" },
		{ "-", "" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.CombineDirectory}", "${CombineDir}" },
		{ "-", "" },
		{ "${res:Dialog.Options.ExternalTool.QuickInsertMenu.SharpDevelopStartupPath}", "${StartupPath}" }
	};

	private static string[] dependendControlNames = new string[16]
	{
		"titleTextBox", "commandTextBox", "argumentTextBox", "workingDirTextBox", "promptArgsCheckBox", "useOutputPadCheckBox", "addSeparatorCheckBox", "titleLabel", "argumentLabel", "commandLabel",
		"workingDirLabel", "browseButton", "argumentQuickInsertButton", "workingDirQuickInsertButton", "moveUpButton", "moveDownButton"
	};

	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.ExternalToolOptions.xfrm"));
		((ListBox)ControlDictionary["toolListBox"]).BeginUpdate();
		try
		{
			foreach (ExternalTool item in ToolLoader.Tool)
			{
				((ListBox)ControlDictionary["toolListBox"]).Items.Add(item);
			}
		}
		finally
		{
			((ListBox)ControlDictionary["toolListBox"]).EndUpdate();
		}
		MenuService.CreateQuickInsertMenu((TextBox)ControlDictionary["argumentTextBox"], ControlDictionary["argumentQuickInsertButton"], argumentQuickInsertMenu);
		MenuService.CreateQuickInsertMenu((TextBox)ControlDictionary["workingDirTextBox"], ControlDictionary["workingDirQuickInsertButton"], workingDirInsertMenu);
		((ListBox)ControlDictionary["toolListBox"]).SelectedIndexChanged += selectEvent;
		ControlDictionary["removeButton"].Click += removeEvent;
		ControlDictionary["addButton"].Click += addEvent;
		ControlDictionary["moveUpButton"].Click += moveUpEvent;
		ControlDictionary["moveDownButton"].Click += moveDownEvent;
		ControlDictionary["browseButton"].Click += browseEvent;
		selectEvent(this, EventArgs.Empty);
	}

	private void browseEvent(object sender, EventArgs e)
	{
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.CheckFileExists = true;
		openFileDialog.Filter = StringParser.Parse("${res:SharpDevelop.FileFilter.ExecutableFiles}|*.exe;*.com;*.pif;*.bat;*.cmd|${res:SharpDevelop.FileFilter.AllFiles}|*.*");
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			ControlDictionary["commandTextBox"].Text = openFileDialog.FileName;
		}
	}

	private void moveUpEvent(object sender, EventArgs e)
	{
		int selectedIndex = ((ListBox)ControlDictionary["toolListBox"]).SelectedIndex;
		if (selectedIndex > 0)
		{
			((ListBox)ControlDictionary["toolListBox"]).SelectedIndexChanged -= selectEvent;
			try
			{
				object value = ((ListBox)ControlDictionary["toolListBox"]).Items[selectedIndex - 1];
				((ListBox)ControlDictionary["toolListBox"]).Items[selectedIndex - 1] = ((ListBox)ControlDictionary["toolListBox"]).Items[selectedIndex];
				((ListBox)ControlDictionary["toolListBox"]).Items[selectedIndex] = value;
				((ListBox)ControlDictionary["toolListBox"]).SetSelected(selectedIndex, value: false);
				((ListBox)ControlDictionary["toolListBox"]).SetSelected(selectedIndex - 1, value: true);
			}
			finally
			{
				((ListBox)ControlDictionary["toolListBox"]).SelectedIndexChanged += selectEvent;
			}
		}
	}

	private void moveDownEvent(object sender, EventArgs e)
	{
		int selectedIndex = ((ListBox)ControlDictionary["toolListBox"]).SelectedIndex;
		if (selectedIndex >= 0 && selectedIndex < ((ListBox)ControlDictionary["toolListBox"]).Items.Count - 1)
		{
			((ListBox)ControlDictionary["toolListBox"]).SelectedIndexChanged -= selectEvent;
			try
			{
				object value = ((ListBox)ControlDictionary["toolListBox"]).Items[selectedIndex + 1];
				((ListBox)ControlDictionary["toolListBox"]).Items[selectedIndex + 1] = ((ListBox)ControlDictionary["toolListBox"]).Items[selectedIndex];
				((ListBox)ControlDictionary["toolListBox"]).Items[selectedIndex] = value;
				((ListBox)ControlDictionary["toolListBox"]).SetSelected(selectedIndex, value: false);
				((ListBox)ControlDictionary["toolListBox"]).SetSelected(selectedIndex + 1, value: true);
			}
			finally
			{
				((ListBox)ControlDictionary["toolListBox"]).SelectedIndexChanged += selectEvent;
			}
		}
	}

	public override bool StorePanelContents()
	{
		List<ExternalTool> list = new List<ExternalTool>();
		foreach (ExternalTool item in ((ListBox)ControlDictionary["toolListBox"]).Items)
		{
			if (!FileUtility.IsValidFileName(item.Command))
			{
				MessageService.ShowError($"The command of tool \"{item.MenuCommand}\" is invalid.");
				return false;
			}
			if (item.InitialDirectory != "" && !FileUtility.IsValidFileName(item.InitialDirectory))
			{
				MessageService.ShowError($"The working directory of tool \"{item.MenuCommand}\" is invalid.");
				return false;
			}
			list.Add(item);
		}
		ToolLoader.Tool = list;
		ToolLoader.SaveTools();
		return true;
	}

	private void propertyValueChanged(object sender, PropertyValueChangedEventArgs e)
	{
		foreach (ListViewItem item in ((ListView)ControlDictionary["toolListView"]).Items)
		{
			if (item.Tag != null)
			{
				item.Text = item.Tag.ToString();
			}
		}
	}

	private void setToolValues(object sender, EventArgs e)
	{
		ExternalTool externalTool = ((ListBox)ControlDictionary["toolListBox"]).SelectedItem as ExternalTool;
		externalTool.MenuCommand = ControlDictionary["titleTextBox"].Text;
		externalTool.Command = ControlDictionary["commandTextBox"].Text;
		externalTool.Arguments = ControlDictionary["argumentTextBox"].Text;
		externalTool.InitialDirectory = ControlDictionary["workingDirTextBox"].Text;
		externalTool.PromptForArguments = ((CheckBox)ControlDictionary["promptArgsCheckBox"]).Checked;
		externalTool.UseOutputPad = ((CheckBox)ControlDictionary["useOutputPadCheckBox"]).Checked;
		externalTool.AddTopSeparator = ((CheckBox)ControlDictionary["addSeparatorCheckBox"]).Checked;
	}

	private void selectEvent(object sender, EventArgs e)
	{
		SetEnabledStatus(((ListBox)ControlDictionary["toolListBox"]).SelectedItems.Count > 0, "removeButton");
		ControlDictionary["titleTextBox"].TextChanged -= setToolValues;
		ControlDictionary["commandTextBox"].TextChanged -= setToolValues;
		ControlDictionary["argumentTextBox"].TextChanged -= setToolValues;
		ControlDictionary["workingDirTextBox"].TextChanged -= setToolValues;
		((CheckBox)ControlDictionary["promptArgsCheckBox"]).CheckedChanged -= setToolValues;
		((CheckBox)ControlDictionary["useOutputPadCheckBox"]).CheckedChanged -= setToolValues;
		((CheckBox)ControlDictionary["addSeparatorCheckBox"]).CheckedChanged -= setToolValues;
		if (((ListBox)ControlDictionary["toolListBox"]).SelectedItems.Count == 1)
		{
			ExternalTool externalTool = ((ListBox)ControlDictionary["toolListBox"]).SelectedItem as ExternalTool;
			SetEnabledStatus(enabled: true, dependendControlNames);
			ControlDictionary["titleTextBox"].Text = externalTool.MenuCommand;
			ControlDictionary["commandTextBox"].Text = externalTool.Command;
			ControlDictionary["argumentTextBox"].Text = externalTool.Arguments;
			ControlDictionary["workingDirTextBox"].Text = externalTool.InitialDirectory;
			((CheckBox)ControlDictionary["promptArgsCheckBox"]).Checked = externalTool.PromptForArguments;
			((CheckBox)ControlDictionary["useOutputPadCheckBox"]).Checked = externalTool.UseOutputPad;
			((CheckBox)ControlDictionary["addSeparatorCheckBox"]).Checked = externalTool.AddTopSeparator;
		}
		else
		{
			SetEnabledStatus(enabled: false, dependendControlNames);
			ControlDictionary["titleTextBox"].Text = string.Empty;
			ControlDictionary["commandTextBox"].Text = string.Empty;
			ControlDictionary["argumentTextBox"].Text = string.Empty;
			ControlDictionary["workingDirTextBox"].Text = string.Empty;
			((CheckBox)ControlDictionary["promptArgsCheckBox"]).Checked = false;
			((CheckBox)ControlDictionary["useOutputPadCheckBox"]).Checked = false;
			((CheckBox)ControlDictionary["addSeparatorCheckBox"]).Checked = false;
		}
		ControlDictionary["titleTextBox"].TextChanged += setToolValues;
		ControlDictionary["commandTextBox"].TextChanged += setToolValues;
		ControlDictionary["argumentTextBox"].TextChanged += setToolValues;
		ControlDictionary["workingDirTextBox"].TextChanged += setToolValues;
		((CheckBox)ControlDictionary["promptArgsCheckBox"]).CheckedChanged += setToolValues;
		((CheckBox)ControlDictionary["useOutputPadCheckBox"]).CheckedChanged += setToolValues;
		((CheckBox)ControlDictionary["addSeparatorCheckBox"]).CheckedChanged += setToolValues;
	}

	private void removeEvent(object sender, EventArgs e)
	{
		((ListBox)ControlDictionary["toolListBox"]).BeginUpdate();
		try
		{
			int selectedIndex = ((ListBox)ControlDictionary["toolListBox"]).SelectedIndex;
			object[] array = new object[((ListBox)ControlDictionary["toolListBox"]).SelectedItems.Count];
			((ListBox)ControlDictionary["toolListBox"]).SelectedItems.CopyTo(array, 0);
			((ListBox)ControlDictionary["toolListBox"]).SelectedIndexChanged -= selectEvent;
			object[] array2 = array;
			foreach (object value in array2)
			{
				((ListBox)ControlDictionary["toolListBox"]).Items.Remove(value);
			}
			((ListBox)ControlDictionary["toolListBox"]).SelectedIndexChanged += selectEvent;
			if (((ListBox)ControlDictionary["toolListBox"]).Items.Count == 0)
			{
				selectEvent(this, EventArgs.Empty);
			}
			else
			{
				((ListBox)ControlDictionary["toolListBox"]).SelectedIndex = Math.Min(selectedIndex, ((ListBox)ControlDictionary["toolListBox"]).Items.Count - 1);
			}
		}
		finally
		{
			((ListBox)ControlDictionary["toolListBox"]).EndUpdate();
		}
	}

	private void addEvent(object sender, EventArgs e)
	{
		((ListBox)ControlDictionary["toolListBox"]).BeginUpdate();
		try
		{
			((ListBox)ControlDictionary["toolListBox"]).Items.Add(new ExternalTool());
			((ListBox)ControlDictionary["toolListBox"]).SelectedIndexChanged -= selectEvent;
			((ListBox)ControlDictionary["toolListBox"]).ClearSelected();
			((ListBox)ControlDictionary["toolListBox"]).SelectedIndexChanged += selectEvent;
			((ListBox)ControlDictionary["toolListBox"]).SelectedIndex = ((ListBox)ControlDictionary["toolListBox"]).Items.Count - 1;
		}
		finally
		{
			((ListBox)ControlDictionary["toolListBox"]).EndUpdate();
		}
	}
}

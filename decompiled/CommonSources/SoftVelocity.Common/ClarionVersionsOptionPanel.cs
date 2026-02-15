using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Ide.Core;

namespace SoftVelocity.Common;

public abstract class ClarionVersionsOptionPanel : AbstractOptionPanel
{
	private List<VersionInformation> m_versions;

	protected VersionInformation curVersion;

	private int selectedVer;

	private bool inLoad;

	protected abstract List<VersionInformation> VersionList { get; }

	protected abstract string BinFilterPath { get; }

	protected abstract string LibsrcFilterPath { get; }

	protected abstract VersionInformation NewVersion(string path);

	protected virtual void UpdateCurrentVersion()
	{
		if (curVersion == null)
		{
			return;
		}
		curVersion.Name = ((XmlUserControl)this).ControlDictionary["nameTextBox"].Text;
		curVersion.Directory = ((XmlUserControl)this).ControlDictionary["binTextBox"].Text;
		curVersion.RedirectionFileName = ((XmlUserControl)this).ControlDictionary["redNameTextBox"].Text;
		curVersion.UseInclude = ((CheckBox)((XmlUserControl)this).ControlDictionary["includeCheckBox"]).Checked;
		ListView listView = (ListView)((XmlUserControl)this).ControlDictionary["libSrcListView"];
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ListViewItem item in listView.Items)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Append(';');
			}
			stringBuilder.Append(item.Text);
		}
		curVersion.Libsrc = stringBuilder.ToString();
	}

	protected virtual void LoadPanelContents(VersionInformation version)
	{
		inLoad = true;
		curVersion = version;
		((XmlUserControl)this).ControlDictionary["nameTextBox"].Text = version.Name;
		((XmlUserControl)this).ControlDictionary["binTextBox"].Text = version.Directory;
		((XmlUserControl)this).ControlDictionary["redNameTextBox"].Text = version.RedirectionFileName;
		((CheckBox)((XmlUserControl)this).ControlDictionary["includeCheckBox"]).Checked = version.UseInclude;
		BuildMacroList();
		BuildLibSrcList();
		SetEnabledStatus();
		SetEnabledStatusLibSrc();
		inLoad = false;
	}

	protected virtual void FinishLoadPanelContents()
	{
		LoadPanelContents(m_versions[0]);
	}

	public ClarionVersionsOptionPanel()
	{
	}

	public override void LoadPanelContents()
	{
		m_versions = VersionList;
		((XmlUserControl)this).SetupFromXmlStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CommonSources.Resources.ClarionVersionsOptionsPanel.xfrm"));
		Button button = (Button)((XmlUserControl)this).ControlDictionary["binButton"];
		button.Tag = new ButtonTag(string.Join("|", (string[])AddInTree.GetTreeNode(BinFilterPath).BuildChildItems((object)null).ToArray(typeof(string))) + "|${res:SharpDevelop.FileFilter.AllFiles}|*.*", StringParser.Parse("${res:ClarionVersionsOptionsPanel.FindBin.Title}"), ((XmlUserControl)this).ControlDictionary["binTextBox"]);
		button.Click += EllipsisSelected;
		((XmlUserControl)this).ControlDictionary["removeButton"].Click += RemoveEvent;
		((XmlUserControl)this).ControlDictionary["addButton"].Click += AddEvent;
		((XmlUserControl)this).ControlDictionary["editButton"].Click += EditEvent;
		((XmlUserControl)this).ControlDictionary["removeLibSrcButton"].Click += RemoveLibSrcEvent;
		((XmlUserControl)this).ControlDictionary["addLibSrcButton"].Click += AddLibSrcEvent;
		((XmlUserControl)this).ControlDictionary["editLibSrcButton"].Click += EditLibSrcEvent;
		if (ProjectService.OpenSolution != null)
		{
			((XmlUserControl)this).ControlDictionary["deleteVersionButton"].Enabled = false;
		}
		else
		{
			((XmlUserControl)this).ControlDictionary["deleteVersionButton"].Enabled = true;
			((XmlUserControl)this).ControlDictionary["deleteVersionButton"].Click += DeleteVersionEvent;
		}
		ListView listView = (ListView)((XmlUserControl)this).ControlDictionary["macrosListView"];
		listView.ItemActivate += EditEvent;
		listView.SelectedIndexChanged += MacroIndexChange;
		ListView listView2 = (ListView)((XmlUserControl)this).ControlDictionary["libSrcListView"];
		listView2.ItemActivate += EditLibSrcEvent;
		listView2.SelectedIndexChanged += LibSrcIndexChange;
		((XmlUserControl)this).ControlDictionary["nameTextBox"].TextChanged += VersionNameChanged;
		LoadVersions();
		ToolTip toolTip = new ToolTip();
		toolTip.SetToolTip(((XmlUserControl)this).ControlDictionary["includeCheckBox"], StringParser.Parse("${res:ClarionVersionsOptionsPanel.RedirectionTab.Include.ToolTip}"));
		toolTip = new ToolTip();
		toolTip.SetToolTip(((XmlUserControl)this).ControlDictionary["macrosListView"], StringParser.Parse("${res:ClarionVersionsOptionsPanel.RedirectionTab.Macros.ToolTip}"));
		toolTip = new ToolTip();
		toolTip.SetToolTip(((XmlUserControl)this).ControlDictionary["binTextBox"], StringParser.Parse("${res:ClarionVersionsOptionsPanel.GeneralTab.Bin.ToolTip}"));
		FinishLoadPanelContents();
		ComboBox comboBox = (ComboBox)((XmlUserControl)this).ControlDictionary["versionsComboBox"];
		comboBox.SelectedIndex = 0;
		comboBox.Select();
		comboBox.Focus();
	}

	private void LoadVersions()
	{
		ComboBox comboBox = (ComboBox)((XmlUserControl)this).ControlDictionary["versionsComboBox"];
		comboBox.BeginUpdate();
		comboBox.Items.Clear();
		foreach (VersionInformation version in m_versions)
		{
			comboBox.Items.Add(version);
		}
		comboBox.Items.Add(StringParser.Parse("${res:ClarionVersionsOptionsPanel.NewVersion}"));
		comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
		comboBox.EndUpdate();
		comboBox.SelectedIndexChanged += NewVersion;
	}

	private void BuildMacroList()
	{
		ListView.ListViewItemCollection items = ((ListView)((XmlUserControl)this).ControlDictionary["macrosListView"]).Items;
		items.Clear();
		foreach (KeyValuePair<string, string> macro in curVersion.Macros)
		{
			ListViewItem listViewItem = new ListViewItem(new string[2] { macro.Key, macro.Value });
			listViewItem.Tag = macro;
			items.Add(listViewItem);
		}
	}

	private void BuildLibSrcList()
	{
		ListView.ListViewItemCollection items = ((ListView)((XmlUserControl)this).ControlDictionary["libSrcListView"]).Items;
		items.Clear();
		string[] array = curVersion.Libsrc.Split(';');
		string[] array2 = array;
		foreach (string text in array2)
		{
			ListViewItem value = new ListViewItem(new string[1] { text });
			items.Add(value);
		}
	}

	private void SetEnabledStatusForList(string listName, string buttonText)
	{
		switch (((ListView)((XmlUserControl)this).ControlDictionary[listName]).SelectedItems.Count)
		{
		case 0:
			((BaseSharpDevelopUserControl)this).SetEnabledStatus(false, new string[2]
			{
				"edit" + buttonText + "Button",
				"remove" + buttonText + "Button"
			});
			break;
		case 1:
			((BaseSharpDevelopUserControl)this).SetEnabledStatus(true, new string[2]
			{
				"edit" + buttonText + "Button",
				"remove" + buttonText + "Button"
			});
			break;
		default:
			((BaseSharpDevelopUserControl)this).SetEnabledStatus(true, new string[1] { "remove" + buttonText + "Button" });
			((BaseSharpDevelopUserControl)this).SetEnabledStatus(false, new string[1] { "edit" + buttonText + "Button" });
			break;
		}
	}

	private void SetEnabledStatus()
	{
		SetEnabledStatusForList("macrosListView", "");
	}

	private void SetEnabledStatusLibSrc()
	{
		SetEnabledStatusForList("libSrcListView", "LibSrc");
	}

	protected void VersionNameChanged(object sender, EventArgs args)
	{
		if (curVersion != null && !inLoad)
		{
			ComboBox comboBox = (ComboBox)((XmlUserControl)this).ControlDictionary["versionsComboBox"];
			int selectedIndex = comboBox.SelectedIndex;
			Control control = (Control)sender;
			curVersion.Name = control.Text;
			LoadVersions();
			comboBox.SelectedIndex = selectedIndex;
		}
	}

	protected void EllipsisSelected(object sender, EventArgs args)
	{
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		ButtonTag buttonTag = (ButtonTag)((Control)sender).Tag;
		openFileDialog.Title = buttonTag.title;
		openFileDialog.Filter = StringParser.Parse(buttonTag.filter);
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			buttonTag.entryControl.Text = Path.Combine(Path.GetPathRoot(openFileDialog.FileName), Path.GetDirectoryName(openFileDialog.FileName));
		}
	}

	private void AddVersion(ComboBox combo)
	{
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.Title = StringParser.Parse("${res:ClarionVersionsOptionsPanel.FindBin.Title}");
		string text = string.Join("|", (string[])AddInTree.GetTreeNode(BinFilterPath).BuildChildItems((object)null).ToArray(typeof(string))) + "|${res:SharpDevelop.FileFilter.AllFiles}|*.*";
		openFileDialog.Filter = StringParser.Parse(text);
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			VersionInformation versionInformation = null;
			try
			{
				versionInformation = NewVersion(openFileDialog.FileName);
			}
			catch (InvalidOperationException)
			{
				return;
			}
			m_versions.Add(versionInformation);
			LoadVersions();
			combo.SelectedItem = versionInformation;
		}
		else
		{
			combo.SelectedIndex = selectedVer;
		}
	}

	protected void NewVersion(object sender, EventArgs args)
	{
		ComboBox comboBox = (ComboBox)sender;
		int selectedIndex = comboBox.SelectedIndex;
		if (selectedIndex == comboBox.Items.Count - 1)
		{
			AddVersion(comboBox);
		}
		else if (selectedIndex != -1)
		{
			selectedVer = selectedIndex;
			UpdateCurrentVersion();
			LoadPanelContents(m_versions[selectedIndex]);
		}
	}

	private void MacroIndexChange(object sender, EventArgs e)
	{
		SetEnabledStatus();
	}

	private void LibSrcIndexChange(object sender, EventArgs e)
	{
		SetEnabledStatusLibSrc();
	}

	private void DeleteVersionEvent(object sender, EventArgs e)
	{
		if (MessageBox.Show(StringParser.Parse("${res:ClarionVersionsOptionsPanel.RemoveVersion.Text}"), StringParser.Parse("${res:ClarionVersionsOptionsPanel.RemoveVersion.Title}"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			ComboBox comboBox = (ComboBox)((XmlUserControl)this).ControlDictionary["versionsComboBox"];
			int num = comboBox.SelectedIndex;
			if (num > 0)
			{
				num--;
			}
			m_versions.Remove(curVersion);
			curVersion.Remove();
			LoadVersions();
			comboBox.SelectedIndex = num;
		}
	}

	private void RemoveEvent(object sender, EventArgs e)
	{
		ListView listView = (ListView)((XmlUserControl)this).ControlDictionary["macrosListView"];
		object[] array = new object[listView.SelectedItems.Count];
		listView.SelectedItems.CopyTo(array, 0);
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ListViewItem listViewItem = (ListViewItem)array2[i];
			curVersion.Macros.Remove(((KeyValuePair<string, string>)listViewItem.Tag).Key);
			listView.Items.Remove(listViewItem);
		}
	}

	private void AddEvent(object sender, EventArgs e)
	{
		EditMacroDialog editMacroDialog = new EditMacroDialog("", "");
		try
		{
			if (((Form)(object)editMacroDialog).ShowDialog((IWin32Window)WorkbenchSingleton.MainForm) == DialogResult.OK)
			{
				curVersion.Macros.Add(editMacroDialog.Macro, editMacroDialog.Value);
				ListView listView = (ListView)((XmlUserControl)this).ControlDictionary["macrosListView"];
				listView.SelectedItems.Clear();
				BuildMacroList();
				listView.Select();
			}
		}
		finally
		{
			((IDisposable)editMacroDialog)?.Dispose();
		}
	}

	private void EditEvent(object sender, EventArgs e)
	{
		ListView listView = (ListView)((XmlUserControl)this).ControlDictionary["macrosListView"];
		ListViewItem listViewItem = listView.SelectedItems[0];
		KeyValuePair<string, string> keyValuePair = (KeyValuePair<string, string>)listViewItem.Tag;
		EditMacroDialog editMacroDialog = new EditMacroDialog(keyValuePair.Key, keyValuePair.Value);
		try
		{
			if (((Form)(object)editMacroDialog).ShowDialog((IWin32Window)WorkbenchSingleton.MainForm) == DialogResult.OK)
			{
				Dictionary<string, string> macros = curVersion.Macros;
				if (editMacroDialog.Macro != keyValuePair.Key)
				{
					macros.Remove(keyValuePair.Key);
					macros.Add(editMacroDialog.Macro, editMacroDialog.Value);
				}
				else
				{
					macros[editMacroDialog.Macro] = editMacroDialog.Value;
				}
				BuildMacroList();
			}
		}
		finally
		{
			((IDisposable)editMacroDialog)?.Dispose();
		}
	}

	private void RemoveLibSrcEvent(object sender, EventArgs e)
	{
		ListView listView = (ListView)((XmlUserControl)this).ControlDictionary["libSrcListView"];
		object[] array = new object[listView.SelectedItems.Count];
		listView.SelectedItems.CopyTo(array, 0);
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ListViewItem item = (ListViewItem)array2[i];
			listView.Items.Remove(item);
		}
	}

	private void AddLibSrcEvent(object sender, EventArgs e)
	{
		ListView listView = (ListView)((XmlUserControl)this).ControlDictionary["libSrcListView"];
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.Title = StringParser.Parse("${res:ClarionVersionsOptionsPanel.FindLibsrc.Title}");
		string text = string.Join("|", (string[])AddInTree.GetTreeNode(LibsrcFilterPath).BuildChildItems((object)null).ToArray(typeof(string))) + "|${res:SharpDevelop.FileFilter.AllFiles}|*.*";
		openFileDialog.Filter = StringParser.Parse(text);
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			ListViewItem value = new ListViewItem(new string[1] { Path.Combine(Path.GetPathRoot(openFileDialog.FileName), Path.GetDirectoryName(openFileDialog.FileName)) });
			listView.Items.Add(value);
		}
	}

	private void EditLibSrcEvent(object sender, EventArgs e)
	{
		ListView listView = (ListView)((XmlUserControl)this).ControlDictionary["libSrcListView"];
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.Title = StringParser.Parse("${res:ClarionVersionsOptionsPanel.FindLibsrc.Title}");
		string text = string.Join("|", (string[])AddInTree.GetTreeNode(LibsrcFilterPath).BuildChildItems((object)null).ToArray(typeof(string))) + "|${res:SharpDevelop.FileFilter.AllFiles}|*.*";
		openFileDialog.Filter = StringParser.Parse(text);
		openFileDialog.InitialDirectory = listView.SelectedItems[0].Text;
		openFileDialog.ForceInitialDirectory = true;
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			listView.SelectedItems[0].Text = Path.Combine(Path.GetPathRoot(openFileDialog.FileName), Path.GetDirectoryName(openFileDialog.FileName));
		}
	}

	public override bool StorePanelContents()
	{
		UpdateCurrentVersion();
		foreach (VersionInformation version in m_versions)
		{
			if (!version.Store())
			{
				return false;
			}
		}
		return true;
	}
}

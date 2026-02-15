using System;
using System.Collections;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class CodeTemplatePanel : AbstractOptionPanel
{
	private ArrayList templateGroups;

	private int currentSelectedGroup = -1;

	public CodeTemplateGroup CurrentTemplateGroup
	{
		get
		{
			if (currentSelectedGroup < 0 || currentSelectedGroup >= templateGroups.Count)
			{
				return null;
			}
			return (CodeTemplateGroup)templateGroups[currentSelectedGroup];
		}
	}

	public override void LoadPanelContents()
	{
		templateGroups = CopyCodeTemplateGroups(CodeTemplateLoader.TemplateGroups);
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.CodeTemplatePanel.xfrm"));
		ControlDictionary["removeButton"].Click += RemoveEvent;
		ControlDictionary["addButton"].Click += AddEvent;
		ControlDictionary["editButton"].Click += EditEvent;
		ControlDictionary["addGroupButton"].Click += AddGroupEvent;
		ControlDictionary["removeGroupButton"].Click += RemoveGroupEvent;
		((TextBox)ControlDictionary["templateTextBox"]).Font = ResourceService.DefaultMonospacedFont;
		((TextBox)ControlDictionary["templateTextBox"]).TextChanged += TextChange;
		((ListView)ControlDictionary["templateListView"]).Activation = ItemActivation.Standard;
		((ListView)ControlDictionary["templateListView"]).ItemActivate += EditEvent;
		((ListView)ControlDictionary["templateListView"]).SelectedIndexChanged += IndexChange;
		((ComboBox)ControlDictionary["groupComboBox"]).DropDown += FillGroupBoxEvent;
		if (templateGroups.Count > 0)
		{
			currentSelectedGroup = 0;
		}
		FillGroupComboBox();
		BuildListView();
		IndexChange(null, null);
		SetEnabledStatus();
	}

	public override bool StorePanelContents()
	{
		CodeTemplateLoader.TemplateGroups = templateGroups;
		CodeTemplateLoader.SaveTemplates();
		return true;
	}

	private void FillGroupBoxEvent(object sender, EventArgs e)
	{
		FillGroupComboBox();
	}

	private void SetEnabledStatus()
	{
		bool flag = CurrentTemplateGroup != null;
		bool enabled = templateGroups.Count != 0;
		SetEnabledStatus(flag, "addButton", "editButton", "removeButton", "templateListView", "templateTextBox");
		SetEnabledStatus(enabled, "groupComboBox", "extensionLabel");
		if (flag)
		{
			bool enabled2 = ((ListView)ControlDictionary["templateListView"]).SelectedItems.Count == 1;
			bool enabled3 = ((ListView)ControlDictionary["templateListView"]).SelectedItems.Count > 0;
			SetEnabledStatus(enabled2, "editButton", "templateTextBox");
			SetEnabledStatus(enabled3, "removeButton");
		}
	}

	private void SetGroupSelection(object sender, EventArgs e)
	{
		currentSelectedGroup = ((ComboBox)ControlDictionary["groupComboBox"]).SelectedIndex;
		BuildListView();
	}

	private void GroupComboBoxTextChanged(object sender, EventArgs e)
	{
		if (((ComboBox)ControlDictionary["groupComboBox"]).SelectedIndex >= 0)
		{
			currentSelectedGroup = ((ComboBox)ControlDictionary["groupComboBox"]).SelectedIndex;
		}
		if (CurrentTemplateGroup != null)
		{
			CurrentTemplateGroup.ExtensionStrings = ((ComboBox)ControlDictionary["groupComboBox"]).Text.Split(';');
		}
	}

	private void AddGroupEvent(object sender, EventArgs e)
	{
		templateGroups.Add(new CodeTemplateGroup(".???"));
		FillGroupComboBox();
		((ComboBox)ControlDictionary["groupComboBox"]).SelectedIndex = templateGroups.Count - 1;
		SetEnabledStatus();
	}

	private void RemoveGroupEvent(object sender, EventArgs e)
	{
		if (CurrentTemplateGroup != null)
		{
			templateGroups.RemoveAt(currentSelectedGroup);
			if (templateGroups.Count == 0)
			{
				currentSelectedGroup = -1;
			}
			else
			{
				((ComboBox)ControlDictionary["groupComboBox"]).SelectedIndex = Math.Min(currentSelectedGroup, templateGroups.Count - 1);
			}
			FillGroupComboBox();
			BuildListView();
			SetEnabledStatus();
		}
	}

	private void RemoveEvent(object sender, EventArgs e)
	{
		object[] array = new object[((ListView)ControlDictionary["templateListView"]).SelectedItems.Count];
		((ListView)ControlDictionary["templateListView"]).SelectedItems.CopyTo(array, 0);
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ListViewItem item = (ListViewItem)array2[i];
			((ListView)ControlDictionary["templateListView"]).Items.Remove(item);
		}
		StoreTemplateGroup();
	}

	private void AddEvent(object sender, EventArgs e)
	{
		CodeTemplate codeTemplate = new CodeTemplate();
		using EditTemplateDialog editTemplateDialog = new EditTemplateDialog(codeTemplate);
		if (editTemplateDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			CurrentTemplateGroup.Templates.Add(codeTemplate);
			((ListView)ControlDictionary["templateListView"]).SelectedItems.Clear();
			BuildListView();
			((ListView)ControlDictionary["templateListView"]).Select();
		}
	}

	private void EditEvent(object sender, EventArgs e)
	{
		int currentIndex = GetCurrentIndex();
		if (currentIndex == -1)
		{
			return;
		}
		ListViewItem listViewItem = ((ListView)ControlDictionary["templateListView"]).SelectedItems[0];
		CodeTemplate codeTemplate = (CodeTemplate)listViewItem.Tag;
		codeTemplate = new CodeTemplate(codeTemplate.Shortcut, codeTemplate.Description, codeTemplate.Text);
		using (EditTemplateDialog editTemplateDialog = new EditTemplateDialog(codeTemplate))
		{
			if (editTemplateDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
			{
				listViewItem.Tag = codeTemplate;
				StoreTemplateGroup();
			}
		}
		BuildListView();
	}

	private void FillGroupComboBox()
	{
		((ComboBox)ControlDictionary["groupComboBox"]).TextChanged -= GroupComboBoxTextChanged;
		((ComboBox)ControlDictionary["groupComboBox"]).SelectedIndexChanged -= SetGroupSelection;
		((ComboBox)ControlDictionary["groupComboBox"]).Items.Clear();
		foreach (CodeTemplateGroup templateGroup in templateGroups)
		{
			((ComboBox)ControlDictionary["groupComboBox"]).Items.Add(string.Join(";", templateGroup.ExtensionStrings));
		}
		((ComboBox)ControlDictionary["groupComboBox"]).Text = ((CurrentTemplateGroup != null) ? ((ComboBox)ControlDictionary["groupComboBox"]).Items[currentSelectedGroup].ToString() : string.Empty);
		if (currentSelectedGroup >= 0)
		{
			((ComboBox)ControlDictionary["groupComboBox"]).SelectedIndex = currentSelectedGroup;
		}
		((ComboBox)ControlDictionary["groupComboBox"]).SelectedIndexChanged += SetGroupSelection;
		((ComboBox)ControlDictionary["groupComboBox"]).TextChanged += GroupComboBoxTextChanged;
	}

	private int GetCurrentIndex()
	{
		if (((ListView)ControlDictionary["templateListView"]).SelectedItems.Count == 1)
		{
			return ((ListView)ControlDictionary["templateListView"]).SelectedItems[0].Index;
		}
		return -1;
	}

	private void IndexChange(object sender, EventArgs e)
	{
		int currentIndex = GetCurrentIndex();
		if (currentIndex != -1)
		{
			ControlDictionary["templateTextBox"].Text = ((CodeTemplate)((ListView)ControlDictionary["templateListView"]).SelectedItems[0].Tag).Text;
		}
		else
		{
			ControlDictionary["templateTextBox"].Text = string.Empty;
		}
		SetEnabledStatus();
	}

	private void TextChange(object sender, EventArgs e)
	{
		int currentIndex = GetCurrentIndex();
		if (currentIndex != -1)
		{
			((CodeTemplate)((ListView)ControlDictionary["templateListView"]).SelectedItems[0].Tag).Text = ControlDictionary["templateTextBox"].Text;
		}
	}

	private void StoreTemplateGroup()
	{
		if (CurrentTemplateGroup == null)
		{
			return;
		}
		CurrentTemplateGroup.Templates.Clear();
		foreach (ListViewItem item in ((ListView)ControlDictionary["templateListView"]).Items)
		{
			CurrentTemplateGroup.Templates.Add((CodeTemplate)item.Tag);
		}
	}

	private void BuildListView()
	{
		((ListView)ControlDictionary["templateListView"]).Items.Clear();
		if (CurrentTemplateGroup != null)
		{
			foreach (CodeTemplate template in CurrentTemplateGroup.Templates)
			{
				ListViewItem listViewItem = new ListViewItem(new string[2] { template.Shortcut, template.Description });
				listViewItem.Tag = template;
				((ListView)ControlDictionary["templateListView"]).Items.Add(listViewItem);
			}
		}
		IndexChange(this, EventArgs.Empty);
	}

	private ArrayList CopyCodeTemplateGroups(ArrayList groups)
	{
		ArrayList arrayList = new ArrayList();
		foreach (CodeTemplateGroup group in groups)
		{
			CodeTemplateGroup codeTemplateGroup2 = new CodeTemplateGroup(string.Join(";", group.ExtensionStrings));
			foreach (CodeTemplate template in group.Templates)
			{
				CodeTemplate item = new CodeTemplate(template.Shortcut, template.Description, template.Text);
				codeTemplateGroup2.Templates.Add(item);
			}
			arrayList.Add(codeTemplateGroup2);
		}
		return arrayList;
	}
}

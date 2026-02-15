using System;
using System.Windows.Forms;
using DockPanelSkin;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class SelectStylePanel : AbstractOptionPanel
{
	private CheckBox showExtensionsCheckBox = new CheckBox();

	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.SelectStylePanel.xfrm"));
		Get<CheckBox>("showExtensions").Checked = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.ProjectBrowser.ShowExtensions", defaultValue: true);
		AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/Workbench/Ambiences");
		foreach (Codon codon in treeNode.Codons)
		{
			((ComboBox)ControlDictionary["selectAmbienceComboBox"]).Items.Add(codon.Id);
		}
		ControlDictionary["selectAmbienceComboBox"].Text = PropertyService.Get("SharpDevelop.UI.CurrentAmbience", "Clarion");
		Get<CheckBox>("preferProjectAmbience").Checked = AmbienceService.UseProjectAmbienceIfPossible;
		Get<CheckBox>("showStatusBar").Checked = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.StatusBarVisible", defaultValue: true);
		Get<CheckBox>("showToolBar").Checked = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.ToolBarVisible", defaultValue: true);
		Get<CheckBox>("useProfessionalStyle").Checked = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.UseProfessionalRenderer", defaultValue: true);
		Get<CheckBox>("useProfessionalStyle").CheckedChanged += delegate
		{
			RefreshStatus();
		};
		Get<CheckBox>("showFullPathOnTitle").Checked = PropertyService.Get("Workbench.ShowFullPathOnTitle", defaultValue: true);
		ColorThemesListService.Refresh();
		string[] items = ColorThemesListService.Items;
		foreach (string item in items)
		{
			Get<ComboBox>("professionalStyleOptions").Items.Add(item);
		}
		Get<ComboBox>("professionalStyleOptions").Text = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.ProfessionalRendererColorTableStyles", "Win10Blue");
		string[] names = Enum.GetNames(typeof(Extender.Style));
		string[] array = names;
		foreach (string item2 in array)
		{
			Get<ComboBox>("dockPanelStyleOptions").Items.Add(item2);
		}
		Get<ComboBox>("dockPanelStyleOptions").Text = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.DockPanelStyle", Extender.Style.VS2013.ToString());
		if (ColorThemesListService.ThemeExist(PropertyService.Get("ICSharpCode.SharpDevelop.Gui.ProfessionalRendererColorTableStyles", "Win10Blue")))
		{
			Get<ComboBox>("professionalStyleOptions").Text = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.ProfessionalRendererColorTableStyles", "Win10Blue");
		}
		else
		{
			Get<ComboBox>("professionalStyleOptions").Text = "Default";
		}
		Get<Button>("professionalStyleEditor").Click += OnOpenEditor_Click;
		Get<CheckBox>("useSmallIconsInToolbar").Checked = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.UseSmallIconsInToolbar", defaultValue: false);
		Get<Button>("toolbarSize").Click += OnOpenToolbarEditor_Click;
		RefreshStatus();
	}

	private void OnOpenToolbarEditor_Click(object sender, EventArgs e)
	{
		ToolbarSizeEditor.Edit();
	}

	private void OnOpenEditor_Click(object sender, EventArgs e)
	{
		string s = Get<ComboBox>("professionalStyleOptions").SelectedItem.ToString();
		if (ColorThemesListService.EditTheme(Get<ComboBox>("professionalStyleOptions").Text) == DialogResult.OK)
		{
			Get<ComboBox>("professionalStyleOptions").Items.Clear();
			string[] items = ColorThemesListService.Items;
			foreach (string item in items)
			{
				Get<ComboBox>("professionalStyleOptions").Items.Add(item);
			}
			Get<ComboBox>("professionalStyleOptions").SelectedIndex = Get<ComboBox>("professionalStyleOptions").FindString(s);
		}
	}

	private void RefreshStatus()
	{
		Get<ComboBox>("professionalStyleOptions").Enabled = Get<CheckBox>("useProfessionalStyle").Checked;
		Get<Label>("professionalStyleOptions").Enabled = Get<CheckBox>("useProfessionalStyle").Checked;
	}

	public override bool StorePanelContents()
	{
		PropertyService.Set("ICSharpCode.SharpDevelop.Gui.ProjectBrowser.ShowExtensions", ((CheckBox)ControlDictionary["showExtensionsCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.UI.CurrentAmbience", ((ComboBox)ControlDictionary["selectAmbienceComboBox"]).Text);
		PropertyService.Set("ICSharpCode.SharpDevelop.Gui.StatusBarVisible", ((CheckBox)ControlDictionary["showStatusBarCheckBox"]).Checked);
		PropertyService.Set("ICSharpCode.SharpDevelop.Gui.ToolBarVisible", ((CheckBox)ControlDictionary["showToolBarCheckBox"]).Checked);
		PropertyService.Set("ICSharpCode.SharpDevelop.Gui.UseProfessionalRenderer", Get<CheckBox>("useProfessionalStyle").Checked);
		PropertyService.Set("ICSharpCode.SharpDevelop.Gui.ProfessionalRendererColorTableStyles", Get<ComboBox>("professionalStyleOptions").Text);
		PropertyService.Set("ICSharpCode.SharpDevelop.Gui.DockPanelStyle", Get<ComboBox>("dockPanelStyleOptions").Text);
		PropertyService.Set("ICSharpCode.SharpDevelop.Gui.UseSmallIconsInToolbar", Get<CheckBox>("useSmallIconsInToolbar").Checked);
		PropertyService.Set("Workbench.ShowFullPathOnTitle", Get<CheckBox>("showFullPathOnTitle").Checked);
		AmbienceService.UseProjectAmbienceIfPossible = Get<CheckBox>("preferProjectAmbience").Checked;
		return true;
	}
}

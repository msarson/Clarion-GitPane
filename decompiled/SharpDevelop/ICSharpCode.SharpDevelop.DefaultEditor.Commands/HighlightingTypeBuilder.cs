using System;
using System.Collections;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class HighlightingTypeBuilder : ISubmenuBuilder
{
	private TextEditorControl control;

	private ToolStripItem[] menuCommands;

	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		control = (TextEditorControl)owner;
		ArrayList arrayList = new ArrayList();
		foreach (DictionaryEntry highlightingDefinition in HighlightingManager.Manager.HighlightingDefinitions)
		{
			MenuCheckBox menuCheckBox = new MenuCheckBox(highlightingDefinition.Key.ToString());
			menuCheckBox.Click += ChangeSyntax;
			menuCheckBox.Checked = control.Document.HighlightingStrategy.Name == highlightingDefinition.Key.ToString();
			arrayList.Add(menuCheckBox);
		}
		menuCommands = (ToolStripItem[])arrayList.ToArray(typeof(ToolStripItem));
		return menuCommands;
	}

	private void ChangeSyntax(object sender, EventArgs e)
	{
		if (control == null)
		{
			return;
		}
		MenuCheckBox menuCheckBox = (MenuCheckBox)sender;
		ToolStripItem[] array = menuCommands;
		for (int i = 0; i < array.Length; i++)
		{
			MenuCheckBox menuCheckBox2 = (MenuCheckBox)array[i];
			menuCheckBox2.Checked = false;
		}
		menuCheckBox.Checked = true;
		try
		{
			IHighlightingStrategy highlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(menuCheckBox.Text);
			if (highlightingStrategy == null)
			{
				throw new Exception("Strategy can't be null");
			}
			control.Document.HighlightingStrategy = highlightingStrategy;
			if (control is SharpDevelopTextAreaControl)
			{
				((SharpDevelopTextAreaControl)control).InitializeAdvancedHighlighter();
			}
		}
		catch (HighlightingDefinitionInvalidException ex)
		{
			MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		control.Refresh();
	}
}

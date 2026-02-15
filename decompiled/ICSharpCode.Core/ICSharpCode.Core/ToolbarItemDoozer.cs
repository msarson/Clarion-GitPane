using System;
using System.Collections;

namespace ICSharpCode.Core;

public class ToolbarItemDoozer : IDoozer
{
	public bool HandleConditions => true;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		string text = (codon.Properties.Contains("type") ? codon.Properties["type"] : "Item");
		string text2 = codon.Properties["loadclasslazy"];
		bool createCommand = !string.IsNullOrEmpty(text2) && text2.ToLower() == "false";
		return text switch
		{
			"Separator" => new ToolBarSeparator(codon, caller), 
			"CheckBox" => new ToolBarCheckBox(codon, caller), 
			"Item" => new ToolBarCommand(codon, caller, createCommand), 
			"ComboBox" => new ToolBarComboBox(codon, caller), 
			"TextBox" => new ToolBarTextBox(codon, caller), 
			"Label" => new ToolBarLabel(codon, caller), 
			"DropDownButton" => new ToolBarDropDownButton(codon, caller, subItems), 
			"SplitButton" => new ToolBarSplitButton(codon, caller, subItems), 
			"Builder" => codon.AddIn.CreateObject(codon.Properties["class"]), 
			_ => throw new NotSupportedException("unsupported menu item type : " + text), 
		};
	}
}

using System;
using System.Collections;

namespace ICSharpCode.Core;

public class MenuItemDoozer : IDoozer
{
	public bool HandleConditions => true;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		string text = (codon.Properties.Contains("type") ? codon.Properties["type"] : "Command");
		bool createCommand = codon.Properties["loadclasslazy"] == "false";
		switch (text)
		{
		case "Separator":
			return new MenuSeparator(codon, caller);
		case "CheckBox":
			return new MenuCheckBox(codon, caller);
		case "Item":
		case "Command":
			return new MenuCommand(codon, caller, createCommand);
		case "Menu":
			return new Menu(codon, caller, subItems);
		case "Builder":
			return codon.AddIn.CreateObject(codon.Properties["class"]);
		default:
			throw new NotSupportedException("unsupported menu item type : " + text);
		}
	}
}

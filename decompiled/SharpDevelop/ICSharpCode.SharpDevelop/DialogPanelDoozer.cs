using System.Collections;
using System.Collections.Generic;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class DialogPanelDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		string input = codon.Properties["label"];
		if (subItems == null || subItems.Count == 0)
		{
			if (codon.Properties.Contains("class"))
			{
				return new DefaultDialogPanelDescriptor(codon.Id, StringParser.Parse(input), codon.AddIn, codon.Properties["class"]);
			}
			return new DefaultDialogPanelDescriptor(codon.Id, StringParser.Parse(input));
		}
		List<IDialogPanelDescriptor> list = new List<IDialogPanelDescriptor>();
		foreach (IDialogPanelDescriptor subItem in subItems)
		{
			list.Add(subItem);
		}
		return new DefaultDialogPanelDescriptor(codon.Id, StringParser.Parse(input), list);
	}
}

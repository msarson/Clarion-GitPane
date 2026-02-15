using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class CodeCompletionBindingDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		string text = codon.Properties["extensions"];
		if (text != null && text.Length > 0)
		{
			return new LazyCodeCompletionBinding(codon, text.Split(';'));
		}
		return codon.AddIn.CreateObject(codon.Properties["class"]);
	}
}

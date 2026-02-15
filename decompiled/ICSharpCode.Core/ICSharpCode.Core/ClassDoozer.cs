using System.Collections;

namespace ICSharpCode.Core;

public class ClassDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return codon.AddIn.CreateObject(codon.Properties["class"]);
	}
}

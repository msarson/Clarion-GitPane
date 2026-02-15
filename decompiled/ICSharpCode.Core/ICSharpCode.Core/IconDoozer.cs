using System.Collections;

namespace ICSharpCode.Core;

public class IconDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new IconDescriptor(codon);
	}
}

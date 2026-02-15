using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class PadDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new PadDescriptor(codon);
	}
}

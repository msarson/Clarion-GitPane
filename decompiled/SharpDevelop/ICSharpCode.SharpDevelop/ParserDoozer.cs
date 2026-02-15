using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public sealed class ParserDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new ParserDescriptor(codon);
	}
}

using System.Collections;

namespace ICSharpCode.Core;

public interface IDoozer
{
	bool HandleConditions { get; }

	object BuildItem(object caller, Codon codon, ArrayList subItems);
}

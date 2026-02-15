using System.Collections;
using ICSharpCode.Core;

namespace SearchAndReplace;

public class SearchAndReplaceDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new SearchAndReplaceDescriptor(codon);
	}
}

using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class SchemeExtensionDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new SchemeExtensionDescriptor(codon);
	}
}

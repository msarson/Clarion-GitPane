using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

internal class BinaryFileGeneratorDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		if (codon.Properties.Contains("class"))
		{
			return new BinaryFileGeneratorDescriptor(codon.AddIn, codon.Properties["class"]);
		}
		return null;
	}
}

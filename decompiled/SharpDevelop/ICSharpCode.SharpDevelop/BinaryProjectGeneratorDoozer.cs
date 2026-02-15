using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

internal class BinaryProjectGeneratorDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		if (codon.Properties.Contains("class"))
		{
			return new BinaryProjectGeneratorDescriptor(codon.AddIn, codon.Properties["class"]);
		}
		return null;
	}
}

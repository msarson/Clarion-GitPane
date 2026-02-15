using System.Collections;
using ICSharpCode.Core;

namespace SoftVelocity.Generator.UI;

public class GenEditorsBindingDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new GenEditorBindingDescriptor(codon);
	}
}

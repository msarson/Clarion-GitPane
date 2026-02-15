using System.Collections;
using ICSharpCode.Core;

namespace SoftVelocity.Generator;

public class AppGenEditorDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new GenEditorDescriptor(codon);
	}
}

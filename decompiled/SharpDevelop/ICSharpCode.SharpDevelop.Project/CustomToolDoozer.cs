using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class CustomToolDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new CustomToolDescriptor(codon.Id, codon.Properties["fileNamePattern"], codon.Properties["class"], codon.AddIn);
	}
}

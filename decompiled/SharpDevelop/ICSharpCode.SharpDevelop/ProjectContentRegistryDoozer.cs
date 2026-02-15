using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public sealed class ProjectContentRegistryDoozer : IDoozer
{
	public bool HandleConditions => true;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new ProjectContentRegistryDescriptor(codon);
	}
}

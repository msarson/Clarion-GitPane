using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Debugging;

public class DebuggerDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new DebuggerDescriptor(codon);
	}
}

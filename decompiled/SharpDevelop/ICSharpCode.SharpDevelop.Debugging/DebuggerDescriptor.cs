using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Debugging;

public class DebuggerDescriptor
{
	private Codon codon;

	private IDebugger debugger;

	public IDebugger Debugger
	{
		get
		{
			if (debugger == null)
			{
				debugger = (IDebugger)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			return debugger;
		}
	}

	public bool SupportsStart => codon.Properties["supportsStart"] != "false";

	public bool SupportsStartWithoutDebugging => codon.Properties["supportsStartWithoutDebugger"] != "false";

	public bool SupportsStop => codon.Properties["supportsStop"] != "false";

	public bool SupportsStepping => codon.Properties["supportsStepping"] == "true";

	public bool SupportsExecutionControl => codon.Properties["supportsExecutionControl"] == "true";

	public DebuggerDescriptor(Codon codon)
	{
		this.codon = codon;
	}
}

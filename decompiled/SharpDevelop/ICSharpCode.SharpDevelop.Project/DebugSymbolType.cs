using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.Project;

public enum DebugSymbolType
{
	[Description("${res:Dialog.ProjectOptions.DebugSymbolType.None}")]
	None,
	[Description("${res:Dialog.ProjectOptions.DebugSymbolType.Full}")]
	Full,
	[Description("${res:Dialog.ProjectOptions.DebugSymbolType.PdbOnly}")]
	PdbOnly
}

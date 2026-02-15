using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.Project;

public enum OutputType
{
	[Description("${res:Dialog.Options.PrjOptions.Configuration.CompileTarget.Exe}")]
	Exe,
	[Description("${res:Dialog.Options.PrjOptions.Configuration.CompileTarget.WinExe}")]
	WinExe,
	[Description("${res:Dialog.Options.PrjOptions.Configuration.CompileTarget.Library}")]
	Library,
	[Description("${res:Dialog.Options.PrjOptions.Configuration.CompileTarget.Module}")]
	Module
}

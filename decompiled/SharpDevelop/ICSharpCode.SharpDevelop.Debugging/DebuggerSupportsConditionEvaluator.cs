using System;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Debugging;

public class DebuggerSupportsConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		DebuggerDescriptor descriptor = DebuggerService.Descriptor;
		return condition.Properties["debuggersupports"] switch
		{
			"Start" => descriptor?.SupportsStart ?? true, 
			"StartWithoutDebugging" => descriptor?.SupportsStartWithoutDebugging ?? true, 
			"Stop" => descriptor?.SupportsStop ?? true, 
			"ExecutionControl" => descriptor?.SupportsExecutionControl ?? false, 
			"Stepping" => descriptor?.SupportsStepping ?? false, 
			_ => throw new ArgumentException("Unknown debugger support for : >" + condition.Properties["debuggersupports"] + "< please fix addin file.", "debuggersupports"), 
		};
	}
}

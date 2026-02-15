using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Debugging;

public class IsProcessRunningConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		string text = condition.Properties.Get("isdebugging", string.Empty);
		string text2 = condition.Properties.Get("isprocessrunning", string.Empty);
		bool flag = DebuggerService.IsDebuggerLoaded && DebuggerService.CurrentDebugger.IsDebugging;
		bool flag2 = DebuggerService.IsDebuggerLoaded && DebuggerService.CurrentDebugger.IsProcessRunning;
		bool flag3 = text == string.Empty || flag == bool.Parse(text);
		bool result = text2 == string.Empty || flag2 == bool.Parse(text2);
		if (flag3)
		{
			return result;
		}
		return false;
	}
}

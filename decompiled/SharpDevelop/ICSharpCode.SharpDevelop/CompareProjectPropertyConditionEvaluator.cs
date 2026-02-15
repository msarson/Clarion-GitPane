using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class CompareProjectPropertyConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		MSBuildBasedProject mSBuildBasedProject = ((!(caller is IProject)) ? (ProjectService.CurrentProject as MSBuildBasedProject) : (caller as MSBuildBasedProject));
		if (mSBuildBasedProject == null)
		{
			return false;
		}
		string value = condition.Properties["comparisonType"];
		StringComparison comparisonType = ((!string.IsNullOrEmpty(value)) ? ((StringComparison)Enum.Parse(typeof(StringComparison), value)) : StringComparison.InvariantCultureIgnoreCase);
		string text = mSBuildBasedProject.GetEvaluatedProperty(StringParser.Parse(condition.Properties["property"]));
		if (text == null)
		{
			text = string.Empty;
		}
		return string.Equals(text, StringParser.Parse(condition.Properties["equals"]), comparisonType);
	}
}

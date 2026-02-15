using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.Conditions;

public class IsGeneratingOrBuilding : IConditionEvaluator
{
	public static bool IsValid()
	{
		if (!ApplicationService.IsGenerating)
		{
			return ProjectService.IsBuilding;
		}
		return true;
	}

	public bool IsValid(object caller, Condition condition)
	{
		return IsValid();
	}
}

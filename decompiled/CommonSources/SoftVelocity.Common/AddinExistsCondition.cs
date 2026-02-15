using ICSharpCode.Core;

namespace SoftVelocity.Common;

public class AddinExistsCondition : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		string value = condition.Properties["addin"];
		foreach (AddIn addIn in AddInTree.AddIns)
		{
			if (addIn.Name.Equals(value))
			{
				return true;
			}
		}
		return false;
	}
}

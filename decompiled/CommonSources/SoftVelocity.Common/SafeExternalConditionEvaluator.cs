using System;
using ICSharpCode.Core;

namespace SoftVelocity.Common;

public class SafeExternalConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		string key = condition.Properties["evaluator"];
		bool.TryParse(condition.Properties["defaultReturn"], out var result);
		bool.TryParse(condition.Properties["invert"], out var result2);
		bool flag;
		if (AddInTree.ConditionEvaluators.ContainsKey(key))
		{
			try
			{
				flag = AddInTree.ConditionEvaluators[key].IsValid(caller, condition);
				if (result2)
				{
					flag = !flag;
				}
			}
			catch (Exception)
			{
				flag = result;
			}
		}
		else
		{
			flag = result;
		}
		return flag;
	}
}

using System;

namespace ICSharpCode.Core;

public class OwnerStateConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (caller is IOwnerState)
		{
			try
			{
				Enum internalState = ((IOwnerState)caller).InternalState;
				Enum obj = (Enum)Enum.Parse(internalState.GetType(), condition.Properties["ownerstate"]);
				int num = int.Parse(internalState.ToString("D"));
				int num2 = int.Parse(obj.ToString("D"));
				return (num & num2) > 0;
			}
			catch (Exception)
			{
				throw new ApplicationException("can't parse '" + condition.Properties["state"] + "'. Not a valid value.");
			}
		}
		return false;
	}
}

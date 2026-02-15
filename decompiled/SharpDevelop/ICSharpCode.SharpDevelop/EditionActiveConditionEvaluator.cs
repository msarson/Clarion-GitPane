using System;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class EditionActiveConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		try
		{
			return VersionService.Version == (IDEVersion)Enum.Parse(typeof(IDEVersion), condition.Properties["edition"]);
		}
		catch (Exception)
		{
			return false;
		}
	}
}

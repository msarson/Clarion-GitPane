using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class GeneratorAddinMissing : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (AddInTree.ExistAddin("ApplicationGenerator"))
		{
			return false;
		}
		return true;
	}
}

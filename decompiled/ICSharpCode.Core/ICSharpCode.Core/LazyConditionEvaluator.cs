namespace ICSharpCode.Core;

public class LazyConditionEvaluator : IConditionEvaluator
{
	private AddIn addIn;

	private string name;

	private string className;

	public string Name => name;

	public string ClassName => className;

	public LazyConditionEvaluator(AddIn addIn, Properties properties)
	{
		this.addIn = addIn;
		name = properties["name"];
		className = properties["class"];
	}

	public bool IsValid(object caller, Condition condition)
	{
		IConditionEvaluator conditionEvaluator = (IConditionEvaluator)addIn.CreateObject(className);
		if (conditionEvaluator == null)
		{
			return false;
		}
		AddInTree.ConditionEvaluators[name] = conditionEvaluator;
		return conditionEvaluator.IsValid(caller, condition);
	}

	public override string ToString()
	{
		return $"[LazyLoadConditionEvaluator: className = {className}, name = {name}]";
	}
}

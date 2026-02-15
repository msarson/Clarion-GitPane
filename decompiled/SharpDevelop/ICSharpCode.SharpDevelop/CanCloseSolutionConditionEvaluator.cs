using System.ComponentModel;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class CanCloseSolutionConditionEvaluator : IConditionEvaluator
{
	private static IConditionEvaluator appLoaded = null;

	public static CancelEventHandler CanClose;

	public bool IsValid(object caller, Condition condition)
	{
		if (ProjectService.OpenSolution == null)
		{
			return false;
		}
		if (appLoaded == null && AddInTree.ConditionEvaluators.ContainsKey("AreApplicationsLoaded"))
		{
			appLoaded = AddInTree.ConditionEvaluators["AreApplicationsLoaded"];
		}
		if (appLoaded != null)
		{
			return !appLoaded.IsValid(caller, condition);
		}
		if (CanClose != null)
		{
			CancelEventArgs e = new CancelEventArgs();
			CanClose(this, e);
			return !e.Cancel;
		}
		return true;
	}
}

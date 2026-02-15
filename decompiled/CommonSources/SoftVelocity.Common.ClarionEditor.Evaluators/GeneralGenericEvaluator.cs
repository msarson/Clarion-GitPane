using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.Evaluators;

public class GeneralGenericEvaluator<T> : GeneralEvaluator
{
	public GeneralGenericEvaluator(IsCondition isC, bool isReverse)
		: base(isC, isReverse)
	{
	}

	protected virtual bool IsSelectedObject()
	{
		if (base.View.BaseReportDesignerControl.SelectedObject is T)
		{
			return true;
		}
		return false;
	}

	public override bool IsValid(object caller, Condition condition)
	{
		if (IsSelectedObject() && m_IsCond != null && m_IsCond())
		{
			return norm_res;
		}
		return !norm_res;
	}
}

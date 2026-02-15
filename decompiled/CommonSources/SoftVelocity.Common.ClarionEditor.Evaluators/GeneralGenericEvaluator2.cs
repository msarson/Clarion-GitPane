namespace SoftVelocity.Common.ClarionEditor.Evaluators;

public class GeneralGenericEvaluator2<T1, T2> : GeneralGenericEvaluator<T1>
{
	public GeneralGenericEvaluator2(IsCondition isC, bool isReverse)
		: base(isC, isReverse)
	{
	}

	protected override bool IsSelectedObject()
	{
		if (base.View.BaseReportDesignerControl.SelectedObject is T1 || base.View.BaseReportDesignerControl.SelectedObject is T2)
		{
			return true;
		}
		return false;
	}
}

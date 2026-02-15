using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.Evaluators;

public class GeneralEvaluator : ClaReportEvaluatorBase
{
	public delegate bool IsCondition();

	protected IsCondition m_IsCond;

	protected bool norm_res = true;

	public GeneralEvaluator(IsCondition isC, bool isReverse)
	{
		m_IsCond = isC;
		if (isReverse)
		{
			norm_res = !norm_res;
		}
	}

	public override bool IsValid(object caller, Condition condition)
	{
		if (m_IsCond != null && m_IsCond())
		{
			return norm_res;
		}
		return !norm_res;
	}
}

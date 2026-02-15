using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using VisualHint.SmartPropertyGrid;

namespace SoftVelocity.Common.ClarionEditor.Evaluators;

public class GenericEvaluator2<T1, T2> : ClaReportEvaluatorBase
{
	public override bool IsValid(object caller, Condition condition)
	{
		if ((base.View.BaseReportDesignerControl.SelectedObject is T1 || base.View.BaseReportDesignerControl.SelectedObject is T2) && ((PropertyGrid)(object)PropertyPad.Grid).SelectedObject != base.View.ReportDesignerControl.ReportSettings)
		{
			return true;
		}
		return false;
	}
}

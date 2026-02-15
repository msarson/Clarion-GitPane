using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using VisualHint.SmartPropertyGrid;

namespace SoftVelocity.Common.ClarionEditor.Evaluators;

public class GenericEvaluator<T> : ClaReportEvaluatorBase
{
	public override bool IsValid(object caller, Condition condition)
	{
		if (base.View.BaseReportDesignerControl.SelectedObject is T && PropertyPad.Grid != null && ((PropertyGrid)(object)PropertyPad.Grid).SelectedObject != base.View.ReportDesignerControl.ReportSettings)
		{
			return true;
		}
		return false;
	}
}

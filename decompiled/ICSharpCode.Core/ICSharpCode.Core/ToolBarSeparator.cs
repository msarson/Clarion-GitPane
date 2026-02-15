using System.Windows.Forms;

namespace ICSharpCode.Core;

public class ToolBarSeparator : ToolStripSeparator, IStatusUpdate
{
	private object caller;

	private Codon codon;

	public string CodonId => codon.Id;

	public ToolBarSeparator()
	{
		RightToLeft = RightToLeft.Inherit;
	}

	public ToolBarSeparator(Codon codon, object caller)
	{
		RightToLeft = RightToLeft.Inherit;
		this.caller = caller;
		this.codon = codon;
	}

	public virtual void UpdateStatus()
	{
		if (codon != null)
		{
			ConditionFailedAction failedAction = codon.GetFailedAction(caller);
			Enabled = failedAction != ConditionFailedAction.Disable;
			base.Visible = failedAction != ConditionFailedAction.Exclude;
		}
	}

	public virtual void UpdateText()
	{
	}
}

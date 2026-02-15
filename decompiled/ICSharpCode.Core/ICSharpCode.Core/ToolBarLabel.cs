using System.Windows.Forms;

namespace ICSharpCode.Core;

public class ToolBarLabel : ToolStripLabel, IStatusUpdate
{
	private object caller;

	private Codon codon;

	private ICommand menuCommand;

	public object Caller => caller;

	public override bool Enabled
	{
		get
		{
			if (codon == null)
			{
				return base.Enabled;
			}
			ConditionFailedAction failedAction = codon.GetFailedAction(caller);
			return failedAction != ConditionFailedAction.Disable;
		}
	}

	public string CodonId => codon.Id;

	public ToolBarLabel(Codon codon, object caller)
	{
		RightToLeft = RightToLeft.Inherit;
		this.caller = caller;
		this.codon = codon;
		if (codon.Properties.Contains("class"))
		{
			menuCommand = (ICommand)codon.AddIn.CreateObject(codon.Properties["class"]);
			menuCommand.Owner = this;
		}
		UpdateText();
		UpdateStatus();
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
		if (codon.Properties.Contains("label"))
		{
			Text = StringParser.Parse(codon.Properties["label"]);
		}
		if (codon.Properties.Contains("tooltip"))
		{
			base.ToolTipText = StringParser.Parse(codon.Properties["tooltip"]);
		}
	}
}

using System;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class ToolBarCommand : ToolStripMenuItem, IStatusUpdate
{
	private object caller;

	private Codon codon;

	private ICommand menuCommand;

	public string CodonId => codon.Id;

	public ToolBarCommand(Codon codon, object caller, bool createCommand)
	{
		RightToLeft = RightToLeft.Inherit;
		this.caller = caller;
		this.codon = codon;
		if (createCommand)
		{
			menuCommand = (ICommand)codon.AddIn.CreateObject(codon.Properties["class"]);
		}
		if (codon.Properties.Contains("label"))
		{
			Text = StringParser.Parse(codon.Properties["label"]);
		}
		if (Image == null && codon.Properties.Contains("icon"))
		{
			Image = ResourceService.GetBitmap(StringParser.Parse(codon.Properties["icon"]));
		}
		UpdateStatus();
		UpdateText();
	}

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);
		CreateMenuCommand();
		if (menuCommand != null)
		{
			menuCommand.Run();
		}
	}

	private void CreateMenuCommand()
	{
		if (menuCommand == null)
		{
			menuCommand = (ICommand)codon.AddIn.CreateObject(codon.Properties["class"]);
		}
		if (menuCommand != null)
		{
			menuCommand.Owner = caller;
		}
	}

	public virtual void UpdateStatus()
	{
		if (codon != null)
		{
			ConditionFailedAction failedAction = codon.GetFailedAction(caller);
			base.Visible = failedAction != ConditionFailedAction.Exclude;
			bool flag = failedAction != ConditionFailedAction.Disable;
			if (flag && menuCommand != null && menuCommand is IMenuCommand)
			{
				flag = ((IMenuCommand)menuCommand).IsEnabled;
			}
			Enabled = flag;
			if (base.Visible && codon.Properties.Contains("icon"))
			{
				Image = ResourceService.GetBitmap(StringParser.Parse(codon.Properties["icon"]));
			}
		}
	}

	public virtual void UpdateText()
	{
		if (codon != null)
		{
			if (codon.Properties.Contains("tooltip"))
			{
				base.ToolTipText = StringParser.Parse(codon.Properties["tooltip"]);
			}
			if (codon.Properties.Contains("label"))
			{
				Text = StringParser.Parse(codon.Properties["label"]);
			}
		}
	}
}

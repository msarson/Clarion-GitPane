using System;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class ToolBarCheckBox : ToolStripButton, IStatusUpdate
{
	private object caller;

	private Codon codon;

	private string description = string.Empty;

	private ICheckableMenuCommand menuCommand;

	public ICheckableMenuCommand MenuCommand => menuCommand;

	public object Caller => caller;

	public string Description
	{
		get
		{
			return description;
		}
		set
		{
			description = value;
		}
	}

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

	public ToolBarCheckBox(string text)
	{
		RightToLeft = RightToLeft.Inherit;
		Text = text;
	}

	public ToolBarCheckBox(Codon codon, object caller)
	{
		RightToLeft = RightToLeft.Inherit;
		this.caller = caller;
		this.codon = codon;
		try
		{
			menuCommand = (ICheckableMenuCommand)codon.AddIn.CreateObject(codon.Properties["class"]);
		}
		catch (Exception)
		{
		}
		if (menuCommand == null)
		{
			MessageService.ShowError("Can't create toolbar checkbox : " + codon.Id);
		}
		menuCommand.Owner = this;
		if (codon.Properties.Contains("label"))
		{
			Text = StringParser.Parse(codon.Properties["label"]);
		}
		if (Image == null && codon.Properties.Contains("icon"))
		{
			Image = ResourceService.GetBitmap(StringParser.Parse(codon.Properties["icon"]));
		}
		UpdateText();
		UpdateStatus();
	}

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);
		if (menuCommand != null)
		{
			menuCommand.Run();
			base.Checked = menuCommand.IsChecked;
		}
	}

	public virtual void UpdateStatus()
	{
		if (codon == null)
		{
			return;
		}
		ConditionFailedAction failedAction = codon.GetFailedAction(caller);
		bool flag = failedAction != ConditionFailedAction.Exclude;
		if (flag != base.Visible)
		{
			base.Visible = flag;
		}
		if (menuCommand != null)
		{
			bool isChecked = menuCommand.IsChecked;
			if (isChecked != base.Checked)
			{
				base.Checked = isChecked;
			}
		}
		if (base.Visible && codon.Properties.Contains("icon"))
		{
			Image = ResourceService.GetBitmap(StringParser.Parse(codon.Properties["icon"]));
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

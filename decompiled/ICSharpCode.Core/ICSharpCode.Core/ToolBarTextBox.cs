using System;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class ToolBarTextBox : ToolStripTextBox, IStatusUpdate
{
	private object caller;

	private Codon codon;

	private string description = string.Empty;

	private ITextBoxCommand menuCommand;

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

	public ITextBoxCommand MenuCommand => menuCommand;

	public override Size Size
	{
		get
		{
			return base.Size;
		}
		set
		{
			base.Size = value;
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
			bool flag = failedAction != ConditionFailedAction.Disable;
			if (menuCommand != null)
			{
				flag &= menuCommand.IsEnabled;
			}
			return flag;
		}
	}

	public string CodonId => codon.Id;

	public ToolBarTextBox(Codon codon, object caller)
	{
		RightToLeft = RightToLeft.Inherit;
		this.caller = caller;
		this.codon = codon;
		base.TextBox.KeyDown += TextBox_KeyDown;
		menuCommand = (ITextBoxCommand)codon.AddIn.CreateObject(codon.Properties["class"]);
		menuCommand.Owner = this;
		if (menuCommand == null)
		{
			throw new NullReferenceException("Can't create textbox toolbox command");
		}
		ToolBarItemHelper.SetControlWidth(codon, this);
		UpdateText();
		UpdateStatus();
	}

	private void TextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			MenuCommand.Run();
		}
	}

	public virtual void UpdateStatus()
	{
		bool flag = base.Visible;
		if (codon != null)
		{
			ConditionFailedAction failedAction = codon.GetFailedAction(caller);
			flag = flag && failedAction != ConditionFailedAction.Exclude;
		}
		if (base.Visible != flag)
		{
			base.Visible = flag;
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

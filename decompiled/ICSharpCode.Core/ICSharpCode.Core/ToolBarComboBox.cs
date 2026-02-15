using System;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class ToolBarComboBox : ToolStripComboBox, IStatusUpdate
{
	private object caller;

	private Codon codon;

	private string description = string.Empty;

	private IComboBoxCommand menuCommand;

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

	public IComboBoxCommand MenuCommand => menuCommand;

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

	public ToolBarComboBox(Codon codon, object caller)
	{
		RightToLeft = RightToLeft.Inherit;
		base.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
		base.ComboBox.SelectionChangeCommitted += selectionChanged;
		base.ComboBox.KeyDown += ComboBoxKeyDown;
		this.caller = caller;
		this.codon = codon;
		menuCommand = (IComboBoxCommand)codon.AddIn.CreateObject(codon.Properties["class"]);
		menuCommand.Owner = this;
		if (menuCommand == null)
		{
			throw new NullReferenceException("Can't create combobox menu command");
		}
		if (codon.Properties.Contains("width"))
		{
			int result = 0;
			if (int.TryParse(codon.Properties["width"].Trim('"'), out result))
			{
				base.Width = result;
			}
		}
		UpdateText();
		UpdateStatus();
	}

	private void ComboBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			MenuCommand.Run();
		}
	}

	private void selectionChanged(object sender, EventArgs e)
	{
		base.ComboBox.Text = base.ComboBox.SelectedItem.ToString();
		MenuCommand.Run();
	}

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);
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

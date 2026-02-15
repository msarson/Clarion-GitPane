using System;
using System.Collections;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class ToolBarDropDownButton : ToolStripDropDownButton, IStatusUpdate
{
	private object caller;

	private Codon codon;

	private ICommand menuBuilder;

	private ArrayList subItems;

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

	public ToolBarDropDownButton(Codon codon, object caller, ArrayList subItems)
	{
		RightToLeft = RightToLeft.Inherit;
		this.caller = caller;
		this.codon = codon;
		this.subItems = subItems;
		if (codon.Properties.Contains("label"))
		{
			Text = StringParser.Parse(codon.Properties["label"]);
		}
		if (Image == null && codon.Properties.Contains("icon"))
		{
			Image = ResourceService.GetBitmap(StringParser.Parse(codon.Properties["icon"]));
		}
		if (menuBuilder == null && codon.Properties.Contains("class"))
		{
			menuBuilder = codon.AddIn.CreateObject(StringParser.Parse(codon.Properties["class"])) as ICommand;
			menuBuilder.Owner = this;
		}
		UpdateStatus();
		UpdateText();
	}

	private void CreateDropDownItems()
	{
		if (menuBuilder != null || subItems == null || subItems.Count == 0)
		{
			return;
		}
		base.DropDownItems.Clear();
		foreach (object subItem in subItems)
		{
			if (subItem is ToolStripItem)
			{
				base.DropDownItems.Add((ToolStripItem)subItem);
				if (subItem is IStatusUpdate)
				{
					((IStatusUpdate)subItem).UpdateStatus();
					((IStatusUpdate)subItem).UpdateText();
				}
			}
			else
			{
				ISubmenuBuilder submenuBuilder = (ISubmenuBuilder)subItem;
				base.DropDownItems.AddRange(submenuBuilder.BuildSubmenu(codon, caller));
			}
		}
	}

	protected override void OnDropDownShow(EventArgs e)
	{
		if (codon != null && !base.DropDown.Visible)
		{
			CreateDropDownItems();
		}
		base.OnDropDownShow(e);
	}

	public virtual void UpdateStatus()
	{
		if (codon != null)
		{
			ConditionFailedAction failedAction = codon.GetFailedAction(caller);
			bool flag = failedAction != ConditionFailedAction.Exclude;
			if (base.Visible != flag)
			{
				base.Visible = flag;
			}
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

using System;
using System.Collections;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class Menu : ToolStripMenuItem, IStatusUpdate
{
	private Codon codon;

	private object caller;

	private ArrayList subItems;

	private bool isInitialized;

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

	public Menu(Codon codon, object caller, ArrayList subItems)
	{
		if (subItems == null)
		{
			subItems = new ArrayList();
		}
		this.codon = codon;
		this.caller = caller;
		this.subItems = subItems;
		RightToLeft = RightToLeft.Inherit;
		UpdateText();
	}

	public Menu(string text, params ToolStripItem[] subItems)
	{
		Text = StringParser.Parse(text);
		base.DropDownItems.AddRange(subItems);
	}

	private void CreateDropDownItems()
	{
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
		if (codon == null)
		{
			return;
		}
		ConditionFailedAction failedAction = codon.GetFailedAction(caller);
		base.Visible = failedAction != ConditionFailedAction.Exclude;
		if (!isInitialized && failedAction != ConditionFailedAction.Exclude)
		{
			isInitialized = true;
			CreateDropDownItems();
			if (base.DropDownItems.Count == 0 && subItems.Count > 0)
			{
				base.DropDownItems.Add(new ToolStripMenuItem());
			}
		}
	}

	public virtual void UpdateText()
	{
		if (codon != null)
		{
			Text = StringParser.Parse(codon.Properties["label"]);
		}
	}
}

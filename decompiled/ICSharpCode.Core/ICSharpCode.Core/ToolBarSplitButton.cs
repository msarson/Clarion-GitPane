using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class ToolBarSplitButton : ToolStripSplitButton, IStatusUpdate
{
	private object caller;

	private Codon codon;

	private ArrayList subItems;

	private ICommand menuCommand;

	private Image imgButtonEnabled;

	private Image imgButtonDisabled;

	private bool buttonEnabled = true;

	private bool dropDownEnabled = true;

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
			if (menuCommand != null && menuCommand is IMenuCommand)
			{
				flag &= ((IMenuCommand)menuCommand).IsEnabled || dropDownEnabled;
			}
			return flag;
		}
	}

	public bool ButtonEnabled
	{
		get
		{
			return buttonEnabled;
		}
		set
		{
			buttonEnabled = value;
			UpdateButtonImage();
		}
	}

	public bool DropDownEnabled
	{
		get
		{
			return dropDownEnabled;
		}
		set
		{
			dropDownEnabled = value;
		}
	}

	public string CodonId => codon.Id;

	public ToolBarSplitButton(Codon codon, object caller, ArrayList subItems)
	{
		RightToLeft = RightToLeft.Inherit;
		this.caller = caller;
		this.codon = codon;
		this.subItems = subItems;
		if (subItems == null)
		{
			dropDownEnabled = false;
		}
		if (codon.Properties.Contains("label"))
		{
			Text = StringParser.Parse(codon.Properties["label"]);
		}
		if (imgButtonEnabled == null && codon.Properties.Contains("icon"))
		{
			imgButtonEnabled = ResourceService.GetBitmap(StringParser.Parse(codon.Properties["icon"]));
		}
		if (imgButtonDisabled == null && codon.Properties.Contains("disabledIcon"))
		{
			imgButtonDisabled = ResourceService.GetBitmap(StringParser.Parse(codon.Properties["disabledIcon"]));
		}
		if (imgButtonDisabled == null)
		{
			imgButtonDisabled = imgButtonEnabled;
		}
		menuCommand = codon.AddIn.CreateObject(codon.Properties["class"]) as ICommand;
		menuCommand.Owner = this;
		UpdateStatus();
		UpdateText();
	}

	private void CreateDropDownItems()
	{
		ToolStripItem[] array = null;
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
				array = submenuBuilder.BuildSubmenu(codon, caller);
				if (array != null)
				{
					base.DropDownItems.AddRange(array);
				}
			}
		}
	}

	protected override void OnDropDownShow(EventArgs e)
	{
		if (dropDownEnabled)
		{
			if (codon != null && !base.DropDown.Visible)
			{
				CreateDropDownItems();
			}
			base.OnDropDownShow(e);
		}
	}

	protected override void OnButtonClick(EventArgs e)
	{
		if (buttonEnabled)
		{
			base.OnButtonClick(e);
			menuCommand.Run();
		}
	}

	private void UpdateButtonImage()
	{
		Image = (buttonEnabled ? imgButtonEnabled : imgButtonDisabled);
	}

	public virtual void UpdateStatus()
	{
		if (codon == null)
		{
			return;
		}
		ConditionFailedAction failedAction = codon.GetFailedAction(caller);
		bool flag = failedAction != ConditionFailedAction.Exclude;
		if (base.Visible != flag)
		{
			base.Visible = flag;
		}
		if (base.Visible)
		{
			if (buttonEnabled && imgButtonEnabled != null)
			{
				Image = imgButtonEnabled;
			}
			else if (imgButtonDisabled != null)
			{
				Image = imgButtonDisabled;
			}
		}
		base.Enabled = Enabled;
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

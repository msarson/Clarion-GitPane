using System;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class MenuCheckBox : ToolStripMenuItem, IStatusUpdate
{
	private object caller;

	private Codon codon;

	private string description = string.Empty;

	private ICheckableMenuCommand menuCommand;

	public ICheckableMenuCommand MenuCommand
	{
		get
		{
			CreateMenuCommand();
			return menuCommand;
		}
	}

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

	private void CreateMenuCommand()
	{
		if (menuCommand == null)
		{
			try
			{
				menuCommand = (ICheckableMenuCommand)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex, "Can't create menu command : " + codon.Id);
			}
		}
	}

	public MenuCheckBox(string text)
	{
		RightToLeft = RightToLeft.Inherit;
		Text = text;
	}

	public MenuCheckBox(Codon codon, object caller)
	{
		RightToLeft = RightToLeft.Inherit;
		this.caller = caller;
		this.codon = codon;
		UpdateText();
		if (codon.Properties.Contains("shortcut"))
		{
			string originalShortcutKeysText = codon.Properties["shortcut"];
			originalShortcutKeysText = MenuShortcutService.GetShortcut(codon.ShortcutId, originalShortcutKeysText);
			base.ShortcutKeys = ICSharpCode.Core.MenuCommand.ParseShortcut(originalShortcutKeysText);
			string text = ICSharpCode.Core.MenuCommand.MakeShortcutText(base.ShortcutKeys);
			if (text == null)
			{
				base.ShortcutKeys = Keys.None;
			}
			else if (text != string.Empty)
			{
				base.ShortcutKeyDisplayString = text;
			}
		}
	}

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);
		if (codon != null)
		{
			MenuCommand.Run();
			base.Checked = MenuCommand.IsChecked;
		}
	}

	public virtual void UpdateStatus()
	{
		if (codon == null)
		{
			return;
		}
		ConditionFailedAction failedAction = codon.GetFailedAction(caller);
		base.Visible = failedAction != ConditionFailedAction.Exclude;
		if (menuCommand == null && !string.IsNullOrEmpty(codon.Properties["checked"]))
		{
			base.Checked = string.Equals(StringParser.Parse(codon.Properties["checked"]), bool.TrueString, StringComparison.OrdinalIgnoreCase);
			return;
		}
		CreateMenuCommand();
		if (menuCommand != null)
		{
			base.Checked = menuCommand.IsChecked;
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

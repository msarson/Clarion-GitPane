using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class MenuCommand : ToolStripMenuItem, IStatusUpdate
{
	private const int MAPVK_VK_TO_CHAR = 2;

	private object caller;

	private Codon codon;

	private ICommand menuCommand;

	private string description = "";

	public static Converter<string, ICommand> LinkCommandCreator;

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

	public ICommand Command
	{
		get
		{
			if (menuCommand == null)
			{
				CreateCommand();
			}
			return menuCommand;
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
			if (menuCommand != null && menuCommand is IMenuCommand)
			{
				flag &= ((IMenuCommand)menuCommand).IsEnabled;
			}
			return flag;
		}
	}

	public string CodonId => codon.Id;

	private void CreateCommand()
	{
		try
		{
			string text = codon.Properties["link"];
			if (text != null && text.Length > 0)
			{
				if (LinkCommandCreator == null)
				{
					throw new NotSupportedException("MenuCommand.LinkCommandCreator is not set, cannot create LinkCommands.");
				}
				menuCommand = LinkCommandCreator(codon.Properties["link"]);
			}
			else
			{
				menuCommand = (ICommand)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			if (menuCommand != null)
			{
				menuCommand.Owner = caller;
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex, "Can't create menu command : " + codon.Id);
		}
	}

	public MenuCommand(Codon codon, object caller)
		: this(codon, caller, createCommand: false)
	{
	}

	public static Keys ParseShortcut(string shortcutString)
	{
		Keys keys = Keys.None;
		if (!string.IsNullOrEmpty(shortcutString) && shortcutString.Trim().Length > 0)
		{
			try
			{
				shortcutString = shortcutString.Trim();
				string[] array = shortcutString.Split('|');
				foreach (string value in array)
				{
					keys |= (Keys)Enum.Parse(typeof(Keys), value);
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
				return Keys.None;
			}
		}
		return keys;
	}

	public static string MakeShortcutText(Keys shortcutKeys)
	{
		Keys keys = shortcutKeys & Keys.KeyCode;
		string text = TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(keys);
		if (text.StartsWith("oem", StringComparison.InvariantCultureIgnoreCase))
		{
			char c = (char)(MapVirtualKey((int)keys, 2) % 65535);
			if (c != 0)
			{
				string text2 = TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(shortcutKeys);
				return text2.Replace(text, c.ToString());
			}
			return null;
		}
		return string.Empty;
	}

	[DllImport("User32.dll")]
	private static extern uint MapVirtualKey(int uCode, int uMapType);

	public MenuCommand(Codon codon, object caller, bool createCommand)
	{
		RightToLeft = RightToLeft.Inherit;
		this.caller = caller;
		this.codon = codon;
		if (createCommand)
		{
			CreateCommand();
		}
		UpdateText();
		if (!codon.Properties.Contains("shortcut"))
		{
			return;
		}
		string originalShortcutKeysText = codon.Properties["shortcut"];
		originalShortcutKeysText = MenuShortcutService.GetShortcut(codon.ShortcutId, originalShortcutKeysText);
		try
		{
			base.ShortcutKeys = ParseShortcut(originalShortcutKeysText);
			string text = MakeShortcutText(base.ShortcutKeys);
			if (text == null)
			{
				base.ShortcutKeys = Keys.None;
			}
			else if (text != string.Empty)
			{
				base.ShortcutKeyDisplayString = text;
			}
		}
		catch
		{
			base.ShortcutKeys = Keys.None;
			base.ShortcutKeyDisplayString = string.Empty;
		}
	}

	public MenuCommand(string label, EventHandler handler)
		: this(label)
	{
		base.Click += handler;
	}

	public MenuCommand(string label)
	{
		RightToLeft = RightToLeft.Inherit;
		codon = null;
		caller = null;
		Text = StringParser.Parse(label);
	}

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);
		if (codon != null && GetVisible() && Enabled)
		{
			Command?.Run();
		}
	}

	private bool GetVisible()
	{
		if (codon == null)
		{
			return true;
		}
		return codon.GetFailedAction(caller) != ConditionFailedAction.Exclude;
	}

	public virtual void UpdateStatus()
	{
		if (codon == null)
		{
			return;
		}
		if (Image == null && codon.Properties.Contains("icon"))
		{
			try
			{
				Image = ResourceService.GetBitmap(codon.Properties["icon"]);
			}
			catch (ResourceNotFoundException)
			{
			}
		}
		base.Visible = GetVisible();
	}

	public virtual void UpdateText()
	{
		if (codon != null)
		{
			Text = StringParser.Parse(codon.Properties["label"]);
		}
	}
}

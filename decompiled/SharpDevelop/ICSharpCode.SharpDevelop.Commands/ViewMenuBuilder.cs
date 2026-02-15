using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public abstract class ViewMenuBuilder : ISubmenuBuilder
{
	private class MyMenuItem : MenuCommand
	{
		private PadDescriptor padDescriptor;

		public MyMenuItem(PadDescriptor padDescriptor)
			: base(null, null)
		{
			this.padDescriptor = padDescriptor;
			Text = StringParser.Parse(padDescriptor.Title);
			if (!string.IsNullOrEmpty(padDescriptor.Icon))
			{
				try
				{
					base.Image = IconService.GetBitmap(padDescriptor.Icon);
				}
				catch (InvalidCastException)
				{
					base.Image = IconService.GetIcon(padDescriptor.Icon).ToBitmap();
				}
			}
			if (padDescriptor.Shortcut != null)
			{
				base.ShortcutKeys = MenuCommand.ParseShortcut(padDescriptor.Shortcut);
				string text = MenuCommand.MakeShortcutText(base.ShortcutKeys);
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
			padDescriptor.BringPadToFront();
		}
	}

	protected abstract string Category { get; }

	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		List<ToolStripItem> list = new List<ToolStripItem>();
		foreach (PadDescriptor item in WorkbenchSingleton.Workbench.PadContentCollection)
		{
			if (item.Category == Category)
			{
				list.Add(new MyMenuItem(item));
			}
		}
		return list.ToArray();
	}
}

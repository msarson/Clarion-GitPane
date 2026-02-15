using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public static class MenuService
{
	private class QuickInsertMenuHandler
	{
		private TextBoxBase targetControl;

		private string text;

		public EventHandler EventHandler => PopupMenuHandler;

		public QuickInsertMenuHandler(TextBoxBase targetControl, string text)
		{
			this.targetControl = targetControl;
			this.text = text;
		}

		private void PopupMenuHandler(object sender, EventArgs e)
		{
			targetControl.SelectedText += text;
		}
	}

	private class QuickInsertHandler
	{
		private Control popupControl;

		private ContextMenuStrip quickInsertMenu;

		public QuickInsertHandler(Control popupControl, ContextMenuStrip quickInsertMenu)
		{
			this.popupControl = popupControl;
			this.quickInsertMenu = quickInsertMenu;
			popupControl.Click += showQuickInsertMenu;
		}

		private void showQuickInsertMenu(object sender, EventArgs e)
		{
			Point position = new Point(popupControl.Width, 0);
			quickInsertMenu.Show(popupControl, position);
		}
	}

	private static bool isContextMenuOpen;

	public static bool IsContextMenuOpen => isContextMenuOpen;

	public static void AddItemsToMenu(ToolStripItemCollection collection, object owner, string addInTreePath)
	{
		ArrayList arrayList = AddInTree.GetTreeNode(addInTreePath).BuildChildItems(owner);
		foreach (object item in arrayList)
		{
			if (item is ToolStripItem)
			{
				collection.Add((ToolStripItem)item);
				if (item is IStatusUpdate)
				{
					((IStatusUpdate)item).UpdateStatus();
				}
			}
			else
			{
				ISubmenuBuilder submenuBuilder = (ISubmenuBuilder)item;
				collection.AddRange(submenuBuilder.BuildSubmenu(null, owner));
			}
		}
	}

	public static ContextMenuStrip CreateContextMenu(object owner, string addInTreePath)
	{
		if (addInTreePath == null)
		{
			return null;
		}
		try
		{
			ArrayList buildItems = AddInTree.GetTreeNode(addInTreePath).BuildChildItems(owner);
			ContextMenuStrip contextMenu = new ContextMenuStrip();
			contextMenu.Items.Add(new ToolStripMenuItem("dummy"));
			contextMenu.Opening += delegate
			{
				contextMenu.Items.Clear();
				foreach (object item in buildItems)
				{
					if (item is ToolStripItem)
					{
						contextMenu.Items.Add((ToolStripItem)item);
					}
					else
					{
						ISubmenuBuilder submenuBuilder = (ISubmenuBuilder)item;
						contextMenu.Items.AddRange(submenuBuilder.BuildSubmenu(null, owner));
					}
				}
			};
			contextMenu.Opened += ContextMenuOpened;
			contextMenu.Closed += ContextMenuClosed;
			return contextMenu;
		}
		catch (TreePathNotFoundException)
		{
			MessageService.ShowError("Warning tree path '" + addInTreePath + "' not found.");
			return null;
		}
	}

	private static void ContextMenuOpened(object sender, EventArgs e)
	{
		isContextMenuOpen = true;
		ContextMenuStrip contextMenuStrip = (ContextMenuStrip)sender;
		foreach (object item in contextMenuStrip.Items)
		{
			if (item is IStatusUpdate)
			{
				((IStatusUpdate)item).UpdateStatus();
			}
		}
	}

	private static void ContextMenuClosed(object sender, EventArgs e)
	{
		isContextMenuOpen = false;
	}

	public static void ShowContextMenu(object owner, string addInTreePath, Control parent, int x, int y)
	{
		CreateContextMenu(owner, addInTreePath)?.Show(parent, new Point(x, y));
	}

	public static void CreateQuickInsertMenu(TextBoxBase targetControl, Control popupControl, string[,] quickInsertMenuItems)
	{
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		for (int i = 0; i < quickInsertMenuItems.GetLength(0); i++)
		{
			if (quickInsertMenuItems[i, 0] == "-")
			{
				contextMenuStrip.Items.Add(new MenuSeparator());
				continue;
			}
			MenuCommand value = new MenuCommand(quickInsertMenuItems[i, 0], new QuickInsertMenuHandler(targetControl, quickInsertMenuItems[i, 1]).EventHandler);
			contextMenuStrip.Items.Add(value);
		}
		new QuickInsertHandler(popupControl, contextMenuStrip);
	}
}

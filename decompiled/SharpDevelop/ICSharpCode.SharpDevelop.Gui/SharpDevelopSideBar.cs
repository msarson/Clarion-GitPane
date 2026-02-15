using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Gui;

public class SharpDevelopSideBar : SideBarControl, IOwnerState
{
	[Flags]
	public enum SidebarState
	{
		Nothing = 0,
		CanMoveUp = 1,
		CanMoveDown = 2,
		TabCanBeDeleted = 4,
		CanMoveItemUp = 8,
		CanMoveItemDown = 0x10,
		CanBeRenamed = 0x20
	}

	private static readonly string contextMenuPath = "/SharpDevelop/Workbench/SharpDevelopSideBar/ContextMenu";

	private static readonly string sideTabContextMenuPath = "/SharpDevelop/Workbench/SharpDevelopSideBar/SideTab/ContextMenu";

	private Point mousePosition;

	private Point itemMousePosition;

	public SideTab ClipboardRing;

	protected SidebarState internalState = SidebarState.TabCanBeDeleted;

	private Hashtable standardTabs = new Hashtable();

	public static SharpDevelopSideBar SideBar;

	public Point ItemMousePosition => itemMousePosition;

	public Point SideBarMousePosition => mousePosition;

	public Enum InternalState => internalState;

	public event SideTabEventHandler SideTabDeleted;

	public SharpDevelopSideBar(XmlElement el)
		: this()
	{
		SetOptions(el);
	}

	public SharpDevelopSideBar()
	{
		Font = FontService.GetFont(FontService.FontType.ListControls);
		SideBar = this;
		base.SideTabItemFactory = new SharpDevelopSideTabItemFactory();
		base.MouseUp += SetContextMenu;
		sideTabContent.MouseUp += SetItemContextMenu;
		foreach (TextTemplate textTemplate in TextTemplate.TextTemplates)
		{
			SideTab sideTab = new SideTab(this, textTemplate.Name);
			sideTab.DisplayName = StringParser.Parse(sideTab.Name);
			sideTab.CanSaved = false;
			foreach (TextTemplate.Entry entry in textTemplate.Entries)
			{
				sideTab.Items.Add(base.SideTabItemFactory.CreateSideTabItem(entry.Display, entry.Value));
			}
			bool canBeDeleted = (sideTab.CanDragDrop = false);
			sideTab.CanBeDeleted = canBeDeleted;
			standardTabs[sideTab] = true;
			base.Tabs.Add(sideTab);
		}
		sideTabContent.DoubleClick += MyDoubleClick;
	}

	public void MyDoubleClick(object sender, EventArgs e)
	{
	}

	public void PutInClipboardRing(string text)
	{
		foreach (SideTab tab in base.Tabs)
		{
			if (tab.IsClipboardRing)
			{
				tab.Items.Add("Text:" + text.Trim(), text);
				if (tab.Items.Count > 20)
				{
					tab.Items.RemoveAt(0);
				}
				break;
			}
		}
	}

	public void DeleteSideTab(SideTab tab)
	{
		if (tab != null)
		{
			base.Tabs.Remove(tab);
			OnSideTabDeleted(tab);
		}
	}

	private void SetDeletedState(SideTabItem item)
	{
		if (item != null)
		{
			SetDeletedState(item.CanBeDeleted);
		}
		else
		{
			SetDeletedState(canBeDeleted: false);
		}
	}

	private void SetDeletedState(bool canBeDeleted)
	{
		if (canBeDeleted)
		{
			internalState |= SidebarState.TabCanBeDeleted;
		}
		else
		{
			internalState &= ~SidebarState.TabCanBeDeleted;
		}
	}

	private void SetRenameState(SideTabItem item)
	{
		if (item != null)
		{
			SetRenameState(item.CanBeRenamed);
		}
		else
		{
			SetRenameState(canBeRenamed: false);
		}
	}

	private void SetRenameState(bool canBeRenamed)
	{
		if (canBeRenamed)
		{
			internalState |= SidebarState.CanBeRenamed;
		}
		else
		{
			internalState &= ~SidebarState.CanBeRenamed;
		}
	}

	private void SetContextMenu(object sender, MouseEventArgs e)
	{
		ExitRenameMode();
		int tabIndexAt = GetTabIndexAt(e.X, e.Y);
		if (tabIndexAt >= 0)
		{
			SideTab sideTab = base.Tabs[tabIndexAt];
			SetDeletedState(sideTab.CanBeDeleted);
			SetRenameState(sideTab.CanBeRenamed);
			if (tabIndexAt > 0)
			{
				internalState |= SidebarState.CanMoveUp;
			}
			else
			{
				internalState &= ~SidebarState.CanMoveUp;
			}
			if (tabIndexAt < base.Tabs.Count - 1)
			{
				internalState |= SidebarState.CanMoveDown;
			}
			else
			{
				internalState &= ~SidebarState.CanMoveDown;
			}
			base.Tabs.DragOverTab = sideTab;
			Refresh();
			base.Tabs.DragOverTab = null;
		}
		if (e.Button == MouseButtons.Right)
		{
			MenuService.ShowContextMenu(this, contextMenuPath, this, e.X, e.Y);
		}
	}

	private void SetItemContextMenu(object sender, MouseEventArgs e)
	{
		ExitRenameMode();
		if (e.Button == MouseButtons.Right)
		{
			int num = base.Tabs.IndexOf(base.ActiveTab);
			if (num > 0)
			{
				internalState |= SidebarState.CanMoveUp;
			}
			else
			{
				internalState &= ~SidebarState.CanMoveUp;
			}
			if (num < base.Tabs.Count - 1)
			{
				internalState |= SidebarState.CanMoveDown;
			}
			else
			{
				internalState &= ~SidebarState.CanMoveDown;
			}
			base.Tabs.DragOverTab = base.ActiveTab;
			Refresh();
			base.Tabs.DragOverTab = null;
		}
		if (e.Button == MouseButtons.Right)
		{
			SetDeletedState(base.ActiveTab.SelectedItem);
			SetRenameState(base.ActiveTab.SelectedItem);
			int num2 = base.ActiveTab.Items.IndexOf(base.ActiveTab.SelectedItem);
			if (num2 > 0)
			{
				internalState |= SidebarState.CanMoveItemUp;
			}
			else
			{
				internalState &= ~SidebarState.CanMoveItemUp;
			}
			if (num2 < base.ActiveTab.Items.Count - 1)
			{
				internalState |= SidebarState.CanMoveItemDown;
			}
			else
			{
				internalState &= ~SidebarState.CanMoveItemDown;
			}
			MenuService.ShowContextMenu(this, sideTabContextMenuPath, sideTabContent, e.X, e.Y);
		}
	}

	private void MoveItem(object sender, MouseEventArgs e)
	{
		itemMousePosition = new Point(e.X, e.Y);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		mousePosition = new Point(e.X, e.Y);
	}

	private void SetOptions(XmlElement el)
	{
		foreach (XmlElement childNode in el.ChildNodes)
		{
			SideTab sideTab = new SideTab(this, childNode.GetAttribute("text"));
			sideTab.DisplayName = StringParser.Parse(sideTab.Name);
			if (sideTab.Name == el.GetAttribute("activetab"))
			{
				base.ActiveTab = sideTab;
			}
			else if (base.ActiveTab == null)
			{
				base.ActiveTab = sideTab;
			}
			foreach (XmlElement childNode2 in childNode.ChildNodes)
			{
				sideTab.Items.Add(base.SideTabItemFactory.CreateSideTabItem(childNode2.GetAttribute("text"), childNode2.GetAttribute("value")));
			}
			if (childNode.GetAttribute("clipboardring") == "true")
			{
				sideTab.CanBeDeleted = false;
				sideTab.CanDragDrop = false;
				sideTab.Name = "${res:SharpDevelop.SideBar.ClipboardRing}";
				sideTab.DisplayName = StringParser.Parse(sideTab.Name);
				sideTab.IsClipboardRing = true;
			}
			base.Tabs.Add(sideTab);
		}
	}

	public XmlElement ToXmlElement(XmlDocument doc)
	{
		if (doc == null)
		{
			throw new ArgumentNullException("doc");
		}
		XmlElement xmlElement = doc.CreateElement("SideBar");
		xmlElement.SetAttribute("activetab", base.ActiveTab.Name);
		foreach (SideTab tab in base.Tabs)
		{
			if (!tab.CanSaved || standardTabs[tab] != null)
			{
				continue;
			}
			XmlElement xmlElement2 = doc.CreateElement("SideTab");
			if (tab.IsClipboardRing)
			{
				xmlElement2.SetAttribute("clipboardring", "true");
			}
			xmlElement2.SetAttribute("text", tab.Name);
			foreach (SideTabItem item in tab.Items)
			{
				XmlElement xmlElement3 = doc.CreateElement("SideTabItem");
				xmlElement3.SetAttribute("text", item.Name);
				xmlElement3.SetAttribute("value", item.Tag.ToString());
				xmlElement2.AppendChild(xmlElement3);
			}
			xmlElement.AppendChild(xmlElement2);
		}
		return xmlElement;
	}

	private void OnSideTabDeleted(SideTab tab)
	{
		if (this.SideTabDeleted != null)
		{
			this.SideTabDeleted(this, new SideTabEventArgs(tab));
		}
	}
}

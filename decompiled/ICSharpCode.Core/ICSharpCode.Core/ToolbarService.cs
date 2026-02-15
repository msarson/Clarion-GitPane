using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public static class ToolbarService
{
	public enum ToolbarSize
	{
		Frame,
		Pad,
		Document
	}

	private class LanguageChangeWatcher
	{
		private ToolStrip toolStrip;

		public LanguageChangeWatcher(ToolStrip toolStrip)
		{
			this.toolStrip = toolStrip;
			toolStrip.Disposed += Disposed;
			ResourceService.LanguageChanged += OnLanguageChanged;
		}

		private void OnLanguageChanged(object sender, EventArgs e)
		{
			UpdateToolbarText(toolStrip);
		}

		private void Disposed(object sender, EventArgs e)
		{
			ResourceService.LanguageChanged -= OnLanguageChanged;
		}
	}

	private static int _DocumentIconSize;

	private static int _DocumentHeight;

	private static int _FrameIconSize;

	private static int _FrameHeight;

	private static int _PadIconSize;

	private static int _PadHeight;

	public static bool UseSmallIconsInToolbar => PropertyService.Get("ICSharpCode.SharpDevelop.Gui.UseSmallIconsInToolbar", defaultValue: false);

	public static int DocumentIconSize => _DocumentIconSize;

	public static int DocumentHeight => _DocumentHeight;

	public static int MainFrameIconSize => _FrameIconSize;

	public static int MainFrameHeight => _FrameHeight;

	public static int PadIconSize => _PadIconSize;

	public static int PadHeight => _PadHeight;

	public static ToolStripItem[] CreateToolStripItems(object owner, AddInTreeNode treeNode)
	{
		List<ToolStripItem> list = new List<ToolStripItem>();
		foreach (object item in treeNode.BuildChildItems(owner))
		{
			if (item is ToolStripItem)
			{
				list.Add((ToolStripItem)item);
				continue;
			}
			ISubmenuBuilder submenuBuilder = (ISubmenuBuilder)item;
			list.AddRange(submenuBuilder.BuildSubmenu(null, owner));
		}
		return list.ToArray();
	}

	static ToolbarService()
	{
		_DocumentIconSize = 24;
		_DocumentHeight = 42;
		_FrameIconSize = 24;
		_FrameHeight = 42;
		_PadIconSize = 24;
		_PadHeight = 28;
		SetSize();
	}

	public static void GetSize(bool areSmallSize, bool areDefault, out int pDocumentIconSize, out int pDocumentHeight, out int pFrameIconSize, out int pFrameHeight, out int pPadIconSize, out int pPadHeight)
	{
		if (areSmallSize)
		{
			pFrameIconSize = 16;
			pFrameHeight = 34;
			pDocumentIconSize = 16;
			pDocumentHeight = 34;
			pPadIconSize = 16;
			pPadHeight = 34;
		}
		else
		{
			pFrameIconSize = 24;
			pFrameHeight = 42;
			pDocumentIconSize = 24;
			pDocumentHeight = 42;
			pPadIconSize = 24;
			pPadHeight = 34;
		}
		if (!areDefault)
		{
			Properties properties = PropertyService.Get("ToolbarService", new Properties());
			if (areSmallSize)
			{
				pDocumentIconSize = properties.Get("DocumentIconSize16", pDocumentIconSize);
				pDocumentHeight = properties.Get("DocumentHeight16", pDocumentHeight);
				pFrameIconSize = properties.Get("FrameIconSize16", pFrameIconSize);
				pFrameHeight = properties.Get("FrameHeight16", pFrameHeight);
				pPadIconSize = properties.Get("PadIconSize16", pPadIconSize);
				pPadHeight = properties.Get("PadHeight16", pPadHeight);
			}
			else
			{
				pDocumentIconSize = properties.Get("DocumentIconSize", pDocumentIconSize);
				pDocumentHeight = properties.Get("DocumentHeight", pDocumentHeight);
				pFrameIconSize = properties.Get("FrameIconSize", pFrameIconSize);
				pFrameHeight = properties.Get("FrameHeight", pFrameHeight);
				pPadIconSize = properties.Get("PadIconSize", pPadIconSize);
				pPadHeight = properties.Get("PadHeight", pPadHeight);
			}
		}
	}

	public static void SetSize(bool areSmallSize, int pDocumentIconSize, int pDocumentHeight, int pFrameIconSize, int pFrameHeight, int pPadIconSize, int pPadHeight)
	{
		try
		{
			Properties properties = PropertyService.Get("ToolbarService", new Properties());
			if (areSmallSize)
			{
				properties.Set("DocumentIconSize16", pDocumentIconSize);
				properties.Set("DocumentHeight16", pDocumentHeight);
				properties.Set("FrameIconSize16", pFrameIconSize);
				properties.Set("FrameHeight16", pFrameHeight);
				properties.Set("PadIconSize16", pPadIconSize);
				properties.Set("PadHeight16", pPadHeight);
			}
			else
			{
				properties.Set("DocumentIconSize", pDocumentIconSize);
				properties.Set("DocumentHeight", pDocumentHeight);
				properties.Set("FrameIconSize", pFrameIconSize);
				properties.Set("FrameHeight", pFrameHeight);
				properties.Set("PadIconSize", pPadIconSize);
				properties.Set("PadHeight", pPadHeight);
			}
			if (areSmallSize == UseSmallIconsInToolbar)
			{
				_DocumentIconSize = pDocumentIconSize;
				_DocumentHeight = pDocumentHeight;
				_FrameIconSize = pFrameIconSize;
				_FrameHeight = pFrameHeight;
				_PadIconSize = pPadIconSize;
				_PadHeight = pPadHeight;
			}
		}
		catch
		{
		}
	}

	public static void SetSize()
	{
		try
		{
			GetSize(UseSmallIconsInToolbar, areDefault: false, out _DocumentIconSize, out _DocumentHeight, out _FrameIconSize, out _FrameHeight, out _PadIconSize, out _PadHeight);
		}
		catch
		{
			SetDefaulSize(useSmall: false);
		}
	}

	private static void SetDefaulSize(bool useSmall)
	{
		GetSize(useSmall, areDefault: true, out _DocumentIconSize, out _DocumentHeight, out _FrameIconSize, out _FrameHeight, out _PadIconSize, out _PadHeight);
	}

	public static void ResetSize()
	{
		try
		{
			SetSize(areSmallSize: true, _DocumentIconSize, _DocumentHeight, _FrameIconSize, _FrameHeight, _PadIconSize, _PadHeight);
			SetSize(areSmallSize: false, _DocumentIconSize, _DocumentHeight, _FrameIconSize, _FrameHeight, _PadIconSize, _PadHeight);
		}
		catch
		{
		}
	}

	private static bool IsDerivedFrom(Type theObjectType, string baseType)
	{
		if (theObjectType.FullName == baseType)
		{
			return true;
		}
		if (theObjectType.BaseType != null)
		{
			return IsDerivedFrom(theObjectType.BaseType, baseType);
		}
		return false;
	}

	private static ToolbarSize GetToolbarSize(object owner)
	{
		if (owner == null)
		{
			return ToolbarSize.Pad;
		}
		string text = "";
		if (owner is UserControl)
		{
			UserControl userControl = (UserControl)owner;
			if (userControl.ParentForm == null)
			{
				return ToolbarSize.Pad;
			}
			text = userControl.ParentForm.GetType().ToString();
		}
		else
		{
			text = owner.GetType().ToString();
		}
		if (text == "ICSharpCode.SharpDevelop.Gui.DefaultWorkbench" || (owner is Form && ((Form)owner).IsMdiContainer))
		{
			return ToolbarSize.Frame;
		}
		Type type = owner.GetType().GetInterface("IViewContent");
		Type type2 = owner.GetType().GetInterface("IBaseViewContent");
		if (type != null || type2 != null || IsDerivedFrom(owner.GetType(), "ICSharpCode.SharpDevelop.BrowserDisplayBinding.HtmlViewPane"))
		{
			return ToolbarSize.Document;
		}
		Type type3 = owner.GetType().GetInterface("IPadContent");
		if (type3 != null || IsDerivedFrom(owner.GetType(), "ICSharpCode.SharpDevelop.Gui.AbstractPadContent") || IsDerivedFrom(owner.GetType(), "ICSharpCode.SharpDevelop.Gui.SdiWorkbenchLayout+PadContentWrapper"))
		{
			return ToolbarSize.Pad;
		}
		if (IsDerivedFrom(owner.GetType(), "WeifenLuo.WinFormsUI.DockContent") || IsDerivedFrom(owner.GetType(), "System.Windows.Forms.Form") || IsDerivedFrom(owner.GetType(), "System.Windows.Forms.ToolStripContainer"))
		{
			return ToolbarSize.Document;
		}
		return ToolbarSize.Pad;
	}

	public static void SetToolStripSize(object owner, ToolStrip[] toolStrips)
	{
		ToolbarSize toolbarSize = GetToolbarSize(owner);
		for (int i = 0; i < toolStrips.Length; i++)
		{
			SetToolStripSize(toolbarSize, toolStrips[i]);
		}
	}

	public static void SetToolStripSize(object owner, ToolStrip toolStrip)
	{
		SetToolStripSize(GetToolbarSize(owner), toolStrip);
	}

	public static void SetToolStripSize(ToolbarSize size, ToolStrip toolStrip)
	{
		if (toolStrip != null)
		{
			toolStrip.GripStyle = ToolStripGripStyle.Hidden;
			toolStrip.Margin = new Padding(0, 0, 0, 3);
			toolStrip.Padding = new Padding(2, 2, 1, 3);
			switch (size)
			{
			case ToolbarSize.Frame:
				toolStrip.AutoSize = false;
				toolStrip.ImageScalingSize = new Size(MainFrameIconSize, MainFrameIconSize);
				toolStrip.Size = new Size(282, MainFrameHeight);
				toolStrip.Height = MainFrameHeight;
				break;
			case ToolbarSize.Pad:
				toolStrip.AutoSize = false;
				toolStrip.ImageScalingSize = new Size(PadIconSize, PadIconSize);
				toolStrip.Size = new Size(282, PadHeight);
				toolStrip.Height = PadHeight;
				break;
			case ToolbarSize.Document:
				toolStrip.AutoSize = false;
				toolStrip.ImageScalingSize = new Size(DocumentIconSize, DocumentIconSize);
				toolStrip.Size = new Size(282, DocumentHeight);
				toolStrip.Height = DocumentHeight;
				break;
			default:
				toolStrip.AutoSize = false;
				toolStrip.ImageScalingSize = new Size(PadIconSize, PadIconSize);
				toolStrip.Size = new Size(282, PadHeight);
				toolStrip.Height = PadHeight;
				break;
			}
		}
	}

	public static ToolStrip CreateToolStrip(object owner, AddInTreeNode treeNode, params AddInTreeNode[] treeNodes)
	{
		ToolStrip toolStrip = new ToolStrip();
		SetToolStripSize(owner, toolStrip);
		if (treeNode != null)
		{
			if (treeNode.Codons.Count > 0)
			{
				toolStrip.Items.AddRange(CreateToolStripItems(owner, treeNode));
			}
			else if (treeNode.ChildNodes.Count > 0)
			{
				foreach (AddInTreeNode value in treeNode.ChildNodes.Values)
				{
					if (toolStrip.Items.Count > 0)
					{
						toolStrip.Items.Add(new ToolStripSeparator());
					}
					toolStrip.Items.AddRange(CreateToolStripItems(owner, value));
				}
			}
		}
		if (treeNodes != null && treeNodes.Length > 0)
		{
			foreach (AddInTreeNode treeNode2 in treeNodes)
			{
				toolStrip.Items.AddRange(CreateToolStripItems(owner, treeNode2));
			}
		}
		foreach (ToolStripItem item in toolStrip.Items)
		{
			item.Overflow = ToolStripItemOverflow.AsNeeded;
		}
		UpdateToolbar(toolStrip);
		new LanguageChangeWatcher(toolStrip);
		return toolStrip;
	}

	public static void ShowItem(ToolStrip tools, string itemName)
	{
		VisibleChangeItem(tools, itemName, visible: true);
	}

	public static void HideItem(ToolStrip tools, string itemName)
	{
		VisibleChangeItem(tools, itemName, visible: false);
	}

	private static void VisibleChangeItem(ToolStrip tools, string itemName, bool visible)
	{
		ToolStripItem itemReturned = null;
		if (TryGetItem(tools, itemName, ref itemReturned))
		{
			itemReturned.Visible = visible;
		}
	}

	public static bool TryGetItem(ToolStrip tools, string itemName, ref ToolStripItem itemReturned)
	{
		if (tools.Items.ContainsKey(itemName))
		{
			itemReturned = tools.Items[itemName];
			return true;
		}
		foreach (ToolStripItem item in tools.Items)
		{
			if (item is IStatusUpdate && ((IStatusUpdate)item).CodonId.Equals(itemName, StringComparison.OrdinalIgnoreCase))
			{
				itemReturned = item;
				return true;
			}
		}
		LoggingService.WarnFormatted("Toolbar ({0}) does not contain item ({1})", tools.Name, itemName);
		return false;
	}

	public static ToolStrip CreateToolStrip(object owner, string addInTreePath, params string[] addInTreePaths)
	{
		List<AddInTreeNode> list = new List<AddInTreeNode>();
		if (addInTreePaths != null && addInTreePaths.Length > 0)
		{
			foreach (string path in addInTreePaths)
			{
				list.Add(AddInTree.GetTreeNode(path));
			}
		}
		return CreateToolStrip(owner, AddInTree.GetTreeNode(addInTreePath), list.ToArray());
	}

	private static List<ToolStrip> CreateToolbar(object owner, string addInTreePath)
	{
		AddInTreeNode treeNode;
		try
		{
			treeNode = AddInTree.GetTreeNode(addInTreePath);
		}
		catch (TreePathNotFoundException)
		{
			return null;
		}
		List<ToolStrip> list = new List<ToolStrip>();
		foreach (AddInTreeNode value in treeNode.ChildNodes.Values)
		{
			list.Add(CreateToolStrip(owner, value));
		}
		return list;
	}

	public static ToolStrip[] CreateToolbars(object owner, string addInTreePath, params string[] addInTreePaths)
	{
		List<ToolStrip> list = new List<ToolStrip>();
		try
		{
			list.AddRange(CreateToolbar(owner, addInTreePath));
			if (addInTreePaths != null && addInTreePaths.Length > 0)
			{
				foreach (string addInTreePath2 in addInTreePaths)
				{
					list.AddRange(CreateToolbar(owner, addInTreePath2));
				}
			}
		}
		catch
		{
		}
		return list.ToArray();
	}

	public static void UpdateToolbar(ToolStrip toolStrip)
	{
		toolStrip.SuspendLayout();
		foreach (ToolStripItem item in toolStrip.Items)
		{
			if (item is IStatusUpdate)
			{
				((IStatusUpdate)item).UpdateStatus();
			}
		}
		toolStrip.ResumeLayout();
	}

	public static void UpdateToolbarText(ToolStrip toolStrip)
	{
		toolStrip.SuspendLayout();
		foreach (ToolStripItem item in toolStrip.Items)
		{
			if (item is IStatusUpdate)
			{
				((IStatusUpdate)item).UpdateText();
			}
		}
		toolStrip.ResumeLayout();
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

namespace ICSharpCode.SharpDevelop.Gui;

public class ExtTreeView : TreeView
{
	private static Color _BackgroundColor;

	private static Color _TextColor;

	private static Color _BarActiveBackgroundColor;

	private static Color _BarActiveTextColor;

	private static Color _BarInactiveBackgroundColor;

	private static Color _BarInactiveTextColor;

	private static SolidBrush _BackgroundBrush;

	private static SolidBrush _TextBrush;

	private static SolidBrush _BarActiveBackgroundBrush;

	private static SolidBrush _BarActiveTextBrush;

	private static SolidBrush _BarInactiveBackgroundBrush;

	private static SolidBrush _BarInactiveTextBrush;

	public static readonly Bitmap expandImgPlus;

	public static readonly Bitmap expandImgMinus;

	public static readonly int expandImgWidth;

	private Dictionary<string, int> imageIndexTable = new Dictionary<string, int>();

	private List<ExtTreeNode> cutNodes = new List<ExtTreeNode>();

	private bool isSorted = true;

	private bool _FullNodeRowSelect = true;

	private bool _OwnerDraw = true;

	private IComparer<TreeNode> nodeSorter = new ExtTreeViewComparer();

	private bool inRefresh;

	private bool activateItemOnEnterKeyPress = true;

	private bool activateItemOnDoubleClick = true;

	private bool canClearSelection = true;

	private int mouseClickNum;

	private bool activateItemOnExpand = true;

	public static Color BackgroundColor => _BackgroundColor;

	public static Color TextColor => _TextColor;

	public static Color BarActiveBackgroundColor => _BarActiveBackgroundColor;

	public static Color BarActiveTextColor => _BarActiveTextColor;

	public static Color BarInactiveBackgroundColor => _BarInactiveBackgroundColor;

	public static Color BarInactiveTextColor => _BarInactiveTextColor;

	public static SolidBrush BackgroundBrush => _BackgroundBrush;

	public static SolidBrush TextBrush => _TextBrush;

	public static SolidBrush BarActiveBackgroundBrush => _BarActiveBackgroundBrush;

	public static SolidBrush BarActiveTextBrush => _BarActiveTextBrush;

	public static SolidBrush BarInactiveBackgroundBrush => _BarInactiveBackgroundBrush;

	public static SolidBrush BarInactiveTextBrush => _BarInactiveTextBrush;

	public bool IsSorted
	{
		get
		{
			return isSorted;
		}
		set
		{
			isSorted = value;
		}
	}

	public bool FullNodeRowSelect
	{
		get
		{
			return _FullNodeRowSelect;
		}
		set
		{
			_FullNodeRowSelect = value;
		}
	}

	[Obsolete("Use IsSorted instead!")]
	public new bool Sorted
	{
		get
		{
			return base.Sorted;
		}
		set
		{
			base.Sorted = value;
		}
	}

	public bool OwnerDraw
	{
		get
		{
			return _OwnerDraw;
		}
		set
		{
			if (_OwnerDraw != value)
			{
				if (value)
				{
					base.DrawMode = TreeViewDrawMode.OwnerDrawText;
				}
				else
				{
					base.DrawMode = TreeViewDrawMode.Normal;
				}
			}
			_OwnerDraw = value;
		}
	}

	public List<ExtTreeNode> CutNodes => cutNodes;

	public IComparer<TreeNode> NodeSorter
	{
		get
		{
			return nodeSorter;
		}
		set
		{
			nodeSorter = value;
		}
	}

	[Obsolete("Use NodeSorter instead!")]
	public new IComparer TreeViewNodeSorter
	{
		get
		{
			return base.TreeViewNodeSorter;
		}
		set
		{
			base.TreeViewNodeSorter = value;
		}
	}

	public bool ActivateItemOnEnterKeyPress
	{
		get
		{
			return activateItemOnEnterKeyPress;
		}
		set
		{
			activateItemOnEnterKeyPress = value;
		}
	}

	public bool ActivateItemOnDoubleClick
	{
		get
		{
			return activateItemOnDoubleClick;
		}
		set
		{
			activateItemOnDoubleClick = value;
		}
	}

	public bool CanClearSelection
	{
		get
		{
			return canClearSelection;
		}
		set
		{
			canClearSelection = value;
		}
	}

	public bool ActivateItemOnExpand
	{
		get
		{
			return activateItemOnExpand;
		}
		set
		{
			activateItemOnExpand = value;
		}
	}

	static ExtTreeView()
	{
		expandImgPlus = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("Resources.TreeOpen.png"));
		expandImgMinus = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("Resources.TreeClose.png"));
		expandImgWidth = expandImgPlus.Width;
		SetColorTable();
	}

	public static void SetColorTable()
	{
		if (ToolStripManager.Renderer is ToolStripProfessionalRenderer)
		{
			ToolStripProfessionalRenderer toolStripProfessionalRenderer = ToolStripManager.Renderer as ToolStripProfessionalRenderer;
			if (toolStripProfessionalRenderer.ColorTable is IListCustomColor)
			{
				SetColors((IListCustomColor)toolStripProfessionalRenderer.ColorTable);
			}
			else
			{
				SetColorsSystemColor();
			}
		}
		else
		{
			SetColorsSystemColor();
		}
	}

	private static void SetColorsSystemColor()
	{
		SetColors(SystemColors.Window, SystemColors.WindowText, SystemColors.Highlight, SystemColors.HighlightText, SystemColors.InactiveCaption, SystemColors.InactiveCaptionText);
	}

	private static void SetColors(ProfessionalColorTable colors)
	{
		SetColorsSystemColor();
	}

	private static void SetColors(IListCustomColor colors)
	{
		SetColors(colors.Background, colors.Text, colors.BarActiveBackground, colors.BarActiveText, colors.BarInactiveBackground, colors.BarInactiveText);
	}

	public static void SetColors(Color backgroundColor, Color TextColor, Color barActiveBackgroundColor, Color barActiveTextColor, Color barInactiveBackgroundColor, Color barInactiveTextColor)
	{
		_BackgroundColor = backgroundColor;
		_TextColor = TextColor;
		_BarActiveBackgroundColor = barActiveBackgroundColor;
		_BarActiveTextColor = barActiveTextColor;
		_BarInactiveBackgroundColor = barInactiveBackgroundColor;
		_BarInactiveTextColor = barInactiveTextColor;
		_BackgroundBrush = new SolidBrush(backgroundColor);
		_TextBrush = new SolidBrush(TextColor);
		_BarActiveBackgroundBrush = new SolidBrush(barActiveBackgroundColor);
		_BarActiveTextBrush = new SolidBrush(barActiveTextColor);
		_BarInactiveBackgroundBrush = new SolidBrush(barInactiveBackgroundColor);
		_BarInactiveTextBrush = new SolidBrush(barInactiveTextColor);
	}

	public ExtTreeView()
	{
		base.DrawMode = TreeViewDrawMode.OwnerDrawText;
		base.HideSelection = false;
		AllowDrop = true;
		base.FullRowSelect = false;
		FullNodeRowSelect = true;
		base.ImageList = new ImageList
		{
			ImageSize = new Size(16, 16),
			ColorDepth = ColorDepth.Depth32Bit
		};
		Font = FontService.GetFont(FontService.FontType.ListControls);
		BackColor = BackgroundColor;
		ForeColor = _TextColor;
	}

	public new void Sort()
	{
		SortNodes(base.Nodes, recursive: true);
	}

	public void SortNodes(TreeNodeCollection nodes, bool recursive)
	{
		if (!isSorted || nodes.Count == 0 || (nodes.Count == 1 && !recursive))
		{
			return;
		}
		TreeNode[] array = new TreeNode[nodes.Count];
		nodes.CopyTo(array, 0);
		Array.Sort(array, nodeSorter);
		nodes.Clear();
		nodes.AddRange(array);
		if (recursive)
		{
			TreeNode[] array2 = array;
			foreach (TreeNode treeNode in array2)
			{
				SortNodes(treeNode.Nodes, recursive: true);
			}
		}
	}

	public void ClearCutNodes()
	{
		foreach (ExtTreeNode cutNode in CutNodes)
		{
			cutNode.DoPerformCut = false;
		}
		CutNodes.Clear();
	}

	public new void ExpandAll()
	{
		if (base.Nodes == null || base.Nodes.Count <= 0)
		{
			return;
		}
		SuspendLayout();
		Stack<TreeNode> stack = new Stack<TreeNode>();
		foreach (TreeNode node in base.Nodes)
		{
			stack.Push(node);
		}
		while (stack.Count > 0)
		{
			TreeNode treeNode = stack.Pop();
			treeNode.Expand();
			foreach (TreeNode node2 in treeNode.Nodes)
			{
				stack.Push(node2);
			}
		}
		ResumeLayout();
	}

	public new void CollapseAll()
	{
		if (base.Nodes == null || base.Nodes.Count <= 0)
		{
			return;
		}
		SuspendLayout();
		bool flag = false;
		if (base.Nodes.Count == 1)
		{
			flag = true;
		}
		Stack<TreeNode> stack = new Stack<TreeNode>();
		foreach (TreeNode node in base.Nodes)
		{
			stack.Push(node);
		}
		while (stack.Count > 0)
		{
			TreeNode treeNode = stack.Pop();
			if (flag)
			{
				flag = false;
			}
			else
			{
				treeNode.Collapse();
			}
			foreach (TreeNode node2 in treeNode.Nodes)
			{
				stack.Push(node2);
			}
		}
		ResumeLayout();
	}

	public void Clear()
	{
		if (base.IsDisposed)
		{
			return;
		}
		base.SelectedNode = null;
		TreeNode[] array = new TreeNode[base.Nodes.Count];
		base.Nodes.CopyTo(array, 0);
		base.Nodes.Clear();
		TreeNode[] array2 = array;
		foreach (TreeNode treeNode in array2)
		{
			if (treeNode is IDisposable)
			{
				((IDisposable)treeNode).Dispose();
			}
		}
	}

	public void StartLabelEdit(ExtTreeNode node)
	{
		if (node != null && node.CanLabelEdit)
		{
			node.EnsureVisible();
			base.SelectedNode = node;
			base.LabelEdit = true;
			node.BeforeLabelEdit();
			node.BeginEdit();
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		bool flag = false;
		TreeNode treeNode = null;
		if (base.SelectedNode == null || !base.SelectedNode.IsEditing)
		{
			switch (keyData)
			{
			case Keys.F2:
				StartLabelEdit(base.SelectedNode as ExtTreeNode);
				break;
			case Keys.Delete:
				if (base.SelectedNode != null)
				{
					treeNode = base.SelectedNode.PrevNode;
					if (treeNode != null && treeNode.Parent != base.SelectedNode.Parent)
					{
						treeNode = null;
					}
				}
				DeleteNode(base.SelectedNode as ExtTreeNode);
				break;
			}
		}
		flag = base.ProcessCmdKey(ref msg, keyData);
		if (treeNode != null)
		{
			TreeNode[] array = base.Nodes.Find(treeNode.Text, searchAllChildren: true);
			if (array.Length > 0)
			{
				base.SelectedNode = array[0];
			}
		}
		return flag;
	}

	protected override void OnAfterLabelEdit(NodeLabelEditEventArgs e)
	{
		string text = e.Node.Text;
		base.OnAfterLabelEdit(e);
		base.LabelEdit = false;
		e.CancelEdit = true;
		if (e.Node is ExtTreeNode extTreeNode)
		{
			extTreeNode.AfterLabelEdit(e.Label ?? extTreeNode.Text);
		}
		if (text != e.Node.Text)
		{
			SortParentNodes(e.Node);
			base.SelectedNode = e.Node;
		}
	}

	private void SortParentNodes(TreeNode treeNode)
	{
		TreeNode treeNode2 = treeNode.Parent;
		SortNodes((treeNode2 == null) ? base.Nodes : treeNode2.Nodes, recursive: false);
	}

	protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
	{
		if (mouseClickNum == 2)
		{
			mouseClickNum = 0;
			e.Cancel = true;
			return;
		}
		base.OnBeforeExpand(e);
		if (e.Node == null)
		{
			return;
		}
		try
		{
			if (e.Node is ExtTreeNode)
			{
				if (!((ExtTreeNode)e.Node).IsInitialized && !inRefresh)
				{
					inRefresh = true;
					BeginUpdate();
				}
				((ExtTreeNode)e.Node).Expanding();
			}
			if (inRefresh)
			{
				SortNodes(e.Node.Nodes, recursive: false);
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
		if (e.Node.Nodes.Count == 0 && inRefresh)
		{
			inRefresh = false;
			EndUpdate();
		}
	}

	protected override void OnAfterExpand(TreeViewEventArgs e)
	{
		base.OnAfterExpand(e);
		if (inRefresh)
		{
			inRefresh = false;
			EndUpdate();
		}
	}

	protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
	{
		if (mouseClickNum == 2)
		{
			mouseClickNum = 0;
			e.Cancel = true;
			return;
		}
		base.OnBeforeCollapse(e);
		if (e.Node is ExtTreeNode)
		{
			((ExtTreeNode)e.Node).Collapsing();
		}
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		base.OnKeyPress(e);
		if (e.KeyChar == '\r' && ActivateItemOnEnterKeyPress)
		{
			if (base.SelectedNode is ExtTreeNode extTreeNode)
			{
				extTreeNode.ActivateItem();
			}
			e.Handled = true;
		}
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		base.OnMouseDoubleClick(e);
		if (ActivateItemOnDoubleClick && GetNodeAt(e.Location) is ExtTreeNode extTreeNode)
		{
			extTreeNode.ActivateItem();
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		mouseClickNum = e.Clicks;
		base.OnMouseDown(e);
		TreeNode nodeAt = GetNodeAt(e.X, e.Y);
		if (nodeAt != null)
		{
			if (nodeAt.Nodes.Count > 0 && base.ShowPlusMinus && !ActivateItemOnExpand)
			{
				TreeViewHitTestInfo treeViewHitTestInfo = HitTest(e.Location);
				if (treeViewHitTestInfo.Location == TreeViewHitTestLocations.PlusMinus)
				{
					return;
				}
			}
			if (base.SelectedNode != nodeAt)
			{
				base.SelectedNode = nodeAt;
			}
		}
		else if (canClearSelection)
		{
			base.SelectedNode = null;
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		mouseClickNum = 0;
		base.OnMouseUp(e);
	}

	protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
	{
		base.OnBeforeSelect(e);
		if (e.Node is ExtTreeNode extTreeNode)
		{
			extTreeNode.ContextMenuStrip = MenuService.CreateContextMenu(e.Node, extTreeNode.ContextmenuAddinTreePath);
		}
	}

	protected override void OnAfterCheck(TreeViewEventArgs e)
	{
		base.OnAfterCheck(e);
		if (e.Node is ExtTreeNode extTreeNode)
		{
			extTreeNode.CheckedChanged();
		}
	}

	protected override void OnDrawNode(DrawTreeNodeEventArgs e)
	{
		if (!inRefresh)
		{
			if (e.Bounds.Location.X > 0 || e.Bounds.Location.Y > 0)
			{
				if (e.Node is ExtTreeNode extTreeNode && (!extTreeNode.DrawDefault || FullNodeRowSelect))
				{
					extTreeNode.Draw(e);
					e.DrawDefault = false;
				}
				else if ((e.State & (TreeNodeStates.Focused | TreeNodeStates.Selected)) == TreeNodeStates.Selected)
				{
					e.Graphics.FillRectangle(BarInactiveBackgroundBrush, e.Bounds);
					e.Graphics.DrawString(e.Node.Text, Font, BarInactiveBackgroundBrush, e.Bounds.Location);
					e.DrawDefault = false;
				}
				else
				{
					e.DrawDefault = true;
				}
			}
		}
		else
		{
			e.DrawDefault = false;
		}
		base.OnDrawNode(e);
	}

	protected override void OnItemDrag(ItemDragEventArgs e)
	{
		base.OnItemDrag(e);
		if (e.Item is ExtTreeNode { DragDropDataObject: { } dragDropDataObject } extTreeNode)
		{
			DoDragDrop(dragDropDataObject, DragDropEffects.All);
			SortParentNodes(extTreeNode);
		}
	}

	protected override void OnDragEnter(DragEventArgs e)
	{
		base.OnDragEnter(e);
		e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
	}

	protected override void OnDragOver(DragEventArgs e)
	{
		base.OnDragOver(e);
		Point pt = PointToClient(new Point(e.X, e.Y));
		if (GetNodeAt(pt) is ExtTreeNode extTreeNode)
		{
			DragDropEffects dragDropEffects = DragDropEffects.None;
			e.Effect = extTreeNode.GetDragDropEffect(proposedEffect: ((e.KeyState & 8) > 0) ? DragDropEffects.Copy : DragDropEffects.Move, dataObject: e.Data);
			if (e.Effect != DragDropEffects.None)
			{
				base.SelectedNode = extTreeNode;
			}
		}
	}

	protected override void OnDragDrop(DragEventArgs e)
	{
		base.OnDragDrop(e);
		Point pt = PointToClient(new Point(e.X, e.Y));
		if (GetNodeAt(pt) is ExtTreeNode extTreeNode)
		{
			extTreeNode.DoDragDrop(e.Data, e.Effect);
			SortParentNodes(extTreeNode);
		}
	}

	public int GetImageIndexForImage(string image, bool performCutBitmap)
	{
		string key = (performCutBitmap ? (image + "_ghost") : image);
		if (!imageIndexTable.ContainsKey(key))
		{
			base.ImageList.Images.Add(performCutBitmap ? IconService.GetGhostBitmap(image) : IconService.GetBitmap(image));
			imageIndexTable[key] = base.ImageList.Images.Count - 1;
			return base.ImageList.Images.Count - 1;
		}
		return imageIndexTable[key];
	}

	private void DeleteNode(ExtTreeNode node)
	{
		if (node != null && node.EnableDelete)
		{
			node.EnsureVisible();
			base.SelectedNode = node;
			node.Delete();
		}
	}

	public static string GetViewStateString(TreeView treeView)
	{
		if (treeView.Nodes.Count == 0)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		WriteViewStateString(stringBuilder, treeView.Nodes[0]);
		return stringBuilder.ToString();
	}

	private static void WriteViewStateString(StringBuilder b, TreeNode node)
	{
		b.Append('[');
		foreach (TreeNode node2 in node.Nodes)
		{
			if (node2.IsExpanded && node2.Text.IndexOf('[') < 0)
			{
				b.Append(node2.Text);
				WriteViewStateString(b, node2);
			}
		}
		b.Append(']');
	}

	public static void ApplyViewStateString(string viewState, TreeView treeView)
	{
		if (viewState.Length != 0)
		{
			int pos = 0;
			ApplyViewStateString(treeView.Nodes[0], viewState, ref pos);
		}
	}

	private static void ApplyViewStateString(TreeNode node, string viewState, ref int pos)
	{
		if (viewState[pos++] != '[')
		{
			throw new ArgumentException("pos must point to '['");
		}
		while (viewState[pos] != ']')
		{
			StringBuilder stringBuilder = new StringBuilder();
			char value;
			while ((value = viewState[pos++]) != '[')
			{
				stringBuilder.Append(value);
			}
			pos--;
			string text = stringBuilder.ToString();
			TreeNode treeNode = null;
			if (node != null)
			{
				foreach (TreeNode node2 in node.Nodes)
				{
					if (node2.Text == text)
					{
						treeNode = node2;
						break;
					}
				}
			}
			treeNode?.Expand();
			ApplyViewStateString(treeNode, viewState, ref pos);
			pos++;
		}
	}
}

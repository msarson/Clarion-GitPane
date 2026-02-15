using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ExtTreeNode : TreeNode, IDisposable, IClipboardHandler
{
	private const TreeNodeStates SelectedAndFocused = TreeNodeStates.Focused | TreeNodeStates.Selected;

	private string contextmenuAddinTreePath;

	protected bool isInitialized;

	private string image;

	private TreeNode internalParent;

	protected bool canLabelEdit;

	protected List<ExtTreeNode> invisibleNodes = new List<ExtTreeNode>();

	private bool isDisposed;

	protected bool drawDefault = true;

	private static Font regularBigFont;

	private static Font boldBigFont;

	private static Font italicBigFont;

	private static Font boldMonospacedFont;

	private static Font italicMonospacedFont;

	private static Font boldDefaultFont;

	private static Font italicDefaultFont;

	private bool doPerformCut;

	protected int sortOrder;

	public bool IsInitialized => isInitialized;

	public virtual string ContextmenuAddinTreePath
	{
		get
		{
			return contextmenuAddinTreePath;
		}
		set
		{
			contextmenuAddinTreePath = value;
		}
	}

	public ExtTreeView TreeViewExt => base.TreeView as ExtTreeView;

	public new TreeNode Parent => internalParent;

	public virtual bool CanLabelEdit => canLabelEdit;

	public virtual bool Visible => true;

	public IEnumerable<ExtTreeNode> AllNodes
	{
		get
		{
			foreach (ExtTreeNode node in base.Nodes)
			{
				yield return node;
			}
			foreach (ExtTreeNode invisibleNode in invisibleNodes)
			{
				yield return invisibleNode;
			}
		}
	}

	public bool IsDisposed => isDisposed;

	public bool DrawDefault => drawDefault;

	public static Font RegularMonospacedFont => ResourceService.DefaultMonospacedFont;

	public static Font BoldMonospacedFont => boldMonospacedFont ?? (boldMonospacedFont = ResourceService.LoadDefaultMonospacedFont(FontStyle.Bold));

	public static Font ItalicMonospacedFont => italicMonospacedFont ?? (italicMonospacedFont = ResourceService.LoadDefaultMonospacedFont(FontStyle.Italic));

	public static Font RegularDefaultFont => FontService.GetFont(FontService.FontType.ListControls);

	public static Font BoldDefaultFont => boldDefaultFont ?? (boldDefaultFont = ResourceService.LoadFont(RegularDefaultFont, FontStyle.Bold));

	public static Font ItalicDefaultFont => italicDefaultFont ?? (italicDefaultFont = ResourceService.LoadFont(RegularDefaultFont, FontStyle.Italic));

	public static Font RegularBigFont => regularBigFont ?? (regularBigFont = ResourceService.LoadFont(RegularDefaultFont.Name, (int)RegularDefaultFont.Size + 2));

	public static Font BoldBigFont => boldBigFont ?? (boldBigFont = ResourceService.LoadFont(RegularDefaultFont.Name, (int)RegularDefaultFont.Size + 2, FontStyle.Bold));

	public static Font ItalicBigFont => italicBigFont ?? (italicBigFont = ResourceService.LoadFont(RegularDefaultFont.Name, (int)RegularDefaultFont.Size + 2, FontStyle.Italic));

	public virtual DataObject DragDropDataObject => null;

	public virtual bool DoPerformCut
	{
		get
		{
			if (Parent is ExtTreeNode extTreeNode)
			{
				return doPerformCut | extTreeNode.DoPerformCut;
			}
			return doPerformCut;
		}
		set
		{
			doPerformCut = value;
			if (doPerformCut)
			{
				((ExtTreeView)base.TreeView).CutNodes.Add(this);
			}
			Refresh();
		}
	}

	public virtual bool EnableCut => false;

	public virtual bool EnableCopy => false;

	public virtual bool EnablePaste => false;

	public virtual bool EnableDelete => false;

	public virtual bool EnableSelectAll => false;

	public virtual int SortOrder => sortOrder;

	public virtual string CompareString => base.Text;

	public ExtTreeNode(string text)
		: base(text)
	{
	}

	public ExtTreeNode()
	{
	}

	public ExtTreeNode(string text, TreeNode[] children)
		: base(text, children)
	{
	}

	public void SetIcon(string iconName)
	{
		if (iconName == null)
		{
			return;
		}
		image = iconName;
		ExtTreeView treeViewExt = TreeViewExt;
		if (treeViewExt != null)
		{
			int imageIndexForImage = treeViewExt.GetImageIndexForImage(iconName, DoPerformCut);
			if (base.ImageIndex != imageIndexForImage)
			{
				int imageIndex = (base.SelectedImageIndex = imageIndexForImage);
				base.ImageIndex = imageIndex;
			}
		}
	}

	public void AddTo(TreeNode node)
	{
		internalParent = node;
		AddTo(node.Nodes);
	}

	public void AddTo(TreeView view)
	{
		internalParent = null;
		AddTo(view.Nodes);
	}

	public void Insert(int index, TreeNode parentNode)
	{
		internalParent = parentNode;
		parentNode.Nodes.Insert(index, this);
	}

	public void Insert(int index, TreeView view)
	{
		internalParent = null;
		view.Nodes.Insert(index, this);
	}

	private void AddTo(TreeNodeCollection nodes)
	{
		nodes.Add(this);
		Refresh();
	}

	protected virtual void Initialize()
	{
	}

	public void PerformInitialization()
	{
		if (!isInitialized)
		{
			Initialize();
			isInitialized = true;
		}
	}

	public virtual void Expanding()
	{
		PerformInitialization();
	}

	public virtual void Collapsing()
	{
	}

	public virtual void ActivateItem()
	{
		Toggle();
	}

	public virtual void CheckedChanged()
	{
	}

	public virtual void Refresh()
	{
		SetIcon(image);
		foreach (TreeNode node in base.Nodes)
		{
			if (node is ExtTreeNode)
			{
				((ExtTreeNode)node).Refresh();
			}
		}
	}

	public virtual void BeforeLabelEdit()
	{
	}

	public virtual void AfterLabelEdit(string newName)
	{
		throw new NotImplementedException();
	}

	public virtual void UpdateVisibility()
	{
		int num = 0;
		while (num < invisibleNodes.Count)
		{
			if (invisibleNodes[num].Visible)
			{
				invisibleNodes[num].AddTo(this);
				invisibleNodes.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		foreach (TreeNode node in base.Nodes)
		{
			if (node is ExtTreeNode)
			{
				ExtTreeNode extTreeNode = (ExtTreeNode)node;
				if (!extTreeNode.Visible)
				{
					invisibleNodes.Add(extTreeNode);
				}
			}
		}
		foreach (ExtTreeNode invisibleNode in invisibleNodes)
		{
			base.Nodes.Remove(invisibleNode);
		}
		foreach (TreeNode node2 in base.Nodes)
		{
			if (node2 is ExtTreeNode)
			{
				((ExtTreeNode)node2).UpdateVisibility();
			}
		}
	}

	public virtual void Dispose()
	{
		isDisposed = true;
		foreach (TreeNode node in base.Nodes)
		{
			if (node is IDisposable)
			{
				((ExtTreeNode)node).Dispose();
			}
		}
	}

	public virtual void DrawBackground(DrawTreeNodeEventArgs e)
	{
		Graphics graphics = e.Graphics;
		Rectangle rect;
		if (TreeViewExt.FullNodeRowSelect)
		{
			rect = new Rectangle(0, e.Bounds.Y, base.TreeView.Width - 2, e.Bounds.Height);
		}
		else
		{
			int width = MeasureItemWidth(e);
			rect = new Rectangle(e.Bounds.X, e.Bounds.Y, width, e.Bounds.Height);
		}
		if ((e.State & (TreeNodeStates.Focused | TreeNodeStates.Selected)) == TreeNodeStates.Selected)
		{
			graphics.FillRectangle(ExtTreeView.BarInactiveBackgroundBrush, rect);
		}
		else if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
		{
			graphics.FillRectangle(ExtTreeView.BarActiveBackgroundBrush, rect);
		}
		else
		{
			graphics.FillRectangle(ExtTreeView.BackgroundBrush, rect);
		}
		if ((e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused)
		{
			rect.Width--;
			rect.Height--;
			using Pen pen = new Pen(ExtTreeView.TextColor);
			pen.DashStyle = DashStyle.Dot;
			graphics.DrawRectangle(pen, rect);
			Color barActiveTextColor = ExtTreeView.BarActiveTextColor;
			pen.Color = Color.FromArgb(255 - barActiveTextColor.R, 255 - barActiveTextColor.G, 255 - barActiveTextColor.B);
			pen.DashOffset = 1f;
			graphics.DrawRectangle(pen, rect);
		}
	}

	protected virtual int MeasureItemWidth(DrawTreeNodeEventArgs e)
	{
		return MeasureTextWidth(e.Graphics, base.Text, base.TreeView.Font);
	}

	protected virtual void DrawForeground(DrawTreeNodeEventArgs e, float x)
	{
		if (e.Node != null)
		{
			DrawForegroundExpandImg(e, ref x);
			DrawForegroundIcon(e, ref x);
			DrawForegroundText(e, ref x);
		}
	}

	protected void DrawForegroundExpandImg(DrawTreeNodeEventArgs e, ref float x)
	{
		if (e.Node != null && e.Node.TreeView.ShowPlusMinus)
		{
			Image image = null;
			if (e.Node.Nodes.Count > 0)
			{
				image = ((!e.Node.IsExpanded) ? ExtTreeView.expandImgMinus : ExtTreeView.expandImgPlus);
			}
			if (image != null)
			{
				int num = e.Node.Bounds.Y + (e.Node.Bounds.Height - image.Height) / 2;
				e.Graphics.DrawImage(image, x, num);
			}
			x += ExtTreeView.expandImgWidth + 2;
		}
	}

	protected void DrawForegroundText(DrawTreeNodeEventArgs e, ref float x)
	{
		if (e.Node != null && !string.IsNullOrEmpty(e.Node.Text))
		{
			DrawText(e, e.Node.Text, ExtTreeView.TextBrush, RegularDefaultFont, ref x);
		}
	}

	protected void DrawForegroundIcon(DrawTreeNodeEventArgs e, Image icon, ref float x)
	{
		if (icon != null)
		{
			int num = e.Node.Bounds.Y + (e.Node.Bounds.Height - base.TreeView.ImageList.ImageSize.Height) / 2;
			e.Graphics.DrawImage(icon, x, num, icon.Width, icon.Height);
			x += icon.Width + 2;
		}
	}

	protected void DrawForegroundIcon(DrawTreeNodeEventArgs e, ref float x)
	{
		if (e.Node == null || TreeViewExt.ImageList == null || TreeViewExt.ImageList.Images == null || TreeViewExt.ImageList.Images.Count <= 0)
		{
			return;
		}
		int y = e.Node.Bounds.Y + (e.Node.Bounds.Height - base.TreeView.ImageList.ImageSize.Height) / 2;
		if (!string.IsNullOrEmpty(image))
		{
			base.TreeView.ImageList.Draw(e.Graphics, (int)x, y, TreeViewExt.GetImageIndexForImage(image, performCutBitmap: false));
		}
		else
		{
			int num = (((e.State & TreeNodeStates.Selected) != TreeNodeStates.Selected) ? e.Node.ImageIndex : e.Node.SelectedImageIndex);
			if (num < 0 || num >= TreeViewExt.ImageList.Images.Count)
			{
				num = 0;
			}
			base.TreeView.ImageList.Draw(e.Graphics, (int)x, y, num);
		}
		x += base.TreeView.ImageList.ImageSize.Width + 2;
	}

	public void Draw(DrawTreeNodeEventArgs e)
	{
		DrawBackground(e);
		float x = e.Bounds.X - base.TreeView.Indent * 2;
		DrawForeground(e, x);
	}

	protected int MeasureTextWidth(Graphics g, string text, Font font)
	{
		return (int)g.MeasureString(text, font).Width;
	}

	protected void DrawText(DrawTreeNodeEventArgs e, string text, Brush brush, Font font)
	{
		float x = e.Bounds.X;
		DrawText(e, text, brush, font, ref x);
	}

	protected void DrawText(DrawTreeNodeEventArgs e, string text, Brush brush, Font font, ref float x)
	{
		if ((e.State & (TreeNodeStates.Focused | TreeNodeStates.Selected)) == (TreeNodeStates.Focused | TreeNodeStates.Selected))
		{
			TextRenderer.DrawText(e.Graphics, text, font, new Point((int)x, e.Bounds.Y), ExtTreeView.BarActiveTextColor);
		}
		else if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
		{
			TextRenderer.DrawText(e.Graphics, text, font, new Point((int)x, e.Bounds.Y), ExtTreeView.BarInactiveTextColor);
		}
		else
		{
			TextRenderer.DrawText(e.Graphics, text, font, new Point((int)x, e.Bounds.Y), ExtTreeView.TextColor);
		}
		x += e.Graphics.MeasureString(text, font).Width;
	}

	protected Color GetTextColor(TreeNodeStates state, Color c)
	{
		if ((state & (TreeNodeStates.Focused | TreeNodeStates.Selected)) == (TreeNodeStates.Focused | TreeNodeStates.Selected))
		{
			return ExtTreeView.BarActiveTextColor;
		}
		if ((state & TreeNodeStates.Selected) == TreeNodeStates.Selected)
		{
			return ExtTreeView.BarInactiveTextColor;
		}
		if (c == Color.Empty)
		{
			return ExtTreeView.TextColor;
		}
		return c;
	}

	public virtual DragDropEffects GetDragDropEffect(IDataObject dataObject, DragDropEffects proposedEffect)
	{
		return DragDropEffects.None;
	}

	public virtual void DoDragDrop(IDataObject dataObject, DragDropEffects effect)
	{
		throw new NotImplementedException();
	}

	public virtual void Cut()
	{
		throw new NotImplementedException();
	}

	public virtual void Copy()
	{
		throw new NotImplementedException();
	}

	public virtual void Paste()
	{
		throw new NotImplementedException();
	}

	public virtual void Delete()
	{
		throw new NotImplementedException();
	}

	public virtual void SelectAll()
	{
		throw new NotImplementedException();
	}
}

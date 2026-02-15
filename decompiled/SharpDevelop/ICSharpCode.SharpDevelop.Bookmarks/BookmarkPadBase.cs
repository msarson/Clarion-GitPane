using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public abstract class BookmarkPadBase : AbstractPadContent
{
	private Panel myPanel = new Panel();

	private ExtTreeView bookmarkTreeView = new ExtTreeView();

	private Dictionary<string, BookmarkFolderNode> fileNodes = new Dictionary<string, BookmarkFolderNode>();

	public override Control Control => myPanel;

	public BookmarkNode CurrentNode => bookmarkTreeView.SelectedNode as BookmarkNode;

	public IEnumerable<TreeNode> AllNodes
	{
		get
		{
			Stack<TreeNode> treeNodes = new Stack<TreeNode>();
			foreach (TreeNode node2 in bookmarkTreeView.Nodes)
			{
				treeNodes.Push(node2);
			}
			while (treeNodes.Count > 0)
			{
				TreeNode node = treeNodes.Pop();
				foreach (TreeNode node3 in node.Nodes)
				{
					treeNodes.Push(node3);
				}
				yield return node;
			}
		}
	}

	protected virtual ToolStrip CreateToolStrip()
	{
		ToolStrip toolStrip = ToolbarService.CreateToolStrip(this, "/SharpDevelop/Pads/BookmarkPad/Toolbar");
		toolStrip.Stretch = true;
		toolStrip.GripStyle = ToolStripGripStyle.Hidden;
		return toolStrip;
	}

	protected BookmarkPadBase()
	{
		bookmarkTreeView.Dock = DockStyle.Fill;
		bookmarkTreeView.CheckBoxes = true;
		bookmarkTreeView.HideSelection = false;
		bookmarkTreeView.Font = ExtTreeNode.RegularBigFont;
		bookmarkTreeView.IsSorted = false;
		myPanel.Controls.AddRange(new Control[2]
		{
			bookmarkTreeView,
			CreateToolStrip()
		});
		BookmarkManager.Added += BookmarkManagerAdded;
		BookmarkManager.Removed += BookmarkManagerRemoved;
		foreach (SDBookmark bookmark in BookmarkManager.Bookmarks)
		{
			AddMark(bookmark);
		}
	}

	public void EnableDisableAll()
	{
		bool flag = false;
		foreach (TreeNode allNode in AllNodes)
		{
			if (allNode is BookmarkNode && ((BookmarkNode)allNode).Checked)
			{
				flag = true;
				break;
			}
		}
		foreach (TreeNode allNode2 in AllNodes)
		{
			if (allNode2 is BookmarkNode)
			{
				((BookmarkNode)allNode2).Checked = !flag;
			}
		}
	}

	private void AddMark(SDBookmark mark)
	{
		if (ShowBookmarkInThisPad(mark))
		{
			if (!fileNodes.ContainsKey(mark.FileName))
			{
				BookmarkFolderNode bookmarkFolderNode = new BookmarkFolderNode(mark.FileName);
				fileNodes.Add(mark.FileName, bookmarkFolderNode);
				bookmarkTreeView.Nodes.Add(bookmarkFolderNode);
			}
			fileNodes[mark.FileName].AddMark(mark);
			fileNodes[mark.FileName].Expand();
		}
	}

	protected virtual bool ShowBookmarkInThisPad(SDBookmark mark)
	{
		if (mark.IsVisibleInBookmarkPad)
		{
			return !(mark is BreakpointBookmark);
		}
		return false;
	}

	private void BookmarkManagerAdded(object sender, BookmarkEventArgs e)
	{
		AddMark(e.Bookmark);
	}

	private void BookmarkManagerRemoved(object sender, BookmarkEventArgs e)
	{
		if (fileNodes.ContainsKey(e.Bookmark.FileName))
		{
			fileNodes[e.Bookmark.FileName].RemoveMark(e.Bookmark);
			if (fileNodes[e.Bookmark.FileName].Marks.Count == 0)
			{
				bookmarkTreeView.Nodes.Remove(fileNodes[e.Bookmark.FileName]);
				fileNodes.Remove(e.Bookmark.FileName);
			}
		}
	}

	private void TreeViewDoubleClick(object sender, EventArgs e)
	{
		TreeNode selectedNode = bookmarkTreeView.SelectedNode;
		if (selectedNode != null && selectedNode.Tag is SDBookmark sDBookmark)
		{
			FileService.JumpToFilePosition(sDBookmark.FileName, sDBookmark.LineNumber, 0);
		}
	}
}

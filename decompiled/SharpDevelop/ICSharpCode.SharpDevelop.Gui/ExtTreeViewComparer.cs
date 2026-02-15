using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

public class ExtTreeViewComparer : IComparer<TreeNode>
{
	public int Compare(TreeNode x, TreeNode y)
	{
		ExtTreeNode extTreeNode = x as ExtTreeNode;
		ExtTreeNode extTreeNode2 = y as ExtTreeNode;
		if (extTreeNode == null || extTreeNode2 == null)
		{
			return x.Text.CompareTo(y.Text);
		}
		if (extTreeNode.SortOrder != extTreeNode2.SortOrder)
		{
			return Math.Sign(extTreeNode.SortOrder - extTreeNode2.SortOrder);
		}
		return extTreeNode.CompareString.CompareTo(extTreeNode2.CompareString);
	}
}

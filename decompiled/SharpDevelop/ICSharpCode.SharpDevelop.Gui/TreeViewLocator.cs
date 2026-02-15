using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

[ProvideProperty("DoLocate", typeof(TreeView))]
public class TreeViewLocator : ListLocatorBase
{
	public class SearchFoundEventArgs : SearchEventArgs
	{
		private object _objectSearched;

		private bool _Found;

		private Stack<Control> _controlsToSelect;

		public object ObjectSearched => _objectSearched;

		public bool Found
		{
			get
			{
				return _Found;
			}
			set
			{
				_Found = value;
			}
		}

		public SearchFoundEventArgs(object objectSearched, string searchText, bool alreadyFound)
			: base(searchText)
		{
			_objectSearched = objectSearched;
			_controlsToSelect = new Stack<Control>();
		}

		public SearchFoundEventArgs(object objectSearched, string searchText, bool alreadyFound, Stack<Control> controlsToSelect)
			: this(objectSearched, searchText, alreadyFound)
		{
			_controlsToSelect = controlsToSelect;
		}

		public void AddControl(Control controlToSelect)
		{
			_controlsToSelect.Push(controlToSelect);
		}

		public void SelectControls()
		{
			int count = _controlsToSelect.Count;
			for (int num = count - 1; num >= 0; num--)
			{
				Control control = _controlsToSelect.Pop();
				control.Select();
			}
		}
	}

	private TreeView treeToSearch;

	[Description("Tree to search.\r\nIf no tree is selected you can select the locator from the tree it self.\r\nThis option is to allow the locator to work in multiple tree at the same time.")]
	[Category("Search Behavior")]
	public TreeView TreeToSearch
	{
		get
		{
			return treeToSearch;
		}
		set
		{
			if (treeToSearch != null)
			{
				treeToSearch.KeyPress -= base.KeyPressOnTree;
			}
			treeToSearch = value;
			if (treeToSearch != null)
			{
				treeToSearch.KeyPress += base.KeyPressOnTree;
			}
		}
	}

	public override bool HasListToLocate => TreeToSearch != null;

	public override bool ListHasItems
	{
		get
		{
			if (HasListToLocate)
			{
				return TreeToSearch.Nodes.Count > 0;
			}
			return false;
		}
	}

	public event EventHandler<SearchFoundEventArgs> ObjectSerchRequested;

	protected override void ExpandAll()
	{
		if (treeToSearch != null)
		{
			treeToSearch.ExpandAll();
		}
	}

	protected override void ContractAll()
	{
		if (treeToSearch != null)
		{
			treeToSearch.CollapseAll();
		}
	}

	public TreeViewLocator()
	{
		Font = FontService.GetFont(FontService.FontType.ListControls);
	}

	private bool OnObjectSearchRequested(TreeNode currentNode, string textToSearch, Stack<Control> controlsToSelect)
	{
		bool result = false;
		if (this.ObjectSerchRequested != null)
		{
			SearchFoundEventArgs e = new SearchFoundEventArgs(currentNode.Tag, textToSearch, alreadyFound: false, controlsToSelect);
			this.ObjectSerchRequested(null, e);
			result = e.Found;
		}
		return result;
	}

	public override bool Search(string text, bool fromTop)
	{
		if (ListHasItems)
		{
			return Search(TreeToSearch, text, fromTop);
		}
		return false;
	}

	private void SetSelectedNode(TreeView tree, TreeNode node)
	{
		if (tree == null || node == null)
		{
			return;
		}
		if (tree.SelectedNode != node)
		{
			if (node.Parent != null)
			{
				SetSelectedNode(tree, node.Parent);
			}
			tree.SelectedNode = node;
		}
		tree.SelectedNode.Expand();
	}

	private void SelectFoundControl(Stack<Control> _controlsToSelect)
	{
		int count = _controlsToSelect.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			Control control = _controlsToSelect.Pop();
			if (control.CanSelect)
			{
				control.Select();
			}
			if (control is TabPage)
			{
				TabControl tabControl = (TabControl)control.Parent;
				tabControl.SelectedTab = (TabPage)control;
			}
		}
	}

	private bool Search(TreeView tree, string textToSearch, bool fromTop)
	{
		TreeNode treeNode = null;
		if (fromTop || tree.SelectedNode == null)
		{
			tree.SelectedNode = tree.Nodes[0];
		}
		TreeNode selectedNode = tree.SelectedNode;
		Stack<Control> controlsToSelect = new Stack<Control>();
		if (selectedNode != null)
		{
			treeNode = Search(selectedNode, selectedNode, textToSearch.ToUpper(), skipFirst: true, controlsToSelect);
		}
		if (treeNode != null)
		{
			SetSelectedNode(tree, treeNode);
			SelectFoundControl(controlsToSelect);
		}
		if (tree.SelectedNode != null)
		{
			if (tree.SelectedNode.Parent != null)
			{
				tree.SelectedNode.Parent.Expand();
			}
			else
			{
				tree.SelectedNode.Expand();
			}
		}
		return treeNode != null;
	}

	private TreeNode Search(TreeNode origin, TreeNode node, string textToSearch, bool skipFirst, Stack<Control> controlsToSelect)
	{
		if (node != null)
		{
			if (!skipFirst && ((base.InString && node.Text.ToUpper().Contains(textToSearch)) || (!base.InString && node.Text.ToUpper().StartsWith(textToSearch)) || OnObjectSearchRequested(node, textToSearch, controlsToSelect)))
			{
				return node;
			}
			TreeNode treeNode = null;
			foreach (TreeNode node2 in node.Nodes)
			{
				treeNode = Search(origin, node2, textToSearch, skipFirst: false, controlsToSelect);
				if (treeNode != null)
				{
					return treeNode;
				}
			}
			treeNode = Search(origin, node.NextNode, textToSearch, skipFirst: false, controlsToSelect);
			if (treeNode != null)
			{
				return treeNode;
			}
			if (node.Parent != null && node == origin)
			{
				TreeNode treeNode2 = null;
				TreeNode treeNode3 = node;
				while (treeNode2 == null && treeNode3 != null)
				{
					treeNode3 = treeNode3.Parent;
					if (treeNode3 != null)
					{
						treeNode2 = treeNode3.NextNode;
					}
				}
				if (treeNode2 != null)
				{
					return Search(treeNode2, treeNode2, textToSearch, skipFirst: false, controlsToSelect);
				}
			}
		}
		return null;
	}

	protected override bool CanLocateAtControl(object extendee)
	{
		return extendee is TreeView;
	}

	[DefaultValue("")]
	[Category("Search Behavior")]
	[Description("Sets the locator for this Tree")]
	public bool GetDoLocate(Control extendee)
	{
		return TreeToSearch == extendee;
	}

	public void SetDoLocate(Control extendee, object value)
	{
		if (!(extendee is TreeView treeView))
		{
			return;
		}
		if (value == null || !bool.Parse(value.ToString()))
		{
			if (TreeToSearch == extendee)
			{
				TreeToSearch = null;
			}
		}
		else
		{
			TreeToSearch = treeView;
		}
	}
}

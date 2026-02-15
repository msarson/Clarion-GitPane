using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public abstract class AbstractProjectBrowserTreeNode : ExtTreeNode, IDisposable
{
	private string toolbarAddinTreePath;

	protected bool autoClearNodes = true;

	private Image overlay;

	public virtual string ToolbarAddinTreePath
	{
		get
		{
			return toolbarAddinTreePath;
		}
		set
		{
			toolbarAddinTreePath = value;
		}
	}

	public virtual Solution Solution
	{
		get
		{
			if (base.Parent is AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode)
			{
				return abstractProjectBrowserTreeNode.Solution;
			}
			return null;
		}
	}

	public virtual IProject Project
	{
		get
		{
			if (base.Parent is AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode)
			{
				return abstractProjectBrowserTreeNode.Project;
			}
			return null;
		}
	}

	public static bool ShowAll
	{
		get
		{
			return PropertyService.Get("ProjectBrowser.ShowAll", defaultValue: false);
		}
		set
		{
			PropertyService.Set("ProjectBrowser.ShowAll", value);
		}
	}

	public Image Overlay
	{
		get
		{
			return overlay;
		}
		set
		{
			if (overlay != value)
			{
				overlay = value;
				if (base.TreeView != null && base.IsVisible)
				{
					Rectangle bounds = base.Bounds;
					bounds.Width += bounds.X;
					bounds.X = 0;
					base.TreeView.Invalidate(bounds);
				}
			}
		}
	}

	public static event TreeViewEventHandler AfterNodeInitialize;

	public override void Expanding()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			if (autoClearNodes)
			{
				base.Nodes.Clear();
			}
			Initialize();
			base.UpdateVisibility();
		}
	}

	public virtual void ShowProperties()
	{
		WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(typeof(PropertyPad).FullName);
	}

	public static bool IsSomewhereBelow(string path, ProjectItem item)
	{
		return item.Include.StartsWith(path);
	}

	public static LinkedListNode<T> Remove<T>(LinkedList<T> list, LinkedListNode<T> item)
	{
		LinkedListNode<T> next = item.Next;
		if (item == list.First)
		{
			list.RemoveFirst();
		}
		else if (item == list.Last)
		{
			list.RemoveLast();
		}
		else
		{
			list.Remove(item);
		}
		return next;
	}

	protected override void Initialize()
	{
		base.Initialize();
		if (AbstractProjectBrowserTreeNode.AfterNodeInitialize != null)
		{
			AbstractProjectBrowserTreeNode.AfterNodeInitialize(null, new TreeViewEventArgs(this));
		}
	}

	public abstract object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data);

	public virtual object AcceptChildren(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		foreach (TreeNode node in base.Nodes)
		{
			if (node is AbstractProjectBrowserTreeNode)
			{
				((AbstractProjectBrowserTreeNode)node).AcceptVisitor(visitor, data);
			}
		}
		return data;
	}

	protected string GetQuestionText(string question)
	{
		return StringParser.Parse(question, new string[1, 2] { { "FileName", base.Text } });
	}

	protected void SelectPreviousNode(TreeNode prev)
	{
		if (prev == null || base.Parent == null || base.Parent.TreeView == null)
		{
			return;
		}
		foreach (TreeNode node in base.Parent.Nodes)
		{
			if (node.Text == prev.Text)
			{
				base.Parent.TreeView.SelectedNode = node;
				break;
			}
		}
	}
}

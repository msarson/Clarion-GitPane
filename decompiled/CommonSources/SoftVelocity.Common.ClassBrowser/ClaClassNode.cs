using System;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.ClassBrowser;

public class ClaClassNode : ExtTreeNode, IClassNode
{
	private IClass c;

	private IProject project;

	private bool isIncluded;

	public static int QueueIcon;

	public static int FileIcon;

	public static int RoutineIcon;

	public static int KeyIcon;

	public static int EmptyIcon;

	public static int CodeSnippetIcon;

	public IClass Class
	{
		get
		{
			return c;
		}
		set
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			c = value;
			((TreeNode)this).TreeView.BeginUpdate();
			string viewStateString = GetViewStateString(this);
			((ExtTreeNode)this).Initialize();
			if (((TreeNode)this).TreeView is ExtTreeView)
			{
				((ExtTreeView)((TreeNode)this).TreeView).SortNodes(((TreeNode)this).Nodes, false);
			}
			ApplyViewStateString(viewStateString, this);
			((TreeNode)this).TreeView.EndUpdate();
		}
	}

	public override bool Visible
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			ClassBrowserFilter filter = ClassBrowserPad.Instance.Filter;
			if ((filter & 0x40) == 0 && isIncluded)
			{
				return false;
			}
			return true;
		}
	}

	static ClaClassNode()
	{
		ResourceService.RegisterImages("CommonSources.Resources.Clarion.ClassBrowser.BitmapResources", Assembly.GetExecutingAssembly());
		QueueIcon = ClassBrowserIconService.ImageList.Images.Count;
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.QueueIcon"));
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.InternalQueueIcon"));
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.ProtectedQueueIcon"));
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.PrivateQueueIcon"));
		FileIcon = ClassBrowserIconService.ImageList.Images.Count;
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.FileIcon"));
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.InternalFileIcon"));
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.FileIcon"));
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.FileIcon"));
		RoutineIcon = ClassBrowserIconService.ImageList.Images.Count;
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.RoutineIcon"));
		EmptyIcon = ClassBrowserIconService.ImageList.Images.Count;
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.EmptyIcon"));
		KeyIcon = ClassBrowserIconService.ImageList.Images.Count;
		ClassBrowserIconService.ImageList.Images.Add(ResourceService.GetIcon("Clarion.ClassBrowser.KeyIcon"));
		CodeSnippetIcon = ClassBrowserIconService.ImageList.Images.Count;
		ClassBrowserIconService.ImageList.Images.Add(IconService.GetBitmap("Icons.16x16.TextFileIcon"));
	}

	public static int GetIconIndexForClass(IClass c, ref int sortOrder)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (c is ClaClass)
		{
			ClaClass claClass = (ClaClass)(object)c;
			switch (claClass.ClarionType)
			{
			case ClarionType.FILE:
				sortOrder = 5;
				return FileIcon + GetModifierOffset(((IDecoration)c).Modifiers);
			case ClarionType.QUEUE:
				sortOrder = 6;
				return QueueIcon + GetModifierOffset(((IDecoration)c).Modifiers);
			case ClarionType.GROUP:
			case ClarionType.RECORD:
				sortOrder = 7;
				return 22 + GetModifierOffset(((IDecoration)c).Modifiers);
			case ClarionType.STRUCT:
				sortOrder = 8;
				return 22 + GetModifierOffset(((IDecoration)c).Modifiers);
			case ClarionType.INTERFACE:
				sortOrder = 4;
				return 26 + GetModifierOffset(((IDecoration)c).Modifiers);
			default:
				return ClassBrowserIconService.GetIcon(c);
			}
		}
		return ClassBrowserIconService.GetIcon(c);
	}

	private static int GetModifierOffset(ModifierEnum modifier)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if ((modifier & 8) == 8)
		{
			return 0;
		}
		if ((modifier & 4) == 4)
		{
			return 2;
		}
		if ((modifier & 2) == 2)
		{
			return 1;
		}
		return 3;
	}

	public ClaClassNode(IProject project, IClass c)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Invalid comparison between Unknown and I4
		if (c is ClaGlobalClass)
		{
			base.sortOrder = 2;
			((TreeNode)this).Text = ClaGlobalClass.globalClassName;
		}
		else
		{
			if (project is CommonClarionProject)
			{
				((ExtTreeNode)this).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserClassMenuPath;
			}
			base.sortOrder = 3;
			((TreeNode)this).Text = ClassNameWithPre(c);
			if (c is ClaClass)
			{
				ClaClass claClass = (ClaClass)(object)c;
				if (!c.CompilationUnit.FileName.Equals(claClass.ClaRegion.FileName) && !c.CompilationUnit.FileName.Equals(claClass.ClaBodyRegion.FileName))
				{
					isIncluded = true;
				}
			}
		}
		this.project = project;
		this.c = c;
		int selectedImageIndex = (((TreeNode)this).ImageIndex = GetIconIndexForClass(c, ref base.sortOrder));
		((TreeNode)this).SelectedImageIndex = selectedImageIndex;
		if ((int)c.ClassType != 4)
		{
			((TreeNode)this).Nodes.Add(new TreeNode());
		}
	}

	public override void ActivateItem()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (c is ClaClass)
		{
			string fileName = ((ClaClass)(object)c).ClaRegion.FileName;
			DomRegion region = c.Region;
			int num = ((DomRegion)(ref region)).BeginLine - 1;
			DomRegion region2 = c.Region;
			FileService.JumpToFilePosition(fileName, num, ((DomRegion)(ref region2)).BeginColumn - 1);
		}
		else if (c.CompilationUnit != null)
		{
			string fileName2 = c.CompilationUnit.FileName;
			DomRegion region3 = c.Region;
			int num2 = ((DomRegion)(ref region3)).BeginLine - 1;
			DomRegion region4 = c.Region;
			FileService.JumpToFilePosition(fileName2, num2, ((DomRegion)(ref region4)).BeginColumn - 1);
		}
	}

	protected override void Initialize()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		((ExtTreeNode)this).Initialize();
		((TreeNode)this).Nodes.Clear();
		if ((int)c.ClassType == 4)
		{
			return;
		}
		if (c.BaseTypes.Count > 0 && c.BaseType != null && (c.BaseType == null || !(c.BaseType.GetUnderlyingClass() is FakeParentClass)))
		{
			((ExtTreeNode)new ClaBaseTypesNode(project, c)).AddTo((TreeNode)(object)this);
		}
		if ((((IDecoration)c).Modifiers & 0x40) != 64)
		{
			((ExtTreeNode)new ClaDerivedTypesNode(project, c)).AddTo((TreeNode)(object)this);
		}
		foreach (IClass innerClass in c.InnerClasses)
		{
			((ExtTreeNode)new ClaClassNode(project, innerClass)).AddTo((TreeNode)(object)this);
		}
		foreach (IMethod method in c.Methods)
		{
			ClaMemberNode claMemberNode = ClaMemberNode.Create(project, (IMember)(object)method);
			((ExtTreeNode)claMemberNode).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserMemberMenuPath;
			((ExtTreeNode)claMemberNode).AddTo((TreeNode)(object)this);
		}
		foreach (IProperty property in c.Properties)
		{
			ClaMemberNode claMemberNode2 = ClaMemberNode.Create(project, (IMember)(object)property);
			((ExtTreeNode)claMemberNode2).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserMemberMenuPath;
			((ExtTreeNode)claMemberNode2).AddTo((TreeNode)(object)this);
		}
		foreach (IField field in c.Fields)
		{
			ClaMemberNode claMemberNode3 = ClaMemberNode.Create(project, (IMember)(object)field);
			((ExtTreeNode)claMemberNode3).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserMemberMenuPath;
			((ExtTreeNode)claMemberNode3).AddTo((TreeNode)(object)this);
		}
		foreach (IEvent @event in c.Events)
		{
			ClaMemberNode claMemberNode4 = ClaMemberNode.Create(project, (IMember)(object)@event);
			((ExtTreeNode)claMemberNode4).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserMemberMenuPath;
			((ExtTreeNode)claMemberNode4).AddTo((TreeNode)(object)this);
		}
		((ExtTreeNode)this).UpdateVisibility();
		if (((TreeNode)this).TreeView is ExtTreeView)
		{
			((ExtTreeView)((TreeNode)this).TreeView).SortNodes(((TreeNode)this).Nodes, false);
		}
	}

	public static string ClassNameWithPre(IClass c)
	{
		if (!(c is ClaClass claClass))
		{
			return c.Name;
		}
		if (!string.IsNullOrEmpty(claClass.PreName) && !claClass.Name.Equals(claClass.PreName, StringComparison.InvariantCultureIgnoreCase))
		{
			return claClass.Name + " (" + claClass.PreName + ")";
		}
		return claClass.Name;
	}

	public static string GetViewStateString(ClaClassNode node)
	{
		if (!((TreeNode)(object)node).IsExpanded || ((TreeNode)(object)node).Nodes.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		WriteViewStateString(stringBuilder, (TreeNode)(object)node);
		return stringBuilder.ToString();
	}

	private static void WriteViewStateString(StringBuilder b, TreeNode node)
	{
		b.Append('{');
		foreach (TreeNode node2 in node.Nodes)
		{
			if (node2.IsExpanded && node2.Text.IndexOf('{') < 0)
			{
				b.Append(node2.Text);
				WriteViewStateString(b, node2);
			}
		}
		b.Append('}');
	}

	public static void ApplyViewStateString(string viewState, ClaClassNode node)
	{
		if (!string.IsNullOrEmpty(viewState))
		{
			int pos = 0;
			ApplyViewStateString((TreeNode)(object)node, viewState, ref pos);
		}
	}

	private static bool ApplyViewStateString(TreeNode node, string viewState, ref int pos)
	{
		if (viewState[pos++] != '{')
		{
			return false;
		}
		while (viewState[pos] != '}')
		{
			StringBuilder stringBuilder = new StringBuilder();
			char value;
			while ((value = viewState[pos++]) != '{')
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
			if (!ApplyViewStateString(treeNode, viewState, ref pos))
			{
				return false;
			}
			pos++;
		}
		return true;
	}
}

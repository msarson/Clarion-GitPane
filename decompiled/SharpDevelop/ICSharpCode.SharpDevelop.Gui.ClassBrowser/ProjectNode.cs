using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ProjectNode : AbstractProjectNode
{
	protected ProjectNode()
	{
	}

	public ProjectNode(IProject project)
		: base(project)
	{
		sortOrder = 0;
		base.Text = base.Project.Name;
		SetIcon(IconService.GetImageForProjectType(base.Project.Language));
		base.Nodes.Add(new TreeNode(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Pads.ClassScout.LoadingNode}")));
	}

	public override void UpdateParseInformation(ICompilationUnit oldUnit, ICompilationUnit unit)
	{
		Dictionary<string, IClass> dictionary = new Dictionary<string, IClass>();
		Dictionary<string, bool> dictionary2 = new Dictionary<string, bool>();
		if (oldUnit != null)
		{
			foreach (IClass @class in oldUnit.Classes)
			{
				dictionary[@class.FullyQualifiedName] = @class.GetCompoundClass();
				dictionary2[@class.FullyQualifiedName] = false;
			}
		}
		if (unit != null)
		{
			foreach (IClass class2 in unit.Classes)
			{
				TreeNode nodeByPath = GetNodeByPath(class2.Namespace, create: true);
				if (GetNodeByName(nodeByPath.Nodes, class2.Name) is ClassNode classNode)
				{
					classNode.Class = class2.GetCompoundClass();
				}
				else
				{
					new ClassNode(base.Project, class2.GetCompoundClass()).AddTo(nodeByPath);
				}
				dictionary2[class2.FullyQualifiedName] = true;
			}
		}
		foreach (KeyValuePair<string, bool> item in dictionary2)
		{
			if (item.Value)
			{
				continue;
			}
			IClass obj = dictionary[item.Key];
			TreeNode nodeByPath2 = GetNodeByPath(obj.Namespace, create: true);
			if (GetNodeByName(nodeByPath2.Nodes, obj.Name) is ClassNode classNode2)
			{
				if (obj is CompoundClass compoundClass)
				{
					classNode2.Class = compoundClass;
					continue;
				}
				nodeByPath2.Nodes.Remove(classNode2);
				RemoveEmptyNamespace(nodeByPath2);
			}
		}
	}

	private void RemoveEmptyNamespace(TreeNode path)
	{
		if (path.Tag is string && path.Nodes.Count == 0)
		{
			TreeNode treeNode = path.Parent;
			treeNode.Nodes.Remove(path);
			RemoveEmptyNamespace(treeNode);
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		IProjectContent projectContent = ParserService.GetProjectContent(base.Project);
		if (projectContent == null)
		{
			return;
		}
		base.Nodes.Clear();
		ReferenceFolderNode referencesNode = new ReferenceFolderNode(base.Project);
		referencesNode.AddTo(this);
		projectContent.ReferencedContentsChanged += delegate
		{
			WorkbenchSingleton.SafeThreadAsyncCall(referencesNode.UpdateReferenceNodes);
		};
		foreach (ProjectItem item in base.Project.GetItemsOfType(ItemType.Compile))
		{
			ParseInformation parseInformation = ParserService.GetParseInformation(item.FileName);
			if (parseInformation != null)
			{
				InsertParseInformation(parseInformation.BestCompilationUnit);
			}
		}
	}

	private void InsertParseInformation(ICompilationUnit unit)
	{
		foreach (IClass @class in unit.Classes)
		{
			TreeNode nodeByPath = GetNodeByPath(@class.Namespace, create: true);
			TreeNode nodeByName = GetNodeByName(nodeByPath.Nodes, @class.Name);
			if (nodeByName == null)
			{
				new ClassNode(base.Project, @class.GetCompoundClass()).AddTo(nodeByPath);
			}
		}
	}

	protected virtual string StripRootNamespace(string directory)
	{
		if (base.Project != null)
		{
			string rootNamespace = base.Project.RootNamespace;
			if (directory.StartsWith(rootNamespace))
			{
				directory = directory.Substring(rootNamespace.Length);
			}
		}
		return directory;
	}

	public override TreeNode GetNodeByPath(string directory, bool create)
	{
		return FindNodeByPath(directory, create, expand: false);
	}

	public override TreeNode ExpandNodeByPath(string directory, bool create)
	{
		return FindNodeByPath(directory, create, expand: true);
	}

	private TreeNode FindNodeByPath(string directory, bool create, bool expand)
	{
		directory = StripRootNamespace(directory);
		string[] array = directory.Split('.');
		TreeNodeCollection treeNodeCollection = base.Nodes;
		TreeNode treeNode = this;
		if (expand)
		{
			treeNode.Expand();
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.Length == 0 || text[0] == '.')
			{
				continue;
			}
			TreeNode nodeByName = GetNodeByName(treeNodeCollection, text);
			if (nodeByName == null)
			{
				if (!create)
				{
					return null;
				}
				ExtTreeNode extTreeNode = new ExtTreeNode();
				extTreeNode.Tag = text;
				extTreeNode.Text = text;
				int imageIndex = (extTreeNode.SelectedImageIndex = 3);
				extTreeNode.ImageIndex = imageIndex;
				treeNodeCollection.Add(extTreeNode);
				if (expand)
				{
					extTreeNode.Expand();
				}
				treeNode = extTreeNode;
				treeNodeCollection = treeNode.Nodes;
			}
			else
			{
				treeNode = nodeByName;
				treeNodeCollection = treeNode.Nodes;
				if (expand)
				{
					treeNode.Expand();
				}
			}
		}
		return treeNode;
	}

	private static TreeNode GetNodeByName(TreeNodeCollection collection, string name)
	{
		foreach (TreeNode item in collection)
		{
			if (!(item is ReferenceFolderNode) && item.Text == name)
			{
				return item;
			}
		}
		return null;
	}
}

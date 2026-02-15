using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ShellTree : TreeView
{
	public string NodePath
	{
		get
		{
			return (string)base.SelectedNode.Tag;
		}
		set
		{
			PopulateShellTree(value);
		}
	}

	public ShellTree()
	{
		base.Sorted = true;
		TreeNode treeNode = base.Nodes.Add(Path.GetFileName(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)));
		treeNode.ImageIndex = 6;
		treeNode.SelectedImageIndex = 6;
		treeNode.Tag = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
		TreeNode treeNode2 = treeNode.Nodes.Add(ResourceService.GetString("MainWindow.Windows.FileScout.MyDocuments"));
		treeNode2.ImageIndex = 7;
		treeNode2.SelectedImageIndex = 7;
		try
		{
			treeNode2.Tag = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}
		catch (Exception)
		{
			treeNode2.Tag = "C:\\";
		}
		treeNode2.Nodes.Add("");
		TreeNode treeNode3 = treeNode.Nodes.Add(ResourceService.GetString("MainWindow.Windows.FileScout.MyComputer"));
		treeNode3.ImageIndex = 8;
		treeNode3.SelectedImageIndex = 8;
		try
		{
			treeNode3.Tag = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}
		catch (Exception)
		{
			treeNode3.Tag = "C:\\";
		}
		string[] logicalDrives = Environment.GetLogicalDrives();
		foreach (string text in logicalDrives)
		{
			DriveObject driveObject = new DriveObject(text);
			TreeNode treeNode4 = new TreeNode(driveObject.ToString())
			{
				Nodes = 
				{
					new TreeNode("")
				},
				Tag = text.Substring(0, text.Length - 1)
			};
			treeNode3.Nodes.Add(treeNode4);
			switch (DriveObject.GetDriveType(text))
			{
			case DriveType.Removeable:
			{
				int imageIndex5 = (treeNode4.SelectedImageIndex = 2);
				treeNode4.ImageIndex = imageIndex5;
				break;
			}
			case DriveType.Fixed:
			{
				int imageIndex4 = (treeNode4.SelectedImageIndex = 3);
				treeNode4.ImageIndex = imageIndex4;
				break;
			}
			case DriveType.Cdrom:
			{
				int imageIndex3 = (treeNode4.SelectedImageIndex = 4);
				treeNode4.ImageIndex = imageIndex3;
				break;
			}
			case DriveType.Remote:
			{
				int imageIndex2 = (treeNode4.SelectedImageIndex = 5);
				treeNode4.ImageIndex = imageIndex2;
				break;
			}
			default:
			{
				int imageIndex = (treeNode4.SelectedImageIndex = 3);
				treeNode4.ImageIndex = imageIndex;
				break;
			}
			}
		}
		string[] directories = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
		foreach (string text2 in directories)
		{
			TreeNode treeNode5 = treeNode.Nodes.Add(Path.GetFileName(text2));
			treeNode5.Tag = text2;
			int imageIndex6 = (treeNode5.SelectedImageIndex = 0);
			treeNode5.ImageIndex = imageIndex6;
			treeNode5.Nodes.Add(new TreeNode(""));
		}
		treeNode.Expand();
		treeNode3.Expand();
		InitializeComponent();
	}

	private int getNodeLevel(TreeNode node)
	{
		TreeNode treeNode = node;
		int num = 0;
		while (true)
		{
			treeNode = treeNode.Parent;
			if (treeNode == null)
			{
				break;
			}
			num++;
		}
		return num;
	}

	private void InitializeComponent()
	{
		base.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(SetClosedIcon);
		base.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(SetOpenedIcon);
	}

	private void SetClosedIcon(object sender, TreeViewCancelEventArgs e)
	{
		if (base.SelectedNode != null && getNodeLevel(base.SelectedNode) > 2)
		{
			TreeNode treeNode = base.SelectedNode;
			int imageIndex = (base.SelectedNode.SelectedImageIndex = 0);
			treeNode.ImageIndex = imageIndex;
		}
	}

	private void SetOpenedIcon(object sender, TreeViewEventArgs e)
	{
		if (getNodeLevel(e.Node) > 2 && e.Node.Parent != null && e.Node.Parent.Parent != null)
		{
			TreeNode node = e.Node;
			int imageIndex = (e.Node.SelectedImageIndex = 1);
			node.ImageIndex = imageIndex;
		}
	}

	private void PopulateShellTree(string path)
	{
		string[] array = path.Split(Path.DirectorySeparatorChar);
		TreeNodeCollection treeNodeCollection = base.Nodes;
		string[] array2 = array;
		foreach (string value in array2)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if (((string)item.Tag).Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					base.SelectedNode = item;
					PopulateSubDirectory(item, 2);
					item.Expand();
					treeNodeCollection = item.Nodes;
					break;
				}
			}
		}
	}

	private void PopulateSubDirectory(TreeNode curNode, int depth)
	{
		if (--depth < 0)
		{
			return;
		}
		if (curNode.Nodes.Count == 1 && curNode.Nodes[0].Text.Equals(""))
		{
			string[] array = null;
			curNode.Nodes.Clear();
			try
			{
				array = Directory.GetDirectories(curNode.Tag.ToString() + Path.DirectorySeparatorChar);
			}
			catch (Exception)
			{
				return;
			}
			string[] array2 = array;
			foreach (string path in array2)
			{
				try
				{
					string fileName = Path.GetFileName(path);
					FileAttributes attributes = File.GetAttributes(path);
					if ((attributes & FileAttributes.Hidden) == 0)
					{
						TreeNode treeNode = curNode.Nodes.Add(fileName);
						treeNode.Tag = curNode.Tag.ToString() + Path.DirectorySeparatorChar + fileName;
						int imageIndex = (treeNode.SelectedImageIndex = 0);
						treeNode.ImageIndex = imageIndex;
						treeNode.Nodes.Add("");
						PopulateSubDirectory(treeNode, depth);
					}
				}
				catch (Exception)
				{
				}
			}
			return;
		}
		foreach (TreeNode node in curNode.Nodes)
		{
			PopulateSubDirectory(node, depth);
		}
	}

	protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		try
		{
			if (e.Node.Parent != null && e.Node.Parent.Parent != null)
			{
				PopulateSubDirectory(e.Node, 2);
				Cursor.Current = Cursors.Default;
			}
			else
			{
				PopulateSubDirectory(e.Node, 1);
				Cursor.Current = Cursors.Default;
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex, "Device error");
			e.Cancel = true;
		}
		Cursor.Current = Cursors.Default;
	}
}

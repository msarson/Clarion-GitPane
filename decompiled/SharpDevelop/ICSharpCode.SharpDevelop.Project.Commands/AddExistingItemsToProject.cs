using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddExistingItemsToProject : AbstractMenuCommand
{
	public enum ReplaceExistingFile
	{
		Yes,
		YesToAll,
		No,
		Cancel
	}

	public static ReplaceExistingFile ShowReplaceExistingFileDialog(string caption, string fileName, bool replacingMultiple)
	{
		if (caption == null)
		{
			caption = "${res:ProjectComponent.ContextMenu.AddExistingFiles.ReplaceExistingFile.Title}";
		}
		string text = StringParser.Parse("${res:ProjectComponent.ContextMenu.AddExistingFiles.ReplaceExistingFile}", new string[1, 2] { { "FileName", fileName } });
		if (replacingMultiple)
		{
			return (ReplaceExistingFile)MessageService.ShowCustomDialog(caption, text, 0, 3, "${res:Global.Yes}", "${res:Global.YesToAll}", "${res:Global.No}", "${res:Global.CancelButtonText}");
		}
		if (!MessageService.AskQuestion(text, caption))
		{
			return ReplaceExistingFile.No;
		}
		return ReplaceExistingFile.Yes;
	}

	private int GetFileFilterIndex(IProject project, string[] fileFilters)
	{
		if (project != null)
		{
			LanguageBindingDescriptor codonPerLanguageName = LanguageBindingService.GetCodonPerLanguageName(project.Language);
			if (codonPerLanguageName != null)
			{
				for (int i = 0; i < fileFilters.Length; i++)
				{
					for (int j = 0; j < codonPerLanguageName.CodeFileExtensions.Length; j++)
					{
						if (fileFilters[i].ToUpperInvariant().IndexOf(codonPerLanguageName.CodeFileExtensions[j].ToUpperInvariant()) >= 0)
						{
							return i + 1;
						}
					}
				}
			}
		}
		return 0;
	}

	public static void CopyDirectory(string directoryName, DirectoryNode node, bool includeInProject)
	{
		directoryName = Path.GetFullPath(directoryName);
		string text = Path.Combine(node.Directory, Path.GetFileName(directoryName));
		LoggingService.Debug("Copy " + directoryName + " to " + text);
		if (!FileUtility.IsEqualFileName(directoryName, text))
		{
			if (includeInProject && ProjectService.OpenSolution != null)
			{
				foreach (IProject project in ProjectService.OpenSolution.Projects)
				{
					if (!FileUtility.IsBaseDirectory(project.Directory, directoryName))
					{
						continue;
					}
					LoggingService.Debug("Searching for child items in " + project.Name);
					foreach (ProjectItem item in project.Items)
					{
						if (!(item is FileProjectItem fileProjectItem))
						{
							continue;
						}
						string text2 = Path.Combine(project.Directory, fileProjectItem.VirtualName);
						if (FileUtility.IsBaseDirectory(directoryName, text2) && (!(item.ItemType == ItemType.Folder) || !FileUtility.IsEqualFileName(directoryName, text2)))
						{
							LoggingService.Debug("Found file " + text2);
							FileProjectItem fileProjectItem2 = new FileProjectItem(node.Project, fileProjectItem.ItemType);
							if (FileUtility.IsBaseDirectory(directoryName, fileProjectItem.FileName))
							{
								fileProjectItem2.FileName = FileUtility.RenameBaseDirectory(fileProjectItem.FileName, directoryName, text);
							}
							else
							{
								fileProjectItem2.FileName = fileProjectItem.FileName;
							}
							fileProjectItem.CopyMetadataTo(fileProjectItem2);
							if (fileProjectItem.IsLink)
							{
								string absPath = FileUtility.RenameBaseDirectory(text2, directoryName, text);
								fileProjectItem.SetEvaluatedMetadata("Link", FileUtility.GetRelativePath(node.Project.Directory, absPath));
							}
							ProjectService.AddProjectItem(node.Project, fileProjectItem2);
						}
					}
				}
			}
			FileUtility.DeepCopy(directoryName, text, overwrite: true);
			DirectoryNode directoryNode = new DirectoryNode(text);
			directoryNode.AddTo(node);
			if (includeInProject)
			{
				IncludeFileInProject.IncludeDirectoryNode(directoryNode, includeSubNodes: false);
			}
			directoryNode.Expanding();
		}
		else
		{
			if (!includeInProject)
			{
				return;
			}
			foreach (TreeNode node2 in node.Nodes)
			{
				if (node2 is DirectoryNode)
				{
					DirectoryNode directoryNode2 = (DirectoryNode)node2;
					if (FileUtility.IsEqualFileName(directoryNode2.Directory, text))
					{
						IncludeFileInProject.IncludeDirectoryNode(directoryNode2, includeSubNodes: true);
					}
				}
			}
		}
	}

	public static FileProjectItem CopyFile(string fileName, DirectoryNode node, bool includeInProject)
	{
		string text = Path.Combine(node.Directory, Path.GetFileName(fileName));
		if (!FileUtility.IsEqualFileName(fileName, text))
		{
			File.Copy(fileName, text, overwrite: true);
		}
		if (includeInProject)
		{
			FileNode fileNode;
			foreach (ExtTreeNode allNode in node.AllNodes)
			{
				if (!(allNode is FileNode))
				{
					continue;
				}
				fileNode = (FileNode)allNode;
				if (FileUtility.IsEqualFileName(fileNode.FileName, text))
				{
					if (fileNode.FileNodeStatus == FileNodeStatus.Missing)
					{
						fileNode.FileNodeStatus = FileNodeStatus.InProject;
					}
					else if (fileNode.FileNodeStatus == FileNodeStatus.None)
					{
						return IncludeFileInProject.IncludeFileNode(fileNode);
					}
					return fileNode.ProjectItem as FileProjectItem;
				}
			}
			fileNode = new FileNode(text);
			fileNode.AddTo(node);
			return IncludeFileInProject.IncludeFileNode(fileNode);
		}
		return null;
	}

	public static IEnumerable<string> FindAdditionalFiles(string fileName)
	{
		List<string> list = new List<string>();
		StringParser.Properties["Extension"] = Path.GetExtension(fileName);
		string text = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
		foreach (string item in AddInTree.BuildItems("/SharpDevelop/Workbench/DependentFileExtensions", null, throwOnNotFound: false))
		{
			if (File.Exists(text + item))
			{
				list.Add(text + item);
			}
		}
		return list;
	}

	public override void Run()
	{
		TreeNode selectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		DirectoryNode directoryNode = selectedNode as DirectoryNode;
		if (directoryNode == null)
		{
			directoryNode = selectedNode.Parent as DirectoryNode;
		}
		if (directoryNode == null)
		{
			return;
		}
		directoryNode.Expanding();
		directoryNode.Expand();
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.AddExtension = true;
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		string[] array = (string[])AddInTree.GetTreeNode("/SharpDevelop/Workbench/FileFilter").BuildChildItems(this).ToArray(typeof(string));
		openFileDialog.FilterIndex = GetFileFilterIndex(directoryNode.Project, array);
		openFileDialog.Filter = string.Join("|", array);
		openFileDialog.Multiselect = true;
		openFileDialog.CheckFileExists = true;
		openFileDialog.Title = StringParser.Parse("${res:ProjectComponent.ContextMenu.AddExistingFiles}");
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) != DialogResult.OK)
		{
			return;
		}
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(openFileDialog.FileNames.Length);
		string[] fileNames = openFileDialog.FileNames;
		foreach (string key in fileNames)
		{
			list.Add(new KeyValuePair<string, string>(key, ""));
		}
		bool flag = false;
		string[] fileNames2 = openFileDialog.FileNames;
		foreach (string text in fileNames2)
		{
			foreach (string additionalFile in FindAdditionalFiles(text))
			{
				Predicate<KeyValuePair<string, string>> match = (KeyValuePair<string, string> pair) => FileUtility.IsEqualFileName(pair.Key, additionalFile);
				if (!list.Exists(match))
				{
					flag = true;
					list.Add(new KeyValuePair<string, string>(additionalFile, Path.GetFileName(text)));
				}
			}
		}
		string fileName = Path.Combine(directoryNode.Directory, Path.GetFileName(list[0].Key));
		if (!FileUtility.IsEqualFileName(list[0].Key, fileName))
		{
			switch (MessageService.ShowCustomDialog(openFileDialog.Title, "${res:ProjectComponent.ContextMenu.AddExistingFiles.Question}", 0, 2, "${res:ProjectComponent.ContextMenu.AddExistingFiles.Copy}", "${res:ProjectComponent.ContextMenu.AddExistingFiles.Link}", "${res:Global.CancelButtonText}"))
			{
			case 1:
				foreach (KeyValuePair<string, string> item in list)
				{
					string key2 = item.Key;
					string relativePath = FileUtility.GetRelativePath(directoryNode.Project.Directory, key2);
					FileNode fileNode = new FileNode(key2, FileNodeStatus.InProject);
					FileProjectItem fileProjectItem = new FileProjectItem(directoryNode.Project, directoryNode.Project.GetDefaultItemType(key2), relativePath);
					fileProjectItem.SetEvaluatedMetadata("Link", Path.Combine(directoryNode.RelativePath, Path.GetFileName(key2)));
					fileProjectItem.DependentUpon = item.Value;
					fileNode.ProjectItem = fileProjectItem;
					fileNode.AddTo(directoryNode);
					ProjectService.AddProjectItem(directoryNode.Project, fileProjectItem);
				}
				directoryNode.Project.Save();
				if (flag)
				{
					directoryNode.RecreateSubNodes();
				}
				return;
			case 2:
				return;
			}
		}
		bool flag2 = false;
		foreach (KeyValuePair<string, string> item2 in list)
		{
			fileName = Path.Combine(directoryNode.Directory, Path.GetFileName(item2.Key));
			if (!flag2 && File.Exists(fileName) && !FileUtility.IsEqualFileName(item2.Key, fileName))
			{
				ReplaceExistingFile replaceExistingFile = ShowReplaceExistingFileDialog(openFileDialog.Title, Path.GetFileName(item2.Key), replacingMultiple: true);
				if (replaceExistingFile == ReplaceExistingFile.YesToAll)
				{
					flag2 = true;
				}
				else
				{
					switch (replaceExistingFile)
					{
					case ReplaceExistingFile.No:
						continue;
					case ReplaceExistingFile.Cancel:
						goto end_IL_03b4;
					}
				}
			}
			FileProjectItem fileProjectItem2 = CopyFile(item2.Key, directoryNode, includeInProject: true);
			if (fileProjectItem2 != null)
			{
				fileProjectItem2.DependentUpon = item2.Value;
			}
			continue;
			end_IL_03b4:
			break;
		}
		directoryNode.Project.Save();
		if (flag)
		{
			directoryNode.RecreateSubNodes();
		}
	}
}

using System;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Project;

[Serializable]
public class FileOperationClipboardObject
{
	private string fileName;

	private bool performMove;

	public string FileName => fileName;

	public bool PerformMove => performMove;

	public FileOperationClipboardObject(string fileName, bool performMove)
	{
		this.fileName = fileName;
		this.performMove = performMove;
	}

	public static IDataObject CreateDataObject(FileNode node, bool performMove)
	{
		return new DataObject(typeof(FileNode).ToString(), new FileOperationClipboardObject(node.FileName, performMove));
	}

	public static IDataObject CreateDataObject(SolutionItemNode node, bool performMove)
	{
		return new DataObject(typeof(SolutionItemNode).ToString(), new FileOperationClipboardObject(node.FileName, performMove));
	}

	public static IDataObject CreateDataObject(DirectoryNode node, bool performMove)
	{
		return new DataObject(typeof(DirectoryNode).ToString(), new FileOperationClipboardObject(node.Directory, performMove));
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Util;

namespace ICSharpCode.SharpDevelop.Project;

public class ParseableFileContentEnumerator : IEnumerator<KeyValuePair<string, string>>, IDisposable, IEnumerator
{
	private KeyValuePair<string, string> current;

	private IList<ProjectItem> projectItems;

	private bool isOnMainThread;

	private Encoding defaultEncoding;

	private ProjectItem nextItem;

	private int index;

	private IViewContent[] viewContentCollection;

	object IEnumerator.Current => current;

	public KeyValuePair<string, string> Current => current;

	public string CurrentFileName => current.Key;

	public string CurrentFileContent => current.Value;

	public int ItemCount => projectItems.Count;

	public int Index => index;

	void IEnumerator.Reset()
	{
		throw new NotSupportedException();
	}

	public void Dispose()
	{
	}

	public ParseableFileContentEnumerator(IProject project)
		: this(project.Items)
	{
	}

	public ParseableFileContentEnumerator(IList<ProjectItem> projectItems)
	{
		isOnMainThread = !WorkbenchSingleton.InvokeRequired;
		this.projectItems = projectItems;
		if (projectItems.Count > 0)
		{
			nextItem = projectItems[0];
		}
		defaultEncoding = ParserService.DefaultFileEncoding;
	}

	private string GetParseableFileContent(IProject project, string fileName)
	{
		return FileReader.ReadFileContent(fileName, defaultEncoding);
	}

	public bool MoveNext()
	{
		ProjectItem projectItem = nextItem;
		nextItem = ((++index < projectItems.Count) ? projectItems[index] : null);
		if (projectItem == null)
		{
			return false;
		}
		if (projectItem.ItemType != ItemType.Compile)
		{
			return MoveNext();
		}
		string fileContent;
		try
		{
			fileContent = GetFileContent(projectItem);
		}
		catch (FileNotFoundException ex)
		{
			LoggingService.Warn("ParseableFileContentEnumerator: " + ex.Message);
			return MoveNext();
		}
		catch (IOException ex2)
		{
			LoggingService.Warn("ParseableFileContentEnumerator: " + ex2.Message);
			return MoveNext();
		}
		current = new KeyValuePair<string, string>(projectItem.FileName, fileContent);
		return true;
	}

	private string GetFileContent(ProjectItem item)
	{
		try
		{
			string fileName = item.FileName;
			if (IsFileOpen(fileName))
			{
				string fileContentFromOpenFile = GetFileContentFromOpenFile(fileName);
				if (fileContentFromOpenFile != null)
				{
					return fileContentFromOpenFile;
				}
			}
			return GetParseableFileContent(item.Project, fileName);
		}
		catch
		{
			return string.Empty;
		}
	}

	private IViewContent[] GetViewContentCollection()
	{
		return WorkbenchSingleton.Workbench.ViewContentCollection.ToArray();
	}

	private bool IsFileOpen(string fileName)
	{
		if (WorkbenchSingleton.Workbench != null && WorkbenchSingleton.Workbench.ViewContentCollection != null)
		{
			if (viewContentCollection == null)
			{
				viewContentCollection = WorkbenchSingleton.SafeThreadFunction(GetViewContentCollection);
			}
			if (viewContentCollection != null)
			{
				IViewContent[] array = viewContentCollection;
				foreach (IViewContent viewContent in array)
				{
					string text = (viewContent.IsUntitled ? viewContent.UntitledName : viewContent.FileName);
					if (text != null && FileUtility.IsEqualFileName(fileName, text))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private string GetFileContentFromOpenFile(string fileName)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction(GetFileContentFromOpenFile, fileName);
		}
		IWorkbenchWindow openFile = FileService.GetOpenFile(fileName);
		if (openFile != null)
		{
			IViewContent viewContent = openFile.ViewContent;
			if (viewContent is IEditable editable)
			{
				return editable.Text;
			}
		}
		return null;
	}
}

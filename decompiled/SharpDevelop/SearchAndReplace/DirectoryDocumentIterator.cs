using System;
using System.Collections.Generic;
using ICSharpCode.Core;

namespace SearchAndReplace;

public class DirectoryDocumentIterator : InFilesDocumentIterator
{
	private string[] searchDirectory;

	private string[] fileMask;

	private bool searchSubdirectories;

	public DirectoryDocumentIterator(string searchDirectory, string fileMask, bool searchSubdirectories)
	{
		this.searchDirectory = searchDirectory.Split(';');
		if (string.IsNullOrEmpty(fileMask))
		{
			this.fileMask = new string[1] { "*.*" };
		}
		else
		{
			this.fileMask = fileMask.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (this.fileMask.Length == 0)
			{
				this.fileMask = new string[1] { "*.*" };
			}
		}
		this.searchSubdirectories = searchSubdirectories;
		Reset();
	}

	protected override void FillFiles()
	{
		List<string> list = new List<string>();
		try
		{
			string[] array = searchDirectory;
			foreach (string directory in array)
			{
				if (base.ProgressMonitor != null && base.ProgressMonitor.IsCancelled)
				{
					break;
				}
				string[] array2 = fileMask;
				foreach (string filemask in array2)
				{
					if (base.ProgressMonitor != null && base.ProgressMonitor.IsCancelled)
					{
						break;
					}
					list.AddRange(FileUtility.SearchDirectory(directory, filemask, searchSubdirectories));
				}
			}
		}
		catch (UnauthorizedAccessException)
		{
		}
		base.Files = list.ToArray();
	}
}

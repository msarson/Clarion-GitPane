using System;
using System.Collections.Generic;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;
using SearchAndReplace;

namespace SoftVelocity.Common;

internal class RedirectionDocumentIterator : DirectoryDocumentIterator
{
	private string patterns;

	public RedirectionDocumentIterator(string filePatterns)
		: base(".", filePatterns, false)
	{
		patterns = filePatterns;
	}

	protected override void FillFiles()
	{
		List<string> list = new List<string>();
		try
		{
			RedirectionFile val = CommonClarionProject.CurrentRedirectionFile(ProjectService.CurrentProject, ClarionAddins.DotNetPresent);
			string[] array = patterns.Split(';');
			foreach (string text in array)
			{
				if (((InFilesDocumentIterator)this).ProgressMonitor != null && ((InFilesDocumentIterator)this).ProgressMonitor.IsCancelled)
				{
					break;
				}
				Dictionary<string, List<string>> dictionary = val.EvaluatedPaths(text, RedirectionFile.CurrentDirectory);
				if (dictionary == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, List<string>> item in dictionary)
				{
					if (((InFilesDocumentIterator)this).ProgressMonitor != null && ((InFilesDocumentIterator)this).ProgressMonitor.IsCancelled)
					{
						break;
					}
					foreach (string item2 in item.Value)
					{
						if (((InFilesDocumentIterator)this).ProgressMonitor == null || !((InFilesDocumentIterator)this).ProgressMonitor.IsCancelled)
						{
							list.AddRange(FileUtility.SearchDirectory(item2, item.Key, false));
							continue;
						}
						break;
					}
				}
			}
		}
		catch (UnauthorizedAccessException)
		{
		}
		((InFilesDocumentIterator)this).Files = list.ToArray();
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class InFilesDocumentIterator : IDocumentIterator
{
	private string[] files;

	private int curIndex = -1;

	private IProgressNotificationTaskInstance monitor;

	public IProgressNotificationTaskInstance ProgressMonitor
	{
		get
		{
			return monitor;
		}
		set
		{
			monitor = value;
		}
	}

	protected string[] Files
	{
		get
		{
			if (files == null)
			{
				FillFiles();
			}
			return files;
		}
		set
		{
			files = value;
		}
	}

	public ProvidedDocumentInformation Current
	{
		get
		{
			if (curIndex < 0 || curIndex >= files.Length)
			{
				return null;
			}
			string text = Files[curIndex].ToString();
			if (!File.Exists(text) || !SearchReplaceUtilities.IsSearchable(text))
			{
				curIndex++;
				return Current;
			}
			foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				if (item.FileName != null && FileUtility.IsEqualFileName(item.FileName, text) && item is ITextEditorControlProvider)
				{
					IDocument document = ((ITextEditorControlProvider)item).TextEditorControl.Document;
					return new ProvidedDocumentInformation(document, text, 0);
				}
			}
			ITextBufferStrategy textBufferStrategy = null;
			try
			{
				textBufferStrategy = StringTextBufferStrategy.CreateTextBufferFromFile(text);
			}
			catch (Exception)
			{
				return null;
			}
			return new ProvidedDocumentInformation(textBufferStrategy, text, 0);
		}
	}

	public string CurrentFileName
	{
		get
		{
			if (curIndex < 0 || curIndex >= Files.Length)
			{
				return null;
			}
			return Files[curIndex].ToString();
		}
	}

	protected InFilesDocumentIterator()
	{
	}

	public InFilesDocumentIterator(string files)
	{
		this.files = files.Split(';');
		Reset();
	}

	public InFilesDocumentIterator(List<string> files)
	{
		this.files = files.ToArray();
		Reset();
	}

	protected virtual void FillFiles()
	{
		files = new string[0];
	}

	public void Reset()
	{
		curIndex = -1;
	}

	public bool MoveForward()
	{
		return ++curIndex < Files.Length;
	}

	public bool MoveBackward()
	{
		if (curIndex == -1)
		{
			curIndex = Files.Length - 1;
			return true;
		}
		return --curIndex >= -1;
	}
}

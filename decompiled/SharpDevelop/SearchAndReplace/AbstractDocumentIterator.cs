using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public abstract class AbstractDocumentIterator : IDocumentIterator
{
	protected ArrayList files = new ArrayList();

	private int curIndex = -1;

	private Regex[] patterns;

	public string CurrentFileName
	{
		get
		{
			if (curIndex < 0 || curIndex >= files.Count)
			{
				return null;
			}
			return files[curIndex].ToString();
		}
	}

	public ProvidedDocumentInformation Current
	{
		get
		{
			if (curIndex < 0 || curIndex >= files.Count)
			{
				return null;
			}
			string text = files[curIndex].ToString();
			if (!File.Exists(text) || !FileUtility.Matches(text, patterns))
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

	public AbstractDocumentIterator(string filePatterns)
	{
		patterns = FileUtility.ToRegEx(filePatterns);
		Reset();
	}

	public bool MoveForward()
	{
		return ++curIndex < files.Count;
	}

	public bool MoveBackward()
	{
		if (curIndex == -1)
		{
			curIndex = files.Count - 1;
			return true;
		}
		return --curIndex >= -1;
	}

	protected abstract void FillList();

	public void Reset()
	{
		files.Clear();
		FillList();
		curIndex = -1;
	}
}

using System.IO;
using ICSharpCode.Core;

namespace SearchAndReplace;

public sealed class DummyDocumentIterator : IDocumentIterator
{
	private bool invalidDirectory;

	public bool InvalidDirectory
	{
		get
		{
			return invalidDirectory;
		}
		set
		{
			invalidDirectory = value;
		}
	}

	public ProvidedDocumentInformation Current => null;

	public string CurrentFileName => null;

	public DummyDocumentIterator()
	{
	}

	public DummyDocumentIterator(bool invalidDirectory)
	{
		this.invalidDirectory = invalidDirectory;
	}

	public void InvalidDirectoryMessage()
	{
		MessageService.ShowMessageFormatted("${res:Dialog.NewProject.SearchReplace.SearchStringNotFound.Title}", "${res:Dialog.NewProject.SearchReplace.LookIn.DirectoryNotFound}", Path.GetFullPath(SearchOptions.LookIn));
	}

	public bool MoveForward()
	{
		return false;
	}

	public bool MoveBackward()
	{
		return false;
	}

	public void Reset()
	{
	}
}

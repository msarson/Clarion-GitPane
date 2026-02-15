using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class InCurrentDirectory : DirectoryDocumentIterator
{
	public InCurrentDirectory(string fileMask, bool searchSubdirectories)
		: base(GetCurrentDirectory(), fileMask, searchSubdirectories)
	{
	}

	public static string GetCurrentDirectory()
	{
		try
		{
			return Path.GetDirectoryName(Path.GetFullPath(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName));
		}
		catch
		{
			return Application.ExecutablePath;
		}
	}
}

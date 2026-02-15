using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.Redirection;

internal class EditRedirectionFile : RedMenuCommand
{
	protected virtual void AfterLoad()
	{
	}

	protected override void DoRun()
	{
		AfterLoad();
		IWorkbenchWindow val = FileService.OpenFile(redFile.FullName);
		if (val == null)
		{
			MessageBox.Show(ResourceService.GetString("Clarion.Gui.EditRedirectionFile.RedFileNotFound"));
		}
	}
}

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Commands;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.Redirection;

public class OpenFileCommand : AbstractMenuCommand
{
	public override void Run()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		AbstractMenuCommand val = null;
		if (OpenFileCommandUsingRedFile.Value)
		{
			IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
			if (activeWorkbenchWindow == null || !(activeWorkbenchWindow.ViewContent is ITextEditorControlProvider))
			{
				val = (AbstractMenuCommand)(object)new OpenViaRedirectionFile();
			}
			else
			{
				OpenSelectedViaRedirectionFile openSelectedViaRedirectionFile = new OpenSelectedViaRedirectionFile();
				openSelectedViaRedirectionFile.ForceEmptyDialog = true;
				val = (AbstractMenuCommand)(object)openSelectedViaRedirectionFile;
			}
		}
		else
		{
			val = (AbstractMenuCommand)new OpenFile();
		}
		((AbstractCommand)val).Run();
	}
}

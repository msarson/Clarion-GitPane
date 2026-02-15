using System.Drawing.Printing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class Print : AbstractMenuCommand
{
	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null)
		{
			return;
		}
		if (activeWorkbenchWindow.ViewContent is IPrintable)
		{
			PrintDocument printDocument = ((IPrintable)activeWorkbenchWindow.ViewContent).PrintDocument;
			if (printDocument != null)
			{
				using (PrintDialog printDialog = new PrintDialog())
				{
					printDialog.Document = printDocument;
					printDialog.AllowSomePages = true;
					if (printDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
					{
						printDocument.Print();
					}
					return;
				}
			}
			MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Commands.Print.CreatePrintDocumentError}");
		}
		else
		{
			MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Commands.Print.CantPrintWindowContentError}");
		}
	}
}

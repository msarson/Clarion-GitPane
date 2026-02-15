using System.Drawing.Printing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class PrintPreview : AbstractMenuCommand
{
	public override void Run()
	{
		try
		{
			IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
			if (activeWorkbenchWindow == null || !(activeWorkbenchWindow.ViewContent is IPrintable))
			{
				return;
			}
			using PrintDocument printDocument = ((IPrintable)activeWorkbenchWindow.ViewContent).PrintDocument;
			if (printDocument != null)
			{
				PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
				printPreviewDialog.Owner = (Form)WorkbenchSingleton.Workbench;
				printPreviewDialog.TopMost = true;
				printPreviewDialog.Document = printDocument;
				printPreviewDialog.Show();
			}
			else
			{
				MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Commands.Print.CreatePrintDocumentError}");
			}
		}
		catch (InvalidPrinterException)
		{
		}
	}
}

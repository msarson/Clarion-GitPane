using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.Print;

public class FilePrinter : IDisposable
{
	private delegate void JobProc(bool preview);

	private string fileToPrint;

	private string pageTitle;

	private FontInfo fontToPrint;

	private StreamReader streamToPrint;

	private int pageNo;

	private string timeStamp;

	private bool previewCompleted;

	private bool printDlg;

	public bool UsePrintDialog
	{
		get
		{
			return printDlg;
		}
		set
		{
			printDlg = value;
		}
	}

	public FilePrinter(string filename, string title)
	{
		fileToPrint = filename;
		fontToPrint = new FontInfo();
		pageTitle = title;
		UsePrintDialog = true;
	}

	public void Dispose()
	{
		if (fontToPrint != null)
		{
			fontToPrint.Dispose();
		}
		if (streamToPrint != null)
		{
			streamToPrint.Dispose();
		}
	}

	public void Print()
	{
		DoJob(preview: false);
	}

	public void Preview()
	{
		Form form = (Form)(object)WorkbenchSingleton.Workbench;
		if (form.InvokeRequired)
		{
			IAsyncResult asyncResult = form.BeginInvoke(new JobProc(DoJob), true);
			while (!asyncResult.IsCompleted)
			{
				Application.DoEvents();
			}
			form.EndInvoke(asyncResult);
		}
		else
		{
			DoJob(preview: true);
		}
	}

	private void DoJob(bool preview)
	{
		try
		{
			streamToPrint = new StreamReader(fileToPrint);
			PrintDocument printDocument = new PrintDocument();
			printDocument.DocumentName = pageTitle;
			if (UsePrintDialog && !PrinterDialog(printDocument))
			{
				return;
			}
			printDocument.PrintPage += PrintPage;
			try
			{
				timeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
				pageNo = 0;
				if (!preview)
				{
					DoPrint(printDocument);
				}
				else
				{
					DoPreview(printDocument);
				}
			}
			finally
			{
				printDocument.PrintPage -= PrintPage;
				streamToPrint.Close();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	private void DoPrint(PrintDocument pd)
	{
		pd.Print();
	}

	private void DoPreview(PrintDocument pd)
	{
		PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
		try
		{
			printPreviewDialog.Owner = (Form)(object)WorkbenchSingleton.Workbench;
			printPreviewDialog.TopMost = true;
			printPreviewDialog.Document = pd;
			printPreviewDialog.Name = pd.DocumentName;
			printPreviewDialog.Closed += PreviewClosed;
			previewCompleted = false;
			printPreviewDialog.ShowDialog();
			while (!previewCompleted)
			{
				Application.DoEvents();
			}
		}
		finally
		{
			printPreviewDialog.Closed -= PreviewClosed;
			printPreviewDialog.Dispose();
		}
	}

	private void PreviewClosed(object sender, EventArgs args)
	{
		previewCompleted = true;
	}

	private void PrintPage(object sender, PrintPageEventArgs ev)
	{
		Font font = fontToPrint.MakeFont();
		float num = ev.MarginBounds.Left;
		float num2 = ev.MarginBounds.Right;
		float num3 = ev.MarginBounds.Top;
		float x = (num + num2) / 2f;
		pageNo++;
		StringFormat genericDefault = StringFormat.GenericDefault;
		genericDefault.Alignment = StringAlignment.Near;
		ev.Graphics.DrawString(timeStamp, font, Brushes.Black, num, num3, genericDefault);
		genericDefault.Alignment = StringAlignment.Center;
		ev.Graphics.DrawString(pageTitle, font, Brushes.Black, x, num3, genericDefault);
		genericDefault.Alignment = StringAlignment.Far;
		ev.Graphics.DrawString("Page " + pageNo, font, Brushes.Black, num2, num3, genericDefault);
		genericDefault.Alignment = StringAlignment.Near;
		int num4 = (int)Math.Floor((float)ev.MarginBounds.Height / font.GetHeight(ev.Graphics));
		int i = 2;
		ev.HasMorePages = true;
		string text = null;
		for (; i < num4; i++)
		{
			text = streamToPrint.ReadLine();
			if (text == null)
			{
				ev.HasMorePages = false;
				pageNo = 0;
				break;
			}
			float y = num3 + (float)i * font.GetHeight(ev.Graphics);
			ev.Graphics.DrawString(text, font, Brushes.Black, num, y, genericDefault);
		}
	}

	private bool PrinterDialog(PrintDocument pd)
	{
		using PrintDialog printDialog = new PrintDialog();
		printDialog.ShowHelp = true;
		printDialog.AllowCurrentPage = true;
		printDialog.AllowPrintToFile = true;
		printDialog.AllowSelection = true;
		printDialog.UseEXDialog = true;
		printDialog.Document = pd;
		return printDialog.ShowDialog() == DialogResult.OK;
	}
}

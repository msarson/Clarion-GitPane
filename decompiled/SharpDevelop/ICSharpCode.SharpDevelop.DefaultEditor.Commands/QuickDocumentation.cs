using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class QuickDocumentation : AbstractMenuCommand
{
	private class ToolWindowForm : Form
	{
		public ToolWindowForm(TextEditorControl textEditorControl, string html)
		{
			Point screenPosition = textEditorControl.ActiveTextAreaControl.Caret.ScreenPosition;
			Point p = new Point(Math.Min(Math.Max(screenPosition.X, textEditorControl.ActiveTextAreaControl.TextArea.TextView.DrawingPosition.Left), textEditorControl.ActiveTextAreaControl.TextArea.TextView.DrawingPosition.Right), Math.Min(Math.Max(screenPosition.Y, textEditorControl.ActiveTextAreaControl.TextArea.TextView.DrawingPosition.Top), textEditorControl.ActiveTextAreaControl.TextArea.TextView.DrawingPosition.Bottom));
			base.Location = textEditorControl.ActiveTextAreaControl.TextArea.PointToScreen(p);
			WebBrowser webBrowser = new WebBrowser();
			webBrowser.DocumentText = html;
			webBrowser.Dock = DockStyle.Fill;
			webBrowser.Navigating += BrowserNavigateCancel;
			base.Controls.Add(webBrowser);
			base.ShowInTaskbar = false;
			base.FormBorderStyle = FormBorderStyle.None;
			base.StartPosition = FormStartPosition.Manual;
		}

		private void BrowserNavigateCancel(object sender, WebBrowserNavigatingEventArgs e)
		{
			e.Cancel = true;
		}

		protected override void OnDeactivate(EventArgs e)
		{
			Close();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}
	}

	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null || !(activeWorkbenchWindow.ViewContent is ITextEditorControlProvider))
		{
			return;
		}
		TextEditorControl textEditorControl = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
		int num = textEditorControl.Document.GetLineNumberForOffset(textEditorControl.ActiveTextAreaControl.Caret.Offset);
		int i = num;
		LineSegment lineSegment = textEditorControl.Document.GetLineSegment(num);
		string text = textEditorControl.Document.GetText(lineSegment.Offset, lineSegment.Length).Trim();
		if (!text.StartsWith("///") && !text.StartsWith("'''"))
		{
			return;
		}
		while (num > 0)
		{
			lineSegment = textEditorControl.Document.GetLineSegment(num);
			text = textEditorControl.Document.GetText(lineSegment.Offset, lineSegment.Length).Trim();
			if (!text.StartsWith("///") && !text.StartsWith("'''"))
			{
				break;
			}
			num--;
		}
		for (; i < textEditorControl.Document.TotalNumberOfLines - 1; i++)
		{
			lineSegment = textEditorControl.Document.GetLineSegment(i);
			text = textEditorControl.Document.GetText(lineSegment.Offset, lineSegment.Length).Trim();
			if (!text.StartsWith("///") && !text.StartsWith("'''"))
			{
				break;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int j = num + 1; j < i; j++)
		{
			lineSegment = textEditorControl.Document.GetLineSegment(j);
			text = textEditorControl.Document.GetText(lineSegment.Offset, lineSegment.Length).Trim();
			if (text.StartsWith("///"))
			{
				stringBuilder.Append(text.Substring(3));
			}
			else
			{
				stringBuilder.Append(text.Substring(2));
			}
			stringBuilder.Append('\n');
		}
		string xml = "<member>" + stringBuilder.ToString() + "</member>";
		string html = string.Empty;
		try
		{
			XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
			xslCompiledTransform.Load(Path.Combine(Path.Combine(PropertyService.DataDirectory, "ConversionStyleSheets"), "ShowXmlDocumentation.xsl"));
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			StringBuilder stringBuilder2 = new StringBuilder();
			TextWriter textWriter = new StringWriter(stringBuilder2);
			XmlWriter xmlWriter = new XmlTextWriter(textWriter);
			xslCompiledTransform.Transform(xmlDocument, xmlWriter);
			html = stringBuilder2.ToString();
			textWriter.Close();
			xmlWriter.Close();
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString());
		}
		new ToolWindowForm(textEditorControl, html).Show();
	}
}

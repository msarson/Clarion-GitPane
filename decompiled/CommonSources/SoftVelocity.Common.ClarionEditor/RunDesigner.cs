using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Windows.Forms;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.ClarionEditor.Dialogs;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Generator.Editor;

namespace SoftVelocity.Common.ClarionEditor;

public class RunDesigner : AbstractMenuCommand
{
	public static void ShowDesigner(IBaseViewContent viewContent)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		bool isTemplate = false;
		TextEditorDisplayBindingWrapper val = (TextEditorDisplayBindingWrapper)(object)((viewContent is TextEditorDisplayBindingWrapper) ? viewContent : null);
		if (val == null || !(val is IStructureDesignerCompatible structureDesignerCompatible))
		{
			return;
		}
		LineSegment lineSegment = ((TextEditorControlBase)val.TextEditorControl).ActiveTextAreaControl.Document.GetLineSegment(((TextEditorControlBase)val.TextEditorControl).ActiveTextAreaControl.Caret.Line);
		bool flag = true;
		if (lineSegment.Words != null)
		{
			foreach (TextWord word in lineSegment.Words)
			{
				if ((int)word.Type == 0)
				{
					flag = false;
					break;
				}
			}
		}
		string fileName;
		string fileContent;
		int line;
		int column;
		if (flag)
		{
			string template = string.Empty;
			if (!GetNewReportTemplate(structureDesignerCompatible, ref template))
			{
				return;
			}
			fileName = ((AbstractViewContent)val).FileName ?? ((AbstractViewContent)val).TitleName ?? ((AbstractViewContent)val).UntitledName;
			fileContent = "\tMEMBER\r\n" + template;
			line = 2;
			column = 0;
			isTemplate = true;
		}
		else
		{
			fileName = ((AbstractViewContent)val).FileName ?? ((AbstractViewContent)val).TitleName ?? ((AbstractViewContent)val).UntitledName;
			fileContent = structureDesignerCompatible.GetContentForDesigner();
			line = ((TextEditorControlBase)val.TextEditorControl).ActiveTextAreaControl.Caret.Line + 1;
			column = ((TextEditorControlBase)val.TextEditorControl).ActiveTextAreaControl.Caret.Column + 1;
		}
		Cursor.Current = Cursors.WaitCursor;
		ClarionType structureType;
		CompilerResults cr;
		ReportDeclaration reportDeclaration = structureDesignerCompatible.ParseStructure(fileName, fileContent, line, column, out structureType, out cr);
		Cursor.Current = Cursors.Default;
		if (reportDeclaration == null && cr.Errors.Count == 0)
		{
			return;
		}
		ReportValidator.Validate(reportDeclaration, cr);
		IViewContent val2 = (IViewContent)((viewContent.WorkbenchWindow == null) ? ((object)((viewContent is IViewContent) ? viewContent : null)) : ((object)viewContent.WorkbenchWindow.ViewContent));
		if (val2 == null)
		{
			return;
		}
		if (val is CommonGenEditor)
		{
			CommonClarionGenDesignerView commonClarionGenDesignerView = null;
			for (int i = 0; i < val2.SecondaryViewContents.Count; i++)
			{
				if (val2.SecondaryViewContents[i] is CommonClarionGenDesignerView)
				{
					commonClarionGenDesignerView = (CommonClarionGenDesignerView)(object)val2.SecondaryViewContents[i];
					if (!commonClarionGenDesignerView.IsAppGenDesigner)
					{
						break;
					}
				}
			}
			if (commonClarionGenDesignerView != null)
			{
				((CommonGenEditor)(object)val).ShowDesigner(commonClarionGenDesignerView, reportDeclaration, cr, structureType != ClarionType.REPORT, structureType != ClarionType.APPLICATION, isTemplate);
			}
			return;
		}
		CommonClarionDesignerView commonClarionDesignerView = null;
		for (int j = 0; j < val2.SecondaryViewContents.Count; j++)
		{
			if (val2.SecondaryViewContents[j] is CommonClarionDesignerView)
			{
				commonClarionDesignerView = (CommonClarionDesignerView)(object)val2.SecondaryViewContents[j];
				break;
			}
		}
		commonClarionDesignerView?.ShowDesigner(reportDeclaration, cr, structureType != ClarionType.REPORT, structureType != ClarionType.APPLICATION, isTemplate);
	}

	public override void Run()
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent != null)
		{
			IBaseViewContent activeViewContent = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent;
			ShowDesigner(activeViewContent);
		}
	}

	private static bool GetNewReportTemplate(IStructureDesignerCompatible editor, ref string template)
	{
		StreamReader streamReader = null;
		bool flag;
		try
		{
			string path = CommonClarionProject.CurrentRedirectionFile(null, editor.IsWin).OpenName(editor.GetTemplatesFileName(), RedirectionFile.CurrentDirectory);
			streamReader = new StreamReader(path);
			flag = true;
		}
		catch (Exception)
		{
			flag = false;
		}
		if (!flag)
		{
			using (NewDefaultStructureDlg newDefaultStructureDlg = new NewDefaultStructureDlg())
			{
				if (newDefaultStructureDlg.ShowDialog() == DialogResult.OK)
				{
					template = newDefaultStructureDlg.ReturnStructure;
					return true;
				}
				return false;
			}
		}
		try
		{
			using NewReportDialog newReportDialog = new NewReportDialog(streamReader, isAll: true);
			newReportDialog.ShowDialog();
			if (newReportDialog.DialogResult == DialogResult.OK)
			{
				template = newReportDialog.NewReport;
				return true;
			}
			return false;
		}
		finally
		{
			streamReader?.Dispose();
		}
	}
}

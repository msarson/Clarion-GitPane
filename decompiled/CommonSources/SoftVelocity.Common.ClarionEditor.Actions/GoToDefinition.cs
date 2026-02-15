using System;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.Parser.IDE.Ast;
using SoftVelocity.Generator;
using SoftVelocity.Generator.Editor;

namespace SoftVelocity.Common.ClarionEditor.Actions;

public class GoToDefinition : AbstractEditAction
{
	public override void Execute(TextArea textArea)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		TextEditorControl motherTextEditorControl = textArea.MotherTextEditorControl;
		IDocument document = ((TextEditorControlBase)motherTextEditorControl).Document;
		string textContent = document.TextContent;
		int num = document.GetLineNumberForOffset(((TextEditorControlBase)motherTextEditorControl).ActiveTextAreaControl.Caret.Offset) + 1;
		int num2 = ((TextEditorControlBase)motherTextEditorControl).ActiveTextAreaControl.Caret.Offset - document.GetLineSegment(num - 1).Offset + 1;
		IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(((TextEditorControlBase)motherTextEditorControl).FileName);
		if (expressionFinder == null)
		{
			return;
		}
		ExpressionResult val = expressionFinder.FindFullExpression(textContent, ((TextEditorControlBase)motherTextEditorControl).ActiveTextAreaControl.Caret.Offset);
		if (val.Expression == null || val.Expression.Length == 0)
		{
			return;
		}
		ResolveResult val2 = ParserService.Resolve(val, num, num2, ((TextEditorControlBase)motherTextEditorControl).FileName, textContent);
		if (val2 == null || !(val2 is MemberResolveResult) || !(((MemberResolveResult)val2).ResolvedMember is ClaMethod))
		{
			return;
		}
		ClaMethod claMethod = (ClaMethod)(object)((MemberResolveResult)val2).ResolvedMember;
		string text = null;
		int num3 = 0;
		if (claMethod.ClaBodyRegion.IsEmpty)
		{
			if (!claMethod.IsExternal || !(claMethod.DeclaringType is ClaGlobalClass))
			{
				return;
			}
			object project = claMethod.DeclaringType.CompilationUnit.ProjectContent.Project;
			IProject val3 = (IProject)((project is IProject) ? project : null);
			if (CommonClarionProject.CurrentRedirectionFile(val3).Exists(claMethod.ExternalModuleName, val3.Directory))
			{
				ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(CommonClarionProject.CurrentRedirectionFile(val3).OpenName(claMethod.ExternalModuleName, val3.Directory));
				if (parseInformationIfExist != null && parseInformationIfExist.BestCompilationUnit is ClaCompilationUnit { GlobalClass: { } globalClass })
				{
					foreach (ClaMethod method in globalClass.Methods)
					{
						if (method.CompareTo((IMethod)(object)claMethod, fullName: false) == 0 && !method.ClaBodyRegion.IsEmpty)
						{
							text = method.ClaBodyRegion.FileName;
							num3 = method.ClaBodyRegion.BeginLine - 1;
							break;
						}
					}
				}
			}
		}
		else
		{
			text = claMethod.ClaBodyRegion.FileName;
			num3 = claMethod.ClaBodyRegion.BeginLine - 1;
		}
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (AppGenEditorsService.IsRegistered(text))
		{
			CommonGenEditor pweeEditor = AppGenEditorsService.GetPweeEditor(text);
			if (pweeEditor != null)
			{
				((IPositionable)pweeEditor).JumpTo(Math.Max(0, num3), 0);
			}
			return;
		}
		try
		{
			if (num3 < 0)
			{
				FileService.OpenFile(text);
			}
			else
			{
				FileService.JumpToFilePosition(text, num3, 0);
			}
		}
		catch (Exception)
		{
		}
	}
}

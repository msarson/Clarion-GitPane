using System;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.Parser.IDE.Ast;
using SoftVelocity.Generator;
using SoftVelocity.Generator.Editor;

namespace SoftVelocity.Common.ClarionEditor.Actions;

public class GoToDeclaration : AbstractEditAction
{
	public override void Execute(TextArea textArea)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
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
		if (val2 == null)
		{
			return;
		}
		string text = null;
		int num3 = -1;
		if (val2 is MemberResolveResult)
		{
			if (((MemberResolveResult)val2).ResolvedMember is ClaAbstractMember)
			{
				ClaAbstractMember claAbstractMember = (ClaAbstractMember)(object)((MemberResolveResult)val2).ResolvedMember;
				if (claAbstractMember.ClaRegion.IsEmpty)
				{
					if (claAbstractMember is ClaMethod && claAbstractMember.DeclaringType != null)
					{
						ClaMethod value = (ClaMethod)claAbstractMember;
						if (claAbstractMember.DeclaringType.CompilationUnit.ProjectContent.Project is CommonClarionProject commonClarionProject)
						{
							ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(commonClarionProject.ProgramFileName);
							if (parseInformationIfExist != null && parseInformationIfExist.BestCompilationUnit is ClaCompilationUnit { GlobalClass: { } globalClass })
							{
								foreach (ClaMethod method in globalClass.Methods)
								{
									if (method.CompareTo((IMethod)(object)value, fullName: false) == 0 && method.ClaBodyRegion.IsEmpty)
									{
										text = method.ClaRegion.FileName;
										num3 = method.ClaRegion.BeginLine - 1;
										break;
									}
								}
							}
						}
					}
				}
				else
				{
					text = claAbstractMember.ClaRegion.FileName;
					num3 = claAbstractMember.ClaRegion.BeginLine - 1;
				}
			}
		}
		else
		{
			if (val2 is MethodResolveResult)
			{
				return;
			}
			if (val2 is LocalResolveResult)
			{
				if (((LocalResolveResult)val2).Field is ClaField)
				{
					ClaField claField = (ClaField)(object)((LocalResolveResult)val2).Field;
					text = claField.ClaRegion.FileName;
					num3 = claField.ClaRegion.BeginLine - 1;
				}
			}
			else
			{
				if (val2.ResolvedType == null)
				{
					return;
				}
				if (val2.ResolvedType.GetUnderlyingClass() is ClaClass)
				{
					ClaClass claClass = (ClaClass)(object)val2.ResolvedType.GetUnderlyingClass();
					if (claClass is ClaGlobalClass)
					{
						return;
					}
					text = claClass.ClaRegion.FileName;
					num3 = claClass.ClaRegion.BeginLine - 1;
				}
			}
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

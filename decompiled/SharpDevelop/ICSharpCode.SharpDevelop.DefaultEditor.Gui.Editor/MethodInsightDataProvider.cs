using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.InsightWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class MethodInsightDataProvider : IInsightDataProvider
{
	private string fileName;

	private IDocument document;

	private TextArea textArea;

	protected List<IMethodOrProperty> methods = new List<IMethodOrProperty>();

	private int defaultIndex = -1;

	private int lookupOffset;

	private bool setupOnlyOnce;

	private int initialOffset;

	public List<IMethodOrProperty> Methods => methods;

	public int InsightDataCount => methods.Count;

	public int DefaultIndex
	{
		get
		{
			return defaultIndex;
		}
		set
		{
			defaultIndex = value;
		}
	}

	public string GetInsightData(int number)
	{
		IMember member = methods[number];
		IAmbience currentAmbience = AmbienceService.CurrentAmbience;
		currentAmbience.ConversionFlags = ConversionFlags.StandardConversionFlags;
		string documentation = member.Documentation;
		string text = ((member is IMethod) ? currentAmbience.Convert(member as IMethod) : ((!(member is IProperty)) ? member.ToString() : currentAmbience.Convert(member as IProperty)));
		return text + "\n" + CodeCompletionData.GetDocumentation(documentation);
	}

	public MethodInsightDataProvider()
	{
		lookupOffset = -1;
	}

	public MethodInsightDataProvider(int lookupOffset, bool setupOnlyOnce)
	{
		this.lookupOffset = lookupOffset;
		this.setupOnlyOnce = setupOnlyOnce;
	}

	public void SetupDataProvider(string fileName, TextArea textArea)
	{
		if (setupOnlyOnce && this.textArea != null)
		{
			return;
		}
		IDocument document = textArea.Document;
		this.fileName = fileName;
		this.document = document;
		this.textArea = textArea;
		int num = (initialOffset = ((lookupOffset < 0) ? textArea.Caret.Offset : lookupOffset));
		ExpressionResult expressionResult = ParserService.GetExpressionFinder(fileName)?.FindExpression(textArea.Document.TextContent, num - 1) ?? new ExpressionResult(TextUtilities.GetExpressionBeforeOffset(textArea, num));
		if (expressionResult.Expression == null)
		{
			return;
		}
		expressionResult.Expression = expressionResult.Expression.Trim();
		if (LoggingService.IsDebugEnabled)
		{
			if (expressionResult.Context == ExpressionContext.Default)
			{
				LoggingService.DebugFormatted("ShowInsight for >>{0}<<", expressionResult.Expression);
			}
			else
			{
				LoggingService.DebugFormatted("ShowInsight for >>{0}<<, context={1}", expressionResult.Expression, expressionResult.Context);
			}
		}
		int num2 = document.GetLineNumberForOffset(num) + 1;
		int caretColumn = num - document.GetLineSegment(num2 - 1).Offset + 1;
		SetupDataProvider(fileName, document, expressionResult, num2, caretColumn);
	}

	protected virtual void SetupDataProvider(string fileName, IDocument document, ExpressionResult expressionResult, int caretLineNumber, int caretColumn)
	{
		bool flag = false;
		if (expressionResult.Context.IsAttributeContext)
		{
			flag = true;
		}
		else if (expressionResult.Context.IsObjectCreation)
		{
			flag = true;
			expressionResult.Context = ExpressionContext.Type;
		}
		ResolveResult resolveResult = ParserService.Resolve(expressionResult, caretLineNumber, caretColumn, fileName, document.TextContent);
		LanguageProperties language = ParserService.CurrentProjectContent.Language;
		TypeResolveResult typeResolveResult = resolveResult as TypeResolveResult;
		if (typeResolveResult == null && language.AllowObjectConstructionOutsideContext && resolveResult is MixedResolveResult)
		{
			typeResolveResult = (resolveResult as MixedResolveResult).TypeResult;
		}
		if (typeResolveResult != null && !flag && language.AllowObjectConstructionOutsideContext)
		{
			flag = true;
		}
		if (flag)
		{
			if (typeResolveResult == null)
			{
				return;
			}
			foreach (IMethod method2 in typeResolveResult.ResolvedType.GetMethods())
			{
				if (method2.IsConstructor && !method2.IsStatic)
				{
					methods.Add(method2);
				}
			}
			if (methods.Count == 0 && typeResolveResult.ResolvedClass != null && !typeResolveResult.ResolvedClass.IsAbstract && !typeResolveResult.ResolvedClass.IsStatic)
			{
				methods.Add(Constructor.CreateDefault(typeResolveResult.ResolvedClass));
			}
		}
		else
		{
			if (!(resolveResult is MethodResolveResult methodResolveResult))
			{
				return;
			}
			bool isClassInInheritanceTree = false;
			if (methodResolveResult.CallingClass != null)
			{
				isClassInInheritanceTree = methodResolveResult.CallingClass.IsTypeInInheritanceTree(methodResolveResult.ContainingType.GetUnderlyingClass());
			}
			foreach (IMethod method3 in methodResolveResult.ContainingType.GetMethods())
			{
				if (language.NameComparer.Equals(method3.Name, methodResolveResult.Name) && method3.IsAccessible(methodResolveResult.CallingClass, isClassInInheritanceTree))
				{
					methods.Add(method3);
				}
			}
			if (methods.Count != 0 || methodResolveResult.CallingClass == null || !language.SupportsExtensionMethods)
			{
				return;
			}
			ArrayList arrayList = new ArrayList();
			ResolveResult.AddExtensions(language, arrayList, methodResolveResult.CallingClass, methodResolveResult.ContainingType);
			foreach (IMethodOrProperty item in arrayList)
			{
				if (language.NameComparer.Equals(item.Name, methodResolveResult.Name) && item is IMethod)
				{
					IMethod method = (IMethod)item.Clone();
					method.Parameters.RemoveAt(0);
					methods.Add(method);
				}
			}
		}
	}

	public bool CaretOffsetChanged()
	{
		bool flag = textArea.Caret.Offset <= initialOffset;
		int num = 0;
		int num2 = 0;
		if (!flag)
		{
			bool flag2 = false;
			bool flag3 = false;
			for (int i = initialOffset; i < Math.Min(textArea.Caret.Offset, document.TextLength); i++)
			{
				switch (document.GetCharAt(i))
				{
				case '\'':
					flag2 = !flag2;
					break;
				case '(':
					if (!flag2 && !flag3)
					{
						num++;
					}
					break;
				case ')':
					if (!flag2 && !flag3)
					{
						num--;
					}
					if (num <= 0)
					{
						return true;
					}
					break;
				case '"':
					flag3 = !flag3;
					break;
				case '}':
					if (!flag2 && !flag3)
					{
						num2--;
					}
					if (num2 < 0)
					{
						return true;
					}
					break;
				case '{':
					if (!flag2 && !flag3)
					{
						num2++;
					}
					break;
				case ';':
					if (!flag2 && !flag3)
					{
						return true;
					}
					break;
				}
			}
		}
		return flag;
	}

	public bool CharTyped()
	{
		return false;
	}
}

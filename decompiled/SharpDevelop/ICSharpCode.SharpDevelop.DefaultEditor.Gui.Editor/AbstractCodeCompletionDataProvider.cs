using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public abstract class AbstractCodeCompletionDataProvider : AbstractCompletionDataProvider
{
	protected Hashtable insertedElements = new Hashtable();

	protected Hashtable insertedPropertiesElements = new Hashtable();

	protected Hashtable insertedEventElements = new Hashtable();

	protected int caretLineNumber;

	protected int caretColumn;

	protected string fileName;

	protected List<ICompletionData> completionData;

	protected ExpressionContext overrideContext;

	public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
	{
		completionData = new List<ICompletionData>();
		this.fileName = fileName;
		IDocument document = textArea.Document;
		caretLineNumber = document.GetLineNumberForOffset(textArea.Caret.Offset) + 1;
		caretColumn = textArea.Caret.Offset - document.GetLineSegment(caretLineNumber - 1).Offset + 1;
		GenerateCompletionData(textArea, charTyped);
		return completionData.ToArray();
	}

	protected ExpressionResult GetExpression(TextArea textArea)
	{
		IDocument document = textArea.Document;
		IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(fileName);
		if (expressionFinder == null)
		{
			return new ExpressionResult(TextUtilities.GetExpressionBeforeOffset(textArea, textArea.Caret.Offset));
		}
		ExpressionResult result = expressionFinder.FindExpression(document.GetText(0, textArea.Caret.Offset), textArea.Caret.Offset - 1);
		if (overrideContext != null)
		{
			result.Context = overrideContext;
		}
		return result;
	}

	protected abstract void GenerateCompletionData(TextArea textArea, char charTyped);

	protected virtual void AddResolveResults(ICollection list, ExpressionContext context)
	{
		if (list == null)
		{
			return;
		}
		completionData.Capacity += list.Count;
		CodeCompletionData codeCompletionData = null;
		foreach (object item in list)
		{
			if (context == null || context.ShowEntry(item))
			{
				CodeCompletionData codeCompletionData2 = CreateItem(item, context);
				if (object.Equals(item, context.SuggestedItem))
				{
					codeCompletionData = codeCompletionData2;
				}
				if (codeCompletionData2 != null)
				{
					completionData.Add(codeCompletionData2);
				}
			}
		}
		if (context.SuggestedItem == null)
		{
			return;
		}
		if (codeCompletionData == null)
		{
			codeCompletionData = CreateItem(context.SuggestedItem, context);
			if (codeCompletionData != null)
			{
				completionData.Add(codeCompletionData);
			}
		}
		if (codeCompletionData != null)
		{
			completionData.Sort();
			base.DefaultIndex = completionData.IndexOf(codeCompletionData);
		}
	}

	protected virtual CodeCompletionData CreateItem(object o, ExpressionContext context)
	{
		if (o is string)
		{
			return new CodeCompletionData(o.ToString(), 3);
		}
		if (o is IClass)
		{
			return new CodeCompletionData((IClass)o);
		}
		if (o is IProperty)
		{
			IProperty property = (IProperty)o;
			if (property.Name != null && insertedPropertiesElements[property.Name] == null)
			{
				insertedPropertiesElements[property.Name] = property;
				return new CodeCompletionData(property);
			}
		}
		else if (o is IMethod)
		{
			IMethod method = (IMethod)o;
			if (method.Name != null && !method.IsConstructor)
			{
				CodeCompletionData codeCompletionData = new CodeCompletionData(method);
				if (insertedElements[method.Name] == null)
				{
					insertedElements[method.Name] = codeCompletionData;
					return codeCompletionData;
				}
				((CodeCompletionData)insertedElements[method.Name]).Overloads++;
			}
		}
		else
		{
			if (o is IField)
			{
				return new CodeCompletionData((IField)o);
			}
			if (!(o is IEvent))
			{
				throw new ApplicationException("Unknown object: " + o);
			}
			IEvent obj = (IEvent)o;
			if (obj.Name != null && insertedEventElements[obj.Name] == null)
			{
				insertedEventElements[obj.Name] = obj;
				return new CodeCompletionData(obj);
			}
		}
		return null;
	}

	protected void AddResolveResults(ResolveResult results, ExpressionContext context)
	{
		insertedElements.Clear();
		insertedPropertiesElements.Clear();
		insertedEventElements.Clear();
		if (results != null)
		{
			AddResolveResults(results.GetCompletionData(ParserService.CurrentProjectContent), context);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public abstract class ClaAbstractCtrlSpaceCompletionDataProvider : ClaAbstractCodeCompletionDataProvider
{
	public class CodeSnippet
	{
		public string codeSnippet;

		public string description;

		public CodeSnippet(string codeSnippet, string description)
		{
			this.codeSnippet = codeSnippet;
			this.description = description;
		}
	}

	public class Keyword
	{
		public string keyword;

		public Keyword(string keyword)
		{
			this.keyword = keyword;
		}
	}

	private bool forceNewExpression;

	protected abstract bool AddCodeSnippets { get; }

	public bool ForceNewExpression
	{
		get
		{
			return forceNewExpression;
		}
		set
		{
			forceNewExpression = value;
		}
	}

	protected abstract Keyword[] GetKeywordsList();

	protected ClaAbstractCtrlSpaceCompletionDataProvider(bool mergeOverriddenMethods)
		: base(mergeOverriddenMethods)
	{
	}

	protected ClaAbstractCtrlSpaceCompletionDataProvider(bool mergeOverriddenMethods, ExpressionContext overrideContext)
		: base(mergeOverriddenMethods)
	{
		((AbstractCodeCompletionDataProvider)this).overrideContext = overrideContext;
	}

	public override CompletionDataProviderKeyResult ProcessKey(char key)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (key == ':')
		{
			((AbstractCompletionDataProvider)this).InsertSpace = false;
			return (CompletionDataProviderKeyResult)0;
		}
		return base.ProcessKey(key);
	}

	protected override void AddResolveResults(ICollection list, ExpressionContext context)
	{
		if (list == null)
		{
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		((AbstractCodeCompletionDataProvider)this).completionData.Capacity += list.Count;
		CodeCompletionData val = null;
		foreach (object item2 in list)
		{
			if (context != null && !context.ShowEntry(item2))
			{
				continue;
			}
			CodeCompletionData val2 = ((AbstractCodeCompletionDataProvider)this).CreateItem(item2, context);
			if (object.Equals(item2, context.SuggestedItem))
			{
				val = val2;
			}
			if (val2 != null)
			{
				((AbstractCodeCompletionDataProvider)this).completionData.Add((ICompletionData)(object)val2);
			}
			val2 = CreatePre(item2, context);
			if (val2 != null && !dictionary.ContainsKey(val2.Text))
			{
				((AbstractCodeCompletionDataProvider)this).completionData.Add((ICompletionData)(object)val2);
				dictionary.Add(val2.Text, string.Empty);
			}
			if (!(item2 is ClaClass) || ((ClaClass)item2).ClarionType != ClarionType.ITEMIZE)
			{
				continue;
			}
			ClaClass claClass = (ClaClass)item2;
			if (string.IsNullOrEmpty(claClass.PreName))
			{
				continue;
			}
			string text = claClass.PreName + ":";
			foreach (ClaField field in claClass.Fields)
			{
				ClaEquate claEquate = new ClaEquate(text + field.Name, ClaDomRegion.Empty, null);
				claEquate.SetDeclarationText(field.DeclarationText, cutLabel: true);
				CodeCompletionData item = ((AbstractCodeCompletionDataProvider)this).CreateItem((object)claEquate, context);
				((AbstractCodeCompletionDataProvider)this).completionData.Add((ICompletionData)(object)item);
			}
		}
		if (context.SuggestedItem == null)
		{
			return;
		}
		if (val == null)
		{
			val = ((AbstractCodeCompletionDataProvider)this).CreateItem(context.SuggestedItem, context);
			if (val != null)
			{
				((AbstractCodeCompletionDataProvider)this).completionData.Add((ICompletionData)(object)val);
			}
		}
		if (val != null)
		{
			((AbstractCodeCompletionDataProvider)this).completionData.Sort();
			((AbstractCompletionDataProvider)this).DefaultIndex = ((AbstractCodeCompletionDataProvider)this).completionData.IndexOf((ICompletionData)(object)val);
		}
	}

	protected override CodeCompletionData CreateItem(object o, ExpressionContext context)
	{
		bool isAttributeContext = context.IsAttributeContext;
		if (o is CodeSnippet)
		{
			if (isAttributeContext)
			{
				return null;
			}
			return (CodeCompletionData)(object)new ClaCodeCompletionData(ta, (CodeSnippet)o);
		}
		if (o is Keyword)
		{
			if (isAttributeContext)
			{
				return null;
			}
			return (CodeCompletionData)(object)new ClaCodeCompletionData(ta, (Keyword)o);
		}
		ClaCodeCompletionData claCodeCompletionData = (ClaCodeCompletionData)(object)base.CreateItem(o, context);
		if (claCodeCompletionData != null && context.IsAttributeContext)
		{
			if (((CodeCompletionData)claCodeCompletionData).Class != null && ((IDecoration)((CodeCompletionData)claCodeCompletionData).Class).IsAbstract)
			{
				return null;
			}
			if (((CodeCompletionData)claCodeCompletionData).Text.EndsWith("Attribute"))
			{
				((CodeCompletionData)claCodeCompletionData).Text = ((CodeCompletionData)claCodeCompletionData).Text.Substring(0, ((CodeCompletionData)claCodeCompletionData).Text.Length - 9);
			}
		}
		return (CodeCompletionData)(object)claCodeCompletionData;
	}

	protected virtual CodeCompletionData CreatePre(object o, ExpressionContext context)
	{
		if (context.IsAttributeContext || context.IsObjectCreation)
		{
			return null;
		}
		if (o is ClaClass)
		{
			ClaClass claClass = (ClaClass)o;
			if (!string.IsNullOrEmpty(claClass.PreName) && !claClass.Name.Equals(claClass.PreName, StringComparison.InvariantCultureIgnoreCase))
			{
				return (CodeCompletionData)(object)new ClaPreCompletionData(ta, claClass.PreName, 13);
			}
		}
		return null;
	}

	protected override void GenerateCompletionData(TextArea textArea, char charTyped)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(((AbstractCodeCompletionDataProvider)this).fileName))
		{
			return;
		}
		if (ForceNewExpression)
		{
			((AbstractCompletionDataProvider)this).preSelection = "";
			if (charTyped != 0)
			{
				((AbstractCompletionDataProvider)this).preSelection = null;
			}
			ExpressionContext val = ((AbstractCodeCompletionDataProvider)this).overrideContext;
			if (val == null)
			{
				val = ExpressionContext.Default;
			}
			ArrayList arrayList = new ArrayList(GetKeywordsList());
			arrayList.AddRange(GetCodeSnippetsList(((TextEditorControlBase)textArea.MotherTextEditorControl).FileName));
			arrayList.AddRange(ParserService.CtrlSpace(((AbstractCodeCompletionDataProvider)this).caretLineNumber, ((AbstractCodeCompletionDataProvider)this).caretColumn, ((AbstractCodeCompletionDataProvider)this).fileName, textArea.Document.TextContent, val));
			((AbstractCodeCompletionDataProvider)this).AddResolveResults((ICollection)arrayList, val);
			return;
		}
		ExpressionResult expression = ((AbstractCodeCompletionDataProvider)this).GetExpression(textArea);
		string expression2 = expression.Expression;
		((AbstractCompletionDataProvider)this).preSelection = null;
		if (expression2 == null || expression2.Length == 0 || char.IsWhiteSpace(expression2[expression2.Length - 1]))
		{
			((AbstractCompletionDataProvider)this).preSelection = "";
			if (charTyped != 0)
			{
				((AbstractCompletionDataProvider)this).preSelection = null;
			}
			ArrayList arrayList2 = new ArrayList(GetKeywordsList());
			arrayList2.AddRange(GetCodeSnippetsList(((TextEditorControlBase)textArea.MotherTextEditorControl).FileName));
			arrayList2.AddRange(ParserService.CtrlSpace(((AbstractCodeCompletionDataProvider)this).caretLineNumber, ((AbstractCodeCompletionDataProvider)this).caretColumn, ((AbstractCodeCompletionDataProvider)this).fileName, textArea.Document.TextContent, expression.Context));
			((AbstractCodeCompletionDataProvider)this).AddResolveResults((ICollection)arrayList2, expression.Context);
			return;
		}
		int num = expression2.LastIndexOf('.');
		if (num > 0)
		{
			((AbstractCompletionDataProvider)this).preSelection = expression2.Substring(num + 1);
			expression.Expression = expression2.Substring(0, num);
			if (charTyped != 0)
			{
				((AbstractCompletionDataProvider)this).preSelection = null;
			}
			((CodeCompletionDataProvider)this).GenerateCompletionData(textArea, expression);
			return;
		}
		((AbstractCompletionDataProvider)this).preSelection = expression2;
		if (charTyped != 0)
		{
			((AbstractCompletionDataProvider)this).preSelection = null;
		}
		ArrayList arrayList3 = new ArrayList(GetKeywordsList());
		arrayList3.AddRange(GetCodeSnippetsList(((TextEditorControlBase)textArea.MotherTextEditorControl).FileName));
		arrayList3.AddRange(ParserService.CtrlSpace(((AbstractCodeCompletionDataProvider)this).caretLineNumber, ((AbstractCodeCompletionDataProvider)this).caretColumn, ((AbstractCodeCompletionDataProvider)this).fileName, textArea.Document.TextContent, expression.Context));
		((AbstractCodeCompletionDataProvider)this).AddResolveResults((ICollection)arrayList3, expression.Context);
	}

	public ArrayList GetCodeSnippetsList(string fileName)
	{
		ArrayList arrayList = new ArrayList();
		if (string.IsNullOrEmpty(fileName) || !SharpDevelopTextEditorProperties.Instance.AutoInsertTemplates)
		{
			return arrayList;
		}
		if (!AddCodeSnippets)
		{
			return arrayList;
		}
		CodeTemplateGroup templateGroupPerFilename = CodeTemplateLoader.GetTemplateGroupPerFilename(fileName);
		if (templateGroupPerFilename != null)
		{
			foreach (CodeTemplate template in templateGroupPerFilename.Templates)
			{
				arrayList.Add(new CodeSnippet(template.Shortcut, template.Description + Environment.NewLine + template.Text));
			}
		}
		return arrayList;
	}
}

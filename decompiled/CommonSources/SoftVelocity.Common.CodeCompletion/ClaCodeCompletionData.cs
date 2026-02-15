using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using SoftVelocity.Common.ClarionEditor;
using SoftVelocity.Common.ClassBrowser;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaCodeCompletionData : CodeCompletionData
{
	private ClaAbstractCtrlSpaceCompletionDataProvider.CodeSnippet codeSnippet;

	private ClaAbstractCtrlSpaceCompletionDataProvider.Keyword keyword;

	private string declText = string.Empty;

	private ClarionCommonTextAreaControl ta;

	public string DeclText
	{
		get
		{
			return declText;
		}
		set
		{
			declText = value ?? string.Empty;
		}
	}

	public virtual bool IsPre => false;

	public ClaCodeCompletionData(ClarionCommonTextAreaControl textArea, string s, int imageIndex)
		: base(s, imageIndex)
	{
		ta = textArea;
	}

	public ClaCodeCompletionData(ClarionCommonTextAreaControl textArea, ClaAbstractCtrlSpaceCompletionDataProvider.CodeSnippet snippet)
		: base(snippet.codeSnippet, ClaClassNode.CodeSnippetIcon)
	{
		codeSnippet = snippet;
		((CodeCompletionData)this).Description = snippet.description;
		ta = textArea;
	}

	public ClaCodeCompletionData(ClarionCommonTextAreaControl textArea, ClaAbstractCtrlSpaceCompletionDataProvider.Keyword kw)
		: base(kw.keyword, ClaClassNode.EmptyIcon)
	{
		keyword = kw;
		ta = textArea;
	}

	public ClaCodeCompletionData(ClarionCommonTextAreaControl textArea, IClass c)
		: base(c)
	{
		int sortOrder = 0;
		((CodeCompletionData)this).ImageIndex = ClaClassNode.GetIconIndexForClass(c, ref sortOrder);
		if (c is ClaAbstractDecoration)
		{
			DeclText = ((ClaAbstractDecoration)(object)c).DeclarationText;
		}
		ta = textArea;
	}

	public ClaCodeCompletionData(ClarionCommonTextAreaControl textArea, IMethod method)
		: base(method)
	{
		if (method is ClaRoutine)
		{
			((CodeCompletionData)this).ImageIndex = ClaClassNode.RoutineIcon;
		}
		if (method is ClaAbstractDecoration)
		{
			DeclText = ((ClaAbstractDecoration)(object)method).DeclarationText;
		}
		ta = textArea;
	}

	public ClaCodeCompletionData(ClarionCommonTextAreaControl textArea, IField field)
		: base(field)
	{
		if (field is ClaAbstractDecoration)
		{
			DeclText = ((ClaAbstractDecoration)(object)field).DeclarationText;
		}
		ta = textArea;
		if (field is ClaKeyField)
		{
			((CodeCompletionData)this).ImageIndex = ClaClassNode.KeyIcon;
		}
	}

	public ClaCodeCompletionData(ClarionCommonTextAreaControl textArea, IProperty property)
		: base(property)
	{
		if (property is ClaAbstractDecoration)
		{
			DeclText = ((ClaAbstractDecoration)(object)property).DeclarationText;
		}
		ta = textArea;
	}

	public ClaCodeCompletionData(ClarionCommonTextAreaControl textArea, IEvent e)
		: base(e)
	{
		if (e is ClaAbstractDecoration)
		{
			DeclText = ((ClaAbstractDecoration)(object)e).DeclarationText;
		}
		ta = textArea;
	}

	public override bool InsertAction(TextArea textArea, char ch)
	{
		if (codeSnippet == null)
		{
			if (keyword != null)
			{
				if (ta == null || ta.KeywordsCompletionRule == CommonCompletionBinding.CompletionRule.Upper)
				{
					((CodeCompletionData)this).Text = ((CodeCompletionData)this).Text.ToUpperInvariant();
				}
				else
				{
					((CodeCompletionData)this).Text = ((CodeCompletionData)this).Text.ToLowerInvariant();
				}
			}
			else
			{
				if (IsPre)
				{
					((CodeCompletionData)this).Text = ((CodeCompletionData)this).Text.Substring(0, ((CodeCompletionData)this).Text.Length - 1);
				}
				if (ta != null)
				{
					if (ta.NamesCompletionRule == CommonCompletionBinding.CompletionRule.Upper)
					{
						((CodeCompletionData)this).Text = ((CodeCompletionData)this).Text.ToUpperInvariant();
					}
					else if (ta.NamesCompletionRule == CommonCompletionBinding.CompletionRule.Lower)
					{
						((CodeCompletionData)this).Text = ((CodeCompletionData)this).Text.ToLowerInvariant();
					}
				}
			}
		}
		return ((CodeCompletionData)this).InsertAction(textArea, ch);
	}
}

using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaMethodInsightDataProvider : MethodInsightDataProvider
{
	public ClaMethodInsightDataProvider()
	{
	}

	public ClaMethodInsightDataProvider(int lookupOffset, bool setupOnlyOnce)
		: base(lookupOffset, setupOnlyOnce)
	{
	}

	protected override void SetupDataProvider(string fileName, IDocument document, ExpressionResult expressionResult, int caretLineNumber, int caretColumn)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (expressionResult.Context.IsAttributeContext)
		{
			flag = true;
		}
		else if (expressionResult.Context.IsObjectCreation)
		{
			flag = true;
		}
		CodeCompletionTagInfo codeCompletionTagInfo = new CodeCompletionTagInfo();
		codeCompletionTagInfo.IsMethodInsight = true;
		expressionResult.Tag = codeCompletionTagInfo;
		ResolveResult val = ParserService.Resolve(expressionResult, caretLineNumber, caretColumn, fileName, document.TextContent);
		LanguageProperties language = ParserService.CurrentProjectContent.Language;
		TypeResolveResult val2 = (TypeResolveResult)(object)((val is TypeResolveResult) ? val : null);
		if (val2 == null && language.AllowObjectConstructionOutsideContext && val is MixedResolveResult)
		{
			val2 = ((MixedResolveResult)((val is MixedResolveResult) ? val : null)).TypeResult;
		}
		if (val2 != null && !flag && language.AllowObjectConstructionOutsideContext)
		{
			flag = true;
		}
		if (flag)
		{
			if (val2 == null)
			{
				return;
			}
			foreach (IMethod method in ((ResolveResult)val2).ResolvedType.GetMethods())
			{
				if (method.IsConstructor && !((IDecoration)method).IsStatic)
				{
					base.methods.Add((IMethodOrProperty)(object)method);
				}
			}
			if (base.methods.Count == 0 && val2.ResolvedClass != null && !((IDecoration)val2.ResolvedClass).IsAbstract && !((IDecoration)val2.ResolvedClass).IsStatic)
			{
				base.methods.Add((IMethodOrProperty)(object)Constructor.CreateDefault(val2.ResolvedClass));
			}
			return;
		}
		IClass val3 = null;
		IReturnType val4 = null;
		string text = null;
		MethodResolveResult val5 = (MethodResolveResult)(object)((val is MethodResolveResult) ? val : null);
		if (val5 != null)
		{
			val3 = ((ResolveResult)val5).CallingClass;
			val4 = val5.ContainingType;
			text = val5.Name;
		}
		else
		{
			MemberResolveResult val6 = (MemberResolveResult)(object)((val is MemberResolveResult) ? val : null);
			if (val6 != null)
			{
				val3 = ((ResolveResult)val6).CallingClass;
				if (val6.ResolvedMember != null && ((IDecoration)val6.ResolvedMember).DeclaringType != null)
				{
					text = val6.ResolvedMember.Name;
					val4 = ((IDecoration)val6.ResolvedMember).DeclaringType.DefaultReturnType;
				}
			}
		}
		if (val4 == null || text == null)
		{
			return;
		}
		bool flag2 = false;
		if (val3 != null)
		{
			flag2 = val3.IsTypeInInheritanceTree(val4.GetUnderlyingClass());
		}
		foreach (IMethod method2 in val4.GetMethods())
		{
			if (language.NameComparer.Equals(((IMember)method2).Name, text) && ((IDecoration)method2).IsAccessible(val3, flag2))
			{
				base.methods.Add((IMethodOrProperty)(object)method2);
			}
		}
	}
}

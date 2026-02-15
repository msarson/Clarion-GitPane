using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class OverrideCompletionDataProvider : AbstractCompletionDataProvider
{
	public static IMethod[] GetOverridableMethods(IClass c)
	{
		if (c == null)
		{
			throw new ArgumentException("c");
		}
		List<IMethod> list = new List<IMethod>();
		foreach (IMethod method in c.DefaultReturnType.GetMethods())
		{
			if (method.IsOverridable && !method.IsConst && !method.IsPrivate && method.DeclaringType.FullyQualifiedName != c.FullyQualifiedName)
			{
				list.Add(method);
			}
		}
		return list.ToArray();
	}

	public static IProperty[] GetOverridableProperties(IClass c)
	{
		if (c == null)
		{
			throw new ArgumentException("c");
		}
		List<IProperty> list = new List<IProperty>();
		foreach (IProperty property in c.DefaultReturnType.GetProperties())
		{
			if (property.IsOverridable && !property.IsConst && !property.IsPrivate && property.DeclaringType.FullyQualifiedName != c.FullyQualifiedName)
			{
				list.Add(property);
			}
		}
		return list.ToArray();
	}

	public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
	{
		ParseInformation parseInformation = ParserService.GetParseInformation(fileName);
		if (parseInformation == null)
		{
			return null;
		}
		IClass innermostClass = parseInformation.MostRecentCompilationUnit.GetInnermostClass(textArea.Caret.Line, textArea.Caret.Column);
		if (innermostClass == null)
		{
			return null;
		}
		List<ICompletionData> list = new List<ICompletionData>();
		IMethod[] overridableMethods = GetOverridableMethods(innermostClass);
		foreach (IMethod method in overridableMethods)
		{
			list.Add(new OverrideCompletionData(method));
		}
		IProperty[] overridableProperties = GetOverridableProperties(innermostClass);
		foreach (IProperty property in overridableProperties)
		{
			list.Add(new OverrideCompletionData(property));
		}
		return list.ToArray();
	}
}

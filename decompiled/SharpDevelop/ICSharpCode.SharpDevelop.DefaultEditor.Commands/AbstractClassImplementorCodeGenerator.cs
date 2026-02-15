using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class AbstractClassImplementorCodeGenerator : InterfaceOrAbstractClassCodeGenerator
{
	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.AbstractClass}";

	public override string Hint => "${res:ICSharpCode.SharpDevelop.CodeGenerator.AbstractClass.Hint}";

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		foreach (IProperty property in currentClass.DefaultReturnType.GetProperties())
		{
			if (property.IsAbstract)
			{
				AttributedNode attributedNode = CodeGenerator.ConvertMember(property, classFinderContext);
				attributedNode.Modifier &= ~(Modifiers.Dim | Modifiers.Virtual);
				attributedNode.Modifier |= Modifiers.Override;
				nodes.Add(attributedNode);
			}
		}
		foreach (IMethod method in currentClass.DefaultReturnType.GetMethods())
		{
			if (method.IsAbstract)
			{
				AttributedNode attributedNode2 = CodeGenerator.ConvertMember(method, classFinderContext);
				attributedNode2.Modifier &= ~(Modifiers.Dim | Modifiers.Virtual);
				attributedNode2.Modifier |= Modifiers.Override;
				nodes.Add(attributedNode2);
			}
		}
	}

	protected override void InitContent()
	{
		if (currentClass.ClassType != ICSharpCode.SharpDevelop.Dom.ClassType.Class)
		{
			return;
		}
		for (int i = 0; i < currentClass.BaseTypes.Count; i++)
		{
			IReturnType baseType = currentClass.GetBaseType(i);
			IClass obj = baseType?.GetUnderlyingClass();
			if (obj != null && obj.ClassType == ICSharpCode.SharpDevelop.Dom.ClassType.Class && obj.IsAbstract)
			{
				base.Content.Add(new ClassWrapper(baseType));
			}
		}
	}
}

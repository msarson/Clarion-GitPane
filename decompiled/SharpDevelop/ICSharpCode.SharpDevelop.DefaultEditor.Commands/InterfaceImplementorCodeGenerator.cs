using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class InterfaceImplementorCodeGenerator : InterfaceOrAbstractClassCodeGenerator
{
	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.ImplementInterface}";

	public override string Hint => "${res:ICSharpCode.SharpDevelop.CodeGenerator.ImplementInterface.Hint}";

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		foreach (ClassWrapper item in items)
		{
			codeGen.ImplementInterface(nodes, item.ClassType, !currentClass.ProjectContent.Language.SupportsImplicitInterfaceImplementation, currentClass);
		}
	}

	protected override void InitContent()
	{
		for (int i = 0; i < currentClass.BaseTypes.Count; i++)
		{
			IReturnType baseType = currentClass.GetBaseType(i);
			IClass obj = baseType?.GetUnderlyingClass();
			if (obj != null && obj.ClassType == ICSharpCode.SharpDevelop.Dom.ClassType.Interface)
			{
				base.Content.Add(new ClassWrapper(baseType));
			}
		}
	}
}

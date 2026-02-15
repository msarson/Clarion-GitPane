using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class SetterCodeGenerator : AbstractPropertyCodeGenerator
{
	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.Setter}";

	public override string Hint => "${res:ICSharpCode.SharpDevelop.CodeGenerator.Setter.Hint}";

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		foreach (FieldWrapper item in items)
		{
			nodes.Add(codeGen.CreateProperty(item.Field, createGetter: false, createSetter: true));
		}
	}
}

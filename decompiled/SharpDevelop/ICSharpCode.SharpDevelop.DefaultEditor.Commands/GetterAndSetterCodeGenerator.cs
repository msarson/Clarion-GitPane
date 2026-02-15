using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class GetterAndSetterCodeGenerator : AbstractPropertyCodeGenerator
{
	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.GetterAndSetter}";

	public override string Hint => "${res:ICSharpCode.SharpDevelop.CodeGenerator.GetterAndSetter.Hint}";

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		foreach (FieldWrapper item in items)
		{
			nodes.Add(codeGen.CreateProperty(item.Field, createGetter: true, createSetter: true));
		}
	}
}

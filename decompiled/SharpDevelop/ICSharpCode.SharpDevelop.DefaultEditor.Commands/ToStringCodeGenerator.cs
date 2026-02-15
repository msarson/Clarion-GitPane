using System.Collections;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.NRefactory.Ast;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ToStringCodeGenerator : AbstractFieldCodeGenerator
{
	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.ToString}";

	public override string Hint => "${res:ICSharpCode.SharpDevelop.CodeGenerator.ToString.Hint}";

	public override int ImageIndex => 34;

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		TypeReference typeReference = new TypeReference("System.String");
		MethodDeclaration methodDeclaration = new MethodDeclaration("ToString", Modifiers.Public | Modifiers.Override, typeReference, null, null);
		methodDeclaration.Body = new BlockStatement();
		Expression targetObject = new FieldReferenceExpression(new TypeReferenceExpression(typeReference), "Format");
		InvocationExpression invocationExpression = new InvocationExpression(targetObject);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('[');
		stringBuilder.Append(currentClass.Name);
		for (int i = 0; i < items.Count; i++)
		{
			stringBuilder.Append(' ');
			stringBuilder.Append(codeGen.GetPropertyName(((FieldWrapper)items[i]).Field.Name));
			stringBuilder.Append("={");
			stringBuilder.Append(i);
			stringBuilder.Append('}');
		}
		stringBuilder.Append(']');
		invocationExpression.Arguments.Add(new PrimitiveExpression(stringBuilder.ToString(), stringBuilder.ToString()));
		foreach (FieldWrapper item in items)
		{
			invocationExpression.Arguments.Add(new FieldReferenceExpression(new ThisReferenceExpression(), item.Field.Name));
		}
		methodDeclaration.Body.AddChild(new ReturnStatement(invocationExpression));
		nodes.Add(methodDeclaration);
	}
}

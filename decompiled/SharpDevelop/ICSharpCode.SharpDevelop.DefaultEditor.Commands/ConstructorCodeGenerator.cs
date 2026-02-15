using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ConstructorCodeGenerator : AbstractFieldCodeGenerator
{
	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.Constructor}";

	public override string Hint => "${res:ICSharpCode.SharpDevelop.CodeGenerator.Constructor.Hint}";

	public override int ImageIndex => 34;

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		ConstructorDeclaration constructorDeclaration = new ConstructorDeclaration(currentClass.Name, Modifiers.Public, null, null);
		constructorDeclaration.Body = new BlockStatement();
		foreach (FieldWrapper item in items)
		{
			string parameterName = codeGen.GetParameterName(item.Field.Name);
			constructorDeclaration.Parameters.Add(new ParameterDeclarationExpression(ConvertType(item.Field.ReturnType), parameterName));
			Expression left = new FieldReferenceExpression(new ThisReferenceExpression(), item.Field.Name);
			Expression right = new IdentifierExpression(parameterName);
			Expression expression = new AssignmentExpression(left, AssignmentOperatorType.Assign, right);
			constructorDeclaration.Body.AddChild(new ExpressionStatement(expression));
		}
		nodes.Add(constructorDeclaration);
	}
}

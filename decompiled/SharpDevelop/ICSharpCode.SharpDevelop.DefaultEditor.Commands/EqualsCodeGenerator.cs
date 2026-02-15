using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class EqualsCodeGenerator : CodeGeneratorBase
{
	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.GenerateEqualsAndGetHashCode}";

	public override bool IsActive
	{
		get
		{
			if (currentClass.Fields != null)
			{
				return currentClass.Fields.Count > 0;
			}
			return false;
		}
	}

	public override int ImageIndex => 34;

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		TypeReference typeReference = new TypeReference("System.Int32");
		MethodDeclaration methodDeclaration = new MethodDeclaration("GetHashCode", Modifiers.Public | Modifiers.Override, typeReference, null, null);
		methodDeclaration.Body = new BlockStatement();
		VariableDeclaration variableDeclaration = new VariableDeclaration("hashCode", new PrimitiveExpression(0, "0"), typeReference);
		methodDeclaration.Body.AddChild(new LocalVariableDeclaration(variableDeclaration));
		Expression expression;
		foreach (IField field in currentClass.Fields)
		{
			if (!field.IsStatic)
			{
				expression = new AssignmentExpression(new IdentifierExpression(variableDeclaration.Name), AssignmentOperatorType.ExclusiveOr, new InvocationExpression(new FieldReferenceExpression(new IdentifierExpression(field.Name), "GetHashCode")));
				if (IsValueType(field.ReturnType))
				{
					methodDeclaration.Body.AddChild(new ExpressionStatement(expression));
				}
				else
				{
					methodDeclaration.Body.AddChild(new IfElseStatement(new BinaryOperatorExpression(new IdentifierExpression(field.Name), BinaryOperatorType.ReferenceInequality, new PrimitiveExpression(null, "null")), new ExpressionStatement(expression)));
				}
			}
		}
		methodDeclaration.Body.AddChild(new ReturnStatement(new IdentifierExpression(variableDeclaration.Name)));
		nodes.Add(methodDeclaration);
		TypeReference typeReference2 = new TypeReference("System.Boolean");
		TypeReference typeReference3 = new TypeReference("System.Object");
		methodDeclaration = new MethodDeclaration("Equals", Modifiers.Public | Modifiers.Override, typeReference2, null, null);
		methodDeclaration.Parameters.Add(new ParameterDeclarationExpression(typeReference3, "obj"));
		methodDeclaration.Body = new BlockStatement();
		TypeReference typeReference4 = ConvertType(currentClass.DefaultReturnType);
		expression = new TypeOfIsExpression(new IdentifierExpression("obj"), typeReference4);
		expression = new ParenthesizedExpression(expression);
		expression = new UnaryOperatorExpression(expression, UnaryOperatorType.Not);
		methodDeclaration.Body.AddChild(new IfElseStatement(expression, new ReturnStatement(new PrimitiveExpression(false, "false"))));
		variableDeclaration = new VariableDeclaration("my" + currentClass.Name, new CastExpression(typeReference4, new IdentifierExpression("obj"), CastType.Cast), typeReference4);
		methodDeclaration.Body.AddChild(new LocalVariableDeclaration(variableDeclaration));
		expression = null;
		foreach (IField field2 in currentClass.Fields)
		{
			if (!field2.IsStatic)
			{
				expression = ((expression != null) ? new BinaryOperatorExpression(expression, BinaryOperatorType.LogicalAnd, TestEquality(variableDeclaration.Name, field2)) : TestEquality(variableDeclaration.Name, field2));
			}
		}
		methodDeclaration.Body.AddChild(new ReturnStatement(expression ?? new PrimitiveExpression(true, "true")));
		nodes.Add(methodDeclaration);
	}

	private static bool IsValueType(IReturnType type)
	{
		IClass underlyingClass = type.GetUnderlyingClass();
		if (underlyingClass != null)
		{
			if (underlyingClass.ClassType != ICSharpCode.SharpDevelop.Dom.ClassType.Struct)
			{
				return underlyingClass.ClassType == ICSharpCode.SharpDevelop.Dom.ClassType.Enum;
			}
			return true;
		}
		return false;
	}

	private static bool CanCompareEqualityWithOperator(IReturnType type)
	{
		IClass underlyingClass = type.GetUnderlyingClass();
		if (underlyingClass != null && underlyingClass.FullyQualifiedName != "System.Single" && underlyingClass.FullyQualifiedName != "System.Double")
		{
			if (underlyingClass.ClassType != ICSharpCode.SharpDevelop.Dom.ClassType.Struct && underlyingClass.ClassType != ICSharpCode.SharpDevelop.Dom.ClassType.Enum)
			{
				return underlyingClass.FullyQualifiedName == "System.String";
			}
			return true;
		}
		return false;
	}

	private static Expression TestEquality(string other, IField field)
	{
		if (CanCompareEqualityWithOperator(field.ReturnType))
		{
			return new BinaryOperatorExpression(new FieldReferenceExpression(new ThisReferenceExpression(), field.Name), BinaryOperatorType.Equality, new FieldReferenceExpression(new IdentifierExpression(other), field.Name));
		}
		InvocationExpression invocationExpression = new InvocationExpression(new FieldReferenceExpression(new TypeReferenceExpression("System.Object"), "Equals"));
		invocationExpression.Arguments.Add(new FieldReferenceExpression(new ThisReferenceExpression(), field.Name));
		invocationExpression.Arguments.Add(new FieldReferenceExpression(new IdentifierExpression(other), field.Name));
		return invocationExpression;
	}
}

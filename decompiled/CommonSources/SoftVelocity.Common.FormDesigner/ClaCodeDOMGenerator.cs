using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using ICSharpCode.FormsDesigner;

namespace SoftVelocity.Common.FormDesigner;

internal class ClaCodeDOMGenerator : CodeDOMGenerator
{
	public ClaCodeDOMGenerator(CodeDomProvider codeProvider, string indentation)
		: base(codeProvider, indentation)
	{
	}

	public override void ConvertContentDefinition(CodeMemberMethod method, TextWriter writer)
	{
		CodeGeneratorOptions createCodeGeneratorOptions = base.CodeDOMGeneratorUtility.CreateCodeGeneratorOptions;
		createCodeGeneratorOptions.IndentString = base.Indentation;
		foreach (CodeStatement statement2 in method.Statements)
		{
			if (statement2 is CodeVariableDeclarationStatement)
			{
				try
				{
					base.CodeDomProvider.GenerateCodeFromStatement(statement2, writer, createCodeGeneratorOptions);
				}
				catch (Exception)
				{
				}
			}
		}
		writer.Write(createCodeGeneratorOptions.IndentString + "CODE\r\n");
		foreach (CodeStatement statement3 in method.Statements)
		{
			CodeStatement statement = statement3;
			if (statement3 is CodeVariableDeclarationStatement)
			{
				if (((CodeVariableDeclarationStatement)statement3).InitExpression == null)
				{
					continue;
				}
				statement = new CodeAssignStatement(new CodeVariableReferenceExpression(((CodeVariableDeclarationStatement)statement3).Name), ((CodeVariableDeclarationStatement)statement3).InitExpression);
			}
			writer.Write(createCodeGeneratorOptions.IndentString);
			try
			{
				base.CodeDomProvider.GenerateCodeFromStatement(statement, writer, createCodeGeneratorOptions);
			}
			catch (Exception)
			{
			}
		}
	}

	public string GenerateFieldDeclaration(CodeMemberField field)
	{
		string text = ",PRIVATE";
		switch (field.Attributes)
		{
		case MemberAttributes.Public:
			text = ",PUBLIC";
			break;
		case MemberAttributes.Assembly:
			text = ",INTERNAL";
			break;
		case MemberAttributes.FamilyAndAssembly:
			text = ",INTERNAL";
			break;
		case MemberAttributes.Family:
			text = ",PROTECTED";
			break;
		case MemberAttributes.FamilyOrAssembly:
			text = ",PROTECTED,INTERNAL";
			break;
		}
		return field.Name + " " + field.Type.BaseType + text;
	}
}

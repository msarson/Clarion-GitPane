using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Text;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class CodeDOMGeneratorUtility
{
	private Hashtable namespaces = new Hashtable();

	public CodeGeneratorOptions CreateCodeGeneratorOptions
	{
		get
		{
			CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
			codeGeneratorOptions.BlankLinesBetweenMembers = AmbienceService.CodeGenerationProperties.Get("BlankLinesBetweenMembers", defaultValue: true);
			codeGeneratorOptions.BracingStyle = (AmbienceService.CodeGenerationProperties.Get("StartBlockOnSameLine", defaultValue: true) ? "Block" : "C");
			codeGeneratorOptions.ElseOnClosing = AmbienceService.CodeGenerationProperties.Get("ElseOnClosing", defaultValue: true);
			Properties properties = PropertyService.Get("TextEditorSettings", new Properties());
			if (properties.Get("TabsToSpaces", defaultValue: true))
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < properties.Get("IndentationSize", 4); i++)
				{
					stringBuilder.Append(' ');
				}
				codeGeneratorOptions.IndentString = stringBuilder.ToString();
			}
			else
			{
				codeGeneratorOptions.IndentString = "\t";
			}
			return codeGeneratorOptions;
		}
	}

	public CodeTypeReference GetTypeReference(string type)
	{
		if (AmbienceService.UseFullyQualifiedNames)
		{
			return new CodeTypeReference(type);
		}
		string[] array = type.Split('.');
		string text = array[array.Length - 1];
		if (type.Length - text.Length - 1 > 0)
		{
			string key = type.Substring(0, type.Length - text.Length - 1);
			namespaces[key] = "";
		}
		return new CodeTypeReference(text);
	}

	public CodeTypeReference GetTypeReference(Type type)
	{
		if (AmbienceService.UseFullyQualifiedNames)
		{
			return new CodeTypeReference(type.FullName);
		}
		namespaces[type.Namespace] = "";
		return new CodeTypeReference(type.Name);
	}

	public CodeTypeReferenceExpression GetTypeReferenceExpression(string type)
	{
		return new CodeTypeReferenceExpression(GetTypeReference(type));
	}

	public CodeTypeReferenceExpression GetTypeReferenceExpression(Type type)
	{
		return new CodeTypeReferenceExpression(GetTypeReference(type));
	}

	public void AddNamespaceImport(string ns)
	{
		namespaces[ns] = "";
	}

	public void GenerateNamespaceImports(CodeNamespace cnamespace)
	{
		foreach (string key in namespaces.Keys)
		{
			cnamespace.Imports.Add(new CodeNamespaceImport(key));
		}
	}
}

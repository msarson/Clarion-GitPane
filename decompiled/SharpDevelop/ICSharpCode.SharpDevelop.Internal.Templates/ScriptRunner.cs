using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class ScriptRunner
{
	private FileTemplate item;

	private FileDescriptionTemplate file;

	private static readonly Regex scriptRegex = new Regex("<%.*?%>");

	private static readonly Regex replaceRegex = new Regex("\"");

	public string CompileScript(FileTemplate item, FileDescriptionTemplate file)
	{
		if (file.Content == null)
		{
			throw new ArgumentException("file must have textual content");
		}
		Match match = scriptRegex.Match(file.Content);
		match = match.NextMatch();
		if (match.Success)
		{
			this.item = item;
			this.file = file;
			return CompileAndGetOutput(GenerateCode());
		}
		return file.Content;
	}

	private byte[] GetBytes(string fileName)
	{
		using FileStream fileStream = new FileStream(fileName, FileMode.Open);
		long length = fileStream.Length;
		byte[] array = new byte[length];
		fileStream.Read(array, 0, (int)length);
		fileStream.Close();
		return array;
	}

	private string CompileAndGetOutput(string fileContent)
	{
		TempFileCollection tempFileCollection = new TempFileCollection();
		string text = Path.Combine(tempFileCollection.BasePath, tempFileCollection.TempDir);
		Directory.CreateDirectory(text);
		string text2 = Path.Combine(text, "InternalGeneratedScript.cs");
		string text3 = Path.Combine(text, "A.DLL");
		tempFileCollection.AddFile(text2, keepFile: false);
		tempFileCollection.AddFile(text3, keepFile: false);
		StreamWriter streamWriter = new StreamWriter(text2);
		streamWriter.Write(fileContent);
		streamWriter.Close();
		string outputName = string.Empty;
		string errorName = string.Empty;
		Executor.ExecWaitWithCapture(GetCompilerName() + " /target:library \"/out:" + text3 + "\" \"" + text2 + "\"", tempFileCollection, ref outputName, ref errorName);
		if (!File.Exists(text3))
		{
			StreamReader streamReader = File.OpenText(outputName);
			string message = streamReader.ReadToEnd();
			streamReader.Close();
			MessageService.ShowMessage(message);
			return ">>>>ERROR IN CODE GENERATION GENERATED SCRIPT WAS:\n" + fileContent + "\n>>>>END";
		}
		Assembly assembly = Assembly.Load(GetBytes(text3));
		object obj = assembly.CreateInstance("Template");
		foreach (TemplateProperty property in item.Properties)
		{
			FieldInfo field = obj.GetType().GetField(property.Name);
			field.SetValue(obj, Convert.ChangeType(StringParser.Properties["Properties." + property.Name], property.Type.StartsWith("Types:") ? typeof(string) : Type.GetType(property.Type)));
		}
		MethodInfo method = obj.GetType().GetMethod("GenerateOutput");
		string result = method.Invoke(obj, null).ToString();
		tempFileCollection.Delete();
		return result;
	}

	private string GetCompilerName()
	{
		string runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
		return '"' + Path.Combine(runtimeDirectory, "csc.exe") + '"';
	}

	private string GenerateCode()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		stringBuilder.Append("public class Template {\n");
		foreach (TemplateProperty property in item.Properties)
		{
			stringBuilder.Append("public ");
			if (property.Type.StartsWith("Types:"))
			{
				stringBuilder.Append("string");
			}
			else
			{
				stringBuilder.Append(property.Type);
			}
			stringBuilder.Append(' ');
			stringBuilder.Append(property.Name);
			stringBuilder.Append(";\n");
		}
		stringBuilder.Append("public string GenerateOutput() {\n");
		stringBuilder.Append("System.Text.StringBuilder outPut = new System.Text.StringBuilder();\n");
		Match match = scriptRegex.Match(file.Content);
		while (match.Success)
		{
			Group obj = match.Groups[0];
			stringBuilder.Append("outPut.Append(@\"");
			stringBuilder.Append(file.Content.Substring(num, obj.Index - num));
			stringBuilder.Append("\");\n");
			stringBuilder.Append(obj.Value.Substring(2, obj.Length - 4));
			num = obj.Index + obj.Length;
			match = match.NextMatch();
		}
		stringBuilder.Append("outPut.Append(@\"");
		string value = replaceRegex.Replace(file.Content.Substring(num, file.Content.Length - num), "\"\"");
		stringBuilder.Append(value);
		stringBuilder.Append("\");\n");
		stringBuilder.Append("return outPut.ToString();\n");
		stringBuilder.Append("}}\n");
		return stringBuilder.ToString();
	}
}

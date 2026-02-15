using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
using ICSharpCode.Core;
using ICSharpCode.TextEditor.Util;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Project.Converter;

public static class PrjxToSolutionProject
{
	public class Conversion
	{
		public Dictionary<string, Guid> NameToGuid = new Dictionary<string, Guid>();

		public Dictionary<string, string> NameToPath = new Dictionary<string, string>();

		public Dictionary<Guid, string> GuidToPath = new Dictionary<Guid, string>();

		public bool IsVisualBasic;

		public List<string> Resources;

		public string basePath;

		private string rootNamespace;

		public string GetLanguageName()
		{
			if (!IsVisualBasic)
			{
				return "CSharp";
			}
			return "VisualBasic";
		}

		public string GetGuid(string name)
		{
			return "{" + NameToGuid[name].ToString().ToUpperInvariant() + "}";
		}

		public string GetRelativeProjectPath(string name)
		{
			if (!NameToPath.ContainsKey(name))
			{
				if (MessageService.AskQuestion("Project reference to " + name + " could not be resolved.\nDo you want to specify it manually?"))
				{
					using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
					openFileDialog.Title = "Find " + name;
					openFileDialog.InitialDirectory = basePath;
					openFileDialog.Filter = "SharpDevelop 1.x project|*.prjx";
					if (openFileDialog.ShowDialog() == DialogResult.OK)
					{
						NameToPath[name] = openFileDialog.FileName;
						NameToGuid[name] = Guid.NewGuid();
						return FileUtility.GetRelativePath(basePath, NameToPath[name]);
					}
				}
				return "NotFound." + name + ".proj";
			}
			return FileUtility.GetRelativePath(basePath, NameToPath[name]);
		}

		public string GetRelativeProjectPathByGuid(string name, string guidText)
		{
			if (!NameToPath.ContainsKey(name))
			{
				Guid key = new Guid(guidText);
				if (!GuidToPath.ContainsKey(key))
				{
					MessageService.ShowWarning("Project reference to " + name + " could not be resolved.");
					return "NotFound." + name + ".proj";
				}
				return GuidToPath[key];
			}
			return NameToPath[name];
		}

		public bool IsNotGacReference(string hintPath)
		{
			if (hintPath == null || hintPath.Length == 0)
			{
				return false;
			}
			return !FileUtility.IsBaseDirectory(FileUtility.NETFrameworkInstallRoot, hintPath);
		}

		public string SetRootNamespace(string ns)
		{
			return rootNamespace = ns;
		}

		public string ConvertResource(string fileName)
		{
			if (Resources == null)
			{
				Resources = new List<string>();
			}
			fileName = CanocializeFileName(fileName);
			string fileName2 = Path.GetFileName(fileName);
			if (rootNamespace.Length > 0)
			{
				if (fileName2.StartsWith(rootNamespace + "."))
				{
					fileName2 = fileName2.Substring(rootNamespace.Length + 1);
					fileName2 = ConvertResourceInternal(fileName, fileName2);
					if (fileName2 != null)
					{
						return fileName2;
					}
				}
			}
			else
			{
				fileName2 = ConvertResourceInternal(fileName, fileName2);
				if (fileName2 != null)
				{
					return fileName2;
				}
			}
			Resources.Add(Path.Combine(basePath, fileName));
			return fileName;
		}

		private string ConvertResourceInternal(string fileName, string name)
		{
			string[] array = name.Split('.');
			string path = basePath;
			for (int i = 0; i < array.Length; i++)
			{
				if (Directory.Exists(Path.Combine(path, array[i])))
				{
					path = Path.Combine(path, array[i]);
					continue;
				}
				path = Path.Combine(path, array[i]);
				for (int j = i + 1; j < array.Length; j++)
				{
					path = path + '.' + array[j];
				}
				try
				{
					File.Move(Path.Combine(basePath, fileName), path);
					return FileUtility.GetRelativePath(basePath, path);
				}
				catch
				{
				}
				break;
			}
			return null;
		}

		public string CanocializeFileName(string fileName)
		{
			if ((fileName.StartsWith("..\\") || fileName.StartsWith("../")) && !File.Exists(Path.Combine(basePath, fileName)))
			{
				string text = fileName.Substring(3);
				if (File.Exists(Path.Combine(basePath, text)))
				{
					fileName = text;
				}
			}
			if (fileName.StartsWith("./") || fileName.StartsWith(".\\"))
			{
				return fileName.Substring(2);
			}
			return fileName;
		}

		public string CanocializePath(string fileName)
		{
			return CanocializeFileName(fileName) + Path.DirectorySeparatorChar;
		}

		public string Negate(string booleanString)
		{
			return "false".Equals(booleanString, StringComparison.OrdinalIgnoreCase).ToString();
		}

		public string GetFileName(string fileName)
		{
			return Path.GetFileName(fileName);
		}

		public string GetFileNameWithoutExtension(string fileName)
		{
			return Path.GetFileNameWithoutExtension(fileName);
		}

		public string ConvertBuildEvent(string executeScript, string arguments)
		{
			if (executeScript == null || executeScript.Length == 0)
			{
				return "";
			}
			if (arguments != null && arguments.Length > 0)
			{
				return FileUtility.GetAbsolutePath(basePath, executeScript) + " " + arguments;
			}
			return FileUtility.GetAbsolutePath(basePath, executeScript);
		}

		public static string GetProjectName(string fileName)
		{
			XmlTextReader xmlTextReader = new XmlTextReader(fileName);
			try
			{
				xmlTextReader.MoveToContent();
				if (xmlTextReader.MoveToAttribute("name"))
				{
					return xmlTextReader.Value;
				}
				return fileName;
			}
			finally
			{
				xmlTextReader.Close();
			}
		}
	}

	private static Dictionary<string, XslCompiledTransform> xsltDict = new Dictionary<string, XslCompiledTransform>();

	public static void RunConverter(TextReader inFile, string outFile, string script, Conversion conversion)
	{
		XslCompiledTransform xslCompiledTransform;
		if (xsltDict.ContainsKey(script))
		{
			xslCompiledTransform = xsltDict[script];
		}
		else
		{
			xslCompiledTransform = new XslCompiledTransform();
			xslCompiledTransform.Load(FileUtility.Combine(PropertyService.DataDirectory, "ConversionStyleSheets", script));
			xsltDict[script] = xslCompiledTransform;
		}
		StringWriter stringWriter = new StringWriter();
		using (XmlTextReader input = new XmlTextReader(inFile))
		{
			using XmlTextWriter results = new XmlTextWriter(stringWriter);
			XsltArgumentList xsltArgumentList = new XsltArgumentList();
			xsltArgumentList.AddExtensionObject("urn:Conversion", conversion);
			xslCompiledTransform.Transform(input, xsltArgumentList, results, null);
		}
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(outFile, Encoding.UTF8);
		xmlTextWriter.Formatting = Formatting.Indented;
		using XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(stringWriter.ToString()));
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.Significant;
		xmlTextWriter.WriteNode(xmlTextReader, defattr: false);
	}

	public static IProject ConvertOldProject(string fileName, Conversion conversion, IMSBuildEngineProvider provider)
	{
		string text = ((!conversion.IsVisualBasic) ? Path.ChangeExtension(fileName, ".csproj") : Path.ChangeExtension(fileName, ".vbproj"));
		conversion.basePath = Path.GetDirectoryName(fileName);
		using (StreamReader inFile = new StreamReader(fileName))
		{
			RunConverter(inFile, text, "CSharp_prjx2csproj.xsl", conversion);
		}
		using (StreamReader inFile2 = new StreamReader(fileName))
		{
			RunConverter(inFile2, text + ".user", "CSharp_prjx2csproj_user.xsl", conversion);
		}
		return LanguageBindingService.LoadProject(provider, text, Conversion.GetProjectName(fileName));
	}

	public static void ConvertVSNetProject(string fileName)
	{
		string text = fileName + ".old";
		string text2 = fileName + ".user";
		string text3 = fileName + ".user.old";
		File.Copy(fileName, text, overwrite: true);
		File.Delete(fileName);
		if (File.Exists(text2))
		{
			File.Copy(text2, text3, overwrite: true);
			File.Delete(text2);
		}
		Conversion conversion = new Conversion();
		if (Path.GetExtension(fileName).ToLowerInvariant() == ".vbproj")
		{
			conversion.IsVisualBasic = true;
		}
		if (Solution.SolutionBeingLoaded != null)
		{
			Solution.ReadSolutionInformation(Solution.SolutionBeingLoaded.FileName, conversion);
		}
		conversion.basePath = Path.GetDirectoryName(fileName);
		Encoding encoding = Encoding.Default;
		string s = FileReader.ReadFileContent(text, encoding);
		RunConverter(new StringReader(s), fileName, "vsnet2msbuild.xsl", conversion);
		if (File.Exists(text3))
		{
			s = FileReader.ReadFileContent(text3, encoding);
			RunConverter(new StringReader(s), text2, "vsnet2msbuild_user.xsl", conversion);
		}
	}
}

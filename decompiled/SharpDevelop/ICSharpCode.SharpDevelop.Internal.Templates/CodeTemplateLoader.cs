using System;
using System.Collections;
using System.IO;
using System.Xml;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class CodeTemplateLoader
{
	private static string TemplateFileName;

	private static string TemplateVersion;

	private static ArrayList templateGroups;

	public static ArrayList TemplateGroups
	{
		get
		{
			return templateGroups;
		}
		set
		{
			templateGroups = value;
		}
	}

	public static CodeTemplateGroup GetTemplateGroupPerFilename(string fileName)
	{
		return GetTemplateGroupPerExtension(Path.GetExtension(fileName));
	}

	public static CodeTemplateGroup GetTemplateGroupPerExtension(string extension)
	{
		foreach (CodeTemplateGroup templateGroup in templateGroups)
		{
			foreach (string extension2 in templateGroup.Extensions)
			{
				if (extension2.Equals(extension.Trim().ToLower(), StringComparison.OrdinalIgnoreCase))
				{
					return templateGroup;
				}
			}
		}
		return null;
	}

	private static bool LoadTemplatesFromStream(string filename)
	{
		if (!File.Exists(filename))
		{
			return false;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.PreserveWhitespace = true;
			xmlDocument.Load(filename);
			templateGroups = new ArrayList();
			if (xmlDocument.DocumentElement.GetAttribute("version") != TemplateVersion)
			{
				return false;
			}
			foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
			{
				if (childNode is XmlElement el)
				{
					templateGroups.Add(new CodeTemplateGroup(el));
				}
			}
		}
		catch (FileNotFoundException)
		{
			return false;
		}
		return true;
	}

	private static void WriteTemplatesToFile(string fileName)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml("<CodeTemplates version = \"" + TemplateVersion + "\" />");
		foreach (CodeTemplateGroup templateGroup in templateGroups)
		{
			xmlDocument.DocumentElement.AppendChild(templateGroup.ToXmlElement(xmlDocument));
		}
		FileUtility.ObservedSave(xmlDocument.Save, fileName, FileErrorPolicy.ProvideAlternative);
	}

	static CodeTemplateLoader()
	{
		TemplateFileName = "CodeSnippets.xml";
		TemplateVersion = "2.0";
		templateGroups = new ArrayList();
		if (!LoadTemplatesFromStream(Path.Combine(PropertyService.ConfigDirectory, TemplateFileName)))
		{
			LoggingService.Info("Templates: can't load user defaults, reading system defaults");
			if (!LoadTemplatesFromStream(FileUtility.Combine(PropertyService.DataDirectory, "options", TemplateFileName)))
			{
				MessageService.ShowWarning("${res:Internal.Templates.CodeTemplateLoader.CantLoadTemplatesWarning}");
			}
		}
	}

	public static void SaveTemplates()
	{
		WriteTemplatesToFile(Path.Combine(PropertyService.ConfigDirectory, TemplateFileName));
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class FileTemplate : IComparable
{
	public static List<FileTemplate> FileTemplates;

	private string author;

	private string name;

	private string category;

	private string languagename;

	private string icon;

	private string description;

	private string wizardpath;

	private string defaultName;

	private string subcategory;

	private string binaryFileGeneratorPath;

	private string templateFamily;

	private string templateChain;

	private string templateName;

	private bool newFileDialogVisible = true;

	private List<FileDescriptionTemplate> files = new List<FileDescriptionTemplate>();

	private List<TemplateProperty> properties = new List<TemplateProperty>();

	private List<TemplateScript> scripts = new List<TemplateScript>();

	private List<TemplateType> customTypes = new List<TemplateType>();

	private XmlElement fileoptions;

	public string Author => author;

	public string Name => name;

	public string Category => category;

	public string Subcategory => subcategory;

	public string LanguageName => languagename;

	public string Icon => icon;

	public string Description => description;

	public string WizardPath => wizardpath;

	public string DefaultName => defaultName;

	public XmlElement Fileoptions => fileoptions;

	public bool NewFileDialogVisible => newFileDialogVisible;

	public string BinaryFileGeneratorPath => binaryFileGeneratorPath;

	public string TemplateFamily => templateFamily;

	public string TemplateChain => templateChain;

	public string TemplateName => templateName;

	public List<FileDescriptionTemplate> FileDescriptionTemplates => files;

	public List<TemplateProperty> Properties => properties;

	public List<TemplateType> CustomTypes => customTypes;

	public bool HasProperties
	{
		get
		{
			if (properties != null)
			{
				return properties.Count > 0;
			}
			return false;
		}
	}

	public List<TemplateScript> Scripts => scripts;

	public bool HasScripts
	{
		get
		{
			if (scripts != null)
			{
				return scripts.Count > 0;
			}
			return false;
		}
	}

	int IComparable.CompareTo(object other)
	{
		if (!(other is FileTemplate fileTemplate))
		{
			return -1;
		}
		int num = category.CompareTo(fileTemplate.category);
		if (num != 0)
		{
			return num;
		}
		return name.CompareTo(fileTemplate.name);
	}

	public FileTemplate(string filename)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(filename);
		author = xmlDocument.DocumentElement.GetAttribute("author");
		XmlElement xmlElement = xmlDocument.DocumentElement["Config"];
		name = xmlElement.GetAttribute("name");
		icon = xmlElement.GetAttribute("icon");
		category = xmlElement.GetAttribute("category");
		defaultName = xmlElement.GetAttribute("defaultname");
		languagename = xmlElement.GetAttribute("language");
		if (xmlElement.HasAttribute("subcategory"))
		{
			subcategory = xmlElement.GetAttribute("subcategory");
		}
		string attribute = xmlElement.GetAttribute("newfiledialogvisible");
		if (attribute != null && attribute.Length != 0 && attribute.Equals("false", StringComparison.OrdinalIgnoreCase))
		{
			newFileDialogVisible = false;
		}
		if (xmlDocument.DocumentElement["Description"] != null)
		{
			description = xmlDocument.DocumentElement["Description"].InnerText;
		}
		if (xmlElement["Wizard"] != null)
		{
			wizardpath = xmlElement["Wizard"].Attributes["path"].InnerText;
		}
		if (xmlElement["BinaryFileGenerator"] != null)
		{
			binaryFileGeneratorPath = xmlElement["BinaryFileGenerator"].InnerText;
			if (xmlElement["BinaryFileGenerator"].Attributes["family"] != null)
			{
				templateFamily = xmlElement["BinaryFileGenerator"].Attributes["family"].Value;
			}
			if (xmlElement["BinaryFileGenerator"].Attributes["chain"] != null)
			{
				templateChain = xmlElement["BinaryFileGenerator"].Attributes["chain"].Value;
			}
			if (xmlElement["BinaryFileGenerator"].Attributes["name"] != null)
			{
				templateName = xmlElement["BinaryFileGenerator"].Attributes["name"].Value;
			}
		}
		if (xmlDocument.DocumentElement["Properties"] != null)
		{
			XmlNodeList xmlNodeList = xmlDocument.DocumentElement["Properties"].SelectNodes("Property");
			foreach (XmlElement item in xmlNodeList)
			{
				properties.Add(new TemplateProperty(item));
			}
		}
		if (xmlDocument.DocumentElement["Types"] != null)
		{
			XmlNodeList xmlNodeList2 = xmlDocument.DocumentElement["Types"].SelectNodes("Type");
			foreach (XmlElement item2 in xmlNodeList2)
			{
				customTypes.Add(new TemplateType(item2));
			}
		}
		fileoptions = xmlDocument.DocumentElement["AdditionalOptions"];
		xmlDocument.DocumentElement.SetAttribute("fileName", filename);
		XmlElement xmlElement2 = xmlDocument.DocumentElement["Files"];
		XmlNodeList childNodes = xmlElement2.ChildNodes;
		foreach (XmlNode item3 in childNodes)
		{
			if (item3 is XmlElement)
			{
				files.Add(new FileDescriptionTemplate((XmlElement)item3, Path.GetDirectoryName(filename)));
			}
		}
		XmlNodeList xmlNodeList3 = xmlDocument.DocumentElement.SelectNodes("Script");
		foreach (XmlElement item4 in xmlNodeList3)
		{
			scripts.Add(new TemplateScript(item4));
		}
	}

	static FileTemplate()
	{
		FileTemplates = new List<FileTemplate>();
		string directory = FileUtility.Combine(PropertyService.DataDirectory, "templates", "file");
		List<string> list = FileUtility.SearchDirectory(directory, "*.xft");
		foreach (string item in AddInTree.BuildItems("/SharpDevelop/BackendBindings/Templates", null, throwOnNotFound: false))
		{
			list.AddRange(FileUtility.SearchDirectory(item, "*.xft"));
		}
		foreach (string item2 in list)
		{
			try
			{
				FileTemplates.Add(new FileTemplate(item2));
			}
			catch (XmlException ex)
			{
				MessageService.ShowError("Error loading template file " + item2 + ":\n" + ex.Message);
			}
			catch (TemplateLoadException ex2)
			{
				MessageService.ShowError("Error loading template file " + item2 + ":\n" + ex2.ToString());
			}
			catch (Exception ex3)
			{
				MessageService.ShowError(ex3, "Error loading template file " + item2 + ".");
			}
		}
		FileTemplates.Sort();
	}
}

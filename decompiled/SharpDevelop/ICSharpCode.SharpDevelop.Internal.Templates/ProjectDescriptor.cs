using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public sealed class ProjectDescriptor
{
	public class ProjectProperty
	{
		public string Name;

		public string Value;

		public string Configuration;

		public string Platform;

		public PropertyStorageLocations Location;

		public bool ValueIsLiteral;

		public ProjectProperty(string name, string value, string configuration, string platform, PropertyStorageLocations location)
		{
			Name = name;
			Value = value;
			Configuration = configuration;
			Platform = platform;
			Location = location;
		}
	}

	private string name;

	private string relativePath;

	private string languageName;

	private bool clearExistingImports;

	private List<KeyValuePair<string, string>> projectImports = new List<KeyValuePair<string, string>>();

	private string importsFailureMessage;

	private List<FileDescriptionTemplate> files = new List<FileDescriptionTemplate>();

	private List<ProjectItem> projectItems = new List<ProjectItem>();

	private List<ProjectProperty> projectProperties = new List<ProjectProperty>();

	public string Name => name;

	public string RelativePath => relativePath;

	public string LanguageName => languageName;

	public List<ProjectProperty> ProjectProperties => projectProperties;

	public ProjectDescriptor(XmlElement element, string hintPath)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (hintPath == null)
		{
			throw new ArgumentNullException("hintPath");
		}
		if (element.HasAttribute("name"))
		{
			name = element.GetAttribute("name");
		}
		else
		{
			name = "${ProjectName}";
		}
		if (element.HasAttribute("directory"))
		{
			relativePath = element.GetAttribute("directory");
		}
		else
		{
			relativePath = ".";
		}
		languageName = element.GetAttribute("language");
		string.IsNullOrEmpty(languageName);
		LoadElementChildren(element, hintPath);
	}

	private void LoadElementChildren(XmlElement parentElement, string hintPath)
	{
		foreach (XmlElement item in ChildElements(parentElement))
		{
			LoadElement(item, hintPath);
		}
	}

	private static IEnumerable<XmlElement> ChildElements(XmlElement parentElement)
	{
		return Linq.OfType<XmlElement>(parentElement.ChildNodes);
	}

	private void LoadElement(XmlElement node, string hintPath)
	{
		switch (node.Name)
		{
		case "ProjectItems":
			LoadProjectItems(node);
			break;
		case "Files":
			LoadFiles(node, hintPath);
			break;
		case "Imports":
			LoadImports(node);
			break;
		case "PropertyGroup":
			LoadPropertyGroup(node);
			break;
		case "Include":
		{
			TemplateLoadException.AssertAttributeExists(node, "src");
			string text = Path.Combine(hintPath, node.GetAttribute("src"));
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(text);
				xmlDocument.DocumentElement.SetAttribute("fileName", text);
				if (xmlDocument.DocumentElement.Name == "Include")
				{
					LoadElementChildren(xmlDocument.DocumentElement, Path.GetDirectoryName(text));
				}
				else
				{
					LoadElement(xmlDocument.DocumentElement, Path.GetDirectoryName(text));
				}
				break;
			}
			catch (XmlException innerException)
			{
				throw new TemplateLoadException("Error loading include file " + text, innerException);
			}
		}
		default:
			throw new TemplateLoadException("Unknown node in <Project>: " + node.Name);
		case "Options":
			break;
		}
	}

	private void LoadProjectItems(XmlElement projectItemsElement)
	{
		foreach (XmlElement item in ChildElements(projectItemsElement))
		{
			ProjectItem projectItem = new UnknownProjectItem(null, item.Name, item.GetAttribute("Include"));
			foreach (XmlElement item2 in ChildElements(item))
			{
				projectItem.SetMetadata(item2.Name, item2.InnerText);
			}
			projectItems.Add(projectItem);
		}
	}

	private void LoadPropertyGroup(XmlElement propertyGroupElement)
	{
		string attribute = propertyGroupElement.GetAttribute("configuration");
		string attribute2 = propertyGroupElement.GetAttribute("platform");
		PropertyStorageLocations propertyStorageLocations;
		if (string.IsNullOrEmpty(attribute) && string.IsNullOrEmpty(attribute2))
		{
			propertyStorageLocations = PropertyStorageLocations.Base;
		}
		else
		{
			propertyStorageLocations = PropertyStorageLocations.Unchanged;
			if (!string.IsNullOrEmpty(attribute))
			{
				propertyStorageLocations |= PropertyStorageLocations.ConfigurationSpecific;
			}
			if (!string.IsNullOrEmpty(attribute2))
			{
				propertyStorageLocations |= PropertyStorageLocations.PlatformSpecific;
			}
		}
		if (string.Equals(propertyGroupElement.GetAttribute("userFile"), "true", StringComparison.OrdinalIgnoreCase))
		{
			propertyStorageLocations |= PropertyStorageLocations.UserFile;
		}
		foreach (XmlElement item in ChildElements(propertyGroupElement))
		{
			ProjectProperty projectProperty = new ProjectProperty(item.Name, item.InnerText, attribute, attribute2, propertyStorageLocations);
			if (string.Equals(propertyGroupElement.GetAttribute("escapeValue"), "false", StringComparison.OrdinalIgnoreCase))
			{
				projectProperty.ValueIsLiteral = false;
			}
			else
			{
				projectProperty.ValueIsLiteral = true;
			}
			projectProperties.Add(projectProperty);
		}
	}

	private void LoadImports(XmlElement importsElement)
	{
		if (string.Equals(importsElement.GetAttribute("clear"), "true", StringComparison.OrdinalIgnoreCase))
		{
			clearExistingImports = true;
		}
		if (importsElement.HasAttribute("failureMessage"))
		{
			importsFailureMessage = importsElement.GetAttribute("failureMessage");
		}
		foreach (XmlElement item in ChildElements(importsElement))
		{
			TemplateLoadException.AssertAttributeExists(item, "Project");
			projectImports.Add(new KeyValuePair<string, string>(item.GetAttribute("Project"), item.HasAttribute("Condition") ? item.GetAttribute("Condition") : null));
		}
	}

	private void LoadFiles(XmlElement filesElement, string hintPath)
	{
		foreach (XmlElement item in ChildElements(filesElement))
		{
			files.Add(new FileDescriptionTemplate(item, hintPath));
		}
	}

	public IProject CreateProject(ProjectCreateInformation projectCreateInformation, string defaultLanguage)
	{
		string projectBasePath = projectCreateInformation.ProjectBasePath;
		string projectName = projectCreateInformation.ProjectName;
		try
		{
			projectCreateInformation.ProjectBasePath = Path.Combine(projectCreateInformation.ProjectBasePath, relativePath);
			if (!Directory.Exists(projectCreateInformation.ProjectBasePath))
			{
				Directory.CreateDirectory(projectCreateInformation.ProjectBasePath);
			}
			string text = (string.IsNullOrEmpty(languageName) ? defaultLanguage : languageName);
			ILanguageBinding languageBinding = LanguageBindingService.GetCodonPerLanguageName(text)?.Binding;
			if (languageBinding == null)
			{
				StringParser.Properties["type"] = text;
				MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.CantCreateProjectWithTypeError}");
				return null;
			}
			string text2 = StringParser.Parse(name, new string[1, 2] { { "ProjectName", projectCreateInformation.ProjectName } });
			string fullPath = Path.GetFullPath(Path.Combine(projectCreateInformation.ProjectBasePath, text2 + LanguageBindingService.GetProjectFileExtension(text)));
			StringBuilder stringBuilder = new StringBuilder();
			if (text2 != null && text2.Length > 0)
			{
				char c = '.';
				for (int i = 0; i < text2.Length; i++)
				{
					if (c == '.')
					{
						c = text2[i];
						if (!char.IsLetter(c))
						{
							stringBuilder.Append('_');
						}
						else
						{
							stringBuilder.Append(c);
						}
					}
					else
					{
						c = text2[i];
						if (!char.IsLetterOrDigit(c) && c != '.')
						{
							stringBuilder.Append('_');
						}
						else
						{
							stringBuilder.Append(c);
						}
					}
				}
			}
			projectCreateInformation.OutputProjectFileName = fullPath;
			projectCreateInformation.RootNamespace = stringBuilder.ToString();
			projectCreateInformation.ProjectName = text2;
			StringParser.Properties["StandardNamespace"] = projectCreateInformation.RootNamespace;
			IProject project = languageBinding.CreateProject(projectCreateInformation);
			foreach (ProjectItem projectItem2 in projectItems)
			{
				ProjectItem projectItem = new UnknownProjectItem(project, StringParser.Parse(projectItem2.ItemType.ItemName), StringParser.Parse(projectItem2.Include));
				foreach (string metadataName in projectItem2.MetadataNames)
				{
					projectItem.SetEvaluatedMetadata(StringParser.Parse(metadataName), StringParser.Parse(projectItem2.GetMetadata(metadataName)));
				}
				((IProjectItemListProvider)project).AddProjectItem(projectItem);
			}
			if (clearExistingImports || projectImports.Count > 0)
			{
				if (!(project is MSBuildBasedProject))
				{
					throw new Exception("<Imports> may be only used in project templates for MSBuildBasedProjects");
				}
				if (clearExistingImports)
				{
					MSBuildInternals.ClearImports(((MSBuildBasedProject)project).MSBuildProject);
				}
				try
				{
					foreach (KeyValuePair<string, string> projectImport in projectImports)
					{
						((MSBuildBasedProject)project).MSBuildProject.AddNewImport(projectImport.Key, projectImport.Value);
					}
					((MSBuildBasedProject)project).CreateItemsListFromMSBuild();
				}
				catch (InvalidProjectFileException ex)
				{
					if (string.IsNullOrEmpty(importsFailureMessage))
					{
						MessageService.ShowError("Error creating project:\n" + ex.Message);
					}
					else
					{
						MessageService.ShowError(importsFailureMessage + "\n\n" + ex.Message);
					}
					return null;
				}
			}
			if (projectProperties.Count > 0)
			{
				if (!(project is MSBuildBasedProject))
				{
					throw new Exception("<PropertyGroup> may be only used in project templates for MSBuildBasedProjects");
				}
				foreach (ProjectProperty projectProperty in projectProperties)
				{
					((MSBuildBasedProject)project).SetProperty(StringParser.Parse(projectProperty.Configuration), StringParser.Parse(projectProperty.Platform), StringParser.Parse(projectProperty.Name), StringParser.Parse(projectProperty.Value), projectProperty.Location, projectProperty.ValueIsLiteral);
				}
			}
			foreach (FileDescriptionTemplate file in files)
			{
				string text3 = Path.Combine(projectCreateInformation.ProjectBasePath, StringParser.Parse(file.Name, new string[1, 2] { { "ProjectName", projectCreateInformation.ProjectName } }));
				FileProjectItem fileProjectItem = new FileProjectItem(project, project.GetDefaultItemType(text3));
				fileProjectItem.Include = FileUtility.GetRelativePath(project.Directory, text3);
				file.SetProjectItemProperties(fileProjectItem);
				((IProjectItemListProvider)project).AddProjectItem(fileProjectItem);
				if (File.Exists(text3))
				{
					StringParser.Properties["fileName"] = text3;
					if (!MessageService.AskQuestion("${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.OverwriteQuestion}", "${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.OverwriteQuestion.InfoName}"))
					{
						continue;
					}
				}
				try
				{
					if (!Directory.Exists(Path.GetDirectoryName(text3)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(text3));
					}
					if (file.ContentData != null)
					{
						File.WriteAllBytes(text3, file.ContentData);
						continue;
					}
					StreamWriter streamWriter = new StreamWriter(File.Create(text3), ParserService.DefaultFileEncoding);
					streamWriter.Write(StringParser.Parse(StringParser.Parse(file.Content, new string[2, 2]
					{
						{ "ProjectName", projectCreateInformation.ProjectName },
						{ "FileName", text3 }
					})));
					streamWriter.Close();
				}
				catch (Exception ex2)
				{
					StringParser.Properties["fileName"] = text3;
					MessageService.ShowError(ex2, "${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.FileCouldntBeWrittenError}");
				}
			}
			if (File.Exists(fullPath))
			{
				StringParser.Properties["projectLocation"] = fullPath;
				if (MessageService.AskQuestion("${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.OverwriteProjectQuestion}", "${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.OverwriteQuestion.InfoName}"))
				{
					project.Save();
				}
			}
			else
			{
				project.Save();
				project.AddToolsVersionAttribute();
			}
			projectCreateInformation.CreatedProjects.Add(project.FileName);
			return project;
		}
		finally
		{
			projectCreateInformation.ProjectBasePath = projectBasePath;
			projectCreateInformation.ProjectName = projectName;
		}
	}
}

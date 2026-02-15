using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class ProjectTemplate : IComparable
{
	public const string TemplatePath = "/SharpDevelop/BackendBindings/Templates";

	private static List<ProjectTemplate> projectTemplates;

	private string originator;

	private string created;

	private string lastmodified;

	private string name;

	private string category;

	private string languagename;

	private string description;

	private string icon;

	private string wizardpath;

	private string subcategory;

	private string binaryProjectGeneratorPath;

	private string templateTarget;

	private string templateFamily;

	private string templateChain;

	private string templateName;

	private string templateMain;

	private bool templateAskForDct;

	private bool newProjectDialogVisible = true;

	private ArrayList actions = new ArrayList();

	private CombineDescriptor combineDescriptor;

	private ProjectDescriptor projectDescriptor;

	public static ReadOnlyCollection<ProjectTemplate> ProjectTemplates
	{
		get
		{
			if (projectTemplates == null)
			{
				LoadProjectTemplates();
			}
			return projectTemplates.AsReadOnly();
		}
	}

	public string WizardPath => wizardpath;

	public string Originator => originator;

	public string Created => created;

	public string LastModified => lastmodified;

	public string Name => name;

	public string Category => category;

	public string Subcategory => subcategory;

	public string LanguageName => languagename;

	public string Description => description;

	public string Icon => icon;

	public bool NewProjectDialogVisible => newProjectDialogVisible;

	[Browsable(false)]
	public CombineDescriptor CombineDescriptor => combineDescriptor;

	[Browsable(false)]
	public ProjectDescriptor ProjectDescriptor => projectDescriptor;

	public string TemplateChain => templateChain;

	public string TemplateFamily => templateFamily;

	public string TemplateName => templateName;

	public string TemplateTarget => templateTarget;

	public string TemplateMain => templateMain;

	public bool TemplateAskForDct => templateAskForDct;

	int IComparable.CompareTo(object other)
	{
		if (!(other is ProjectTemplate projectTemplate))
		{
			return -1;
		}
		int num = category.CompareTo(projectTemplate.category);
		if (num != 0)
		{
			return num;
		}
		return name.CompareTo(projectTemplate.name);
	}

	protected ProjectTemplate(string fileName)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(fileName);
		LoadFromXml(xmlDocument.DocumentElement, fileName);
	}

	private void LoadFromXml(XmlElement templateElement, string xmlFileName)
	{
		templateElement.SetAttribute("fileName", xmlFileName);
		originator = templateElement.GetAttribute("originator");
		created = templateElement.GetAttribute("created");
		lastmodified = templateElement.GetAttribute("lastModified");
		string attribute = templateElement.GetAttribute("newprojectdialogvisible");
		if (string.Equals(attribute, "false", StringComparison.OrdinalIgnoreCase))
		{
			newProjectDialogVisible = false;
		}
		XmlElement xmlElement = templateElement["TemplateConfiguration"];
		if (xmlElement["Wizard"] != null)
		{
			wizardpath = xmlElement["Wizard"].InnerText;
		}
		if (xmlElement["BinaryProjectGenerator"] != null)
		{
			binaryProjectGeneratorPath = xmlElement["BinaryProjectGenerator"].InnerText;
			templateFamily = "";
			if (xmlElement["BinaryProjectGenerator"].Attributes["family"] != null)
			{
				templateFamily = xmlElement["BinaryProjectGenerator"].Attributes["family"].Value;
			}
			templateChain = "";
			if (xmlElement["BinaryProjectGenerator"].Attributes["chain"] != null)
			{
				templateChain = xmlElement["BinaryProjectGenerator"].Attributes["chain"].Value;
			}
			templateName = "";
			if (xmlElement["BinaryProjectGenerator"].Attributes["name"] != null)
			{
				templateName = xmlElement["BinaryProjectGenerator"].Attributes["name"].Value;
			}
			templateTarget = "EXE";
			if (xmlElement["BinaryProjectGenerator"].Attributes["target"] != null)
			{
				templateTarget = xmlElement["BinaryProjectGenerator"].Attributes["target"].Value;
				if (!string.IsNullOrEmpty(templateTarget))
				{
					templateTarget = templateTarget.ToUpper().Trim();
					if (!(templateTarget == "EXE") && !(templateTarget == "DLL"))
					{
						templateTarget = "EXE";
					}
				}
			}
			if (xmlElement["BinaryProjectGenerator"].Attributes["main"] != null)
			{
				templateMain = xmlElement["BinaryProjectGenerator"].Attributes["main"].Value;
			}
			else
			{
				templateMain = "Main";
			}
			if (xmlElement["BinaryProjectGenerator"].Attributes["askfordct"] != null)
			{
				string value = xmlElement["BinaryProjectGenerator"].Attributes["askfordct"].Value;
				templateAskForDct = true;
				if (!string.IsNullOrEmpty(value))
				{
					value = value.ToUpper().Trim();
					if (!(value == "YES") && !(value == "TRUE"))
					{
						templateAskForDct = false;
					}
				}
			}
		}
		name = xmlElement["Name"].InnerText;
		category = xmlElement["Category"].InnerText;
		if (xmlElement["LanguageName"] != null)
		{
			languagename = xmlElement["LanguageName"].InnerText;
		}
		if (xmlElement["Subcategory"] != null)
		{
			subcategory = xmlElement["Subcategory"].InnerText;
		}
		if (xmlElement["Description"] != null)
		{
			description = xmlElement["Description"].InnerText;
		}
		if (xmlElement["Icon"] != null)
		{
			icon = xmlElement["Icon"].InnerText;
		}
		string directoryName = Path.GetDirectoryName(xmlFileName);
		if (templateElement["Solution"] != null)
		{
			combineDescriptor = CombineDescriptor.CreateCombineDescriptor(templateElement["Solution"], directoryName);
		}
		else if (templateElement["Combine"] != null)
		{
			combineDescriptor = CombineDescriptor.CreateCombineDescriptor(templateElement["Combine"], directoryName);
		}
		if (templateElement["Project"] != null)
		{
			projectDescriptor = new ProjectDescriptor(templateElement["Project"], directoryName);
		}
		if ((combineDescriptor == null && projectDescriptor == null) || (combineDescriptor != null && projectDescriptor != null))
		{
			throw new TemplateLoadException("Template must contain either Project or Solution node!");
		}
		if (templateElement["Actions"] == null)
		{
			return;
		}
		foreach (XmlElement item in templateElement["Actions"])
		{
			actions.Add(new OpenFileAction(item.Attributes["filename"].InnerText));
		}
	}

	[Conditional("DEBUG")]
	internal static void WarnObsoleteNode(XmlElement element, string message)
	{
		MessageService.ShowWarning("Obsolete node <" + element.Name + "> used in '" + element.OwnerDocument.DocumentElement.GetAttribute("fileName") + "':\n" + message);
	}

	[Conditional("DEBUG")]
	internal static void WarnObsoleteAttribute(XmlElement element, string attribute, string message)
	{
		MessageService.ShowWarning("Obsolete attribute <" + element.Name + " " + attribute + "=...>used in '" + element.OwnerDocument.DocumentElement.GetAttribute("fileName") + "':\n" + message);
	}

	[Conditional("DEBUG")]
	internal static void WarnAttributeMissing(XmlElement element, string attribute)
	{
		MessageService.ShowWarning("Missing attribute <" + element.Name + " " + attribute + "=...> in '" + element.OwnerDocument.DocumentElement.GetAttribute("fileName") + "'");
	}

	public string CreateProject(ProjectCreateInformation projectCreateInformation)
	{
		if (wizardpath != null)
		{
			Properties properties = new Properties();
			properties.Set("ProjectCreateInformation", projectCreateInformation);
			properties.Set("ProjectTemplate", this);
			using WizardDialog wizardDialog = new WizardDialog("Project Wizard", properties, wizardpath);
			if (wizardDialog.ShowDialog(WorkbenchSingleton.MainForm) != DialogResult.OK)
			{
				return null;
			}
		}
		if (binaryProjectGeneratorPath != null)
		{
			IBinaryProjectGenerator generator = BinaryProjectGeneratorLoader.GetGenerator(binaryProjectGeneratorPath);
			if (generator == null)
			{
				return null;
			}
			if (!generator.GenerateFiles(this, projectCreateInformation))
			{
				return null;
			}
			if (generator.ProjectCreated && !string.IsNullOrEmpty(generator.ProjectCreatedName))
			{
				return generator.ProjectCreatedName;
			}
		}
		if (combineDescriptor != null)
		{
			return combineDescriptor.CreateSolution(projectCreateInformation, languagename);
		}
		if (projectDescriptor != null)
		{
			if (projectCreateInformation.Solution == null)
			{
				projectCreateInformation.Solution = new Solution();
			}
			return projectDescriptor.CreateProject(projectCreateInformation, languagename)?.FileName;
		}
		return null;
	}

	public void RunOpenActions(ProjectCreateInformation projectCreateInformation)
	{
		foreach (OpenFileAction action in actions)
		{
			action.Run(projectCreateInformation);
		}
	}

	private static void LoadProjectTemplates()
	{
		projectTemplates = new List<ProjectTemplate>();
		string directory = FileUtility.Combine(PropertyService.DataDirectory, "templates", "project");
		List<string> list = FileUtility.SearchDirectory(directory, "*.xpt");
		foreach (string item in AddInTree.BuildItems("/SharpDevelop/BackendBindings/Templates", null, throwOnNotFound: false))
		{
			list.AddRange(FileUtility.SearchDirectory(item, "*.xpt"));
		}
		foreach (string item2 in list)
		{
			try
			{
				projectTemplates.Add(new ProjectTemplate(item2));
			}
			catch (XmlException ex)
			{
				MessageService.ShowError(ResourceService.GetString("Internal.Templates.ProjectTemplate.LoadingError") + "\n(" + item2 + ")\n" + ex.Message);
			}
			catch (TemplateLoadException ex2)
			{
				MessageService.ShowError(ResourceService.GetString("Internal.Templates.ProjectTemplate.LoadingError") + "\n(" + item2 + ")\n" + ex2.ToString());
			}
			catch (Exception ex3)
			{
				MessageService.ShowError(ex3, ResourceService.GetString("Internal.Templates.ProjectTemplate.LoadingError") + "\n(" + item2 + ")\n");
			}
		}
		projectTemplates.Sort();
	}
}

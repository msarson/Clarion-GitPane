using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.Core;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public static class MSBuildInternals
{
	private const string MSBuildXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

	private static readonly Regex configurationRegEx = new Regex("'(?<property>[^']*)'\\s*==\\s*'(?<value>[^']*)'", RegexOptions.Compiled);

	public static string Escape(string text)
	{
		return Utilities.Escape(text);
	}

	public static string Unescape(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		StringBuilder stringBuilder = null;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '%' && i + 2 < text.Length)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(text, 0, i, text.Length);
				}
				string s = text[i + 1].ToString() + text[i + 2];
				if (int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
				{
					stringBuilder.Append((char)result);
					i += 2;
				}
				else
				{
					stringBuilder.Append('%');
				}
			}
			else
			{
				stringBuilder?.Append(c);
			}
		}
		if (stringBuilder != null)
		{
			return stringBuilder.ToString();
		}
		return text;
	}

	internal static void AddItemToGroup(BuildItemGroup group, ProjectItem item)
	{
		if (group == null)
		{
			throw new ArgumentNullException("group");
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (item.IsAddedToProject)
		{
			throw new ArgumentException("item is already added to project", "item");
		}
		BuildItem buildItem = group.AddNewItem(item.ItemType.ToString(), item.Include, treatItemIncludeAsLiteral: true);
		foreach (string metadataName in item.MetadataNames)
		{
			buildItem.SetMetadata(metadataName, item.GetMetadata(metadataName));
		}
		item.BuildItem = buildItem;
	}

	internal static void EnsureCorrectTempProject(Microsoft.Build.BuildEngine.Project baseProject, string configuration, string platform, ref Microsoft.Build.BuildEngine.Project tempProject)
	{
		if (configuration == null && platform == null)
		{
			if (tempProject != null && tempProject != baseProject)
			{
				tempProject.ParentEngine.UnloadAllProjects();
			}
			tempProject = null;
			return;
		}
		if (configuration == null)
		{
			configuration = baseProject.GetEvaluatedProperty("Configuration");
		}
		if (platform == null)
		{
			platform = baseProject.GetEvaluatedProperty("Platform");
		}
		if (tempProject != null && tempProject.GetEvaluatedProperty("Configuration") == configuration && tempProject.GetEvaluatedProperty("Platform") == platform)
		{
			return;
		}
		if (baseProject.GetEvaluatedProperty("Configuration") == configuration && baseProject.GetEvaluatedProperty("Platform") == platform)
		{
			tempProject = baseProject;
			return;
		}
		if (tempProject != null && tempProject != baseProject)
		{
			tempProject.ParentEngine.UnloadAllProjects();
		}
		try
		{
			Engine engine = CreateEngine();
			tempProject = engine.CreateNewProject();
			MSBuildBasedProject.InitializeMSBuildProject(tempProject);
			tempProject.LoadXml(baseProject.Xml);
			tempProject.SetProperty("Configuration", configuration);
			tempProject.SetProperty("Platform", platform);
		}
		catch (Exception ex)
		{
			MessageService.ShowWarning(ex.ToString());
			tempProject = baseProject;
		}
	}

	internal static PropertyStorageLocations GetLocationFromCondition(string condition)
	{
		if (string.IsNullOrEmpty(condition))
		{
			return PropertyStorageLocations.Base;
		}
		PropertyStorageLocations propertyStorageLocations = PropertyStorageLocations.Unchanged;
		if (condition.Contains("$(Configuration)"))
		{
			propertyStorageLocations |= PropertyStorageLocations.ConfigurationSpecific;
		}
		if (condition.Contains("$(Platform)"))
		{
			propertyStorageLocations |= PropertyStorageLocations.PlatformSpecific;
		}
		return propertyStorageLocations;
	}

	internal static void GetConfigurationAndPlatformFromCondition(string condition, out string configuration, out string platform)
	{
		Match match = configurationRegEx.Match(condition);
		if (match.Success)
		{
			string text = match.Result("${property}");
			string text2 = match.Result("${value}");
			switch (text)
			{
			case "$(Configuration)|$(Platform)":
				configuration = AbstractProject.GetConfigurationNameFromKey(text2);
				platform = AbstractProject.GetPlatformNameFromKey(text2);
				break;
			case "$(Configuration)":
				configuration = text2;
				platform = null;
				break;
			case "$(Platform)":
				configuration = null;
				platform = text2;
				break;
			default:
				configuration = null;
				platform = null;
				break;
			}
		}
		else
		{
			configuration = null;
			platform = null;
		}
	}

	internal static bool EvaluateCondition(Microsoft.Build.BuildEngine.Project project, string configuration, string platform, string condition, ref Microsoft.Build.BuildEngine.Project tempProject)
	{
		if (string.IsNullOrEmpty(condition))
		{
			return true;
		}
		EnsureCorrectTempProject(project, configuration, platform, ref tempProject);
		return EvaluateCondition(tempProject, condition);
	}

	internal static bool EvaluateCondition(Microsoft.Build.BuildEngine.Project project, string condition)
	{
		BuildPropertyGroup buildPropertyGroup = project.AddNewPropertyGroup(insertAtEndOfProject: true);
		buildPropertyGroup.AddNewProperty("MSBuildInternalsEvaluateConditionDummyPropertyName", "ConditionFalse");
		buildPropertyGroup.AddNewProperty("MSBuildInternalsEvaluateConditionDummyPropertyName", "ConditionTrue").Condition = condition;
		bool result = false;
		try
		{
			result = project.GetEvaluatedProperty("MSBuildInternalsEvaluateConditionDummyPropertyName") == "ConditionTrue";
		}
		catch (InvalidProjectFileException)
		{
		}
		project.RemovePropertyGroup(buildPropertyGroup);
		return result;
	}

	public static BuildProperty GetProperty(BuildPropertyGroup pg, string name)
	{
		return Linq.Find(Linq.CastTo<BuildProperty>(pg), (BuildProperty p) => p.Name == name);
	}

	public static Engine CreateEngine()
	{
		return new Engine(RuntimeEnvironment.GetRuntimeDirectory());
	}

	public static void ClearImports(Microsoft.Build.BuildEngine.Project project)
	{
		XmlElement xmlElement = BeginXmlManipulation(project);
		List<XmlNode> list = new List<XmlNode>();
		foreach (XmlNode childNode in xmlElement.ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Element && childNode.Name == "Import")
			{
				list.Add(childNode);
			}
		}
		foreach (XmlNode item in list)
		{
			xmlElement.RemoveChild(item);
		}
	}

	public static void SetImportProjectPath(MSBuildBasedProject project, Import import, string newRawPath)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		if (import == null)
		{
			throw new ArgumentNullException("import");
		}
		if (newRawPath == null)
		{
			throw new ArgumentNullException("newRawPath");
		}
		lock (project.SyncRoot)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)typeof(Import).InvokeMember("ProjectPathAttribute", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty, null, import, null);
			xmlAttribute.Value = newRawPath;
			EndXmlManipulation(project.MSBuildProject);
		}
		project.CreateItemsListFromMSBuild();
	}

	public static string[] GetCustomMetadataNames(BuildItem item)
	{
		object obj = typeof(BuildItem).InvokeMember("GetAllCustomMetadataNames", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, item, null);
		if (obj is ArrayList)
		{
			return (string[])((ArrayList)obj).ToArray(typeof(string));
		}
		return ((List<string>)obj).ToArray();
	}

	private static XmlElement CreateElement(XmlDocument document, string name)
	{
		return document.CreateElement(name, "http://schemas.microsoft.com/developer/msbuild/2003");
	}

	private static XmlElement BeginXmlManipulation(Microsoft.Build.BuildEngine.Project project)
	{
		return (XmlElement)typeof(Microsoft.Build.BuildEngine.Project).InvokeMember("ProjectElement", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty, null, project, null);
	}

	private static void EndXmlManipulation(Microsoft.Build.BuildEngine.Project project)
	{
		typeof(Microsoft.Build.BuildEngine.Project).InvokeMember("MarkProjectAsDirtyForReprocessXml", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, project, null);
	}
}

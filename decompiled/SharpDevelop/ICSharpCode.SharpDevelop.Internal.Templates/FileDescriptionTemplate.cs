using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class FileDescriptionTemplate
{
	private string name;

	private string language;

	private string content;

	private byte[] contentData;

	private string itemType;

	private Dictionary<string, string> metadata = new Dictionary<string, string>();

	private bool skip;

	private bool noScript;

	private static readonly Set<string> knownAttributes = new Set<string>("name", "language", "buildAction", "src", "binary", "noScript");

	public bool IsDependentFile => metadata.ContainsKey("DependentUpon");

	public bool Skip
	{
		get
		{
			return skip;
		}
		set
		{
			skip = value;
		}
	}

	public bool ProcessScripts => !noScript;

	public Dictionary<string, string> Metadata => metadata;

	public string Name => name;

	public string Language => language;

	public string Content => content;

	public byte[] ContentData => contentData;

	public FileDescriptionTemplate(XmlElement xml, string hintPath)
	{
		TemplateLoadException.AssertAttributeExists(xml, "name");
		name = xml.GetAttribute("name");
		language = xml.GetAttribute("language");
		if (xml.HasAttribute("noScript"))
		{
			string attribute = xml.GetAttribute("noScript");
			if (!string.IsNullOrEmpty(attribute))
			{
				bool.TryParse(attribute, out noScript);
			}
		}
		if (xml.HasAttribute("buildAction"))
		{
			itemType = xml.GetAttribute("buildAction");
		}
		foreach (XmlAttribute attribute2 in xml.Attributes)
		{
			string text = attribute2.Name;
			if (!knownAttributes.Contains(text))
			{
				if (text == "copyToOutputDirectory")
				{
					text = "CopyToOutputDirectory";
				}
				if (text == "dependentUpon")
				{
					text = "DependentUpon";
				}
				if (text == "subType")
				{
					text = "SubType";
				}
				metadata[text] = attribute2.Value;
			}
		}
		if (xml.HasAttribute("src"))
		{
			string text2 = Path.Combine(hintPath, StringParser.Parse(xml.GetAttribute("src")));
			try
			{
				if (xml.HasAttribute("binary") && bool.Parse(xml.GetAttribute("binary")))
				{
					contentData = File.ReadAllBytes(text2);
				}
				else
				{
					content = File.ReadAllText(text2);
				}
				return;
			}
			catch (Exception ex)
			{
				content = "Error reading content from " + text2 + ":\n" + ex.ToString();
				LoggingService.Warn(content);
				return;
			}
		}
		content = xml.InnerText;
	}

	public bool SetProjectItemProperties(ProjectItem projectItem)
	{
		if (projectItem == null)
		{
			throw new ArgumentNullException("projectItem");
		}
		if (itemType != null)
		{
			projectItem.ItemType = new ItemType(itemType);
		}
		foreach (KeyValuePair<string, string> metadatum in metadata)
		{
			projectItem.SetMetadata(metadatum.Key, StringParser.Parse(metadatum.Value));
		}
		if (itemType == null)
		{
			return metadata.Count > 0;
		}
		return true;
	}

	public FileDescriptionTemplate(string name, string language, string content)
	{
		this.name = name;
		this.language = language;
		this.content = content;
	}
}

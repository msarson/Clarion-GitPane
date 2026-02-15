using System;
using System.Collections.Generic;
using System.Xml;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class CodeTemplateGroup
{
	private List<string> extensions = new List<string>();

	private List<CodeTemplate> templates = new List<CodeTemplate>();

	public List<string> Extensions => extensions;

	public List<CodeTemplate> Templates => templates;

	public string[] ExtensionStrings
	{
		get
		{
			string[] array = new string[extensions.Count];
			extensions.CopyTo(array, 0);
			return array;
		}
		set
		{
			extensions.Clear();
			foreach (string text in value)
			{
				if (!extensions.Contains(text.Trim().ToLower()))
				{
					extensions.Add(text.Trim().ToLower());
				}
			}
		}
	}

	public CodeTemplateGroup(string extensions)
	{
		ExtensionStrings = extensions.Split(';');
	}

	public CodeTemplateGroup(XmlElement el)
	{
		if (el == null)
		{
			throw new ArgumentNullException("el");
		}
		string[] array = el.GetAttribute("extensions").Split(';');
		string[] array2 = array;
		foreach (string item in array2)
		{
			extensions.Add(item);
		}
		foreach (XmlNode childNode in el.ChildNodes)
		{
			if (childNode is XmlElement el2)
			{
				templates.Add(new CodeTemplate(el2));
			}
		}
	}

	public XmlElement ToXmlElement(XmlDocument doc)
	{
		if (doc == null)
		{
			throw new ArgumentNullException("doc");
		}
		XmlElement xmlElement = doc.CreateElement("CodeTemplateGroup");
		xmlElement.SetAttribute("extensions", string.Join(";", ExtensionStrings));
		foreach (CodeTemplate template in templates)
		{
			xmlElement.AppendChild(template.ToXmlElement(doc));
		}
		return xmlElement;
	}
}

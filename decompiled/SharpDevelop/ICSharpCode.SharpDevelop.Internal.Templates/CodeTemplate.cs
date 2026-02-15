using System;
using System.Xml;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class CodeTemplate
{
	private string shortcut = string.Empty;

	private string description = string.Empty;

	private string text = string.Empty;

	public string Shortcut
	{
		get
		{
			return shortcut;
		}
		set
		{
			shortcut = value;
		}
	}

	public string Description
	{
		get
		{
			return description;
		}
		set
		{
			description = value;
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	public CodeTemplate()
	{
	}

	public CodeTemplate(string shortcut, string description, string text)
	{
		this.shortcut = shortcut;
		this.description = description;
		this.text = text;
	}

	public CodeTemplate(XmlElement el)
	{
		if (el == null)
		{
			throw new ArgumentNullException("el");
		}
		if (el.Attributes["template"] == null || el.Attributes["description"] == null)
		{
			throw new Exception("CodeTemplate(XmlElement el) : template and description attributes must exist (check the CodeTemplate XML)");
		}
		Shortcut = el.GetAttribute("template");
		Description = el.GetAttribute("description");
		Text = el.InnerText;
	}

	public XmlElement ToXmlElement(XmlDocument doc)
	{
		if (doc == null)
		{
			throw new ArgumentNullException("doc");
		}
		XmlElement xmlElement = doc.CreateElement("CodeTemplate");
		xmlElement.SetAttribute("template", Shortcut);
		xmlElement.SetAttribute("description", Description);
		xmlElement.InnerText = Text;
		return xmlElement;
	}
}

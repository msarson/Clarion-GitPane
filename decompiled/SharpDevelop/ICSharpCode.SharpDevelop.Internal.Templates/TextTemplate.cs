using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class TextTemplate
{
	public class Entry
	{
		public string Display;

		public string Value;

		public Entry(XmlElement el)
		{
			Display = el.Attributes["display"].InnerText;
			Value = el.Attributes["value"].InnerText;
		}

		public override string ToString()
		{
			return Display;
		}
	}

	public static List<TextTemplate> TextTemplates;

	private string name;

	private List<Entry> entries = new List<Entry>();

	public string Name => name;

	public List<Entry> Entries => entries;

	public TextTemplate(string filename)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			name = xmlDocument.DocumentElement.Attributes["name"].InnerText;
			XmlNodeList childNodes = xmlDocument.DocumentElement.ChildNodes;
			foreach (XmlElement item in childNodes)
			{
				entries.Add(new Entry(item));
			}
		}
		catch (Exception inner)
		{
			throw new FileLoadException("Can't load standard sidebar template file", filename, inner);
		}
	}

	private static void LoadTextTemplate(string filename)
	{
		TextTemplates.Add(new TextTemplate(filename));
	}

	static TextTemplate()
	{
		TextTemplates = new List<TextTemplate>();
		List<string> list = FileUtility.SearchDirectory(FileUtility.Combine(PropertyService.DataDirectory, "options", "textlib"), "*.xml");
		foreach (string item in list)
		{
			LoadTextTemplate(item);
		}
	}
}

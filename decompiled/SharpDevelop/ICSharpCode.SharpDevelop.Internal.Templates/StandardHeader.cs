using System;
using System.Collections;
using System.IO;
using System.Xml;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class StandardHeader
{
	private static string version;

	private static string TemplateFileName;

	private static ArrayList standardHeaders;

	private string name;

	private string header;

	public static ArrayList StandardHeaders => standardHeaders;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public string Header
	{
		get
		{
			return header;
		}
		set
		{
			header = value;
		}
	}

	private static bool LoadHeaders(string fileName)
	{
		if (!File.Exists(fileName))
		{
			return false;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(fileName);
			if (xmlDocument.DocumentElement.GetAttribute("version") != version)
			{
				return false;
			}
			foreach (XmlElement childNode in xmlDocument.DocumentElement.ChildNodes)
			{
				standardHeaders.Add(new StandardHeader(childNode));
			}
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public static void StoreHeaders()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml("<StandardProperties version = \"" + version + "\" />");
		foreach (StandardHeader standardHeader in standardHeaders)
		{
			XmlElement xmlElement = xmlDocument.CreateElement("Property");
			xmlElement.SetAttribute("name", standardHeader.Name);
			xmlElement.InnerText = standardHeader.Header;
			xmlDocument.DocumentElement.AppendChild(xmlElement);
		}
		xmlDocument.Save(Path.Combine(PropertyService.ConfigDirectory, TemplateFileName));
		SetHeaders();
	}

	public static void SetHeaders()
	{
		foreach (StandardHeader standardHeader in standardHeaders)
		{
			StringParser.Properties[standardHeader.Name] = standardHeader.Header;
		}
	}

	static StandardHeader()
	{
		version = "1.0";
		TemplateFileName = "StandardHeader.xml";
		standardHeaders = new ArrayList();
		if (!LoadHeaders(Path.Combine(PropertyService.ConfigDirectory, TemplateFileName)) && !LoadHeaders(FileUtility.Combine(PropertyService.DataDirectory, "options", TemplateFileName)))
		{
			MessageService.ShowWarning("Can not load standard headers");
		}
	}

	public StandardHeader(XmlElement el)
	{
		name = el.GetAttribute("name");
		header = el.InnerText;
	}

	public override string ToString()
	{
		return Name.Substring("StandardHeader.".Length);
	}
}

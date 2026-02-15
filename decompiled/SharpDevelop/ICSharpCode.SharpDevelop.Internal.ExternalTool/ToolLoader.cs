using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Internal.ExternalTool;

public class ToolLoader
{
	private static string TOOLFILE;

	private static string TOOLFILEVERSION;

	private static List<ExternalTool> tool;

	public static List<ExternalTool> Tool
	{
		get
		{
			return tool;
		}
		set
		{
			tool = value;
		}
	}

	private static bool LoadToolsFromStream(string filename)
	{
		if (!File.Exists(filename))
		{
			return false;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(filename);
			if (xmlDocument.DocumentElement.Attributes["VERSION"].InnerText != TOOLFILEVERSION)
			{
				return false;
			}
			XmlNodeList childNodes = xmlDocument.DocumentElement.ChildNodes;
			foreach (XmlElement item in childNodes)
			{
				tool.Add(new ExternalTool(item));
			}
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private static void WriteToolsToFile(string fileName)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml("<TOOLS VERSION = \"" + TOOLFILEVERSION.ToString() + "\" />");
		foreach (ExternalTool item in tool)
		{
			xmlDocument.DocumentElement.AppendChild(item.ToXmlElement(xmlDocument));
		}
		FileUtility.ObservedSave(xmlDocument.Save, fileName, FileErrorPolicy.ProvideAlternative);
	}

	static ToolLoader()
	{
		TOOLFILE = "Clarion-tools.xml";
		TOOLFILEVERSION = "1";
		tool = new List<ExternalTool>();
		string[] array = PropertyService.Get("MergedTools").Split(',');
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string item in array2)
		{
			list.Add(item);
		}
		string text = FileUtility.Combine(PropertyService.DataDirectory, "options", "tools");
		if (FileUtility.EnsureFolder(text))
		{
			string[] files = Directory.GetFiles(text);
			foreach (string text2 in files)
			{
				if (!list.Contains(Path.GetFileNameWithoutExtension(text2)))
				{
					LoadToolsFromStream(text2);
					list.Add(Path.GetFileNameWithoutExtension(text2));
				}
			}
		}
		if (!LoadToolsFromStream(Path.Combine(PropertyService.ConfigDirectory, TOOLFILE)))
		{
			list.Clear();
			tool.Clear();
			if (FileUtility.EnsureFolder(text))
			{
				string[] files2 = Directory.GetFiles(text);
				foreach (string text3 in files2)
				{
					LoadToolsFromStream(text3);
					list.Add(Path.GetFileNameWithoutExtension(text3));
				}
			}
		}
		PropertyService.Set("MergedTools", string.Join(",", list.ToArray()));
		SaveTools();
	}

	public static void SaveTools()
	{
		WriteToolsToFile(Path.Combine(PropertyService.ConfigDirectory, TOOLFILE));
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class TemplateCategorySortOrderFile
{
	public const int UndefinedSortOrder = -1;

	public const string ProjectCategorySortOrderFileName = "ProjectCategorySortOrder.xml";

	public const string FileCategorySortOrderFileName = "FileCategorySortOrder.xml";

	private Dictionary<string, int> sortOrders = new Dictionary<string, int>();

	private static List<TemplateCategorySortOrderFile> projectCategorySortOrderFiles;

	private static List<TemplateCategorySortOrderFile> fileCategorySortOrderFiles;

	public TemplateCategorySortOrderFile(string fileName)
		: this(new XmlTextReader(new StreamReader(fileName, detectEncodingFromByteOrderMarks: true)))
	{
	}

	public TemplateCategorySortOrderFile(XmlTextReader reader)
	{
		using (reader)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(reader);
			foreach (XmlElement item in xmlDocument.DocumentElement.SelectNodes("Category"))
			{
				string text = StringParser.Parse(item.GetAttribute("Name"));
				if (text.Length > 0 && item.HasAttribute("SortOrder"))
				{
					sortOrders.Add(text, GetSortOrder(item.GetAttribute("SortOrder")));
				}
				foreach (XmlElement item2 in item.SelectNodes("Category"))
				{
					if (item2.HasAttribute("Name"))
					{
						sortOrders.Add(text + "," + StringParser.Parse(item2.GetAttribute("Name")), GetSortOrder(item2.GetAttribute("SortOrder")));
					}
				}
			}
		}
	}

	public int GetCategorySortOrder(string name)
	{
		if (sortOrders.ContainsKey(name))
		{
			return sortOrders[name];
		}
		return -1;
	}

	public int GetCategorySortOrder(string name, string subcategoryName)
	{
		string name2 = name + "," + subcategoryName;
		return GetCategorySortOrder(name2);
	}

	public static int GetProjectCategorySortOrder(string name)
	{
		if (projectCategorySortOrderFiles == null)
		{
			ReadProjectCategorySortOrderFiles();
		}
		foreach (TemplateCategorySortOrderFile projectCategorySortOrderFile in projectCategorySortOrderFiles)
		{
			int categorySortOrder = projectCategorySortOrderFile.GetCategorySortOrder(name);
			if (categorySortOrder != -1)
			{
				return categorySortOrder;
			}
		}
		return -1;
	}

	public static int GetProjectCategorySortOrder(string name, string subcategoryName)
	{
		string name2 = name + "," + subcategoryName;
		return GetProjectCategorySortOrder(name2);
	}

	public static int GetFileCategorySortOrder(string name)
	{
		if (fileCategorySortOrderFiles == null)
		{
			ReadFileCategorySortOrderFiles();
		}
		foreach (TemplateCategorySortOrderFile fileCategorySortOrderFile in fileCategorySortOrderFiles)
		{
			int categorySortOrder = fileCategorySortOrderFile.GetCategorySortOrder(name);
			if (categorySortOrder != -1)
			{
				return categorySortOrder;
			}
		}
		return -1;
	}

	public static int GetFileCategorySortOrder(string name, string subcategoryName)
	{
		string name2 = name + "," + subcategoryName;
		return GetFileCategorySortOrder(name2);
	}

	private int GetSortOrder(string s)
	{
		if (int.TryParse(s, out var result))
		{
			return result;
		}
		return -1;
	}

	private static void ReadProjectCategorySortOrderFiles()
	{
		projectCategorySortOrderFiles = new List<TemplateCategorySortOrderFile>();
		string directory = FileUtility.Combine(PropertyService.DataDirectory, "templates", "project");
		List<string> list = FileUtility.SearchDirectory(directory, "ProjectCategorySortOrder.xml");
		foreach (string item in AddInTree.BuildItems("/SharpDevelop/BackendBindings/Templates", null, throwOnNotFound: false))
		{
			list.AddRange(FileUtility.SearchDirectory(item, "ProjectCategorySortOrder.xml"));
		}
		foreach (string item2 in list)
		{
			try
			{
				projectCategorySortOrderFiles.Add(new TemplateCategorySortOrderFile(item2));
			}
			catch (Exception ex)
			{
				LoggingService.Debug("Failed to load project category sort order file: " + item2 + " : " + ex.ToString());
			}
		}
	}

	private static void ReadFileCategorySortOrderFiles()
	{
		fileCategorySortOrderFiles = new List<TemplateCategorySortOrderFile>();
		string directory = FileUtility.Combine(PropertyService.DataDirectory, "templates", "file");
		List<string> list = FileUtility.SearchDirectory(directory, "FileCategorySortOrder.xml");
		foreach (string item in AddInTree.BuildItems("/SharpDevelop/BackendBindings/Templates", null, throwOnNotFound: false))
		{
			list.AddRange(FileUtility.SearchDirectory(item, "FileCategorySortOrder.xml"));
		}
		foreach (string item2 in list)
		{
			try
			{
				fileCategorySortOrderFiles.Add(new TemplateCategorySortOrderFile(item2));
			}
			catch (Exception ex)
			{
				LoggingService.Debug("Failed to load project category sort order file: " + item2 + " : " + ex.ToString());
			}
		}
	}
}

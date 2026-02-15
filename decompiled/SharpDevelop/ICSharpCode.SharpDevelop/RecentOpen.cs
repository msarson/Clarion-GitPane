using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class RecentOpen
{
	public class RecentOpenDescription
	{
		private string fileName;

		private Properties prop;

		public string FileName
		{
			get
			{
				return fileName;
			}
			set
			{
				fileName = value;
			}
		}

		public Properties AdditionalProperties => prop;

		public RecentOpenDescription(string fileName, Properties prop)
		{
			this.fileName = fileName;
			this.prop = prop;
		}
	}

	public static string defaultTypeFiles = "File";

	public static string defaultTypeProjects = "Project";

	public static string defaultApp1 = ".app";

	public static string defaultApp2 = ".appx";

	private static int MAX_LENGTH = 16;

	private Dictionary<string, List<RecentOpenDescription>> recents;

	private static string addinTreePath = "/SharpDevelop/RecentOpenCategories";

	public IEnumerable<string> RecentOpenCategories
	{
		get
		{
			foreach (string key in recents.Keys)
			{
				yield return key;
			}
		}
	}

	public static bool RemoveMissingFileEnties
	{
		get
		{
			return PropertyService.Get("RemoveMissingRecents", defaultValue: true);
		}
		set
		{
			PropertyService.Set("RemoveMissingRecents", value);
		}
	}

	public static int MaximumEntriesPerCategory
	{
		get
		{
			return PropertyService.Get("MaximumRecentEntries", MAX_LENGTH);
		}
		set
		{
			PropertyService.Set("MaximumRecentEntries", value);
		}
	}

	public event RecentOpenEventHandler RecentChanged;

	public bool IsCategoryExists(string categoryName)
	{
		return recents.ContainsKey(categoryName);
	}

	public ReadOnlyCollection<RecentOpenDescription> GetRecentsFromCategory(string categoryName)
	{
		if (!recents.ContainsKey(categoryName))
		{
			return new List<RecentOpenDescription>().AsReadOnly();
		}
		return recents[categoryName].AsReadOnly();
	}

	private void OnRecentChange(string category)
	{
		if (this.RecentChanged != null)
		{
			this.RecentChanged(this, new RecentOpenEventArgs(category));
		}
	}

	public RecentOpen()
	{
		recents = new Dictionary<string, List<RecentOpenDescription>>();
		string[] array = (string[])AddInTree.GetTreeNode(addinTreePath).BuildChildItems(this).ToArray(typeof(string));
		for (int i = 0; i < array.Length; i++)
		{
			recents.Add(array[i], new List<RecentOpenDescription>());
		}
	}

	public RecentOpen(Properties p)
		: this()
	{
		try
		{
			bool flag = !RemoveMissingFileEnties;
			int maximumEntriesPerCategory = MaximumEntriesPerCategory;
			bool flag2 = false;
			foreach (string recentOpenCategory in RecentOpenCategories)
			{
				if (!p.Contains(recentOpenCategory))
				{
					continue;
				}
				flag2 = false;
				if (p.Get(recentOpenCategory) is Properties)
				{
					Properties properties = (Properties)p.Get(recentOpenCategory);
					string[] elements = properties.Elements;
					foreach (string text in elements)
					{
						if (!(properties.Get(text) is Properties))
						{
							continue;
						}
						RecentOpenDescription recentOpenDescription = new RecentOpenDescription(text, (Properties)properties.Get(text));
						if (File.Exists(text) || flag || (recentOpenDescription.AdditionalProperties != null && recentOpenDescription.AdditionalProperties.Get("AlwaysShow", defaultValue: false)))
						{
							if (recents[recentOpenCategory].Count >= maximumEntriesPerCategory)
							{
								flag2 = true;
								break;
							}
							recents[recentOpenCategory].Add(recentOpenDescription);
						}
					}
				}
				else
				{
					string[] array = p[recentOpenCategory].Split(',');
					string[] array2 = array;
					foreach (string text2 in array2)
					{
						if (File.Exists(text2) || flag)
						{
							if (recents[recentOpenCategory].Count >= maximumEntriesPerCategory)
							{
								flag2 = true;
								break;
							}
							recents[recentOpenCategory].Add(new RecentOpenDescription(text2, null));
						}
					}
				}
				if (flag2)
				{
					OnRecentChange(recentOpenCategory);
				}
			}
		}
		catch
		{
		}
	}

	public void RemoveItem(string category, int index)
	{
		if (recents.ContainsKey(category) && index < recents[category].Count)
		{
			recents[category].RemoveAt(index);
		}
	}

	public void AddLastItem(string category, string name, Properties prop)
	{
		if (!recents.ContainsKey(category))
		{
			return;
		}
		string extension = Path.GetExtension(name);
		if (extension != null && (extension.Equals(defaultApp1, StringComparison.OrdinalIgnoreCase) || extension.Equals(defaultApp2, StringComparison.OrdinalIgnoreCase)) && category.Equals("file", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		List<RecentOpenDescription> list = recents[category];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].FileName.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				list.RemoveAt(i);
			}
		}
		while (list.Count >= PropertyService.Get("MaximumRecentEntries", MAX_LENGTH))
		{
			list.RemoveAt(list.Count - 1);
		}
		if (list.Count > 0)
		{
			list.Insert(0, new RecentOpenDescription(name, prop));
		}
		else
		{
			list.Add(new RecentOpenDescription(name, prop));
		}
		OnRecentChange(category);
	}

	public void ClearRecentItems(string category)
	{
		if (recents.ContainsKey(category))
		{
			recents[category].Clear();
			OnRecentChange(category);
		}
	}

	public void ClearAllRecentItems()
	{
		foreach (KeyValuePair<string, List<RecentOpenDescription>> recent in recents)
		{
			recent.Value.Clear();
			OnRecentChange(recent.Key);
		}
	}

	public static RecentOpen FromXmlElement(Properties properties)
	{
		return new RecentOpen(properties);
	}

	public Properties ToProperties()
	{
		Properties properties = new Properties();
		foreach (KeyValuePair<string, List<RecentOpenDescription>> recent in recents)
		{
			bool flag = false;
			foreach (RecentOpenDescription item in recent.Value)
			{
				if (item.AdditionalProperties != null)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				Properties properties2 = new Properties();
				for (int i = 0; i < recent.Value.Count; i++)
				{
					Properties value = ((recent.Value[i].AdditionalProperties != null) ? recent.Value[i].AdditionalProperties : new Properties());
					properties2.Set(recent.Value[i].FileName, value);
				}
				properties.Set(recent.Key, properties2);
				continue;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int j = 0; j < recent.Value.Count; j++)
			{
				if (j > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(recent.Value[j].FileName);
			}
			properties[recent.Key] = stringBuilder.ToString();
		}
		return properties;
	}

	public void FileRemoved(object sender, FileEventArgs e)
	{
		foreach (KeyValuePair<string, List<RecentOpenDescription>> recent in recents)
		{
			for (int i = 0; i < recent.Value.Count; i++)
			{
				if (e.FileName == recent.Value[i].FileName)
				{
					recent.Value.RemoveAt(i);
					OnRecentChange(recent.Key);
				}
			}
		}
	}

	public void FileRenamed(object sender, FileRenameEventArgs e)
	{
		foreach (KeyValuePair<string, List<RecentOpenDescription>> recent in recents)
		{
			for (int i = 0; i < recent.Value.Count; i++)
			{
				if (e.SourceFile == recent.Value[i].FileName)
				{
					recent.Value[i].FileName = e.TargetFile;
					OnRecentChange(recent.Key);
				}
			}
		}
	}
}

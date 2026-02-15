using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ICSharpCode.Core;

public static class AddInManager
{
	private static string configurationFileName;

	private static string addInInstallTemp;

	private static string userAddInPath;

	public static string UserAddInPath
	{
		get
		{
			return userAddInPath;
		}
		set
		{
			userAddInPath = value;
		}
	}

	public static string AddInInstallTemp
	{
		get
		{
			return addInInstallTemp;
		}
		set
		{
			addInInstallTemp = value;
		}
	}

	public static string ConfigurationFileName
	{
		get
		{
			return configurationFileName;
		}
		set
		{
			configurationFileName = value;
		}
	}

	public static void InstallAddIns(List<string> disabled)
	{
		if (!Directory.Exists(addInInstallTemp))
		{
			return;
		}
		LoggingService.Info("AddInManager.InstallAddIns started");
		if (!Directory.Exists(userAddInPath))
		{
			Directory.CreateDirectory(userAddInPath);
		}
		string path = Path.Combine(addInInstallTemp, "remove.txt");
		bool flag = true;
		List<string> list = new List<string>();
		if (File.Exists(path))
		{
			using (StreamReader streamReader = new StreamReader(path))
			{
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					text = text.Trim();
					if (text.Length != 0)
					{
						string targetDir = Path.Combine(userAddInPath, text);
						if (!UninstallAddIn(disabled, text, targetDir))
						{
							list.Add(text);
							flag = false;
						}
					}
				}
			}
			if (list.Count == 0)
			{
				LoggingService.Info("Deleting remove.txt");
				File.Delete(path);
			}
			else
			{
				LoggingService.Info("Rewriting remove.txt");
				using StreamWriter streamWriter = new StreamWriter(path);
				list.ForEach(streamWriter.WriteLine);
			}
		}
		string[] directories = Directory.GetDirectories(addInInstallTemp);
		foreach (string text2 in directories)
		{
			string fileName = Path.GetFileName(text2);
			string text3 = Path.Combine(userAddInPath, fileName);
			if (list.Contains(fileName))
			{
				LoggingService.Info("Skipping installation of " + fileName + " because deinstallation failed.");
			}
			else if (UninstallAddIn(disabled, fileName, text3))
			{
				LoggingService.Info("Installing " + fileName + "...");
				Directory.Move(text2, text3);
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			try
			{
				Directory.Delete(addInInstallTemp, recursive: false);
			}
			catch (Exception exception)
			{
				LoggingService.Warn("Error removing install temp", exception);
			}
		}
		LoggingService.Info("AddInManager.InstallAddIns finished");
	}

	private static bool UninstallAddIn(List<string> disabled, string addInName, string targetDir)
	{
		if (Directory.Exists(targetDir))
		{
			LoggingService.Info("Removing " + addInName + "...");
			try
			{
				Directory.Delete(targetDir, recursive: true);
			}
			catch (Exception ex)
			{
				disabled.Add(addInName);
				MessageService.ShowError("Error removing " + addInName + ":\n" + ex.Message + "\nThe AddIn will be removed on the next start of " + MessageService.ProductName + " and is disabled for now.");
				return false;
			}
		}
		return true;
	}

	public static void RemoveUserAddInOnNextStart(string identity)
	{
		List<string> list = new List<string>();
		string path = Path.Combine(addInInstallTemp, "remove.txt");
		if (File.Exists(path))
		{
			using (StreamReader streamReader = new StreamReader(path))
			{
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					text = text.Trim();
					if (text.Length > 0)
					{
						list.Add(text);
					}
				}
			}
			if (list.Contains(identity))
			{
				return;
			}
		}
		list.Add(identity);
		if (!Directory.Exists(addInInstallTemp))
		{
			Directory.CreateDirectory(addInInstallTemp);
		}
		using StreamWriter streamWriter = new StreamWriter(path);
		list.ForEach(streamWriter.WriteLine);
	}

	public static void AbortRemoveUserAddInOnNextStart(string identity)
	{
		string path = Path.Combine(addInInstallTemp, "remove.txt");
		if (!File.Exists(path))
		{
			return;
		}
		List<string> list = new List<string>();
		using (StreamReader streamReader = new StreamReader(path))
		{
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				text = text.Trim();
				if (text.Length > 0)
				{
					list.Add(text);
				}
			}
		}
		if (!list.Remove(identity))
		{
			return;
		}
		using StreamWriter streamWriter = new StreamWriter(path);
		list.ForEach(streamWriter.WriteLine);
	}

	public static void AddExternalAddIns(IList<AddIn> addIns)
	{
		List<string> list = new List<string>();
		List<string> disabledAddIns = new List<string>();
		LoadAddInConfiguration(list, disabledAddIns);
		foreach (AddIn addIn in addIns)
		{
			if (!list.Contains(addIn.FileName))
			{
				list.Add(addIn.FileName);
			}
			addIn.Enabled = false;
			addIn.Action = AddInAction.Install;
			AddInTree.InsertAddIn(addIn);
		}
		SaveAddInConfiguration(list, disabledAddIns);
	}

	public static void RemoveExternalAddIns(IList<AddIn> addIns)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		LoadAddInConfiguration(list, list2);
		foreach (AddIn addIn in addIns)
		{
			foreach (string key in addIn.Manifest.Identities.Keys)
			{
				list2.Remove(key);
			}
			list.Remove(addIn.FileName);
			addIn.Action = AddInAction.Uninstall;
			if (!addIn.Enabled)
			{
				AddInTree.RemoveAddIn(addIn);
			}
		}
		SaveAddInConfiguration(list, list2);
	}

	public static void Enable(IList<AddIn> addIns)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		LoadAddInConfiguration(list, list2);
		foreach (AddIn addIn in addIns)
		{
			foreach (string key in addIn.Manifest.Identities.Keys)
			{
				list2.Remove(key);
			}
			if (addIn.Action == AddInAction.Uninstall)
			{
				if (FileUtility.IsBaseDirectory(userAddInPath, addIn.FileName))
				{
					foreach (string key2 in addIn.Manifest.Identities.Keys)
					{
						AbortRemoveUserAddInOnNextStart(key2);
					}
				}
				else if (!list.Contains(addIn.FileName))
				{
					list.Add(addIn.FileName);
				}
			}
			addIn.Action = AddInAction.Enable;
		}
		SaveAddInConfiguration(list, list2);
	}

	public static void Disable(IList<AddIn> addIns)
	{
		List<string> addInFiles = new List<string>();
		List<string> list = new List<string>();
		LoadAddInConfiguration(addInFiles, list);
		foreach (AddIn addIn in addIns)
		{
			string primaryIdentity = addIn.Manifest.PrimaryIdentity;
			if (primaryIdentity == null)
			{
				throw new ArgumentException("The AddIn cannot be disabled because it has no identity.");
			}
			if (!list.Contains(primaryIdentity))
			{
				list.Add(primaryIdentity);
			}
			addIn.Action = AddInAction.Disable;
		}
		SaveAddInConfiguration(addInFiles, list);
	}

	public static void LoadAddInConfiguration(List<string> addInFiles, List<string> disabledAddIns)
	{
		if (!File.Exists(configurationFileName))
		{
			return;
		}
		using XmlTextReader xmlTextReader = new XmlTextReader(configurationFileName);
		string text = null;
		string text2 = null;
		bool flag = false;
		while (xmlTextReader.Read())
		{
			if (xmlTextReader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			if (xmlTextReader.Name == "AddIn")
			{
				text = xmlTextReader.GetAttribute("file");
				if (text == null || text.Length <= 0)
				{
					continue;
				}
				flag = false;
				foreach (string addInFile in addInFiles)
				{
					if (addInFile.Equals(text, StringComparison.OrdinalIgnoreCase))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					addInFiles.Add(text);
				}
			}
			else if (xmlTextReader.Name == "Disable")
			{
				text2 = xmlTextReader.GetAttribute("addin");
				if (text2 != null && text2.Length > 0)
				{
					disabledAddIns.Add(text2);
				}
			}
		}
	}

	public static void SaveAddInConfiguration(List<string> addInFiles, List<string> disabledAddIns)
	{
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(configurationFileName, Encoding.UTF8);
		xmlTextWriter.Formatting = Formatting.Indented;
		xmlTextWriter.WriteStartDocument();
		xmlTextWriter.WriteStartElement("AddInConfiguration");
		foreach (string addInFile in addInFiles)
		{
			xmlTextWriter.WriteStartElement("AddIn");
			xmlTextWriter.WriteAttributeString("file", addInFile);
			xmlTextWriter.WriteEndElement();
		}
		foreach (string disabledAddIn in disabledAddIns)
		{
			xmlTextWriter.WriteStartElement("Disable");
			xmlTextWriter.WriteAttributeString("addin", disabledAddIn);
			xmlTextWriter.WriteEndElement();
		}
		xmlTextWriter.WriteEndDocument();
	}
}

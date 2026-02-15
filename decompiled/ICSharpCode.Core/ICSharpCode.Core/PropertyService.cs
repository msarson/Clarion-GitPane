using System;
using System.IO;
using System.Text;
using System.Xml;

namespace ICSharpCode.Core;

public static class PropertyService
{
	private static string propertyFileName;

	private static string propertyXmlRootNodeName;

	private static string configDirectory;

	private static string dataDirectory;

	private static Properties properties;

	private static bool KeepPreviousFile = true;

	public static bool Initialized => properties != null;

	public static string ConfigDirectory => configDirectory;

	public static string DataDirectory => dataDirectory;

	public static event PropertyChangedEventHandler PropertyChanged;

	public static void InitializeService(string configDirectory, string dataDirectory, string propertiesName)
	{
		if (properties != null)
		{
			throw new InvalidOperationException("Service is already initialized.");
		}
		if (configDirectory == null || dataDirectory == null || propertiesName == null)
		{
			throw new ArgumentNullException();
		}
		properties = new Properties();
		if (configDirectory.EndsWith("\\"))
		{
			configDirectory = configDirectory.Substring(0, configDirectory.Length - 1);
		}
		PropertyService.configDirectory = configDirectory;
		PropertyService.dataDirectory = dataDirectory;
		propertyXmlRootNodeName = propertiesName;
		propertyFileName = propertiesName + ".xml";
		properties.PropertyChanged += PropertiesPropertyChanged;
	}

	public static string Get(string property)
	{
		return properties[property];
	}

	public static T Get<T>(string property, T defaultValue)
	{
		if (properties != null)
		{
			return properties.Get(property, defaultValue);
		}
		return defaultValue;
	}

	public static T Get<T>(string property, T defaultValue, string subPropertyName, params string[] subPropertyNames)
	{
		Properties subProperties = GetSubProperties(subPropertyName, subPropertyNames);
		if (subProperties != null)
		{
			return subProperties.Get(property, defaultValue);
		}
		return defaultValue;
	}

	public static Properties GetSubProperties(string subPropertyName, params string[] subPropertyNames)
	{
		Properties properties = Get(subPropertyName, new Properties());
		for (int i = 0; i < subPropertyNames.Length && !string.IsNullOrEmpty(subPropertyNames[i]); i++)
		{
			properties = properties.Get(subPropertyNames[i], new Properties());
		}
		return properties;
	}

	public static void Remove(string property)
	{
		properties.Remove(property);
	}

	public static void Set<T>(string property, T value)
	{
		properties.Set(property, value);
	}

	public static void Set<T>(string property, T value, string subPropertyName, params string[] subPropertyNames)
	{
		GetSubProperties(subPropertyName, subPropertyNames)?.Set(property, value);
	}

	public static void Load()
	{
		if (properties == null)
		{
			throw new InvalidOperationException("Service is not initialized.");
		}
		if (!Directory.Exists(configDirectory))
		{
			Directory.CreateDirectory(configDirectory);
		}
		if (!LoadPropertiesFromStream(Path.Combine(configDirectory, propertyFileName)))
		{
			LoadPropertiesFromStream(FileUtility.Combine(DataDirectory, "options", propertyFileName));
		}
	}

	public static bool LoadPropertiesFromStream(string fileName)
	{
		if (!File.Exists(fileName))
		{
			return false;
		}
		if (KeepPreviousFile)
		{
			FileInfo fileInfo = new FileInfo(fileName);
			if (fileInfo.Length == 0)
			{
				string text = fileName + ".old";
				if (File.Exists(text))
				{
					File.Copy(text, fileName, overwrite: true);
				}
			}
		}
		try
		{
			using XmlTextReader xmlTextReader = new XmlTextReader(fileName);
			while (xmlTextReader.Read())
			{
				if (xmlTextReader.IsStartElement() && xmlTextReader.LocalName == propertyXmlRootNodeName)
				{
					properties.ReadProperties(xmlTextReader, propertyXmlRootNodeName);
					return true;
				}
			}
			xmlTextReader.Close();
		}
		catch (XmlException ex)
		{
			MessageService.ShowError("Error loading properties: " + ex.Message + "\nSettings have been restored to default values.");
		}
		return false;
	}

	public static void Save()
	{
		if (!Directory.Exists(configDirectory))
		{
			Directory.CreateDirectory(configDirectory);
		}
		string text = Path.Combine(configDirectory, propertyFileName);
		string text2 = text + ".tmp";
		if (File.Exists(text2))
		{
			File.Delete(text2);
		}
		using (XmlTextWriter xmlTextWriter = new XmlTextWriter(text2, Encoding.UTF8))
		{
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlTextWriter.WriteStartElement(propertyXmlRootNodeName);
			properties.WriteProperties(xmlTextWriter);
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.Close();
		}
		if (KeepPreviousFile)
		{
			string text3 = text + ".old";
			if (File.Exists(text))
			{
				File.Replace(text2, text, text3, ignoreMetadataErrors: true);
				return;
			}
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
			File.Copy(text2, text3);
			File.Move(text2, text);
		}
		else
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			File.Move(text2, text);
		}
	}

	private static void PropertiesPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (PropertyService.PropertyChanged != null)
		{
			PropertyService.PropertyChanged(null, e);
		}
	}
}

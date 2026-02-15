using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;

namespace ICSharpCode.Core;

public class Properties
{
	private Dictionary<string, object> properties = new Dictionary<string, object>();

	public string this[string property]
	{
		get
		{
			return Convert.ToString(Get(property));
		}
		set
		{
			Set(property, value);
		}
	}

	public string[] Elements
	{
		get
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, object> property in properties)
			{
				list.Add(property.Key);
			}
			return list.ToArray();
		}
	}

	public int Count => properties.Count;

	public event PropertyChangedEventHandler PropertyChanged;

	public object Get(string property)
	{
		if (!properties.ContainsKey(property))
		{
			return null;
		}
		return properties[property];
	}

	public void Set<T>(string property, T value)
	{
		T val = default(T);
		if (!properties.ContainsKey(property))
		{
			properties.Add(property, value);
		}
		else
		{
			val = Get(property, value);
			properties[property] = value;
		}
		OnPropertyChanged(new PropertyChangedEventArgs(this, property, val, value));
	}

	public bool Contains(string property)
	{
		return properties.ContainsKey(property);
	}

	public bool Remove(string property)
	{
		return properties.Remove(property);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[Properties:{");
		foreach (KeyValuePair<string, object> property in properties)
		{
			stringBuilder.Append(property.Key);
			stringBuilder.Append("=");
			stringBuilder.Append(property.Value);
			stringBuilder.Append(",");
		}
		stringBuilder.Append("}]");
		return stringBuilder.ToString();
	}

	public static Properties ReadFromAttributes(XmlReader reader)
	{
		Properties properties = new Properties();
		if (reader.HasAttributes)
		{
			for (int i = 0; i < reader.AttributeCount; i++)
			{
				reader.MoveToAttribute(i);
				properties[reader.Name] = reader.Value;
			}
			reader.MoveToElement();
		}
		return properties;
	}

	public void ReadProperties(XmlReader reader, string endElement)
	{
		if (reader.IsEmptyElement)
		{
			return;
		}
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.EndElement:
				if (reader.LocalName == endElement)
				{
					return;
				}
				break;
			case XmlNodeType.Element:
			{
				string localName = reader.LocalName;
				if (localName == "Properties")
				{
					localName = reader.GetAttribute(0);
					Properties properties = new Properties();
					properties.ReadProperties(reader, "Properties");
					this.properties[localName] = properties;
				}
				else if (localName == "Array")
				{
					localName = reader.GetAttribute(0);
					this.properties[localName] = ReadArray(reader);
				}
				else
				{
					this.properties[localName] = (reader.HasAttributes ? reader.GetAttribute(0) : null);
				}
				break;
			}
			}
		}
	}

	private ArrayList ReadArray(XmlReader reader)
	{
		if (reader.IsEmptyElement)
		{
			return new ArrayList(0);
		}
		ArrayList arrayList = new ArrayList();
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.EndElement:
				if (reader.LocalName == "Array")
				{
					return arrayList;
				}
				break;
			case XmlNodeType.Element:
				arrayList.Add(reader.HasAttributes ? reader.GetAttribute(0) : null);
				break;
			}
		}
		return arrayList;
	}

	public void WriteProperties(XmlWriter writer)
	{
		foreach (KeyValuePair<string, object> property in properties)
		{
			object value = property.Value;
			if (value is Properties)
			{
				writer.WriteStartElement("Properties");
				writer.WriteAttributeString("name", property.Key);
				((Properties)value).WriteProperties(writer);
				writer.WriteEndElement();
			}
			else if (value is Array || value is ArrayList)
			{
				writer.WriteStartElement("Array");
				writer.WriteAttributeString("name", property.Key);
				foreach (object item in (IEnumerable)value)
				{
					writer.WriteStartElement("Element");
					WriteValue(writer, item);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
			else
			{
				writer.WriteStartElement(property.Key);
				WriteValue(writer, value);
				writer.WriteEndElement();
			}
		}
	}

	private void WriteValue(XmlWriter writer, object val)
	{
		if (val != null)
		{
			if (val is string)
			{
				writer.WriteAttributeString("value", val.ToString());
				return;
			}
			TypeConverter converter = TypeDescriptor.GetConverter(val.GetType());
			writer.WriteAttributeString("value", converter.ConvertToInvariantString(val));
		}
	}

	public void Save(string fileName)
	{
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(fileName, Encoding.UTF8);
		xmlTextWriter.Formatting = Formatting.Indented;
		xmlTextWriter.WriteStartElement("Properties");
		WriteProperties(xmlTextWriter);
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.Close();
	}

	public static Properties Load(string fileName)
	{
		if (!File.Exists(fileName))
		{
			return null;
		}
		using (XmlTextReader xmlTextReader = new XmlTextReader(fileName))
		{
			while (xmlTextReader.Read())
			{
				string localName;
				if (xmlTextReader.IsStartElement() && (localName = xmlTextReader.LocalName) != null && localName == "Properties")
				{
					Properties properties = new Properties();
					properties.ReadProperties(xmlTextReader, "Properties");
					return properties;
				}
			}
			xmlTextReader.Close();
		}
		return null;
	}

	public T Get<T>(string property, T defaultValue)
	{
		if (!properties.ContainsKey(property))
		{
			properties.Add(property, defaultValue);
			return defaultValue;
		}
		object obj = properties[property];
		if (obj is string && typeof(T) != typeof(string))
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			try
			{
				obj = converter.ConvertFromInvariantString(obj.ToString());
			}
			catch (Exception ex)
			{
				MessageService.ShowWarning("Error loading property '" + property + "': " + ex.Message);
				obj = defaultValue;
			}
			properties[property] = obj;
		}
		else if (obj is ArrayList && typeof(T).IsArray)
		{
			ArrayList arrayList = (ArrayList)obj;
			Type elementType = typeof(T).GetElementType();
			Array array = Array.CreateInstance(elementType, arrayList.Count);
			TypeConverter converter2 = TypeDescriptor.GetConverter(elementType);
			try
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (arrayList[i] != null)
					{
						array.SetValue(converter2.ConvertFromInvariantString(arrayList[i].ToString()), i);
					}
				}
				obj = array;
			}
			catch (Exception ex2)
			{
				MessageService.ShowWarning("Error loading property '" + property + "': " + ex2.Message);
				obj = defaultValue;
			}
			properties[property] = obj;
		}
		else if (!(obj is string) && typeof(T) == typeof(string))
		{
			TypeConverter converter3 = TypeDescriptor.GetConverter(typeof(T));
			obj = ((!converter3.CanConvertTo(typeof(string))) ? obj.ToString() : converter3.ConvertToInvariantString(obj));
		}
		try
		{
			return (T)obj;
		}
		catch (NullReferenceException)
		{
			return defaultValue;
		}
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}
}

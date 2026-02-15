using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ICSharpCode.Core;

public static class ObjectXMLSerializer<T> where T : class
{
	public static T Load(string path)
	{
		return LoadFromDocumentFormat(null, path, null);
	}

	public static T Load(string path, SerializedFormat serializedFormat)
	{
		T val = null;
		return serializedFormat switch
		{
			SerializedFormat.Binary => LoadFromBinaryFormat(path, null), 
			_ => LoadFromDocumentFormat(null, path, null), 
		};
	}

	public static T Load(Stream source, SerializedFormat serializedFormat)
	{
		T val = null;
		return serializedFormat switch
		{
			SerializedFormat.Binary => LoadFromBinaryFormat(source), 
			_ => LoadFromDocumentFormat(null, source), 
		};
	}

	public static T Load(string path, Type[] extraTypes)
	{
		return LoadFromDocumentFormat(extraTypes, path, null);
	}

	public static T Load(string fileName, IsolatedStorageFile isolatedStorageDirectory)
	{
		return LoadFromDocumentFormat(null, fileName, isolatedStorageDirectory);
	}

	public static T Load(string fileName, IsolatedStorageFile isolatedStorageDirectory, SerializedFormat serializedFormat)
	{
		T val = null;
		return serializedFormat switch
		{
			SerializedFormat.Binary => LoadFromBinaryFormat(fileName, isolatedStorageDirectory), 
			_ => LoadFromDocumentFormat(null, fileName, isolatedStorageDirectory), 
		};
	}

	public static T Load(string fileName, IsolatedStorageFile isolatedStorageDirectory, Type[] extraTypes)
	{
		return LoadFromDocumentFormat(null, fileName, isolatedStorageDirectory);
	}

	public static void Save(T serializableObject, string path)
	{
		SaveToDocumentFormat(serializableObject, null, path, null);
	}

	public static void Save(T serializableObject, string path, SerializedFormat serializedFormat)
	{
		switch (serializedFormat)
		{
		case SerializedFormat.Binary:
			SaveToBinaryFormat(serializableObject, path, null);
			break;
		default:
			SaveToDocumentFormat(serializableObject, null, path, null);
			break;
		}
	}

	public static void Save(T serializableObject, string path, Type[] extraTypes)
	{
		SaveToDocumentFormat(serializableObject, extraTypes, path, null);
	}

	public static void Save(T serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory)
	{
		SaveToDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
	}

	public static void Save(T serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory, SerializedFormat serializedFormat)
	{
		switch (serializedFormat)
		{
		case SerializedFormat.Binary:
			SaveToBinaryFormat(serializableObject, fileName, isolatedStorageDirectory);
			break;
		default:
			SaveToDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
			break;
		}
	}

	public static void Save(T serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory, Type[] extraTypes)
	{
		SaveToDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
	}

	private static FileStream CreateFileStream(IsolatedStorageFile isolatedStorageFolder, string path)
	{
		FileStream fileStream = null;
		if (isolatedStorageFolder == null)
		{
			return new FileStream(path, FileMode.OpenOrCreate);
		}
		return new IsolatedStorageFileStream(path, FileMode.OpenOrCreate, isolatedStorageFolder);
	}

	private static T LoadFromBinaryFormat(string path, IsolatedStorageFile isolatedStorageFolder)
	{
		T val = null;
		using FileStream serializationStream = CreateFileStream(isolatedStorageFolder, path);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		return binaryFormatter.Deserialize(serializationStream) as T;
	}

	private static T LoadFromBinaryFormat(Stream source)
	{
		T val = null;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		return binaryFormatter.Deserialize(source) as T;
	}

	private static T LoadFromDocumentFormat(Type[] extraTypes, string path, IsolatedStorageFile isolatedStorageFolder)
	{
		T val = null;
		using TextReader textReader = CreateTextReader(isolatedStorageFolder, path);
		XmlSerializer xmlSerializer = CreateXmlSerializer(extraTypes);
		return xmlSerializer.Deserialize(textReader) as T;
	}

	private static T LoadFromDocumentFormat(Type[] extraTypes, Stream source)
	{
		T val = null;
		using TextReader textReader = CreateTextReader(source);
		XmlSerializer xmlSerializer = CreateXmlSerializer(extraTypes);
		return xmlSerializer.Deserialize(textReader) as T;
	}

	private static TextReader CreateTextReader(Stream source)
	{
		return new StreamReader(source, Encoding.UTF8);
	}

	private static TextReader CreateTextReader(IsolatedStorageFile isolatedStorageFolder, string path)
	{
		TextReader textReader = null;
		if (isolatedStorageFolder == null)
		{
			return new StreamReader(path, Encoding.UTF8);
		}
		return new StreamReader(new IsolatedStorageFileStream(path, FileMode.Open, isolatedStorageFolder), Encoding.UTF8);
	}

	private static TextWriter CreateTextWriter(IsolatedStorageFile isolatedStorageFolder, string path)
	{
		TextWriter textWriter = null;
		if (isolatedStorageFolder == null)
		{
			return new StreamWriter(path, append: false, Encoding.UTF8);
		}
		return new StreamWriter(new IsolatedStorageFileStream(path, FileMode.OpenOrCreate, isolatedStorageFolder), Encoding.UTF8);
	}

	private static XmlTextWriter CreateXmlTextWriter(IsolatedStorageFile isolatedStorageFolder, string path)
	{
		return new XmlTextWriterFormattedNoDeclaration(CreateTextWriter(isolatedStorageFolder, path));
	}

	private static XmlSerializer CreateXmlSerializer(Type[] extraTypes)
	{
		Type typeFromHandle = typeof(T);
		XmlSerializer xmlSerializer = null;
		if (extraTypes != null)
		{
			return new XmlSerializer(typeFromHandle, extraTypes);
		}
		return new XmlSerializer(typeFromHandle);
	}

	private static void SaveToDocumentFormat(T serializableObject, Type[] extraTypes, string path, IsolatedStorageFile isolatedStorageFolder)
	{
		using XmlTextWriter xmlWriter = CreateXmlTextWriter(isolatedStorageFolder, path);
		XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
		xmlSerializerNamespaces.Add("", "");
		XmlSerializer xmlSerializer = CreateXmlSerializer(extraTypes);
		xmlSerializer.Serialize(xmlWriter, serializableObject, xmlSerializerNamespaces);
	}

	private static void SaveToBinaryFormat(T serializableObject, string path, IsolatedStorageFile isolatedStorageFolder)
	{
		using FileStream serializationStream = CreateFileStream(isolatedStorageFolder, path);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(serializationStream, serializableObject);
	}
}

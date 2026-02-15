using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

namespace ICSharpCode.Core;

public sealed class AddIn
{
	private Properties properties = new Properties();

	private List<Runtime> runtimes = new List<Runtime>();

	private List<string> bitmapResources = new List<string>();

	private List<string> stringResources = new List<string>();

	private string _addInFileName;

	private AddInManifest manifest = new AddInManifest();

	private Dictionary<string, ExtensionPath> paths = new Dictionary<string, ExtensionPath>();

	private AddInAction action = AddInAction.Disable;

	private bool enabled;

	private static bool hasShownErrorMessage;

	private string customErrorMessage;

	private bool isExternal;

	public string CustomErrorMessage
	{
		get
		{
			return customErrorMessage;
		}
		internal set
		{
			if (value != null)
			{
				Enabled = false;
				Action = AddInAction.CustomError;
			}
			customErrorMessage = value;
		}
	}

	public AddInAction Action
	{
		get
		{
			return action;
		}
		set
		{
			action = value;
		}
	}

	public List<Runtime> Runtimes => runtimes;

	public Version Version => manifest.PrimaryVersion;

	public string FileName
	{
		get
		{
			return _addInFileName;
		}
		private set
		{
			_addInFileName = value;
			isExternal = IsExternalAddIn(_addInFileName);
		}
	}

	public bool IsExternal => isExternal;

	public string Name => properties["name"];

	public AddInManifest Manifest => manifest;

	public Dictionary<string, ExtensionPath> Paths => paths;

	public Properties Properties => properties;

	public List<string> BitmapResources
	{
		get
		{
			return bitmapResources;
		}
		set
		{
			bitmapResources = value;
		}
	}

	public List<string> StringResources
	{
		get
		{
			return stringResources;
		}
		set
		{
			stringResources = value;
		}
	}

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		internal set
		{
			enabled = value;
			Action = ((!value) ? AddInAction.Disable : AddInAction.Enable);
		}
	}

	public object CreateObject(string className)
	{
		string text = string.Empty;
		Exception ex = null;
		try
		{
			foreach (Runtime runtime in runtimes)
			{
				object obj = runtime.CreateInstance(className);
				if (obj != null)
				{
					return obj;
				}
			}
		}
		catch (TargetInvocationException ex2)
		{
			if (ex2.InnerException is FileNotFoundException)
			{
				text = Environment.NewLine + "One file or its dependency can not be found." + Environment.NewLine + "Could not load file or assembly: " + ((FileNotFoundException)ex2.InnerException).FileName;
			}
			else if (ex2.InnerException is TypeLoadException)
			{
				text = Environment.NewLine + "Type not found: " + ((TypeLoadException)ex2.InnerException).TypeName;
			}
			else
			{
				ex = ex2;
				text = Environment.NewLine + "InnerException Message: " + ex2.InnerException.Message;
			}
		}
		catch (FileNotFoundException ex3)
		{
			text = Environment.NewLine + "One file or its dependency can not be found." + Environment.NewLine + "Could not load file or assembly: " + ex3.FileName;
		}
		catch (TypeLoadException ex4)
		{
			text = Environment.NewLine + "Type not found: " + ex4.TypeName;
		}
		if (hasShownErrorMessage)
		{
			if (ex != null)
			{
				LoggingService.Error("Cannot create object: " + className + " in addin: " + FileName + text, ex);
			}
			else
			{
				LoggingService.Error("Cannot create object: " + className + " in addin: " + FileName + text);
			}
		}
		else
		{
			hasShownErrorMessage = true;
			if (ex != null)
			{
				MessageService.ShowError(ex, "Cannot create object: " + className + Environment.NewLine + "Addin: " + FileName + text + Environment.NewLine + "The missing objects will not cause an error message the next time.");
			}
			else
			{
				MessageService.ShowError("Cannot create object: " + className + Environment.NewLine + "Addin: " + FileName + text + Environment.NewLine + "The missing objects will not cause an error message the next time.");
			}
		}
		return null;
	}

	public override string ToString()
	{
		return "[AddIn: " + Name + "]";
	}

	internal AddIn()
	{
	}

	private static void SetupAddIn(XmlReader reader, AddIn addIn, string hintPath)
	{
		while (reader.Read())
		{
			if (reader.NodeType != XmlNodeType.Element || !reader.IsStartElement())
			{
				continue;
			}
			switch (reader.LocalName)
			{
			case "StringResources":
			case "BitmapResources":
			{
				if (reader.AttributeCount != 1)
				{
					throw new AddInLoadException("BitmapResources requires ONE attribute.");
				}
				string item = StringParser.Parse(reader.GetAttribute("file"));
				if (reader.LocalName == "BitmapResources")
				{
					addIn.BitmapResources.Add(item);
				}
				else
				{
					addIn.StringResources.Add(item);
				}
				break;
			}
			case "Runtime":
				if (!reader.IsEmptyElement)
				{
					Runtime.ReadSection(reader, addIn, hintPath);
				}
				break;
			case "Include":
			{
				if (reader.AttributeCount != 1)
				{
					throw new AddInLoadException("Include requires ONE attribute.");
				}
				if (!reader.IsEmptyElement)
				{
					throw new AddInLoadException("Include nodes must be empty!");
				}
				if (hintPath == null)
				{
					throw new AddInLoadException("Cannot use include nodes when hintPath was not specified (e.g. when AddInManager reads a .addin file)!");
				}
				string text = Path.Combine(hintPath, reader.GetAttribute(0));
				XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
				xmlReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
				using (XmlReader reader2 = XmlReader.Create(text, xmlReaderSettings))
				{
					SetupAddIn(reader2, addIn, Path.GetDirectoryName(text));
				}
				break;
			}
			case "Path":
			{
				if (reader.AttributeCount != 1)
				{
					throw new AddInLoadException("Import node requires ONE attribute.");
				}
				string attribute = reader.GetAttribute(0);
				ExtensionPath extensionPath = addIn.GetExtensionPath(attribute);
				if (!reader.IsEmptyElement)
				{
					ExtensionPath.SetUp(extensionPath, reader, "Path");
				}
				break;
			}
			case "Manifest":
				addIn.Manifest.ReadManifestSection(reader, hintPath);
				break;
			default:
				throw new AddInLoadException("Unknown root path node:" + reader.LocalName);
			}
		}
	}

	public ExtensionPath GetExtensionPath(string pathName)
	{
		if (!paths.ContainsKey(pathName))
		{
			return paths[pathName] = new ExtensionPath(pathName, this);
		}
		return paths[pathName];
	}

	private static AddIn Load(TextReader textReader)
	{
		return Load(textReader, null);
	}

	private static AddIn Load(TextReader textReader, string hintPath)
	{
		AddIn addIn = new AddIn();
		using XmlTextReader reader = new XmlTextReader(textReader);
		LoadInternal(addIn, reader, hintPath);
		return addIn;
	}

	private static AddIn Load(string addinText, string hintPath)
	{
		AddIn addIn = new AddIn();
		byte[] bytes = Encoding.ASCII.GetBytes(addinText);
		using MemoryStream input = new MemoryStream(bytes);
		using XmlTextReader reader = new XmlTextReader(input);
		LoadInternal(addIn, reader, hintPath);
		return addIn;
	}

	private static void LoadInternal(AddIn addIn, XmlTextReader reader, string hintPath)
	{
		while (reader.Read())
		{
			if (reader.IsStartElement())
			{
				string localName;
				if ((localName = reader.LocalName) == null || !(localName == "AddIn"))
				{
					throw new AddInLoadException("Unknown add-in file.");
				}
				addIn.properties = Properties.ReadFromAttributes(reader);
				SetupAddIn(reader, addIn, hintPath);
			}
		}
	}

	public static AddIn Load(ZipFile file)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		ZipEntry val = null;
		foreach (ZipEntry item in file)
		{
			ZipEntry val2 = item;
			if (val2.Name.EndsWith(".addin"))
			{
				if (val != null)
				{
					throw new AddInLoadException("The package may only contain one .addin file.");
				}
				val = val2;
			}
		}
		if (val == null)
		{
			throw new AddInLoadException("The package must contain one .addin file.");
		}
		AddIn addIn = null;
		using Stream stream = file.GetInputStream(val);
		using StreamReader textReader = new StreamReader(stream);
		return Load((TextReader)textReader);
	}

	public static AddIn Load(string fileName)
	{
		AddIn addIn = null;
		try
		{
			using TextReader textReader = File.OpenText(fileName);
			addIn = Load(textReader, Path.GetDirectoryName(fileName));
			addIn.FileName = fileName;
			return addIn;
		}
		catch (Exception innerException)
		{
			throw new AddInLoadException("Can't load " + fileName, innerException);
		}
	}

	public static bool IsExternalAddIn(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return false;
		}
		if (PropertyService.ConfigDirectory != FileUtility.ApplicationRootPath)
		{
			if (!FileUtility.IsBaseDirectory(Path.Combine(FileUtility.ApplicationRootPath, "bin"), fileName))
			{
				return !FileUtility.IsBaseDirectory(PropertyService.ConfigDirectory, fileName);
			}
			return false;
		}
		return !FileUtility.IsBaseDirectory(Path.Combine(FileUtility.ApplicationRootPath, "bin"), fileName);
	}
}

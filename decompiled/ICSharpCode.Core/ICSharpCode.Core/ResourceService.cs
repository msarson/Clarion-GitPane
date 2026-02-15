using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public static class ResourceService
{
	private class ResourceAssembly
	{
		private Assembly assembly;

		private string baseResourceName;

		private bool isIcons;

		public ResourceAssembly(Assembly assembly, string baseResourceName, bool isIcons)
		{
			this.assembly = assembly;
			this.baseResourceName = baseResourceName;
			this.isIcons = isIcons;
		}

		private ResourceManager TrySatellite(string language)
		{
			string path = Path.GetFileNameWithoutExtension(assembly.Location) + ".resources.dll";
			path = Path.Combine(Path.Combine(Path.GetDirectoryName(assembly.Location), language), path);
			if (File.Exists(path))
			{
				LoggingService.Info("Loging resources " + baseResourceName + " loading from satellite " + language);
				return new ResourceManager(baseResourceName, Assembly.LoadFrom(path));
			}
			return null;
		}

		public void Load()
		{
			string text = "Loading resources " + baseResourceName + "." + currentLanguage + ": ";
			ResourceManager resourceManager = null;
			if (assembly.GetManifestResourceInfo(baseResourceName + "." + currentLanguage + ".resources") != null)
			{
				LoggingService.Info(text + " loading from main assembly");
				resourceManager = new ResourceManager(baseResourceName + "." + currentLanguage, assembly);
			}
			else if (currentLanguage.IndexOf('-') > 0 && assembly.GetManifestResourceInfo(baseResourceName + "." + currentLanguage.Split('-')[0] + ".resources") != null)
			{
				LoggingService.Info(text + " loading from main assembly (no country match)");
				resourceManager = new ResourceManager(baseResourceName + "." + currentLanguage.Split('-')[0], assembly);
			}
			else
			{
				resourceManager = TrySatellite(currentLanguage);
				if (resourceManager == null && currentLanguage.IndexOf('-') > 0)
				{
					resourceManager = TrySatellite(currentLanguage.Split('-')[0]);
				}
			}
			if (resourceManager == null)
			{
				LoggingService.Warn(text + "NOT FOUND");
			}
			else if (isIcons)
			{
				localIconsResMgrs.Add(resourceManager);
			}
			else
			{
				localStringsResMgrs.Add(resourceManager);
			}
		}
	}

	private const string uiLanguageProperty = "CoreProperties.UILanguage";

	private const string stringResources = "StringResources";

	private const string imageResources = "BitmapResources";

	private static string resourceDirectory;

	private static List<ResourceManager> strings = new List<ResourceManager>();

	private static List<ResourceManager> icons = new List<ResourceManager>();

	private static Hashtable localStrings = null;

	private static Hashtable localIcons = null;

	private static Dictionary<string, Icon> iconCache = new Dictionary<string, Icon>();

	private static Dictionary<string, Bitmap> bitmapCache = new Dictionary<string, Bitmap>();

	private static List<ResourceManager> localStringsResMgrs = new List<ResourceManager>();

	private static List<ResourceManager> localIconsResMgrs = new List<ResourceManager>();

	private static List<ResourceAssembly> resourceAssemblies = new List<ResourceAssembly>();

	private static string currentLanguage;

	private static Font defaultMonospacedFont;

	public static string Language
	{
		get
		{
			return PropertyService.Get("CoreProperties.UILanguage", Thread.CurrentThread.CurrentUICulture.Name);
		}
		set
		{
			PropertyService.Set("CoreProperties.UILanguage", value);
		}
	}

	public static Font DefaultMonospacedFont
	{
		get
		{
			if (defaultMonospacedFont == null)
			{
				defaultMonospacedFont = LoadDefaultMonospacedFont(FontStyle.Regular);
			}
			return defaultMonospacedFont;
		}
	}

	public static event EventHandler LanguageChanged;

	public static void InitializeService(string resourceDirectory)
	{
		if (ResourceService.resourceDirectory != null)
		{
			throw new InvalidOperationException("Service is already initialized.");
		}
		if (resourceDirectory == null)
		{
			throw new ArgumentNullException("resourceDirectory");
		}
		ResourceService.resourceDirectory = resourceDirectory;
		PropertyService.PropertyChanged += OnPropertyChange;
		LoadLanguageResources(Language);
	}

	public static void RegisterStrings(string baseResourceName, Assembly assembly)
	{
		RegisterNeutralStrings(new ResourceManager(baseResourceName, assembly));
		ResourceAssembly resourceAssembly = new ResourceAssembly(assembly, baseResourceName, isIcons: false);
		resourceAssemblies.Add(resourceAssembly);
		resourceAssembly.Load();
	}

	public static void RegisterNeutralStrings(ResourceManager stringManager)
	{
		strings.Add(stringManager);
	}

	public static void RegisterImages(string baseResourceName, Assembly assembly)
	{
		RegisterNeutralImages(new ResourceManager(baseResourceName, assembly));
		ResourceAssembly resourceAssembly = new ResourceAssembly(assembly, baseResourceName, isIcons: true);
		resourceAssemblies.Add(resourceAssembly);
		resourceAssembly.Load();
	}

	public static void RegisterNeutralImages(ResourceManager imageManager)
	{
		icons.Add(imageManager);
	}

	private static void OnPropertyChange(object sender, PropertyChangedEventArgs e)
	{
		if (e.Key == "CoreProperties.UILanguage" && e.NewValue != e.OldValue)
		{
			LoadLanguageResources((string)e.NewValue);
			if (ResourceService.LanguageChanged != null)
			{
				ResourceService.LanguageChanged(null, e);
			}
		}
	}

	private static void LoadLanguageResources(string language)
	{
		iconCache.Clear();
		bitmapCache.Clear();
		try
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
		}
		catch (Exception)
		{
			try
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(language.Split('-')[0]);
			}
			catch (Exception)
			{
			}
		}
		localStrings = Load("StringResources", language);
		if (localStrings == null && language.IndexOf('-') > 0)
		{
			localStrings = Load("StringResources", language.Split('-')[0]);
		}
		localIcons = Load("BitmapResources", language);
		if (localIcons == null && language.IndexOf('-') > 0)
		{
			localIcons = Load("BitmapResources", language.Split('-')[0]);
		}
		localStringsResMgrs.Clear();
		localIconsResMgrs.Clear();
		currentLanguage = language;
		foreach (ResourceAssembly resourceAssembly in resourceAssemblies)
		{
			resourceAssembly.Load();
		}
	}

	public static Font LoadDefaultMonospacedFont(FontStyle style)
	{
		if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6)
		{
			return LoadFont("Courier New", 10, style);
		}
		return LoadFont("Courier New", 10, style);
	}

	public static Font LoadFont(string fontName, int size)
	{
		return LoadFont(fontName, size, FontStyle.Regular);
	}

	public static Font LoadFont(string fontName, int size, FontStyle style)
	{
		try
		{
			return new Font(fontName, size, style);
		}
		catch (Exception message)
		{
			LoggingService.Warn(message);
			return SystemInformation.MenuFont;
		}
	}

	public static Font LoadFont(string fontName, int size, GraphicsUnit unit)
	{
		return LoadFont(fontName, size, FontStyle.Regular, unit);
	}

	public static Font LoadFont(string fontName, int size, FontStyle style, GraphicsUnit unit)
	{
		try
		{
			return new Font(fontName, size, style, unit);
		}
		catch (Exception message)
		{
			LoggingService.Warn(message);
			return SystemInformation.MenuFont;
		}
	}

	public static Font LoadFont(Font baseFont, FontStyle newStyle)
	{
		try
		{
			return new Font(baseFont, newStyle);
		}
		catch (Exception message)
		{
			LoggingService.Warn(message);
			return baseFont;
		}
	}

	private static Hashtable Load(string fileName)
	{
		if (File.Exists(fileName))
		{
			Hashtable hashtable = new Hashtable();
			ResourceReader resourceReader = new ResourceReader(fileName);
			foreach (DictionaryEntry item in resourceReader)
			{
				hashtable.Add(item.Key, item.Value);
			}
			resourceReader.Close();
			return hashtable;
		}
		return null;
	}

	private static Hashtable Load(string name, string language)
	{
		return Load(resourceDirectory + Path.DirectorySeparatorChar + name + "." + language + ".resources");
	}

	public static string GetString(string name)
	{
		if (localStrings != null && localStrings[name] != null)
		{
			return localStrings[name].ToString();
		}
		string text = null;
		foreach (ResourceManager localStringsResMgr in localStringsResMgrs)
		{
			try
			{
				text = localStringsResMgr.GetString(name);
			}
			catch (Exception)
			{
			}
			if (text != null)
			{
				break;
			}
		}
		if (text == null)
		{
			foreach (ResourceManager @string in strings)
			{
				try
				{
					text = @string.GetString(name);
				}
				catch (Exception)
				{
				}
				if (text != null)
				{
					break;
				}
			}
		}
		if (text == null)
		{
			throw new ResourceNotFoundException("string >" + name + "<");
		}
		return text;
	}

	public static string GetString(string name, params object[] args)
	{
		return string.Format(GetString(name), args);
	}

	private static object GetImageResource(string name)
	{
		object obj = null;
		if (localIcons != null && localIcons[name] != null)
		{
			obj = localIcons[name];
		}
		else
		{
			foreach (ResourceManager localIconsResMgr in localIconsResMgrs)
			{
				obj = localIconsResMgr.GetObject(name);
				if (obj != null)
				{
					break;
				}
			}
			if (obj == null)
			{
				foreach (ResourceManager icon in icons)
				{
					try
					{
						obj = icon.GetObject(name);
					}
					catch (Exception)
					{
					}
					if (obj != null)
					{
						break;
					}
				}
			}
		}
		return obj;
	}

	public static Icon GetIcon(string name)
	{
		lock (iconCache)
		{
			if (iconCache.TryGetValue(name, out var value))
			{
				return value;
			}
			object imageResource = GetImageResource(name);
			if (imageResource == null)
			{
				return null;
			}
			value = ((!(imageResource is Icon)) ? Icon.FromHandle(((Bitmap)imageResource).GetHicon()) : ((Icon)imageResource));
			iconCache[name] = value;
			return value;
		}
	}

	public static Bitmap GetBitmap(string name)
	{
		lock (bitmapCache)
		{
			if (bitmapCache.TryGetValue(name, out var value))
			{
				return value;
			}
			try
			{
				object imageResource = GetImageResource(name);
				if (imageResource is Icon)
				{
					Icon icon = (Icon)imageResource;
					value = icon.ToBitmap();
				}
				else
				{
					value = (Bitmap)imageResource;
				}
			}
			catch
			{
				value = null;
			}
			if (value == null)
			{
				throw new ResourceNotFoundException(name);
			}
			bitmapCache[name] = value;
			return value;
		}
	}
}

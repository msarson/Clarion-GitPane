using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;

namespace ICSharpCode.SharpDevelop.Project;

public class TypeLibrary
{
	private enum RegKind
	{
		Default,
		Register,
		None
	}

	private string name;

	private string description;

	private string path;

	private string guid;

	private string version;

	private string lcid;

	private bool isolated;

	public string Guid => guid;

	public bool Isolated => isolated;

	public string Lcid => lcid;

	public string Name
	{
		get
		{
			if (name == null)
			{
				name = GetTypeLibName();
			}
			return name;
		}
	}

	public string Description => description;

	public string Path => path;

	public string Version => version;

	public int VersionMajor
	{
		get
		{
			if (version == null)
			{
				return -1;
			}
			string[] array = version.Split('.');
			if (array.Length != 0)
			{
				return GetVersion(array[0]);
			}
			return -1;
		}
	}

	public int VersionMinor
	{
		get
		{
			if (version == null)
			{
				return -1;
			}
			string[] array = version.Split('.');
			if (array.Length >= 2)
			{
				return GetVersion(array[1]);
			}
			return -1;
		}
	}

	public string WrapperTool => "tlbimp";

	public static IEnumerable<TypeLibrary> Libraries
	{
		get
		{
			RegistryKey typeLibsKey = Registry.ClassesRoot.OpenSubKey("TypeLib");
			try
			{
				string[] subKeyNames = typeLibsKey.GetSubKeyNames();
				foreach (string typeLibKeyName in subKeyNames)
				{
					RegistryKey typeLibKey = typeLibsKey.OpenSubKey(typeLibKeyName);
					if (typeLibKey != null)
					{
						TypeLibrary lib = Create(typeLibKey);
						if (lib != null && lib.Description != null && lib.Path != null && lib.Description.Length > 0 && lib.Path.Length > 0)
						{
							yield return lib;
						}
					}
				}
			}
			finally
			{
			}
		}
	}

	private static TypeLibrary Create(RegistryKey typeLibKey)
	{
		string[] subKeyNames = typeLibKey.GetSubKeyNames();
		if (subKeyNames.Length > 0)
		{
			TypeLibrary typeLibrary = new TypeLibrary();
			typeLibrary.version = subKeyNames[subKeyNames.Length - 1];
			RegistryKey registryKey = typeLibKey.OpenSubKey(typeLibrary.version);
			typeLibrary.description = (string)registryKey.GetValue(null);
			typeLibrary.path = GetTypeLibPath(registryKey, ref typeLibrary.lcid);
			typeLibrary.guid = System.IO.Path.GetFileName(typeLibKey.Name);
			return typeLibrary;
		}
		return null;
	}

	private static string GetTypeLibPath(RegistryKey versionKey, ref string lcid)
	{
		string[] subKeyNames = versionKey.GetSubKeyNames();
		if (subKeyNames == null || subKeyNames.Length == 0)
		{
			return null;
		}
		for (int i = 0; i < subKeyNames.Length; i++)
		{
			if (int.TryParse(subKeyNames[i], out var _))
			{
				lcid = subKeyNames[i];
				RegistryKey registryKey = versionKey.OpenSubKey(subKeyNames[i]);
				registryKey.GetSubKeyNames();
				RegistryKey registryKey2 = registryKey.OpenSubKey("win32");
				if (registryKey2 != null && registryKey2.GetValue(null) != null)
				{
					return GetTypeLibPath(registryKey2.GetValue(null).ToString());
				}
				return null;
			}
		}
		return null;
	}

	private static int GetVersion(string s)
	{
		if (int.TryParse(s, out var result))
		{
			return result;
		}
		return -1;
	}

	private string GetTypeLibName()
	{
		string text = null;
		if (this.guid != null && lcid != null && int.TryParse(lcid, out var result))
		{
			Guid guid = new Guid(this.guid);
			text = GetTypeLibNameFromGuid(ref guid, (short)VersionMajor, (short)VersionMinor, result);
		}
		if (text == null)
		{
			text = GetTypeLibNameFromFile(path);
		}
		if (text != null)
		{
			return text;
		}
		return description;
	}

	private static string GetTypeLibPath(string fileName)
	{
		if (fileName != null)
		{
			int num = fileName.LastIndexOf('\\');
			if (num > 0 && num + 1 < fileName.Length && char.IsDigit(fileName[num + 1]))
			{
				return fileName.Substring(0, num);
			}
		}
		return fileName;
	}

	private static string GetTypeLibNameFromFile(string fileName)
	{
		if (fileName != null && fileName.Length > 0 && File.Exists(fileName) && LoadTypeLibEx(fileName, RegKind.None, out var pptlib) == 0)
		{
			try
			{
				return Marshal.GetTypeLibName(pptlib);
			}
			finally
			{
				Marshal.ReleaseComObject(pptlib);
			}
		}
		return null;
	}

	private static string GetTypeLibNameFromGuid(ref Guid guid, short versionMajor, short versionMinor, int lcid)
	{
		if (LoadRegTypeLib(ref guid, versionMajor, versionMinor, lcid, out var pptlib) == 0)
		{
			try
			{
				return Marshal.GetTypeLibName(pptlib);
			}
			finally
			{
				Marshal.ReleaseComObject(pptlib);
			}
		}
		return null;
	}

	[DllImport("oleaut32.dll")]
	private static extern int LoadTypeLibEx([MarshalAs(UnmanagedType.BStr)] string szFile, RegKind regkind, out ITypeLib pptlib);

	[DllImport("oleaut32.dll")]
	private static extern int LoadRegTypeLib(ref Guid rguid, short wVerMajor, short wVerMinor, int lcid, out ITypeLib pptlib);
}

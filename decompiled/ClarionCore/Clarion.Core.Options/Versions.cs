using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Clarion.Core.Redirection;
using Clarion.Core.Resources;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace Clarion.Core.Options;

public class Versions
{
	private class Compilers : Properties
	{
		private static string comps = "clw;cpp;mod;asm;lpe";

		public Compilers()
		{
			SetupCompiler("clw");
			SetupCompiler("cpp");
			SetupCompiler("mod");
			SetupCompiler("asm");
			SetupCompiler("lpe");
		}

		public Compilers(FileInfo[] compilerList, int endLen)
		{
			foreach (FileInfo fileInfo in compilerList)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
				if (fileNameWithoutExtension.Length < 6)
				{
					continue;
				}
				int j = 1;
				if (fileNameWithoutExtension.StartsWith("Cla", StringComparison.InvariantCultureIgnoreCase))
				{
					j = 3;
				}
				else
				{
					for (; char.IsNumber(fileNameWithoutExtension[j]); j++)
					{
					}
				}
				if (j == fileNameWithoutExtension.Length - endLen)
				{
					string text = fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - endLen, 3);
					if (comps.Contains(text.ToLower()))
					{
						Set(text.ToLower(), Path.GetFileName(fileInfo.FullName));
					}
				}
			}
		}

		public void SetupCompiler(string name)
		{
			Set(name, "Cla" + name + ".dll");
		}
	}

	public delegate void VersionChangingDelegate(string newVersion, bool forWindows);

	private static Dictionary<bool, ClarionVersion> currentVersion = new Dictionary<bool, ClarionVersion>();

	private static Properties versions = null;

	private static string locker = new string('c', 1);

	public static VersionChangingDelegate VersionChanging;

	public static VersionChangingDelegate VersionChanged;

	internal static string RedDir
	{
		get
		{
			string fullPath = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
			return Path.Combine(Path.GetPathRoot(fullPath), Path.GetDirectoryName(fullPath));
		}
	}

	private static string ActiveWinVersion
	{
		get
		{
			return PropertyService.Get("Clarion.Version");
		}
		set
		{
			if (IsCurrent(value, forWin: true))
			{
				PropertyService.Set<string>("Clarion.Version", null);
			}
			else
			{
				PropertyService.Set("Clarion.Version", value);
			}
		}
	}

	private static string ActiveNetVersion
	{
		get
		{
			return PropertyService.Get("Clarion.Net.Version");
		}
		set
		{
			if (IsCurrent(value, forWin: false))
			{
				PropertyService.Set<string>("Clarion.Net.Version", null);
			}
			else
			{
				PropertyService.Set("Clarion.Net.Version", value);
			}
		}
	}

	private static bool IsCurrent(string value, bool forWin)
	{
		if (!string.IsNullOrEmpty(value) && !(value == GetVersion(forWin).Name))
		{
			return value == "Current";
		}
		return true;
	}

	private static void SetupCompilers(FileInfo[] compilerList, ClarionVersion v, int endLen)
	{
		Compilers compilers = new Compilers(compilerList, endLen);
		v.Compilers = compilers;
	}

	private static void FindFiles(RedirectionFile red, string startDir, DirectoryInfo dirInfo, List<string> dirs, string pattern)
	{
		DirectoryInfo[] directories = dirInfo.GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			if (!directoryInfo.Name.ToUpper().Contains("EXAMP") && !directoryInfo.Name.ToUpper().Contains("TEST"))
			{
				FindFiles(red, Path.Combine(startDir, directoryInfo.Name), directoryInfo, dirs, pattern);
			}
		}
		FileInfo[] files = dirInfo.GetFiles(pattern);
		if (files.Length > 0 && !red.Exists(files[0].Name, startDir))
		{
			dirs.Add(startDir);
		}
	}

	private static string AddLine(bool root, string pattern, List<string> thirdPartyDir)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("*.");
		stringBuilder.Append(pattern);
		stringBuilder.Append(" = ");
		bool flag = false;
		if (root)
		{
			stringBuilder.Append("%ROOT%\\bin");
			flag = true;
		}
		foreach (string item in thirdPartyDir)
		{
			if (flag)
			{
				stringBuilder.Append(';');
			}
			stringBuilder.Append("%ROOT%\\");
			stringBuilder.Append(item);
			flag = true;
		}
		return stringBuilder.ToString();
	}

	private static void SetupVersion(ClarionVersion v, string binDir, string exeName, string root, bool forWin)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(binDir);
		FileVersionInfo fileVersionInfo = null;
		if (forWin)
		{
			string fileName = Path.Combine(binDir, exeName);
			try
			{
				fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);
			}
			catch (FileNotFoundException)
			{
				return;
			}
			v.Is62 = fileVersionInfo.FileMinorPart == 6 && fileVersionInfo.FileBuildPart >= 200;
			FileInfo[] files = directoryInfo.GetFiles("*.ovl");
			if (files.Length == 0)
			{
				MessageBox.Show(string.Format(IntenalResources.GetString("Clarion.Options.NoOvlFile"), binDir, v.Name), IntenalResources.GetString("Clarion.Options.Title"));
				throw new InvalidOperationException();
			}
			v.OvlFile = Path.GetFileName(files[0].FullName);
			string text = Path.ChangeExtension(exeName, "ini");
			FileInfo[] files2 = directoryInfo.GetFiles(text);
			if (files2.Length > 0)
			{
				v.IniFile = text;
			}
			if (fileVersionInfo.FileMajorPart > 6)
			{
				v.Libsrc = FileUtility.Combine(root, "libsrc", "win") + ";" + FileUtility.Combine(root, "Accessory", "libsrc", "win");
			}
			else
			{
				v.Libsrc = Path.Combine(root, "libsrc");
			}
			FileInfo[] files3 = directoryInfo.GetFiles("c*clwx.dll");
			if (files3.Length != 0)
			{
				SetupCompilers(directoryInfo.GetFiles("c*x.dll"), v, 4);
			}
			else
			{
				FileInfo[] files4 = directoryInfo.GetFiles("c*clwx.exe");
				files3 = directoryInfo.GetFiles("c*clw.dll");
				if (files4.Length != 0)
				{
					if (files2.Length > 0)
					{
						IniFileReader iniFileReader = new IniFileReader(Path.Combine(binDir, text));
						if (iniFileReader.Exists("Project System", "ddetimeout"))
						{
							v.DDETimeOut = int.Parse(iniFileReader.Entry("Project System", "ddetimeout"));
						}
					}
					files4 = directoryInfo.GetFiles("c*x.exe");
					SetupCompilers(files4, v, 4);
				}
				else
				{
					SetupCompilers(directoryInfo.GetFiles("c*.dll"), v, 3);
				}
			}
		}
		else
		{
			v.Libsrc = FileUtility.Combine(root, "libsrc", "DotNet");
		}
		FileInfo[] files5 = directoryInfo.GetFiles("*.red");
		if (files5.Length > 0)
		{
			RedirectionVersion redirectionVersion = new RedirectionVersion(Path.GetFileName(files5[0].FullName));
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			list.Add(new KeyValuePair<string, string>("root", root));
			list.Add(new KeyValuePair<string, string>("reddir", binDir));
			redirectionVersion.Macros = new ReadOnlyCollection<KeyValuePair<string, string>>(list);
			redirectionVersion.SupportsInclude = !forWin || fileVersionInfo.FileMajorPart > 6;
			v.RedirectionFile = redirectionVersion;
		}
		versions.Set(v.Name, v.Properties);
		RedirectionFile redirectionFile = RedirectionFile.GetRedirectionFile(RedirectionFile.CurrentDirectory, v.Name, forWin);
		FileInfo[] files6 = directoryInfo.GetFiles("c*run*.dll");
		if (files6.Length > 0)
		{
			string name = files6[0].Name;
			string text2 = ((!char.IsDigit(name[2])) ? name.Substring(1, 1) : name.Substring(1, 2));
			bool flag = !redirectionFile.Exists("c" + text2 + "help.hlp", root);
			bool flag2 = !redirectionFile.Exists(name, root);
			bool flag3 = !redirectionFile.Exists(directoryInfo.GetFiles("*.exe")[0].Name, root);
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			string text3 = "3rdParty";
			string text4 = Path.Combine(root, text3);
			DirectoryInfo directoryInfo2 = null;
			if (Directory.Exists(Path.Combine(text4, "bin")))
			{
				directoryInfo2 = new DirectoryInfo(text4);
			}
			else
			{
				text3 = "Accessory";
				text4 = Path.Combine(root, text3);
				if (Directory.Exists(Path.Combine(text4, "bin")))
				{
					directoryInfo2 = new DirectoryInfo(text4);
				}
			}
			if (directoryInfo2 != null)
			{
				FindFiles(redirectionFile, text3, directoryInfo2, list2, "*.hlp");
				FindFiles(redirectionFile, text3, directoryInfo2, list3, "*.dll");
				FindFiles(redirectionFile, text3, directoryInfo2, list4, "*.exe");
			}
			if (flag || flag2 || flag3 || list2.Count > 0 || list3.Count > 0 || list4.Count > 0)
			{
				try
				{
					string[] array = File.ReadAllLines(redirectionFile.FullName);
					using StreamWriter streamWriter = new StreamWriter(redirectionFile.FullName);
					string[] array2 = array;
					foreach (string value in array2)
					{
						streamWriter.WriteLine(value);
					}
					new StringBuilder();
					if (flag || list2.Count > 0)
					{
						streamWriter.WriteLine(AddLine(flag, "hlp", list2));
					}
					if (flag2 || list3.Count > 0)
					{
						streamWriter.WriteLine(AddLine(flag2, "dll", list3));
					}
					if (flag3 || list4.Count > 0)
					{
						streamWriter.WriteLine(AddLine(flag3, "exe", list4));
					}
				}
				catch (Exception)
				{
					MessageBox.Show(string.Format(IntenalResources.GetString("Redirection.CannotUpdate"), v.Name), IntenalResources.GetString("Redirection.CannotUpdate.Title"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}
		v.SetCwVersion();
	}

	private static void SetupVersion(string section, IniFileReader winIni)
	{
		string text = winIni.Entry(section, "exename");
		string text2 = winIni.Entry(section, "root");
		string text3 = Path.Combine(text2, "bin");
		ClarionVersion clarionVersion = new ClarionVersion(section, text3);
		clarionVersion.IsWindowsVersion = true;
		try
		{
			SetupVersion(clarionVersion, text3, text + ".exe", text2, forWin: true);
		}
		catch (InvalidOperationException)
		{
		}
	}

	private static void UpgradeVersion(ClarionVersion v, string newName)
	{
		v.Name = newName;
		v.SetCwVersion();
		RedirectionVersion redirectionFile = v.RedirectionFile;
		if (v.IsWindowsVersion)
		{
			redirectionFile.Properties.Set("Name", "Clarion110.red");
		}
		else
		{
			redirectionFile.Properties.Set("Name", "ClarionNet40.red");
		}
	}

	private static ClarionVersion CreateCurrentWinVersion()
	{
		string redDir = RedDir;
		ClarionVersion clarionVersion = new ClarionVersion(CurrentVersionName(forWin: true), redDir);
		clarionVersion.IsWindowsVersion = true;
		clarionVersion.Is62 = false;
		RedirectionVersion redirectionVersion = new RedirectionVersion("Clarion110.red");
		RootedMacros rootedMacros = (RootedMacros)(redirectionVersion.Macros = new RootedMacros());
		clarionVersion.RedirectionFile = redirectionVersion;
		clarionVersion.Libsrc = FileUtility.Combine(rootedMacros["root"], "libsrc", "win") + ";" + FileUtility.Combine(rootedMacros["root"], "Accessory", "libsrc", "win");
		Compilers compilers = new Compilers();
		clarionVersion.Compilers = compilers;
		clarionVersion.SetCwVersion();
		versions.Set(clarionVersion.Name, clarionVersion.Properties);
		return clarionVersion;
	}

	private static ClarionVersion CreateCurrentDotNetVersion()
	{
		string redDir = RedDir;
		ClarionVersion clarionVersion = new ClarionVersion(CurrentVersionName(forWin: false), redDir);
		clarionVersion.IsWindowsVersion = false;
		RedirectionVersion redirectionVersion = new RedirectionVersion("ClarionNet40.red");
		RootedMacros rootedMacros = (RootedMacros)(redirectionVersion.Macros = new RootedMacros());
		clarionVersion.RedirectionFile = redirectionVersion;
		clarionVersion.Libsrc = FileUtility.Combine(rootedMacros["root"], "libsrc", "DotNet");
		versions.Set(clarionVersion.Name, clarionVersion.Properties);
		return clarionVersion;
	}

	private static void SetupVersions()
	{
		int num = (int)Convert.ToDecimal("11.1", new CultureInfo("en-US", useUserOverride: false));
		Properties properties = new Properties();
		try
		{
			using XmlTextReader xmlTextReader = new XmlTextReader(FileUtility.Combine(PropertyService.ConfigDirectory, "..", num - 1 + ".0", "Clarionproperties.xml"));
			while (xmlTextReader.Read())
			{
				if (xmlTextReader.IsStartElement() && xmlTextReader.LocalName == "ClarionProperties")
				{
					properties.ReadProperties(xmlTextReader, "ClarionProperties");
					break;
				}
			}
		}
		catch (FileNotFoundException)
		{
		}
		catch (DirectoryNotFoundException)
		{
		}
		catch (XmlException)
		{
		}
		Properties properties2 = properties.Get("Clarion.Versions", new Properties());
		string[] elements = properties2.Elements;
		foreach (string text in elements)
		{
			ClarionVersion clarionVersion = new ClarionVersion(text, (Properties)properties2.Get(text));
			versions.Set(clarionVersion.Name, clarionVersion.Properties);
		}
		CreateCurrentWinVersion();
		CreateCurrentDotNetVersion();
		if (properties2.Count == 0)
		{
			try
			{
				IniFileReader winIni = IniFileReader.WinIni;
				Dictionary<string, string> sections = winIni.Sections;
				foreach (string key in sections.Keys)
				{
					if (winIni.Exists(key, "root") && winIni.Exists(key, "exename"))
					{
						SetupVersion(key, winIni);
					}
				}
			}
			catch (FileNotFoundException)
			{
			}
		}
		PropertyService.Set("Clarion.Versions", versions);
	}

	private static void Initialise()
	{
		if (versions != null)
		{
			return;
		}
		versions = PropertyService.Get("Clarion.Versions", new Properties());
		if (versions.Count == 0)
		{
			SetupVersions();
			return;
		}
		GetVersion(forWin: true);
		GetVersion(forWin: false);
		string redDir = RedDir;
		for (int i = 0; i < versions.Elements.Length; i++)
		{
			string version = versions.Elements[i];
			ClarionVersion version2 = GetVersion(version);
			if (redDir.Equals(version2.Path, StringComparison.InvariantCultureIgnoreCase) && version2.Name.StartsWith("Clarion") && !CurrentVersionName(version2.IsWindowsVersion).Equals(version2.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				ClarionVersion clarionVersion = new ClarionVersion(CurrentVersionName(version2.IsWindowsVersion), (Properties)versions.Get(version2.Name));
				versions.Set(clarionVersion.Name, clarionVersion.Properties);
				versions.Remove(version2.Name);
				i = 0;
			}
		}
	}

	public static string CurrentVersionName(bool forWin)
	{
		if (forWin)
		{
			return "Clarion 11.1.13855";
		}
		return "Clarion.NET 4.0.13855";
	}

	public static ClarionVersion GetVersion(string version)
	{
		Initialise();
		if (!versions.Contains(version))
		{
			throw new InvalidVersionException(version);
		}
		return new ClarionVersion(version, (Properties)versions.Get(version));
	}

	public static bool VersionExists(string version)
	{
		Initialise();
		if (!string.IsNullOrEmpty(version))
		{
			return versions.Contains(version);
		}
		return false;
	}

	public static ClarionVersion GetVersion(bool forWin)
	{
		Initialise();
		if (currentVersion.ContainsKey(forWin))
		{
			return currentVersion[forWin];
		}
		string redDir = RedDir;
		ClarionVersion clarionVersion = null;
		bool flag = false;
		string[] elements = versions.Elements;
		foreach (string version in elements)
		{
			clarionVersion = GetVersion(version);
			if (clarionVersion.IsWindowsVersion == forWin && clarionVersion.Name.StartsWith("Clarion") && redDir.Equals(clarionVersion.Path, StringComparison.InvariantCultureIgnoreCase))
			{
				UpgradeVersion(clarionVersion, CurrentVersionName(forWin));
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			clarionVersion = ((!forWin) ? CreateCurrentDotNetVersion() : CreateCurrentWinVersion());
		}
		currentVersion.Add(forWin, clarionVersion);
		return clarionVersion;
	}

	public static ClarionVersion GetVersion(string version, bool forWin)
	{
		if (string.IsNullOrEmpty(version))
		{
			return GetVersion(forWin);
		}
		return GetVersion(version);
	}

	public static string[] VersionList(bool forWin)
	{
		Initialise();
		List<string> list = new List<string>();
		new RootedMacros();
		string[] elements = versions.Elements;
		foreach (string text in elements)
		{
			ClarionVersion version = GetVersion(text);
			if (version.IsWindowsVersion == forWin)
			{
				list.Add(text);
			}
		}
		return list.ToArray();
	}

	public static ClarionVersion NewVersion(string exeFile, bool forWindows)
	{
		string text = Path.Combine(Path.GetPathRoot(exeFile), Path.GetDirectoryName(exeFile));
		ClarionVersion clarionVersion = new ClarionVersion(text);
		clarionVersion.IsWindowsVersion = forWindows;
		clarionVersion.Path = text;
		string fullName = Directory.GetParent(text).FullName;
		SetupVersion(clarionVersion, text, Path.GetFileName(exeFile), fullName, forWindows);
		return clarionVersion;
	}

	public static bool Remove(ClarionVersion v, bool forceToCurrent = false)
	{
		bool flag = false;
		bool flag2 = false;
		if (forceToCurrent)
		{
			flag2 = IsActiveVersion(v);
			flag = v.IsWindowsVersion;
		}
		bool result = versions.Remove(v.Name);
		if (forceToCurrent && flag2)
		{
			string text = CurrentVersionName(flag);
			SetVersion(text, flag);
			if (WorkbenchSingleton.Workbench != null)
			{
				WorkbenchSingleton.Workbench.SetClarionVersion(text, flag);
				WorkbenchSingleton.Workbench.SetProjectTitle(ProjectService.CurrentProject);
			}
		}
		return result;
	}

	internal static void RenameVersion(string oldName, string newName)
	{
		Properties properties = (Properties)versions.Get(oldName);
		if (properties != null)
		{
			versions.Remove(oldName);
		}
		versions.Set(newName, properties);
	}

	public static bool IsActiveVersion(ClarionVersion v)
	{
		if (v == null)
		{
			return false;
		}
		string activeVersion = GetActiveVersion(v.IsWindowsVersion);
		return activeVersion == v.Name;
	}

	private static void SetSolutionVersion(Properties p, string value, bool forWin)
	{
		string property = ((!forWin) ? "ActiveNetVersion" : "ActiveVersion");
		p.Set(property, string.IsNullOrEmpty(value) ? "Current" : value);
	}

	private static string ActiveVersion(bool forWin)
	{
		if (!forWin)
		{
			return ActiveNetVersion;
		}
		return ActiveWinVersion;
	}

	private static void SetVersion(string value, bool forWin)
	{
		if (forWin)
		{
			ActiveWinVersion = value;
		}
		else
		{
			ActiveNetVersion = value;
		}
		if (VersionChanged != null)
		{
			VersionChanged(value, forWin);
		}
	}

	private static void ChangeVersion(string newVer, bool forWin)
	{
		string text = (forWin ? ActiveWinVersion : ActiveNetVersion);
		if (!string.IsNullOrEmpty(newVer) && text != newVer && (!IsCurrent(text, forWin) || !IsCurrent(newVer, forWin)) && MessageBox.Show(string.Format(ResourceService.GetString("Clarion.Version.SolutionForcingChange"), newVer), ResourceService.GetString("Clarion.Version.Title"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
		{
			if (newVer == "Current")
			{
				newVer = null;
			}
			SetVersion(newVer, forWin);
			WorkbenchSingleton.Workbench.SetClarionVersion(newVer, forWin);
		}
	}

	public static bool SetActiveVersion(string value, bool forWin)
	{
		if (VersionChanging != null)
		{
			try
			{
				VersionChanging(value, forWin);
			}
			catch (VersionChangedNotAllowedException)
			{
				return false;
			}
		}
		Properties properties = ((ProjectService.OpenSolution == null) ? null : ProjectService.OpenSolution.Preferences.Properties);
		if (properties != null)
		{
			SetSolutionVersion(properties, value, forWin);
		}
		SetVersion(value, forWin);
		if (WorkbenchSingleton.Workbench != null)
		{
			WorkbenchSingleton.Workbench.SetClarionVersion(value, forWin);
		}
		return true;
	}

	public static void SetActiveVersionFromSolution()
	{
		lock (locker)
		{
			Properties properties = ((ProjectService.OpenSolution == null) ? null : ProjectService.OpenSolution.Preferences.Properties);
			if (properties == null)
			{
				return;
			}
			string property = "ActiveVersion";
			string text = properties.Get<string>(property, null);
			if (!string.IsNullOrEmpty(text))
			{
				ChangeVersion(text, forWin: true);
				properties.Set(property, ActiveWinVersion);
				return;
			}
			property = "ActiveNetVersion";
			text = properties.Get<string>(property, null);
			if (text == "Current")
			{
				ChangeVersion(null, forWin: false);
				properties.Set(property, ActiveNetVersion);
			}
			else if (!string.IsNullOrEmpty(text))
			{
				ChangeVersion(text, forWin: false);
				properties.Set(property, ActiveNetVersion);
			}
		}
	}

	public static ClarionVersion GetVersionActive(bool forWin)
	{
		string activeVersion = GetActiveVersion(forWin);
		if (string.IsNullOrEmpty(activeVersion))
		{
			return GetVersion(forWin);
		}
		return GetVersion(activeVersion);
	}

	public static string GetActiveVersion(bool forWin)
	{
		lock (locker)
		{
			Initialise();
			Properties properties = ((ProjectService.OpenSolution == null) ? null : ProjectService.OpenSolution.Preferences.Properties);
			if (properties != null)
			{
				string property = ((!forWin) ? "ActiveNetVersion" : "ActiveVersion");
				string text = properties.Get<string>(property, null);
				if (!string.IsNullOrEmpty(text))
				{
					ChangeVersion(text, forWin);
				}
				else
				{
					SetSolutionVersion(properties, ActiveVersion(forWin), forWin);
				}
			}
		}
		return ActiveVersion(forWin);
	}
}

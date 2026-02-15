using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Clarion.Core.Options;
using Clarion.Core.Resources;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common;

namespace Clarion.Core.Redirection;

[DebuggerDisplay("Name: {FullName}, Active Sections: {ActiveSection}")]
public class RedirectionFile
{
	internal class Macros
	{
		private Dictionary<string, string> macros;

		private Guid id;

		private static string[] reservedMacros = new string[4] { "bin", "redname", "WinUserApplicationData", "WinCommonApplicationData" };

		internal Macros()
		{
			id = Guid.NewGuid();
			GuidLinkedList<Macros>.Instance.AddObject(id, this);
		}

		internal Macros(ClarionVersion ver)
			: this()
		{
			RedirectionVersion redirectionFile = ver.RedirectionFile;
			SoftEventHandlerList<VersionChangeEventHandlers>.Instance.Object(id).Init(id, redirectionFile);
			macros = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			foreach (KeyValuePair<string, string> macro in redirectionFile.Macros)
			{
				if (!IsReservedMacro(macro.Key))
				{
					macros.Add(macro.Key, macro.Value);
				}
			}
			string path = ver.Path;
			if (string.IsNullOrEmpty(path))
			{
				macros.Add("bin", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			}
			else
			{
				macros.Add("bin", path);
			}
			macros.Add("redname", redirectionFile.Name);
			macros.Add("WinUserApplicationData", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
			macros.Add("WinCommonApplicationData", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
			string text = Path.Combine(ver.Properties.Get("path", ""), ver.Properties.Get("ini", ""));
			if (!File.Exists(text))
			{
				return;
			}
			IniFileReader iniFileReader = new IniFileReader(text);
			try
			{
				foreach (KeyValuePair<string, string> item in iniFileReader.Entries("Redirection Macros"))
				{
					if (macros.ContainsKey(item.Key))
					{
						if (!item.Value.Equals(macros[item.Key], StringComparison.OrdinalIgnoreCase))
						{
							MessageBox.Show(string.Format(IntenalResources.GetString("Redirection.Macro.MacroExists"), item.Key, item.Value, macros[item.Key]), "Redirection System");
						}
					}
					else
					{
						macros.Add(item.Key, item.Value);
					}
				}
			}
			catch (NoSectionException)
			{
			}
		}

		~Macros()
		{
			SoftEventHandlerList<VersionChangeEventHandlers>.Instance.RemoveObject(id);
		}

		private static bool IsReservedMacro(string macro)
		{
			string[] array = reservedMacros;
			foreach (string text in array)
			{
				if (text.Equals(macro, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		internal void MacrosChanged(MacrosChangedEvent newMacro)
		{
			foreach (KeyValuePair<string, string> item in newMacro.List)
			{
				if (macros.ContainsKey(item.Key))
				{
					macros[item.Key] = item.Value;
				}
				else
				{
					macros.Add(item.Key, item.Value);
				}
			}
		}

		internal string Expand(string inStr, string file, string line, bool inCopySection)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			do
			{
				int num2 = inStr.IndexOf('%', num);
				if (num2 == -1)
				{
					stringBuilder.Append(inStr.Substring(num));
					num = 0;
					continue;
				}
				stringBuilder.Append(inStr.Substring(num, num2 - num));
				int num3 = inStr.IndexOf('%', num2 + 1);
				if (num3 == -1)
				{
					throw new MacroException(IntenalResources.GetString("Redirection.Macro.NotTerminated"), inStr.Substring(num2 + 1), file, line);
				}
				string text = inStr.Substring(num2 + 1, num3 - 1 - num2);
				if (text.Equals("Configuration", StringComparison.OrdinalIgnoreCase) || (inCopySection && text.Equals("libpath", StringComparison.OrdinalIgnoreCase)))
				{
					stringBuilder.Append("%");
					stringBuilder.Append(text);
					stringBuilder.Append("%");
				}
				else
				{
					if (!macros.ContainsKey(text))
					{
						throw new MacroException(IntenalResources.GetString("Redirection.Macro.NotFound"), text, macros, file, line);
					}
					stringBuilder.Append(macros[text]);
				}
				num = num3 + 1;
			}
			while (num != 0);
			string text2 = stringBuilder.ToString();
			if (text2.Contains("%") && !text2.Equals(inStr))
			{
				return Expand(text2, file, line, inCopySection);
			}
			return stringBuilder.ToString();
		}

		internal void Cleanup()
		{
			GuidLinkedList<Macros>.Instance.RemoveObject(id);
		}
	}

	private class Section
	{
		internal class Item
		{
			private Regex mask;

			private string maskStr;

			private string[] dirs;

			private bool stoppingDir;

			private bool inCopySection;

			private RedirectionFile parentRedFile;

			private string inDir;

			private static string StripQuotes(string dir, string file, string line)
			{
				dir = dir.Trim();
				if (dir.Length == 0 || dir[0] != '"')
				{
					return dir;
				}
				if (dir[dir.Length - 1] != '"')
				{
					throw new ParserException(IntenalResources.GetString("Redirection.MissingQuote"), file, line);
				}
				return dir.Substring(1, dir.Length - 2);
			}

			public Item(string mask, string[] unparsedDirs, Macros macros, RedirectionFile parent, string file, string line, bool copySection)
			{
				parentRedFile = parent;
				inCopySection = copySection;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append('^');
				for (int i = 0; i < mask.Length; i++)
				{
					char c = mask[i];
					switch (c)
					{
					case '?':
						stringBuilder.Append('.');
						break;
					case '*':
						stringBuilder.Append(".*");
						break;
					default:
						stringBuilder.Append(Regex.Escape(c.ToString()));
						break;
					}
				}
				if (mask[mask.Length - 1] != '*')
				{
					stringBuilder.Append('$');
				}
				this.mask = new Regex(stringBuilder.ToString(), RegexOptions.IgnoreCase);
				maskStr = mask;
				List<string> list = new List<string>();
				for (int j = 0; j < unparsedDirs.Length; j++)
				{
					if (stoppingDir)
					{
						break;
					}
					string text = unparsedDirs[j];
					if (text.Length <= 0)
					{
						continue;
					}
					if (text[text.Length - 1] == '|')
					{
						stoppingDir = true;
						text = text.Substring(0, text.Length - 1);
					}
					string text2 = macros.Expand(StripQuotes(text, parent.FullName, line), file, line, inCopySection);
					if (!string.IsNullOrEmpty(text2))
					{
						if (text2[text2.Length - 1] == '|')
						{
							stoppingDir = true;
							text2 = text2.Substring(0, text2.Length - 1);
						}
						list.Add(text2);
					}
				}
				dirs = list.ToArray();
			}

			public bool Matches(string name)
			{
				if (dirs.Length <= 0)
				{
					return false;
				}
				return mask.IsMatch(name);
			}

			private static void AddEnd(string pattern, int i, StringBuilder sb)
			{
				while (i < pattern.Length && (pattern[i] == '*' || pattern[i] == '?'))
				{
					sb.Append(pattern[i++]);
				}
				if (i != pattern.Length)
				{
					throw new Exception($"Index i={i.ToString()} and pattern.Length={pattern.Length.ToString()}");
				}
			}

			private static void AddStar(string p1, string p2, int i1, int i2, StringBuilder sb)
			{
				if (i1 == p1.Length - 1)
				{
					sb.Append(p2.Substring(i2));
					return;
				}
				while (i2 < p2.Length && p1[i1 + 1] != p2[i2])
				{
					sb.Append(p2[i2++]);
				}
				if (i2 == p2.Length)
				{
					throw new Exception($"Index i2={i2.ToString()} and p2.Length={p2.Length.ToString()}");
				}
				CombinedPattern(p1, p2, i1 + 1, i2, sb);
			}

			private static void AddQuestion(string p1, string p2, int i1, int i2, StringBuilder sb)
			{
				sb.Append(p2[i2]);
				i1++;
				i2++;
				CombinedPattern(p1, p2, i1, i2, sb);
			}

			private static void CombinedPattern(string p1, string p2, int i1, int i2, StringBuilder sb)
			{
				while (i1 < p1.Length && i2 < p2.Length && string.Equals(p1[i1].ToString(), p2[i2].ToString(), StringComparison.OrdinalIgnoreCase))
				{
					sb.Append(p1[i1]);
					i1++;
					i2++;
				}
				if (i1 == p1.Length)
				{
					if (p1[i1 - 1] == '*')
					{
						sb.Append(p2.Substring(i2));
					}
					else
					{
						AddEnd(p2, i2, sb);
					}
					return;
				}
				if (i2 == p2.Length)
				{
					if (p2[i2 - 1] == '*')
					{
						sb.Append(p1.Substring(i1));
					}
					else
					{
						AddEnd(p1, i1, sb);
					}
					return;
				}
				if (p1[i1] == '*')
				{
					AddStar(p1, p2, i1, i2, sb);
					return;
				}
				if (p2[i2] == '*')
				{
					AddStar(p2, p1, i2, i1, sb);
					return;
				}
				if (p1[i1] == '?')
				{
					AddQuestion(p1, p2, i1, i2, sb);
					return;
				}
				if (p2[i2] == '?')
				{
					AddQuestion(p2, p1, i2, i1, sb);
					return;
				}
				throw new Exception($"CombinedPattern fail. p1={p1}, p2{p2}, i1={i1}, i2={i2}");
			}

			internal string CombinedPattern(string pattern)
			{
				StringBuilder stringBuilder = new StringBuilder();
				try
				{
					CombinedPattern(maskStr, pattern, 0, 0, stringBuilder);
				}
				catch (Exception)
				{
					return null;
				}
				return stringBuilder.ToString();
			}

			private bool MatchesDir(string d)
			{
				return d.Equals(inDir, StringComparison.OrdinalIgnoreCase);
			}

			internal bool AddPaths(string root, List<string> dirList)
			{
				string[] array = dirs;
				foreach (string dir in array)
				{
					inDir = RealPath(dir, root, parentRedFile, inCopySection);
					if (dirList.FindIndex(MatchesDir) == -1 && Directory.Exists(inDir))
					{
						dirList.Add(inDir);
					}
				}
				return stoppingDir;
			}

			private string FullPath(string root, string dir, string name)
			{
				dir = RealPath(dir, root, parentRedFile, inCopySection);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
				return Path.Combine(dir, name);
			}

			private static string ParentDir(string root, string child, RedirectionFile redFile, bool inCopySection)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(root);
				DirectoryInfo parent = directoryInfo.Parent;
				if (child.Length > 1 && child[0] == Path.DirectorySeparatorChar)
				{
					child = child.Substring(1);
				}
				if (child.Length == 0)
				{
					return parent.FullName;
				}
				return RealPath(child, parent.FullName, redFile, inCopySection);
			}

			internal static string RealPath(string dir, string root, RedirectionFile redFile, bool inCopySection)
			{
				int num = dir.IndexOf("%Configuration%", StringComparison.OrdinalIgnoreCase);
				if (num != -1)
				{
					string[] array = null;
					int i = 0;
					if (redFile.ActiveSection != null)
					{
						for (array = redFile.ActiveSection.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries); i < array.Length && array[i].Equals("copy", StringComparison.OrdinalIgnoreCase); i++)
						{
						}
					}
					string newValue = ((array != null && i < array.Length) ? array[i] : string.Empty);
					dir = dir.Replace(dir.Substring(num, 15), newValue);
				}
				if (inCopySection)
				{
					int num2 = dir.IndexOf("%libpath%", StringComparison.OrdinalIgnoreCase);
					if (num2 != -1)
					{
						dir = dir.Replace(dir.Substring(num2, 9), redFile.libPath);
					}
				}
				if (dir.Length > 1 && dir[0] == '"' && dir[dir.Length - 1] == '"')
				{
					dir = dir.Substring(1, dir.Length - 2);
				}
				if (dir.Length > 0 && dir[dir.Length - 1] == Path.DirectorySeparatorChar)
				{
					dir = dir.Substring(0, dir.Length - 1);
				}
				if ((dir.Length > 0 && dir[0] == Path.DirectorySeparatorChar) || dir.Contains(":"))
				{
					return dir;
				}
				if (dir.Length > 0 && dir[0] == '.')
				{
					if (dir.Length == 1)
					{
						return root;
					}
					if (dir[1] == '.')
					{
						return ParentDir(root, dir.Substring(2), redFile, inCopySection);
					}
					return Path.Combine(root, dir.Substring(2));
				}
				return Path.Combine(root, dir);
			}

			internal bool OpenNames(string name, string root, ref List<string> strings)
			{
				bool flag = name.Contains("?") || name.Contains("*");
				string text = null;
				string text2 = null;
				string[] array = dirs;
				foreach (string dir in array)
				{
					try
					{
						text = RealPath(dir, root, parentRedFile, inCopySection);
						if (!Directory.Exists(text))
						{
							continue;
						}
						if (flag)
						{
							string[] files = Directory.GetFiles(text, name);
							if (files.Length > 0)
							{
								strings.AddRange(files);
							}
						}
						else
						{
							text2 = Path.Combine(text, name);
							if (File.Exists(text2))
							{
								strings.Add(text2);
							}
						}
					}
					catch (ArgumentException)
					{
					}
				}
				return stoppingDir;
			}

			public string GetName(string name, string root, bool forCreate, ref bool stop)
			{
				stop = stoppingDir;
				if (forCreate)
				{
					return FullPath(root, dirs[0], name);
				}
				string[] array = dirs;
				foreach (string dir in array)
				{
					try
					{
						string text = Path.Combine(RealPath(dir, root, parentRedFile, inCopySection), name);
						if (File.Exists(text))
						{
							return text;
						}
					}
					catch (ArgumentException)
					{
					}
				}
				return null;
			}

			public bool Exists(string name, string root, ref bool stop, out string fullName)
			{
				stop = stoppingDir;
				string[] array = dirs;
				foreach (string dir in array)
				{
					try
					{
						string text = Path.Combine(RealPath(dir, root, parentRedFile, inCopySection), name);
						if (File.Exists(text))
						{
							fullName = text;
							return true;
						}
					}
					catch (ArgumentException)
					{
					}
				}
				fullName = null;
				return false;
			}

			public bool Trace(string fileName, string root, List<string> ret, ref bool stop)
			{
				ret.Add(ToString() + " matches");
				stop = stoppingDir;
				string[] array = dirs;
				foreach (string text in array)
				{
					try
					{
						string text2 = Path.Combine(RealPath(text, root, parentRedFile, inCopySection), fileName);
						if (File.Exists(text2))
						{
							ret.Add("FOUND: " + text2);
							return true;
						}
						ret.Add("Not found in " + text);
						if (stoppingDir)
						{
							ret.Add("Search stopped because of stop marker");
						}
					}
					catch (ArgumentException)
					{
					}
				}
				return false;
			}

			public override string ToString()
			{
				return "Pattern: " + maskStr;
			}
		}

		private string name;

		private string redFileName;

		private RedirectionFile parentRedFile;

		protected bool active;

		private Macros macros;

		private List<Item> items = new List<Item>();

		public bool IsActive => active;

		public string Name => name;

		public Section(string name, Macros macros, string fileName, RedirectionFile parent)
		{
			this.name = name;
			this.macros = macros;
			redFileName = fileName;
			parentRedFile = parent;
		}

		public virtual void Deactivate()
		{
			active = false;
		}

		public virtual void Deactivate(string name)
		{
			if (name.Equals(this.name, StringComparison.OrdinalIgnoreCase))
			{
				active = false;
			}
		}

		public virtual void Activate(string name)
		{
			if (name.Equals(this.name, StringComparison.OrdinalIgnoreCase))
			{
				active = true;
			}
		}

		public void Parse(string line, string file)
		{
			int num = line.IndexOf('=');
			if (num == -1)
			{
				throw new ParserException(IntenalResources.GetString("Redirection.BadLine"), file, line);
			}
			items.Add(new Item(line.Substring(0, num).Trim(), line.Substring(num + 1).Trim().Split(';'), macros, parentRedFile, file, line, "copy".Equals(name, StringComparison.OrdinalIgnoreCase)));
		}

		internal bool OpenNames(string name, string root, ref List<string> strings)
		{
			foreach (Item item in items)
			{
				if (item.Matches(name) && item.OpenNames(name, root, ref strings))
				{
					return true;
				}
			}
			return false;
		}

		internal bool EvaluatedPaths(Dictionary<string, List<string>> dirs, string pattern, string root)
		{
			bool flag = false;
			for (int i = 0; i < items.Count; i++)
			{
				if (flag)
				{
					break;
				}
				Item item = items[i];
				string text = item.CombinedPattern(pattern);
				if (text != null)
				{
					bool flag2 = !dirs.ContainsKey(text);
					List<string> list = ((!flag2) ? dirs[text] : new List<string>());
					flag = item.AddPaths(root, list);
					if (flag2 && list.Count > 0)
					{
						dirs.Add(text, list);
					}
				}
			}
			return flag;
		}

		public string GetName(string name, string root, bool forCreate, ref bool stop)
		{
			foreach (Item item in items)
			{
				if (item.Matches(name))
				{
					string text = item.GetName(name, root, forCreate, ref stop);
					if (text != null || stop)
					{
						return text;
					}
				}
			}
			return null;
		}

		public bool Exists(string name, string root, ref bool stop, out string fullName)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (stop)
				{
					break;
				}
				Item item = items[i];
				if (item.Matches(name) && item.Exists(name, root, ref stop, out fullName))
				{
					return true;
				}
			}
			fullName = null;
			return false;
		}

		public bool Trace(string fileName, string root, List<string> ret, ref bool stop)
		{
			ret.Add("Looking in " + ToString());
			for (int i = 0; i < items.Count; i++)
			{
				if (stop)
				{
					break;
				}
				Item item = items[i];
				if (item.Matches(fileName))
				{
					if (item.Trace(fileName, root, ret, ref stop))
					{
						return true;
					}
				}
				else
				{
					ret.Add(item.ToString() + " does not match");
				}
			}
			return false;
		}

		public override string ToString()
		{
			return "Section:  " + name + " in file " + redFileName;
		}
	}

	private class CommonSection : Section
	{
		public CommonSection(Macros macros, string fileName, RedirectionFile parent)
			: base("Common", macros, fileName, parent)
		{
			active = true;
		}

		public override void Activate(string name)
		{
		}

		public override void Deactivate(string name)
		{
		}

		public override void Deactivate()
		{
		}
	}

	private class FileListCache
	{
		private RedirectionFile parent;

		private Dictionary<string, string> cache;

		private Dictionary<string, string> Cache
		{
			get
			{
				if (parent.ConfigurationChanged || parent.FileChanged)
				{
					parent.UpdateActiveSection();
					cache = null;
				}
				if (cache == null)
				{
					cache = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
				}
				return cache;
			}
		}

		internal FileListCache(RedirectionFile parentRed)
		{
			parent = parentRed;
		}

		private static string MakeKeyName(string fileName, string root)
		{
			if (string.IsNullOrEmpty(root) || root == ".")
			{
				root = CurrentDirectory;
			}
			return root + "|" + fileName;
		}

		internal bool TryGetFile(string fileName, string root, out string fullPath)
		{
			return Cache.TryGetValue(MakeKeyName(fileName, root), out fullPath);
		}

		internal void Add(string fileName, string root, string fullPath)
		{
			Cache.Add(MakeKeyName(fileName, root), fullPath);
		}

		internal void Remove(string fileName, string root)
		{
			Cache.Remove(MakeKeyName(fileName, root));
		}
	}

	private static string globalActiveSectionValue;

	private Macros macros;

	private string fullName;

	private string activeSection;

	private string libPath;

	private List<Section> sections;

	private static CoreStartup core = null;

	private Guid id;

	private static Dictionary<string, RedirectionFile> cachedRedFiles;

	private static List<string> macroExceptionsDisplayed;

	private FileListCache fileCache;

	private static bool throwLoadError = true;

	private static string lastProjectDir;

	private static List<string> MacroExceptionList
	{
		get
		{
			if (macroExceptionsDisplayed == null)
			{
				macroExceptionsDisplayed = new List<string>();
			}
			return macroExceptionsDisplayed;
		}
	}

	private bool FileChanged => SoftEventHandlerList<FileWatcherInRedFile>.Instance.Object(id).Reload;

	private static Dictionary<string, RedirectionFile> RedCache
	{
		get
		{
			if (cachedRedFiles == null)
			{
				cachedRedFiles = new Dictionary<string, RedirectionFile>(StringComparer.InvariantCultureIgnoreCase);
			}
			return cachedRedFiles;
		}
	}

	public static string GlobalActiveSection
	{
		get
		{
			return globalActiveSectionValue;
		}
		set
		{
			globalActiveSectionValue = value;
		}
	}

	private bool ConfigurationChanged => SoftEventHandlerList<ConfigurationChangedInRedFile>.Instance.Object(id).ConfigurationChanged;

	public string ActiveSection
	{
		get
		{
			return activeSection;
		}
		set
		{
			lock (this)
			{
				activeSection = value;
				if (sections == null || value == null)
				{
					return;
				}
				string[] array = value.Split(';');
				foreach (Section section in sections)
				{
					section.Deactivate();
				}
				string[] array2 = array;
				foreach (string name in array2)
				{
					foreach (Section section2 in sections)
					{
						section2.Activate(name);
					}
				}
			}
		}
	}

	public string FullName => fullName;

	public static bool ThrowErrorOnLoadFailure
	{
		get
		{
			return throwLoadError;
		}
		set
		{
			throwLoadError = value;
		}
	}

	public static string CurrentDirectory
	{
		get
		{
			string result;
			if (ProjectService.CurrentProject != null)
			{
				result = ProjectService.CurrentProject.Directory;
			}
			else if (ProjectService.OpenSolution != null && !string.IsNullOrEmpty(ProjectService.OpenSolution.Directory))
			{
				result = ProjectService.OpenSolution.Directory;
			}
			else if (lastProjectDir != null)
			{
				result = lastProjectDir;
			}
			else
			{
				string version = Versions.GetActiveVersion(forWin: true);
				if (!Versions.VersionExists(version))
				{
					version = Versions.CurrentVersionName(forWin: true);
				}
				result = Versions.GetVersion(version).Path;
				if (WorkbenchSingleton.Workbench != null)
				{
					IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
					if (activeWorkbenchWindow != null)
					{
						IViewContent viewContent = null;
						if (activeWorkbenchWindow.ActiveViewContent is IViewContent)
						{
							viewContent = (IViewContent)activeWorkbenchWindow.ActiveViewContent;
						}
						else if (activeWorkbenchWindow.ViewContent != null)
						{
							viewContent = activeWorkbenchWindow.ViewContent;
						}
						if (viewContent != null && !string.IsNullOrEmpty(viewContent.FileName))
						{
							result = Path.GetDirectoryName(viewContent.FileName);
						}
					}
				}
			}
			lastProjectDir = result;
			return result;
		}
	}

	private void Load(string fName)
	{
		fName = fName.Trim();
		if (fName[0] == '"')
		{
			fName = fName.Substring(1, fName.Trim().Length - 2);
		}
		string[] array = File.ReadAllLines(fName);
		FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(Path.Combine(Path.GetPathRoot(fName), Path.GetDirectoryName(fName)), Path.GetFileName(fName));
		fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
		fileSystemWatcher.EnableRaisingEvents = true;
		SoftEventHandlerList<FileWatcherInRedFile>.Instance.Object(id).AddWatcher(fileSystemWatcher);
		Section section = null;
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			if (text.Trim().Length == 0)
			{
				continue;
			}
			int num = text.ToUpperInvariant().IndexOf("%THISDIR%");
			if (num != -1)
			{
				text = text.Replace(text.Substring(num, 9), Path.GetDirectoryName(fName));
			}
			switch (text[0])
			{
			case '[':
			{
				int num2 = text.IndexOf(']');
				if (num2 == -1)
				{
					throw new ParserException(IntenalResources.GetString("Redirection.BadSection"), fName, text);
				}
				if (text[num2 - 1] == '2' && text[num2 - 2] == '3')
				{
					num2 -= 2;
				}
				section = ((!text.Substring(1, num2 - 1).Equals("COMMON", StringComparison.OrdinalIgnoreCase)) ? new Section(text.Substring(1, num2 - 1), macros, fName, this) : new CommonSection(macros, fName, this));
				sections.Add(section);
				continue;
			}
			case '{':
			{
				int num3 = text.IndexOf("INCLUDE", StringComparison.OrdinalIgnoreCase);
				if (num3 == -1)
				{
					throw new ParserException(IntenalResources.GetString("Redirection.UnknownCommand"), fName, text);
				}
				int num4 = text.IndexOf('}');
				if (num4 == -1)
				{
					throw new ParserException(IntenalResources.GetString("Redirection.BadCommand"), fName, text);
				}
				for (num3 += "INCLUDE".Length; char.IsWhiteSpace(text[num3]); num3++)
				{
				}
				try
				{
					bool inCopySection = section != null && "copy".Equals(section.Name, StringComparison.OrdinalIgnoreCase);
					Load(Section.Item.RealPath(macros.Expand(text.Substring(num3, num4 - num3), fName, text, inCopySection), Path.GetDirectoryName(fName), this, inCopySection));
				}
				catch (FileNotFoundException)
				{
					throw new ParserException(IntenalResources.GetString("Redirection.FileNotFound"), fName, text);
				}
				continue;
			}
			}
			if (text.Trim().Length > 1 && !text.Substring(0, 2).Equals("--"))
			{
				if (section == null)
				{
					section = new CommonSection(macros, fName, this);
					sections.Add(section);
				}
				section.Parse(text.Trim(), fName);
			}
		}
	}

	private void Load()
	{
		Initialize(null);
		if (!FileChanged)
		{
			return;
		}
		SoftEventHandlerList<FileWatcherInRedFile>.Instance.RemoveObject(id);
		SoftEventHandlerList<FileWatcherInRedFile>.Instance.Object(id).Reload = false;
		sections = new List<Section>();
		try
		{
			Load(fullName);
		}
		catch (MacroException ex)
		{
			if (!MacroExceptionList.Contains(ex.Line))
			{
				MacroExceptionList.Add(ex.Line);
				using MacroExceptionDialog macroExceptionDialog = new MacroExceptionDialog(ex);
				macroExceptionDialog.ShowDialog();
			}
		}
		catch (ParserException ex2)
		{
			if (throwLoadError)
			{
				throw ex2;
			}
			MessageBox.Show(ex2.ToString(), "Redirection System");
		}
		ActiveSection = ActiveSection;
	}

	private void CreateRed(bool forWin)
	{
		ClarionVersion version = Versions.GetVersion(forWin);
		using StreamWriter streamWriter = new StreamWriter(Path.Combine(version.Path, version.RedirectionFile.Name));
		streamWriter.Write("-- Default Redirection for Clarion");
		if (forWin)
		{
			streamWriter.Write(" ");
			streamWriter.WriteLine("11.1");
		}
		else
		{
			streamWriter.Write(".NET ");
			streamWriter.WriteLine("4.0");
		}
		streamWriter.WriteLine("");
		streamWriter.WriteLine("[Copy]");
		streamWriter.WriteLine("-- Directories only used when copying dlls");
		streamWriter.WriteLine("*.dll = %BIN%;%BIN%\\AddIns\\BackendBindings\\ClarionBinding\\Common;%ROOT%\\Accessory\\bin;%libpath%\\bin\\%configuration%");
		streamWriter.WriteLine("");
		streamWriter.WriteLine("[Debug]");
		streamWriter.WriteLine("-- Directories only used when building with Debug configuration");
		streamWriter.WriteLine("");
		if (forWin)
		{
			streamWriter.WriteLine("*.obj = obj\\debug");
			streamWriter.WriteLine("*.res = obj\\debug");
			streamWriter.WriteLine("*.rsc = obj\\debug");
			streamWriter.WriteLine("*.lib = obj\\debug");
			streamWriter.WriteLine("*.FileList.xml = obj\\debug");
			streamWriter.WriteLine("*.map = map\\debug");
		}
		else
		{
			streamWriter.WriteLine("*.FileList.txt = obj\\debug");
			streamWriter.WriteLine("*.pdb = bin\\debug");
			streamWriter.WriteLine("*.lib = obj\\debug");
		}
		streamWriter.WriteLine("");
		streamWriter.WriteLine("[Release]");
		streamWriter.WriteLine("-- Directories only used when building with Release configuration");
		streamWriter.WriteLine("");
		if (forWin)
		{
			streamWriter.WriteLine("*.obj = obj\\release");
			streamWriter.WriteLine("*.res = obj\\release");
			streamWriter.WriteLine("*.rsc = obj\\release");
			streamWriter.WriteLine("*.lib = obj\\release");
			streamWriter.WriteLine("*.FileList.xml = obj\\release");
			streamWriter.WriteLine("*.map = map\\release");
		}
		else
		{
			streamWriter.WriteLine("*.FileList.txt = obj\\release");
			streamWriter.WriteLine("*.pdb = bin\\release");
			streamWriter.WriteLine("*.lib = obj\\release");
		}
		streamWriter.WriteLine("");
		streamWriter.WriteLine("[Common]");
		string text = (forWin ? "\\win" : "\\dotnet");
		streamWriter.WriteLine("*.chm = %BIN%;%ROOT%\\Accessory\\bin");
		streamWriter.WriteLine("*.tp? = %ROOT%\\template" + text);
		streamWriter.WriteLine("*.trf = %ROOT%\\template" + text);
		streamWriter.WriteLine("*.txs = %ROOT%\\template" + text);
		streamWriter.WriteLine("*.stt = %ROOT%\\template" + text);
		streamWriter.WriteLine("*.*   = .; %ROOT%\\libsrc" + text + "; %ROOT%\\images; %ROOT%\\template" + text);
		if (forWin)
		{
			streamWriter.WriteLine("*.lib = %ROOT%\\lib");
			streamWriter.WriteLine("*.obj = %ROOT%\\lib");
			streamWriter.WriteLine("*.res = %ROOT%\\lib");
		}
		else
		{
			streamWriter.WriteLine();
		}
		streamWriter.WriteLine("*.hlp = %BIN%;%ROOT%\\Accessory\\bin");
		streamWriter.WriteLine("*.dll = %BIN%;%ROOT%\\Accessory\\bin");
		streamWriter.WriteLine("*.exe = %BIN%;%ROOT%\\Accessory\\bin");
		streamWriter.WriteLine("*.tp? = %ROOT%\\Accessory\\template" + text);
		streamWriter.WriteLine("*.txs = %ROOT%\\Accessory\\template" + text);
		streamWriter.WriteLine("*.stt = %ROOT%\\Accessory\\template" + text);
		if (forWin)
		{
			streamWriter.WriteLine("*.lib = %ROOT%\\Accessory\\lib");
			streamWriter.WriteLine("*.obj = %ROOT%\\Accessory\\lib");
			streamWriter.WriteLine("*.res = %ROOT%\\Accessory\\lib");
		}
		streamWriter.WriteLine("*.dll = %ROOT%\\Accessory\\bin");
		streamWriter.WriteLine("*.*   = %ROOT%\\Accessory\\images; %ROOT%\\Accessory\\resources; %ROOT%\\Accessory\\libsrc" + text + "; %ROOT%\\Accessory\\template" + text);
	}

	private void BaseInit(string directory, string version, bool forWin, bool useDefault, bool forceRoot)
	{
		if (forceRoot)
		{
			Init(version, forWin);
		}
		ClarionVersion version2 = Versions.GetVersion(version, forWin);
		fullName = Path.Combine(directory, version2.RedirectionFile.Name);
		if (!File.Exists(fullName) && useDefault)
		{
			Init(version, forWin);
			return;
		}
		if (!File.Exists(fullName))
		{
			throw new FileNotFoundException(IntenalResources.GetString("Redirection.FileNotFound") + $". Clarion version is {version}, missing file is {fullName}", fullName);
		}
		if (id == Guid.Empty)
		{
			id = Guid.NewGuid();
		}
		if (macros == null)
		{
			macros = new Macros(version2);
		}
		if (fileCache == null)
		{
			fileCache = new FileListCache(this);
		}
	}

	private void BaseInit(string version, bool forWin)
	{
		ClarionVersion version2 = Versions.GetVersion(version, forWin);
		BaseInit(version2.Path, version, forWin, useDefault: false, forceRoot: false);
	}

	private void Init(string version, bool forWin)
	{
		try
		{
			BaseInit(version, forWin);
		}
		catch (FileNotFoundException innerException)
		{
			ClarionVersion version2 = Versions.GetVersion(forWin);
			if (!string.IsNullOrEmpty(version) && !(version == version2.Name) && !(version + " (Debug)" == version2.Name))
			{
				throw new InvalidVersionException(version, innerException);
			}
			CreateRed(forWin);
			BaseInit(version, forWin);
		}
		InitializeActiveSection();
	}

	private void Init(string directory, string version, bool forWin, bool useDefault)
	{
		Initialize(null);
		string text = Versions.GetActiveVersion(forWin);
		if (string.IsNullOrEmpty(text))
		{
			text = Versions.CurrentVersionName(forWin);
		}
		BaseInit(directory ?? Versions.GetVersion(text).Path, version, forWin, useDefault, forceRoot: true);
	}

	private RedirectionFile()
	{
		Init("", forWin: false);
	}

	private RedirectionFile(bool forWin)
	{
		Init("", forWin);
	}

	private RedirectionFile(bool forWin, string version)
	{
		Init(version, forWin);
	}

	private RedirectionFile(string directory)
	{
		Init(directory, "", forWin: false, useDefault: true);
	}

	private RedirectionFile(string directory, bool forWin)
	{
		Init(directory, "", forWin, useDefault: true);
	}

	private RedirectionFile(string directory, string version)
	{
		Init(directory, version, forWin: false, useDefault: true);
	}

	private RedirectionFile(string directory, string version, bool forWin)
	{
		Init(directory, version, forWin, useDefault: true);
	}

	public RedirectionFile(string directory, string version, bool forWin, bool useDefault)
	{
		Init(directory, version, forWin, useDefault);
	}

	public static RedirectionFile GetRedirectionFile(bool forWin, string version = "")
	{
		return GetRedirectionFile(".", version, forWin);
	}

	public static RedirectionFile GetActiveRedirectionFile()
	{
		bool forWin = !ClarionAddins.DotNetPresent;
		return GetRedirectionFile(CurrentDirectory, Versions.GetActiveVersion(forWin), forWin);
	}

	public static RedirectionFile GetActiveRedirectionFile(bool forWindows)
	{
		return GetRedirectionFile(CurrentDirectory, Versions.GetActiveVersion(forWindows), forWindows);
	}

	public static RedirectionFile GetRedirectionFile(string directory = ".", string version = "", bool forWin = false, bool useDefault = true)
	{
		if (version == null)
		{
			version = string.Empty;
		}
		if (directory == null)
		{
			directory = ".";
		}
		if (!Versions.VersionExists(version))
		{
			version = Versions.CurrentVersionName(forWin);
		}
		ClarionVersion version2 = Versions.GetVersion(version, forWin);
		string text = Path.Combine(directory, version2.RedirectionFile.Name);
		if (!RedCache.TryGetValue(text, out var value))
		{
			if (File.Exists(text))
			{
				value = new RedirectionFile(directory, version, forWin, useDefault: false);
				RedCache.Add(text, value);
			}
			else
			{
				text = Path.Combine(version2.Path, version2.RedirectionFile.Name);
				if (!useDefault || !RedCache.TryGetValue(text, out value))
				{
					value = new RedirectionFile(directory, version, forWin, useDefault);
					RedCache.Add(text, value);
				}
			}
		}
		else if (!File.Exists(text))
		{
			RedCache.Remove(text);
			return GetRedirectionFile(directory, version, forWin, useDefault);
		}
		return value;
	}

	~RedirectionFile()
	{
		SoftEventHandlerList<FileWatcherInRedFile>.Instance.RemoveObject(id);
		SoftEventHandlerList<ConfigurationChangedInRedFile>.Instance.RemoveObject(id);
		if (macros != null)
		{
			macros.Cleanup();
		}
	}

	public static RedirectionFile Create(string directory)
	{
		return Create(directory, "", forWin: false);
	}

	public static RedirectionFile Create(string directory, bool forWin)
	{
		return Create(directory, "", forWin);
	}

	public static RedirectionFile Create(string directory, string version)
	{
		return Create(directory, version, forWin: false);
	}

	private static RedirectionFile Create(string directory, string version, bool forWin)
	{
		ClarionVersion version2 = Versions.GetVersion(version, forWin);
		using (StreamWriter streamWriter = new StreamWriter(Path.Combine(directory, version2.RedirectionFile.Name)))
		{
			if (version2.RedirectionFile.SupportsInclude)
			{
				streamWriter.WriteLine("");
				streamWriter.WriteLine("-- Add paths that are effective for all configurations");
				streamWriter.WriteLine("-- eg *.exe = exe");
				streamWriter.WriteLine("-- The redirection system has an order of precedence where a line has priority over later lines");
				streamWriter.WriteLine("");
				streamWriter.WriteLine("[Debug]");
				streamWriter.WriteLine("-- Add paths that are only effective when Debug configuration is being built");
				streamWriter.WriteLine("-- eg *.exe = exe\\debug");
				streamWriter.WriteLine("");
				streamWriter.WriteLine("[Release]");
				streamWriter.WriteLine("-- Add paths that are only effective when Release configuration is being built");
				streamWriter.WriteLine("-- eg *.exe = exe\\release");
				streamWriter.WriteLine("");
				streamWriter.WriteLine("[Common]");
				streamWriter.WriteLine("-- Add paths that are effective for all configurations");
				streamWriter.WriteLine("-- eg *.exe = exe");
				streamWriter.WriteLine("");
				streamWriter.WriteLine("{include %REDDIR%\\%REDNAME% }");
			}
			else
			{
				streamWriter.WriteLine("");
				streamWriter.WriteLine("-- Add paths that are effective for all configurations");
				streamWriter.WriteLine("-- eg *.obj = obj");
				streamWriter.WriteLine("-- The redirection system has an order of precedence where a line has priority over later lines");
				streamWriter.WriteLine("");
				streamWriter.WriteLine("[Debug32]");
				streamWriter.WriteLine("-- Add paths that are only effective when Debug configuration is being built");
				streamWriter.WriteLine("*.obj = %ROOT%\\obj32\\debug");
				streamWriter.WriteLine("*.res = %ROOT%\\obj32\\debug");
				streamWriter.WriteLine("*.rsc = %ROOT%\\obj32\\debug");
				streamWriter.WriteLine("");
				streamWriter.WriteLine("[Release32]");
				streamWriter.WriteLine("-- Add paths that are only effective when Release configuration is being built");
				streamWriter.WriteLine("*.obj = %ROOT%\\obj32\\release");
				streamWriter.WriteLine("*.res = %ROOT%\\obj32\\release");
				streamWriter.WriteLine("*.rsc = %ROOT%\\obj32\\release");
				streamWriter.WriteLine("");
				streamWriter.WriteLine("[Common]");
				streamWriter.WriteLine("-- Add paths that are effective for all configurations");
				streamWriter.WriteLine("*.dll = .;%ROOT%\\bin");
				streamWriter.WriteLine("*.tp? = %ROOT%\\template\\");
				streamWriter.WriteLine("*.trf = %ROOT%\\template\\");
				streamWriter.WriteLine("*.txs = %ROOT%\\template\\");
				streamWriter.WriteLine("*.stt = %ROOT%\\template\\");
				streamWriter.WriteLine("*.*   = .; %ROOT%\\libsrc; %ROOT%\\images; %ROOT%\\template; %ROOT%\\examples; %ROOT%\\convsrc");
				streamWriter.WriteLine("*.lib = %ROOT%\\lib");
				streamWriter.WriteLine("*.obj = %ROOT%\\lib");
				streamWriter.WriteLine("*.res = %ROOT%\\lib");
			}
		}
		return new RedirectionFile(directory, version, forWin, useDefault: false);
	}

	public static void Initialize(string configDir)
	{
		if (core == null && !PropertyService.Initialized)
		{
			core = new CoreStartup("Clarion");
			if (configDir == null)
			{
				configDir = FileUtility.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoftVelocity", "Clarion", "11.0") + Path.DirectorySeparatorChar;
			}
			try
			{
				new DirectoryInfo(configDir);
			}
			catch (SecurityException)
			{
				configDir = ".";
			}
			core.ConfigDirectory = configDir;
			core.StartCoreServices();
		}
	}

	private bool ExpandedName(string fileName)
	{
		if (fileName.IndexOf('\\') == -1 && fileName.IndexOf('/') == -1)
		{
			return fileName.IndexOf(':') != -1;
		}
		return true;
	}

	private string GetName(string name, string root, bool forCreate)
	{
		lock (this)
		{
			if (ExpandedName(name))
			{
				if (!forCreate && !File.Exists(name))
				{
					throw new FileNotFoundException(IntenalResources.GetString("Redirection.FileDoesNotExist"), name);
				}
				return name;
			}
			Load();
			UpdateActiveSection();
			bool stop = false;
			for (int i = 0; i < sections.Count; i++)
			{
				if (stop)
				{
					break;
				}
				Section section = sections[i];
				if (section.IsActive)
				{
					string name2 = section.GetName(name, root, forCreate, ref stop);
					if (name2 != null)
					{
						return Path.GetFullPath(name2);
					}
				}
			}
			throw new FileNotFoundException(IntenalResources.GetString("Redirection.FileDoesNotExistInSection"), name);
		}
	}

	private void InitializeActiveSection()
	{
		SoftEventHandlerList<ConfigurationChangedInRedFile>.Instance.AddObject(id);
		string value = null;
		if (ProjectService.OpenSolution != null)
		{
			value = ProjectService.OpenSolution.Preferences.ActiveConfiguration;
		}
		if (string.IsNullOrEmpty(value))
		{
			value = globalActiveSectionValue;
		}
		ActiveSection = value;
		UpdateActiveSection();
	}

	private void UpdateActiveSection()
	{
		ConfigurationChangedInRedFile configurationChangedInRedFile = SoftEventHandlerList<ConfigurationChangedInRedFile>.Instance.Object(id);
		if (configurationChangedInRedFile.ConfigurationChanged)
		{
			if (ProjectService.OpenSolution != null)
			{
				ActiveSection = ProjectService.OpenSolution.Preferences.ActiveConfiguration;
			}
			configurationChangedInRedFile.ConfigurationChanged = false;
		}
	}

	public void ActivateSection(string section)
	{
		lock (this)
		{
			if (sections != null && !string.IsNullOrEmpty(section))
			{
				if (!string.IsNullOrEmpty(activeSection))
				{
					activeSection += ";";
				}
				activeSection += section;
				{
					foreach (Section section2 in sections)
					{
						section2.Activate(section);
					}
					return;
				}
			}
			if (sections == null)
			{
				activeSection = section;
			}
		}
	}

	public void DeactivateSection(string section)
	{
		lock (this)
		{
			if (sections == null || string.IsNullOrEmpty(section))
			{
				return;
			}
			string[] array = activeSection.Split(';');
			StringBuilder stringBuilder = new StringBuilder();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!text.Equals(section, StringComparison.OrdinalIgnoreCase))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(';');
					}
					stringBuilder.Append(text);
				}
			}
			activeSection = stringBuilder.ToString();
			foreach (Section section2 in sections)
			{
				section2.Deactivate(section);
			}
		}
	}

	public string CreateName(string fileName, string root)
	{
		return GetName(fileName, root, !Exists(fileName, root));
	}

	public string OpenName(string fileName, string root)
	{
		if (!fileCache.TryGetFile(fileName, root, out var fullPath))
		{
			fullPath = GetName(fileName, root, forCreate: false);
			fileCache.Add(fileName, root, fullPath);
		}
		return fullPath;
	}

	public List<string> OpenNames(string fileName, string root)
	{
		lock (this)
		{
			List<string> strings = new List<string>();
			Load();
			bool flag = false;
			for (int i = 0; i < sections.Count; i++)
			{
				if (flag)
				{
					break;
				}
				Section section = sections[i];
				if (section.IsActive)
				{
					flag = section.OpenNames(fileName, root, ref strings);
				}
			}
			if (strings.Count == 0)
			{
				throw new FileNotFoundException(IntenalResources.GetString("Redirection.PatternDoesNotExistInSection"), fileName);
			}
			return strings;
		}
	}

	public Dictionary<string, List<string>> EvaluatedPaths(string pattern, string root)
	{
		lock (this)
		{
			Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
			Load();
			bool flag = false;
			for (int i = 0; i < sections.Count; i++)
			{
				if (flag)
				{
					break;
				}
				Section section = sections[i];
				if (section.IsActive)
				{
					flag = section.EvaluatedPaths(dictionary, pattern, root);
				}
			}
			if (dictionary.Count == 0)
			{
				return null;
			}
			return dictionary;
		}
	}

	public bool Exists(string fileName, string root)
	{
		lock (this)
		{
			if (ExpandedName(fileName))
			{
				return File.Exists(fileName);
			}
			if (fileCache.TryGetFile(fileName, root, out var fullPath))
			{
				if (File.Exists(fullPath))
				{
					return true;
				}
				fileCache.Remove(fileName, root);
			}
			Load();
			bool stop = false;
			for (int i = 0; i < sections.Count; i++)
			{
				if (stop)
				{
					break;
				}
				Section section = sections[i];
				if (section.IsActive && section.Exists(fileName, root, ref stop, out var fullPath2))
				{
					fileCache.Add(fileName, root, fullPath2);
					return true;
				}
			}
			return false;
		}
	}

	public void SetLibPath(string libPath)
	{
		this.libPath = libPath;
	}

	public List<string> Trace(string fileName, string root)
	{
		List<string> list = new List<string>();
		lock (this)
		{
			list.Add("Looking for " + fileName);
			if (ExpandedName(fileName))
			{
				list.Add(fileName + " is pathed.  No redirection used");
			}
			else
			{
				Load();
				bool stop = false;
				for (int i = 0; i < sections.Count; i++)
				{
					if (stop)
					{
						break;
					}
					Section section = sections[i];
					if (section.IsActive)
					{
						if (section.Trace(fileName, root, list, ref stop))
						{
							return list;
						}
					}
					else
					{
						list.Add(section.ToString() + " is not active.  Section skipped.");
					}
				}
			}
		}
		return list;
	}
}

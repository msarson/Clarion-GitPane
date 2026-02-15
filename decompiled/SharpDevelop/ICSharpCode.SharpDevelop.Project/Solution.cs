using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project.Converter;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public class Solution : SolutionFolder, IDisposable, IMSBuildEngineProvider
{
	internal class ProjectConfigurationPlatformMatching
	{
		public readonly IProject Project;

		public string Configuration;

		public string Platform;

		public SolutionItem SolutionItem;

		public ProjectConfigurationPlatformMatching(IProject project, string configuration, string platform, SolutionItem solutionItem)
		{
			Project = project;
			Configuration = configuration;
			Platform = platform;
			SolutionItem = solutionItem;
		}

		public void SetSolutionConfigurationPlatform(ProjectSection section, string newConfiguration, string newPlatform)
		{
			if (SolutionItem == null)
			{
				return;
			}
			string s = SolutionItem.Name;
			SolutionItem.Name = Project.IdGuid + "." + newConfiguration + "|" + newPlatform + ".Build.0";
			string s2 = SolutionItem.Name;
			if (!StripBuild0(ref s) || !StripBuild0(ref s2))
			{
				return;
			}
			s += ".ActiveCfg";
			s2 += ".ActiveCfg";
			foreach (SolutionItem item in section.Items)
			{
				if (item.Name == s)
				{
					item.Name = s2;
				}
			}
		}

		public void SetProjectConfigurationPlatform(ProjectSection section, string newConfiguration, string newPlatform)
		{
			Configuration = newConfiguration;
			Platform = newPlatform;
			if (SolutionItem == null)
			{
				return;
			}
			SolutionItem.Location = newConfiguration + "|" + newPlatform;
			string s = SolutionItem.Name;
			if (!StripBuild0(ref s))
			{
				return;
			}
			s += ".ActiveCfg";
			foreach (SolutionItem item in section.Items)
			{
				if (item.Name == s)
				{
					item.Location = SolutionItem.Location;
				}
			}
		}

		internal static bool StripBuild0(ref string s)
		{
			if (s.EndsWith(".Build.0"))
			{
				s = s.Substring(0, s.Length - ".Build.0".Length);
				return true;
			}
			return false;
		}
	}

	private Dictionary<string, ISolutionFolder> guidDictionary = new Dictionary<string, ISolutionFolder>();

	private string fileName = string.Empty;

	private Engine buildEngine = MSBuildInternals.CreateEngine();

	private SolutionPreferences preferences;

	private static Regex versionPattern = new Regex("Microsoft Visual Studio Solution File, Format Version\\s+(?<Version>.*)", RegexOptions.Compiled);

	private static Regex projectLinePattern = new Regex("Project\\(\"(?<ProjectGuid>.*)\"\\)\\s+=\\s+\"(?<Title>.*)\",\\s*\"(?<Location>.*)\",\\s*\"(?<Guid>.*)\"", RegexOptions.Compiled);

	private static Regex globalSectionPattern = new Regex("\\s*GlobalSection\\((?<Name>.*)\\)\\s*=\\s*(?<Type>.*)", RegexOptions.Compiled);

	private static Solution solutionBeingLoaded;

	[Browsable(false)]
	public Engine BuildEngine => buildEngine;

	[Browsable(false)]
	public IEnumerable<IProject> Projects
	{
		get
		{
			Stack<ISolutionFolder> stack = new Stack<ISolutionFolder>();
			foreach (ISolutionFolder folder in base.Folders)
			{
				stack.Push(folder);
			}
			while (stack.Count > 0)
			{
				ISolutionFolder currentFolder = stack.Pop();
				if (currentFolder is IProject)
				{
					yield return (IProject)currentFolder;
				}
				if (!(currentFolder is ISolutionFolderContainer))
				{
					continue;
				}
				ISolutionFolderContainer solutionFolderContainer = (ISolutionFolderContainer)currentFolder;
				foreach (ISolutionFolder folder2 in solutionFolderContainer.Folders)
				{
					stack.Push(folder2);
				}
			}
		}
	}

	[Browsable(false)]
	public IEnumerable<ISolutionFolderContainer> SolutionFolderContainers
	{
		get
		{
			Stack<ISolutionFolder> stack = new Stack<ISolutionFolder>();
			foreach (ISolutionFolder folder in base.Folders)
			{
				stack.Push(folder);
			}
			while (stack.Count > 0)
			{
				ISolutionFolder currentFolder = stack.Pop();
				if (!(currentFolder is ISolutionFolderContainer))
				{
					continue;
				}
				ISolutionFolderContainer currentContainer = (ISolutionFolderContainer)currentFolder;
				yield return currentContainer;
				foreach (ISolutionFolder folder2 in currentContainer.Folders)
				{
					stack.Push(folder2);
				}
			}
		}
	}

	[Browsable(false)]
	public IEnumerable<ISolutionFolder> SolutionFolders
	{
		get
		{
			Stack<ISolutionFolder> stack = new Stack<ISolutionFolder>();
			foreach (ISolutionFolder folder in base.Folders)
			{
				stack.Push(folder);
			}
			while (stack.Count > 0)
			{
				ISolutionFolder currentFolder = stack.Pop();
				yield return currentFolder;
				if (!(currentFolder is ISolutionFolderContainer))
				{
					continue;
				}
				ISolutionFolderContainer solutionFolderContainer = (ISolutionFolderContainer)currentFolder;
				foreach (ISolutionFolder folder2 in solutionFolderContainer.Folders)
				{
					stack.Push(folder2);
				}
			}
		}
	}

	[Browsable(false)]
	public IEnumerable<ISolutionFolder> NoneProjectSolutionFolders
	{
		get
		{
			Stack<ISolutionFolder> stack = new Stack<ISolutionFolder>();
			foreach (ISolutionFolder folder in base.Folders)
			{
				if (!(folder is IProject))
				{
					stack.Push(folder);
				}
			}
			while (stack.Count > 0)
			{
				ISolutionFolder currentFolder = stack.Pop();
				yield return currentFolder;
				if (!(currentFolder is ISolutionFolderContainer))
				{
					continue;
				}
				ISolutionFolderContainer solutionFolderContainer = (ISolutionFolderContainer)currentFolder;
				foreach (ISolutionFolder folder2 in solutionFolderContainer.Folders)
				{
					if (!(folder2 is IProject))
					{
						stack.Push(folder2);
					}
				}
			}
		}
	}

	[Browsable(false)]
	public IProject StartupProject
	{
		get
		{
			if (!HasProjects)
			{
				return null;
			}
			IProject startupProject = preferences.StartupProject;
			if (startupProject != null)
			{
				return startupProject;
			}
			foreach (IProject project in Projects)
			{
				if (project.IsStartable)
				{
					return project;
				}
			}
			return null;
		}
	}

	[Browsable(false)]
	public bool HasProjects => Projects.GetEnumerator().MoveNext();

	[Browsable(false)]
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

	[Browsable(false)]
	public string Directory => Path.GetDirectoryName(fileName);

	[Browsable(false)]
	public bool IsDirty
	{
		get
		{
			foreach (IProject project in Projects)
			{
				if (project.IsDirty)
				{
					return true;
				}
			}
			return false;
		}
	}

	[Browsable(false)]
	public SolutionPreferences Preferences => preferences;

	[Browsable(false)]
	public override Solution ParentSolution => this;

	public override ProjectSection SolutionItems
	{
		get
		{
			foreach (ISolutionFolder folder in base.Folders)
			{
				if (folder.Name == "Solution Items" && folder is SolutionFolder)
				{
					return (folder as SolutionFolder).SolutionItems;
				}
			}
			SolutionFolder solutionFolder = new SolutionFolder("Solution Items", "Solution Items", "{2150E333-8FDC-42A3-9474-1A3956D46DE8}");
			AddFolder(solutionFolder);
			return solutionFolder.SolutionItems;
		}
	}

	public static Solution SolutionBeingLoaded => solutionBeingLoaded;

	public Solution()
	{
		preferences = new SolutionPreferences(this);
	}

	public IProject FindProject(string projectName)
	{
		projectName = Path.GetFileName(projectName);
		foreach (IProject project in Projects)
		{
			if (Path.GetFileName(project.FileName).Equals(projectName, StringComparison.CurrentCultureIgnoreCase))
			{
				return project;
			}
		}
		return null;
	}

	public IProject FindProjectContainingFile(string fileName)
	{
		IProject currentProject = ProjectService.CurrentProject;
		if (currentProject != null && currentProject.IsFileInProject(fileName))
		{
			return currentProject;
		}
		foreach (IProject project in Projects)
		{
			if (project.IsFileInProject(fileName))
			{
				return project;
			}
		}
		return null;
	}

	public SolutionFolder[] GetNoneProjectSolutionFolders()
	{
		List<SolutionFolder> list = new List<SolutionFolder>();
		foreach (ISolutionFolder noneProjectSolutionFolder in NoneProjectSolutionFolders)
		{
			if (noneProjectSolutionFolder is SolutionFolder item)
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public ISolutionFolder GetSolutionFolder(string guid)
	{
		foreach (ISolutionFolder solutionFolder in SolutionFolders)
		{
			if (solutionFolder.IdGuid == guid)
			{
				return solutionFolder;
			}
		}
		return null;
	}

	public SolutionFolder CreateFolder(string folderName)
	{
		return new SolutionFolder(folderName, folderName, "{" + Guid.NewGuid().ToString().ToUpperInvariant() + "}");
	}

	public override void AddFolder(ISolutionFolder folder)
	{
		base.AddFolder(folder);
		guidDictionary[folder.IdGuid] = folder;
	}

	public void Save()
	{
		Save(forceShowError: true);
	}

	public void Save(bool forceShowError)
	{
		try
		{
			Save(fileName);
		}
		catch (IOException ex)
		{
			string message = $"Could not save the solution {fileName}\n{ex.Message}";
			MessageService.ShowError(message);
		}
		catch (UnauthorizedAccessException)
		{
			string text = "The access to the Solution is restricted (probably it is Read Only) and can not be saved.";
			if (forceShowError)
			{
				MessageService.ShowError(text);
			}
			else if (!PropertyService.Get("SharpDevelop.SilentReadOnlyWarnings", defaultValue: false))
			{
				if (PropertyService.Get("SharpDevelop.ReadOnlyPrjWarning", defaultValue: true))
				{
					MessageService.ShowWarning(text);
				}
				TaskService.BuildMessageViewCategory.AppendLine(text);
				TaskService.Add(new Task(null, text, 0, 0, TaskType.Warning));
			}
		}
	}

	public void Save(string fileName)
	{
		this.fileName = fileName;
		string directoryName = Path.GetDirectoryName(fileName);
		if (!string.IsNullOrEmpty(directoryName) && !System.IO.Directory.Exists(directoryName))
		{
			System.IO.Directory.CreateDirectory(directoryName);
		}
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		List<ISolutionFolder> list = base.Folders;
		Stack<ISolutionFolder> stack = new Stack<ISolutionFolder>(list.Count);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			stack.Push(list[num]);
		}
		while (stack.Count > 0)
		{
			ISolutionFolder solutionFolder = stack.Pop();
			stringBuilder.Append("Project(\"");
			stringBuilder.Append(solutionFolder.TypeGuid);
			stringBuilder.Append("\")");
			stringBuilder.Append(" = ");
			stringBuilder.Append('"');
			stringBuilder.Append(solutionFolder.Name);
			stringBuilder.Append("\", ");
			if (solutionFolder is IProject)
			{
				solutionFolder.Location = ((IProject)solutionFolder).FileName;
			}
			string value = ((!Path.IsPathRooted(solutionFolder.Location)) ? solutionFolder.Location : FileUtility.GetRelativePath(Path.GetDirectoryName(FileName), solutionFolder.Location));
			stringBuilder.Append('"');
			stringBuilder.Append(value);
			stringBuilder.Append("\", ");
			stringBuilder.Append('"');
			stringBuilder.Append(solutionFolder.IdGuid);
			stringBuilder.Append("\"");
			stringBuilder.AppendLine();
			if (solutionFolder is IProject)
			{
				IProject project = (IProject)solutionFolder;
				SaveProjectSections(project.ProjectSections, stringBuilder);
			}
			else if (solutionFolder is SolutionFolder)
			{
				SolutionFolder solutionFolder2 = (SolutionFolder)solutionFolder;
				SaveProjectSections(solutionFolder2.Sections, stringBuilder);
				foreach (ISolutionFolder folder in solutionFolder2.Folders)
				{
					stack.Push(folder);
					stringBuilder2.Append("\t\t");
					stringBuilder2.Append(folder.IdGuid);
					stringBuilder2.Append(" = ");
					stringBuilder2.Append(solutionFolder2.IdGuid);
					stringBuilder2.Append(Environment.NewLine);
				}
			}
			else
			{
				LoggingService.Warn("Solution.Load(): unknown folder : " + solutionFolder);
			}
			stringBuilder.Append("EndProject");
			stringBuilder.Append(Environment.NewLine);
		}
		StringBuilder stringBuilder3 = new StringBuilder();
		stringBuilder3.Append("Global");
		stringBuilder3.Append(Environment.NewLine);
		foreach (ProjectSection section in base.Sections)
		{
			stringBuilder3.Append("\tGlobalSection(");
			stringBuilder3.Append(section.Name);
			stringBuilder3.Append(") = ");
			stringBuilder3.Append(section.SectionType);
			stringBuilder3.Append(Environment.NewLine);
			section.AppendSection(stringBuilder3, "\t\t");
			stringBuilder3.Append("\tEndGlobalSection");
			stringBuilder3.Append(Environment.NewLine);
		}
		string text = fileName + ".tmp";
		using (StreamWriter streamWriter = new StreamWriter(text, append: false, Encoding.UTF8))
		{
			streamWriter.WriteLine();
			streamWriter.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
			streamWriter.WriteLine("# Visual Studio 2012");
			streamWriter.WriteLine("# Clarion 2.1.0.2447");
			streamWriter.Write(stringBuilder.ToString());
			streamWriter.Write(stringBuilder3.ToString());
			if (stringBuilder2.Length > 0)
			{
				streamWriter.WriteLine("\tGlobalSection(NestedProjects) = preSolution");
				streamWriter.Write(stringBuilder2.ToString());
				streamWriter.WriteLine("\tEndGlobalSection");
			}
			streamWriter.WriteLine("EndGlobal");
		}
		if (!FileUtility.FilesAreEqual(fileName, text))
		{
			File.Copy(text, fileName, overwrite: true);
		}
		File.Delete(text);
	}

	private static void SaveProjectSections(IEnumerable<ProjectSection> sections, StringBuilder projectSection)
	{
		foreach (ProjectSection section in sections)
		{
			projectSection.Append("\tProjectSection(");
			projectSection.Append(section.Name);
			projectSection.Append(") = ");
			projectSection.Append(section.SectionType);
			projectSection.Append(Environment.NewLine);
			section.AppendSection(projectSection, "\t\t");
			projectSection.Append("\tEndProjectSection");
			projectSection.Append(Environment.NewLine);
		}
	}

	private static string GetFirstNonCommentLine(TextReader sr)
	{
		string text = "";
		while ((text = sr.ReadLine()) != null)
		{
			text = text.Trim();
			if (text.Length > 0 && text[0] != '#')
			{
				return text;
			}
		}
		return "";
	}

	public static string ReadSolutionInformation(string solutionFileName, PrjxToSolutionProject.Conversion conversion)
	{
		LoggingService.Debug("ReadSolutionInformation: " + solutionFileName);
		string directoryName = Path.GetDirectoryName(solutionFileName);
		using StreamReader streamReader = File.OpenText(solutionFileName);
		string firstNonCommentLine = GetFirstNonCommentLine(streamReader);
		Match match = versionPattern.Match(firstNonCommentLine);
		if (!match.Success)
		{
			return null;
		}
		string result = match.Result("${Version}");
		while ((firstNonCommentLine = streamReader.ReadLine()) != null)
		{
			match = projectLinePattern.Match(firstNonCommentLine);
			if (match.Success)
			{
				match.Result("${ProjectGuid}");
				string text = match.Result("${Title}");
				string value = Path.Combine(directoryName, match.Result("${Location}"));
				string text2 = FixGuid(match.Result("${Guid}"));
				LoggingService.Debug(text2 + ": " + text);
				conversion.NameToGuid[text] = new Guid(text2);
				conversion.NameToPath[text] = value;
				conversion.GuidToPath[new Guid(text2)] = value;
			}
		}
		return result;
	}

	private static string FixGuid(string inStr)
	{
		if (inStr.Length > 2 && inStr[1] == '{')
		{
			return inStr.Substring(1, inStr.Length - 2);
		}
		return inStr;
	}

	private static bool SetupSolution(Solution newSolution, string fileName)
	{
		string directoryName = Path.GetDirectoryName(fileName);
		ProjectSection projectSection = null;
		bool flag = false;
		using (StreamReader streamReader = new StreamReader(fileName, Encoding.Default, detectEncodingFromByteOrderMarks: true))
		{
			string firstNonCommentLine = GetFirstNonCommentLine(streamReader);
			Match match = versionPattern.Match(firstNonCommentLine);
			if (!match.Success)
			{
				MessageService.ShowErrorFormatted("${res:SharpDevelop.Solution.InvalidSolutionFile}", fileName);
				return false;
			}
			switch (match.Result("${Version}"))
			{
			case "7.00":
				flag = true;
				if (!MessageService.AskQuestion("${res:SharpDevelop.Solution.ConvertSolutionVersion7}"))
				{
					return false;
				}
				break;
			case "8.00":
				flag = true;
				if (!MessageService.AskQuestion("${res:SharpDevelop.Solution.ConvertSolutionVersion8}"))
				{
					return false;
				}
				break;
			default:
				MessageService.ShowErrorFormatted("${res:SharpDevelop.Solution.UnknownSolutionVersion}", match.Result("${Version}"));
				return false;
			case "9.00":
			case "10.00":
			case "11.00":
			case "12.00":
				break;
			}
			while (true)
			{
				firstNonCommentLine = streamReader.ReadLine();
				if (firstNonCommentLine == null)
				{
					break;
				}
				match = projectLinePattern.Match(firstNonCommentLine);
				if (match.Success)
				{
					string text = match.Result("${ProjectGuid}");
					string title = match.Result("${Title}");
					string text2 = match.Result("${Location}");
					string guid = FixGuid(match.Result("${Guid}"));
					if (!FileUtility.IsUrl(text2))
					{
						text2 = Path.GetFullPath(Path.Combine(directoryName, text2));
					}
					if (text == "{2150E333-8FDC-42A3-9474-1A3956D46DE8}")
					{
						SolutionFolder folder = SolutionFolder.ReadFolder(streamReader, title, text2, guid);
						newSolution.AddFolder(folder);
					}
					else
					{
						IProject project = LanguageBindingService.LoadProject(newSolution, text2, title, text);
						SolutionFolder.ReadProjectSections(streamReader, project.ProjectSections);
						project.IdGuid = guid;
						newSolution.AddFolder(project);
					}
					match = match.NextMatch();
					continue;
				}
				match = globalSectionPattern.Match(firstNonCommentLine);
				if (match.Success)
				{
					ProjectSection projectSection2 = ProjectSection.ReadGlobalSection(streamReader, match.Result("${Name}"), match.Result("${Type}"));
					if (projectSection2.Name == "NestedProjects")
					{
						projectSection = projectSection2;
					}
					else
					{
						newSolution.Sections.Add(projectSection2);
					}
				}
			}
		}
		if (projectSection != null)
		{
			foreach (SolutionItem item in projectSection.Items)
			{
				string key = item.Name;
				string key2 = item.Location;
				if (newSolution.guidDictionary.ContainsKey(key2) && newSolution.guidDictionary.ContainsKey(key))
				{
					ISolutionFolderContainer solutionFolderContainer = newSolution.guidDictionary[key2] as ISolutionFolderContainer;
					solutionFolderContainer.AddFolder(newSolution.guidDictionary[key]);
				}
			}
		}
		if (newSolution.FixSolutionConfiguration(newSolution.Projects) || flag)
		{
			newSolution.Save();
		}
		return true;
	}

	public ProjectSection GetSolutionConfigurationsSection()
	{
		foreach (ProjectSection section in base.Sections)
		{
			if (section.Name == "SolutionConfigurationPlatforms")
			{
				return section;
			}
		}
		ProjectSection projectSection = new ProjectSection("SolutionConfigurationPlatforms", "preSolution");
		base.Sections.Insert(0, projectSection);
		foreach (ProjectSection section2 in base.Sections)
		{
			if (!(section2.Name == "SolutionConfiguration"))
			{
				continue;
			}
			base.Sections.Remove(section2);
			foreach (SolutionItem item in section2.Items)
			{
				projectSection.Items.Add(new SolutionItem(item.Location + "|Any CPU", item.Location + "|Any CPU"));
			}
			break;
		}
		return projectSection;
	}

	public ProjectSection GetProjectConfigurationsSection()
	{
		foreach (ProjectSection section in base.Sections)
		{
			if (section.Name == "ProjectConfigurationPlatforms")
			{
				return section;
			}
		}
		ProjectSection projectSection = new ProjectSection("ProjectConfigurationPlatforms", "postSolution");
		base.Sections.Add(projectSection);
		foreach (ProjectSection section2 in base.Sections)
		{
			if (!(section2.Name == "ProjectConfiguration"))
			{
				continue;
			}
			base.Sections.Remove(section2);
			foreach (SolutionItem item2 in section2.Items)
			{
				string text = item2.Name;
				string text2 = item2.Location;
				if (!text.Contains("|"))
				{
					int num = text.LastIndexOf('.');
					if (num > 0)
					{
						string text3 = text.Substring(0, num);
						string text4 = text.Substring(num);
						if (text4 == ".0")
						{
							num = text3.LastIndexOf('.');
							if (num > 0)
							{
								text4 = text.Substring(num);
								text3 = text.Substring(0, num);
							}
						}
						text = text3 + "|Any CPU" + text4;
					}
					num = text2.LastIndexOf('|');
					if (num < 0)
					{
						text2 += "|Any CPU";
					}
					else
					{
						string item = text2.Substring(num + 1);
						bool flag = false;
						foreach (IProject project in Projects)
						{
							if (project.PlatformNames.Contains(item))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							text2 = text2.Substring(0, num) + "|Any CPU";
						}
					}
				}
				projectSection.Items.Add(new SolutionItem(text, text2));
			}
			break;
		}
		return projectSection;
	}

	public bool FixSolutionConfiguration(IEnumerable<IProject> projects)
	{
		ProjectSection solutionConfigurationsSection = GetSolutionConfigurationsSection();
		ProjectSection projectConfigurationsSection = GetProjectConfigurationsSection();
		bool result = false;
		if (solutionConfigurationsSection.Items.Count == 0)
		{
			solutionConfigurationsSection.Items.Add(new SolutionItem("Debug|Any CPU", "Debug|Any CPU"));
			solutionConfigurationsSection.Items.Add(new SolutionItem("Release|Any CPU", "Release|Any CPU"));
			LoggingService.Warn("!! Inserted default SolutionConfigurationPlatforms !!");
			result = true;
		}
		foreach (IProject project in projects)
		{
			string text = project.IdGuid.ToUpperInvariant();
			foreach (SolutionItem item in solutionConfigurationsSection.Items)
			{
				string searchKey = text + "." + item.Name + ".Build.0";
				if (!projectConfigurationsSection.Items.Exists((SolutionItem item) => item.Name == searchKey))
				{
					projectConfigurationsSection.Items.Add(new SolutionItem(searchKey, item.Location));
					result = true;
				}
				searchKey = text + "." + item.Name + ".ActiveCfg";
				if (!projectConfigurationsSection.Items.Exists((SolutionItem item) => item.Name == searchKey))
				{
					projectConfigurationsSection.Items.Add(new SolutionItem(searchKey, item.Location));
					result = true;
				}
			}
		}
		return result;
	}

	public IList<string> GetConfigurationNames()
	{
		List<string> list = new List<string>();
		foreach (SolutionItem item in GetSolutionConfigurationsSection().Items)
		{
			string configurationNameFromKey = AbstractProject.GetConfigurationNameFromKey(item.Name);
			if (!list.Contains(configurationNameFromKey))
			{
				list.Add(configurationNameFromKey);
			}
		}
		return list;
	}

	public IList<string> GetPlatformNames()
	{
		List<string> list = new List<string>();
		foreach (SolutionItem item in GetSolutionConfigurationsSection().Items)
		{
			string platformNameFromKey = AbstractProject.GetPlatformNameFromKey(item.Name);
			if (!list.Contains(platformNameFromKey))
			{
				list.Add(platformNameFromKey);
			}
		}
		return list;
	}

	public void ApplySolutionConfigurationAndPlatformToProjects()
	{
		foreach (ProjectConfigurationPlatformMatching activeConfigurationsAndPlatformsForProject in GetActiveConfigurationsAndPlatformsForProjects(preferences.ActiveConfiguration, preferences.ActivePlatform))
		{
			activeConfigurationsAndPlatformsForProject.Project.ActiveConfiguration = activeConfigurationsAndPlatformsForProject.Configuration;
			activeConfigurationsAndPlatformsForProject.Project.ActivePlatform = FixPlatformNameForProject(activeConfigurationsAndPlatformsForProject.Platform);
		}
	}

	private static string FixPlatformNameForProject(string platformName)
	{
		if (platformName == "Any CPU")
		{
			return "AnyCPU";
		}
		return platformName;
	}

	private static string FixPlatformNameForSolution(string platformName)
	{
		if (platformName == "AnyCPU")
		{
			return "Any CPU";
		}
		return platformName;
	}

	internal List<ProjectConfigurationPlatformMatching> GetActiveConfigurationsAndPlatformsForProjects(string solutionConfiguration, string solutionPlatform)
	{
		List<ProjectConfigurationPlatformMatching> list = new List<ProjectConfigurationPlatformMatching>();
		ProjectSection projectConfigurationsSection = GetProjectConfigurationsSection();
		Dictionary<string, SolutionItem> dictionary = new Dictionary<string, SolutionItem>(StringComparer.InvariantCultureIgnoreCase);
		foreach (SolutionItem item in projectConfigurationsSection.Items)
		{
			dictionary[item.Name] = item;
		}
		string text = "." + solutionConfiguration + "|" + solutionPlatform + ".Build.0";
		foreach (IProject project in Projects)
		{
			string key = project.IdGuid + text;
			if (dictionary.TryGetValue(key, out var value))
			{
				string text2 = value.Location;
				if (text2.IndexOf('|') > 0)
				{
					string configurationNameFromKey = AbstractProject.GetConfigurationNameFromKey(text2);
					string platformNameFromKey = AbstractProject.GetPlatformNameFromKey(text2);
					list.Add(new ProjectConfigurationPlatformMatching(project, configurationNameFromKey, platformNameFromKey, value));
				}
				else
				{
					list.Add(new ProjectConfigurationPlatformMatching(project, text2, solutionPlatform, value));
				}
			}
			else
			{
				list.Add(new ProjectConfigurationPlatformMatching(project, solutionConfiguration, solutionPlatform, null));
			}
		}
		return list;
	}

	internal SolutionItem CreateMatchingItem(string solutionConfiguration, string solutionPlatform, IProject project, string initialLocation)
	{
		SolutionItem solutionItem = new SolutionItem(project.IdGuid + "." + solutionConfiguration + "|" + solutionPlatform + ".Build.0", initialLocation);
		GetProjectConfigurationsSection().Items.Add(solutionItem);
		GetProjectConfigurationsSection().Items.Add(new SolutionItem(project.IdGuid + "." + solutionConfiguration + "|" + solutionPlatform + ".ActiveCfg", initialLocation));
		return solutionItem;
	}

	public void RenameSolutionConfiguration(string oldName, string newName)
	{
		foreach (string platformName in GetPlatformNames())
		{
			foreach (ProjectConfigurationPlatformMatching activeConfigurationsAndPlatformsForProject in GetActiveConfigurationsAndPlatformsForProjects(oldName, platformName))
			{
				activeConfigurationsAndPlatformsForProject.SetSolutionConfigurationPlatform(GetProjectConfigurationsSection(), newName, platformName);
			}
		}
		foreach (SolutionItem item in GetSolutionConfigurationsSection().Items)
		{
			if (AbstractProject.GetConfigurationNameFromKey(item.Name) == oldName)
			{
				item.Name = newName + "|" + AbstractProject.GetPlatformNameFromKey(item.Name);
				item.Location = item.Name;
			}
		}
	}

	public void RenameSolutionPlatform(string oldName, string newName)
	{
		foreach (string configurationName in GetConfigurationNames())
		{
			foreach (ProjectConfigurationPlatformMatching activeConfigurationsAndPlatformsForProject in GetActiveConfigurationsAndPlatformsForProjects(configurationName, oldName))
			{
				activeConfigurationsAndPlatformsForProject.SetSolutionConfigurationPlatform(GetProjectConfigurationsSection(), configurationName, newName);
			}
		}
		foreach (SolutionItem item in GetSolutionConfigurationsSection().Items)
		{
			if (AbstractProject.GetPlatformNameFromKey(item.Name) == oldName)
			{
				item.Name = AbstractProject.GetConfigurationNameFromKey(item.Name) + "|" + newName;
				item.Location = item.Name;
			}
		}
	}

	public bool RenameProjectConfiguration(IProject project, string oldName, string newName)
	{
		if (!(project is IProjectAllowChangeConfigurations projectAllowChangeConfigurations))
		{
			return false;
		}
		if (!projectAllowChangeConfigurations.RenameProjectConfiguration(oldName, newName))
		{
			return false;
		}
		foreach (SolutionItem item in GetProjectConfigurationsSection().Items)
		{
			if (item.Name.ToLowerInvariant().StartsWith(project.IdGuid.ToLowerInvariant()) && AbstractProject.GetConfigurationNameFromKey(item.Location) == oldName)
			{
				item.Location = newName + "|" + AbstractProject.GetPlatformNameFromKey(item.Location);
			}
		}
		return true;
	}

	public bool RenameProjectPlatform(IProject project, string oldName, string newName)
	{
		if (!(project is IProjectAllowChangeConfigurations projectAllowChangeConfigurations))
		{
			return false;
		}
		if (!projectAllowChangeConfigurations.RenameProjectPlatform(FixPlatformNameForProject(oldName), FixPlatformNameForProject(newName)))
		{
			return false;
		}
		foreach (SolutionItem item in GetProjectConfigurationsSection().Items)
		{
			if (item.Name.ToLowerInvariant().StartsWith(project.IdGuid.ToLowerInvariant()) && AbstractProject.GetPlatformNameFromKey(item.Location) == oldName)
			{
				item.Location = AbstractProject.GetConfigurationNameFromKey(item.Location) + "|" + newName;
			}
		}
		return true;
	}

	public void AddSolutionConfiguration(string newName, string copyFrom, bool createInProjects)
	{
		foreach (string platformName in GetPlatformNames())
		{
			AddSolutionConfigurationPlatform(newName, platformName, copyFrom, createInProjects, addPlatform: false);
		}
	}

	public void AddSolutionPlatform(string newName, string copyFrom, bool createInProjects)
	{
		foreach (string configurationName in GetConfigurationNames())
		{
			AddSolutionConfigurationPlatform(configurationName, newName, copyFrom, createInProjects, addPlatform: true);
		}
	}

	private void AddSolutionConfigurationPlatform(string newConfiguration, string newPlatform, string copyFrom, bool createInProjects, bool addPlatform)
	{
		List<ProjectConfigurationPlatformMatching> list = (string.IsNullOrEmpty(copyFrom) ? new List<ProjectConfigurationPlatformMatching>() : ((!addPlatform) ? GetActiveConfigurationsAndPlatformsForProjects(copyFrom, newPlatform) : GetActiveConfigurationsAndPlatformsForProjects(newConfiguration, copyFrom)));
		GetSolutionConfigurationsSection().Items.Add(new SolutionItem(newConfiguration + "|" + newPlatform, newConfiguration + "|" + newPlatform));
		foreach (IProject project in Projects)
		{
			Predicate<ProjectConfigurationPlatformMatching> match = (ProjectConfigurationPlatformMatching m) => m.Project == project;
			ProjectConfigurationPlatformMatching projectConfigurationPlatformMatching = list.Find(match);
			string text;
			string text2;
			if (projectConfigurationPlatformMatching != null)
			{
				text = projectConfigurationPlatformMatching.Configuration;
				text2 = projectConfigurationPlatformMatching.Platform;
			}
			else
			{
				text = Linq.ToArray(project.ConfigurationNames)[0];
				text2 = FixPlatformNameForSolution(Linq.ToArray(project.PlatformNames)[0]);
			}
			if (createInProjects)
			{
				ICollection<string> collection = (addPlatform ? project.PlatformNames : project.ConfigurationNames);
				if (collection.Contains(addPlatform ? newPlatform : newConfiguration))
				{
					if (addPlatform)
					{
						text2 = newPlatform;
					}
					else
					{
						text = newConfiguration;
					}
				}
				else if (project is IProjectAllowChangeConfigurations projectAllowChangeConfigurations)
				{
					if (addPlatform)
					{
						if (projectAllowChangeConfigurations.AddProjectPlatform(FixPlatformNameForProject(newPlatform), FixPlatformNameForProject(text2)))
						{
							text2 = newPlatform;
						}
					}
					else if (projectAllowChangeConfigurations.AddProjectConfiguration(newConfiguration, text))
					{
						text = newConfiguration;
					}
				}
			}
			CreateMatchingItem(newConfiguration, newPlatform, project, text + "|" + text2);
		}
	}

	private static string GetKeyFromProjectConfItem(string name)
	{
		int num = name.IndexOf('.');
		if (num < 0)
		{
			return null;
		}
		name = name.Substring(num + 1);
		if (!ProjectConfigurationPlatformMatching.StripBuild0(ref name))
		{
			num = name.LastIndexOf('.');
			if (num < 0)
			{
				return null;
			}
			name = name.Substring(0, num);
		}
		return name;
	}

	public void RemoveSolutionConfiguration(string name)
	{
		GetSolutionConfigurationsSection().Items.RemoveAll((SolutionItem item) => AbstractProject.GetConfigurationNameFromKey(item.Name) == name);
		GetProjectConfigurationsSection().Items.RemoveAll((SolutionItem item) => AbstractProject.GetConfigurationNameFromKey(GetKeyFromProjectConfItem(item.Name)) == name);
	}

	public void RemoveSolutionPlatform(string name)
	{
		GetSolutionConfigurationsSection().Items.RemoveAll((SolutionItem item) => AbstractProject.GetPlatformNameFromKey(item.Name) == name);
		GetProjectConfigurationsSection().Items.RemoveAll((SolutionItem item) => AbstractProject.GetPlatformNameFromKey(GetKeyFromProjectConfItem(item.Name)) == name);
	}

	public void RemoveProjectConfigurationPlatforms(IProject project)
	{
		RemoveProjectConfigurationPlatforms(project.IdGuid.ToLowerInvariant());
	}

	public void RemoveProjectConfigurationPlatforms(string guid)
	{
		GetProjectConfigurationsSection().Items.RemoveAll((SolutionItem item) => item.Name.ToLowerInvariant().StartsWith(guid.ToLowerInvariant()));
	}

	public bool RemoveProjectConfiguration(IProject project, string name)
	{
		if (!(project is IProjectAllowChangeConfigurations projectAllowChangeConfigurations))
		{
			return false;
		}
		if (!projectAllowChangeConfigurations.RemoveProjectConfiguration(name))
		{
			return false;
		}
		string text = "other";
		foreach (string configurationName in project.ConfigurationNames)
		{
			text = configurationName;
		}
		foreach (SolutionItem item in GetProjectConfigurationsSection().Items)
		{
			if (item.Name.ToLowerInvariant().StartsWith(project.IdGuid.ToLowerInvariant()) && AbstractProject.GetConfigurationNameFromKey(item.Location) == name)
			{
				item.Location = text + "|" + AbstractProject.GetPlatformNameFromKey(item.Location);
			}
		}
		return true;
	}

	public bool RemoveProjectPlatform(IProject project, string name)
	{
		if (!(project is IProjectAllowChangeConfigurations projectAllowChangeConfigurations))
		{
			return false;
		}
		if (!projectAllowChangeConfigurations.RemoveProjectPlatform(name))
		{
			return false;
		}
		string text = "other";
		foreach (string platformName in project.PlatformNames)
		{
			text = platformName;
		}
		foreach (SolutionItem item in GetProjectConfigurationsSection().Items)
		{
			if (item.Name.ToLowerInvariant().StartsWith(project.IdGuid.ToLowerInvariant()) && AbstractProject.GetPlatformNameFromKey(item.Location) == name)
			{
				item.Location = AbstractProject.GetConfigurationNameFromKey(item.Location) + "|" + text;
			}
		}
		return true;
	}

	public static Solution Load(string fileName)
	{
		Solution solution = (solutionBeingLoaded = new Solution());
		solution.Name = Path.GetFileNameWithoutExtension(fileName);
		string text = Path.GetExtension(fileName).ToUpperInvariant();
		if (text == ".CMBX")
		{
			if (!MessageService.AskQuestion("${res:SharpDevelop.Solution.ImportCmbx}"))
			{
				return null;
			}
			solution.fileName = Path.ChangeExtension(fileName, ".sln");
			CombineToSolution.ConvertSolution(solution, fileName);
			if (solution.FixSolutionConfiguration(solution.Projects))
			{
				solution.Save();
			}
		}
		else if (text == ".PRJX")
		{
			if (!MessageService.AskQuestion("${res:SharpDevelop.Solution.ImportPrjx}"))
			{
				return null;
			}
			solution.fileName = Path.ChangeExtension(fileName, ".sln");
			CombineToSolution.ConvertProject(solution, fileName);
			if (solution.FixSolutionConfiguration(solution.Projects))
			{
				solution.Save();
			}
		}
		else
		{
			solution.fileName = fileName;
			if (!SetupSolution(solution, fileName))
			{
				return null;
			}
		}
		solutionBeingLoaded = null;
		return solution;
	}

	public void Dispose()
	{
		foreach (IProject project in Projects)
		{
			project.Dispose();
		}
		if (buildEngine != null)
		{
			buildEngine.UnloadAllProjects();
			buildEngine = null;
		}
	}

	public void StartBuild(BuildOptions options)
	{
		MSBuildBasedProject.RunMSBuild(this, null, Preferences.ActiveConfiguration, Preferences.ActivePlatform, options);
	}

	internal bool IsFileInSolution(string openFileName)
	{
		Stack<ISolutionFolder> stack = new Stack<ISolutionFolder>();
		foreach (ISolutionFolder folder in base.Folders)
		{
			if (!(folder is IProject))
			{
				stack.Push(folder);
			}
		}
		while (stack.Count > 0)
		{
			ISolutionFolder solutionFolder = stack.Pop();
			if (!(solutionFolder is SolutionFolder))
			{
				continue;
			}
			SolutionFolder solutionFolder2 = solutionFolder as SolutionFolder;
			foreach (ISolutionFolder folder2 in solutionFolder2.Folders)
			{
				stack.Push(folder2);
			}
			foreach (ProjectSection section in solutionFolder2.Sections)
			{
				foreach (SolutionItem item in section.Items)
				{
					_ = item.Location;
					if (FileUtility.IsEqualFileName(item.Name, openFileName))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}

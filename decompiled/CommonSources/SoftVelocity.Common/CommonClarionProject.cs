using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Clarion.Core.Options;
using Clarion.Core.Redirection;
using Clarion.PRJ;
using CommonSources.Project;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Build.BuildEngine;
using SoftVelocity.Common.Parser.IDE;
using SoftVelocity.Generator;

namespace SoftVelocity.Common;

public abstract class CommonClarionProject : CompilableProject
{
	public enum ProjectParsingConditions
	{
		ParseAll,
		ParseHandCodedAndOpenedApp,
		DoNotParse
	}

	private Hashtable programEquates;

	private string programFileName = string.Empty;

	protected bool disposed;

	private bool _merging;

	private bool multiBuildImportAdded;

	private bool preBuildEventExists;

	private bool postBuildEventExists;

	protected abstract RedirectionFile RedFile { get; }

	[Browsable(false)]
	public abstract bool IsWin { get; }

	[Browsable(false)]
	public abstract ProjectParsingConditions ProjectParsingCondition { get; }

	[Browsable(false)]
	public abstract bool ProjectParsingEnabled { get; }

	[Browsable(false)]
	public abstract bool LightweightParsingModeEnabled { get; }

	[Browsable(false)]
	public abstract string ClassBrowserClassMenuPath { get; }

	[Browsable(false)]
	public abstract string ClassBrowserMemberMenuPath { get; }

	[Browsable(false)]
	public Hashtable ProgramEquates
	{
		get
		{
			return programEquates;
		}
		set
		{
			if (programEquates != null)
			{
				programEquates.Clear();
			}
			programEquates = value;
		}
	}

	[Browsable(false)]
	public string ProgramFileName
	{
		get
		{
			return programFileName;
		}
		set
		{
			programFileName = value;
		}
	}

	[Browsable(false)]
	public override string VersionName
	{
		get
		{
			string text = Versions.GetActiveVersion(IsWin);
			if (string.IsNullOrEmpty(text))
			{
				text = Versions.CurrentVersionName(IsWin);
			}
			return text.Replace("Clarion.NET", ResourceService.GetString("Clarion.Version.Text")).Replace("Clarion", ResourceService.GetString("Clarion.Version.Text"));
		}
	}

	[Browsable(false)]
	public virtual string Version => Versions.GetActiveVersion(IsWin);

	[Browsable(false)]
	public virtual List<Pragma> Pragmas => null;

	public bool Merging
	{
		get
		{
			return _merging;
		}
		private set
		{
			_merging = value;
		}
	}

	protected virtual string DebugProperty => "DebugType";

	public abstract bool IsValidFileExtension(string ext);

	public virtual void ModifyParserOptions(CompilerOptions options)
	{
	}

	public virtual string ModifyFileContent(string fileName, string fileContent)
	{
		return fileContent;
	}

	public static RedirectionFile CurrentRedirectionFile(IProject prj)
	{
		return CurrentRedirectionFile(prj, ClarionAddins.IsDefaultProjectWin);
	}

	public static RedirectionFile CurrentRedirectionFile(IProject prj, bool forWin)
	{
		if (prj == null)
		{
			prj = ProjectService.CurrentProject;
		}
		if (prj != null && prj is CommonClarionProject)
		{
			return ((CommonClarionProject)(object)prj).RedFile;
		}
		return RedirectionFile.GetActiveRedirectionFile(forWin);
	}

	public CommonClarionProject(IMSBuildEngineProvider engineProvider)
		: base(engineProvider)
	{
		FileService.FileRenamed += FileRenamed;
		FileService.FileRemoved += FileRemoved;
	}

	public override void Dispose()
	{
		disposed = true;
		FileService.FileRenamed -= FileRenamed;
		FileService.FileRemoved -= FileRemoved;
		ProgramEquates = null;
		programFileName = string.Empty;
		((MSBuildBasedProject)this).Dispose();
	}

	protected override ParseProjectContent CreateProjectContent()
	{
		return ClaParseProjectContent.CreateUninitalized((IProject)(object)this);
	}

	protected virtual void FileRemoved(object sender, FileEventArgs e)
	{
		string fullPath = Path.GetFullPath(e.FileName);
		if (e.IsDirectory)
		{
			if (programFileName != null && programFileName.StartsWith(fullPath, StringComparison.InvariantCultureIgnoreCase))
			{
				programFileName = string.Empty;
				ProgramEquates = null;
			}
		}
		else if (programFileName != null && programFileName.Equals(fullPath, StringComparison.InvariantCultureIgnoreCase))
		{
			programFileName = string.Empty;
			ProgramEquates = null;
		}
	}

	protected virtual void FileRenamed(object sender, FileRenameEventArgs e)
	{
		string fullPath = Path.GetFullPath(e.SourceFile);
		string fullPath2 = Path.GetFullPath(e.TargetFile);
		if (e.IsDirectory)
		{
			if (programFileName != null && programFileName.StartsWith(fullPath, StringComparison.InvariantCultureIgnoreCase))
			{
				programFileName = fullPath2 + programFileName.Substring(fullPath.Length);
			}
		}
		else if (programFileName != null && programFileName.Equals(fullPath, StringComparison.InvariantCultureIgnoreCase))
		{
			programFileName = fullPath2;
		}
	}

	public override bool IsFileInProject(string fileName)
	{
		if (((AbstractProject)this).IsFileInProject(fileName))
		{
			return true;
		}
		if ((object)AppGenEditorsService.GetProjectForFile(fileName) == this)
		{
			return true;
		}
		return false;
	}

	protected virtual void InitRemove()
	{
	}

	protected virtual void FinishRemove()
	{
	}

	protected virtual void RemovePragma(Pragma pragma)
	{
	}

	protected bool RemoveItem(string item)
	{
		foreach (ProjectItem item2 in ((AbstractProject)this).Items)
		{
			if (item2.Include == item)
			{
				ProjectService.RemoveProjectItem((IProject)(object)this, item2);
				return true;
			}
		}
		return false;
	}

	protected virtual void RemoveCompile(ProjectCompile file)
	{
		RemoveItem(file.FileName);
	}

	protected virtual void RemoveLinkedFile(string file)
	{
		if (!RemoveItem(file))
		{
			string[] array = file.Split('(', ')');
			if (array.Length > 1)
			{
				RemoveItem(array[1]);
			}
		}
	}

	protected virtual void RemoveInclude(string file)
	{
		RemoveItem(file);
	}

	protected virtual void RemoveReferencedProject(string file)
	{
		RemoveItem(file);
	}

	protected virtual void RemoveSet(string set)
	{
	}

	public virtual void Merge(PRJFile oldAppPrj, PRJFile newAppPrj, bool initial, string targetLanguage)
	{
		Merging = true;
		AutoLibLinkService.Init();
		bool flag = false;
		foreach (ProjectItem item in ((AbstractProject)this).Items)
		{
			if (item.GetEvaluatedMetadata<bool>("ProjectGenerated", false))
			{
				if (!flag)
				{
					flag = true;
					InitRemove();
				}
				ProjectService.RemoveProjectItem((IProject)(object)this, item);
			}
		}
		foreach (ProjectCompile item2 in (List<ProjectCompile>)(object)oldAppPrj.ProjectFiles)
		{
			if (!newAppPrj.ProjectFiles.Contains(item2.FileName))
			{
				if (!flag)
				{
					flag = true;
					InitRemove();
				}
				RemoveCompile(item2);
			}
		}
		foreach (GeneratedFile include in oldAppPrj.Includes)
		{
			if (include.IsGenerated && !newAppPrj.Includes.Contains(include))
			{
				if (!flag)
				{
					flag = true;
					InitRemove();
				}
				RemoveInclude(include.Name);
			}
		}
		foreach (GeneratedFile linkFile in oldAppPrj.LinkFiles)
		{
			if (linkFile.IsGenerated && !newAppPrj.LinkFiles.Contains(linkFile))
			{
				if (!flag)
				{
					flag = true;
					InitRemove();
				}
				RemoveLinkedFile(linkFile.Name);
			}
		}
		foreach (Pragma pragma in oldAppPrj.Pragmas)
		{
			if (!newAppPrj.Pragmas.Contains(pragma))
			{
				if (!flag)
				{
					flag = true;
					InitRemove();
				}
				RemovePragma(pragma);
			}
		}
		foreach (string reference in oldAppPrj.References)
		{
			if (!newAppPrj.References.Contains(reference))
			{
				if (!flag)
				{
					flag = true;
					InitRemove();
				}
				RemoveReferencedProject(reference);
			}
		}
		foreach (SetCommand item3 in (List<SetCommand>)(object)oldAppPrj.Sets)
		{
			if (!newAppPrj.Sets.Contains(item3.Name))
			{
				if (!flag)
				{
					flag = true;
					InitRemove();
				}
				RemoveSet(item3.Name);
			}
		}
		bool flag2 = false;
		List<ProjectCompile> list = new List<ProjectCompile>((IEnumerable<ProjectCompile>)newAppPrj.ProjectFiles);
		foreach (ProjectItem item4 in ((AbstractProject)this).Items)
		{
			if (string.IsNullOrEmpty(item4.GetMetadata("Generated")))
			{
				continue;
			}
			flag2 = false;
			foreach (ProjectCompile item5 in list)
			{
				if (string.Equals(item4.Include, item5.FileName, StringComparison.OrdinalIgnoreCase))
				{
					flag2 = true;
					list.Remove(item5);
					break;
				}
			}
			if (!flag2)
			{
				if (!flag)
				{
					flag = true;
					InitRemove();
				}
				ProjectService.RemoveProjectItem((IProject)(object)this, item4);
			}
		}
		list.Clear();
		list = null;
		if (flag)
		{
			FinishRemove();
		}
		Merge(newAppPrj, initial, targetLanguage);
		Merging = false;
	}

	protected virtual ProjectItem MakeProjectItem(string itemType, string itemValue)
	{
		return null;
	}

	protected virtual FileProjectItem NewFileProjectItem(ProjectCompile compFile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		return new FileProjectItem((IProject)(object)this, ItemType.Compile, compFile.FileName);
	}

	protected void DoAddProjectItem(ProjectItem newItem)
	{
		bool flag = false;
		foreach (ProjectItem item in ((AbstractProject)this).Items)
		{
			if (!string.Equals(item.FileName, newItem.FileName, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			bool flag2 = false;
			foreach (string metadataName in item.MetadataNames)
			{
				flag2 = false;
				foreach (string metadataName2 in newItem.MetadataNames)
				{
					if (string.Equals(metadataName, metadataName2, StringComparison.OrdinalIgnoreCase))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					break;
				}
			}
			if (flag2)
			{
				int num = 0;
				foreach (string metadataName3 in item.MetadataNames)
				{
					_ = metadataName3;
					num++;
				}
				foreach (string metadataName4 in newItem.MetadataNames)
				{
					_ = metadataName4;
					num--;
				}
				if (num != 0)
				{
					flag2 = false;
				}
			}
			if (!flag2)
			{
				ProjectService.RemoveProjectItem((IProject)(object)this, item);
			}
			else
			{
				flag = true;
			}
			break;
		}
		if (!flag)
		{
			ProjectService.AddProjectItem((IProject)(object)this, newItem);
		}
	}

	private Microsoft.Build.BuildEngine.Project SetupTask(string eventName)
	{
		Microsoft.Build.BuildEngine.Project project = ((MSBuildBasedProject)this).MSBuildProject.ParentEngine.CreateNewProject();
		Target target = project.Targets.AddNewTarget("Build");
		BuildTask buildTask = target.AddNewTask("Exec");
		buildTask.SetParameterValue("WorkingDirectory", "$(OutputPath)");
		buildTask.SetParameterValue("Command", "%(" + eventName + ".Command)");
		buildTask.SetParameterValue("IgnoreExitCode", "%(" + eventName + ".IgnoreExitCode)");
		return project;
	}

	private bool CheckBuildEvent(bool current, string propertyName)
	{
		if (!current && string.IsNullOrEmpty(((MSBuildBasedProject)this).MSBuildProject.GetEvaluatedProperty(propertyName)))
		{
			((MSBuildBasedProject)this).MSBuildProject.SetProperty(propertyName, "echo", null, PropertyPosition.UseExistingOrCreateAfterLastImport);
		}
		return true;
	}

	private void AddBuildEvent(string buildEvent, string value, string configuration, bool ignoreError, ref Microsoft.Build.BuildEngine.Project preBuild, ref Microsoft.Build.BuildEngine.Project postBuild)
	{
		if (!multiBuildImportAdded)
		{
			foreach (Import import in ((MSBuildBasedProject)this).MSBuildProject.Imports)
			{
				if (import.ProjectPath.Contains("SoftVelocity.Build.MultipleBuildEvents.targets"))
				{
					multiBuildImportAdded = true;
					break;
				}
			}
			if (!multiBuildImportAdded)
			{
				((MSBuildBasedProject)this).MSBuildProject.AddNewImport("$(ClarionBinPath)\\SoftVelocity.Build.MultipleBuildEvents.targets", null);
				multiBuildImportAdded = true;
			}
		}
		preBuildEventExists = CheckBuildEvent(preBuildEventExists, "PreBuildEvent");
		postBuildEventExists = CheckBuildEvent(postBuildEventExists, "PostBuildEvent");
		Microsoft.Build.BuildEngine.Project project = null;
		if (buildEvent == "PreBuild")
		{
			if (preBuild == null)
			{
				preBuild = SetupTask("MultiplePreBuildEvent");
			}
			project = preBuild;
		}
		else
		{
			if (postBuild == null)
			{
				postBuild = SetupTask("MultiplePostBuildEvent");
			}
			project = postBuild;
		}
		BuildItemGroup buildItemGroup = null;
		IEnumerator enumerator2 = project.ItemGroups.GetEnumerator();
		try
		{
			if (enumerator2.MoveNext())
			{
				BuildItemGroup buildItemGroup2 = (BuildItemGroup)enumerator2.Current;
				if (buildItemGroup2.Condition.Equals(configuration, StringComparison.InvariantCultureIgnoreCase))
				{
					buildItemGroup = buildItemGroup2;
				}
			}
		}
		finally
		{
			IDisposable disposable = enumerator2 as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		if (buildItemGroup == null)
		{
			buildItemGroup = project.AddNewItemGroup();
			buildItemGroup.Condition = configuration;
		}
		BuildItem buildItem = buildItemGroup.AddNewItem("Multiple" + buildEvent + "Event", ".");
		if (value.Contains("\""))
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				stringBuilder.Append(value[i]);
				if (i < value.Length - 1 && value[i] == '"' && value[i + 1] == '"')
				{
					i++;
				}
			}
			value = stringBuilder.ToString();
		}
		buildItem.SetMetadata("Command", value);
		buildItem.SetMetadata("Generated", bool.TrueString);
		if (ignoreError)
		{
			buildItem.SetMetadata("IgnoreExitCode", bool.TrueString);
		}
	}

	private Microsoft.Build.BuildEngine.Project CleanupBuildFile(string fileName, string itemName)
	{
		Microsoft.Build.BuildEngine.Project project = null;
		if (File.Exists(fileName))
		{
			project = ((MSBuildBasedProject)this).MSBuildProject.ParentEngine.CreateNewProject();
			project.Load(fileName);
			foreach (BuildItemGroup itemGroup in project.ItemGroups)
			{
				while (true)
				{
					IL_003f:
					foreach (BuildItem item in itemGroup)
					{
						if (item.Name == itemName && item.HasMetadata("Generated"))
						{
							itemGroup.RemoveItem(item);
							goto IL_003f;
						}
					}
					break;
				}
			}
		}
		return project;
	}

	public virtual void Merge(PRJFile appPrj, bool initial, string targetLanguage)
	{
		IClarionProjectContent clarionProjectContent = ParserService.GetProjectContent((IProject)(object)this) as IClarionProjectContent;
		if (clarionProjectContent != null)
		{
			clarionProjectContent.IsParseNewItems = false;
		}
		bool flag = false;
		foreach (ProjectCompile item in (List<ProjectCompile>)(object)appPrj.ProjectFiles)
		{
			flag = false;
			foreach (ProjectItem item2 in ((AbstractProject)this).Items)
			{
				if (string.Equals(item2.Include, item.FileName, StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
					if (string.IsNullOrEmpty(item2.GetMetadata("Generated")))
					{
						item2.SetMetadata("Generated", "true");
					}
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			FileProjectItem val = NewFileProjectItem(item);
			if (val != null)
			{
				if (item.Generated)
				{
					((ProjectItem)val).SetMetadata("Generated", "true");
				}
				DoAddProjectItem((ProjectItem)(object)val);
			}
		}
		Microsoft.Build.BuildEngine.Project preBuild = CleanupBuildFile(((AbstractProject)this).FileName + ".MultiplePreBuilds.Targets", "MultiplePreBuildEvent");
		Microsoft.Build.BuildEngine.Project postBuild = CleanupBuildFile(((AbstractProject)this).FileName + ".MultiplePostBuilds.Targets", "MultiplePostBuildEvent");
		string text = null;
		string text2 = null;
		string text3 = null;
		foreach (GeneratedFile linkFile in appPrj.LinkFiles)
		{
			string name = linkFile.Name;
			if (!name.Contains("("))
			{
				continue;
			}
			text = string.Empty;
			text2 = string.Empty;
			text3 = string.Empty;
			int num = name.IndexOf('(');
			int num2 = name.LastIndexOf(')');
			if (num2 == -1)
			{
				continue;
			}
			text = name.Substring(0, num).Trim();
			text2 = name.Substring(num + 1, num2 - num - 1).Trim();
			if (num2 != name.Length)
			{
				text3 = name.Substring(num2 + 1);
			}
			if (text.Equals("PreBuild", StringComparison.InvariantCultureIgnoreCase) || text.Equals("PostBuild", StringComparison.InvariantCultureIgnoreCase))
			{
				string text4 = null;
				bool ignoreError = false;
				if (!string.IsNullOrEmpty(text3))
				{
					string[] array = text3.Split(',', '=');
					if (array.Length > 0)
					{
						int num3 = (string.IsNullOrEmpty(array[0].Trim()) ? 1 : 0);
						for (int i = num3; i < array.Length; i++)
						{
							array[i] = array[i].Trim();
							if (array[i].Equals("Configuration", StringComparison.InvariantCultureIgnoreCase))
							{
								text4 = array[++i].Trim();
							}
							else if (array[i].Equals("IgnoreExitCode", StringComparison.InvariantCultureIgnoreCase))
							{
								ignoreError = true;
							}
						}
					}
				}
				AddBuildEvent(text, text2, string.IsNullOrEmpty(text4) ? string.Empty : ("'$(Configuration)' == '" + text4 + "'"), ignoreError, ref preBuild, ref postBuild);
				continue;
			}
			int num4 = 0;
			bool flag2 = true;
			ProjectItem val2 = null;
			if (!text.Equals("remove", StringComparison.InvariantCultureIgnoreCase))
			{
				val2 = MakeProjectItem(text, text2);
				flag2 = val2 != null;
			}
			bool flag3 = false;
			foreach (ProjectItem item3 in ((AbstractProject)this).Items)
			{
				if (item3.Include.Equals(text2, StringComparison.InvariantCultureIgnoreCase))
				{
					num4++;
					ProjectService.RemoveProjectItem((IProject)(object)this, item3);
				}
				if (item3.Include.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					flag3 = true;
					num4++;
					if (flag2)
					{
						ProjectService.RemoveProjectItem((IProject)(object)this, item3);
					}
				}
				if (num4 == 2)
				{
					break;
				}
			}
			if (text.Equals("remove", StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			if (val2 != null)
			{
				if (!string.IsNullOrEmpty(text3))
				{
					string[] array2 = text3.Split(',', '=');
					if (array2.Length > 2)
					{
						int num5 = (string.IsNullOrEmpty(array2[0].Trim()) ? 1 : 0);
						for (int j = num5; j < array2.Length; j += 2)
						{
							val2.SetEvaluatedMetadata(array2[j].Trim(), array2[j + 1].Trim());
						}
					}
				}
				val2.SetEvaluatedMetadata<bool>("Generated", true);
				val2.SetEvaluatedMetadata<bool>("ProjectGenerated", true);
				DoAddProjectItem(val2);
			}
			else if (!flag3)
			{
				MessageBox.Show(string.Format(ResourceService.GetString("Clarion.Project.InvalidNode"), text, text2, name, ((AbstractSolutionFolder)this).Name), ResourceService.GetString("Clarion.Project.Service"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		text = null;
		text2 = null;
		text3 = null;
		((AbstractProject)this).Save();
		preBuild?.Save(((AbstractProject)this).FileName + ".MultiplePreBuilds.Targets");
		postBuild?.Save(((AbstractProject)this).FileName + ".MultiplePostBuilds.Targets");
		if (clarionProjectContent != null)
		{
			clarionProjectContent.IsParseNewItems = true;
		}
	}

	protected virtual bool IsDebug(string value)
	{
		return value == "Full";
	}

	public bool ConfigurationIsDebug(string configuration)
	{
		bool result = false;
		BuildPropertyGroupCollection propertyGroups = ((MSBuildBasedProject)this).MSBuildProject.PropertyGroups;
		foreach (BuildPropertyGroup item in propertyGroups)
		{
			if (item.Condition == null || !item.Condition.Contains(configuration))
			{
				continue;
			}
			foreach (BuildProperty item2 in item)
			{
				if (item2.Name == DebugProperty)
				{
					result = IsDebug(item2.Value);
					break;
				}
			}
			break;
		}
		return result;
	}
}

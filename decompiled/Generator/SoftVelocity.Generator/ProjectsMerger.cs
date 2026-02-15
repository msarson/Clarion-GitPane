using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Clarion.PRJ;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Build.BuildEngine;
using SoftVelocity.Common;

namespace SoftVelocity.Generator;

internal sealed class ProjectsMerger
{
	public class ProjectsMergerEventArgs : EventArgs
	{
		private PRJFile _oldAppPrj;

		private PRJFile _newAppPrj;

		private bool _initial;

		private bool _saveTargetIprj;

		public PRJFile OldAppPrj => _oldAppPrj;

		public PRJFile NewAppPrj => _newAppPrj;

		public bool Initial => _initial;

		public bool SaveTargetIprj => _saveTargetIprj;

		public ProjectsMergerEventArgs(PRJFile oldAppPrj, PRJFile newAppPrj, bool initial, bool saveTargetIprj)
		{
			_oldAppPrj = oldAppPrj;
			_newAppPrj = newAppPrj;
			_initial = initial;
			_saveTargetIprj = saveTargetIprj;
		}
	}

	private static bool _Merging;

	internal static bool Merging => _Merging;

	public static event EventHandler<ProjectsMergerEventArgs> MergingEnded;

	public static event EventHandler<EventArgs> MergerInitRemoving;

	public static event EventHandler<EventArgs> MergerFinishRemoving;

	private ProjectsMerger()
	{
	}

	static ProjectsMerger()
	{
		_Merging = false;
	}

	internal static void Merge(PRJFile oldAppPrj, PRJFile newAppPrj, IProject targetIprj, bool initial, string targetLanguage, bool saveTargetIprj)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		if (oldAppPrj == null || newAppPrj == null)
		{
			LoggingService.Error((object)"ProjectsMerger called with null projects.");
			return;
		}
		LoggingService.Info((object)("ProjectsMerger Merging project: " + targetIprj.FileName));
		FileAttributes fileAttributes = FileAttributes.Normal;
		if (!string.IsNullOrEmpty(targetIprj.FileName) && File.Exists(targetIprj.FileName))
		{
			fileAttributes = File.GetAttributes(targetIprj.FileName);
		}
		if ((fileAttributes & FileAttributes.ReadOnly) != 0)
		{
			ApplicationService.SetText($"The project file is read only: {targetIprj.FileName}");
		}
		else
		{
			_Merging = true;
			StatusBarService.SetMessage("Refreshing Project Data");
			if (targetIprj is CommonClarionProject)
			{
				CommonClarionProject commonClarionProject = (CommonClarionProject)(object)targetIprj;
				commonClarionProject.Merge(oldAppPrj, newAppPrj, initial, targetLanguage);
			}
			else if (targetIprj is MSBuildBasedProject)
			{
				LoggingService.Info((object)("ProjectsMerger Merging Not a Clarion project:" + ((object)targetIprj).GetType()));
				MSBuildBasedProject thisProject = (MSBuildBasedProject)targetIprj;
				ParserService.StopParserThread();
				if (!initial)
				{
					MergeMSBuildProject_RemoveOld(oldAppPrj, newAppPrj, thisProject, initial, targetLanguage);
				}
				MergeMSBuildProject_AddNew(newAppPrj, thisProject, initial, targetLanguage);
				ParserService.StartParserThread();
			}
			ApplicationService.WaitForParser();
			if (saveTargetIprj)
			{
				StatusBarService.SetMessage("Saving Project Data");
				targetIprj.Save();
			}
			_Merging = false;
			if (ProjectsMerger.MergingEnded != null)
			{
				ProjectsMerger.MergingEnded(null, new ProjectsMergerEventArgs(oldAppPrj, newAppPrj, initial, saveTargetIprj));
			}
		}
		LoggingService.Info((object)("ProjectsMerger Merging finish for project: " + targetIprj.FileName));
	}

	private static void DoMergerInitRemoving()
	{
		if (ProjectsMerger.MergerInitRemoving != null)
		{
			ProjectsMerger.MergerInitRemoving(null, EventArgs.Empty);
		}
	}

	private static void DoMergerFinishRemoving()
	{
		if (ProjectsMerger.MergerFinishRemoving != null)
		{
			ProjectsMerger.MergerFinishRemoving(null, EventArgs.Empty);
		}
	}

	private static bool RemoveItemFromProject(MSBuildBasedProject thisProject, string item)
	{
		foreach (ProjectItem item2 in ((AbstractProject)thisProject).Items)
		{
			if (item2.Include == item)
			{
				ProjectService.RemoveProjectItem((IProject)(object)thisProject, item2);
				return true;
			}
		}
		return false;
	}

	private static void MergeMSBuildProject_RemoveOld(PRJFile oldAppPrj, PRJFile newAppPrj, MSBuildBasedProject thisProject, bool initial, string targetLanguage)
	{
		bool flag = false;
		foreach (ProjectItem item in ((AbstractProject)thisProject).Items)
		{
			if (item.GetEvaluatedMetadata<bool>("ProjectGenerated", false))
			{
				ProjectService.RemoveProjectItem((IProject)(object)thisProject, item);
			}
		}
		foreach (ProjectCompile item2 in (List<ProjectCompile>)(object)oldAppPrj.ProjectFiles)
		{
			if (!newAppPrj.ProjectFiles.Contains(item2.FileName))
			{
				if (!flag)
				{
					flag = true;
					DoMergerInitRemoving();
				}
				RemoveItemFromProject(thisProject, item2.FileName);
			}
		}
		foreach (GeneratedFile include in oldAppPrj.Includes)
		{
			if (include.IsGenerated && !newAppPrj.Includes.Contains(include))
			{
				if (!flag)
				{
					flag = true;
					DoMergerInitRemoving();
				}
				RemoveItemFromProject(thisProject, include.Name);
			}
		}
		foreach (GeneratedFile linkFile in oldAppPrj.LinkFiles)
		{
			if (!linkFile.IsGenerated || newAppPrj.LinkFiles.Contains(linkFile))
			{
				continue;
			}
			if (!flag)
			{
				flag = true;
				DoMergerInitRemoving();
			}
			if (!RemoveItemFromProject(thisProject, linkFile.Name))
			{
				string[] array = linkFile.Name.Split('(', ')');
				if (array.Length > 1)
				{
					RemoveItemFromProject(thisProject, array[1]);
				}
			}
		}
		foreach (string reference in oldAppPrj.References)
		{
			if (!newAppPrj.References.Contains(reference))
			{
				if (!flag)
				{
					flag = true;
					DoMergerInitRemoving();
				}
				RemoveItemFromProject(thisProject, reference);
			}
		}
		bool flag2 = false;
		List<ProjectCompile> list = new List<ProjectCompile>((IEnumerable<ProjectCompile>)newAppPrj.ProjectFiles);
		foreach (ProjectItem item3 in ((AbstractProject)thisProject).Items)
		{
			if (string.IsNullOrEmpty(item3.GetMetadata("Generated")))
			{
				continue;
			}
			foreach (ProjectCompile item4 in list)
			{
				if (item3.Include == item4.FileName)
				{
					flag2 = true;
					list.Remove(item4);
					break;
				}
			}
			if (!flag2)
			{
				flag = true;
				ProjectService.RemoveProjectItem((IProject)(object)thisProject, item3);
			}
		}
		list.Clear();
		list = null;
		if (flag)
		{
			DoMergerFinishRemoving();
		}
	}

	private static void MergeMSBuildProject_RemoveFalseAdd(PRJFile appPrj)
	{
		string linkBuildAction = null;
		string linkFileName = null;
		string linkCopyAction = null;
		List<string> list = new List<string>();
		foreach (GeneratedFile linkFile in appPrj.LinkFiles)
		{
			if (ParseTemplatePROJECT(linkFile.Name, out linkFileName, out linkBuildAction, out linkCopyAction) && linkBuildAction.Equals("remove", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add(linkFileName);
			}
		}
		if (list.Count <= 1)
		{
			return;
		}
		for (int num = appPrj.LinkFiles.Count - 1; num >= 0; num--)
		{
			if (list.Contains(appPrj.LinkFiles[num].Name))
			{
				appPrj.LinkFiles.RemoveAt(num);
			}
		}
	}

	private static bool ParseTemplatePROJECT(string linkName, out string linkFileName, out string linkBuildAction, out string linkCopyAction)
	{
		linkBuildAction = string.Empty;
		linkFileName = string.Empty;
		linkCopyAction = string.Empty;
		if (linkName.Contains("("))
		{
			int num = linkName.IndexOf('(');
			int num2 = linkName.LastIndexOf(')');
			if (num2 != -1)
			{
				linkBuildAction = linkName.Substring(0, num).Trim();
				linkFileName = linkName.Substring(num + 1, num2 - num - 1).Trim();
				if (num2 != linkName.Length)
				{
					linkCopyAction = linkName.Substring(num2 + 1);
				}
				return true;
			}
		}
		return false;
	}

	private static void MergeMSBuildProject_AddNew(PRJFile appPrj, MSBuildBasedProject thisProject, bool initial, string targetLanguage)
	{
		MergeMSBuildProject_RemoveFalseAdd(appPrj);
		foreach (ProjectCompile item in (List<ProjectCompile>)(object)appPrj.ProjectFiles)
		{
			bool flag = false;
			foreach (ProjectItem item2 in ((AbstractProject)thisProject).Items)
			{
				if (item2.Include == item.FileName)
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
			BuildItem buildItem = new BuildItem("Compile", item.FileName);
			ProjectItem val = ((AbstractProject)thisProject).CreateProjectItem(buildItem);
			if (val != null)
			{
				if (item.Generated)
				{
					val.SetMetadata("Generated", "true");
				}
				ProjectService.AddProjectItem((IProject)(object)thisProject, val);
			}
		}
		string linkBuildAction = null;
		string linkFileName = null;
		string linkCopyAction = null;
		string empty = string.Empty;
		List<string> list = new List<string>();
		bool flag2 = false;
		foreach (GeneratedFile linkFile in appPrj.LinkFiles)
		{
			empty = linkFile.Name;
			if (!ParseTemplatePROJECT(linkFile.Name, out linkFileName, out linkBuildAction, out linkCopyAction))
			{
				linkBuildAction = "Compile";
				linkFileName = linkFile.Name;
				linkCopyAction = string.Empty;
			}
			if (linkBuildAction.Equals("PreBuild", StringComparison.InvariantCultureIgnoreCase) || linkBuildAction.Equals("PostBuild", StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			int num = 0;
			bool flag3 = true;
			ProjectItem val2 = null;
			if (!linkBuildAction.Equals("remove", StringComparison.InvariantCultureIgnoreCase))
			{
				BuildItem buildItem2 = new BuildItem(linkBuildAction, empty);
				val2 = ((AbstractProject)thisProject).CreateProjectItem(buildItem2);
				if (val2 != null && linkFile.IsGenerated)
				{
					val2.SetMetadata("Generated", "true");
				}
				flag3 = val2 != null;
			}
			bool flag4 = false;
			foreach (ProjectItem item3 in ((AbstractProject)thisProject).Items)
			{
				if (item3.Include.Equals(linkFileName, StringComparison.InvariantCultureIgnoreCase))
				{
					num++;
					ProjectService.RemoveProjectItem((IProject)(object)thisProject, item3);
				}
				if (item3.Include.Equals(empty, StringComparison.InvariantCultureIgnoreCase))
				{
					flag4 = true;
					num++;
					if (flag3)
					{
						ProjectService.RemoveProjectItem((IProject)(object)thisProject, item3);
					}
				}
				if (num == 2)
				{
					break;
				}
			}
			if (linkBuildAction.Equals("remove", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add(linkFileName);
			}
			else if (val2 != null && !list.Contains(linkFileName))
			{
				if (!string.IsNullOrEmpty(linkCopyAction))
				{
					string[] array = linkCopyAction.Split(',', '=');
					if (array.Length > 2)
					{
						int num2 = (string.IsNullOrEmpty(array[0].Trim()) ? 1 : 0);
						for (int i = num2; i < array.Length; i += 2)
						{
							val2.SetEvaluatedMetadata(array[i].Trim(), array[i + 1].Trim());
						}
					}
				}
				val2.SetEvaluatedMetadata<bool>("Generated", true);
				val2.SetEvaluatedMetadata<bool>("ProjectGenerated", true);
				bool flag5 = true;
				while (flag5)
				{
					flag5 = false;
					foreach (ProjectItem item4 in ((AbstractProject)thisProject).Items)
					{
						if (object.Equals(item4.FileName, val2.FileName))
						{
							ProjectService.RemoveProjectItem((IProject)(object)thisProject, item4);
							flag5 = true;
							break;
						}
					}
				}
				ProjectService.AddProjectItem((IProject)(object)thisProject, val2);
			}
			else if (!flag4)
			{
				MessageBox.Show($"Invalid Node  - BuildAction:{linkBuildAction} FileName:{linkFileName} LinkName:{empty} Project:{((AbstractSolutionFolder)thisProject).Name}", "Project Merger", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		linkBuildAction = null;
		linkFileName = null;
		linkCopyAction = null;
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Common;

public class ProjectDependencyEditorHelper : IComparer<IProject>
{
	private static Dictionary<IProject, List<IProject>> TempDependenciesAdded;

	private static Dictionary<IProject, List<IProject>> TempDependenciesRemoved;

	private static bool inited;

	private List<IProject> sortedProjects = new List<IProject>();

	public static void FinishData()
	{
		inited = false;
		InitData();
		inited = false;
	}

	public static void InitData()
	{
		if (inited)
		{
			return;
		}
		if (TempDependenciesAdded != null)
		{
			foreach (List<IProject> value in TempDependenciesAdded.Values)
			{
				value.Clear();
			}
			TempDependenciesAdded.Clear();
		}
		else
		{
			TempDependenciesAdded = new Dictionary<IProject, List<IProject>>();
		}
		if (TempDependenciesRemoved != null)
		{
			foreach (List<IProject> value2 in TempDependenciesRemoved.Values)
			{
				value2.Clear();
			}
			TempDependenciesRemoved.Clear();
		}
		else
		{
			TempDependenciesRemoved = new Dictionary<IProject, List<IProject>>();
		}
		inited = true;
	}

	public static void AddTempDependency(IProject childProject, IProject projectToAdd)
	{
		InitData();
		if (childProject == null)
		{
			throw new NullReferenceException();
		}
		if (projectToAdd == null)
		{
			throw new NullReferenceException();
		}
		if (TempDependenciesRemoved.ContainsKey(childProject) && TempDependenciesRemoved[childProject].Contains(projectToAdd))
		{
			TempDependenciesRemoved[childProject].Remove(projectToAdd);
		}
		if (IsDependency(childProject, projectToAdd))
		{
			return;
		}
		if (TempDependenciesAdded.ContainsKey(childProject))
		{
			if (!TempDependenciesAdded[childProject].Contains(projectToAdd))
			{
				TempDependenciesAdded[childProject].Add(projectToAdd);
			}
		}
		else
		{
			TempDependenciesAdded.Add(childProject, new List<IProject>((IEnumerable<IProject>)(object)new IProject[1] { projectToAdd }));
		}
	}

	public static void RemoveTempDependency(IProject childProject, IProject projectToAdd)
	{
		InitData();
		if (childProject == null)
		{
			throw new NullReferenceException();
		}
		if (projectToAdd == null)
		{
			throw new NullReferenceException();
		}
		if (TempDependenciesAdded.ContainsKey(childProject) && TempDependenciesAdded[childProject].Contains(projectToAdd))
		{
			TempDependenciesAdded[childProject].Remove(projectToAdd);
		}
		if (!IsDependency(childProject, projectToAdd))
		{
			return;
		}
		if (TempDependenciesRemoved.ContainsKey(childProject))
		{
			if (!TempDependenciesRemoved[childProject].Contains(projectToAdd))
			{
				TempDependenciesRemoved[childProject].Add(projectToAdd);
			}
		}
		else
		{
			TempDependenciesRemoved.Add(childProject, new List<IProject>((IEnumerable<IProject>)(object)new IProject[1] { projectToAdd }));
		}
	}

	public static void UpdateDependenciesOnSolution()
	{
		InitData();
		foreach (IProject key in TempDependenciesRemoved.Keys)
		{
			foreach (IProject item in TempDependenciesRemoved[key])
			{
				RemoveDependency(key, item);
			}
		}
		foreach (IProject key2 in TempDependenciesAdded.Keys)
		{
			foreach (IProject item2 in TempDependenciesAdded[key2])
			{
				AddDependency(key2, item2);
			}
		}
		FinishData();
	}

	private static bool IsTempRemovedDependency(IProject proj, IProject parentProjToTest)
	{
		if (TempDependenciesRemoved.ContainsKey(proj) && TempDependenciesRemoved[proj].Contains(parentProjToTest))
		{
			return true;
		}
		return false;
	}

	public static bool HasTempDependencies(IProject proj)
	{
		InitData();
		if (TempDependenciesAdded.ContainsKey(proj) && TempDependenciesAdded[proj].Count > 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsTempDependency(IProject proj, IProject parentProjToTest)
	{
		InitData();
		if (TempDependenciesAdded.ContainsKey(proj) && TempDependenciesAdded[proj].Contains(parentProjToTest))
		{
			return true;
		}
		if (TempDependenciesRemoved.ContainsKey(proj) && TempDependenciesRemoved[proj].Contains(parentProjToTest))
		{
			return false;
		}
		return IsDependency(proj, parentProjToTest);
	}

	public static void AddDependency(IProject childProject, IProject projectToAdd)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		if (childProject == null)
		{
			throw new NullReferenceException();
		}
		if (projectToAdd == null)
		{
			throw new NullReferenceException();
		}
		ProjectSection val = null;
		if (childProject.ProjectSections == null)
		{
			throw new NullReferenceException("ProjectSections");
		}
		foreach (ProjectSection projectSection in childProject.ProjectSections)
		{
			if (projectSection.Name == "ProjectDependencies")
			{
				val = projectSection;
				break;
			}
		}
		if (val == null)
		{
			val = new ProjectSection("ProjectDependencies", "postProject");
			childProject.ProjectSections.Add(val);
		}
		if (val == null)
		{
			return;
		}
		bool flag = false;
		foreach (SolutionItem item in val.Items)
		{
			if (item.Name == ((ISolutionFolder)projectToAdd).IdGuid)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			val.Items.Add(new SolutionItem(((ISolutionFolder)projectToAdd).IdGuid, ((ISolutionFolder)projectToAdd).IdGuid));
		}
	}

	public static void RemoveDependency(IProject childProject, IProject projectToRemove)
	{
		if (childProject == null)
		{
			throw new NullReferenceException();
		}
		if (projectToRemove == null)
		{
			throw new NullReferenceException();
		}
		if (childProject.ProjectSections == null)
		{
			return;
		}
		ProjectSection val = null;
		foreach (ProjectSection projectSection in childProject.ProjectSections)
		{
			if (projectSection.Name == "ProjectDependencies")
			{
				val = projectSection;
				break;
			}
		}
		if (val == null)
		{
			return;
		}
		foreach (SolutionItem item in val.Items)
		{
			if (((ISolutionFolder)projectToRemove).IdGuid.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
			{
				val.Items.Remove(item);
				break;
			}
		}
	}

	public static bool IsReferenced(IProject proj, IProject parentProjToTest)
	{
		foreach (ProjectItem item in proj.Items)
		{
			if (item is ProjectReferenceProjectItem)
			{
				ProjectReferenceProjectItem val = (ProjectReferenceProjectItem)(object)((item is ProjectReferenceProjectItem) ? item : null);
				if (val != null && ((ISolutionFolder)parentProjToTest).IdGuid.Equals(val.ProjectGuid, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsDependency(IProject proj, IProject parentProjToTest)
	{
		Solution openSolution = ProjectService.OpenSolution;
		if (openSolution != null && proj.ProjectSections != null)
		{
			foreach (ProjectSection projectSection in proj.ProjectSections)
			{
				if (!projectSection.Name.Equals("ProjectDependencies", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				foreach (SolutionItem item in projectSection.Items)
				{
					if (((ISolutionFolder)parentProjToTest).IdGuid.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool IsParentProject(IProject proj, IProject parentProjToTest)
	{
		if (!IsReferenced(proj, parentProjToTest))
		{
			return IsTempDependency(proj, parentProjToTest);
		}
		return true;
	}

	public static bool IsCircularRefence(IProject proj, IProject parentProjToTest)
	{
		List<IProject> visited = new List<IProject>();
		return FindCircularRefenceAux(proj, parentProjToTest, ref visited);
	}

	private static bool FindCircularRefenceAux(IProject proj, IProject parentProjToTest, ref List<IProject> visited)
	{
		if (((ISolutionFolder)proj).IdGuid.Equals(((ISolutionFolder)parentProjToTest).IdGuid, StringComparison.OrdinalIgnoreCase) || IsParentProject(parentProjToTest, proj))
		{
			return true;
		}
		visited.Add(parentProjToTest);
		foreach (ProjectItem item in parentProjToTest.Items)
		{
			if (!(item is ProjectReferenceProjectItem))
			{
				continue;
			}
			ProjectReferenceProjectItem val = (ProjectReferenceProjectItem)(object)((item is ProjectReferenceProjectItem) ? item : null);
			if (val != null)
			{
				IProject referencedProject = val.ReferencedProject;
				if (referencedProject == null)
				{
					string format = ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MissingProject");
					string caption = ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.InvalidProject");
					MessageBox.Show(string.Format(format, ((ISolutionFolder)parentProjToTest).Name, val.ProjectName), caption);
				}
				else if (!visited.Contains(referencedProject) && FindCircularRefenceAux(proj, referencedProject, ref visited))
				{
					return true;
				}
			}
		}
		Solution openSolution = ProjectService.OpenSolution;
		if (openSolution != null && parentProjToTest.ProjectSections != null)
		{
			foreach (ProjectSection projectSection in parentProjToTest.ProjectSections)
			{
				if (!projectSection.Name.Equals("ProjectDependencies", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				foreach (SolutionItem item2 in projectSection.Items)
				{
					foreach (IProject project in openSolution.Projects)
					{
						if (((ISolutionFolder)project).IdGuid.Equals(item2.Name, StringComparison.OrdinalIgnoreCase))
						{
							if (!visited.Contains(project))
							{
								if (IsTempRemovedDependency(parentProjToTest, project))
								{
									visited.Add(project);
									break;
								}
								if (FindCircularRefenceAux(proj, project, ref visited))
								{
									return true;
								}
								break;
							}
							break;
						}
					}
				}
			}
		}
		if (TempDependenciesAdded.ContainsKey(proj))
		{
			foreach (IProject item3 in TempDependenciesAdded[proj])
			{
				if (!visited.Contains(item3) && FindCircularRefenceAux(proj, item3, ref visited))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasReferences(IProject p)
	{
		foreach (ProjectItem item in p.Items)
		{
			if (item is ProjectReferenceProjectItem)
			{
				return true;
			}
		}
		return false;
	}

	private bool HasDependencies(IProject proj)
	{
		if (proj.ProjectSections != null)
		{
			Solution openSolution = ProjectService.OpenSolution;
			if (openSolution != null)
			{
				foreach (ProjectSection projectSection in proj.ProjectSections)
				{
					if (!projectSection.Name.Equals("ProjectDependencies", StringComparison.OrdinalIgnoreCase) || projectSection.Items.Count <= 0)
					{
						continue;
					}
					foreach (SolutionItem item in projectSection.Items)
					{
						foreach (IProject project in openSolution.Projects)
						{
							if (((ISolutionFolder)project).IdGuid.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
							{
								if (!IsTempRemovedDependency(proj, project))
								{
									return true;
								}
								break;
							}
						}
					}
				}
			}
		}
		return false;
	}

	public void Refresh()
	{
		sortedProjects.Clear();
		Solution openSolution = ProjectService.OpenSolution;
		if (openSolution == null)
		{
			return;
		}
		foreach (IProject project in openSolution.Projects)
		{
			if (!HasReferences(project) && !HasDependencies(project) && !HasTempDependencies(project))
			{
				sortedProjects.Add(project);
			}
		}
		foreach (IProject project2 in openSolution.Projects)
		{
			AddProject(project2, sortedProjects, new Stack<IProject>());
		}
	}

	private void AddProject(IProject proj, List<IProject> sortedProjects, Stack<IProject> refStack)
	{
		if (proj == null || sortedProjects.Contains(proj))
		{
			return;
		}
		refStack.Push(proj);
		foreach (ProjectItem item in proj.Items)
		{
			if (item == null || !(item is ProjectReferenceProjectItem))
			{
				continue;
			}
			ProjectReferenceProjectItem val = (ProjectReferenceProjectItem)(object)((item is ProjectReferenceProjectItem) ? item : null);
			if (val != null)
			{
				IProject referencedProject = val.ReferencedProject;
				if (referencedProject == null)
				{
					string format = ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MissingProject");
					string caption = ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.InvalidProject");
					MessageBox.Show(string.Format(format, ((ISolutionFolder)proj).Name, val.ProjectName), caption);
				}
				else if (!refStack.Contains(referencedProject))
				{
					AddProject(referencedProject, sortedProjects, refStack);
				}
			}
		}
		Solution openSolution = ProjectService.OpenSolution;
		if (openSolution != null && proj.ProjectSections != null)
		{
			foreach (ProjectSection projectSection in proj.ProjectSections)
			{
				if (!projectSection.Name.Equals("ProjectDependencies", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				foreach (SolutionItem item2 in projectSection.Items)
				{
					foreach (IProject project in openSolution.Projects)
					{
						if (((ISolutionFolder)project).IdGuid.Equals(item2.Name, StringComparison.OrdinalIgnoreCase))
						{
							if (!refStack.Contains(project) && !IsTempRemovedDependency(proj, project))
							{
								AddProject(project, sortedProjects, refStack);
							}
							break;
						}
					}
				}
			}
		}
		if (TempDependenciesAdded.ContainsKey(proj))
		{
			foreach (IProject item3 in TempDependenciesAdded[proj])
			{
				if (!refStack.Contains(item3))
				{
					AddProject(item3, sortedProjects, refStack);
				}
			}
		}
		refStack.Pop();
		sortedProjects.Add(proj);
	}

	public int Compare(IProject x, IProject y)
	{
		if (x == null || y == null)
		{
			return 0;
		}
		if (sortedProjects.Count == 0)
		{
			Refresh();
		}
		int num;
		int value;
		try
		{
			num = sortedProjects.IndexOf(x);
			value = sortedProjects.IndexOf(y);
		}
		catch
		{
			return 0;
		}
		return num.CompareTo(value);
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace CommonSources.Project;

public static class AutoLibLinkService
{
	private static Dictionary<string, IProject> _prjFiles;

	private static List<string> _tmpLibsFiles;

	private static bool _AllowLibHunterInited;

	private static bool _AllowLibHunter;

	private static Properties prop;

	private static bool _Inited;

	public static bool AllowLibHunter
	{
		get
		{
			Init();
			if (_AllowLibHunterInited)
			{
				return _AllowLibHunter;
			}
			if (!prop.Get<bool>("OnSolutionLoadedAllwaysAllowLibHunter", false))
			{
				if (MessageBox.Show(ResourceService.GetString("Clarion.Project.LibHunterQuestion"), ResourceService.GetString("Global.QuestionText"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					_AllowLibHunter = true;
				}
			}
			else
			{
				_AllowLibHunter = true;
			}
			_AllowLibHunterInited = true;
			return _AllowLibHunter;
		}
	}

	static AutoLibLinkService()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		_prjFiles = new Dictionary<string, IProject>(StringComparer.OrdinalIgnoreCase);
		_tmpLibsFiles = new List<string>();
		_AllowLibHunterInited = false;
		_AllowLibHunter = false;
		prop = PropertyService.Get<Properties>("SoftVelocity.Generator.ApplicationService", new Properties());
		_Inited = false;
	}

	private static void ProjectService_SolutionClosed(object sender, EventArgs e)
	{
		ProjectService.SolutionClosed -= ProjectService_SolutionClosed;
		_prjFiles.Clear();
		_tmpLibsFiles.Clear();
		_Inited = false;
		_AllowLibHunterInited = false;
		_AllowLibHunter = false;
	}

	public static IProject GetProject(string libname)
	{
		Init();
		if (_Inited)
		{
			return _prjFiles[libname];
		}
		return null;
	}

	public static bool IsLibAProject(string libname)
	{
		Init();
		if (_Inited)
		{
			return _prjFiles.ContainsKey(libname);
		}
		return false;
	}

	public static void RemeberLibAsProject(string libname, IProject project)
	{
		if (!IsLibAProject(libname))
		{
			_prjFiles.Add(libname, project);
			if (IsLibProcessing(libname))
			{
				_tmpLibsFiles.Remove(libname);
			}
		}
	}

	public static bool IsLibProcessing(string libname)
	{
		Init();
		if (_Inited)
		{
			return _tmpLibsFiles.Contains(libname);
		}
		return false;
	}

	public static void ProcessingLibAsProject(string libname)
	{
		if (!IsLibProcessing(libname))
		{
			_tmpLibsFiles.Add(libname);
		}
	}

	public static void Init()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (_Inited)
		{
			return;
		}
		ProjectService.SolutionClosed += ProjectService_SolutionClosed;
		if (ProjectService.OpenSolution == null)
		{
			return;
		}
		foreach (IProject project in ProjectService.OpenSolution.Projects)
		{
			foreach (ProjectItem item in project.GetItemsOfType(ItemType.ProjectReference))
			{
				ProjectReferenceProjectItem val = (ProjectReferenceProjectItem)(object)((item is ProjectReferenceProjectItem) ? item : null);
				if (val != null && !_prjFiles.ContainsKey(val.ProjectName))
				{
					_prjFiles.Add(val.ProjectName, val.ReferencedProject);
				}
			}
		}
		_Inited = true;
	}
}

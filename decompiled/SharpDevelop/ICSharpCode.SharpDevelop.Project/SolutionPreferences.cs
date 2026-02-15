using System;
using System.Collections.Generic;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class SolutionPreferences : IMementoCapable
{
	private Solution solution;

	private Properties properties = new Properties();

	private string startupProject = "";

	private string activeConfiguration;

	private string activePlatform = "Any CPU";

	private List<string> _FilesToOpenAfterSolutionLoad = new List<string>();

	public bool HasFilesToOpenAfterSolutionLoad => _FilesToOpenAfterSolutionLoad.Count > 0;

	public string[] FilesToOpenAfterSolutionLoad => _FilesToOpenAfterSolutionLoad.ToArray();

	public Properties Properties => properties;

	public IProject StartupProject
	{
		get
		{
			if (startupProject.Length == 0)
			{
				return null;
			}
			foreach (IProject project in solution.Projects)
			{
				if (project.IdGuid.Equals(startupProject, StringComparison.OrdinalIgnoreCase))
				{
					return project;
				}
			}
			return null;
		}
		set
		{
			SetStartupProject((value != null) ? value.IdGuid : "");
		}
	}

	public string ActiveConfiguration
	{
		get
		{
			return activeConfiguration;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			activeConfiguration = value;
		}
	}

	public string ActivePlatform
	{
		get
		{
			return activePlatform;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			activePlatform = value;
		}
	}

	public event EventHandler StartupProjectChanged;

	public void SetFilesToOpenAfterSolutionLoad(string[] files)
	{
		ClearFilesToOpenAfterSolutionLoad();
		_FilesToOpenAfterSolutionLoad.AddRange(files);
	}

	public void ClearFilesToOpenAfterSolutionLoad()
	{
		_FilesToOpenAfterSolutionLoad.Clear();
	}

	internal SolutionPreferences(Solution solution)
	{
		this.solution = solution;
		activeConfiguration = (PropertyService.Get("SharpDevelop.UseReleaseAsDefault", defaultValue: true) ? "Release" : "Debug");
	}

	private void SetStartupProject(string value)
	{
		if (value != startupProject)
		{
			startupProject = value;
			if (this.StartupProjectChanged != null)
			{
				this.StartupProjectChanged(this, EventArgs.Empty);
			}
		}
	}

	Properties IMementoCapable.CreateMemento()
	{
		Properties properties = this.properties;
		properties.Set("StartupProject", startupProject);
		properties.Set("ActiveConfiguration", activeConfiguration);
		properties.Set("ActivePlatform", activePlatform);
		properties.Set("OpenFiles", FilesToOpenAfterSolutionLoad);
		return properties;
	}

	void IMementoCapable.SetMemento(Properties memento)
	{
		SetStartupProject(memento.Get("StartupProject", ""));
		string item = memento.Get("ActiveConfiguration", activeConfiguration);
		string item2 = memento.Get("ActivePlatform", activePlatform);
		string[] filesToOpenAfterSolutionLoad = memento.Get("OpenFiles", new string[0]);
		SetFilesToOpenAfterSolutionLoad(filesToOpenAfterSolutionLoad);
		IList<string> configurationNames = solution.GetConfigurationNames();
		if (!configurationNames.Contains(item))
		{
			item = configurationNames[0];
		}
		configurationNames = solution.GetPlatformNames();
		if (!configurationNames.Contains(item2))
		{
			item2 = configurationNames[0];
		}
		ActiveConfiguration = item;
		ActivePlatform = item2;
		properties = memento;
	}
}

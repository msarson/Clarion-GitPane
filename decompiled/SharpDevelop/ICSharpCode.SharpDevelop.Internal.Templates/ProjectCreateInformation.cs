using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class ProjectCreateInformation
{
	private string projectName;

	private string combinePath;

	private string projectBasePath;

	private string outputProjectFileName;

	private string rootNamespace;

	private Solution solution;

	internal List<string> CreatedProjects = new List<string>();

	public string OutputProjectFileName
	{
		get
		{
			return outputProjectFileName;
		}
		set
		{
			outputProjectFileName = value;
		}
	}

	public string ProjectName
	{
		get
		{
			return projectName;
		}
		set
		{
			projectName = value;
		}
	}

	public string RootNamespace
	{
		get
		{
			return rootNamespace;
		}
		set
		{
			rootNamespace = value;
		}
	}

	public string SolutionPath
	{
		get
		{
			return combinePath;
		}
		set
		{
			combinePath = value;
		}
	}

	public string ProjectBasePath
	{
		get
		{
			return projectBasePath;
		}
		set
		{
			projectBasePath = value;
		}
	}

	public Solution Solution
	{
		get
		{
			return solution;
		}
		set
		{
			solution = value;
		}
	}
}

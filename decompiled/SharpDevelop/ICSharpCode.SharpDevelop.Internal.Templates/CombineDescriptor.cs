using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class CombineDescriptor
{
	private class SolutionFolderDescriptor
	{
		internal string name;

		internal List<ProjectDescriptor> projectDescriptors = new List<ProjectDescriptor>();

		internal List<SolutionFolderDescriptor> solutionFoldersDescriptors = new List<SolutionFolderDescriptor>();

		internal void Read(XmlElement element, string hintPath)
		{
			name = element.GetAttribute("name");
			foreach (XmlNode childNode in element.ChildNodes)
			{
				switch (childNode.Name)
				{
				case "Project":
					projectDescriptors.Add(new ProjectDescriptor((XmlElement)childNode, hintPath));
					break;
				case "SolutionFolder":
					solutionFoldersDescriptors.Add(new SolutionFolderDescriptor((XmlElement)childNode, hintPath));
					break;
				}
			}
		}

		internal bool AddContents(Solution solution, ProjectCreateInformation projectCreateInformation, string defaultLanguage, ISolutionFolderContainer parentFolder)
		{
			foreach (SolutionFolderDescriptor solutionFoldersDescriptor in solutionFoldersDescriptors)
			{
				SolutionFolder solutionFolder = solution.CreateFolder(solutionFoldersDescriptor.name);
				parentFolder.AddFolder(solutionFolder);
				solutionFoldersDescriptor.AddContents(solution, projectCreateInformation, defaultLanguage, solutionFolder);
			}
			foreach (ProjectDescriptor projectDescriptor in projectDescriptors)
			{
				IProject project = projectDescriptor.CreateProject(projectCreateInformation, defaultLanguage);
				if (project == null)
				{
					return false;
				}
				project.Location = FileUtility.GetRelativePath(projectCreateInformation.SolutionPath, project.FileName);
				parentFolder.AddFolder(project);
			}
			return true;
		}

		public SolutionFolderDescriptor(XmlElement element, string hintPath)
		{
			Read(element, hintPath);
		}

		public SolutionFolderDescriptor(string name)
		{
			this.name = name;
		}
	}

	private SolutionFolderDescriptor mainFolder = new SolutionFolderDescriptor("");

	private string name;

	private string startupProject;

	private string relativeDirectory;

	public string StartupProject => startupProject;

	public List<ProjectDescriptor> ProjectDescriptors => mainFolder.projectDescriptors;

	protected CombineDescriptor(string name)
	{
		this.name = name;
	}

	public string CreateSolution(ProjectCreateInformation projectCreateInformation, string defaultLanguage)
	{
		Solution solution = (projectCreateInformation.Solution = new Solution());
		string text = (solution.Name = StringParser.Parse(name, new string[1, 2] { { "ProjectName", projectCreateInformation.ProjectName } }));
		string solutionPath = projectCreateInformation.SolutionPath;
		string projectBasePath = projectCreateInformation.ProjectBasePath;
		if (relativeDirectory != null && relativeDirectory.Length > 0 && relativeDirectory != ".")
		{
			projectCreateInformation.SolutionPath = Path.Combine(projectCreateInformation.SolutionPath, relativeDirectory);
			projectCreateInformation.ProjectBasePath = Path.Combine(projectCreateInformation.SolutionPath, relativeDirectory);
			if (!Directory.Exists(projectCreateInformation.SolutionPath))
			{
				Directory.CreateDirectory(projectCreateInformation.SolutionPath);
			}
			if (!Directory.Exists(projectCreateInformation.ProjectBasePath))
			{
				Directory.CreateDirectory(projectCreateInformation.ProjectBasePath);
			}
		}
		projectCreateInformation.SolutionPath = solutionPath;
		projectCreateInformation.ProjectBasePath = projectBasePath;
		if (!mainFolder.AddContents(solution, projectCreateInformation, defaultLanguage, solution))
		{
			solution.Dispose();
			return null;
		}
		string text3 = Path.Combine(projectCreateInformation.SolutionPath, text + ".sln");
		if (File.Exists(text3))
		{
			StringParser.Properties["combineLocation"] = text3;
			if (MessageService.AskQuestion("${res:ICSharpCode.SharpDevelop.Internal.Templates.CombineDescriptor.OverwriteProjectQuestion}"))
			{
				solution.Save(text3);
			}
		}
		else
		{
			solution.Save(text3);
		}
		solution.Dispose();
		return text3;
	}

	public static CombineDescriptor CreateCombineDescriptor(XmlElement element, string hintPath)
	{
		CombineDescriptor combineDescriptor = new CombineDescriptor(element.Attributes["name"].InnerText);
		if (element.Attributes["directory"] != null)
		{
			combineDescriptor.relativeDirectory = element.Attributes["directory"].InnerText;
		}
		if (element["Options"] != null && element["Options"]["StartupProject"] != null)
		{
			combineDescriptor.startupProject = element["Options"]["StartupProject"].InnerText;
		}
		combineDescriptor.mainFolder.Read(element, hintPath);
		return combineDescriptor;
	}
}

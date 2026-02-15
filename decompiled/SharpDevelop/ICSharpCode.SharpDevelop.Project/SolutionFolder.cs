using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionFolder : AbstractSolutionFolder, ISolutionFolderContainer
{
	public const string FolderGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";

	private List<ISolutionFolder> folders = new List<ISolutionFolder>();

	private List<ProjectSection> sections = new List<ProjectSection>();

	private static Regex sectionHeaderPattern = new Regex("\\s*ProjectSection\\((?<Name>.*)\\)\\s*=\\s*(?<Type>.*)", RegexOptions.Compiled);

	[Browsable(false)]
	public override string TypeGuid
	{
		get
		{
			return "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	[Browsable(false)]
	public bool IsEmpty
	{
		get
		{
			if (Folders.Count == 0)
			{
				return SolutionItems.Items.Count == 0;
			}
			return false;
		}
	}

	[Browsable(false)]
	public List<ProjectSection> Sections => sections;

	[Browsable(false)]
	public List<ISolutionFolder> Folders => folders;

	[Browsable(false)]
	public virtual ProjectSection SolutionItems
	{
		get
		{
			foreach (ProjectSection section in sections)
			{
				if (section.Name == "SolutionItems")
				{
					return section;
				}
			}
			ProjectSection projectSection = new ProjectSection("SolutionItems", "postProject");
			sections.Add(projectSection);
			return projectSection;
		}
	}

	protected SolutionFolder()
	{
	}

	public SolutionFolder(string name, string location, string idGuid)
	{
		base.Location = location;
		base.Name = name;
		IdGuid = idGuid;
	}

	public virtual void AddFolder(ISolutionFolder folder)
	{
		if (string.IsNullOrEmpty(folder.IdGuid))
		{
			folder.IdGuid = Guid.NewGuid().ToString().ToUpperInvariant();
		}
		if (folder.Parent != null)
		{
			folder.Parent.RemoveFolder(folder);
		}
		folder.Parent = this;
		Folders.Add(folder);
	}

	public virtual void RemoveFolder(ISolutionFolder folder)
	{
		for (int i = 0; i < Folders.Count; i++)
		{
			if (folder.IdGuid == Folders[i].IdGuid)
			{
				Folders.RemoveAt(i);
				break;
			}
		}
		if (ParentSolution != null)
		{
			ParentSolution.RemoveProjectConfigurationPlatforms(folder.IdGuid);
		}
	}

	public bool IsAncestorOf(ISolutionFolder folder)
	{
		object obj = folder;
		while (obj != null && obj is ISolutionFolder)
		{
			ISolutionFolder solutionFolder = (ISolutionFolder)obj;
			if (solutionFolder == this)
			{
				return true;
			}
			obj = solutionFolder.Parent;
		}
		return false;
	}

	public static SolutionFolder ReadFolder(TextReader sr, string title, string location, string guid)
	{
		SolutionFolder solutionFolder = new SolutionFolder(title, location, guid);
		ReadProjectSections(sr, solutionFolder.Sections);
		return solutionFolder;
	}

	public static void ReadProjectSections(TextReader sr, ICollection<ProjectSection> sectionList)
	{
		while (true)
		{
			string text = sr.ReadLine();
			if (text == null || text.Trim() == "EndProject")
			{
				break;
			}
			Match match = sectionHeaderPattern.Match(text);
			if (match.Success)
			{
				sectionList.Add(ProjectSection.ReadProjectSection(sr, match.Result("${Name}"), match.Result("${Type}")));
			}
		}
	}
}

using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop.Project;

public interface ISolutionFolderContainer
{
	Solution ParentSolution { get; }

	List<ProjectSection> Sections { get; }

	List<ISolutionFolder> Folders { get; }

	ProjectSection SolutionItems { get; }

	void AddFolder(ISolutionFolder folder);

	void RemoveFolder(ISolutionFolder folder);

	bool IsAncestorOf(ISolutionFolder folder);
}

using System.Collections.ObjectModel;

namespace ICSharpCode.SharpDevelop.Project;

public interface IProjectItemListProvider
{
	ReadOnlyCollection<ProjectItem> Items { get; }

	void AddProjectItem(ProjectItem item);

	bool RemoveProjectItem(ProjectItem item);
}

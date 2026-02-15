using ICSharpCode.SharpDevelop.Project;

namespace SearchAndReplace;

public class WholeSolutionDocumentIterator : AbstractDocumentIterator
{
	public WholeSolutionDocumentIterator(string filePatterns)
		: base(filePatterns)
	{
	}

	protected override void FillList()
	{
		if (ProjectService.OpenSolution == null)
		{
			return;
		}
		foreach (IProject project in ProjectService.OpenSolution.Projects)
		{
			foreach (ProjectItem item in project.Items)
			{
				if (item is FileProjectItem && SearchReplaceUtilities.IsSearchable(item.FileName))
				{
					files.Add(item.FileName);
				}
			}
		}
	}
}

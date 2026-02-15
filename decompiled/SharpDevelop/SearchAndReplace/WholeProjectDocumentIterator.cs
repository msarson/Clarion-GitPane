using ICSharpCode.SharpDevelop.Project;

namespace SearchAndReplace;

public class WholeProjectDocumentIterator : AbstractDocumentIterator
{
	public WholeProjectDocumentIterator(string filePatterns)
		: base(filePatterns)
	{
	}

	protected override void FillList()
	{
		if (ProjectService.CurrentProject == null)
		{
			return;
		}
		foreach (ProjectItem item in ProjectService.CurrentProject.Items)
		{
			if (item is FileProjectItem && SearchReplaceUtilities.IsSearchable(item.FileName))
			{
				files.Add(item.FileName);
			}
		}
	}
}

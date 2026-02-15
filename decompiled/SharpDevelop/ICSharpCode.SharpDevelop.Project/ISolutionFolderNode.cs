namespace ICSharpCode.SharpDevelop.Project;

public interface ISolutionFolderNode
{
	Solution Solution { get; }

	ISolutionFolder Folder { get; }

	ISolutionFolderContainer Container { get; }

	void AddItem(string fileName);
}

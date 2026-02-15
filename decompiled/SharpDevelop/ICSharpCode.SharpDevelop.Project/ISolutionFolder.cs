namespace ICSharpCode.SharpDevelop.Project;

public interface ISolutionFolder
{
	object SyncRoot { get; }

	ISolutionFolderContainer Parent { get; set; }

	Solution ParentSolution { get; }

	string TypeGuid { get; set; }

	string IdGuid { get; set; }

	string Location { get; set; }

	string Name { get; set; }
}

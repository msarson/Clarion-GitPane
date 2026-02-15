using System;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionFolderEventArgs : EventArgs
{
	private ISolutionFolder solutionFolder;

	public ISolutionFolder SolutionFolder => solutionFolder;

	public SolutionFolderEventArgs(ISolutionFolder solutionFolder)
	{
		this.solutionFolder = solutionFolder;
	}
}

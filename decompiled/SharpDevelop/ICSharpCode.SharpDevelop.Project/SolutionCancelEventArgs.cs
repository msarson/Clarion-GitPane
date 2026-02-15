using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionCancelEventArgs : CancelEventArgs
{
	private Solution solution;

	public Solution Solution => solution;

	public SolutionCancelEventArgs(Solution solution)
	{
		this.solution = solution;
	}
}

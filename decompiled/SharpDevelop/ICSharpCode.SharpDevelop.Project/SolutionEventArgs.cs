using System;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionEventArgs : EventArgs
{
	private Solution solution;

	public Solution Solution => solution;

	public SolutionEventArgs(Solution solution)
	{
		this.solution = solution;
	}
}

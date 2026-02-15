using System;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionItemCreatingEventArgs : EventArgs
{
	private SolutionItemNode newNode;

	private Solution solution;

	private SolutionItem item;

	public SolutionItemNode Node
	{
		get
		{
			return newNode;
		}
		set
		{
			newNode = value;
		}
	}

	public SolutionItem Item => item;

	public Solution Solution => solution;

	internal SolutionItemCreatingEventArgs(Solution solution, SolutionItem item)
	{
		this.solution = solution;
		this.item = item;
	}
}

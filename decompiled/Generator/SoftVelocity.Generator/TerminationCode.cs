using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator;

internal class TerminationCode : AbstractCommand
{
	public override void Run()
	{
		if (StartupCode.Called)
		{
			ApplicationService.Instance.CloseDown();
			SolutionItemNode.SolutionItemCreating = (EventHandler<SolutionItemCreatingEventArgs>)Delegate.Remove(SolutionItemNode.SolutionItemCreating, new EventHandler<SolutionItemCreatingEventArgs>(AppItemNode.AppItemNodeCreator));
			StartupCode.Called = false;
		}
	}
}

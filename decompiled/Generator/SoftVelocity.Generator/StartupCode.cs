using System;
using System.Reflection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator;

internal class StartupCode : AbstractCommand
{
	internal static bool Called;

	public override void Run()
	{
		if (!Called)
		{
			_ = ApplicationService.Instance;
			Called = true;
			ResourceService.RegisterImages("SoftVelocity.Generator.Resources.Generator.BitmapResources", Assembly.GetExecutingAssembly());
			SolutionItemNode.SolutionItemCreating = (EventHandler<SolutionItemCreatingEventArgs>)Delegate.Combine(SolutionItemNode.SolutionItemCreating, new EventHandler<SolutionItemCreatingEventArgs>(AppItemNode.AppItemNodeCreator));
		}
	}
}

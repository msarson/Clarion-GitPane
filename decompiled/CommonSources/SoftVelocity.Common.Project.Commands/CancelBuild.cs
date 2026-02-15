using System;
using System.Reflection;
using System.Threading;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Common.Project.Commands;

public class CancelBuild : AbstractMenuCommand
{
	public override void Run()
	{
		try
		{
			ProjectService.CancelSemaphore.Release();
		}
		catch (SemaphoreFullException)
		{
		}
	}

	internal static void Startup()
	{
		MSBuildEngine.MSBuildProperties.Add("Signal", ProjectService.SemaphoreName);
		ResourceService.RegisterStrings("CommonSources.Resources.AbortCompile.StringResources", Assembly.GetExecutingAssembly());
		ProjectService.EndBuild += ProjectService_EndBuild;
	}

	private static void ProjectService_EndBuild(object sender, EventArgs e)
	{
		if (ProjectService.CancelSemaphore.WaitOne(1, exitContext: false))
		{
			StatusBarService.SetMessage(ResourceService.GetString("SoftVelocity.Common.Commands.BuildCanceled"));
		}
	}
}

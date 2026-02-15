using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator.UI;

namespace SoftVelocity.Generator.Commands;

internal class RefreshSortApplicationsPadAsyncMenuCommand : AbstractGenerationAsyncMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return ApplicationService.ApplicationsList.Count > 0;
		}
		set
		{
		}
	}

	public static void DoRun()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		if (ApplicationService.ApplicationsList.Count > 0)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(new Action(ApplicationBrowserPad.Instance.Applications.RefreshSort));
		}
	}

	public override void ExecuteApplicationService()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			ApplicationBrowserPad.Instance.Applications.RefreshSort();
		}
	}
}

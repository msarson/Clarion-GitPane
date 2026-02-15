using System.Runtime.InteropServices;
using ICSharpCode.Core;

namespace SoftVelocity.Generator.Commands;

public class ClearCachedABCFilesMenuCommand : AbstractMenuCommand, IConditionEvaluator
{
	public override bool IsEnabled
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Invalid comparison between Unknown and I4
			if ((int)VersionService.Version == 1)
			{
				return true;
			}
			return false;
		}
		set
		{
		}
	}

	[DllImport("clatpls.dll", CharSet = CharSet.Auto)]
	private static extern void GenClearCachedABCFiles();

	internal static void DoRun()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)VersionService.Version == 1)
		{
			GenClearCachedABCFiles();
		}
	}

	public override void Run()
	{
		DoRun();
	}

	public bool IsValid(object caller, Condition condition)
	{
		return ((AbstractMenuCommand)this).IsEnabled;
	}
}

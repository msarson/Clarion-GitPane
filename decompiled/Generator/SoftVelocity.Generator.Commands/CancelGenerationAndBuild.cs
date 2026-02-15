using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common.Project.Commands;
using SoftVelocity.Generator.Conditions;

namespace SoftVelocity.Generator.Commands;

internal class CancelGenerationAndBuild : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return IsGeneratingOrBuilding.IsValid();
		}
		set
		{
		}
	}

	internal static void DoRun()
	{
		if (IsGeneratingOrBuilding.IsValid())
		{
			AbstractCommand val = null;
			if (ProjectService.IsBuilding)
			{
				val = (AbstractCommand)(object)new CancelBuild();
			}
			else if (ApplicationService.IsGenerating)
			{
				val = (AbstractCommand)(object)new CancelGeneration();
			}
			val.Run();
		}
	}

	public override void Run()
	{
		DoRun();
	}
}

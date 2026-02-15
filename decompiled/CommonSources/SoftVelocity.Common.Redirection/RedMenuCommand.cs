using Clarion.Core.Redirection;
using ICSharpCode.Core;

namespace SoftVelocity.Common.Redirection;

internal abstract class RedMenuCommand : AbstractMenuCommand
{
	protected RedirectionFile redFile;

	protected abstract void DoRun();

	public override void Run()
	{
		redFile = CommonClarionProject.CurrentRedirectionFile(null);
		DoRun();
		redFile = null;
	}
}

using Microsoft.Build.Framework;

namespace ICSharpCode.SharpDevelop.Project;

public interface IMSBuildAdditionalLogger
{
	ILogger CreateLogger(MSBuildEngineWorker engineWorker);
}

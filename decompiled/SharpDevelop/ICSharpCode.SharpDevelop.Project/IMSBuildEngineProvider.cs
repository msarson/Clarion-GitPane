using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public interface IMSBuildEngineProvider
{
	Engine BuildEngine { get; }
}

using System;
using Clarion.Core.Options;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Common;

public static class ClarionSolutionService
{
	public static void StartBuild(object parent, EventArgs args)
	{
		BuildOptions val = (BuildOptions)((parent is BuildOptions) ? parent : null);
		if (val != null)
		{
			if (string.IsNullOrEmpty(Versions.GetActiveVersion(true)))
			{
				val.AdditionalProperties.Remove("clarion_version");
			}
			else
			{
				val.AdditionalProperties["clarion_version"] = Versions.GetActiveVersion(true);
			}
		}
	}
}

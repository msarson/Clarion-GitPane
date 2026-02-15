using System.Collections.Generic;
using Clarion.Core.Redirection;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Common.CommandLine;

internal class TraceFileViaRedirection : ICommandLine
{
	public bool Enabled => true;

	public void Run(List<List<string>> parameters, ICommandLineLogger logger, object redFile, bool forWindows, string version)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		RedirectionFile val = (RedirectionFile)redFile;
		string text = parameters[0][0];
		List<string> list = val.Trace(text, ".");
		if (list.Count == 0)
		{
			logger.Message("Could not trace");
			return;
		}
		foreach (string item in list)
		{
			logger.Message(item);
		}
	}
}

using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop;

public interface ICommandLine
{
	bool Enabled { get; }

	void Run(List<List<string>> parameters, ICommandLineLogger logger, object redFile, bool forWindows, string version);
}

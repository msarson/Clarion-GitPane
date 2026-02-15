using System.Collections;
using System.IO;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class DirectoryDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return Path.Combine(Path.GetDirectoryName(codon.AddIn.FileName), codon.Properties["path"]);
	}
}

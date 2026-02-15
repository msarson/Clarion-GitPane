using System.Collections;

namespace ICSharpCode.Core;

public class FileFilterDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return StringParser.Parse(codon.Properties["name"]) + "|" + codon.Properties["extensions"];
	}
}

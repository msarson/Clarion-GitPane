using System.Collections;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Codons;

public class SyntaxModeDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		string name = codon.Properties["name"];
		string[] extensions = codon.Properties["extensions"].Split(';');
		string fileName = codon.Properties["resource"];
		Runtime[] array = new Runtime[codon.AddIn.Runtimes.Count];
		int num = 0;
		foreach (Runtime runtime in codon.AddIn.Runtimes)
		{
			array[num++] = runtime;
		}
		return new AddInTreeSyntaxMode(array, fileName, name, extensions);
	}
}

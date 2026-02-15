using ICSharpCode.Core;

namespace SearchAndReplace;

public class SearchAndReplaceDescriptor
{
	private Codon codon;

	private AbstractSearchAndReplaceBinding binding;

	public AbstractSearchAndReplaceBinding Binding
	{
		get
		{
			if (binding == null)
			{
				binding = (AbstractSearchAndReplaceBinding)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			return binding;
		}
	}

	public SearchAndReplaceDescriptor(Codon codon)
	{
		this.codon = codon;
	}
}

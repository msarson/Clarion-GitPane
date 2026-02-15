namespace ICSharpCode.Core;

public class IconDescriptor
{
	private Codon codon;

	public string Id => codon.Id;

	public string Language => codon.Properties["language"];

	public string Resource => codon.Properties["resource"];

	public string[] Extensions => codon.Properties["extensions"].Split(';');

	public IconDescriptor(Codon codon)
	{
		this.codon = codon;
	}
}

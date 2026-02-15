using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class SchemeExtensionDescriptor
{
	private string schemeName;

	private Codon codon;

	private ISchemeExtension extension;

	public string SchemeName => schemeName;

	public ISchemeExtension Extension
	{
		get
		{
			if (extension == null)
			{
				extension = (ISchemeExtension)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			return extension;
		}
	}

	public SchemeExtensionDescriptor(Codon codon)
	{
		this.codon = codon;
		schemeName = codon.Properties["scheme"];
		if (schemeName == null || schemeName.Length == 0)
		{
			schemeName = codon.Id;
		}
	}
}

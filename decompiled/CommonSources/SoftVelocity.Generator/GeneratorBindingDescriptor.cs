using System;
using ICSharpCode.Core;

namespace SoftVelocity.Generator;

internal sealed class GeneratorBindingDescriptor
{
	private IGeneratorBinding binding;

	private Codon codon;

	public string Name => codon.Id;

	public IGeneratorBinding Binding
	{
		get
		{
			if (binding == null)
			{
				binding = (IGeneratorBinding)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			return binding;
		}
	}

	public GeneratorBindingDescriptor(Codon codon)
	{
		this.codon = codon;
	}

	public bool CanAttachToLanguage(string language)
	{
		string text = codon.Properties["language"];
		if (text == null || text.Length == 0)
		{
			return true;
		}
		return text.Equals(language, StringComparison.InvariantCultureIgnoreCase);
	}
}

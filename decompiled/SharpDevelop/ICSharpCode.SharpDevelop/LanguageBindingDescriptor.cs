using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class LanguageBindingDescriptor
{
	private ILanguageBinding binding;

	private Codon codon;

	private string[] codeFileExtensions;

	public ILanguageBinding Binding
	{
		get
		{
			if (binding == null)
			{
				binding = (ILanguageBinding)codon.AddIn.CreateObject(codon.Properties["class"]);
				if (binding != null && binding.Language != Language)
				{
					throw new InvalidOperationException("The Language property of the language binding must be equal to the id of the LanguageBinding codon!");
				}
			}
			return binding;
		}
	}

	public Codon Codon => codon;

	public string ProjectFileExtension => codon.Properties["projectfileextension"];

	public string Guid => codon.Properties["guid"];

	public string Language => codon.Id;

	public string[] CodeFileExtensions
	{
		get
		{
			if (codeFileExtensions == null)
			{
				if (codon.Properties["supportedextensions"].Length == 0)
				{
					codeFileExtensions = new string[0];
				}
				else
				{
					codeFileExtensions = codon.Properties["supportedextensions"].ToLowerInvariant().Split(';');
				}
			}
			return codeFileExtensions;
		}
	}

	public LanguageBindingDescriptor(Codon codon)
	{
		this.codon = codon;
	}
}

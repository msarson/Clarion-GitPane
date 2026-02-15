using System;
using ICSharpCode.Core;
using SoftVelocity.Generator.Editor;

namespace SoftVelocity.Generator;

internal sealed class GenEditorDescriptor
{
	private Codon codon;

	public string Name => codon.Id;

	public CommonGenEditor CreateEditor()
	{
		return (CommonGenEditor)codon.AddIn.CreateObject(codon.Properties["class"]);
	}

	public GenEditorDescriptor(Codon codon)
	{
		this.codon = codon;
	}

	public bool CanAttachToLanguage(string language)
	{
		string text = codon.Properties["language"];
		if (text == null || text.Length == 0)
		{
			return false;
		}
		return text.Equals(language, StringComparison.InvariantCultureIgnoreCase);
	}
}

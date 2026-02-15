using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.UI;

internal sealed class GenEditorBindingDescriptor
{
	private IViewContent binding;

	private Codon codon;

	public string Name => codon.Id;

	public string Language => codon.Properties["language"];

	public IViewContent Binding
	{
		get
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			if (binding == null)
			{
				binding = (IViewContent)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			return binding;
		}
	}

	public GenEditorBindingDescriptor(Codon codon)
	{
		this.codon = codon;
	}
}

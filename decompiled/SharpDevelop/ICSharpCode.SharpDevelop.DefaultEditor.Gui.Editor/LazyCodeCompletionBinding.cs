using System;
using System.IO;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public sealed class LazyCodeCompletionBinding : ICodeCompletionBinding
{
	private Codon codon;

	private string[] extensions;

	private ICodeCompletionBinding binding;

	public LazyCodeCompletionBinding(Codon codon, string[] extensions)
	{
		this.codon = codon;
		this.extensions = extensions;
	}

	public bool HandleKeyPress(SharpDevelopTextAreaControl editor, char ch)
	{
		string extension = Path.GetExtension(editor.FileName);
		string[] array = extensions;
		foreach (string value in array)
		{
			if (extension.Equals(value, StringComparison.InvariantCultureIgnoreCase))
			{
				if (binding == null)
				{
					binding = (ICodeCompletionBinding)codon.AddIn.CreateObject(codon.Properties["class"]);
				}
				return binding.HandleKeyPress(editor, ch);
			}
		}
		return false;
	}
}

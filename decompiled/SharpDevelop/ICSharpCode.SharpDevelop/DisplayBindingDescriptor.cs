using System.Text.RegularExpressions;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class DisplayBindingDescriptor
{
	private object binding;

	private Codon codon;

	private bool isSecondary;

	public IDisplayBinding Binding
	{
		get
		{
			if (binding == null)
			{
				binding = codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			return binding as IDisplayBinding;
		}
	}

	public ISecondaryDisplayBinding SecondaryBinding
	{
		get
		{
			if (binding == null)
			{
				binding = codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			return binding as ISecondaryDisplayBinding;
		}
	}

	public bool IsSecondary => isSecondary;

	public Codon Codon => codon;

	public DisplayBindingDescriptor(Codon codon)
	{
		isSecondary = codon.Properties["type"] == "Secondary";
		if (!isSecondary && codon.Properties["type"] != "" && codon.Properties["type"] != "Primary")
		{
			MessageService.ShowWarning("Unknown display binding type: " + codon.Properties["type"]);
		}
		this.codon = codon;
	}

	public bool CanAttachToFile(string fileName)
	{
		string text = codon.Properties["fileNamePattern"];
		if (text == null || text.Length == 0)
		{
			return true;
		}
		return Regex.IsMatch(fileName, text, RegexOptions.IgnoreCase);
	}

	public bool CanAttachToLanguage(string language)
	{
		string text = codon.Properties["languagePattern"];
		if (text == null || text.Length == 0)
		{
			return true;
		}
		return Regex.IsMatch(language, text, RegexOptions.IgnoreCase);
	}
}

using System;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop;

public sealed class ParserDescriptor
{
	private IParser parser;

	private string[] supportedExtensions;

	private Codon codon;

	public IParser Parser
	{
		get
		{
			if (parser == null)
			{
				parser = (IParser)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			return parser;
		}
	}

	public Codon Codon => codon;

	public string Language => codon.Id;

	public string ProjectFileExtension => codon.Properties["projectfileextension"];

	public string[] Supportedextensions
	{
		get
		{
			if (supportedExtensions == null)
			{
				supportedExtensions = codon.Properties["supportedextensions"].ToUpperInvariant().Split(';');
			}
			return supportedExtensions;
		}
	}

	public bool CanParse(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName))
		{
			string text = null;
			try
			{
				text = Path.GetExtension(fileName);
			}
			catch (ArgumentException ex)
			{
				MessageService.ShowError("Error with the name of the file to be parsed:" + fileName + "\r\nParamName:" + ex.ParamName + "\r\nPlease report this to SoftVelocity.");
				return false;
			}
			if (!string.IsNullOrEmpty(text))
			{
				text = text.ToUpperInvariant();
				string[] supportedextensions = Supportedextensions;
				foreach (string text2 in supportedextensions)
				{
					if (text == text2)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public ParserDescriptor(Codon codon)
	{
		this.codon = codon;
	}
}

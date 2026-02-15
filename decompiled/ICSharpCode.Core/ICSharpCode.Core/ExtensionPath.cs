using System.Collections.Generic;
using System.Xml;

namespace ICSharpCode.Core;

public class ExtensionPath
{
	private string name;

	private AddIn addIn;

	private List<Codon> codons = new List<Codon>();

	public AddIn AddIn => addIn;

	public string Name => name;

	public List<Codon> Codons => codons;

	public ExtensionPath(string name, AddIn addIn)
	{
		this.addIn = addIn;
		this.name = name;
	}

	public static void SetUp(ExtensionPath extensionPath, XmlReader reader, string endElement)
	{
		Stack<ICondition> stack = new Stack<ICondition>();
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.EndElement:
				if (reader.LocalName == "Condition" || reader.LocalName == "ComplexCondition")
				{
					stack.Pop();
				}
				else if (reader.LocalName == endElement)
				{
					return;
				}
				break;
			case XmlNodeType.Element:
			{
				string localName = reader.LocalName;
				if (localName == "Condition")
				{
					stack.Push(Condition.Read(reader));
					break;
				}
				if (localName == "ComplexCondition")
				{
					stack.Push(Condition.ReadComplexCondition(reader));
					break;
				}
				Codon codon = new Codon(extensionPath.AddIn, extensionPath.Name, localName, Properties.ReadFromAttributes(reader), stack.ToArray());
				extensionPath.codons.Add(codon);
				if (!reader.IsEmptyElement)
				{
					ExtensionPath extensionPath2 = extensionPath.AddIn.GetExtensionPath(extensionPath.Name + "/" + codon.Id);
					SetUp(extensionPath2, reader, localName);
				}
				break;
			}
			}
		}
	}
}

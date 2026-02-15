using System.Collections;
using System.Xml;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class TemplateType
{
	private string name;

	private Hashtable pairs = new Hashtable();

	public string Name => name;

	public Hashtable Pairs => pairs;

	public TemplateType(XmlElement enumType)
	{
		name = enumType.GetAttribute("name");
		foreach (XmlElement childNode in enumType.ChildNodes)
		{
			pairs[childNode.GetAttribute("name")] = childNode.GetAttribute("value");
		}
	}
}

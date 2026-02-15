using System.IO;
using System.Xml;

namespace ICSharpCode.Core;

public class XmlTextWriterFormattedNoDeclaration : XmlTextWriter
{
	public XmlTextWriterFormattedNoDeclaration(TextWriter w)
		: base(w)
	{
		base.Formatting = Formatting.Indented;
	}

	public override void WriteStartDocument()
	{
	}
}

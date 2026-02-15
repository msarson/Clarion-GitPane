using System.Drawing;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Generator.PWEE;

namespace SoftVelocity.Generator.Editor;

public class CustomPweeLine : CustomLine
{
	public IPweePart PweePart;

	public bool Dirty;

	public CustomPweeLine(IPweePart pweePart, int lineNr, Color customColor, bool readOnly)
		: base(lineNr, customColor, readOnly)
	{
		PweePart = pweePart;
	}

	public CustomPweeLine(IPweePart pweePart, int startLineNr, int endLineNr, Color customColor, bool readOnly)
		: base(startLineNr, endLineNr, customColor, readOnly)
	{
		PweePart = pweePart;
	}
}

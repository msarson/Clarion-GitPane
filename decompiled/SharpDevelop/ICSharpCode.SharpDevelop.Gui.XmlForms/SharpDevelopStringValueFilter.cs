using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public class SharpDevelopStringValueFilter : IStringValueFilter
{
	public string GetFilteredValue(string originalValue)
	{
		return StringParser.Parse(originalValue);
	}
}

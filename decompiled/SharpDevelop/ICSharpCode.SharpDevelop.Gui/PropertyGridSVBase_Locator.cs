using ICSharpCode.Core;
using VisualHint.SmartPropertyGrid;

namespace ICSharpCode.SharpDevelop.Gui;

public class PropertyGridSVBase_Locator : PropertyGridSVFilteredLocator
{
	public PropertyGridSVBase_Locator()
	{
		Font = FontService.GetFont(FontService.FontType.ListControls);
	}
}

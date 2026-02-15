using Aga.Controls.Tree;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class TreeViewAdvBase_Locator : Locator
{
	public TreeViewAdvBase_Locator()
	{
		Font = FontService.GetFont(FontService.FontType.ListControls);
	}
}

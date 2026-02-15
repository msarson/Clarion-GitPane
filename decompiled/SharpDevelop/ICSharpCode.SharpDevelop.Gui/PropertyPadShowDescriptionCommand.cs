using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class PropertyPadShowDescriptionCommand : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return PropertyPad.Grid.CommentsVisibility;
		}
		set
		{
			PropertyPad.Grid.CommentsVisibility = value;
		}
	}

	public override void Run()
	{
		PropertyPad.Grid.CommentsVisibility = !PropertyPad.Grid.CommentsVisibility;
	}
}

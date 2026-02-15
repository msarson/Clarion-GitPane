using System.Drawing;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public interface IListCustomColor
{
	Color Background { get; }

	Color Text { get; }

	Color BarActiveBackground { get; }

	Color BarActiveText { get; }

	Color BarInactiveBackground { get; }

	Color BarInactiveText { get; }
}

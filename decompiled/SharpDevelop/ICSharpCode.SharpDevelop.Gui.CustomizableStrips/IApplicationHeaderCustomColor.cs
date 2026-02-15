using System.Drawing;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public interface IApplicationHeaderCustomColor
{
	Color ApplicationHeaderGradientBegin { get; }

	Color ApplicationHeaderGradientEnd { get; }
}

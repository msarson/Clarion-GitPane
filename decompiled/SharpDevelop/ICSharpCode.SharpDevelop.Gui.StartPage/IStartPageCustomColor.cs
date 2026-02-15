using System.Drawing;

namespace ICSharpCode.SharpDevelop.Gui.StartPage;

public interface IStartPageCustomColor
{
	Color StartPageSecondaryColor { get; }

	Color StartPagePrimaryColor { get; }

	Color StartPageBackgroundGradientBegin { get; }

	Color StartPageBackgroundGradientEnd { get; }

	Color StartPageButtonImageColor { get; }

	Color StartPageGridHeaderColor { get; }

	Color StartPageGridBodyColor { get; }

	Color StartPageGridAltBodyColor { get; }

	Color StartPageGridLineColor { get; }

	Color StartPageGridHoverColor { get; }
}

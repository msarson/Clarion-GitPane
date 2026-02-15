using System;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public interface IAppearanceControl
{
	AppearanceProperties AppearanceProperties { get; }

	event EventHandler AppearanceChanged;

	void OnAppearanceChanged(EventArgs e);
}

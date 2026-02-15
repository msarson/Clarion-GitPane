using System;

namespace SoftVelocity.Generator.UI.Controls;

public class SelectedTabChangedEventArgs : EventArgs
{
	public readonly TabStripButton SelectedTab;

	public SelectedTabChangedEventArgs(TabStripButton tab)
	{
		SelectedTab = tab;
	}
}

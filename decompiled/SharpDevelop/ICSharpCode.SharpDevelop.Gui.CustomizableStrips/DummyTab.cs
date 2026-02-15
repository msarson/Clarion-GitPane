using System.ComponentModel;
using WeifenLuo.WinFormsUI;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public class DummyTab : DockContent
{
	public DummyTab(string tabName, DockState position)
		: this(tabName)
	{
		base.ShowHint = position;
	}

	public DummyTab(string tabName)
		: this()
	{
		base.ToolTipText = tabName;
		base.TabText = tabName;
		Text = tabName + " - " + tabName;
	}

	public DummyTab()
	{
		new ComponentResourceManager(typeof(AppearanceEditor));
		base.DockableAreas = DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockBottom;
		base.DockPadding.Top = 4;
		base.HideOnClose = true;
		base.ToolTipText = "Tab1";
		base.Name = "DummyTab";
	}
}

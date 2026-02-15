using System.ComponentModel;
using WeifenLuo.WinFormsUI;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public class DummyDoc : DockContent
{
	public DummyDoc(string fileName)
		: this()
	{
		base.ToolTipText = fileName;
		base.TabText = fileName;
		Text = fileName + " - " + fileName;
	}

	public DummyDoc()
	{
		new ComponentResourceManager(typeof(AppearanceEditor));
		base.DockableAreas = DockAreas.Document;
		base.HideOnClose = true;
		base.DockPadding.Top = 4;
		base.ToolTipText = "Doc1";
		base.Name = "DummyDoc";
	}
}

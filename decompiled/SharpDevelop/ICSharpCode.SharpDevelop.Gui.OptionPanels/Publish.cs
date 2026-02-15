namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class Publish : AbstractProjectOptionPanel
{
	public override void LoadPanelContents()
	{
		SetupFromXmlResource("ProjectOptions.Publish.xfrm");
		InitializeHelper();
	}
}

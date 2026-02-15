namespace ICSharpCode.SharpDevelop.Commands.TabStrip;

public class CloseFileTab : AbtractWorkbenchWindowMenuCommand
{
	public override void Run()
	{
		if (IsEnabled)
		{
			base.Window.CloseWindow(force: false);
		}
	}
}

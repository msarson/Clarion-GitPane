namespace ICSharpCode.SharpDevelop.Commands;

public abstract class AbtractNamedWorkbenchWindowMenuCommand : AbtractWorkbenchWindowMenuCommand
{
	public string FileName
	{
		get
		{
			if (IsEnabled)
			{
				return base.Window.ViewContent.FileName;
			}
			return string.Empty;
		}
	}

	public override bool IsEnabled
	{
		get
		{
			if (base.IsEnabled && base.Window.ViewContent != null && !string.IsNullOrEmpty(base.Window.ViewContent.FileName))
			{
				return true;
			}
			return false;
		}
	}
}

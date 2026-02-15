namespace ICSharpCode.SharpDevelop.Commands;

public class OpenRecentFile : AbstractRecentOpenCommand
{
	protected override bool DoOpen()
	{
		return FileService.OpenFile(base.FileDescription.FileName) != null;
	}
}

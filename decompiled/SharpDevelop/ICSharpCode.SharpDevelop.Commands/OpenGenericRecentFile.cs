namespace ICSharpCode.SharpDevelop.Commands;

public class OpenGenericRecentFile : AbstractRecentOpenCommand
{
	protected override bool DoOpen()
	{
		return FileService.OpenFile(base.FileDescription.FileName) != null;
	}

	protected override string GetExtension()
	{
		return "*.*";
	}
}

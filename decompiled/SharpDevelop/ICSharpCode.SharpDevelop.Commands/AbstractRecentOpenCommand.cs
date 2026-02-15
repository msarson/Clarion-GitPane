using System.IO;
using ICSharpCode.Core;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public abstract class AbstractRecentOpenCommand : AbstractCommand
{
	private RecentOpen.RecentOpenDescription desc;

	public RecentOpen.RecentOpenDescription FileDescription
	{
		get
		{
			return desc;
		}
		set
		{
			desc = value;
		}
	}

	protected abstract bool DoOpen();

	protected virtual string GetExtension()
	{
		return "*" + Path.GetExtension(FileDescription.FileName);
	}

	public override void Run()
	{
		if (FileDescription != null && DoOpen())
		{
			FileDialogService.SaveDirectory(GetExtension(), Path.GetDirectoryName(FileDescription.FileName));
		}
	}
}

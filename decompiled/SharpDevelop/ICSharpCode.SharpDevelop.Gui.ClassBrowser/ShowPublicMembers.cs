using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ShowPublicMembers : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowPublic) == ClassBrowserFilter.ShowPublic;
		}
		set
		{
			if (value)
			{
				ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowPublic;
			}
			else
			{
				ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowPublic;
			}
		}
	}
}

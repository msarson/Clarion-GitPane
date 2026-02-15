using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ShowProtectedMembers : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowProtected) == ClassBrowserFilter.ShowProtected;
		}
		set
		{
			if (value)
			{
				ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowProtected;
			}
			else
			{
				ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowProtected;
			}
		}
	}
}

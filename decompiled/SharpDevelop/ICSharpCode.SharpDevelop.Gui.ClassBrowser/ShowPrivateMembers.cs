using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ShowPrivateMembers : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowPrivate) == ClassBrowserFilter.ShowPrivate;
		}
		set
		{
			if (value)
			{
				ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowPrivate;
			}
			else
			{
				ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowPrivate;
			}
		}
	}
}

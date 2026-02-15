using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ShowProjectReferences : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowProjectReferences) == ClassBrowserFilter.ShowProjectReferences;
		}
		set
		{
			if (value)
			{
				ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowProjectReferences;
			}
			else
			{
				ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowProjectReferences;
			}
		}
	}
}

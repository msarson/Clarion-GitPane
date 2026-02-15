using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ShowBaseAndDerivedTypes : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowBaseAndDerivedTypes) == ClassBrowserFilter.ShowBaseAndDerivedTypes;
		}
		set
		{
			if (value)
			{
				ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowBaseAndDerivedTypes;
			}
			else
			{
				ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowBaseAndDerivedTypes;
			}
		}
	}
}

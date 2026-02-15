using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;

namespace SoftVelocity.Common.ClassBrowser.Commands;

internal class ShowIncludedEntities : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Invalid comparison between Unknown and I4
			return (ClassBrowserPad.Instance.Filter & 0x40) == 64;
		}
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (value)
			{
				ClassBrowserPad instance = ClassBrowserPad.Instance;
				instance.Filter = (ClassBrowserFilter)(instance.Filter | 0x40);
			}
			else
			{
				ClassBrowserPad instance2 = ClassBrowserPad.Instance;
				instance2.Filter = (ClassBrowserFilter)(instance2.Filter & -65);
			}
		}
	}
}

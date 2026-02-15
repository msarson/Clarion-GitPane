using System;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

[Obsolete("FormLocationHelper is deprecated, please use FormPositionService.Instance instead.")]
public static class FormLocationHelper
{
	[Obsolete("FormLocationHelper.Apply is deprecated, please use FormPositionService.Instance.AutoApply instead.")]
	public static void Apply(Form form, string propertyName, bool isResizable)
	{
		FormPositionService.Instance.Apply(form, propertyName);
	}
}

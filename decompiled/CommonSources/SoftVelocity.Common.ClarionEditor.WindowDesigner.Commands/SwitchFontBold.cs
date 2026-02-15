using System;
using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class SwitchFontBold : DesignerAbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			try
			{
				if (base.View != null)
				{
					return base.View.GetIsFontBoldValue();
				}
			}
			catch (Exception)
			{
			}
			return false;
		}
		set
		{
			try
			{
				if (base.View != null)
				{
					base.View.SetIsFontBoldValue(value);
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
	}
}

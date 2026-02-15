using System;
using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class SwitchUnderlineBold : DesignerAbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			try
			{
				if (base.View != null)
				{
					return base.View.GetIsFontUnderlineValue();
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
					base.View.SetIsFontUnderlineValue(value);
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
	}
}

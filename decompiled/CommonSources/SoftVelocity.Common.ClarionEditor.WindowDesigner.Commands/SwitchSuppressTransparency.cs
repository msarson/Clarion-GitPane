using System;
using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class SwitchSuppressTransparency : DesignerAbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			try
			{
				if (base.View != null)
				{
					return base.View.IsSuppressTransparency;
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
					base.View.IsSuppressTransparency = value;
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
	}
}

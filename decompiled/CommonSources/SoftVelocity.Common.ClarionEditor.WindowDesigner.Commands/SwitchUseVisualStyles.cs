using System;
using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class SwitchUseVisualStyles : DesignerAbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			try
			{
				if (base.View != null)
				{
					return base.View.IsUseVisualStyles;
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
					base.View.IsUseVisualStyles = value;
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
	}
}

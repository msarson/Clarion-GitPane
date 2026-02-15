using System;
using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class SwitchItalicBold : DesignerAbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			try
			{
				if (base.View != null)
				{
					return base.View.GetIsFontItalicValue();
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
					base.View.SetIsFontItalicValue(value);
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
	}
}

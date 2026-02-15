using System;
using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class TextValueTextBox : AbstractWindowDesignerTextBoxCommand
{
	public override bool RefreshText()
	{
		try
		{
			if (base.View != null)
			{
				textBox.Text = base.View.GetTextValue();
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
		return true;
	}

	public override void Run()
	{
		try
		{
			if (base.View != null)
			{
				base.View.SetTextValue(textBox.Text);
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}
}

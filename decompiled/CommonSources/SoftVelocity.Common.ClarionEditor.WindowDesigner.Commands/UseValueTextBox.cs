using System;
using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class UseValueTextBox : AbstractWindowDesignerTextBoxCommand
{
	public override bool RefreshText()
	{
		try
		{
			if (base.View != null)
			{
				textBox.Text = base.View.GetUSEValue();
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
				if (!string.IsNullOrEmpty(textBox.Text))
				{
					base.View.SetUSEValue(textBox.Text);
				}
				textBox.Text = base.View.GetUSEValue();
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}
}

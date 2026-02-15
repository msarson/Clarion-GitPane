using System;
using System.Drawing;
using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class FontNameComboBox : AbstractWindowDesignerComboBoxCommand
{
	public override bool RefreshText()
	{
		try
		{
			if (base.View != null)
			{
				comboBox.Text = base.View.GetFontNameValue();
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
		return true;
	}

	protected override bool FillInComboBox()
	{
		FontFamily[] families = FontFamily.Families;
		FontFamily[] array = families;
		foreach (FontFamily fontFamily in array)
		{
			comboBox.Items.Add(fontFamily.Name);
		}
		return true;
	}

	public override void Run()
	{
		try
		{
			if (base.View != null)
			{
				base.View.SetFontNameValue(comboBox.Text);
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}
}

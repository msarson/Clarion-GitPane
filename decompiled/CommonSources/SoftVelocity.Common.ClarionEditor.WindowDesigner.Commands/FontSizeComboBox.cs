using System;
using ICSharpCode.Core;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class FontSizeComboBox : AbstractWindowDesignerComboBoxCommand
{
	public override bool RefreshText()
	{
		try
		{
			if (base.View != null)
			{
				comboBox.Text = base.View.GetFontSizeValue();
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
		return true;
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		comboBox.Width /= 5;
	}

	protected override bool FillInComboBox()
	{
		comboBox.Items.AddRange(new string[16]
		{
			"8", "9", "10", "11", "12", "14", "16", "18", "20", "22",
			"24", "26", "28", "36", "48", "72"
		});
		return true;
	}

	public override void Run()
	{
		int fontSizeValue = 8;
		try
		{
			fontSizeValue = int.Parse(comboBox.Text);
		}
		catch (Exception)
		{
		}
		if (base.View != null)
		{
			base.View.SetFontSizeValue(fontSizeValue);
		}
	}
}

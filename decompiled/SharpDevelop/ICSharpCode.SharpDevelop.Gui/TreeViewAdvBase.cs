using System;
using System.Drawing;
using System.Windows.Forms;
using Aga.Controls.Tree;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

namespace ICSharpCode.SharpDevelop.Gui;

public class TreeViewAdvBase : TreeViewAdv
{
	public TreeViewAdvBase()
	{
		Font = FontService.GetFont(FontService.FontType.ListControls);
		base.RowHeight = Font.Height + 4;
		base.ShowPlusMinusTriangle = true;
		base.ShowLines = false;
		base.ThemedBar = false;
		SeCustomColors();
	}

	protected override void OnLoad(EventArgs e)
	{
		SeCustomColors();
		base.OnLoad(e);
	}

	public void SeCustomColors()
	{
		SetColors(this);
	}

	public void SetColors(IListCustomColor colors)
	{
		SetColors(colors.Background, colors.Text, colors.BarActiveBackground, colors.BarActiveText, colors.BarInactiveBackground, colors.BarInactiveText);
	}

	public void SetColorsSystemColor()
	{
		SetColors(SystemColors.Window, SystemColors.WindowText, SystemColors.Highlight, SystemColors.HighlightText, SystemColors.InactiveCaption, SystemColors.InactiveCaptionText);
	}

	public static void SetColors(TreeViewAdv list)
	{
		if (ToolStripManager.Renderer is ToolStripProfessionalRenderer)
		{
			ToolStripProfessionalRenderer toolStripProfessionalRenderer = ToolStripManager.Renderer as ToolStripProfessionalRenderer;
			if (toolStripProfessionalRenderer.ColorTable is IListCustomColor)
			{
				IListCustomColor listCustomColor = (IListCustomColor)toolStripProfessionalRenderer.ColorTable;
				list.SetColors(listCustomColor.Background, listCustomColor.Text, listCustomColor.BarActiveBackground, listCustomColor.BarActiveText, listCustomColor.BarInactiveBackground, listCustomColor.BarInactiveText);
			}
			else
			{
				list.SetColors(SystemColors.Window, SystemColors.WindowText, SystemColors.Highlight, SystemColors.HighlightText, SystemColors.InactiveCaption, SystemColors.InactiveCaptionText);
			}
		}
		else
		{
			list.SetColors(SystemColors.Window, SystemColors.WindowText, SystemColors.Highlight, SystemColors.HighlightText, SystemColors.InactiveCaption, SystemColors.InactiveCaptionText);
		}
	}
}

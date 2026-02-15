using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

namespace SoftVelocity.Controls;

public class GradientLabel : Label
{
	private Color _BackColorGradientEnd = SystemColors.Control;

	private Color _BackColorGradientBegin = SystemColors.Window;

	private LinearGradientMode _GradientMode;

	private bool _UseProfessionalColorTable;

	private bool _UseAutomaticTextColor = true;

	public Color BackColorGradientEnd
	{
		get
		{
			return _BackColorGradientEnd;
		}
		set
		{
			_BackColorGradientEnd = value;
		}
	}

	public Color BackColorGradientBegin
	{
		get
		{
			return _BackColorGradientBegin;
		}
		set
		{
			_BackColorGradientBegin = value;
		}
	}

	public LinearGradientMode GradientMode
	{
		get
		{
			return _GradientMode;
		}
		set
		{
			_GradientMode = value;
		}
	}

	public bool UseProfessionalColorTable
	{
		get
		{
			return _UseProfessionalColorTable;
		}
		set
		{
			_UseProfessionalColorTable = value;
		}
	}

	public bool UseAutomaticTextColor
	{
		get
		{
			return _UseAutomaticTextColor;
		}
		set
		{
			_UseAutomaticTextColor = value;
		}
	}

	public GradientLabel(int fontSize)
		: this()
	{
		Font = new Font("Verdana", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
	}

	public GradientLabel()
	{
		base.ResizeRedraw = true;
		Text = string.Empty;
	}

	protected override void OnPaintBackground(PaintEventArgs pe)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Color color = BackColorGradientEnd;
		Color color2 = BackColorGradientBegin;
		if (UseProfessionalColorTable && ToolStripManager.Renderer is ToolStripProfessionalRenderer)
		{
			ProfessionalColorTable colorTable = ((ToolStripProfessionalRenderer)ToolStripManager.Renderer).ColorTable;
			if (colorTable is IApplicationHeaderCustomColor)
			{
				color2 = ((IApplicationHeaderCustomColor)colorTable).ApplicationHeaderGradientBegin;
				color = ((IApplicationHeaderCustomColor)colorTable).ApplicationHeaderGradientEnd;
			}
			else
			{
				color2 = colorTable.MenuItemPressedGradientBegin;
				color = colorTable.MenuItemPressedGradientEnd;
			}
			if (UseAutomaticTextColor)
			{
				if ((double)color.GetBrightness() < 0.48 && (double)color2.GetBrightness() < 0.48)
				{
					ForeColor = Color.WhiteSmoke;
				}
				else
				{
					ForeColor = SystemColors.ControlText;
				}
			}
		}
		base.OnPaintBackground(pe);
		Graphics graphics = pe.Graphics;
		using LinearGradientBrush brush = new LinearGradientBrush(base.ClientRectangle, color2, color, GradientMode);
		graphics.FillRectangle(brush, new Rectangle(0, 0, base.Width, base.Height));
	}
}

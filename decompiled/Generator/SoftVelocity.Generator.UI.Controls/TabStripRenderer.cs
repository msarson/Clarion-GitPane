using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SoftVelocity.Generator.UI.Controls;

internal class TabStripRenderer : ToolStripRenderer
{
	private const int selOffset = 2;

	private ToolStripRenderer currentRenderer;

	private ToolStripRenderMode renderMode;

	private bool mirrored;

	private bool useVS = System.Windows.Forms.Application.RenderWithVisualStyles;

	public ToolStripRenderMode RenderMode
	{
		get
		{
			return renderMode;
		}
		set
		{
			renderMode = value;
			switch (renderMode)
			{
			case ToolStripRenderMode.Professional:
				currentRenderer = new ToolStripProfessionalRenderer();
				break;
			case ToolStripRenderMode.System:
				currentRenderer = new ToolStripSystemRenderer();
				break;
			default:
				currentRenderer = null;
				break;
			}
		}
	}

	public bool Mirrored
	{
		get
		{
			return mirrored;
		}
		set
		{
			mirrored = value;
		}
	}

	public bool UseVS
	{
		get
		{
			return useVS;
		}
		set
		{
			if (!value || System.Windows.Forms.Application.RenderWithVisualStyles)
			{
				useVS = value;
			}
		}
	}

	protected override void Initialize(ToolStrip ts)
	{
		base.Initialize(ts);
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		Color color = SystemColors.AppWorkspace;
		if (UseVS)
		{
			VisualStyleRenderer visualStyleRenderer = new VisualStyleRenderer(VisualStyleElement.Tab.Pane.Normal);
			color = visualStyleRenderer.GetColor(ColorProperty.BorderColorHint);
		}
		using Pen pen = new Pen(color);
		using Pen pen2 = new Pen(e.BackColor);
		Rectangle bounds = e.ToolStrip.Bounds;
		int num = ((!Mirrored) ? (bounds.Width - 1 - e.ToolStrip.Padding.Horizontal) : 0);
		int num2 = ((!Mirrored) ? (bounds.Height - 1) : 0);
		if (e.ToolStrip.Orientation == Orientation.Horizontal)
		{
			e.Graphics.DrawLine(pen, 0, num2, bounds.Width, num2);
		}
		else
		{
			e.Graphics.DrawLine(pen, num, 0, num, bounds.Height);
			if (!Mirrored)
			{
				for (int i = num + 1; i < bounds.Width; i++)
				{
					e.Graphics.DrawLine(pen2, i, 0, i, bounds.Height);
				}
			}
		}
		foreach (ToolStripItem item in e.ToolStrip.Items)
		{
			if (item.IsOnOverflow || !(item is TabStripButton { Bounds: var bounds2 } tabStripButton))
			{
				continue;
			}
			int num3 = (Mirrored ? bounds2.Left : bounds2.Right);
			int num4 = (Mirrored ? bounds2.Top : (bounds2.Bottom - 1));
			int num5 = ((!Mirrored) ? 1 : 0);
			if (e.ToolStrip.Orientation == Orientation.Horizontal)
			{
				e.Graphics.DrawLine(pen, bounds2.Left, num4, bounds2.Right, num4);
				if (tabStripButton.Checked)
				{
					e.Graphics.DrawLine(pen2, bounds2.Left + 2 - num5, num4, bounds2.Right - 2 - num5, num4);
				}
			}
			else
			{
				e.Graphics.DrawLine(pen, num3, bounds2.Top, num3, bounds2.Bottom);
				if (tabStripButton.Checked)
				{
					e.Graphics.DrawLine(pen2, num3, bounds2.Top + 2 - num5, num3, bounds2.Bottom - 2 - num5);
				}
			}
		}
	}

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawToolStripBackground(e);
		}
		else
		{
			base.OnRenderToolStripBackground(e);
		}
	}

	protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
	{
		Graphics graphics = e.Graphics;
		TabStrip tabStrip = e.ToolStrip as TabStrip;
		TabStripButton tabStripButton = e.Item as TabStripButton;
		if (tabStrip == null || tabStripButton == null)
		{
			if (currentRenderer != null)
			{
				currentRenderer.DrawButtonBackground(e);
			}
			else
			{
				base.OnRenderButtonBackground(e);
			}
			return;
		}
		bool flag = tabStripButton.Checked;
		bool selected = tabStripButton.Selected;
		int num = 0;
		int num2 = 0;
		int num3 = tabStripButton.Bounds.Width - 1;
		int num4 = tabStripButton.Bounds.Height - 1;
		Rectangle bounds;
		if (UseVS)
		{
			if (tabStrip.Orientation == Orientation.Horizontal)
			{
				if (!flag)
				{
					num = 2;
					num4--;
				}
				else
				{
					num = 1;
				}
				bounds = new Rectangle(0, 0, num3, num4);
			}
			else
			{
				if (!flag)
				{
					num2 = 2;
					num3--;
				}
				else
				{
					num2 = 1;
				}
				bounds = new Rectangle(0, 0, num4, num3);
			}
			using Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
			VisualStyleElement element = VisualStyleElement.Tab.TabItem.Normal;
			if (flag)
			{
				element = VisualStyleElement.Tab.TabItem.Pressed;
			}
			if (selected)
			{
				element = VisualStyleElement.Tab.TabItem.Hot;
			}
			if (!tabStripButton.Enabled)
			{
				element = VisualStyleElement.Tab.TabItem.Disabled;
			}
			if (!flag || selected)
			{
				bounds.Width++;
			}
			else
			{
				bounds.Height++;
			}
			using Graphics dc = Graphics.FromImage(bitmap);
			VisualStyleRenderer visualStyleRenderer = new VisualStyleRenderer(element);
			visualStyleRenderer.DrawBackground(dc, bounds);
			if (tabStrip.Orientation == Orientation.Vertical)
			{
				if (Mirrored)
				{
					bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
				}
				else
				{
					bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
				}
			}
			else if (Mirrored)
			{
				bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
			}
			if (Mirrored)
			{
				num2 = tabStripButton.Bounds.Width - bitmap.Width - num2;
				num = tabStripButton.Bounds.Height - bitmap.Height - num;
			}
			graphics.DrawImage(bitmap, num2, num);
			return;
		}
		if (tabStrip.Orientation == Orientation.Horizontal)
		{
			if (!flag)
			{
				num = 2;
				num4--;
			}
			else
			{
				num = 1;
			}
			if (Mirrored)
			{
				num2 = 1;
				num = 0;
			}
			else
			{
				num++;
			}
			num3--;
		}
		else
		{
			if (!flag)
			{
				num2 = 2;
				num3--;
			}
			else
			{
				num2 = 1;
			}
			if (Mirrored)
			{
				num2 = 0;
				num = 1;
			}
		}
		num4--;
		bounds = new Rectangle(num2, num, num3, num4);
		using GraphicsPath graphicsPath = new GraphicsPath();
		if (Mirrored && tabStrip.Orientation == Orientation.Horizontal)
		{
			graphicsPath.AddLine(bounds.Left, bounds.Top, bounds.Left, bounds.Bottom - 2);
			graphicsPath.AddArc(bounds.Left, bounds.Bottom - 3, 2, 2, 90f, 90f);
			graphicsPath.AddLine(bounds.Left + 2, bounds.Bottom, bounds.Right - 2, bounds.Bottom);
			graphicsPath.AddArc(bounds.Right - 2, bounds.Bottom - 3, 2, 2, 0f, 90f);
			graphicsPath.AddLine(bounds.Right, bounds.Bottom - 2, bounds.Right, bounds.Top);
		}
		else if (!Mirrored && tabStrip.Orientation == Orientation.Horizontal)
		{
			graphicsPath.AddLine(bounds.Left, bounds.Bottom, bounds.Left, bounds.Top + 2);
			graphicsPath.AddArc(bounds.Left, bounds.Top + 1, 2, 2, 180f, 90f);
			graphicsPath.AddLine(bounds.Left + 2, bounds.Top, bounds.Right - 2, bounds.Top);
			graphicsPath.AddArc(bounds.Right - 2, bounds.Top + 1, 2, 2, 270f, 90f);
			graphicsPath.AddLine(bounds.Right, bounds.Top + 2, bounds.Right, bounds.Bottom);
		}
		else if (Mirrored && tabStrip.Orientation == Orientation.Vertical)
		{
			graphicsPath.AddLine(bounds.Left, bounds.Top, bounds.Right - 2, bounds.Top);
			graphicsPath.AddArc(bounds.Right - 2, bounds.Top + 1, 2, 2, 270f, 90f);
			graphicsPath.AddLine(bounds.Right, bounds.Top + 2, bounds.Right, bounds.Bottom - 2);
			graphicsPath.AddArc(bounds.Right - 2, bounds.Bottom - 3, 2, 2, 0f, 90f);
			graphicsPath.AddLine(bounds.Right - 2, bounds.Bottom, bounds.Left, bounds.Bottom);
		}
		else
		{
			graphicsPath.AddLine(bounds.Right, bounds.Top, bounds.Left + 2, bounds.Top);
			graphicsPath.AddArc(bounds.Left, bounds.Top + 1, 2, 2, 180f, 90f);
			graphicsPath.AddLine(bounds.Left, bounds.Top + 2, bounds.Left, bounds.Bottom - 2);
			graphicsPath.AddArc(bounds.Left, bounds.Bottom - 3, 2, 2, 90f, 90f);
			graphicsPath.AddLine(bounds.Left + 2, bounds.Bottom, bounds.Right, bounds.Bottom);
		}
		if (flag || selected)
		{
			Color color = (selected ? Color.WhiteSmoke : Color.White);
			if (renderMode == ToolStripRenderMode.Professional)
			{
				color = (selected ? ProfessionalColors.ButtonCheckedGradientBegin : ProfessionalColors.ButtonCheckedGradientEnd);
				using LinearGradientBrush brush = new LinearGradientBrush(tabStripButton.ContentRectangle, color, ProfessionalColors.ButtonCheckedGradientMiddle, LinearGradientMode.Vertical);
				graphics.FillPath(brush, graphicsPath);
			}
			else
			{
				using SolidBrush brush2 = new SolidBrush(color);
				graphics.FillPath(brush2, graphicsPath);
			}
		}
		using Pen pen = new Pen(flag ? ControlPaint.Dark(SystemColors.AppWorkspace) : SystemColors.AppWorkspace);
		graphics.DrawPath(pen, graphicsPath);
	}

	protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
	{
		Rectangle imageRectangle = e.ImageRectangle;
		if (e.Item is TabStripButton tabStripButton)
		{
			int num = ((!Mirrored) ? 1 : (-1)) * (tabStripButton.Checked ? 1 : 2);
			if (e.ToolStrip.Orientation == Orientation.Horizontal)
			{
				imageRectangle.Offset((!Mirrored) ? 1 : 2, num + (Mirrored ? 1 : 0));
			}
			else
			{
				imageRectangle.Offset(num + 2, 0);
			}
		}
		ToolStripItemImageRenderEventArgs e2 = new ToolStripItemImageRenderEventArgs(e.Graphics, e.Item, e.Image, imageRectangle);
		if (currentRenderer != null)
		{
			currentRenderer.DrawItemImage(e2);
		}
		else
		{
			base.OnRenderItemImage(e2);
		}
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		Rectangle textRectangle = e.TextRectangle;
		TabStripButton tabStripButton = e.Item as TabStripButton;
		Color textColor = e.TextColor;
		Font textFont = e.TextFont;
		if (tabStripButton != null)
		{
			int num = ((!Mirrored) ? 1 : (-1)) * (tabStripButton.Checked ? 1 : 2);
			if (e.ToolStrip.Orientation == Orientation.Horizontal)
			{
				textRectangle.Offset((!Mirrored) ? 1 : 2, num + (Mirrored ? 1 : (-1)));
			}
			else
			{
				textRectangle.Offset(num + 2, 0);
			}
			if (tabStripButton.Selected)
			{
				textColor = tabStripButton.HotTextColor;
			}
			else if (tabStripButton.Checked)
			{
				textColor = tabStripButton.SelectedTextColor;
			}
			if (tabStripButton.Checked)
			{
				textFont = tabStripButton.SelectedFont;
			}
		}
		ToolStripItemTextRenderEventArgs e2 = new ToolStripItemTextRenderEventArgs(e.Graphics, e.Item, e.Text, textRectangle, textColor, textFont, e.TextFormat);
		e2.TextDirection = e.TextDirection;
		if (currentRenderer != null)
		{
			currentRenderer.DrawItemText(e2);
		}
		else
		{
			base.OnRenderItemText(e2);
		}
	}

	protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawArrow(e);
		}
		else
		{
			base.OnRenderArrow(e);
		}
	}

	protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawDropDownButtonBackground(e);
		}
		else
		{
			base.OnRenderDropDownButtonBackground(e);
		}
	}

	protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawGrip(e);
		}
		else
		{
			base.OnRenderGrip(e);
		}
	}

	protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawImageMargin(e);
		}
		else
		{
			base.OnRenderImageMargin(e);
		}
	}

	protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawItemBackground(e);
		}
		else
		{
			base.OnRenderItemBackground(e);
		}
	}

	protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawItemCheck(e);
		}
		else
		{
			base.OnRenderItemCheck(e);
		}
	}

	protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawLabelBackground(e);
		}
		else
		{
			base.OnRenderLabelBackground(e);
		}
	}

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawMenuItemBackground(e);
		}
		else
		{
			base.OnRenderMenuItemBackground(e);
		}
	}

	protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawOverflowButtonBackground(e);
		}
		else
		{
			base.OnRenderOverflowButtonBackground(e);
		}
	}

	protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawSeparator(e);
		}
		else
		{
			base.OnRenderSeparator(e);
		}
	}

	protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawSplitButton(e);
		}
		else
		{
			base.OnRenderSplitButtonBackground(e);
		}
	}

	protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawStatusStripSizingGrip(e);
		}
		else
		{
			base.OnRenderStatusStripSizingGrip(e);
		}
	}

	protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawToolStripContentPanelBackground(e);
		}
		else
		{
			base.OnRenderToolStripContentPanelBackground(e);
		}
	}

	protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawToolStripPanelBackground(e);
		}
		else
		{
			base.OnRenderToolStripPanelBackground(e);
		}
	}

	protected override void OnRenderToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e)
	{
		if (currentRenderer != null)
		{
			currentRenderer.DrawToolStripStatusLabelBackground(e);
		}
		else
		{
			base.OnRenderToolStripStatusLabelBackground(e);
		}
	}
}

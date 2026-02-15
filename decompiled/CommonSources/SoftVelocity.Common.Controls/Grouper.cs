using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SoftVelocity.Common.Controls;

[Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
public class Grouper : UserControl
{
	public class GroupBoxConstants
	{
		public const int SweepAngle = 90;

		public const int MinControlHeight = 32;

		public const int MinControlWidth = 96;
	}

	public enum GroupBoxGradientMode
	{
		None = 4,
		BackwardDiagonal = 3,
		ForwardDiagonal = 2,
		Horizontal = 0,
		Vertical = 1
	}

	private Container components;

	private int V_RoundCorners = 10;

	private string V_GroupTitle = "The Grouper";

	private Color V_BorderColor = Color.Black;

	private float V_BorderThickness = 1f;

	private bool V_ShadowControl;

	private Color V_BackgroundColor = Color.White;

	private Color V_BackgroundGradientColor = Color.White;

	private GroupBoxGradientMode V_BackgroundGradientMode = GroupBoxGradientMode.None;

	private Color V_ShadowColor = Color.DarkGray;

	private int V_ShadowThickness = 3;

	private Image V_GroupImage;

	private Color V_CustomGroupBoxColor = Color.White;

	private bool V_PaintGroupBox;

	private Color V_BackColor = Color.Transparent;

	[Category("Appearance")]
	[Description("This feature will paint the background color of the control.")]
	public override Color BackColor
	{
		get
		{
			return V_BackColor;
		}
		set
		{
			V_BackColor = value;
			Refresh();
		}
	}

	[Description("This feature will paint the group title background to the specified color if PaintGroupBox is set to true.")]
	[Category("Appearance")]
	public Color CustomGroupBoxColor
	{
		get
		{
			return V_CustomGroupBoxColor;
		}
		set
		{
			V_CustomGroupBoxColor = value;
			Refresh();
		}
	}

	[Category("Appearance")]
	[Description("This feature will paint the group title background to the CustomGroupBoxColor.")]
	public bool PaintGroupBox
	{
		get
		{
			return V_PaintGroupBox;
		}
		set
		{
			V_PaintGroupBox = value;
			Refresh();
		}
	}

	[Description("This feature can add a 16 x 16 image to the group title bar.")]
	[Category("Appearance")]
	public Image GroupImage
	{
		get
		{
			return V_GroupImage;
		}
		set
		{
			V_GroupImage = value;
			Refresh();
		}
	}

	[Description("This feature will change the control's shadow color.")]
	[Category("Appearance")]
	public Color ShadowColor
	{
		get
		{
			return V_ShadowColor;
		}
		set
		{
			V_ShadowColor = value;
			Refresh();
		}
	}

	[Category("Appearance")]
	[Description("This feature will change the size of the shadow border.")]
	public int ShadowThickness
	{
		get
		{
			return V_ShadowThickness;
		}
		set
		{
			if (value > 10)
			{
				V_ShadowThickness = 10;
			}
			else if (value < 1)
			{
				V_ShadowThickness = 1;
			}
			else
			{
				V_ShadowThickness = value;
			}
			Refresh();
		}
	}

	[Category("Appearance")]
	[Description("This feature will change the group control color. This color can also be used in combination with BackgroundGradientColor for a gradient paint.")]
	public Color BackgroundColor
	{
		get
		{
			return V_BackgroundColor;
		}
		set
		{
			V_BackgroundColor = value;
			Refresh();
		}
	}

	[Category("Appearance")]
	[Description("This feature can be used in combination with BackgroundColor to create a gradient background.")]
	public Color BackgroundGradientColor
	{
		get
		{
			return V_BackgroundGradientColor;
		}
		set
		{
			V_BackgroundGradientColor = value;
			Refresh();
		}
	}

	[Category("Appearance")]
	[Description("This feature turns on background gradient painting.")]
	public GroupBoxGradientMode BackgroundGradientMode
	{
		get
		{
			return V_BackgroundGradientMode;
		}
		set
		{
			V_BackgroundGradientMode = value;
			Refresh();
		}
	}

	[Description("This feature will round the corners of the control.")]
	[Category("Appearance")]
	public int RoundCorners
	{
		get
		{
			return V_RoundCorners;
		}
		set
		{
			if (value > 25)
			{
				V_RoundCorners = 25;
			}
			else if (value < 1)
			{
				V_RoundCorners = 1;
			}
			else
			{
				V_RoundCorners = value;
			}
			Refresh();
		}
	}

	[Description("This feature will add a group title to the control.")]
	[Category("Appearance")]
	public string GroupTitle
	{
		get
		{
			return V_GroupTitle;
		}
		set
		{
			V_GroupTitle = value;
			Refresh();
		}
	}

	[Category("Appearance")]
	[Description("This feature will allow you to change the color of the control's border.")]
	public Color BorderColor
	{
		get
		{
			return V_BorderColor;
		}
		set
		{
			V_BorderColor = value;
			Refresh();
		}
	}

	[Description("This feature will allow you to set the control's border size.")]
	[Category("Appearance")]
	public float BorderThickness
	{
		get
		{
			return V_BorderThickness;
		}
		set
		{
			if (value > 3f)
			{
				V_BorderThickness = 3f;
			}
			else if (value < 1f)
			{
				V_BorderThickness = 1f;
			}
			else
			{
				V_BorderThickness = value;
			}
			Refresh();
		}
	}

	[Description("This feature will allow you to turn on control shadowing.")]
	[Category("Appearance")]
	public bool ShadowControl
	{
		get
		{
			return V_ShadowControl;
		}
		set
		{
			V_ShadowControl = value;
			Refresh();
		}
	}

	public Grouper()
	{
		InitializeStyles();
		InitializeGroupBox();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeStyles()
	{
		SetStyle(ControlStyles.DoubleBuffer, value: true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
	}

	private void InitializeGroupBox()
	{
		components = new Container();
		base.Resize += GroupBox_Resize;
		base.DockPadding.All = 20;
		base.Name = "GroupBox";
		base.Size = new Size(368, 288);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PaintBack(e.Graphics);
		PaintGroupText(e.Graphics);
	}

	private void PaintGroupText(Graphics g)
	{
		if (!(GroupTitle == string.Empty))
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			Size size = g.MeasureString(GroupTitle, Font).ToSize();
			size = new Size(base.Size.Width - 14, size.Height);
			int roundCorners = RoundCorners;
			int roundCorners2 = RoundCorners;
			int num = 0;
			int num2 = size.Width + 14 - (roundCorners + 1);
			int num3 = 0;
			int num4 = 24 - (roundCorners2 + 1);
			GraphicsPath graphicsPath = new GraphicsPath();
			Brush brush = new SolidBrush(BorderColor);
			Pen pen = new Pen(brush, BorderThickness);
			LinearGradientBrush linearGradientBrush = null;
			Brush brush2 = (PaintGroupBox ? new SolidBrush(CustomGroupBoxColor) : new SolidBrush(BackgroundColor));
			SolidBrush solidBrush = new SolidBrush(ForeColor);
			SolidBrush solidBrush2 = null;
			GraphicsPath graphicsPath2 = null;
			if (ShadowControl)
			{
				solidBrush2 = new SolidBrush(ShadowColor);
				graphicsPath2 = new GraphicsPath();
				graphicsPath2.AddArc(num + (ShadowThickness - 1), num3 + (ShadowThickness - 1), roundCorners, roundCorners2, 180f, 90f);
				graphicsPath2.AddArc(num2 + (ShadowThickness - 1), num3 + (ShadowThickness - 1), roundCorners, roundCorners2, 270f, 90f);
				graphicsPath2.AddArc(num2 + (ShadowThickness - 1), num4 + (ShadowThickness - 1), roundCorners, roundCorners2, 360f, 90f);
				graphicsPath2.AddArc(num + (ShadowThickness - 1), num4 + (ShadowThickness - 1), roundCorners, roundCorners2, 90f, 90f);
				graphicsPath2.CloseAllFigures();
				g.FillPath(solidBrush2, graphicsPath2);
			}
			graphicsPath.AddArc(num, num3, roundCorners, roundCorners2, 180f, 90f);
			graphicsPath.AddArc(num2, num3, roundCorners, roundCorners2, 270f, 90f);
			graphicsPath.AddArc(num2, num4, roundCorners, roundCorners2, 360f, 90f);
			graphicsPath.AddArc(num, num4, roundCorners, roundCorners2, 90f, 90f);
			graphicsPath.CloseAllFigures();
			if (PaintGroupBox)
			{
				g.FillPath(brush2, graphicsPath);
			}
			else if (BackgroundGradientMode == GroupBoxGradientMode.None)
			{
				g.FillPath(brush2, graphicsPath);
			}
			else
			{
				linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, base.Width, base.Height), BackgroundColor, BackgroundGradientColor, (LinearGradientMode)BackgroundGradientMode);
				g.FillPath(linearGradientBrush, graphicsPath);
			}
			g.DrawPath(pen, graphicsPath);
			int num5 = ((GroupImage != null) ? 24 : 8);
			g.DrawString(GroupTitle, Font, solidBrush, num5, 5f);
			if (GroupImage != null)
			{
				g.DrawImage(GroupImage, 28, 4, 16, 16);
			}
			graphicsPath?.Dispose();
			brush?.Dispose();
			pen?.Dispose();
			linearGradientBrush?.Dispose();
			brush2?.Dispose();
			solidBrush?.Dispose();
			solidBrush2?.Dispose();
			graphicsPath2?.Dispose();
		}
	}

	private void PaintBack(Graphics g)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		int num = RoundCorners * 2;
		int num2 = RoundCorners * 2;
		int num3 = 0;
		int num4 = (ShadowControl ? (base.Width - (num + 1) - ShadowThickness) : (base.Width - (num + 1)));
		int num5 = 10;
		int num6 = (ShadowControl ? (base.Height - (num2 + 1) - ShadowThickness) : (base.Height - (num2 + 1)));
		GraphicsPath graphicsPath = new GraphicsPath();
		Brush brush = new SolidBrush(BorderColor);
		Pen pen = new Pen(brush, BorderThickness);
		LinearGradientBrush linearGradientBrush = null;
		Brush brush2 = new SolidBrush(BackgroundColor);
		SolidBrush solidBrush = null;
		GraphicsPath graphicsPath2 = null;
		if (ShadowControl)
		{
			solidBrush = new SolidBrush(ShadowColor);
			graphicsPath2 = new GraphicsPath();
			graphicsPath2.AddArc(num3 + ShadowThickness, num5 + ShadowThickness, num, num2, 180f, 90f);
			graphicsPath2.AddArc(num4 + ShadowThickness, num5 + ShadowThickness, num, num2, 270f, 90f);
			graphicsPath2.AddArc(num4 + ShadowThickness, num6 + ShadowThickness, num, num2, 360f, 90f);
			graphicsPath2.AddArc(num3 + ShadowThickness, num6 + ShadowThickness, num, num2, 90f, 90f);
			graphicsPath2.CloseAllFigures();
			g.FillPath(solidBrush, graphicsPath2);
		}
		graphicsPath.AddArc(num3, num5, num, num2, 180f, 90f);
		graphicsPath.AddArc(num4, num5, num, num2, 270f, 90f);
		graphicsPath.AddArc(num4, num6, num, num2, 360f, 90f);
		graphicsPath.AddArc(num3, num6, num, num2, 90f, 90f);
		graphicsPath.CloseAllFigures();
		if (BackgroundGradientMode == GroupBoxGradientMode.None)
		{
			g.FillPath(brush2, graphicsPath);
		}
		else
		{
			linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, base.Width, base.Height), BackgroundColor, BackgroundGradientColor, (LinearGradientMode)BackgroundGradientMode);
			g.FillPath(linearGradientBrush, graphicsPath);
		}
		g.DrawPath(pen, graphicsPath);
		graphicsPath?.Dispose();
		brush?.Dispose();
		pen?.Dispose();
		linearGradientBrush?.Dispose();
		brush2?.Dispose();
		solidBrush?.Dispose();
		graphicsPath2?.Dispose();
	}

	private void GroupBox_Resize(object sender, EventArgs e)
	{
		Refresh();
	}
}

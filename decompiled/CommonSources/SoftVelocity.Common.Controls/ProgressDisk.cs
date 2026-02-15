using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SoftVelocity.Common.Controls;

[DefaultProperty("BlockSize")]
public class ProgressDisk : UserControl
{
	public enum BlockSizeType
	{
		XSmall,
		Small,
		Medium,
		Large,
		XLarge,
		XXLarge
	}

	private GraphicsPath bkGroundPath1 = new GraphicsPath();

	private GraphicsPath bkGroundPath2 = new GraphicsPath();

	private GraphicsPath valuePath = new GraphicsPath();

	private GraphicsPath freGroundPath = new GraphicsPath();

	private int sliceCount;

	private int value;

	private Color backGrndColor = Color.White;

	private Color activeforeColor1 = Color.Blue;

	private Color activeforeColor2 = Color.LightBlue;

	private Color inactiveforeColor1 = Color.Green;

	private Color inactiveforeColor2 = Color.LightGreen;

	private int size = 50;

	private float blockRatio = 0.4f;

	private BlockSizeType bs = BlockSizeType.Small;

	private Region region = new Region();

	private IContainer components;

	[DefaultValue(0)]
	public int Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
			Render();
		}
	}

	[DefaultValue(typeof(Color), "White")]
	public Color BackGroundColor
	{
		get
		{
			return backGrndColor;
		}
		set
		{
			backGrndColor = value;
			Render();
		}
	}

	[DefaultValue(typeof(Color), "Blue")]
	public Color ActiveForeColor1
	{
		get
		{
			return activeforeColor1;
		}
		set
		{
			activeforeColor1 = value;
			Render();
		}
	}

	[DefaultValue(typeof(Color), "LightBlue")]
	public Color ActiveForeColor2
	{
		get
		{
			return activeforeColor2;
		}
		set
		{
			activeforeColor2 = value;
			Render();
		}
	}

	[DefaultValue(typeof(Color), "Green")]
	public Color InactiveForeColor1
	{
		get
		{
			return inactiveforeColor1;
		}
		set
		{
			inactiveforeColor1 = value;
			Render();
		}
	}

	[DefaultValue(typeof(Color), "LightGreen")]
	public Color InactiveForeColor2
	{
		get
		{
			return inactiveforeColor2;
		}
		set
		{
			inactiveforeColor2 = value;
			Render();
		}
	}

	[DefaultValue(50)]
	public int SquareSize
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
			base.Size = new Size(size, size);
		}
	}

	[DefaultValue(typeof(BlockSizeType), "Small")]
	public BlockSizeType BlockSize
	{
		get
		{
			return bs;
		}
		set
		{
			bs = value;
			switch (bs)
			{
			case BlockSizeType.XSmall:
				blockRatio = 0.49f;
				break;
			case BlockSizeType.Small:
				blockRatio = 0.4f;
				break;
			case BlockSizeType.Medium:
				blockRatio = 0.3f;
				break;
			case BlockSizeType.Large:
				blockRatio = 0.2f;
				break;
			case BlockSizeType.XLarge:
				blockRatio = 0.1f;
				break;
			case BlockSizeType.XXLarge:
				blockRatio = 0.01f;
				break;
			}
		}
	}

	[DefaultValue(12)]
	public int SliceCount
	{
		get
		{
			return sliceCount;
		}
		set
		{
			sliceCount = value;
		}
	}

	public ProgressDisk()
	{
		InitializeComponent();
		Render();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		this.region = new Region(base.ClientRectangle);
		if (backGrndColor == Color.Transparent)
		{
			Region region = new Region(base.ClientRectangle);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddPath(bkGroundPath1, connect: false);
			graphicsPath.AddPath(bkGroundPath2, connect: false);
			region.Exclude(graphicsPath);
			this.region.Exclude(region);
			base.Region = this.region;
		}
		e.Graphics.FillPath(new SolidBrush(backGrndColor), bkGroundPath1);
		e.Graphics.FillPath(new LinearGradientBrush(new Rectangle(0, 0, size, size), inactiveforeColor1, inactiveforeColor2, value * 360 / 12, isAngleScaleable: true), valuePath);
		e.Graphics.FillPath(new LinearGradientBrush(new Rectangle(0, 0, size, size), activeforeColor1, activeforeColor2, value * 360 / 12, isAngleScaleable: true), freGroundPath);
		e.Graphics.FillPath(new SolidBrush(backGrndColor), bkGroundPath2);
		base.OnPaint(e);
	}

	private void Render()
	{
		bkGroundPath1.Reset();
		bkGroundPath2.Reset();
		valuePath.Reset();
		freGroundPath.Reset();
		bkGroundPath1.AddPie(new Rectangle(0, 0, size, size), 0f, 360f);
		if (sliceCount == 0)
		{
			sliceCount = 12;
		}
		float num = 360 / sliceCount;
		float sweepAngle = num - 5f;
		for (int i = 0; i < sliceCount; i++)
		{
			if (value != i)
			{
				valuePath.AddPie(0, 0, size, size, (float)i * num, sweepAngle);
			}
		}
		bkGroundPath2.AddPie((float)(size / 2) - (float)size * blockRatio, (float)(size / 2) - (float)size * blockRatio, blockRatio * 2f * (float)size, blockRatio * 2f * (float)size, 0f, 360f);
		freGroundPath.AddPie(new Rectangle(0, 0, size, size), (float)value * num, sweepAngle);
		Invalidate();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		size = Math.Max(base.Width, base.Height);
		base.Size = new Size(size, size);
		Render();
		base.OnSizeChanged(e);
	}

	protected override void OnResize(EventArgs e)
	{
		size = Math.Max(base.Width, base.Height);
		base.Size = new Size(size, size);
		Render();
		base.OnResize(e);
	}

	private void ProgressDisk_Load(object sender, EventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.DoubleBuffered = true;
		base.Name = "ProgressDisk";
		base.Size = new System.Drawing.Size(50, 50);
		base.ResumeLayout(false);
	}
}

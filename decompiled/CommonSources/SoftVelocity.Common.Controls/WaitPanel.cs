using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SoftVelocity.Common.Controls;

public class WaitPanel : UserControl
{
	private bool _DelayStart;

	private AlphaBlendType _AlphaBlend = AlphaBlendType.Blend;

	private bool _UseGradient;

	private Color _BackColorGradientEnd = SystemColors.Control;

	private Color _BackColorGradientBegin = SystemColors.Window;

	private LinearGradientMode _GradientMode;

	private string message = string.Empty;

	private bool progressDiskVisible;

	private Timer waitTimer;

	private bool showing;

	private byte alpha;

	private IContainer components;

	private ProgressDisk progressDisk1;

	private Timer timer1;

	private Label labelText;

	public bool DelayStart
	{
		get
		{
			return _DelayStart;
		}
		set
		{
			_DelayStart = value;
		}
	}

	public AlphaBlendType AlphaBlend
	{
		get
		{
			return _AlphaBlend;
		}
		set
		{
			_AlphaBlend = value;
			switch (_AlphaBlend)
			{
			case AlphaBlendType.None:
				Alpha = byte.MaxValue;
				break;
			case AlphaBlendType.Blend:
				Alpha = 180;
				break;
			case AlphaBlendType.Transparent:
				Alpha = 0;
				break;
			}
		}
	}

	public bool UseGradient
	{
		get
		{
			return _UseGradient;
		}
		set
		{
			_UseGradient = value;
		}
	}

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

	public string Message
	{
		get
		{
			return message;
		}
		set
		{
			message = value;
			if (!string.IsNullOrEmpty(message))
			{
				if (labelText != null)
				{
					labelText.Text = message;
					labelText.Visible = true;
				}
			}
			else if (labelText != null)
			{
				labelText.Visible = false;
			}
		}
	}

	protected override CreateParams CreateParams
	{
		get
		{
			if (_AlphaBlend != AlphaBlendType.Transparent)
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 32;
				return createParams;
			}
			return base.CreateParams;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[Category("Appearance")]
	[Browsable(true)]
	[DefaultValue(0)]
	private byte Alpha
	{
		get
		{
			return alpha;
		}
		set
		{
			alpha = value;
		}
	}

	public WaitPanel()
	{
		Font = new Font("Verdana", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
		InitializeComponent();
		waitTimer = new Timer();
		waitTimer.Interval = 500;
		waitTimer.Tick += waitTimer_Tick;
		Dock = DockStyle.Fill;
		labelText.Text = string.Empty;
		base.UseWaitCursor = true;
	}

	private void ParentForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		BringToFront();
		Select();
	}

	private void ParentForm_GotFocus(object sender, EventArgs e)
	{
		BringToFront();
		Select();
	}

	private void waitTimer_Tick(object sender, EventArgs e)
	{
		progressDisk1.Visible = true;
		progressDiskVisible = true;
		Refresh();
		Invalidate();
		Application.DoEvents();
		waitTimer.Stop();
		labelText.Text += ".";
	}

	public void ShowWaitPanel()
	{
		showing = true;
		Show();
		if (_DelayStart)
		{
			progressDiskVisible = false;
			progressDisk1.Visible = false;
			waitTimer.Start();
		}
		else
		{
			progressDiskVisible = true;
			progressDisk1.Visible = true;
			Invalidate();
		}
		base.Visible = true;
		timer1.Start();
		Refresh();
		Application.DoEvents();
	}

	public void HideWaitPanel()
	{
		showing = false;
		base.Visible = false;
		if (!progressDiskVisible)
		{
			waitTimer.Stop();
			waitTimer.Enabled = false;
		}
		timer1.Stop();
		progressDiskVisible = true;
		progressDisk1.Visible = false;
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		if (progressDiskVisible)
		{
			if (progressDisk1.Value == 100)
			{
				progressDisk1.Value = 0;
			}
			progressDisk1.Value += 1;
		}
	}

	private void WaitPannel_Load(object sender, EventArgs e)
	{
		if (Message != string.Empty)
		{
			labelText.Text = Message;
			labelText.Font = Font;
		}
		else
		{
			labelText.Visible = false;
		}
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.Opaque, value: true);
		SetStyle(ControlStyles.DoubleBuffer, value: true);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.Tab)
		{
			BringToFront();
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	protected void InvalidateEx()
	{
		if (base.Parent != null)
		{
			Rectangle rc = new Rectangle(base.Location, base.Size);
			base.Parent.Invalidate(rc, invalidateChildren: true);
		}
	}

	private void InvalidateParent()
	{
		if (base.Parent == null)
		{
			Invalidate();
		}
		else
		{
			base.Parent.Invalidate(invalidateChildren: true);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (base.ParentForm == null)
		{
			return;
		}
		if (Alpha != 0)
		{
			if (UseGradient)
			{
				Color color = Color.FromArgb(alpha, BackColorGradientBegin.R, BackColorGradientBegin.G, BackColorGradientBegin.B);
				Color color2 = Color.FromArgb(alpha, BackColorGradientEnd.R, BackColorGradientEnd.G, BackColorGradientEnd.B);
				using LinearGradientBrush brush = new LinearGradientBrush(base.ParentForm.ClientRectangle, color, color2, GradientMode);
				e.Graphics.FillRectangle(brush, base.ParentForm.ClientRectangle);
			}
			else
			{
				Color color3 = Color.FromArgb(alpha, BackColor.R, BackColor.G, BackColor.B);
				using Brush brush2 = new SolidBrush(color3);
				e.Graphics.FillRectangle(brush2, base.ParentForm.ClientRectangle);
			}
			if (labelText.Visible)
			{
				labelText.Location = new Point(3, 86);
				labelText.Size = new Size(base.Size.Width - 3, labelText.Size.Height);
			}
		}
		base.OnPaint(e);
	}

	private void WaitPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
	}

	private void labelText_VisibleChanged(object sender, EventArgs e)
	{
		if (base.Visible)
		{
			timer1.Start();
		}
		else
		{
			timer1.Stop();
		}
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
		this.components = new System.ComponentModel.Container();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.labelText = new System.Windows.Forms.Label();
		this.progressDisk1 = new SoftVelocity.Common.Controls.ProgressDisk();
		base.SuspendLayout();
		this.timer1.Interval = 150;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		this.labelText.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelText.BackColor = System.Drawing.Color.Transparent;
		this.labelText.Font = new System.Drawing.Font("Verdana", 13.8f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelText.Location = new System.Drawing.Point(3, 26);
		this.labelText.Name = "labelText";
		this.labelText.Size = new System.Drawing.Size(639, 37);
		this.labelText.TabIndex = 1;
		this.labelText.Text = "labelText";
		this.labelText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelText.VisibleChanged += new System.EventHandler(labelText_VisibleChanged);
		this.progressDisk1.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.progressDisk1.BackColor = System.Drawing.Color.Transparent;
		this.progressDisk1.BackGroundColor = System.Drawing.Color.Transparent;
		this.progressDisk1.BlockSize = SoftVelocity.Common.Controls.ProgressDisk.BlockSizeType.Medium;
		this.progressDisk1.Location = new System.Drawing.Point(284, 156);
		this.progressDisk1.Margin = new System.Windows.Forms.Padding(5);
		this.progressDisk1.Name = "progressDisk1";
		this.progressDisk1.Size = new System.Drawing.Size(79, 79);
		this.progressDisk1.SliceCount = 7;
		this.progressDisk1.SquareSize = 79;
		this.progressDisk1.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Control;
		base.Controls.Add(this.labelText);
		base.Controls.Add(this.progressDisk1);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.Name = "WaitPanel";
		base.Size = new System.Drawing.Size(645, 385);
		base.Load += new System.EventHandler(WaitPannel_Load);
		base.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(WaitPanel_PreviewKeyDown);
		base.ResumeLayout(false);
	}
}

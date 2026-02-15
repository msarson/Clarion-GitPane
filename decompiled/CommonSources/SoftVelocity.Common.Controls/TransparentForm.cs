using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SoftVelocity.Common.Controls;

public class TransparentForm : Form
{
	private const int WS_EX_TOOLWINDOW = 128;

	private const short WM_ACTIVATE = 6;

	private const short WM_NCACTIVATE = 134;

	private Form mainFrame;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= 128;
			return createParams;
		}
	}

	public Form MainFrame
	{
		get
		{
			return mainFrame;
		}
		set
		{
			mainFrame = value;
		}
	}

	public TransparentForm()
	{
		InitializeComponent();
		base.ShowInTaskbar = false;
		base.TopMost = true;
		SetStyle(ControlStyles.Selectable, value: false);
	}

	protected override void WndProc(ref Message m)
	{
		if (base.Visible && m.Msg == 6)
		{
			if ((int)m.WParam == 1)
			{
				base.TopMost = true;
				Select();
				Refresh();
			}
			else
			{
				base.TopMost = false;
			}
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	private void WaitForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		mainFrame.Resize -= ParentForm_Resize;
		mainFrame.Move -= ParentForm_Resize;
		mainFrame.Activated -= mainFrame_GotFocus;
	}

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	private static extern uint SendMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

	private void mainFrame_GotFocus(object sender, EventArgs e)
	{
		if (base.Visible)
		{
			ReShow();
		}
	}

	public void ReShow()
	{
		if (!base.Visible)
		{
			base.Location = new Point(mainFrame.Location.X, mainFrame.Location.Y + 22);
			base.Size = new Size(mainFrame.Size.Width, mainFrame.Size.Height - 22);
		}
		base.Visible = true;
		Select();
		SendMessage(mainFrame.Handle, 134, 1u, 0u);
	}

	private void WaitForm_Shown(object sender, EventArgs e)
	{
		if (mainFrame != null)
		{
			mainFrame.Resize += ParentForm_Resize;
			mainFrame.Move += ParentForm_Resize;
			mainFrame.Activated += mainFrame_GotFocus;
			SendMessage(mainFrame.Handle, 134, 1u, 0u);
		}
	}

	private void ParentForm_Resize(object sender, EventArgs e)
	{
		if (mainFrame != null && base.Visible)
		{
			base.Location = new Point(mainFrame.Location.X, mainFrame.Location.Y + 22);
			base.Size = new Size(mainFrame.Size.Width, mainFrame.Size.Height - 22);
		}
	}

	private void WaitForm_Load(object sender, EventArgs e)
	{
		ParentForm_Resize(null, null);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(524, 323);
		base.ControlBox = false;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Opacity = 0.5;
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		base.TopMost = true;
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(WaitForm_FormClosed);
		base.Shown += new System.EventHandler(WaitForm_Shown);
		base.Load += new System.EventHandler(WaitForm_Load);
		base.ResumeLayout(false);
	}
}

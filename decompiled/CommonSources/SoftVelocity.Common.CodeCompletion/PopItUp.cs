using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SoftVelocity.Common.CodeCompletion;

[ToolboxItem(false)]
public class PopItUp : ToolStripDropDown
{
	private const int frames = 5;

	private const int totalduration = 200;

	private const int frameduration = 40;

	private readonly Control m_contents;

	private readonly ToolStripControlHost m_host;

	private bool m_fade = true;

	public bool EnableFadeEffect
	{
		get
		{
			return m_fade;
		}
		set
		{
			m_fade = value && SystemInformation.IsMenuAnimationEnabled && SystemInformation.IsMenuFadeEnabled;
		}
	}

	public PopItUp(Control contents)
	{
		if (contents == null)
		{
			throw new ArgumentNullException("contents");
		}
		m_contents = contents;
		contents.Location = Point.Empty;
		m_host = new ToolStripControlHost(contents);
		m_host.AutoSize = false;
		m_host.Padding = (m_host.Margin = Padding.Empty);
		base.Padding = (base.Margin = Padding.Empty);
		Items.Add(m_host);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if ((keyData & Keys.Alt) == Keys.Alt)
		{
			return false;
		}
		if (Keys.Tab == keyData)
		{
			Control ctl = null;
			foreach (Control control3 in m_contents.Controls)
			{
				if (control3.Focused)
				{
					ctl = control3;
				}
			}
			m_contents.SelectNextControl(ctl, forward: true, tabStopOnly: true, nested: true, wrap: true);
		}
		if ((keyData & Keys.Shift) == Keys.Shift && (keyData & Keys.Tab) == Keys.Tab)
		{
			Control ctl2 = null;
			foreach (Control control4 in m_contents.Controls)
			{
				if (control4.Focused)
				{
					ctl2 = control4;
				}
			}
			m_contents.SelectNextControl(ctl2, forward: false, tabStopOnly: true, nested: true, wrap: true);
		}
		return base.ProcessDialogKey(keyData);
	}

	public void ShowPopup(Control control, Point location)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		location = control.PointToScreen(location);
		Rectangle workingArea = Screen.FromControl(control).WorkingArea;
		if (location.X + m_contents.Size.Width > workingArea.Left + workingArea.Width)
		{
			location.X = workingArea.Left + workingArea.Width - m_contents.Size.Width;
		}
		if (location.Y + m_contents.Size.Height > workingArea.Top + workingArea.Height)
		{
			location.Y = workingArea.Top + workingArea.Height - m_contents.Size.Height;
		}
		location = control.PointToClient(location);
		Show(control, location, ToolStripDropDownDirection.BelowRight);
	}

	protected override void SetVisibleCore(bool visible)
	{
		double opacity = base.Opacity;
		if (visible && EnableFadeEffect)
		{
			base.Opacity = 0.0;
		}
		base.SetVisibleCore(visible);
		if (!visible || !EnableFadeEffect)
		{
			return;
		}
		for (int i = 1; i <= 5; i++)
		{
			if (i > 1)
			{
				Thread.Sleep(40);
			}
			base.Opacity = opacity * (double)i / 5.0;
		}
		base.Opacity = opacity;
	}

	protected override void OnOpened(EventArgs e)
	{
		m_contents.Focus();
		base.OnOpened(e);
	}
}

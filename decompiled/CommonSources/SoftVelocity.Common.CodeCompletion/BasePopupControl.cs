using System;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Common.CodeCompletion;

public class BasePopupControl : UserControl
{
	protected PopItUp m_popItUp;

	public BasePopupControl()
	{
		m_popItUp = new PopItUp(this);
		m_popItUp.EnableFadeEffect = false;
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if ((keyData & Keys.Alt) == Keys.Alt && (keyData & Keys.F4) == Keys.F4)
		{
			base.Parent.Hide();
			return true;
		}
		if ((keyData & Keys.Return) == Keys.Return && base.ActiveControl is Button)
		{
			(base.ActiveControl as Button).PerformClick();
			return true;
		}
		return base.ProcessDialogKey(keyData);
	}

	protected virtual Point GetPopupLocation(Control control)
	{
		return Point.Empty;
	}

	public void ShowAsContextMenu(Control control)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		m_popItUp.ShowPopup(control, GetPopupLocation(control));
	}

	public void ShowAsContextMenu(Control control, Point p)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		m_popItUp.ShowPopup(control, p);
	}

	public virtual void Close()
	{
		m_popItUp.Close();
	}
}

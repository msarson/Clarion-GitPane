using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.Controls;

public class WaiterPanel
{
	private int count;

	private Form F;

	private Control C;

	private WaitPanel _WaitPanel;

	public static WaiterPanel NewWaiter(Form parent)
	{
		WaiterPanel waiterPanel = new WaiterPanel();
		waiterPanel.F = parent;
		return waiterPanel;
	}

	public static WaiterPanel NewWaiter(Control parent)
	{
		WaiterPanel waiterPanel = new WaiterPanel();
		waiterPanel.F = getParentForm(parent);
		if (waiterPanel.F == null)
		{
			waiterPanel.C = parent;
		}
		return waiterPanel;
	}

	private WaiterPanel()
	{
	}

	private static Form getParentForm(Control c)
	{
		if (c == null)
		{
			return null;
		}
		if (c is Form)
		{
			return (Form)c;
		}
		if (c is ContainerControl)
		{
			return ((ContainerControl)c).ParentForm;
		}
		return getParentForm(c.Parent);
	}

	private void CreateWaitPanel()
	{
		if (_WaitPanel == null)
		{
			_WaitPanel = new WaitPanel();
			_WaitPanel.AlphaBlend = AlphaBlendType.Transparent;
			_WaitPanel.Dock = DockStyle.Fill;
			_WaitPanel.Location = new Point(0, 0);
			_WaitPanel.DelayStart = false;
			if (F == null)
			{
				F = getParentForm(C);
			}
			if (F != null)
			{
				C = null;
				F.Controls.Add(_WaitPanel);
				F.Controls.SetChildIndex(_WaitPanel, 0);
			}
		}
	}

	public void Show()
	{
		Show(withDelay: false);
	}

	public void Show(bool withDelay)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<bool>((Action<bool>)Show, withDelay);
		}
		else if (_WaitPanel != null)
		{
			count++;
			if (count <= 1)
			{
				_WaitPanel.DelayStart = withDelay;
				_WaitPanel.ShowWaitPanel();
			}
		}
		else
		{
			CreateWaitPanel();
			if (_WaitPanel != null)
			{
				Show(withDelay);
			}
		}
	}

	public void Hide()
	{
		Hide(force: false);
	}

	public void Hide(bool force)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<bool>((Action<bool>)Hide, force);
		}
		else if (_WaitPanel != null)
		{
			count--;
			if (force || count <= 0)
			{
				count = 0;
				_WaitPanel.HideWaitPanel();
			}
		}
	}
}

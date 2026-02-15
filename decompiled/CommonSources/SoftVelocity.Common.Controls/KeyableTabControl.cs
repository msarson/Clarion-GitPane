using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace SoftVelocity.Common.Controls;

public class KeyableTabControl : TabControl
{
	private bool useHotKeys;

	private IContainer components;

	private bool altPressed = true;

	private Dictionary<string, bool> _enableTabPageStatus = new Dictionary<string, bool>();

	public KeyableTabControl()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		InitializeComponent();
		Properties val = PropertyService.Get<Properties>("Clarion.Dictionary", new Properties());
		useHotKeys = val.Get<bool>("HotKeysOnTabs", false);
		if (useHotKeys)
		{
			base.DrawMode = TabDrawMode.OwnerDrawFixed;
			SetKeyDownHandlers(this);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
			ClearKeyDownHandlers();
		}
		base.Dispose(disposing);
	}

	[System.Diagnostics.DebuggerStepThrough]
	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
	}

	private void ProcessParentChanged(object sender, EventArgs e)
	{
		if (sender is Control control)
		{
			control.KeyDown -= ProcessKeyDown;
			control.ParentChanged -= ProcessParentChanged;
			SetKeyDownHandlers(control);
		}
	}

	private void SetKeyDownHandlers(Control c)
	{
		for (Control control = c.Parent; control != null; control = control.Parent)
		{
			c = control;
		}
		c.KeyDown += ProcessKeyDown;
		c.ParentChanged += ProcessParentChanged;
	}

	private void ClearKeyDownHandlers()
	{
		for (Control control = base.Parent; control != null; control = control.Parent)
		{
			control.KeyDown -= ProcessKeyDown;
			control.ParentChanged -= ProcessParentChanged;
		}
	}

	private void ProcessKeyDown(object sender, KeyEventArgs args)
	{
		if (args.Alt && !altPressed && useHotKeys)
		{
			altPressed = true;
			Refresh();
		}
	}

	protected override void OnKeyDown(KeyEventArgs ke)
	{
		base.OnKeyDown(ke);
		ProcessKeyDown(this, ke);
	}

	public void SetTabPageEnabled(int tabPageIndex, bool enabled)
	{
		string name = base.TabPages[tabPageIndex].Name;
		SetTabPageEnabled(name, enabled);
	}

	public void SetTabPageEnabled(string tabPageName, bool enabled)
	{
		if (_enableTabPageStatus.ContainsKey(tabPageName))
		{
			_enableTabPageStatus[tabPageName] = enabled;
		}
		else
		{
			_enableTabPageStatus.Add(tabPageName, enabled);
		}
	}

	public bool IsTabPageEnabled(int tabPageIndex)
	{
		string name = base.TabPages[tabPageIndex].Name;
		return IsTabPageEnabled(name);
	}

	public bool IsTabPageEnabled(string tabPageName)
	{
		if (_enableTabPageStatus.ContainsKey(tabPageName))
		{
			return _enableTabPageStatus[tabPageName];
		}
		return true;
	}

	protected override void OnSelecting(TabControlCancelEventArgs e)
	{
		try
		{
			base.OnSelecting(e);
			if (!e.Cancel)
			{
				e.Cancel = !IsTabPageEnabled(e.TabPage.Name);
			}
		}
		catch (Exception)
		{
		}
	}

	protected override bool ProcessMnemonic(char charCode)
	{
		if (useHotKeys && base.CanSelect)
		{
			foreach (TabPage tabPage in base.TabPages)
			{
				if (Control.IsMnemonic(charCode, tabPage.Text))
				{
					base.SelectedTab = tabPage;
					Focus();
					return true;
				}
			}
		}
		return base.ProcessMnemonic(charCode);
	}

	protected override void OnDrawItem(DrawItemEventArgs e)
	{
		base.OnDrawItem(e);
		try
		{
			DrawItemImplementation(e);
		}
		catch (Exception)
		{
		}
	}

	protected virtual void DrawItemImplementation(DrawItemEventArgs e)
	{
		Graphics graphics = e.Graphics;
		StringFormat stringFormat = new StringFormat();
		TabPage tabPage = base.TabPages[e.Index];
		Rectangle tabRect = GetTabRect(e.Index);
		if (e.Index == base.SelectedIndex)
		{
			Rectangle rect = tabRect;
			rect.Inflate(0, 2);
			graphics.FillRectangle(SystemBrushes.ControlLightLight, rect);
		}
		else
		{
			Color controlLightLight = SystemColors.ControlLightLight;
			Color control = SystemColors.Control;
			Brush brush = new LinearGradientBrush(tabRect, controlLightLight, control, LinearGradientMode.Vertical);
			graphics.FillRectangle(brush, tabRect);
		}
		tabRect.Offset(tabPage.Margin.Left, tabPage.Margin.Top);
		if (tabPage.ImageIndex > -1)
		{
			Image image = base.ImageList.Images[tabPage.ImageIndex];
			graphics.DrawImage(image, tabRect.Left, tabRect.Top);
			tabRect.Offset(image.Size.Width + tabPage.Margin.Left, 0);
		}
		Brush brush2 = GetBrush(tabPage);
		Font font = GetFont(tabPage);
		if (altPressed)
		{
			stringFormat.HotkeyPrefix = HotkeyPrefix.Show;
		}
		else
		{
			stringFormat.HotkeyPrefix = HotkeyPrefix.Hide;
		}
		graphics.DrawString(tabPage.Text, font, brush2, tabRect, stringFormat);
	}

	protected virtual Font GetFont(TabPage tp)
	{
		return Font;
	}

	protected virtual Brush GetBrush(TabPage tp)
	{
		if (IsTabPageEnabled(tp.Name))
		{
			if (tp == base.SelectedTab)
			{
				return SystemBrushes.ActiveCaptionText;
			}
			return SystemBrushes.WindowText;
		}
		return SystemBrushes.InactiveCaption;
	}

	protected override void OnControlAdded(ControlEventArgs e)
	{
		base.OnControlAdded(e);
		if (!useHotKeys && e.Control is TabPage tabPage)
		{
			tabPage.TextChanged += PageTextChanged;
		}
	}

	private void PageTextChanged(object sender, EventArgs e)
	{
		TabPage tabPage = (TabPage)sender;
		if (tabPage.Text.Contains("&"))
		{
			tabPage.Text = tabPage.Text.Replace("&", string.Empty);
		}
	}
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Clarion.ASL;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.CWPInvoke;

public sealed class CWDialogForm : Form
{
	private IContainer components;

	private Form _previousForm;

	private CWInternalDialogWindow Interior;

	private bool ClosedFromCW;

	private bool _activatingInternal;

	private bool opened;

	private bool resizable;

	private bool dorefresh = true;

	internal Form PreviousForm
	{
		get
		{
			return _previousForm;
		}
		set
		{
			_previousForm = value;
		}
	}

	public CWDialogForm()
	{
		Interior = new CWInternalDialogWindow(this);
		Interior.SetupWindow();
		InitializeComponent();
		base.Icon = ResourceService.GetIcon("Icons.SharpDevelopIcon");
		Interior.AttachToParentFormCloseEvent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
			Interior = null;
		}
		base.Dispose(disposing);
	}

	public void BindDialogWindow(UINetBinding CWObj)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Interior.BindCWWindow(CWObj);
		opened = true;
		UIBooleanProperty val = (UIBooleanProperty)CWObj.Property((UIControlProperties)10);
		if (val != null && val.ValueOf)
		{
			MakeResizeable();
		}
	}

	public void UnbindDialogWindow()
	{
	}

	internal void OnCWDialogWindowClosed()
	{
		ClosedFromCW = true;
		Application.DoEvents();
		Close();
		Application.DoEvents();
	}

	private void DoActivateInternal()
	{
		if (!_activatingInternal)
		{
			_activatingInternal = true;
			Application.DoEvents();
			Interior.Select();
			Interior.SetFocusOnChild();
			_activatingInternal = false;
		}
	}

	private void CWDialogForm_OnFormActivated(object sender, EventArgs e)
	{
		DoActivateInternal();
	}

	private void CWDialogForm_OnFormEnter(object sender, EventArgs e)
	{
	}

	private void CWDialogForm_OnFormShown(object sender, EventArgs e)
	{
		DoActivateInternal();
	}

	private void CWDialogForm_OnFormClosing(object sender, FormClosingEventArgs e)
	{
		if (!ClosedFromCW)
		{
			e.Cancel = true;
			Interior.RequestClose();
			return;
		}
		SaveDialogPosition(Interior.Name);
		Interior.DisconnectFromParent();
		Interior = null;
		UnregisterEvents();
	}

	internal void OnCWDialogWindowCaptionChanged(string txt)
	{
		Text = txt;
	}

	internal void OnCWDialogWindowNotifyNewSize(Size size)
	{
		if (!opened)
		{
			AutoSize = true;
			Interior.Size = size;
			MinimumSize = new Size(base.Size.Width, base.Size.Height);
			opened = true;
			if (base.FormBorderStyle == FormBorderStyle.Sizable)
			{
				MakeResizeable();
			}
		}
	}

	internal void MakeResizeable()
	{
		base.FormBorderStyle = FormBorderStyle.Sizable;
		if (opened && !resizable)
		{
			resizable = true;
			AutoSize = false;
			base.SizeGripStyle = SizeGripStyle.Show;
			base.MaximizeBox = true;
			Interior.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			base.ResizeEnd += CWDialogForm_ResizeEnd;
			LoadDialogPosition(Interior.Name);
		}
	}

	private void LoadDialogPosition(string DialogName)
	{
		if (string.IsNullOrEmpty(DialogName) || !(DialogName != "CWInternalDialogWindow"))
		{
			return;
		}
		Properties val = PropertyService.Get<Properties>("WindowPositions", (Properties)null);
		if (val == null)
		{
			return;
		}
		Properties val2 = val.Get<Properties>("HostedDialog" + DialogName, (Properties)null);
		if (val2 != null && val2.Contains("bounds"))
		{
			string[] array = val2["bounds"].Split(',');
			if (array.Length == 4)
			{
				base.Bounds = new Rectangle(int.Parse(array[0], NumberFormatInfo.InvariantInfo), int.Parse(array[1], NumberFormatInfo.InvariantInfo), int.Parse(array[2], NumberFormatInfo.InvariantInfo), int.Parse(array[3], NumberFormatInfo.InvariantInfo));
			}
		}
	}

	private void SaveDialogPosition(string DialogName)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		if (!string.IsNullOrEmpty(DialogName) && DialogName != "CWInternalDialogWindow" && base.WindowState != FormWindowState.Maximized)
		{
			Properties val = new Properties();
			val.Set<string>("bounds", $"{base.Bounds.X},{base.Bounds.Y},{base.Bounds.Width},{base.Bounds.Height}");
			Properties val2 = PropertyService.Get<Properties>("WindowPositions", (Properties)null);
			if (val2 == null)
			{
				val2 = new Properties();
			}
			val2.Set<Properties>("HostedDialog" + DialogName, val);
			PropertyService.Set<Properties>("WindowPositions", val2);
		}
	}

	private void CWDialogForm_ResizeEnd(object sender, EventArgs e)
	{
		if (opened)
		{
			Interior.OnHostWindowResize();
		}
		if (dorefresh)
		{
			dorefresh = false;
			WorkbenchSingleton.MainForm.Refresh();
		}
	}

	private void UnregisterEvents()
	{
		base.FormClosing -= CWDialogForm_OnFormClosing;
		base.Shown -= CWDialogForm_OnFormShown;
		base.Activated -= CWDialogForm_OnFormActivated;
		base.Enter -= CWDialogForm_OnFormEnter;
		base.ResizeEnd -= CWDialogForm_ResizeEnd;
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
		base.ClientSize = new System.Drawing.Size(346, 291);
		this.DoubleBuffered = true;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "CWDialogForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Template Actions";
		base.Visible = false;
		base.Controls.Add(this.Interior);
		base.ResumeLayout(false);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(CWDialogForm_OnFormClosing);
		base.Shown += new System.EventHandler(CWDialogForm_OnFormShown);
		base.Activated += new System.EventHandler(CWDialogForm_OnFormActivated);
		base.Enter += new System.EventHandler(CWDialogForm_OnFormEnter);
	}
}

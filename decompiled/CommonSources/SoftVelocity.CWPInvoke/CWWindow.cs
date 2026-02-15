using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Clarion.ASL;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

namespace SoftVelocity.CWPInvoke;

[DesignTimeVisible(false)]
[ToolboxItem(false)]
[DefaultProperty("GlobalRequest")]
public class CWWindow : CWUserControl
{
	public delegate void DisconnectEventHandler(object sender);

	public delegate void WindowOpenedEventHandler(object sender);

	public delegate void WindowClosingEventHandler(object sender);

	public delegate void IsDirtyChangedEventHandler(object sender);

	public delegate void CaptionChangedEventHandler(object sender);

	public delegate void NotifyNewSizeEventHandler(object sender, Size size);

	public delegate void ActivateParentEventHandler(object sender);

	private class CWWindow_TransparentPanel : Panel
	{
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 32;
				return createParams;
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}
	}

	protected UINetBinding Hosted;

	protected CWEventInvoker Invoker;

	private bool _IsModalDialog;

	protected bool SuspendResize;

	protected bool DelayedResize;

	private bool ready;

	private string _HostedWindowCaption;

	protected bool IsOnNotifyNewSize;

	protected bool _ParentClosed;

	private CWChildWindow[] _ChildWindowControls;

	private CWChildWindow _CurrentCWChildWindow;

	private RequestType _GlobalRequest;

	private ResponseType _GlobalResponse = ResponseType.RequestCancelled;

	[Browsable(false)]
	private string _DialogName = string.Empty;

	private static bool _DesignMode = WinFormsDesigner.IsInDesigner;

	protected bool IsModalDialog
	{
		get
		{
			return _IsModalDialog;
		}
		set
		{
			_IsModalDialog = value;
		}
	}

	public UIBindingInterfaceKind UIKind
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (Hosted != null)
			{
				return Hosted.UIKind();
			}
			return (UIBindingInterfaceKind)0;
		}
	}

	[Browsable(false)]
	public string OriginalWindowCaption
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			if (Hosted != null)
			{
				UIStringProperty val = (UIStringProperty)Hosted.Property((UIControlProperties)15);
				if (val != null)
				{
					return val.ValueOf;
				}
			}
			return string.Empty;
		}
	}

	public string HostedWindowCaption
	{
		get
		{
			if (string.IsNullOrEmpty(_HostedWindowCaption))
			{
				_HostedWindowCaption = OriginalWindowCaption;
			}
			return _HostedWindowCaption;
		}
	}

	public static uint FrameThread => GetCurrentThreadId();

	public static IntPtr FrameHandle => WorkbenchSingleton.MainForm.Handle;

	[Editor(typeof(CWChildWindowListUITypeEditor), typeof(UITypeEditor))]
	[DefaultValue(null)]
	public CWChildWindow[] ChildWindowControls
	{
		get
		{
			return _ChildWindowControls;
		}
		set
		{
			_ChildWindowControls = value;
		}
	}

	[Browsable(false)]
	public CWChildWindow CurrentCWChildWindow => _CurrentCWChildWindow;

	[Browsable(false)]
	public IntPtr HostHandle
	{
		set
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Expected O, but got Unknown
			if (Hosted != null)
			{
				LoggingService.Debug((object)("UIBinding: Setting host handle to " + value));
				UIPointerProperty val = (UIPointerProperty)Hosted.Property((UIControlProperties)100);
				if (val != null)
				{
					val.ValueOf = value;
				}
			}
		}
	}

	[DefaultValue(RequestType.None)]
	[Browsable(true)]
	[Description("Request used when call the clarion procedure")]
	[Category("Clarion Proc")]
	public RequestType GlobalRequest
	{
		get
		{
			_GlobalRequest = GetRequest();
			return _GlobalRequest;
		}
		set
		{
			_GlobalRequest = value;
			SetRequest(_GlobalRequest);
		}
	}

	[Browsable(false)]
	public ResponseType GlobalResponse
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			if (Hosted != null)
			{
				UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)4);
				if (val != null)
				{
					_GlobalResponse = (ResponseType)val.ValueOf;
				}
			}
			return _GlobalResponse;
		}
		set
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			_GlobalResponse = value;
			if (Hosted != null)
			{
				UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)4);
				if (val != null)
				{
					val.ValueOf = (int)_GlobalResponse;
				}
			}
		}
	}

	public string DialogName
	{
		get
		{
			if (Hosted != null)
			{
				_DialogName = ((object)Hosted).ToString();
			}
			return _DialogName;
		}
	}

	[Browsable(false)]
	public AutoState State
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			if (Hosted != null)
			{
				UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)5);
				if (val != null)
				{
					return (AutoState)val.ValueOf;
				}
			}
			return AutoState.Passive;
		}
	}

	[Browsable(false)]
	public bool IsDirty
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			if (Hosted != null)
			{
				UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)17);
				if (val != null && !val.ValueOf)
				{
					return false;
				}
			}
			return true;
		}
	}

	[Browsable(false)]
	public bool IsValid
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			if (Hosted == null)
			{
				return true;
			}
			try
			{
				UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)18);
				if (val != null && !val.ValueOf)
				{
					return false;
				}
			}
			catch (AccessViolationException)
			{
				Hosted = null;
			}
			catch (Exception ex2)
			{
				throw ex2;
			}
			return true;
		}
	}

	[Browsable(false)]
	public bool CanClose
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			if (Hosted != null)
			{
				UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)21);
				if (val != null)
				{
					return val.ValueOf;
				}
			}
			return true;
		}
	}

	public new bool DesignMode => _DesignMode;

	[Description("Event raised by the clarion procedure to disconnect CW window from .NET")]
	[Browsable(true)]
	[Category("Clarion Proc")]
	public event DisconnectEventHandler DisconnectFromHosted;

	[Category("Clarion Proc")]
	[Description("Event raised by the clarion procedure when the window open")]
	[Browsable(true)]
	public event WindowOpenedEventHandler WindowOpened;

	[Category("Clarion Proc")]
	[Description("Event raised by the clarion procedure when the window can be closed")]
	[Browsable(true)]
	public event WindowClosingEventHandler WindowClosing;

	[Browsable(true)]
	[Category("Clarion Proc")]
	[Description("Event raised by the clarion procedure when the dirty status changed")]
	public event IsDirtyChangedEventHandler IsDirtyChanged;

	[Browsable(true)]
	[Description("Event raised by the clarion procedure when text of window caption changed")]
	[Category("Clarion Proc")]
	public event CaptionChangedEventHandler CaptionChanged;

	[Browsable(true)]
	[Description("Event raised by the clarion procedure when the window change the size")]
	[Category("Clarion Proc")]
	public event NotifyNewSizeEventHandler NotifyNewSize;

	[Description("Event raised by the clarion procedure when the parent form must be activated")]
	[Browsable(true)]
	[Category("Clarion Proc")]
	public event ActivateParentEventHandler ActivateParent;

	public CWWindow()
		: this(notdocked: true)
	{
	}

	public CWWindow(bool notdocked)
	{
		Hosted = null;
		Invoker = null;
		IsModalDialog = notdocked;
		SuspendResize = false;
		DelayedResize = false;
	}

	public void LinkHosted(UINetBinding CWObj)
	{
		Application.DoEvents();
		CWObj.AddRef();
		Hosted = CWObj;
		Invoker = new CWEventInvoker(this);
		base.Name = DialogName;
		RegisterCWEvents();
		RegisterNetEvents();
	}

	public void UnlinkHosted()
	{
		UnRegisterCWEvents();
		UnregisterNetEvents();
		if (Invoker != null)
		{
			((EventInvokerCPPBase)Invoker).Dispose();
			Invoker = null;
		}
		HostHandle = IntPtr.Zero;
		if (Hosted != null)
		{
			Hosted.Release();
			Hosted = null;
		}
	}

	public void BindCWWindow(UINetBinding CWObj)
	{
		if (Hosted == null)
		{
			LinkHosted(CWObj);
			if (!DesignMode)
			{
				Application.DoEvents();
				RedrawToolbar();
				SetSize();
				HostHandle = base.Handle;
				Application.DoEvents();
			}
		}
	}

	private void SetToolbarColors()
	{
		if (ToolStripManager.Renderer is ToolStripProfessionalRenderer)
		{
			ProfessionalColorTable colorTable = ((ToolStripProfessionalRenderer)ToolStripManager.Renderer).ColorTable;
			SetToolbarGradientFromColor(colorTable.ToolStripGradientBegin);
			SetToolbarGradientToColor(colorTable.ToolStripGradientEnd);
			SetToolbarColor(colorTable.ToolStripGradientMiddle);
		}
		else
		{
			SetToolbarGradientFromColor(Color.FromArgb(227, 239, 255));
			SetToolbarGradientToColor(Color.FromArgb(177, 211, 255));
			SetToolbarColor(Color.FromArgb(111, 157, 217));
		}
		SetToolbarHeight(ToolbarService.DocumentHeight);
		SetToolbarIconSize(ToolbarService.DocumentIconSize);
	}

	private void SetListColors()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (ToolStripManager.Renderer is ToolStripProfessionalRenderer)
		{
			ToolStripProfessionalRenderer toolStripProfessionalRenderer = ToolStripManager.Renderer as ToolStripProfessionalRenderer;
			if (toolStripProfessionalRenderer.ColorTable is IListCustomColor)
			{
				IListCustomColor val = (IListCustomColor)toolStripProfessionalRenderer.ColorTable;
				SetListBkgrdColor(val.Background);
				SetListTextColor(val.Text);
				SetListSelectedBkgrdColor(val.BarActiveBackground);
				SetListSelectedTextColor(val.BarActiveText);
			}
			else
			{
				SetListBkgrdColor(SystemColors.Window);
				SetListTextColor(SystemColors.WindowText);
				SetListSelectedBkgrdColor(SystemColors.Highlight);
				SetListSelectedTextColor(SystemColors.HighlightText);
			}
		}
		else
		{
			SetListBkgrdColor(SystemColors.Window);
			SetListTextColor(SystemColors.WindowText);
			SetListSelectedBkgrdColor(SystemColors.Highlight);
			SetListSelectedTextColor(SystemColors.HighlightText);
		}
	}

	public void CloseInnerWindow()
	{
		UnlinkHosted();
	}

	protected virtual void RegisterNetEvents()
	{
		base.HandleDestroyed += HostWindow_HandleDestroyed;
		base.VisibleChanged += HostWindow_VisibleChanged;
		base.EnabledChanged += HostWindow_EnabledChanged;
		base.Move += HostWindow_Move;
		base.Resize += HostWindow_Resize;
		base.Validating += HostWindow_Validating;
		base.Invalidated += HostWindow_Invalidated;
		base.Enter += HostWindow_Enter;
		base.GotFocus += HostWindow_GotFocus;
		base.LostFocus += HostWindow_LostFocus;
		WorkbenchSingleton.LayoutChanging += HostWindow_LayoutChanging;
	}

	protected virtual void UnregisterNetEvents()
	{
		WorkbenchSingleton.LayoutChanging -= HostWindow_LayoutChanging;
	}

	protected virtual void RegisterCWEvents()
	{
		if (Hosted != null && Invoker != null)
		{
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)2);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)3);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)4);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)7);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)5);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)6);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)10);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)8);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)12);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)13);
			((EventInvokerCPPBase)Invoker).Register(Hosted, (UIControlEvents)14);
		}
	}

	protected virtual void UnRegisterCWEvents()
	{
		if (Hosted != null && Invoker != null)
		{
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)2);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)3);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)4);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)7);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)5);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)6);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)10);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)8);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)12);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)13);
			((EventInvokerCPPBase)Invoker).UnRegister(Hosted, (UIControlEvents)14);
		}
	}

	public override void Dispatch(UIControlEvents ev)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected I4, but got Unknown
		switch (ev - 2)
		{
		case 0:
			Hosted_OnDisconnect();
			break;
		case 1:
			Hosted_OnWindowOpened();
			break;
		case 2:
			Hosted_OnCloseWindow();
			break;
		case 8:
			Hosted_OnDirtyChanged();
			break;
		case 10:
			Hosted_OnSelectParent();
			break;
		case 11:
			Hosted_OnActivateParent();
			break;
		case 7:
			Hosted_OnDesignModeSelectParent();
			break;
		case 3:
		case 4:
		case 5:
		case 6:
		case 9:
			break;
		}
	}

	public override void DispatchLong(UIControlEvents ev, int v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)ev == 5)
		{
			SetWindowVisible(v == 0);
		}
		else if ((int)ev == 6)
		{
			SetWindowEnabled(v == 0);
		}
	}

	public override void DispatchString(UIControlEvents ev, string s)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)ev == 7)
		{
			SetCaption(s);
		}
	}

	public override void DispatchString2(UIControlEvents ev, string s1, string s2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)ev == 14)
		{
			ShowHelp(s1, s2);
		}
	}

	public override void DispatchLong2(UIControlEvents ev, int v1, int v2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)ev == 8)
		{
			Hosted_OnNotifyWindowNewSize(v1, v2);
		}
	}

	protected virtual void Hosted_OnDisconnect()
	{
		if (this.DisconnectFromHosted != null)
		{
			this.DisconnectFromHosted(this);
		}
	}

	protected virtual void Hosted_OnWindowOpened()
	{
		Application.DoEvents();
		if (this.WindowOpened != null)
		{
			this.WindowOpened(this);
		}
		base.Visible = true;
		if (base.ParentForm != null)
		{
			base.ParentForm.Visible = true;
			base.ParentForm.Activate();
		}
		Select();
		SetFocusOnChild();
		ready = true;
		NotifyReady();
	}

	protected virtual void Hosted_OnCloseWindow()
	{
		base.ParentForm.Select();
		if (this.WindowClosing != null)
		{
			this.WindowClosing(this);
		}
	}

	protected virtual void Hosted_OnDirtyChanged()
	{
		if (this.IsDirtyChanged != null)
		{
			this.IsDirtyChanged(this);
		}
	}

	protected virtual void SetWindowVisible(bool on)
	{
		Form parentForm = base.ParentForm;
		if (!on && parentForm != null)
		{
			parentForm.Visible = on;
		}
		base.Visible = on;
		if (on && parentForm != null)
		{
			parentForm.Visible = on;
		}
	}

	protected virtual void SetWindowEnabled(bool on)
	{
		base.Enabled = on;
	}

	protected virtual void SetCaption(string txt)
	{
		_HostedWindowCaption = txt;
		if (this.CaptionChanged != null)
		{
			this.CaptionChanged(this);
		}
	}

	protected virtual void ShowHelp(string helpfile, string topic)
	{
		if (File.Exists(helpfile))
		{
			Help.ShowHelp(WorkbenchSingleton.helpHost, helpfile, HelpNavigator.Topic, topic);
		}
		else
		{
			MessageService.ShowWarning("${res:MainWindow.Windows.HtmlHelp.NotFound} " + helpfile + "/n" + topic);
		}
	}

	protected virtual void Hosted_OnNotifyWindowNewSize(int neww, int newh)
	{
		if (this.NotifyNewSize != null)
		{
			Application.DoEvents();
			Size size = new Size(neww, newh);
			this.NotifyNewSize(this, size);
		}
	}

	protected virtual void Hosted_OnSelectParent()
	{
		base.ParentForm?.Select();
	}

	protected virtual void Hosted_OnActivateParent()
	{
		Application.DoEvents();
		base.ParentForm?.Activate();
		if (this.ActivateParent != null)
		{
			this.ActivateParent(this);
		}
	}

	protected virtual void Hosted_OnDesignModeSelectParent()
	{
		ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
		selectionService.SetSelectedComponents(new object[1] { this });
	}

	private void HostWindow_HandleDestroyed(object sender, EventArgs e)
	{
		base.HandleDestroyed -= HostWindow_HandleDestroyed;
		OnParentClosed();
	}

	public void OnParentClosed()
	{
		if (_ParentClosed)
		{
			return;
		}
		_ParentClosed = true;
		try
		{
			if (!DesignMode && Hosted != null)
			{
				RequestClose();
				CloseInnerWindow();
			}
		}
		catch
		{
		}
	}

	private void HostWindow_VisibleChanged(object sender, EventArgs e)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		if (!DesignMode && ready && Hosted != null)
		{
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)8);
			if (val != null)
			{
				val.ValueOf = base.Visible;
			}
		}
	}

	private void HostWindow_EnabledChanged(object sender, EventArgs e)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (!DesignMode && Hosted != null)
		{
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)9);
			if (val != null)
			{
				val.ValueOf = base.Enabled;
			}
		}
	}

	private void HostWindow_Move(object sender, EventArgs e)
	{
		if (!IsMinimized())
		{
			SetHostToHosted();
		}
	}

	private void HostWindow_Resize(object sender, EventArgs e)
	{
		if (!SuspendResize)
		{
			OnHostWindowResize();
		}
		else
		{
			DelayedResize = true;
		}
	}

	private void HostWindow_LayoutChanging(object sender, BoolEventArg on)
	{
		SuspendResize = on.Arg;
		if (!SuspendResize && DelayedResize)
		{
			DelayedResize = false;
			OnHostWindowResize();
		}
	}

	private void HostWindow_Validating(object sender, CancelEventArgs e)
	{
		if (!DesignMode && Hosted != null)
		{
			e.Cancel = !IsValid;
		}
	}

	private void HostWindow_Invalidated(object sender, InvalidateEventArgs e)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (!DesignMode && Hosted != null && base.Visible && !IsMinimized())
		{
			SetToolbarColors();
			SetListColors();
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)103);
			if (val != null)
			{
				val.ValueOf = true;
			}
		}
	}

	private void HostWindow_Enter(object sender, EventArgs e)
	{
		if (!DesignMode)
		{
			SetFocusOnChild();
		}
	}

	private void HostWindow_GotFocus(object sender, EventArgs e)
	{
		if (!DesignMode)
		{
			SetFocusOnChild();
		}
	}

	private void HostWindow_LostFocus(object sender, EventArgs e)
	{
		if (!DesignMode)
		{
			WindowLostFocus();
		}
	}

	private void SetSize()
	{
		if (IsModalDialog)
		{
			SetHostedToHost();
		}
		else
		{
			SetHostToHosted();
		}
	}

	protected virtual void SetHostedToHost()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		Size hostedWindowSize = GetHostedWindowSize();
		if (base.ParentForm != null)
		{
			base.ParentForm.ClientSize = hostedWindowSize;
		}
		base.ClientSize = hostedWindowSize;
		UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)22);
		int num = ((val == null) ? base.ParentForm.MaximumSize.Width : val.ValueOf);
		val = (UIIntegerProperty)Hosted.Property((UIControlProperties)23);
		int num2 = ((val == null) ? base.ParentForm.MaximumSize.Height : val.ValueOf);
		if (num != 0 || num2 != 0)
		{
			base.ParentForm.MinimumSize = new Size(num, num2);
		}
		val = (UIIntegerProperty)Hosted.Property((UIControlProperties)24);
		num = ((val == null) ? base.ParentForm.MaximumSize.Width : val.ValueOf);
		val = (UIIntegerProperty)Hosted.Property((UIControlProperties)25);
		num2 = ((val == null) ? base.ParentForm.MaximumSize.Height : val.ValueOf);
		if (num != 0 || num2 != 0)
		{
			base.ParentForm.MaximumSize = new Size(num, num2);
		}
	}

	protected virtual void SetHostToHosted()
	{
		SetHostedWindowRect(base.Location, base.ClientSize);
	}

	public void AttachToParentFormCloseEvent()
	{
		if (base.ParentForm != null)
		{
			base.ParentForm.FormClosed += ParentForm_FormClosed;
		}
	}

	private void ParentForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		if (base.ParentForm != null)
		{
			base.ParentForm.FormClosed -= ParentForm_FormClosed;
		}
		OnParentClosed();
	}

	public void ReOpenControl()
	{
	}

	public virtual void AcceptChanges()
	{
	}

	public void ExecuteCommand(int cmd)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)101);
			if (val != null)
			{
				val.ValueOf = cmd;
			}
		}
	}

	public void ExecuteCommand(CommandID cmd)
	{
		ExecuteCommand(cmd.ID);
	}

	public void RedrawToolbar()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (Hosted != null)
		{
			SetToolbarColors();
			SetListColors();
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)110);
			if (val != null)
			{
				val.ValueOf = true;
			}
		}
	}

	public void SetFocusOnChild()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (Hosted != null && base.CanFocus && !IsMinimized())
		{
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)105);
			if (val != null)
			{
				val.ValueOf = true;
			}
		}
	}

	public void WindowLostFocus()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)105);
			if (val != null)
			{
				val.ValueOf = false;
			}
		}
	}

	protected bool IsMinimized()
	{
		try
		{
			for (Form parentForm = base.ParentForm; parentForm != null; parentForm = parentForm.ParentForm)
			{
				if (parentForm.WindowState == FormWindowState.Minimized)
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	public void OnHostWindowResize()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		if (Hosted != null)
		{
			bool flag = IsMinimized();
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)107);
			if (val != null)
			{
				val.ValueOf = flag;
			}
			if (!flag)
			{
				SetHostedWindowRect(base.Location, base.ClientSize);
			}
		}
	}

	public void SetHostedWindowRect(Point pt, Size sz)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!DesignMode && Hosted != null)
		{
			UIRectProperty val = (UIRectProperty)Hosted.Property((UIControlProperties)30);
			if (val != null)
			{
				UIRect val2 = new UIRect();
				val2.X = 0;
				val2.Y = 0;
				val2.W = sz.Width;
				val2.H = sz.Height;
				val.ValueOf = val2;
				val2 = null;
			}
		}
	}

	public Size GetHostedWindowSize()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		Size result = default(Size);
		if (Hosted != null)
		{
			UIRectProperty val = (UIRectProperty)Hosted.Property((UIControlProperties)30);
			if (val != null)
			{
				UIRect valueOf = val.ValueOf;
				result.Width = valueOf.W;
				result.Height = valueOf.H;
				valueOf = null;
			}
		}
		return result;
	}

	private int GetClarionColor(Color value)
	{
		return int.Parse($"00{value.B:X2}{value.G:X2}{value.R:X2}", NumberStyles.HexNumber);
	}

	protected void SetToolbarColor(Color value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)111);
			if (val != null)
			{
				val.ValueOf = GetClarionColor(value);
			}
		}
	}

	protected void SetToolbarGradientFromColor(Color value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)112);
			if (val != null)
			{
				val.ValueOf = GetClarionColor(value);
			}
		}
	}

	protected void SetToolbarGradientToColor(Color value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)113);
			if (val != null)
			{
				val.ValueOf = GetClarionColor(value);
			}
		}
	}

	protected void SetToolbarHeight(int value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)114);
			if (val != null)
			{
				val.ValueOf = value;
			}
		}
	}

	protected void SetToolbarIconSize(int value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)115);
			if (val != null)
			{
				val.ValueOf = value;
			}
		}
	}

	protected void SetListBkgrdColor(Color value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)116);
			if (val != null)
			{
				val.ValueOf = GetClarionColor(value);
			}
		}
	}

	protected void SetListTextColor(Color value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)117);
			if (val != null)
			{
				val.ValueOf = GetClarionColor(value);
			}
		}
	}

	protected void SetListSelectedTextColor(Color value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)118);
			if (val != null)
			{
				val.ValueOf = GetClarionColor(value);
			}
		}
	}

	protected void SetListSelectedBkgrdColor(Color value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)119);
			if (val != null)
			{
				val.ValueOf = GetClarionColor(value);
			}
		}
	}

	protected void SetRequest(RequestType value)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)3);
			if (val != null)
			{
				val.ValueOf = (int)value;
			}
		}
	}

	protected RequestType GetRequest()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIIntegerProperty val = (UIIntegerProperty)Hosted.Property((UIControlProperties)3);
			if (val != null)
			{
				return (RequestType)val.ValueOf;
			}
		}
		return RequestType.None;
	}

	protected void SetDesignMode(bool value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)12);
			if (val != null)
			{
				val.ValueOf = value;
			}
		}
	}

	protected string GetStateString()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIStringProperty val = (UIStringProperty)Hosted.Property((UIControlProperties)6);
			if (val != null)
			{
				return val.ValueOf;
			}
		}
		return "NONE";
	}

	protected void NotifyReady()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hosted != null)
		{
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)108);
			if (val != null)
			{
				val.ValueOf = true;
			}
		}
	}

	[DllImport("kernel32.dll")]
	private static extern uint GetCurrentThreadId();

	public void RequestClose()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		if (Hosted == null)
		{
			return;
		}
		try
		{
			UIBooleanProperty val = (UIBooleanProperty)Hosted.Property((UIControlProperties)102);
			if (val != null)
			{
				val.ValueOf = true;
			}
		}
		catch (InvalidCastException)
		{
		}
	}
}

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Clarion.ASL;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Common.Controls;

namespace SoftVelocity.CWPInvoke;

public class CWDialogService : EventInvokerBase
{
	public delegate void ValidateViewHandler(UINetBinding CWObj, ref IViewContent content);

	public delegate void CreateHostEventHandler(UINetBinding CWObj, UIBindingInterfaceKind kind);

	protected UINetBinding CWDlgSrv;

	protected CWDialogStack DlgStack;

	private static CWDialogService _Instance;

	private bool _IsGenMsgWinOpen;

	private bool _IsGenMsgWinCanceled;

	public static CWDialogService Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = new CWDialogService();
			}
			return _Instance;
		}
	}

	[Browsable(false)]
	public bool IsGenMsgWinOpen
	{
		get
		{
			return _IsGenMsgWinOpen;
		}
		set
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			if (_IsGenMsgWinOpen == value)
			{
				return;
			}
			_IsGenMsgWinOpen = value;
			if (CWDlgSrv != null)
			{
				UIBooleanProperty val = (UIBooleanProperty)CWDlgSrv.Property((UIControlProperties)21);
				if (val != null)
				{
					val.ValueOf = _IsGenMsgWinOpen;
					return;
				}
			}
			throw new InvalidStateException("IsGenMsgWinOpen", InvokeKind.PropertySet);
		}
	}

	[Browsable(false)]
	public bool IsGenMsgWinCanceled
	{
		get
		{
			return _IsGenMsgWinCanceled;
		}
		set
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			if (_IsGenMsgWinCanceled == value)
			{
				return;
			}
			_IsGenMsgWinCanceled = value;
			if (CWDlgSrv != null)
			{
				UIBooleanProperty val = (UIBooleanProperty)CWDlgSrv.Property((UIControlProperties)20);
				if (val != null)
				{
					val.ValueOf = _IsGenMsgWinCanceled;
					return;
				}
			}
			throw new InvalidStateException("IsGenMsgWinCanceled", InvokeKind.PropertySet);
		}
	}

	public event ValidateViewHandler ValidateView;

	public event CreateHostEventHandler CreateHost;

	public event EventHandler<HostedWindowEventArgs> HostedWindowOpening;

	public event EventHandler<HostedWindowEventArgs> HostedWindowClosed;

	public CWDialogService()
		: base(null)
	{
		CWDlgSrv = null;
		DlgStack = new CWDialogStack();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			UnRegisterCWEvents();
			DlgStack.Dispose();
			DlgStack = null;
			CWDlgSrv.Dispose();
			CWDlgSrv = null;
		}
		((EventInvokerCPPBase)this).Dispose(disposing);
	}

	private void InternalStart()
	{
		if (WorkbenchSingleton.MainForm != null)
		{
			((EventInvokerCPPBase)this).wnd = WorkbenchSingleton.MainForm;
		}
		else
		{
			WorkbenchSingleton.WorkbenchCreated += OnWorkbenchCreated;
		}
		SetServer((IntPtr)(object)Commands.GetCWDialogService());
		if (CWDlgSrv == null)
		{
			throw new TypeInitializationException("CWDialogService", new ExternalException("The object was not found by ASL"));
		}
	}

	public static void Start()
	{
		WorkbenchSingleton.WorkbenchCreated += OnWorkbenchSingleton_WorkbenchCreated;
	}

	private static void OnWorkbenchSingleton_WorkbenchCreated(object sender, EventArgs e)
	{
		WorkbenchSingleton.WorkbenchCreated -= OnWorkbenchSingleton_WorkbenchCreated;
		Instance.InternalStart();
	}

	public static void Stop()
	{
		if (_Instance != null)
		{
			((EventInvokerCPPBase)Instance).Dispose();
			_Instance = null;
		}
	}

	private void OnWorkbenchCreated(object sender, EventArgs args)
	{
		WorkbenchSingleton.WorkbenchCreated -= OnWorkbenchCreated;
		((EventInvokerCPPBase)this).wnd = WorkbenchSingleton.MainForm;
	}

	private void SetServer(IntPtr srv)
	{
		if (CWDlgSrv != null)
		{
			UnRegisterCWEvents();
		}
		CWDlgSrv = UINetBinding.MakeDialogService(srv);
		if (CWDlgSrv != null)
		{
			RegisterCWEvents();
		}
	}

	protected virtual void RegisterCWEvents()
	{
		if (CWDlgSrv != null)
		{
			((EventInvokerCPPBase)this).Register(CWDlgSrv, (UIControlEvents)1);
			((EventInvokerCPPBase)this).Register(CWDlgSrv, (UIControlEvents)14);
			((EventInvokerCPPBase)this).Register(CWDlgSrv, (UIControlEvents)15);
			((EventInvokerCPPBase)this).Register(CWDlgSrv, (UIControlEvents)20);
			((EventInvokerCPPBase)this).Register(CWDlgSrv, (UIControlEvents)21);
			((EventInvokerCPPBase)this).Register(CWDlgSrv, (UIControlEvents)22);
			((EventInvokerCPPBase)this).Register(CWDlgSrv, (UIControlEvents)23);
			((EventInvokerCPPBase)this).Register(CWDlgSrv, (UIControlEvents)24);
		}
	}

	protected virtual void UnRegisterCWEvents()
	{
		if (CWDlgSrv != null)
		{
			((EventInvokerCPPBase)this).UnRegister(CWDlgSrv, (UIControlEvents)1);
			((EventInvokerCPPBase)this).UnRegister(CWDlgSrv, (UIControlEvents)14);
			((EventInvokerCPPBase)this).UnRegister(CWDlgSrv, (UIControlEvents)15);
			((EventInvokerCPPBase)this).UnRegister(CWDlgSrv, (UIControlEvents)20);
			((EventInvokerCPPBase)this).UnRegister(CWDlgSrv, (UIControlEvents)21);
			((EventInvokerCPPBase)this).UnRegister(CWDlgSrv, (UIControlEvents)22);
			((EventInvokerCPPBase)this).UnRegister(CWDlgSrv, (UIControlEvents)23);
			((EventInvokerCPPBase)this).UnRegister(CWDlgSrv, (UIControlEvents)24);
		}
	}

	protected override void Dispatch(UIControlEvents ev)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)ev == 23)
		{
			CloseGenMsgWindow();
		}
	}

	protected override void DispatchLong(UIControlEvents ev, int v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if ((int)ev == 20)
		{
			StartGenMsgWindow(v != 0);
		}
		else if ((int)ev == 24)
		{
			HideGenMsgWindow(v != 0);
		}
	}

	protected override void DispatchString(UIControlEvents ev, string s)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)ev == 15)
		{
			WriteToOutput(s);
		}
	}

	protected override void DispatchString2(UIControlEvents ev, string s1, string s2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)ev == 14)
		{
			ShowHelp(s1, s2);
		}
	}

	protected override void DispatchLongString(UIControlEvents ev, int v, string s)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)ev == 21)
		{
			SetGenMsgText(s, v);
		}
		else if ((int)ev == 22)
		{
			SetGenMsgTitle(s, v != 0);
		}
	}

	protected override void DispatchBinding(UIControlEvents ev, UINetBinding b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((int)ev == 1 && b != null)
		{
			if (!OpenWindow(b))
			{
				UIStringProperty val = (UIStringProperty)b.Property((UIControlProperties)13);
				string text = ((val != null) ? val.ValueOf : "<<<Unknown>>>");
				LoggingService.Debug((object)("Error on setting interface: " + text));
				throw new Exception("Error on setting interface: " + text);
			}
			LoggingService.Debug((object)("CWDialogService After OpenWindow: " + b.UIKind()));
		}
	}

	public void WriteToOutput(string Txt)
	{
	}

	public void ShowHelp(string helpfile, string topic)
	{
		CWDllInterop.ShowHelp(helpfile, topic);
	}

	private bool TestCancel()
	{
		if (!IsGenMsgWinOpen)
		{
			return false;
		}
		IsGenMsgWinCanceled = GenMsgWindow.CheckGenCancel();
		return _IsGenMsgWinCanceled;
	}

	public void StartGenMsgWindow(bool EnableCancel)
	{
		IsGenMsgWinCanceled = false;
		GenMsgWindow.StartGenMsg(EnableCancel);
		IsGenMsgWinOpen = true;
	}

	public void CloseGenMsgWindow()
	{
		GenMsgWindow.CloseGenMsgWin();
		TestCancel();
		IsGenMsgWinOpen = false;
	}

	public void HideGenMsgWindow(bool on)
	{
		if (IsGenMsgWinOpen && !IsGenMsgWinCanceled)
		{
			GenMsgWindow.HideGenMsgWin(on);
		}
	}

	public void SetGenMsgText(string Txt, int WhichLine)
	{
		if (!TestCancel())
		{
			IsGenMsgWinCanceled = false;
		}
		GenMsgWindow.SetGenMsgText(Txt, WhichLine);
		IsGenMsgWinOpen = true;
		TestCancel();
	}

	public void SetGenMsgTitle(string caption, bool debugmode)
	{
		if (!TestCancel())
		{
			IsGenMsgWinCanceled = false;
		}
		GenMsgWindow.SetGenMsgTitle(caption, debugmode);
		IsGenMsgWinOpen = true;
		TestCancel();
	}

	private bool OpenWindow(UINetBinding CWObj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected I4, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			UIBindingInterfaceKind val = CWObj.UIKind();
			LoggingService.Debug((object)("CWDialogService OpenWindow kind = " + val));
			UIBindingInterfaceKind val2 = val;
			switch ((int)val2)
			{
			case 0:
				return false;
			case 4:
				DlgStack.RunDialog(CWObj);
				break;
			case 5:
				OpenViewContent(CWObj);
				break;
			default:
				OpenSpecialWindow(CWObj, val);
				break;
			case 1:
			case 2:
			case 3:
				break;
			}
		}
		catch
		{
			LoggingService.Debug((object)("CWDialogService OpenWindow FAIL !!! kind = " + CWObj.UIKind()));
			return false;
		}
		CWObj.Release();
		LoggingService.Debug((object)("CWDialogService OpenWindow OK!!! kind = " + CWObj.UIKind()));
		return true;
	}

	public void OpenViewContent(UINetBinding CWObj)
	{
		IViewContent content = null;
		if (this.ValidateView != null)
		{
			this.ValidateView(CWObj, ref content);
		}
		if (content != null && content == WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent)
		{
			CWDialogViewContent.OpenCWDialog(CWObj);
		}
	}

	public void OpenSpecialWindow(UINetBinding CWObj, UIBindingInterfaceKind kind)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (this.CreateHost != null)
		{
			this.CreateHost(CWObj, kind);
		}
	}

	internal void OnHostedWindowOpening(string IID)
	{
		if (this.HostedWindowOpening != null)
		{
			this.HostedWindowOpening(null, new HostedWindowEventArgs(IID));
		}
	}

	internal void OnHostedWindowClosed(string IID)
	{
		if (this.HostedWindowClosed != null)
		{
			this.HostedWindowClosed(null, new HostedWindowEventArgs(IID));
		}
	}
}

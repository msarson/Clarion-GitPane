using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class ExtendedWebBrowser : WebBrowser
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	[TypeLibType(TypeLibTypeFlags.FHidden)]
	[Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D")]
	private interface DWebBrowserEvents2
	{
		[DispId(273)]
		void NewWindow3([In][MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In][Out] ref bool cancel, [In] ref object flags, [In][MarshalAs(UnmanagedType.BStr)] ref string urlContext, [In][MarshalAs(UnmanagedType.BStr)] ref string url);
	}

	private class WebBrowserExtendedEvents : StandardOleMarshalObject, DWebBrowserEvents2
	{
		private ExtendedWebBrowser browser;

		public WebBrowserExtendedEvents(ExtendedWebBrowser browser)
		{
			this.browser = browser;
		}

		public void NewWindow3(object pDisp, ref bool cancel, ref object flags, ref string urlContext, ref string url)
		{
			NewWindowExtendedEventArgs e = new NewWindowExtendedEventArgs(new Uri(url));
			browser.OnNewWindowExtended(e);
			cancel = e.Cancel;
		}
	}

	private const int WM_KEYFIRST = 256;

	private const int WM_KEYLAST = 264;

	private const int WM_KEYDOWN = 256;

	private AxHost.ConnectionPointCookie cookie;

	private WebBrowserExtendedEvents wevents;

	public event NewWindowExtendedEventHandler NewWindowExtended;

	protected override void CreateSink()
	{
		base.CreateSink();
		wevents = new WebBrowserExtendedEvents(this);
		cookie = new AxHost.ConnectionPointCookie(base.ActiveXInstance, wevents, typeof(DWebBrowserEvents2));
	}

	protected override void DetachSink()
	{
		try
		{
			if (cookie != null)
			{
				cookie.Disconnect();
				cookie = null;
			}
			base.DetachSink();
		}
		catch (Exception)
		{
		}
	}

	protected virtual void OnNewWindowExtended(NewWindowExtendedEventArgs e)
	{
		if (this.NewWindowExtended != null)
		{
			this.NewWindowExtended(this, e);
		}
	}

	public override bool PreProcessMessage(ref Message m)
	{
		bool flag = false;
		if (!base.WebBrowserShortcutsEnabled && m.Msg >= 256 && m.Msg <= 264 && m.Msg == 256)
		{
			flag = ProcessCmdKey(ref m, (Keys)((int)m.WParam | (int)Control.ModifierKeys));
		}
		if (!flag)
		{
			flag = base.PreProcessMessage(ref m);
		}
		return flag;
	}
}

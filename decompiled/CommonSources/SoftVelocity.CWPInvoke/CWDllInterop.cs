using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.CWPInvoke;

public sealed class CWDllInterop
{
	public const int WM_ACTIVATE = 6;

	public const int WM_ACTIVATEAPP = 28;

	public const int WM_NCACTIVATE = 134;

	private static IntPtr _IDEHandle = IntPtr.Zero;

	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern int SendMessage(IntPtr handle, int msg, int wParam, IntPtr lParam);

	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern int SendMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern int PostMessage(IntPtr handle, int msg, int wParam, IntPtr lParam);

	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern int PostMessage(IntPtr handle, int msg, int wParam, uint lParam);

	[DllImport("user32", SetLastError = true)]
	public static extern bool BringWindowToTop(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

	[DllImport("user32")]
	public static extern IntPtr SetFocus(IntPtr hWnd);

	public static void ShowHelp(string helpFile, string windowId)
	{
	}

	public static IntPtr GetIDEFrameHandle()
	{
		if (_IDEHandle == IntPtr.Zero)
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				return WorkbenchSingleton.SafeThreadFunction<IntPtr>((Func<IntPtr>)GetIDEFrameHandle);
			}
			Form mainForm = WorkbenchSingleton.MainForm;
			if (mainForm != null)
			{
				_IDEHandle = mainForm.Handle;
			}
			else
			{
				_IDEHandle = IntPtr.Zero;
			}
		}
		return _IDEHandle;
	}
}

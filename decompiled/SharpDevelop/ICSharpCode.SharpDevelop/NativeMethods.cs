using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop;

internal static class NativeMethods
{
	public const int WM_SETREDRAW = 11;

	public const int WM_USER = 1024;

	public const int WM_GETICON = 127;

	private static readonly IntPtr FALSE = new IntPtr(0);

	private static readonly IntPtr TRUE = new IntPtr(1);

	[DllImport("user32.dll")]
	public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

	public static void SetWindowRedraw(IntPtr hWnd, bool allowRedraw)
	{
		SendMessage(hWnd, 11, allowRedraw ? TRUE : FALSE, IntPtr.Zero);
	}

	[DllImport("user32.dll", ExactSpelling = true)]
	private static extern short GetKeyState(int vKey);

	public static bool IsKeyPressed(Keys key)
	{
		return GetKeyState((int)key) < 0;
	}
}

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ListViewExtensions
{
	public struct HDITEM
	{
		[Flags]
		public enum Mask
		{
			Format = 4
		}

		[Flags]
		public enum Format
		{
			SortDown = 0x200,
			SortUp = 0x400
		}

		public Mask mask;

		public int cxy;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszText;

		public IntPtr hbm;

		public int cchTextMax;

		public Format fmt;

		public IntPtr lParam;

		public int iImage;

		public int iOrder;

		public uint type;

		public IntPtr pvFilter;

		public uint state;
	}

	public const int LVM_FIRST = 4096;

	public const int LVM_GETHEADER = 4127;

	public const int HDM_FIRST = 4608;

	public const int HDM_GETITEM = 4619;

	public const int HDM_SETITEM = 4620;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref HDITEM lParam);

	public static void SetSortIcon(this ListView listViewControl, int columnIndex, SortOrder order)
	{
		IntPtr hWnd = SendMessage(listViewControl.Handle, 4127u, IntPtr.Zero, IntPtr.Zero);
		for (int i = 0; i <= listViewControl.Columns.Count - 1; i++)
		{
			IntPtr wParam = new IntPtr(i);
			HDITEM lParam = new HDITEM
			{
				mask = HDITEM.Mask.Format
			};
			if (SendMessage(hWnd, 4619u, wParam, ref lParam) == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			if (order != SortOrder.None && i == columnIndex)
			{
				switch (order)
				{
				case SortOrder.Descending:
					lParam.fmt &= ~HDITEM.Format.SortDown;
					lParam.fmt |= HDITEM.Format.SortUp;
					break;
				case SortOrder.Ascending:
					lParam.fmt &= ~HDITEM.Format.SortUp;
					lParam.fmt |= HDITEM.Format.SortDown;
					break;
				}
			}
			else
			{
				lParam.fmt &= ~(HDITEM.Format.SortDown | HDITEM.Format.SortUp);
			}
			if (SendMessage(hWnd, 4620u, wParam, ref lParam) == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
		}
	}
}

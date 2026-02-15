using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public static class RightToLeftConverter
{
	public static string[] RightToLeftLanguages = new string[4] { "ar", "he", "fa", "urdu" };

	public static bool IsRightToLeft
	{
		get
		{
			string[] rightToLeftLanguages = RightToLeftLanguages;
			foreach (string value in rightToLeftLanguages)
			{
				if (ResourceService.Language.StartsWith(value))
				{
					return true;
				}
			}
			return false;
		}
	}

	private static AnchorStyles Mirror(AnchorStyles anchor)
	{
		bool flag = (anchor & AnchorStyles.Right) == AnchorStyles.Right;
		bool flag2 = (anchor & AnchorStyles.Left) == AnchorStyles.Left;
		anchor = ((!flag) ? (anchor & ~AnchorStyles.Left) : (anchor | AnchorStyles.Left));
		anchor = ((!flag2) ? (anchor & ~AnchorStyles.Right) : (anchor | AnchorStyles.Right));
		return anchor;
	}

	private static Point MirrorLocation(Control control)
	{
		return new Point(control.Parent.ClientSize.Width - control.Left - control.Width, control.Top);
	}

	private static void Mirror(Control control)
	{
		switch (control.Dock)
		{
		case DockStyle.Left:
			control.Dock = DockStyle.Right;
			break;
		case DockStyle.Right:
			control.Dock = DockStyle.Left;
			break;
		case DockStyle.None:
			control.Anchor = Mirror(control.Anchor);
			control.Location = MirrorLocation(control);
			break;
		}
		if (control.RightToLeft != RightToLeft.Yes)
		{
			return;
		}
		foreach (Control control2 in control.Controls)
		{
			Mirror(control2);
		}
	}

	public static void Convert(Control control)
	{
		if (IsRightToLeft)
		{
			if (control.RightToLeft != RightToLeft.Yes)
			{
				control.RightToLeft = RightToLeft.Yes;
			}
		}
		else if (control.RightToLeft == RightToLeft.Yes)
		{
			control.RightToLeft = RightToLeft.No;
		}
		ConvertLayout(control);
	}

	private static void ConvertLayout(Control control)
	{
		bool isRightToLeft = IsRightToLeft;
		Form form = control as Form;
		ListView listView = control as ListView;
		ProgressBar progressBar = control as ProgressBar;
		TabControl tabControl = control as TabControl;
		TrackBar trackBar = control as TrackBar;
		TreeView treeView = control as TreeView;
		if (form != null && form.RightToLeftLayout != isRightToLeft)
		{
			form.RightToLeftLayout = isRightToLeft;
		}
		if (listView != null && listView.RightToLeftLayout != isRightToLeft)
		{
			listView.RightToLeftLayout = isRightToLeft;
		}
		if (progressBar != null && progressBar.RightToLeftLayout != isRightToLeft)
		{
			progressBar.RightToLeftLayout = isRightToLeft;
		}
		if (tabControl != null && tabControl.RightToLeftLayout != isRightToLeft)
		{
			tabControl.RightToLeftLayout = isRightToLeft;
		}
		if (trackBar != null && trackBar.RightToLeftLayout != isRightToLeft)
		{
			trackBar.RightToLeftLayout = isRightToLeft;
		}
		if (treeView != null && treeView.RightToLeftLayout != isRightToLeft)
		{
			treeView.RightToLeftLayout = isRightToLeft;
		}
	}

	private static void ConvertLayoutRecursive(Control control)
	{
		bool isRightToLeft = IsRightToLeft;
		if (isRightToLeft != (control.RightToLeft == RightToLeft.Yes))
		{
			return;
		}
		ConvertLayout(control);
		foreach (Control control2 in control.Controls)
		{
			ConvertLayoutRecursive(control2);
		}
	}

	public static void ConvertRecursive(Control control)
	{
		if (IsRightToLeft != (control.RightToLeft == RightToLeft.Yes))
		{
			ReConvertRecursive(control);
		}
	}

	public static void ReConvertRecursive(Control control)
	{
		Convert(control);
		foreach (Control control6 in control.Controls)
		{
			ConvertLayoutRecursive(control6);
		}
		if (!IsRightToLeft)
		{
			return;
		}
		if (control is Form)
		{
			foreach (Control control7 in control.Controls)
			{
				foreach (Control control8 in control7.Controls)
				{
					Mirror(control8);
				}
			}
			return;
		}
		foreach (Control control9 in control.Controls)
		{
			Mirror(control9);
		}
	}
}

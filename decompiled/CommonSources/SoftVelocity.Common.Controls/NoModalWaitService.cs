using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.Controls;

public class NoModalWaitService
{
	private static Control parent;

	private static WaitForm fo;

	public static void OpenTransparent()
	{
		OpenTransparent(WorkbenchSingleton.MainForm);
	}

	public static void Open()
	{
		Open(WorkbenchSingleton.MainForm);
	}

	public static void OpenTransparent(Form parent)
	{
		Open(parent, transparent: true);
	}

	public static void Open(Form parent)
	{
		Open(parent, transparent: false);
	}

	private static void Open(Form parent, bool transparent)
	{
		if (NoModalWaitService.parent == null)
		{
			NoModalWaitService.parent = parent;
			fo = new WaitForm();
			fo.MainFrame = parent;
			fo.Show();
		}
	}

	public static void Close()
	{
		if (parent != null)
		{
			fo.Close();
			fo = null;
			parent = null;
		}
	}
}

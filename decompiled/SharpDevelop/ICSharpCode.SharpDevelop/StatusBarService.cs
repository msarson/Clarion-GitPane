using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public static class StatusBarService
{
	private static SdStatusBar statusBar = null;

	private static IProgressNotificationCenter monitor = null;

	private static IProgressNotificationCenter dummyMonitor = null;

	private static bool wasError = false;

	private static string lastMessage = "";

	public static bool Visible
	{
		get
		{
			if (statusBar != null)
			{
				return statusBar.GetVisible();
			}
			return false;
		}
		set
		{
			if (statusBar != null)
			{
				statusBar.SetVisible(value);
			}
		}
	}

	public static Control Control => statusBar;

	public static IProgressNotificationCenter ProgressMonitor
	{
		get
		{
			if (monitor == null)
			{
				if (statusBar == null)
				{
					if (dummyMonitor == null)
					{
						dummyMonitor = new DummyProgressMonitor();
					}
					return dummyMonitor;
				}
				return statusBar;
			}
			return monitor;
		}
		set
		{
			monitor = value;
		}
	}

	internal static void Initialize()
	{
		statusBar = new SdStatusBar();
	}

	public static void ClearCaretancursorText()
	{
		if (statusBar != null)
		{
			statusBar.ClearCaretancursorText();
		}
	}

	public static void SetCaretPosition(int x, int y, int charOffset)
	{
		if (statusBar != null)
		{
			statusBar.SetCaretPositionText(StringParser.Format("${res:StatusBarService.CursorStatusBarPanelTextLine}", y + 1), StringParser.Format("${res:StatusBarService.CursorStatusBarPanelTextCol}", x + 1), StringParser.Format("${res:StatusBarService.CursorStatusBarPanelTextChar}", charOffset + 1));
		}
	}

	public static void SetInsertMode(bool insertMode)
	{
		if (statusBar != null)
		{
			statusBar.SetInsertModeText(insertMode ? StringParser.Parse("${res:StatusBarService.CaretModes.Insert}") : StringParser.Parse("${res:StatusBarService.CaretModes.Overwrite}"));
		}
	}

	public static void ShowErrorMessage(string message)
	{
		if (statusBar != null)
		{
			statusBar.ShowErrorMessage(StringParser.Parse(message));
		}
	}

	public static void ClearMessage()
	{
		if (statusBar != null)
		{
			lastMessage = string.Empty;
			statusBar.SetMessage(string.Empty);
		}
	}

	public static void SetMessage(string message)
	{
		string text = StringParser.Parse(message);
		if (statusBar != null && lastMessage != text)
		{
			lastMessage = text;
			statusBar.SetMessage(text);
		}
	}

	public static void SetMessage(Image image, string message)
	{
		if (statusBar != null)
		{
			statusBar.SetMessage(image, StringParser.Parse(message));
		}
	}

	public static void SetMessage(string message, bool highlighted)
	{
		if (statusBar != null)
		{
			statusBar.SetMessage(message, highlighted);
		}
	}

	public static void SetMessage(string message, bool highlighted, bool force)
	{
		if (statusBar != null)
		{
			statusBar.SetMessage(message, highlighted, force);
		}
	}

	public static void RedrawStatusbar()
	{
		if (wasError)
		{
			ShowErrorMessage(lastMessage);
		}
		else
		{
			SetMessage(lastMessage);
		}
		Visible = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.StatusBarVisible", defaultValue: true);
	}

	public static void Update()
	{
	}
}

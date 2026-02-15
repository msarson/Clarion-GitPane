using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.ClarionNet.Designer;
using SoftVelocity.ClarionNet.Designer.SectionControls;
using SoftVelocity.ClarionNet.WindowDesigner;
using SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

namespace SoftVelocity.Common.ClarionEditor;

public class WindowKeyHandler : IMessageFilter
{
	private class CommandWrapper
	{
		private CommandID commandID;

		private bool restoreSelection;

		public CommandID CommandID => commandID;

		public bool RestoreSelection => restoreSelection;

		public CommandWrapper(CommandID commandID)
			: this(commandID, restoreSelection: false)
		{
		}

		public CommandWrapper(CommandID commandID, bool restoreSelection)
		{
			this.commandID = commandID;
			this.restoreSelection = restoreSelection;
		}
	}

	private const int keyPressedMessage = 256;

	private const int leftMouseButtonDownMessage = 514;

	private Hashtable keyTable = new Hashtable();

	public static bool inserted;

	public static void Insert()
	{
		inserted = true;
		System.Windows.Forms.Application.AddMessageFilter(new WindowKeyHandler());
	}

	public static bool SwapMessageFilters(IMessageFilter f, IMessageFilter w)
	{
		System.Windows.Forms.Application.RemoveMessageFilter(f);
		System.Windows.Forms.Application.AddMessageFilter(w);
		System.Windows.Forms.Application.AddMessageFilter(f);
		return true;
	}

	public WindowKeyHandler()
	{
		keyTable[Keys.Left] = new CommandWrapper(MenuCommands.KeyMoveLeft);
		keyTable[Keys.Right] = new CommandWrapper(MenuCommands.KeyMoveRight);
		keyTable[Keys.Up] = new CommandWrapper(MenuCommands.KeyMoveUp);
		keyTable[Keys.Down] = new CommandWrapper(MenuCommands.KeyMoveDown);
		keyTable[Keys.Tab] = new CommandWrapper(MenuCommands.KeySelectNext);
		keyTable[Keys.Delete] = new CommandWrapper(StandardCommands.Delete);
		keyTable[Keys.Back] = new CommandWrapper(StandardCommands.Delete);
		keyTable[Keys.Left | Keys.Shift] = new CommandWrapper(MenuCommands.KeySizeWidthDecrease);
		keyTable[Keys.Right | Keys.Shift] = new CommandWrapper(MenuCommands.KeySizeWidthIncrease);
		keyTable[Keys.Up | Keys.Shift] = new CommandWrapper(MenuCommands.KeySizeHeightDecrease);
		keyTable[Keys.Down | Keys.Shift] = new CommandWrapper(MenuCommands.KeySizeHeightIncrease);
		keyTable[Keys.Tab | Keys.Shift] = new CommandWrapper(MenuCommands.KeySelectPrevious);
		keyTable[Keys.Delete | Keys.Shift] = new CommandWrapper(StandardCommands.Delete);
		keyTable[Keys.Back | Keys.Shift] = new CommandWrapper(StandardCommands.Delete);
		keyTable[Keys.Left | Keys.Control] = new CommandWrapper(MenuCommands.KeyNudgeLeft);
		keyTable[Keys.Right | Keys.Control] = new CommandWrapper(MenuCommands.KeyNudgeRight);
		keyTable[Keys.Up | Keys.Control] = new CommandWrapper(MenuCommands.KeyNudgeUp);
		keyTable[Keys.Down | Keys.Control] = new CommandWrapper(MenuCommands.KeyNudgeDown);
		keyTable[Keys.Left | Keys.Shift | Keys.Control] = new CommandWrapper(MenuCommands.KeyNudgeWidthDecrease);
		keyTable[Keys.Right | Keys.Shift | Keys.Control] = new CommandWrapper(MenuCommands.KeyNudgeWidthIncrease);
		keyTable[Keys.Up | Keys.Shift | Keys.Control] = new CommandWrapper(MenuCommands.KeyNudgeHeightDecrease);
		keyTable[Keys.Down | Keys.Shift | Keys.Control] = new CommandWrapper(MenuCommands.KeyNudgeHeightIncrease);
	}

	public bool PreFilterMessage(ref Message m)
	{
		if (m.Msg != 256)
		{
			return false;
		}
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent.Control == null || !WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent.Control.ContainsFocus)
		{
			return false;
		}
		if (!(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is CommonClarionDesignerView commonClarionDesignerView))
		{
			return false;
		}
		if (!commonClarionDesignerView.IsFormsDesignerVisible)
		{
			return false;
		}
		Keys keys = (Keys)(m.WParam.ToInt32() | (int)Control.ModifierKeys);
		if (keys == Keys.Escape && commonClarionDesignerView.IsTabOrderMode)
		{
			commonClarionDesignerView.HideTabOrder();
			return true;
		}
		switch (keys)
		{
		case Keys.F2:
		{
			Duplicate duplicate = new Duplicate();
			if (((AbstractMenuCommand)duplicate).IsEnabled)
			{
				((AbstractCommand)duplicate).Run();
			}
			return true;
		}
		case Keys.Left:
		case Keys.Up:
		case Keys.Right:
		case Keys.Down:
			if (commonClarionDesignerView != null && commonClarionDesignerView.IsPropertybarFocused())
			{
				return false;
			}
			break;
		}
		if ((keys == Keys.Delete || keys == Keys.Back || keys == Keys.Tab) && commonClarionDesignerView != null && commonClarionDesignerView.IsPropertybarFocused())
		{
			return false;
		}
		switch (keys)
		{
		case Keys.Return:
			if (commonClarionDesignerView != null)
			{
				if (commonClarionDesignerView.IsPropertybarFocused())
				{
					return false;
				}
				PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(PropertyPad));
				if (pad != null)
				{
					pad.BringPadToFront();
				}
			}
			return true;
		case Keys.Escape:
		{
			IBackToSourceCompatible backToSourceCompatible = ((WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null) ? null : (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as IBackToSourceCompatible));
			if (backToSourceCompatible != null)
			{
				backToSourceCompatible.BackToSource();
				return true;
			}
			break;
		}
		}
		Report report = ((!(commonClarionDesignerView.Host.RootComponent is BaseDesignerControl baseDesignerControl)) ? null : baseDesignerControl.ReportControl);
		GeneralDesiner generalDesiner = commonClarionDesignerView.Host.RootComponent as GeneralDesiner;
		CommandWrapper commandWrapper = (CommandWrapper)keyTable[keys];
		if (commandWrapper != null)
		{
			LoggingService.Debug((object)("Run menu command: " + commandWrapper.CommandID));
			_ = WorkbenchSingleton.ActiveControl;
			IMenuCommandService menuCommandService = (IMenuCommandService)commonClarionDesignerView.Host.GetService(typeof(IMenuCommandService));
			ISelectionService selectionService = (ISelectionService)commonClarionDesignerView.Host.GetService(typeof(ISelectionService));
			ICollection selectedComponents = selectionService.GetSelectedComponents();
			if (selectedComponents.Count == 1)
			{
				foreach (object item in selectedComponents)
				{
					if (item is IComponent)
					{
						IComponent activeComponent = (IComponent)item;
						if (HandleMenuCommand(commonClarionDesignerView, activeComponent, keys))
						{
							return false;
						}
					}
				}
			}
			string unitName = string.Empty;
			bool flag = false;
			bool flag2 = false;
			bool isRefreshRequired = false;
			if (commandWrapper.CommandID == StandardCommands.Copy)
			{
				flag2 = true;
				if (report != null)
				{
					flag = report.PreCopyActions();
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PreCopyActions();
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Cut)
			{
				flag2 = true;
				if (report != null)
				{
					flag = report.PreCutActions();
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PreCutActions();
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Delete)
			{
				flag2 = true;
				if (!((IClipboardHandler)commonClarionDesignerView).EnableDelete)
				{
					return true;
				}
				if (report != null)
				{
					flag = report.PreDeleteActions(ref isRefreshRequired);
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PreDeleteActions(ref isRefreshRequired);
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Paste)
			{
				flag2 = true;
				if (!((IClipboardHandler)commonClarionDesignerView).EnablePaste)
				{
					return true;
				}
				if (report != null)
				{
					flag = report.PrePasteActions();
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PrePasteActions();
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Redo)
			{
				flag2 = true;
				if (report != null)
				{
					flag = report.PreRedoActions();
					if (report.BaseDesignerControl.IsInAppGen)
					{
						unitName = report.UndoEngine.GetTopUnitName(isUndo: false);
					}
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PreRedoActions();
					if (generalDesiner.IsInAppGen)
					{
						unitName = generalDesiner.UndoEngine.GetTopUnitName(isUndo: false);
					}
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Undo)
			{
				flag2 = true;
				if (report != null)
				{
					flag = report.PreUndoActions();
					if (report.BaseDesignerControl.IsInAppGen)
					{
						unitName = report.UndoEngine.GetTopUnitName(isUndo: true);
					}
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PreUndoActions();
					if (generalDesiner.IsInAppGen)
					{
						unitName = generalDesiner.UndoEngine.GetTopUnitName(isUndo: true);
					}
				}
			}
			if (!flag2 || (flag2 && flag))
			{
				menuCommandService.GlobalInvoke(commandWrapper.CommandID);
			}
			if (commandWrapper.CommandID == StandardCommands.Copy)
			{
				if (report != null)
				{
					flag = report.PostCopyActions();
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PostCopyActions();
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Cut)
			{
				if (report != null)
				{
					flag = report.PostCutActions();
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PostCutActions();
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Delete)
			{
				if (report != null)
				{
					flag = report.PostDeleteActions();
					if (isRefreshRequired)
					{
						commonClarionDesignerView.RefreshPads();
					}
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PostDeleteActions();
					if (isRefreshRequired)
					{
						commonClarionDesignerView.RefreshPads();
					}
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Paste)
			{
				if (report != null)
				{
					flag = report.PostPasteActions();
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PostPasteActions();
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Redo)
			{
				if (report != null)
				{
					flag = report.PostRedoActions(unitName, ref isRefreshRequired);
					if (isRefreshRequired)
					{
						commonClarionDesignerView.RefreshPads();
					}
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PostRedoActions(unitName, ref isRefreshRequired);
					if (isRefreshRequired)
					{
						commonClarionDesignerView.RefreshPads();
					}
				}
			}
			else if (commandWrapper.CommandID == StandardCommands.Undo)
			{
				if (report != null)
				{
					flag = report.PostUndoActions(unitName, ref isRefreshRequired);
					if (isRefreshRequired)
					{
						commonClarionDesignerView.RefreshPads();
					}
				}
				else if (generalDesiner != null)
				{
					flag = generalDesiner.PostUndoActions(unitName, ref isRefreshRequired);
					if (isRefreshRequired)
					{
						commonClarionDesignerView.RefreshPads();
					}
				}
			}
			if (commandWrapper.RestoreSelection)
			{
				selectionService.SetSelectedComponents(selectedComponents);
			}
			return true;
		}
		return false;
	}

	private bool HandleMenuCommand(FormsDesignerViewContent formDesigner, IComponent activeComponent, Keys keyPressed)
	{
		Assembly assembly = typeof(WindowsFormsDesignerOptionService).Assembly;
		Type type = assembly.GetType("System.Windows.Forms.Design.ToolStripKeyboardHandlingService");
		object service = formDesigner.Host.GetService(type);
		if (service == null)
		{
			LoggingService.Debug((object)"no ToolStripKeyboardHandlingService found");
			return false;
		}
		if (activeComponent is ToolStripItem)
		{
			switch (keyPressed)
			{
			case Keys.Up:
				type.InvokeMember("ProcessUpDown", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, service, new object[1] { false });
				return true;
			case Keys.Down:
				type.InvokeMember("ProcessUpDown", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, service, new object[1] { true });
				return true;
			}
		}
		if ((bool)type.InvokeMember("TemplateNodeActive", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty, null, service, null))
		{
			return true;
		}
		return false;
	}
}

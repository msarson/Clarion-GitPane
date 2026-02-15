using System;
using System.ComponentModel.Design;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public abstract class AbstractWindowDesignerCommand : AbstractMenuCommand
{
	public abstract CommandID CommandID { get; }

	protected CommonClarionDesignerView WindowDesignerView
	{
		get
		{
			IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
			if (activeWorkbenchWindow == null)
			{
				return null;
			}
			return activeWorkbenchWindow.ActiveViewContent as CommonClarionDesignerView;
		}
	}

	protected IDesignerHost WindowDesignerHost
	{
		get
		{
			if (WindowDesignerView != null)
			{
				return WindowDesignerView.Host;
			}
			return null;
		}
	}

	protected virtual bool CanExecuteCommand(IDesignerHost host)
	{
		return true;
	}

	public override void Run()
	{
		try
		{
			IDesignerHost windowDesignerHost = WindowDesignerHost;
			if (windowDesignerHost != null && CanExecuteCommand(windowDesignerHost))
			{
				IMenuCommandService menuCommandService = (IMenuCommandService)windowDesignerHost.GetService(typeof(IMenuCommandService));
				menuCommandService.GlobalInvoke(CommandID);
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}
}

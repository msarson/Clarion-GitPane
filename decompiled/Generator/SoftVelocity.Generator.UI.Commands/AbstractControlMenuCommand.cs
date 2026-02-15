using System;
using System.ComponentModel.Design;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.UI.Commands;

public abstract class AbstractControlMenuCommand : AbstractMenuCommand
{
	private CWControl_ViewContent _ViewContent
	{
		get
		{
			IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
			if (activeWorkbenchWindow != null && activeWorkbenchWindow.ActiveViewContent is CWControl_ViewContent)
			{
				return (CWControl_ViewContent)(object)activeWorkbenchWindow.ActiveViewContent;
			}
			return null;
		}
	}

	public override bool IsEnabled
	{
		get
		{
			if (_ViewContent != null)
			{
				return true;
			}
			return false;
		}
	}

	public abstract CommandID CommandID { get; }

	public virtual string Description => "Command";

	public override void Run()
	{
		try
		{
			_ViewContent?.CommandInvoke(CommandID);
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}
}

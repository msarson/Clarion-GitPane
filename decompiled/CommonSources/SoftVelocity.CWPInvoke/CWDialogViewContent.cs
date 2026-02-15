using System.ComponentModel;
using System.Windows.Forms;
using Clarion.ASL;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator.UI;

namespace SoftVelocity.CWPInvoke;

internal class CWDialogViewContent : AbstractSecondaryViewContent
{
	private CWDialogViewHost dialogControl;

	private IAppTitleManager AppTitleManager;

	private int previousViewNumber;

	private int currentViewNumber;

	private CWDialogViewHost CWDialog
	{
		get
		{
			if (dialogControl == null)
			{
				dialogControl = new CWDialogViewHost(this);
			}
			return dialogControl;
		}
	}

	public override Control Control => CWDialog;

	public override bool Visible => false;

	public override void Dispose()
	{
		dialogControl = null;
	}

	public override void SwitchedTo()
	{
		if (dialogControl != null)
		{
			CWDialog.OnHostWindowResize();
		}
	}

	private void ConnectToCWWindow(UINetBinding CWObj, IAppTitleManager titleman)
	{
		AppTitleManager = titleman;
		CWDialog.OpenNewControl(CWObj);
	}

	internal void OnWindowOpened()
	{
		((AbstractBaseViewContent)this).WorkbenchWindow.ClosingEvent += WorkbenchWindow_OnClosing;
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.SwitchView(currentViewNumber);
		if (AppTitleManager != null)
		{
			AppTitleManager.AppendHeaderTitle(CWDialog.HostedWindowCaption);
		}
	}

	internal void OnCaptionChanged(string txt)
	{
		if (AppTitleManager != null)
		{
			AppTitleManager.ReplaceHeaderTitle(txt);
		}
	}

	internal void OnWindowClosed()
	{
		((AbstractBaseViewContent)this).WorkbenchWindow.ClosingEvent -= WorkbenchWindow_OnClosing;
		if (AppTitleManager != null)
		{
			AppTitleManager.RemoveCurrentHeaderTitle();
		}
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.SwitchView(previousViewNumber);
	}

	private void WorkbenchWindow_OnClosing(object sender, CancelEventArgs e)
	{
		if (WorkbenchSingleton.Workbench.ActiveContent == this && dialogControl != null)
		{
			dialogControl.WorkbenchWindow_ClosingEvent(e);
		}
		else
		{
			e.Cancel = true;
		}
	}

	public static void OpenCWDialog(UINetBinding CWObj)
	{
		IViewContent viewContent = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent;
		int num = -1;
		CWDialogViewContent cWDialogViewContent = null;
		for (int i = 0; i < viewContent.SecondaryViewContents.Count; i++)
		{
			if (viewContent.SecondaryViewContents[i] is CWDialogViewContent)
			{
				cWDialogViewContent = (CWDialogViewContent)(object)viewContent.SecondaryViewContents[i];
				num = (cWDialogViewContent.currentViewNumber = i + 1);
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		IBaseViewContent activeViewContent = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent;
		cWDialogViewContent.previousViewNumber = 0;
		if ((object)activeViewContent != viewContent)
		{
			for (int j = 0; j < viewContent.SecondaryViewContents.Count; j++)
			{
				if ((object)viewContent.SecondaryViewContents[j] == activeViewContent)
				{
					cWDialogViewContent.previousViewNumber = j + 1;
				}
			}
		}
		cWDialogViewContent.ConnectToCWWindow(CWObj, viewContent as IAppTitleManager);
	}
}

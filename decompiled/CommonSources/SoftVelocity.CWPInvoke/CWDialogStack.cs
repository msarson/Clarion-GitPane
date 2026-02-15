using System;
using System.Windows.Forms;
using Clarion.ASL;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.CWPInvoke;

public class CWDialogStack : IDisposable
{
	private Form _prev;

	private bool _kill;

	public void Dispose()
	{
		_kill = true;
		while (_prev != null)
		{
			_prev.Close();
		}
	}

	internal void RunDialog(UINetBinding CWObj)
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		if (CWObj == null || _kill)
		{
			return;
		}
		string iID = CWObj.IID();
		CWDialogForm cWDialogForm = new CWDialogForm();
		cWDialogForm.PreviousForm = _prev;
		_prev = cWDialogForm;
		cWDialogForm.BindDialogWindow(CWObj);
		WorkbenchSingleton.MainForm.Activate();
		CWDialogService.Instance.OnHostedWindowOpening(iID);
		Application.DoEvents();
		cWDialogForm.Visible = false;
		try
		{
			try
			{
				cWDialogForm.ShowDialog(WorkbenchSingleton.MainForm);
				cWDialogForm.UnbindDialogWindow();
				CWDialogService.Instance.OnHostedWindowClosed(iID);
				_prev = cWDialogForm.PreviousForm;
				cWDialogForm.PreviousForm = null;
				cWDialogForm.Dispose();
				cWDialogForm = null;
			}
			catch (Exception)
			{
			}
			if (_kill)
			{
				return;
			}
			if (_prev != null)
			{
				_prev.Activate();
				return;
			}
			try
			{
				if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow is Form form)
				{
					form.Activate();
					form.Focus();
				}
				Control control = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent.Control;
				control.Focus();
				control.Select();
				if (control is ICWWindowContainer iCWWindowContainer)
				{
					iCWWindowContainer.SetFocusOnChild();
				}
			}
			catch
			{
			}
		}
		finally
		{
			UIBooleanProperty val = (UIBooleanProperty)CWObj.Property((UIControlProperties)109);
			try
			{
				if (val != null)
				{
					val.ValueOf = true;
				}
			}
			catch
			{
			}
			cWDialogForm?.Dispose();
			CWObj.Release();
		}
	}
}

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Clarion.ASL;
using Clarion.GEN;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Internal.Templates;
using SoftVelocity.CWPInvoke;

namespace SoftVelocity.Generator.UI;

internal class NewAppWizardPanel : AbstractWizardPanel
{
	private Win32App _NewApp;

	private NewAppOptionsControl _NewAppOptionsControl;

	private string appFileName = string.Empty;

	private bool created;

	private bool _NewAppCancelled = true;

	public NewAppWizardPanel()
	{
		((AbstractWizardPanel)this).IsLastPanel = true;
		((AbstractWizardPanel)this).EnableNext = false;
		((AbstractWizardPanel)this).EnablePrevious = false;
		((AbstractOptionPanel)this).EnableFinish = false;
		InitializeComponent();
		((AbstractOptionPanel)this).CustomizationObjectChanged += NewAppWizardPanel_CustomizationObjectChanged;
	}

	private void NewAppWizardPanel_CustomizationObjectChanged(object sender, EventArgs e)
	{
		if (created)
		{
			return;
		}
		created = true;
		if (((AbstractOptionPanel)this).CustomizationObject == null)
		{
			throw new Exception("This is supposed be called from a project template.");
		}
		object customizationObject = ((AbstractOptionPanel)this).CustomizationObject;
		Properties val = (Properties)((customizationObject is Properties) ? customizationObject : null);
		if (val != null)
		{
			ProjectCreateInformation val2 = val.Get<ProjectCreateInformation>("ProjectCreateInformation", (ProjectCreateInformation)null);
			ProjectTemplate val3 = val.Get<ProjectTemplate>("ProjectTemplate", (ProjectTemplate)null);
			appFileName = Path.Combine(val2.ProjectBasePath, val2.ProjectName + ".app");
			((Control)this).ParentChanged += NewAppWizardPanel_ParentChanged;
			CWDialogService.Instance.CreateHost += Instance_HostWindowOpen;
			_NewApp = ApplicationService.NewApp(appFileName, val3.LanguageName);
			if (_NewApp != null)
			{
				((AbstractWizardPanel)this).EnableCancel = false;
				return;
			}
			CWDialogService.Instance.CreateHost -= Instance_HostWindowOpen;
			created = false;
			MessageService.ShowError("An error happen!");
		}
	}

	private void NewAppWizardPanel_ParentChanged(object sender, EventArgs e)
	{
		if (((ContainerControl)this).ParentForm == null)
		{
			return;
		}
		((Control)this).ParentChanged -= NewAppWizardPanel_ParentChanged;
		((ContainerControl)this).ParentForm.FormClosing += ParentForm_FormClosing;
		((ContainerControl)this).ParentForm.ControlBox = false;
		((ContainerControl)this).ParentForm.Text = "New Application";
		foreach (Control control in ((ContainerControl)this).ParentForm.Controls)
		{
			if (control.Text == ResourceService.GetString("Global.BackButtonText") || control.Text == ResourceService.GetString("Global.NextButtonText"))
			{
				control.Hide();
			}
		}
	}

	private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		((ContainerControl)this).ParentForm.FormClosing -= ParentForm_FormClosing;
		CancelNewApp();
		if (_NewApp != null)
		{
			_NewApp.WindowClosed();
		}
	}

	private void CancelNewApp()
	{
		if (_NewAppCancelled && created && _NewAppOptionsControl != null)
		{
			_NewAppOptionsControl.RequestClose();
		}
	}

	private void InitializeComponent()
	{
		((Control)this).SuspendLayout();
		((ScrollableControl)(object)this).AutoScroll = true;
		((Control)this).Name = "NewAppWizardPanel";
		((Control)this).ResumeLayout(performLayout: false);
	}

	public override bool ReceiveDialogMessage(DialogMessage message)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if (_NewAppOptionsControl != null && (int)message == 5)
		{
			_NewAppCancelled = false;
			_NewAppOptionsControl.AcceptChanges();
			ApplicationService.PushApplication(appFileName);
			return true;
		}
		return ((AbstractOptionPanel)this).ReceiveDialogMessage(message);
	}

	private void Instance_HostWindowOpen(UINetBinding CWObj, UIBindingInterfaceKind kind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)kind == 1)
		{
			_NewAppOptionsControl = new NewAppOptionsControl();
			((Control)this).SuspendLayout();
			_NewAppOptionsControl.Location = new Point(0, 40);
			_NewAppOptionsControl.Name = "_NewAppOptionsControl";
			_NewAppOptionsControl.Dock = DockStyle.Fill;
			_NewAppOptionsControl.Size = new Size(501, 265);
			((Control)this).Controls.Add(_NewAppOptionsControl);
			((Control)this).ResumeLayout(performLayout: true);
			CWDialogService.Instance.CreateHost -= Instance_HostWindowOpen;
			((AbstractOptionPanel)this).EnableFinish = true;
			((AbstractWizardPanel)this).EnableCancel = true;
		}
	}
}

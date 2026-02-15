using System.Windows.Forms;
using Clarion.GEN;
using Clarion.PRJ;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator.PWEE;

namespace SoftVelocity.Generator.UI;

internal class GeneratorBinding : IGeneratorBinding
{
	public IGeneratorDialog OpenReportFormatter(string name, IFormatter generator)
	{
		return GetApplicationMainWindowControl_ViewContent()?.OpenReportFormatter(name, generator);
	}

	public IGeneratorDialog OpenWindowFormatter(string name, IFormatter generator)
	{
		return GetApplicationMainWindowControl_ViewContent()?.OpenWindowFormatter(name, generator);
	}

	public IGeneratorEditorDialog OpenEmbedEditor(string name, IEmbedEditorDetails generator)
	{
		return GetApplicationMainWindowControl_ViewContent(generator.AppName)?.OpenEmbedEditor(name, generator);
	}

	public IGeneratorEditorDialog OpenWindowReportEditor(string name, IEmbedEditorDetails generator)
	{
		return GetApplicationMainWindowControl_ViewContent(generator.AppName)?.OpenWindowReportEditor(name, generator);
	}

	public IGeneratorEditorDialog OpenFileEditor(string name, bool readOnly, uint initialLine, IEditorDetails generator)
	{
		ApplicationMainWindowControl_ViewContent applicationMainWindowControl_ViewContent = GetApplicationMainWindowControl_ViewContent();
		if (applicationMainWindowControl_ViewContent != null)
		{
			return applicationMainWindowControl_ViewContent.OpenFileEditor(name, readOnly, initialLine, generator);
		}
		OpenFileEditorDocument(name, readOnly, initialLine);
		return null;
	}

	private void OpenFileEditorDocument(string name, bool readOnly, uint initialLine)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall<string, bool, uint>((Action<string, bool, uint>)OpenFileEditorDocument, name, readOnly, initialLine);
			return;
		}
		System.Windows.Forms.Application.DoEvents();
		FileService.OpenFile(name);
		IViewContent val = FileService.JumpToFilePosition(name, (int)(initialLine - 1), 0);
		WorkbenchSingleton.MainForm.Select();
		((IBaseViewContent)val).Control.Focus();
	}

	public IGeneratorEditorDialog OpenPwee(IPweeDetails generator)
	{
		return GetApplicationMainWindowControl_ViewContent(generator.AppName)?.OpenPwee(generator);
	}

	public ApplicationMainWindowControl_ViewContent GetApplicationMainWindowControl_ViewContent(string appname)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<string, ApplicationMainWindowControl_ViewContent>((Func<string, ApplicationMainWindowControl_ViewContent>)GetApplicationMainWindowControl_ViewContent, appname);
		}
		ApplicationService.AllowClearErrors(value: false);
		ApplicationService.EditApplication(appname);
		return GetApplicationMainWindowControl_ViewContent();
	}

	private ApplicationMainWindowControl_ViewContent GetApplicationMainWindowControl_ViewContent()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		System.Windows.Forms.Application.DoEvents();
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null)
		{
			return null;
		}
		ApplicationMainWindowControl_ViewContent applicationMainWindowControl_ViewContent = activeWorkbenchWindow.ViewContent as ApplicationMainWindowControl_ViewContent;
		if (applicationMainWindowControl_ViewContent != null)
		{
			while ((int)applicationMainWindowControl_ViewContent.ActiveWindowInterface == 0)
			{
				System.Windows.Forms.Application.DoEvents();
			}
		}
		ApplicationService.AllowClearErrors(value: true);
		return applicationMainWindowControl_ViewContent;
	}

	public PRJFile GetProjectFile(string appName)
	{
		PRJFile appProject = null;
		string appLanguage = string.Empty;
		ApplicationService.GetApplicationProjectFile(appName, out appProject, out appLanguage);
		return appProject;
	}

	public string GetProjectFileName(string appName)
	{
		return ApplicationService.ProjectFileName(appName);
	}

	public IGeneratorDialog OpenFormDesigner(string name, AppgenSymbols appsymbols)
	{
		return GetApplicationMainWindowControl_ViewContent()?.OpenFormDesigner(name, appsymbols);
	}
}

using System;
using Clarion.GEN;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.UI;

public class ApplicationEditorDisplayBinding : IDisplayBinding
{
	public bool CanCreateContentForFile(string fileName)
	{
		return ApplicationService.IsValidApplicationFile(fileName);
	}

	public IViewContent CreateContentForFile(string fileName)
	{
		if (ApplicationService.IsTemplateRegistryOpen)
		{
			MessageService.ShowMessage(ResourceService.GetString("Clarion.Generator.Error.RegistryInEdit"));
			return null;
		}
		if (!ApplicationService.GetCanOpenEditor(fileName))
		{
			if (ApplicationService.AreApplicationOnEdit)
			{
				MessageService.ShowMessage(ResourceService.GetString("Clarion.Generator.Error.CloseApplicationBefore"));
				return null;
			}
			ProjectService.LoadSolutionOrProject(fileName);
		}
		bool flag = true;
		do
		{
			flag = true;
			foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				if (item != null && item.FileName == fileName && item is ApplicationMainWindowControl_ViewContent)
				{
					if (((IBaseViewContent)item).WorkbenchWindow != null)
					{
						((IBaseViewContent)item).WorkbenchWindow.SelectWindow();
						return item;
					}
					WorkbenchSingleton.Workbench.ViewContentCollection.Remove(item);
					((IDisposable)item).Dispose();
					flag = false;
					break;
				}
			}
		}
		while (!flag);
		ApplicationService.CanOpenEditor = false;
		ApplicationMainWindowControl_ViewContent applicationMainWindowControl_ViewContent = new ApplicationMainWindowControl_ViewContent();
		try
		{
			((AbstractViewContent)applicationMainWindowControl_ViewContent).Load(fileName);
		}
		catch (ApplicationServiceException ex)
		{
			ApplicationService.SetText(ex.ApplicationName);
			ApplicationService.SetText(GeneratorError.AppLoadFailed);
			applicationMainWindowControl_ViewContent = null;
		}
		finally
		{
			ApplicationService.CanOpenEditor = true;
		}
		return (IViewContent)(object)applicationMainWindowControl_ViewContent;
	}

	public bool CanCreateContentForLanguage(string languageName)
	{
		return false;
	}

	public IViewContent CreateContentForLanguage(string languageName, string content)
	{
		throw new NotImplementedException();
	}
}

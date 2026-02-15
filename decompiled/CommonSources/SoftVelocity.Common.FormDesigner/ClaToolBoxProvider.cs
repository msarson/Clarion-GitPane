using System;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.FormsDesigner;
using ICSharpCode.FormsDesigner.Gui;
using ICSharpCode.FormsDesigner.Services;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Widgets.SideBar;
using SoftVelocity.Common.ClarionEditor;

namespace SoftVelocity.Common.FormDesigner;

public class ClaToolBoxProvider
{
	public static bool RemoveSelectedToolUsedHandler(ClaDesignerGenerator.FormDesignerModeenum mode)
	{
		switch (mode)
		{
		case ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner:
		case ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner:
			ToolboxProvider.RemoveSelectedToolUsedHandler();
			ToolboxProvider.ToolboxService.SelectedItemUsed += SelectedToolUsedHandler;
			break;
		case ClaDesignerGenerator.FormDesignerModeenum.Standart:
			ToolboxProvider.RemoveSelectedToolUsedHandler();
			ToolboxProvider.ToolboxService.SelectedItemUsed += DesktopSelectedToolUsedHandler;
			break;
		}
		return true;
	}

	public static bool RemoveNewSelectedToolUsedHandler(ClaDesignerGenerator.FormDesignerModeenum mode)
	{
		switch (mode)
		{
		case ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner:
		case ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner:
			ToolboxProvider.ToolboxService.SelectedItemUsed -= SelectedToolUsedHandler;
			break;
		case ClaDesignerGenerator.FormDesignerModeenum.Standart:
			ToolboxProvider.ToolboxService.SelectedItemUsed -= DesktopSelectedToolUsedHandler;
			break;
		}
		return true;
	}

	private static void SelectedToolUsedHandler(object sender, EventArgs e)
	{
		SideTab activeTab = ((SideBarControl)SharpDevelopSideBar.SideBar).ActiveTab;
		if (activeTab.Items.Count > 0)
		{
			activeTab.ChoosedItem = activeTab.Items[0];
		}
		((Control)(object)SharpDevelopSideBar.SideBar).Refresh();
	}

	public static void DesktopSelectedToolUsedHandler(object sender, EventArgs e)
	{
		if (!(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is FormsDesignerViewContent) || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is CommonClarionDesignerView)
		{
			return;
		}
		SideTab activeTab = ((SideBarControl)SharpDevelopSideBar.SideBar).ActiveTab;
		if (sender != null && sender is ToolboxService)
		{
			ToolboxItem selectedToolboxItem = (sender as IToolboxService).GetSelectedToolboxItem();
			if (activeTab is CustomComponentsSideTab)
			{
				ToolboxProvider.SelectedToolUsedHandler(sender, e);
				return;
			}
			if (selectedToolboxItem != null && selectedToolboxItem.AssemblyName != null)
			{
				IProject currentProject = ProjectService.CurrentProject;
				IProjectContent currentProjectContent = ParserService.CurrentProjectContent;
				if (currentProject != null && currentProjectContent != null && !ProjectContainsHiddenReference(currentProject, currentProjectContent, selectedToolboxItem.AssemblyName))
				{
					ToolboxProvider.SelectedToolUsedHandler(sender, e);
					return;
				}
			}
		}
		if (activeTab.Items.Count > 0)
		{
			activeTab.ChoosedItem = activeTab.Items[0];
		}
		((Control)(object)SharpDevelopSideBar.SideBar).Refresh();
	}

	public static bool ProjectContainsHiddenReference(IProject project, IProjectContent currentProjectContent, AssemblyName referenceName)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		IProject currentProject = ProjectService.CurrentProject;
		bool flag = false;
		if (currentProject is MSBuildBasedProject)
		{
			string evaluatedProperty = ((MSBuildBasedProject)currentProject).GetEvaluatedProperty("noAutoRef");
			if (evaluatedProperty != null && evaluatedProperty.ToUpperInvariant() == "TRUE")
			{
				flag = true;
			}
		}
		if (flag)
		{
			return false;
		}
		foreach (IProjectContent referencedContent in currentProjectContent.ReferencedContents)
		{
			ReflectionProjectContent val = (ReflectionProjectContent)(object)((referencedContent is ReflectionProjectContent) ? referencedContent : null);
			if (val != null && val.AssemblyFullName == referenceName.FullName)
			{
				result = true;
				break;
			}
		}
		return result;
	}
}

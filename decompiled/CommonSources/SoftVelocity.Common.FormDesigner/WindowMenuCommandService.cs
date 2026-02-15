using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ICSharpCode.Core;
using SoftVelocity.ClarionNet.Designer;
using SoftVelocity.ClarionNet.Designer.SectionControls;

namespace SoftVelocity.Common.FormDesigner;

internal class WindowMenuCommandService : MenuCommandService
{
	private IServiceProvider m_serviceProvider;

	private Control panel;

	public WindowMenuCommandService(Control panel, IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		this.panel = panel;
		m_serviceProvider = serviceProvider;
	}

	public override bool GlobalInvoke(CommandID commandID)
	{
		if (commandID == MenuCommands.KeySelectNext)
		{
			ISelectionService selectionService = (ISelectionService)m_serviceProvider.GetService(typeof(ISelectionService));
			if (selectionService.PrimarySelection is ReportSectionObjectControlBase reportSectionObjectControlBase)
			{
				if (reportSectionObjectControlBase.IsPageView)
				{
					reportSectionObjectControlBase.SelectNextControlByTab(selectionService, isForward: true);
					return true;
				}
			}
			else if (selectionService.PrimarySelection is PageViewReportControl)
			{
				((PageViewReportControl)selectionService.PrimarySelection).SelectNextControlByTab(selectionService, isForward: true);
				return true;
			}
		}
		return base.GlobalInvoke(commandID);
	}

	protected override void Dispose(bool disposing)
	{
		panel = null;
		m_serviceProvider = null;
		base.Dispose(disposing);
	}

	public override void ShowContextMenu(CommandID menuID, int x, int y)
	{
		string text = "/ClaWindow/ContextMenus/";
		if (menuID == MenuCommands.ContainerMenu)
		{
			ISelectionService selectionService = (ISelectionService)m_serviceProvider.GetService(typeof(ISelectionService));
			if (selectionService.PrimarySelection is BaseDesignerControl)
			{
				text = "/ClaReport/ContextMenus/";
				if (!((BaseDesignerControl)selectionService.PrimarySelection).IsBandView())
				{
					text += "PageLayoutView/";
				}
			}
			text += "ContainerMenu";
		}
		else
		{
			if (menuID != MenuCommands.SelectionMenu)
			{
				throw new Exception();
			}
			ISelectionService selectionService2 = (ISelectionService)m_serviceProvider.GetService(typeof(ISelectionService));
			if (selectionService2.PrimarySelection is DBPanelBase || selectionService2.PrimarySelection is ReportSectionObjectControlBase)
			{
				text = "/ClaReport/ContextMenus/SectionMenu";
				if (selectionService2.PrimarySelection is DBPanelBase dBPanelBase)
				{
					if (dBPanelBase.IsPageView())
					{
						text = "/ClaReport/ContextMenus/PageLayoutView/SectionMenu";
					}
				}
				else if (selectionService2.PrimarySelection is ReportSectionObjectControlBase { IsPageView: not false })
				{
					text = "/ClaReport/ContextMenus/PageLayoutView/SectionMenu";
				}
			}
			else
			{
				text = ((!(selectionService2.PrimarySelection is PageViewReportControl)) ? (text + "SelectionMenu") : "/ClaReport/ContextMenus/PageLayoutView/ContainerMenu");
			}
		}
		Point point = panel.PointToClient(new Point(x, y));
		MenuService.CreateContextMenu((object)this, text)?.Show(panel, new Point(point.X, point.Y));
	}
}

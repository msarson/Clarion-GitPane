using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Windows.Forms;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;
using SoftVelocity.ClarionNet.Generator;
using SoftVelocity.ClarionNet.ReportDesigner.OptionPanels;
using SoftVelocity.ClarionNet.WindowDesigner;
using SoftVelocity.ClarionNet.WindowDesigner.OptionPanels;
using SoftVelocity.Common.ClarionEditor;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Generator.Pads;

namespace SoftVelocity.Generator.Editor;

public class CommonClarionGenDesignerView : CommonClarionDesignerView, IGeneratorDialog
{
	private IFormatter iRequester;

	private readonly bool appGenDesigner;

	public bool IsAppGenDesigner => appGenDesigner;

	public IFormatter FormatterRequester => iRequester;

	public CommonClarionGenDesignerView(IViewContent viewContent, IDesignerLoaderProvider loaderProvider, IDesignerGenerator generator, bool isWin, bool appGenDesigner)
		: base(viewContent, loaderProvider, generator, isWin)
	{
		this.appGenDesigner = appGenDesigner;
	}

	public bool ShowDesigner(ControlContainer rcd, CompilerResults cr, bool isWindowDesigner, bool isWindowWindow, IFormatter irequester)
	{
		iRequester = irequester;
		if (ShowDesigner(rcd, cr, isWindowDesigner, isWindowWindow, IsTemplate: true) && IsAppGenDesigner)
		{
			PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(ControlTemplatesPad));
			if (pad != null)
			{
				if (isWindowDesigner ? (!WindowGeneralOptionsPanel.OpenControlTemplates) : (!ReportGeneralOptionsPanel.OpenControlTemplates))
				{
					pad.CreatePad();
				}
				else
				{
					pad.BringPadToFront();
				}
				if (pad.PadContent is ControlTemplatesPad controlTemplatesPad)
				{
					controlTemplatesPad.RefreshTemplates(FormatterRequester);
					if (base.BuildSideTab != null)
					{
						base.BuildSideTab.SideTab.ChoosedItemChanged += ControlTemplatesPad.SelectedTabItemChanged;
					}
					if (base.IsReportDesigner)
					{
						if (base.ReportDesignerControl != null)
						{
							base.ReportDesignerControl.GetDragDropDataObject = ControlTemplatesPad.GetDragDropDataObject;
							base.ReportDesignerControl.TemplatePopulated = ControlTemplatesPad.TemplatePopulated;
						}
					}
					else if (base.WindowDesignerControl != null)
					{
						base.WindowDesignerControl.GetDragDropDataObject = ControlTemplatesPad.GetDragDropDataObject;
						base.WindowDesignerControl.TemplatePopulated = ControlTemplatesPad.TemplatePopulated;
					}
				}
			}
			base.IsDirty = false;
			return true;
		}
		return false;
	}

	public override bool RefreshPads()
	{
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(ControlTemplatesPad));
		if (pad != null && pad.PadContent is ControlTemplatesPad controlTemplatesPad)
		{
			controlTemplatesPad.RefreshTemplates(FormatterRequester);
		}
		return true;
	}

	public override void CloseDesigner()
	{
		base.CloseDesigner();
		if (IsAppGenDesigner)
		{
			PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(ControlTemplatesPad));
			if (pad != null && pad.PadContent is ControlTemplatesPad controlTemplatesPad)
			{
				controlTemplatesPad.RefreshTemplates(null);
			}
		}
		if (base.BuildSideTab != null)
		{
			base.BuildSideTab.SideTab.ChoosedItemChanged -= ControlTemplatesPad.SelectedTabItemChanged;
		}
		iRequester.InformDialogClosed();
		iRequester = null;
	}

	protected override bool IsBackVisible()
	{
		return true;
	}

	protected override void WorkbenchWindow_ClosingEvent(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		if (WorkbenchSingleton.Workbench.ActiveContent == this)
		{
			BackToSource();
		}
	}

	protected override bool InitBeforeControls()
	{
		if (base.IsReportDesigner)
		{
			if (base.BaseReportDesignerControl != null && IsAppGenDesigner)
			{
				base.BaseReportDesignerControl.FormatterRequester = FormatterRequester;
			}
		}
		else if (base.WindowDesignerControl != null && IsAppGenDesigner)
		{
			base.WindowDesignerControl.FormatterRequester = FormatterRequester;
		}
		return true;
	}

	protected void BuildGenToolBarItems(string sName)
	{
		SharpDevelopSideBar sideBar = GetSideBar();
		bool flag = false;
		for (int num = ((SideBarControl)SideBarView.sideBar).Tabs.Count - 1; num > 0; num--)
		{
			SideTab val = ((SideBarControl)SideBarView.sideBar).Tabs[num];
			if (val.Name == sName)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			BuildGenSideTab buildGenSideTab = new BuildGenSideTab(sideBar, base.IsReportDesigner, FormatterRequester, CommonClarionDesignerView.ParseControlString);
			buildGenSideTab.CreateSidetabs(base.Host);
		}
	}

	public override void Save()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Cursor.Current = Cursors.WaitCursor;
		string value = (base.IsReportDesigner ? base.ReportDesignerControl.ReportToFile(base.IndentStyle, "    ") : ClaWindowManager.GetWindowText(base.Host, base.IndentStyle, "    "));
		if (FormatterRequester != null)
		{
			try
			{
				FormatterRequester.SetData(value, save: true);
			}
			catch (StructureException)
			{
			}
		}
		Cursor.Current = Cursors.Default;
	}

	public void TempUpdateStructure()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsDirty || FormatterRequester == null || !IsAppGenDesigner)
		{
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		string value = (base.IsReportDesigner ? base.ReportDesignerControl.ReportToFile(base.IndentStyle, "    ") : ClaWindowManager.GetWindowText(base.Host, base.IndentStyle, "    "));
		if (FormatterRequester != null)
		{
			try
			{
				FormatterRequester.SetData(value, save: false);
			}
			catch (StructureException)
			{
			}
		}
		Cursor.Current = Cursors.Default;
	}

	public override void Dispose()
	{
		if (iRequester != null)
		{
			iRequester.InformDialogClosed();
			iRequester = null;
		}
		base.Dispose();
	}

	public override void Deselecting()
	{
	}

	public virtual void Discard()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(new Action(Discard));
			return;
		}
		base.IsDirty = false;
		TryClose();
	}

	public virtual bool HaveChanges()
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<bool>((Func<bool>)HaveChanges);
		}
		return base.IsDirty;
	}

	public virtual bool TryClose()
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<bool>((Func<bool>)TryClose);
		}
		return BackToSource();
	}

	public bool SetTemplate(ControlContainer rcd, uint seqValue, IPopulatedTemplate popTemplate)
	{
		if (base.IsReportDesigner)
		{
			if (base.ReportDesignerControl != null)
			{
				base.ReportDesignerControl.SetTemplate(rcd, seqValue, popTemplate);
			}
		}
		else if (base.WindowManager != null)
		{
			base.WindowManager.SetTemplate(base.Host, rcd, seqValue, popTemplate);
		}
		return true;
	}
}

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.FormsDesigner.Commands;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.ClarionNet;
using SoftVelocity.ClarionNet.Designer;
using SoftVelocity.ClarionNet.Designer.SectionControls;
using SoftVelocity.ClarionNet.ReportDesigner.OptionPanels;
using SoftVelocity.ClarionNet.ReportItems;
using SoftVelocity.ClarionNet.WindowDesigner;
using SoftVelocity.ClarionNet.WindowDesigner.OptionPanels;
using SoftVelocity.Common.ClarionEditor.Evaluators;
using SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.FormDesigner;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.DataDictionary.Design;
using SoftVelocity.DataDictionary.Schema;
using VisualHint.SmartPropertyGrid;

namespace SoftVelocity.Common.ClarionEditor;

public class CommonClarionDesignerView : FormsDesignerViewContent, IUndoHandler, IOwnerState, IBackToSourceCompatible, IClipboardHandler, IFileSchemaPadController
{
	private const string idSaveAndExitReport = "SaveAndExitReport";

	private const string idCancelReport = "CancelReport";

	private const string idExitSeparator = "ExitSeparator";

	private const string idAlignToGrid = "AlignToGrid";

	private const string idSizeToGrid = "sizeToGrid";

	private const string idCenterHorz = "centerHorz";

	private const string idCenterVert = "centerVert";

	private const string idBringToFront = "BringToFront";

	private const string idSendToBack = "SendToBack";

	private const string idPrintPreviewSeparator = "PrintPreviewSeparator";

	private const string idPrintPreview = "PrintPreview";

	private const string idViewTabOrder = "ViewTabOrder";

	private const string idSuppressTransparencySeparator = "SuppressTransparencySeparator";

	private const string idSuppressTransparency = "SuppressTransparency";

	private const string idWindowPreview = "WindowPreview";

	private const string idWindowPreviewSeparator = "WindowPreviewSeparator";

	private const string idUseVisualStylesSeparator = "UseVisualStylesSeparator";

	private const string idUseVisualStyles = "UseVisualStyles";

	private const string idHideHiddenControlsSeparator = "HideHiddenControlsSeparator";

	private const string idHideHiddenControls = "HideHiddenControls";

	private const string idalignLeft = "alignLeft";

	private const string idalignRight = "alignRight";

	private const string idalignTop = "alignTop";

	private const string idalignBottom = "alignBottom";

	private const string idalignHorz = "alignHorz";

	private const string idalignVert = "alignVert";

	private const string idspreadHorz = "spreadHorz";

	private const string idspreadVert = "spreadVert";

	private const string idmakeSameSize = "makeSameSize";

	private const string idmakeSameWidth = "makeSameWidth";

	private const string idmakeSameHeight = "makeSameHeight";

	private static string m_designableStructureNotFound = "No REPORT or WINDOW structure was found.";

	private ClaReportManager m_reportManager;

	private BaseDesignerControl m_rddesignerControl;

	private GeneralDesiner m_wddesignerControl;

	private ClaWindowManager m_windowManager;

	private WindowKeyHandler m_windowKeyHandler;

	private ClaStructureDesignerLoaderProvider m_loaderProvider;

	private ToolStrip toolStrip;

	private ToolStrip m_PropertiesToolStrip;

	private Timer toolbarUpdateTimer;

	private bool m_isWin;

	protected ClaDesignerGenerator.FormDesignerModeenum m_mode = ClaDesignerGenerator.FormDesignerModeenum.Standart;

	protected bool m_isWindowWindow;

	private ControlContainer m_rcd;

	private bool m_IsTemplate;

	private CompilerResults m_cr;

	private bool alreadyShown;

	private TextBox m_errorText;

	private Label m_errortitle;

	private Panel m_largeDesignAreaPanel;

	private SideTab sideTabItem;

	private BuildSideTab m_BuildSideTab;

	private bool forceDesignerIndentation;

	private int m_reportEndLine = -1;

	private bool m_IsTextEditorDirty;

	private bool isDirty;

	private int mainViewIndex;

	private TabOrder tabOrder;

	public Enum InternalState => m_mode;

	protected ClaWindowManager WindowManager => m_windowManager;

	protected ClaStructureDesignerLoaderProvider LoaderProvider => m_loaderProvider;

	public override bool Visible => false;

	public override bool ShowMainMenu => false;

	public WindowKeyHandler WindowKeyHandler => m_windowKeyHandler;

	public new ClaDesignerGenerator DesignerGenerator => base.DesignerGenerator as ClaDesignerGenerator;

	protected bool IsWin => m_isWin;

	public override bool ShouldEventBindingServiceCreate => false;

	public bool IsReportDesigner => m_mode != ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner;

	protected ControlContainer ControlContainerDecl => m_rcd;

	protected CompilerResults CompilerResults => m_cr;

	public bool IsCompilerResults
	{
		get
		{
			if (CompilerResults == null)
			{
				return ControlContainerDecl == null;
			}
			return true;
		}
	}

	public Panel LargeDesignAreaPanel => m_largeDesignAreaPanel;

	bool IUndoHandler.EnableUndo
	{
		get
		{
			if (!IsReportDesigner && WindowDesignerControl != null && WindowDesignerControl.UndoEngine != null)
			{
				return WindowDesignerControl.EnableUndo;
			}
			if (ReportDesignerControl != null && ReportDesignerControl.UndoEngine != null)
			{
				return ReportDesignerControl.EnableUndo;
			}
			return false;
		}
	}

	bool IUndoHandler.EnableRedo
	{
		get
		{
			if (!IsReportDesigner && WindowDesignerControl != null && WindowDesignerControl.UndoEngine != null)
			{
				return WindowDesignerControl.EnableRedo;
			}
			if (ReportDesignerControl != null && ReportDesignerControl.UndoEngine != null)
			{
				return ReportDesignerControl.EnableRedo;
			}
			return false;
		}
	}

	protected BuildSideTab BuildSideTab => m_BuildSideTab;

	public BaseDesignerControl BaseReportDesignerControl
	{
		get
		{
			return m_rddesignerControl;
		}
		set
		{
			m_rddesignerControl = value;
		}
	}

	public Report ReportDesignerControl
	{
		get
		{
			if (BaseReportDesignerControl != null)
			{
				return BaseReportDesignerControl.ReportControl;
			}
			return null;
		}
	}

	public GeneralDesiner WindowDesignerControl
	{
		get
		{
			return m_wddesignerControl;
		}
		set
		{
			m_wddesignerControl = value;
		}
	}

	protected IndentStyle IndentStyle
	{
		get
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (forceDesignerIndentation)
			{
				return (IndentStyle)0;
			}
			return ((TextEditorControlBase)base.TextEditorControl).Document.TextEditorProperties.IndentStyle;
		}
	}

	public bool ForceDesignerIndentation
	{
		get
		{
			return forceDesignerIndentation;
		}
		set
		{
			forceDesignerIndentation = value;
		}
	}

	public int ReportEndLine
	{
		get
		{
			return m_reportEndLine;
		}
		set
		{
			m_reportEndLine = value;
		}
	}

	public bool IsUseVisualStyles
	{
		get
		{
			if (IsReportDesigner)
			{
				if (BaseReportDesignerControl != null)
				{
					return BaseReportDesignerControl.IsUseVisualStyles;
				}
				return false;
			}
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.IsUseVisualStyles;
			}
			return false;
		}
		set
		{
			if (IsReportDesigner)
			{
				if (BaseReportDesignerControl != null)
				{
					BaseReportDesignerControl.IsUseVisualStyles = value;
				}
			}
			else if (WindowDesignerControl != null)
			{
				WindowDesignerControl.IsUseVisualStyles = value;
			}
			((Control)(object)PropertyPad.Grid).Refresh();
		}
	}

	public bool IsSuppressTransparency
	{
		get
		{
			if (IsReportDesigner)
			{
				if (BaseReportDesignerControl != null)
				{
					return BaseReportDesignerControl.IsSuppressTransparency;
				}
				return false;
			}
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.IsSuppressTransparency;
			}
			return false;
		}
		set
		{
			if (IsReportDesigner)
			{
				if (BaseReportDesignerControl != null)
				{
					BaseReportDesignerControl.IsSuppressTransparency = value;
				}
			}
			else if (WindowDesignerControl != null)
			{
				WindowDesignerControl.IsSuppressTransparency = value;
			}
			((Control)(object)PropertyPad.Grid).Refresh();
		}
	}

	public bool IsHideHiddenControls
	{
		get
		{
			if (IsReportDesigner)
			{
				if (BaseReportDesignerControl != null)
				{
					return BaseReportDesignerControl.IsHideHiddenControls;
				}
				return false;
			}
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.IsHideHiddenControls;
			}
			return false;
		}
		set
		{
			if (IsReportDesigner)
			{
				if (BaseReportDesignerControl != null)
				{
					BaseReportDesignerControl.IsHideHiddenControls = value;
				}
			}
			else if (WindowDesignerControl != null)
			{
				WindowDesignerControl.IsHideHiddenControls = value;
			}
			((Control)(object)PropertyPad.Grid).Refresh();
		}
	}

	public bool IsTextEditorDirty => m_IsTextEditorDirty;

	public bool IsDirty
	{
		get
		{
			return isDirty;
		}
		set
		{
			isDirty = value;
		}
	}

	protected override bool ActivateDesigner => false;

	public override bool IsFormDesignerKeyHandler => false;

	public int MainViewIndex
	{
		get
		{
			return mainViewIndex;
		}
		set
		{
			mainViewIndex = value;
		}
	}

	bool IClipboardHandler.EnableCut
	{
		get
		{
			if (!IsReportDesigner && WindowDesignerControl != null)
			{
				if (base.EnableCut)
				{
					return !WindowDesignerControl.IsCurrentTemplateControl();
				}
				return false;
			}
			if (ReportDesignerControl != null)
			{
				if (base.EnableCut && !ReportDesignerControl.IsCurrentTemplateControl())
				{
					return ReportDesignerControl.IsMainMenuCutEnabled();
				}
				return false;
			}
			return base.EnableCut;
		}
	}

	bool IClipboardHandler.EnableCopy
	{
		get
		{
			if (!IsReportDesigner && WindowDesignerControl != null)
			{
				if (base.EnableCopy)
				{
					return !WindowDesignerControl.IsCurrentTemplateControl();
				}
				return false;
			}
			if (ReportDesignerControl != null)
			{
				if (base.EnableCopy && !ReportDesignerControl.IsCurrentTemplateControl())
				{
					return ReportDesignerControl.IsMainMenuCopyEnabled();
				}
				return false;
			}
			return base.EnableCopy;
		}
	}

	bool IClipboardHandler.EnablePaste
	{
		get
		{
			if (!IsReportDesigner && WindowDesignerControl != null)
			{
				if (base.EnablePaste)
				{
					return !WindowDesignerControl.IsCurrentTemplateControl();
				}
				return false;
			}
			if (ReportDesignerControl != null)
			{
				if (base.EnablePaste && ReportDesignerControl.IsPasteAllowed())
				{
					return !ReportDesignerControl.IsCurrentTemplateControl();
				}
				return false;
			}
			return base.EnablePaste;
		}
	}

	bool IClipboardHandler.EnableDelete
	{
		get
		{
			if (!IsReportDesigner && WindowDesignerControl != null)
			{
				if (base.EnableDelete)
				{
					return !WindowDesignerControl.IsCurrentTemplateControl();
				}
				return false;
			}
			if (ReportDesignerControl != null)
			{
				if (base.EnableDelete && ReportDesignerControl.IsDeleteAllowed())
				{
					return !ReportDesignerControl.IsCurrentTemplateControl();
				}
				return false;
			}
			return base.EnableDelete;
		}
	}

	bool IClipboardHandler.EnableSelectAll => false;

	public bool DisplayKeys => false;

	public bool DisplayColumns => true;

	public DictionaryItemClipboardFormat ClipboardFormat
	{
		get
		{
			if (m_reportManager != null)
			{
				return DictionaryItemClipboardFormat.ReportControl;
			}
			return DictionaryItemClipboardFormat.ScreenControl;
		}
	}

	public event EventHandler DesignerClosed;

	private bool NullMethod()
	{
		return true;
	}

	public CommonClarionDesignerView(IViewContent viewContent, IDesignerLoaderProvider loaderProvider, IDesignerGenerator generator, bool isWin)
		: base(viewContent, loaderProvider, generator)
	{
		m_loaderProvider = loaderProvider as ClaStructureDesignerLoaderProvider;
		base.ShouldUndoEngineCreate = false;
		m_isWin = isWin;
	}

	public override void Selected()
	{
		WorkbenchWindow_WindowSelected(null, null);
	}

	protected override void WorkbenchWindow_WindowDeselected(object sender, EventArgs e)
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || (object)WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent == this || IsCompilerResults)
		{
			return;
		}
		if (IsReportDesigner)
		{
			if (ReportDesignerControl != null)
			{
				ReportDesignerControl.ShowPropertiesDropList(b: false);
			}
		}
		else if (WindowDesignerControl != null)
		{
			WindowDesignerControl.ShowPropertiesDropList(b: false);
		}
		((VisualHint.SmartPropertyGrid.PropertyGrid)(object)PropertyPad.Grid).SelectedObjects = null;
		RemoveSideBarItem(IsReportDesigner ? "Report" : "Window");
	}

	protected override void WorkbenchWindow_WindowSelected(object sender, EventArgs e)
	{
		if (disposing || IsCompilerResults)
		{
			return;
		}
		object activeContent = WorkbenchSingleton.Workbench.ActiveContent;
		IHasPropertyContainer val = (IHasPropertyContainer)((activeContent is IHasPropertyContainer) ? activeContent : null);
		if (val != null && val.PropertyContainer == base.PropertyContainer)
		{
			if (IsReportDesigner)
			{
				if (ReportDesignerControl != null)
				{
					ReportDesignerControl.ShowPropertiesDropList(b: true);
				}
			}
			else if (WindowDesignerControl != null)
			{
				WindowDesignerControl.ShowPropertiesDropList(b: true);
			}
			UpdatePropertyPad();
		}
		BuildToolBarItems(IsReportDesigner ? "Clarion Report Controls" : "Clarion Window Controls");
	}

	protected virtual void WorkbenchWindow_ClosingEvent(object sender, CancelEventArgs e)
	{
		((IBaseViewContent)viewContent).WorkbenchWindow.ClosingEvent -= WorkbenchWindow_ClosingEvent;
		if (WorkbenchSingleton.Workbench.ActiveContent == this)
		{
			CloseDesigner();
		}
		else
		{
			e.Cancel = true;
		}
	}

	public bool IsReport()
	{
		return IsReportDesigner;
	}

	public bool IsWindow()
	{
		return m_mode == ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner;
	}

	public bool ShowDesigner(ControlContainer rcd, CompilerResults cr, bool isWindowDesigner, bool isWindowWindow, bool IsTemplate)
	{
		if (alreadyShown)
		{
			return false;
		}
		alreadyShown = true;
		if (((AbstractBaseViewContent)this).WorkbenchWindow == null)
		{
			return false;
		}
		int num = -1;
		for (int i = 0; i < viewContent.SecondaryViewContents.Count; i++)
		{
			if ((object)viewContent.SecondaryViewContents[i] == this)
			{
				num = i + 1;
				break;
			}
		}
		if (num == -1)
		{
			return false;
		}
		Cursor.Current = Cursors.WaitCursor;
		int num2 = 0;
		IBaseViewContent activeViewContent = ((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent;
		if ((object)activeViewContent != viewContent)
		{
			for (int j = 0; j < viewContent.SecondaryViewContents.Count; j++)
			{
				if ((object)viewContent.SecondaryViewContents[j] == activeViewContent)
				{
					num2 = j + 1;
					break;
				}
			}
		}
		MainViewIndex = num2;
		m_rcd = rcd;
		if (m_rcd == null || (cr != null && cr.Errors.HasErrors))
		{
			m_cr = cr;
		}
		else
		{
			m_cr = null;
		}
		if (m_rcd == null || IsTemplate)
		{
			ReportEndLine = ((TextEditorControlBase)base.TextEditorControl).ActiveTextAreaControl.Caret.Line;
		}
		else
		{
			ReportEndLine = m_rcd.BodyReg.LineEnd - 1;
		}
		m_mode = (isWindowDesigner ? ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner : ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner);
		m_IsTemplate = IsTemplate;
		m_isWindowWindow = isWindowWindow;
		if (isWindowDesigner)
		{
			m_windowManager = new ClaWindowManager(this, LoaderProvider, DesignerGenerator, GetITextEditorControlProvider());
		}
		if (IsCompilerResults)
		{
			failedDesignerInitialize = true;
			InitErrorView();
		}
		else
		{
			ArrayList arrayList = new ArrayList();
			ArrayList arrayList2 = null;
			if (m_mode == ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)
			{
				ClaReportExternalControls.InitControls();
				arrayList2 = ClaReportExternalControls.GetAllExternalControls(Report.C6COMPATIBLE_MODE);
			}
			else
			{
				ClaWindowExternalControls.InitControls();
				arrayList2 = ClaWindowExternalControls.GetAllExternalControls(GeneralDesiner.C6COMPATIBLE_MODE);
			}
			foreach (object item in arrayList2)
			{
				ExternalControlPropsBase externalControlPropsBase = (ExternalControlPropsBase)item;
				arrayList.Add(externalControlPropsBase.Assembly);
			}
			if (LoaderProvider != null)
			{
				LoaderProvider.PreCreateInitProvider(m_mode, m_rcd, arrayList, m_isWindowWindow);
			}
			Reload();
			if (base.FailedDesignerInitialize)
			{
				((AbstractBaseViewContent)this).WorkbenchWindow.SwitchView(num);
				return false;
			}
			InitView();
			IsFormsDesignerVisible = true;
			SubscribeWindowEvents();
			((IBaseViewContent)viewContent).WorkbenchWindow.ClosingEvent += WorkbenchWindow_ClosingEvent;
			if (IsReportDesigner)
			{
				if (ReportDesignerControl != null)
				{
					ReportDesignerControl.ParseControlString = ParseControlString;
				}
			}
			else if (WindowDesignerControl != null)
			{
				WindowDesignerControl.ParseControlString = ParseControlString;
			}
		}
		((AbstractBaseViewContent)this).WorkbenchWindow.SwitchView(num);
		SelectRootComponent();
		Cursor.Current = Cursors.Default;
		return true;
	}

	public virtual void CloseDesigner()
	{
		PostOnBackClick();
		base.Deselecting();
		m_rcd = null;
		m_windowManager = null;
		m_reportManager = null;
		UnsubscribeWindowEvents();
		((IBaseViewContent)viewContent).WorkbenchWindow.ClosingEvent -= WorkbenchWindow_ClosingEvent;
		if (toolStrip != null)
		{
			toolStrip.Dispose();
		}
		((AbstractBaseViewContent)this).Control.Controls.Clear();
		toolStrip = null;
		base.Deselected();
		alreadyShown = false;
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
		{
			WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.SwitchView(MainViewIndex);
		}
		if (this.DesignerClosed != null)
		{
			this.DesignerClosed(null, null);
			this.DesignerClosed = null;
		}
	}

	public override void SwitchedTo()
	{
	}

	protected virtual ITextEditorControlProvider GetITextEditorControlProvider()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return (ITextEditorControlProvider)viewContent;
	}

	protected virtual void InitView()
	{
		Cursor.Current = Cursors.WaitCursor;
		AddEvaluator("ClaStructureIsWindow", (IConditionEvaluator)(object)new GeneralEvaluator(IsWindow, isReverse: false));
		AddEvaluator("ClaStructureIsReport", (IConditionEvaluator)(object)new GeneralEvaluator(IsReport, isReverse: false));
		try
		{
			if (!IsReportDesigner)
			{
				InitWindowDesignerView();
			}
			else
			{
				InitReportDesignerView();
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex, ex.Message);
		}
		finally
		{
			Cursor.Current = Cursors.Default;
		}
	}

	private string GetCompilerErrors(CompilerResults cr)
	{
		if (cr == null)
		{
			return m_designableStructureNotFound;
		}
		string text = string.Empty;
		int num = 0;
		foreach (CompilerError error in cr.Errors)
		{
			string text2 = (m_IsTemplate ? (error.FileName + "(" + ((error.Line > 0) ? (error.Line + ",") : "") + error.Column + "): " + error.ErrorText + "\r\n") : ("Structure Template(" + (error.Line - num) + "," + error.Column + "): " + error.ErrorText + "\r\n"));
			text += text2;
		}
		return text;
	}

	private void InitErrorView()
	{
		CreateToolStrip(out toolStrip, islastVisible: false, isAllVisible: true, "/SoftVelocity/Clarion/ToolBar/CompilerErrorView");
		m_errorText = new TextBox();
		m_errorText.Font = new Font(FontService.GetFont((FontType)2).FontFamily, 8f);
		m_errorText.Multiline = true;
		m_errorText.WordWrap = false;
		m_errorText.ScrollBars = ScrollBars.Horizontal;
		m_errorText.Text = GetCompilerErrors(CompilerResults);
		m_errorText.BackColor = SystemColors.Window;
		m_errorText.ReadOnly = true;
		m_errorText.DeselectAll();
		m_errorText.Dock = DockStyle.Fill;
		((AbstractBaseViewContent)this).Control.Controls.Add(m_errorText);
		m_errortitle = new Label();
		m_errortitle.Text = "Failed to load designer. Check the source code for syntax errors and check if all references are available.";
		m_errortitle.Dock = DockStyle.Top;
		((AbstractBaseViewContent)this).Control.Controls.Add(m_errortitle);
		((AbstractBaseViewContent)this).Control.Controls.Add(toolStrip);
	}

	protected virtual void InitReportDesignerView()
	{
		m_IsTextEditorDirty = ((ICanBeDirty)viewContent).IsDirty;
		bool flag = m_IsTemplate || ((ICanBeDirty)viewContent).IsDirty;
		m_reportManager = new ClaReportManager("/ClaReport/ContextMenu/ContextMenu/Section", "/ClaReport/ContextMenu/ContextMenu/Items");
		CreateReportDesignerControl(GetITextEditorControlProvider());
		BuildToolBarItems("Clarion Report Controls");
		if (ReportGeneralOptionsPanel.ShowCommandToolbar)
		{
			CreateToolStrip(out toolStrip, islastVisible: false, isAllVisible: false, "/SoftVelocity/Clarion/ToolBar/ClarionFormater");
			((AbstractBaseViewContent)this).Control.Controls.Add(toolStrip);
		}
		if (ReportGeneralOptionsPanel.ShowPropertiesToolbar)
		{
			CreateToolStrip(out m_PropertiesToolStrip, islastVisible: true, isAllVisible: true, "/SoftVelocity/Clarion/ToolBar/ClaPropertiesToolbar");
			((AbstractBaseViewContent)this).Control.Controls.Add(m_PropertiesToolStrip);
		}
		if (ReportDesignerControl != null)
		{
			BaseReportDesignerControl.DesignerDirty += PropertyChanged;
			ReportDesignerControl.PropertiesToolbarUpdate += UpdatePropertiesToolbar;
		}
		if (ControlContainerDecl != null)
		{
			SetDirty(m_IsTemplate);
		}
		((ICanBeDirty)viewContent).IsDirty = flag;
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(PropertyPad));
		if (ReportGeneralOptionsPanel.SelectToolboxonRun && pad != null)
		{
			pad.CreatePad();
			pad = WorkbenchSingleton.Workbench.GetPad(typeof(SideBarView));
		}
		if (pad != null)
		{
			pad.BringPadToFront();
		}
		((VisualHint.SmartPropertyGrid.PropertyGrid)(object)PropertyPad.Grid).SelectedObject = base.Host.RootComponent;
		ReportDesignerControl.InitPropertyWindowList();
		ReportDesignerControl.CreateUndoRedo();
		if (ReportGeneralOptionsPanel.ShowPropertiesToolbar)
		{
			ISelectionService selectionService = (ISelectionService)base.Host.GetService(typeof(ISelectionService));
			selectionService.SelectionChanged += SelectionChangedHandler;
		}
		toolbarUpdateTimer = new Timer();
		toolbarUpdateTimer.Tick += UpdateReportToolbar;
		toolbarUpdateTimer.Interval = 700;
		toolbarUpdateTimer.Start();
	}

	public void ShowReportSettings(bool isReport)
	{
		if (base.Host != null)
		{
			ISelectionService selectionService = (ISelectionService)base.Host.GetService(typeof(ISelectionService));
			selectionService.SetSelectedComponents(new object[1] { base.Host.RootComponent }, SelectionTypes.Click);
			ShowProperties showProperties = new ShowProperties();
			((AbstractCommand)showProperties).Run();
		}
	}

	public void CollapseAll()
	{
		if (ReportDesignerControl != null)
		{
			ReportDesignerControl.CollapseAll();
		}
	}

	public bool IsCollapseAll()
	{
		if (ReportDesignerControl == null)
		{
			return false;
		}
		return ReportDesignerControl.IsCollapseAll();
	}

	private bool CreateReportDesignerControl(ITextEditorControlProvider viewContent)
	{
		ArrayList arrayList = new ArrayList();
		ArrayList allExternalControls = ClaReportExternalControls.GetAllExternalControls(Report.C6COMPATIBLE_MODE);
		foreach (object item in allExternalControls)
		{
			ExternalControlPropsBase externalControlPropsBase = (ExternalControlPropsBase)item;
			arrayList.Add(externalControlPropsBase.Assembly);
		}
		CreateReportDesignerControl();
		InitBeforeControls();
		ReportDesignerControl.InitReport(base.Host, ControlContainerDecl);
		((Control)base.Host.RootComponent).Location = new Point(0, 0);
		ISelectionService selectionService = (ISelectionService)base.Host.GetService(typeof(ISelectionService));
		if (selectionService != null)
		{
			selectionService.SetSelectedComponents(new object[1] { base.Host.RootComponent }, SelectionTypes.Remove);
			selectionService.SetSelectedComponents(new object[1] { base.Host.RootComponent }, SelectionTypes.Click);
		}
		IComponentChangeService componentChangeService = (IComponentChangeService)base.Host.GetService(typeof(IComponentChangeService));
		componentChangeService.ComponentChanged += OnComponentChanged;
		m_windowKeyHandler = new WindowKeyHandler();
		System.Windows.Forms.Application.AddMessageFilter(WindowKeyHandler);
		return true;
	}

	private bool CreateReportDesignerControl()
	{
		BaseReportDesignerControl = (BaseDesignerControl)base.Host.RootComponent;
		BaseReportDesignerControl.Host = base.Host;
		BaseReportDesignerControl.Init();
		CreateReportEvaluators(isNull: false);
		BaseReportDesignerControl.BorderStyle = BorderStyle.None;
		return true;
	}

	private bool CreateReportEvaluators(bool isNull)
	{
		BaseDesignerControl baseReportDesignerControl = BaseReportDesignerControl;
		AddEvaluator("ClaReportSelectedEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsSelected, isReverse: false));
		AddEvaluator("ClaReportMainDeleteEnabled", (IConditionEvaluator)(object)new GenericEvaluator2<ReportItem, ReportSection>());
		AddEvaluator("ClaReportMainPasteEnabled", (IConditionEvaluator)(object)new GeneralGenericEvaluator2<ReportItem, ReportSection>(baseReportDesignerControl.ReportControl.IsUserControlCopy, isReverse: false));
		AddEvaluator("ClaReportCollapseSectionEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsSectionCollapsed, isReverse: true));
		AddEvaluator("ClaReportExpandSectionEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsSectionCollapsed, isReverse: false));
		AddEvaluator("ClaReportPageHeaderEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsPageHeader, isReverse: false));
		AddEvaluator("ClaReportPageFooterEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsPageFooter, isReverse: false));
		AddEvaluator("ClaReportPageFormEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsPageForm, isReverse: false));
		AddEvaluator("ClaReportGroupHeaderEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsGroupHeader, isReverse: false));
		AddEvaluator("ClaReportGroupFooterEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsGroupFooter, isReverse: false));
		AddEvaluator("ClaReportPastControlEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsUserControlCopy, isReverse: false));
		AddEvaluator("ClaReportDelSectionEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsReportSelected, isReverse: true));
		AddEvaluator("ClaReportSurroundBreakEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsSurroundingBreak, isReverse: false));
		AddEvaluator("ClaReportDetailEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.IsBandView, isReverse: false));
		AddEvaluator("ClaReportBreakGroupEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.IsBandView, isReverse: false));
		AddEvaluator("ClaReportCollapseAllShown", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.IsBandView, isReverse: false));
		AddEvaluator("ClaReportCollapseAllEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsSectionCollapsedAll, isReverse: true));
		AddEvaluator("ClaReportExpandAllEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsSectionCollapsedAll, isReverse: false));
		AddEvaluator("ClaReportSurroundBreakEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsSurroundingBreak, isReverse: false));
		AddEvaluator("ClaReportControlActionEnabled", (IConditionEvaluator)(object)new GenericEvaluator<ReportItem>());
		AddEvaluator("ClaReportPasteControlEnabled", (IConditionEvaluator)(object)new GeneralGenericEvaluator<ReportItem>(baseReportDesignerControl.ReportControl.IsUserControlCopy, isReverse: false));
		AddEvaluator("ClaReportFormatEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsFormatEnabled, isReverse: false));
		AddEvaluator("ClaReportPopulateEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsPopulateEnabled, isReverse: false));
		AddEvaluator("ClaWindowBringToFrontEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsBringToFrontEnabled, isReverse: false));
		AddEvaluator("ClaWindowSendToBackEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsSendToBackEnabled, isReverse: false));
		AddEvaluator("ClaWindowTabOrderEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsTabOrderBackEnabled, isReverse: false));
		AddEvaluator("ClaWindowPasteEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsPasteEnabled, isReverse: false));
		AddEvaluator("ClaWindowCopyEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsCopyEnabled, isReverse: false));
		AddEvaluator("ClaWindowActionEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsActionEnabled, isReverse: false));
		AddEvaluator("ClaWindowCenterMenuEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsCenterInFormEnabled, isReverse: false));
		AddEvaluator("ClaWindowFormatMenuEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(baseReportDesignerControl.ReportControl.IsFormatEnabled, isReverse: false));
		return true;
	}

	protected virtual bool InitWindowDesignerView()
	{
		m_IsTextEditorDirty = ((ICanBeDirty)viewContent).IsDirty;
		bool flag = m_IsTemplate || ((ICanBeDirty)viewContent).IsDirty;
		CreateWindowDesignerControl(GetITextEditorControlProvider());
		BuildToolBarItems("Clarion Window Controls");
		if (WindowGeneralOptionsPanel.ShowCommandToolbar)
		{
			CreateToolStrip(out toolStrip, islastVisible: true, isAllVisible: false, "/SoftVelocity/Clarion/ToolBar/ClarionFormater");
			((AbstractBaseViewContent)this).Control.Controls.Add(toolStrip);
		}
		if (WindowGeneralOptionsPanel.ShowPropertiesToolbar)
		{
			CreateToolStrip(out m_PropertiesToolStrip, islastVisible: true, isAllVisible: true, "/SoftVelocity/Clarion/ToolBar/ClaPropertiesToolbar");
			((AbstractBaseViewContent)this).Control.Controls.Add(m_PropertiesToolStrip);
		}
		if (ControlContainerDecl != null)
		{
			SetDirty(m_IsTemplate);
		}
		((ICanBeDirty)viewContent).IsDirty = flag;
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(PropertyPad));
		if (WindowGeneralOptionsPanel.SelectToolboxonRun && pad != null)
		{
			pad.CreatePad();
			pad = WorkbenchSingleton.Workbench.GetPad(typeof(SideBarView));
		}
		if (pad != null)
		{
			pad.BringPadToFront();
		}
		((VisualHint.SmartPropertyGrid.PropertyGrid)(object)PropertyPad.Grid).SelectedObject = base.Host.RootComponent;
		WindowDesignerControl.InitPropertyWindowList();
		((AbstractPadContent)PropertyPad.Instance).Control.Update();
		((Control)(object)PropertyPad.Grid).Refresh();
		if (WindowGeneralOptionsPanel.ShowPropertiesToolbar)
		{
			ISelectionService selectionService = (ISelectionService)base.Host.GetService(typeof(ISelectionService));
			selectionService.SelectionChanged += SelectionChangedHandler;
		}
		toolbarUpdateTimer = new Timer();
		toolbarUpdateTimer.Tick += UpdateWindowToolbar;
		toolbarUpdateTimer.Interval = 700;
		toolbarUpdateTimer.Start();
		return true;
	}

	private bool CreateWindowDesignerControl(ITextEditorControlProvider viewContent)
	{
		if (base.Host.RootComponent == null)
		{
			return false;
		}
		ArrayList arrayList = new ArrayList();
		ArrayList allExternalControls = ClaWindowExternalControls.GetAllExternalControls(GeneralDesiner.C6COMPATIBLE_MODE);
		foreach (object item in allExternalControls)
		{
			ExternalControlPropsBase externalControlPropsBase = (ExternalControlPropsBase)item;
			arrayList.Add(externalControlPropsBase.Assembly);
		}
		if (base.Host.RootComponent is GeneralDesiner)
		{
			WindowDesignerControl = (GeneralDesiner)base.Host.RootComponent;
			WindowDesignerControl.DesignerDirty += OnDesignerDirty;
			WindowDesignerControl.InitDesigner(ControlContainerDecl);
			CreateWindowEvaluators(WindowDesignerControl);
			WindowDesignerControl.PropertiesToolbarUpdate += UpdatePropertiesToolbar;
		}
		InitBeforeControls();
		WindowManager.DeserializeControls(base.Host, ControlContainerDecl, ReportControlDeserializer.Templatesenum.None);
		ISelectionService selectionService = (ISelectionService)base.Host.GetService(typeof(ISelectionService));
		if (selectionService != null)
		{
			selectionService.SetSelectedComponents(new object[1] { base.Host.RootComponent }, SelectionTypes.Remove);
			selectionService.SetSelectedComponents(new object[1] { base.Host.RootComponent }, SelectionTypes.Click);
		}
		base.PropertyContainer.SelectableObjects = base.Host.Container.Components;
		IComponentChangeService componentChangeService = (IComponentChangeService)base.Host.GetService(typeof(IComponentChangeService));
		if (componentChangeService != null)
		{
			componentChangeService.ComponentChanged += OnComponentChanged;
		}
		m_windowKeyHandler = new WindowKeyHandler();
		System.Windows.Forms.Application.AddMessageFilter(WindowKeyHandler);
		if (WindowDesignerControl != null)
		{
			WindowDesignerControl.CreateUndoRedo();
		}
		return true;
	}

	private bool CreateWindowEvaluators(GeneralDesiner rootComponent)
	{
		AddEvaluator("ClaWindowPopulateEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(rootComponent.IsPopulateEnabled, isReverse: false));
		AddEvaluator("ClaWindowBringToFrontEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(rootComponent.IsBringToFrontEnabled, isReverse: false));
		AddEvaluator("ClaWindowSendToBackEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(rootComponent.IsSendToBackEnabled, isReverse: false));
		AddEvaluator("ClaWindowTabOrderEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(rootComponent.IsTabOrderBackEnabled, isReverse: false));
		AddEvaluator("ClaWindowPasteEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(rootComponent.IsPasteEnabled, isReverse: false));
		AddEvaluator("ClaWindowCopyEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(rootComponent.IsCopyEnabled, isReverse: false));
		AddEvaluator("ClaWindowActionEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(rootComponent.IsActionEnabled, isReverse: false));
		AddEvaluator("ClaWindowCenterMenuEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(rootComponent.IsCenterInFormEnabled, isReverse: false));
		AddEvaluator("ClaWindowFormatMenuEnabled", (IConditionEvaluator)(object)new GeneralEvaluator(rootComponent.IsFormatEnabled, isReverse: false));
		return true;
	}

	private bool CreateToolStrip(out ToolStrip ts, bool islastVisible, bool isAllVisible, string path)
	{
		ts = ToolbarService.CreateToolStrip((object)this, path, new string[0]);
		ts.ShowItemToolTips = true;
		ts.Dock = DockStyle.Top;
		ts.GripStyle = ToolStripGripStyle.Hidden;
		ts.Stretch = true;
		if (IsCompilerResults)
		{
			return true;
		}
		if (isAllVisible)
		{
			return true;
		}
		if (!IsBackVisible())
		{
			ToolbarService.HideItem(ts, "SaveAndExitReport");
			ToolbarService.HideItem(ts, "CancelReport");
		}
		if (IsReportDesigner)
		{
			ToolbarService.HideItem(ts, "WindowPreview");
			ToolbarService.HideItem(ts, "WindowPreviewSeparator");
			ToolbarService.HideItem(ts, "SuppressTransparencySeparator");
			ToolbarService.HideItem(ts, "SuppressTransparency");
			ToolbarService.HideItem(ts, "UseVisualStylesSeparator");
			ToolbarService.HideItem(ts, "UseVisualStyles");
			ToolbarService.HideItem(ts, "HideHiddenControlsSeparator");
			ToolbarService.HideItem(ts, "HideHiddenControls");
		}
		else
		{
			ToolbarService.HideItem(ts, "PrintPreview");
			ToolbarService.HideItem(ts, "PrintPreviewSeparator");
		}
		return true;
	}

	public bool IsPropertybarFocused()
	{
		if (m_PropertiesToolStrip != null)
		{
			foreach (object item in m_PropertiesToolStrip.Items)
			{
				if ((item is ToolBarTextBox || item is ToolBarComboBox) && ((ToolStripControlHost)item).Focused)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool DoPropertybarAction(CommandID commandID)
	{
		if (m_PropertiesToolStrip != null)
		{
			foreach (object item in m_PropertiesToolStrip.Items)
			{
				ToolBarTextBox val = (ToolBarTextBox)((item is ToolBarTextBox) ? item : null);
				if (val != null && ((ToolStripControlHost)(object)val).Focused)
				{
					if (commandID == StandardCommands.Copy)
					{
						((ToolStripTextBox)(object)val).Copy();
						return true;
					}
					if (commandID == StandardCommands.Cut)
					{
						((ToolStripTextBox)(object)val).Cut();
						return true;
					}
					if (commandID == StandardCommands.Paste)
					{
						((ToolStripTextBox)(object)val).Paste();
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsDuplicateAllowed()
	{
		if (base.Host == null)
		{
			return false;
		}
		if (IsReportDesigner)
		{
			if (ReportDesignerControl != null)
			{
				return ReportDesignerControl.IsDuplicateAllowed();
			}
		}
		else
		{
			ISelectionService selectionService = (ISelectionService)base.Host.GetService(typeof(ISelectionService));
			if (selectionService != null)
			{
				if (selectionService.GetComponentSelected(base.Host.RootComponent))
				{
					ICollection selectedComponents = selectionService.GetSelectedComponents();
					if (selectionService.PrimarySelection == base.Host.RootComponent && selectedComponents.Count == 1)
					{
						return false;
					}
					selectionService.SetSelectedComponents(new object[1] { base.Host.RootComponent }, SelectionTypes.Remove);
				}
				return true;
			}
		}
		return false;
	}

	public Control GetParentForDuplicate(Control selControl)
	{
		if (selControl != null)
		{
			if (!IsReportDesigner && GeneralDesiner.C6COMPATIBLE_MODE)
			{
				return WindowDesignerControl.GetControlParent(selControl);
			}
			if (IsReportDesigner)
			{
				return ReportDesignerControl.GetControlParentForDuplicate(selControl);
			}
			return selControl.Parent;
		}
		return null;
	}

	protected virtual bool IsBackVisible()
	{
		return true;
	}

	public void UpdateReportToolbar(object sender, EventArgs e)
	{
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Expected O, but got Unknown
		if (toolStrip == null)
		{
			return;
		}
		foreach (ToolStripItem item in toolStrip.Items)
		{
			if (ReportDesignerControl != null && ReportDesignerControl.IsCurrentTemplateControl())
			{
				item.Enabled = false;
				continue;
			}
			IStatusUpdate val = (IStatusUpdate)(object)((item is IStatusUpdate) ? item : null);
			if (val == null)
			{
				continue;
			}
			switch (val.CodonId)
			{
			case "SaveAndExitReport":
			case "CancelReport":
			case "ExitSeparator":
				if (!IsBackVisible())
				{
					item.Visible = false;
				}
				else
				{
					item.Enabled = true;
				}
				break;
			case "AlignToGrid":
			case "sizeToGrid":
				if (ReportDesignerControl != null && ReportDesignerControl.IsGridEnabled())
				{
					item.Enabled = true;
				}
				else
				{
					item.Enabled = false;
				}
				break;
			case "centerHorz":
			case "centerVert":
			case "BringToFront":
			case "SendToBack":
				if (ReportDesignerControl != null && ReportDesignerControl.IsCenterInFormEnabled())
				{
					item.Enabled = true;
				}
				else
				{
					item.Enabled = false;
				}
				break;
			case "SuppressTransparency":
				((ToolStripButton)(ToolBarCheckBox)item).Checked = ((ToolBarCheckBox)item).MenuCommand.IsChecked;
				break;
			case "WindowPreview":
				item.Visible = false;
				break;
			case "alignLeft":
			case "alignRight":
			case "alignTop":
			case "alignBottom":
			case "alignHorz":
			case "alignVert":
			case "spreadHorz":
			case "spreadVert":
			case "makeSameSize":
			case "makeSameWidth":
			case "makeSameHeight":
				item.Enabled = ReportDesignerControl != null && ReportDesignerControl.IsFormatEnabled();
				break;
			default:
				if (item is ToolBarCommand)
				{
					((ToolBarCommand)item).UpdateStatus();
				}
				break;
			case "PrintPreview":
			case "UseVisualStyles":
			case "HideHiddenControls":
				break;
			}
		}
	}

	public void UpdateWindowToolbar(object sender, EventArgs e)
	{
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		GeneralDesiner windowDesignerControl = WindowDesignerControl;
		if (toolStrip == null || windowDesignerControl == null)
		{
			return;
		}
		foreach (ToolStripItem item in toolStrip.Items)
		{
			if (windowDesignerControl.IsCurrentTemplateControl())
			{
				item.Enabled = false;
				continue;
			}
			IStatusUpdate val = (IStatusUpdate)(object)((item is IStatusUpdate) ? item : null);
			if (val == null)
			{
				continue;
			}
			string codonId = val.CodonId;
			switch (codonId)
			{
			case "SaveAndExitReport":
			case "CancelReport":
			case "ExitSeparator":
				if (!IsBackVisible())
				{
					item.Visible = false;
				}
				else
				{
					item.Enabled = true;
				}
				continue;
			case "AlignToGrid":
			case "sizeToGrid":
				item.Enabled = windowDesignerControl.IsGridEnabled();
				continue;
			case "centerHorz":
			case "centerVert":
				item.Enabled = windowDesignerControl.IsCenterInFormEnabled(base.Host);
				continue;
			case "BringToFront":
				item.Enabled = windowDesignerControl.IsBringToFrontEnabled();
				continue;
			case "SendToBack":
				item.Enabled = windowDesignerControl.IsSendToBackEnabled();
				continue;
			case "PrintPreview":
				item.Visible = false;
				continue;
			case "WindowPreview":
				item.Enabled = windowDesignerControl.IsWindowPreviewEnabled();
				continue;
			case "alignLeft":
			case "alignRight":
			case "alignTop":
			case "alignBottom":
			case "alignHorz":
			case "alignVert":
			case "spreadHorz":
			case "spreadVert":
			case "makeSameSize":
			case "makeSameWidth":
			case "makeSameHeight":
				item.Enabled = windowDesignerControl.IsFormatEnabled(base.Host);
				continue;
			}
			if (item is ToolBarCheckBox)
			{
				((ToolBarCheckBox)item).UpdateStatus();
				if ("ViewTabOrder".Equals(codonId))
				{
					item.Invalidate();
				}
			}
			else if (item is ToolBarCommand)
			{
				((ToolBarCommand)item).UpdateStatus();
			}
		}
	}

	public void UpdatePropertiesToolbar(object sender, EventArgs e)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		if (m_PropertiesToolStrip == null)
		{
			return;
		}
		foreach (object item in m_PropertiesToolStrip.Items)
		{
			if (item is ToolBarTextBox && ((ToolBarTextBox)item).MenuCommand is AbstractWindowDesignerTextBoxCommand)
			{
				((AbstractWindowDesignerTextBoxCommand)(object)((ToolBarTextBox)item).MenuCommand).RefreshText();
			}
			else if (item is ToolBarComboBox && ((ToolBarComboBox)item).MenuCommand is AbstractWindowDesignerComboBoxCommand)
			{
				((AbstractWindowDesignerComboBoxCommand)(object)((ToolBarComboBox)item).MenuCommand).RefreshText();
			}
			else if (item is ToolBarCheckBox)
			{
				if (((ToolBarCheckBox)item).MenuCommand is SwitchFontBold)
				{
					((ToolStripButton)(ToolBarCheckBox)item).Checked = GetIsFontBoldValue();
				}
				else if (((ToolBarCheckBox)item).MenuCommand is SwitchItalicBold)
				{
					((ToolStripButton)(ToolBarCheckBox)item).Checked = GetIsFontItalicValue();
				}
				else if (((ToolBarCheckBox)item).MenuCommand is SwitchUnderlineBold)
				{
					((ToolStripButton)(ToolBarCheckBox)item).Checked = GetIsFontUnderlineValue();
				}
			}
		}
	}

	public string GetTextValue()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.GetTextValue();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.GetTextValue();
		}
		return string.Empty;
	}

	public string GetUSEValue()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.GetUSEValue();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.GetUSEValue();
		}
		return string.Empty;
	}

	public string GetFontNameValue()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.GetFontNameValue();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.GetFontNameValue();
		}
		return string.Empty;
	}

	public string GetFontSizeValue()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.GetFontSizeValue();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.GetFontSizeValue();
		}
		return string.Empty;
	}

	public bool GetIsFontBoldValue()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.GetIsFontBoldValue();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.GetIsFontBoldValue();
		}
		return false;
	}

	public bool GetIsFontItalicValue()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.GetIsFontItalicValue();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.GetIsFontItalicValue();
		}
		return false;
	}

	public bool GetIsFontUnderlineValue()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.GetIsFontUnderlineValue();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.GetIsFontUnderlineValue();
		}
		return false;
	}

	public bool SetTextValue(string val)
	{
		bool result = true;
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				result = WindowDesignerControl.SetTextValue(val);
			}
		}
		else if (ReportDesignerControl != null)
		{
			result = ReportDesignerControl.SetTextValue(val);
		}
		if (PropertyPad.Grid != null)
		{
			((Control)(object)PropertyPad.Grid).Refresh();
		}
		return result;
	}

	public bool SetUSEValue(string val)
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				WindowDesignerControl.SetUSEValue(val);
			}
		}
		else if (ReportDesignerControl != null)
		{
			ReportDesignerControl.SetUSEValue(val);
		}
		if (PropertyPad.Grid != null)
		{
			((Control)(object)PropertyPad.Grid).Refresh();
		}
		return true;
	}

	public bool SetFontNameValue(string val)
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				WindowDesignerControl.SetFontNameValue(val);
			}
		}
		else if (ReportDesignerControl != null)
		{
			ReportDesignerControl.SetFontNameValue(val);
		}
		if (PropertyPad.Grid != null)
		{
			((Control)(object)PropertyPad.Grid).Refresh();
		}
		return true;
	}

	public bool SetFontSizeValue(int val)
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				WindowDesignerControl.SetFontSizeValue(val);
			}
		}
		else if (ReportDesignerControl != null)
		{
			ReportDesignerControl.SetFontSizeValue(val);
		}
		if (PropertyPad.Grid != null)
		{
			((Control)(object)PropertyPad.Grid).Refresh();
		}
		return true;
	}

	public bool SetIsFontBoldValue(bool val)
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				WindowDesignerControl.SetIsFontBoldValue(val);
			}
		}
		else if (ReportDesignerControl != null)
		{
			ReportDesignerControl.SetIsFontBoldValue(val);
		}
		if (PropertyPad.Grid != null)
		{
			((Control)(object)PropertyPad.Grid).Refresh();
		}
		return true;
	}

	public bool SetIsFontItalicValue(bool val)
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				WindowDesignerControl.SetIsFontItalicValue(val);
			}
		}
		else if (ReportDesignerControl != null)
		{
			ReportDesignerControl.SetIsFontItalicValue(val);
		}
		if (PropertyPad.Grid != null)
		{
			((Control)(object)PropertyPad.Grid).Refresh();
		}
		return true;
	}

	public bool SetIsFontUnderlineValue(bool val)
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				WindowDesignerControl.SetIsFontUnderlineValue(val);
			}
		}
		else if (ReportDesignerControl != null)
		{
			ReportDesignerControl.SetIsFontUnderlineValue(val);
		}
		if (PropertyPad.Grid != null)
		{
			((Control)(object)PropertyPad.Grid).Refresh();
		}
		return true;
	}

	public override void Undo()
	{
		bool isRefreshRequired = false;
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null && WindowDesignerControl.UndoEngine != null)
			{
				WindowDesignerControl.PreUndoActions();
				string unitName = string.Empty;
				if (WindowDesignerControl.IsInAppGen)
				{
					unitName = WindowDesignerControl.UndoEngine.GetTopUnitName(isUndo: true);
				}
				WindowDesignerControl.UndoEngine.Undo();
				WindowDesignerControl.PostUndoActions(unitName, ref isRefreshRequired);
				if (isRefreshRequired)
				{
					RefreshPads();
				}
			}
		}
		else if (ReportDesignerControl != null && ReportDesignerControl.UndoEngine != null)
		{
			ReportDesignerControl.PreUndoActions();
			string unitName2 = string.Empty;
			if (ReportDesignerControl.BaseDesignerControl.IsInAppGen)
			{
				unitName2 = ReportDesignerControl.UndoEngine.GetTopUnitName(isUndo: true);
			}
			ReportDesignerControl.UndoEngine.Undo();
			ReportDesignerControl.PostUndoActions(unitName2, ref isRefreshRequired);
			if (isRefreshRequired)
			{
				RefreshPads();
			}
		}
	}

	public override void Redo()
	{
		bool isRefreshRequired = false;
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null && WindowDesignerControl.UndoEngine != null)
			{
				WindowDesignerControl.PreRedoActions();
				string unitName = string.Empty;
				if (WindowDesignerControl.IsInAppGen)
				{
					unitName = WindowDesignerControl.UndoEngine.GetTopUnitName(isUndo: false);
				}
				WindowDesignerControl.UndoEngine.Redo();
				WindowDesignerControl.PostRedoActions(unitName, ref isRefreshRequired);
				if (isRefreshRequired)
				{
					RefreshPads();
				}
			}
		}
		else if (ReportDesignerControl != null && ReportDesignerControl.UndoEngine != null)
		{
			ReportDesignerControl.PreRedoActions();
			string unitName2 = string.Empty;
			if (ReportDesignerControl.BaseDesignerControl.IsInAppGen)
			{
				unitName2 = ReportDesignerControl.UndoEngine.GetTopUnitName(isUndo: false);
			}
			ReportDesignerControl.UndoEngine.Redo();
			ReportDesignerControl.PostRedoActions(unitName2, ref isRefreshRequired);
			if (isRefreshRequired)
			{
				RefreshPads();
			}
		}
	}

	public bool PreUndoActions()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PreUndoActions();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PreUndoActions();
		}
		return false;
	}

	public bool PreRedoActions()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PreRedoActions();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PreRedoActions();
		}
		return false;
	}

	public bool PreCutActions()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PreCutActions();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PreCutActions();
		}
		return false;
	}

	public bool PreCopyActions()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PreCopyActions();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PreCopyActions();
		}
		return false;
	}

	public bool PrePasteActions()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PrePasteActions();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PrePasteActions();
		}
		return false;
	}

	public bool PreDeleteActions(ref bool isRefreshRequired)
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PreDeleteActions(ref isRefreshRequired);
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PreDeleteActions(ref isRefreshRequired);
		}
		return false;
	}

	public bool PostUndoActions()
	{
		bool isRefreshRequired = false;
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PostUndoActions(string.Empty, ref isRefreshRequired);
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PostUndoActions(string.Empty, ref isRefreshRequired);
		}
		return false;
	}

	public bool PostRedoActions()
	{
		bool isRefreshRequired = false;
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PostRedoActions(string.Empty, ref isRefreshRequired);
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PostRedoActions(string.Empty, ref isRefreshRequired);
		}
		return false;
	}

	public bool PostCutActions()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PostCutActions();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PostCutActions();
		}
		return false;
	}

	public bool PostCopyActions()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PostCopyActions();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PostCopyActions();
		}
		return false;
	}

	public bool PostPasteActions()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PostPasteActions();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PostPasteActions();
		}
		return false;
	}

	public bool PostDeleteActions()
	{
		if (!IsReportDesigner)
		{
			if (WindowDesignerControl != null)
			{
				return WindowDesignerControl.PostDeleteActions();
			}
		}
		else if (ReportDesignerControl != null)
		{
			return ReportDesignerControl.PostDeleteActions();
		}
		return false;
	}

	protected SharpDevelopSideBar GetSideBar()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		SideBarView val = (SideBarView)WorkbenchSingleton.Workbench.GetPad(typeof(SideBarView)).PadContent;
		return (SharpDevelopSideBar)val.Control;
	}

	protected virtual void BuildToolBarItems(string sName)
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
			if (m_BuildSideTab != null)
			{
				m_BuildSideTab.Dispose();
			}
			if (IsReportDesigner)
			{
				BuildSideTab buildSideTab = new BuildSideTab(sideBar, isReport: true, ClaReportExternalControls.GetAllExternalControls(Report.C6COMPATIBLE_MODE));
				buildSideTab.CreateSidetabs(base.Host, IsWin);
				m_BuildSideTab = buildSideTab;
			}
			else
			{
				WindowSideTab windowSideTab = new WindowSideTab(sideBar, isReport: false, ClaWindowExternalControls.GetAllExternalControls(GeneralDesiner.C6COMPATIBLE_MODE));
				windowSideTab.CreateSidetabs(base.Host);
				m_BuildSideTab = windowSideTab;
			}
		}
		((Control)(object)sideBar).Refresh();
	}

	private bool RemoveFormDesignerTabs()
	{
		for (int num = ((SideBarControl)SideBarView.sideBar).Tabs.Count - 1; num > 0; num--)
		{
			SideTab val = ((SideBarControl)SideBarView.sideBar).Tabs[num];
			if (val.Name.IndexOf("Components") > 0 || val.Name == "Windows Forms" || val.Name == "Components" || val.Name == "Data" || val.Name == "Clarion.Net Controls")
			{
				((SideBarControl)SideBarView.sideBar).Tabs.Remove(val);
			}
		}
		return true;
	}

	private void RemoveSideBarItem(string namePart)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		if (sideTabItem != null)
		{
			SharpDevelopSideBar sideBar = GetSideBar();
			((SideBarControl)sideBar).Tabs.Remove(sideTabItem);
		}
		SideBarView val = (SideBarView)WorkbenchSingleton.Workbench.GetPad(typeof(SideBarView)).PadContent;
		SharpDevelopSideBar val2 = (SharpDevelopSideBar)val.Control;
		for (int num = ((SideBarControl)SideBarView.sideBar).Tabs.Count - 1; num > 0; num--)
		{
			SideTab val3 = ((SideBarControl)SideBarView.sideBar).Tabs[num];
			if (val3.Name.IndexOf(namePart) > 0)
			{
				((SideBarControl)SideBarView.sideBar).Tabs.Remove(val3);
			}
		}
		((Control)(object)val2).Refresh();
	}

	public bool StartSetTabOrder()
	{
		if (IsReportDesigner)
		{
			if (BaseReportDesignerControl != null)
			{
				BaseReportDesignerControl.StartSetTabOrder();
			}
		}
		else if (WindowDesignerControl != null)
		{
			WindowDesignerControl.StartSetTabOrder();
		}
		return true;
	}

	public bool ResetTabOrder()
	{
		if (IsReportDesigner)
		{
			if (BaseReportDesignerControl != null)
			{
				BaseReportDesignerControl.ResetTabOrder();
			}
		}
		else if (WindowDesignerControl != null)
		{
			WindowDesignerControl.StartSetTabOrder();
		}
		return true;
	}

	public static bool AddEvaluator(string name, IConditionEvaluator evaluator)
	{
		if (AddInTree.ConditionEvaluators.ContainsKey(name))
		{
			AddInTree.ConditionEvaluators[name] = evaluator;
		}
		else
		{
			AddInTree.ConditionEvaluators.Add(name, evaluator);
		}
		return true;
	}

	protected virtual bool InitBeforeControls()
	{
		return true;
	}

	private void PropertyChanged(object sender, EventArgs e)
	{
		SetDirty(dirty: true);
	}

	private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
	{
		SetDirty(dirty: true);
	}

	private void SelectionChangedHandler(object sender, EventArgs args)
	{
		UpdatePropertiesToolbar(sender, args);
	}

	private void OnDesignerDirty(object sender, EventArgs e)
	{
		SetDirty(dirty: true);
	}

	public void SetDirty(bool dirty)
	{
		IsDirty = dirty;
	}

	public bool PrintPreview()
	{
		if (IsReportDesigner && ReportDesignerControl != null)
		{
			ReportDesignerControl.PrintPreview();
		}
		return true;
	}

	public bool WindowPreview()
	{
		if (!IsReportDesigner && WindowDesignerControl != null)
		{
			WindowDesignerControl.WindowPreview();
		}
		return true;
	}

	public string GetWindowText()
	{
		if (!IsReportDesigner && WindowDesignerControl != null)
		{
			return WindowDesignerControl.GetWindowText();
		}
		return string.Empty;
	}

	public override void Deselecting()
	{
		if (alreadyShown)
		{
			CloseDesigner();
		}
	}

	public override void Deselected()
	{
		WorkbenchWindow_WindowDeselected(null, null);
	}

	public bool OnBackClick()
	{
		if (IsDirty)
		{
			DialogResult dialogResult = DialogResult.Yes;
			if (IsReportDesigner ? ReportGeneralOptionsPanel.AskforSaveonClose : WindowGeneralOptionsPanel.AskforSaveonClose)
			{
				dialogResult = MessageBox.Show(ResourceService.GetString("MainWindow.SaveChangesMessage"), ResourceService.GetString("MainWindow.SaveChangesMessageHeader") + (IsReportDesigner ? " Report Designer?" : " Window Designer?"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			}
			switch (dialogResult)
			{
			case DialogResult.Yes:
				return true;
			case DialogResult.No:
				IsDirty = false;
				((ICanBeDirty)viewContent).IsDirty = m_IsTextEditorDirty;
				return true;
			case DialogResult.Cancel:
				return false;
			}
		}
		return true;
	}

	protected virtual void PostOnBackClick()
	{
		if (m_PropertiesToolStrip != null)
		{
			m_PropertiesToolStrip.Dispose();
			m_PropertiesToolStrip = null;
		}
		if (IsCompilerResults)
		{
			if (m_errorText != null)
			{
				m_errorText.Dispose();
				m_errorText = null;
			}
			if (m_errortitle != null)
			{
				m_errortitle.Dispose();
				m_errortitle = null;
			}
			return;
		}
		IComponentChangeService componentChangeService = (IComponentChangeService)base.Host.GetService(typeof(IComponentChangeService));
		if (componentChangeService != null)
		{
			componentChangeService.ComponentChanged -= OnComponentChanged;
			componentChangeService.ComponentRemoved -= base.ComponentRemoved;
		}
		AddEvaluator("ClaStructureIsWindow", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
		AddEvaluator("ClaStructureIsReport", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
		if (IsReportDesigner)
		{
			AddEvaluator("ClaReportSelectedEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportMainDeleteEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportMainPasteEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportCollapseSectionEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportExpandSectionEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportPageHeaderEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportPageFooterEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportPageFormEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportGroupHeaderEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportGroupFooterEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportPastControlEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportDelSectionEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportSurroundBreakEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportDetailEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportBreakGroupEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportControlActionEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportPasteControlEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportCollapseAllShown", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportCollapseAllEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportExpandAllEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportFormatEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaReportPopulateEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowBringToFrontEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowSendToBackEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowTabOrderEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowPasteEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowCopyEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowActionEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowCenterMenuEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowFormatMenuEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			if (ReportDesignerControl != null)
			{
				ReportDesignerControl.RemoveReportEventsHandlers();
			}
		}
		else
		{
			AddEvaluator("ClaWindowPopulateEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowBringToFrontEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowSendToBackEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowTabOrderEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowPasteEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowCopyEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowActionEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowCenterMenuEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			AddEvaluator("ClaWindowFormatMenuEnabled", (IConditionEvaluator)(object)new ClaReportEvaluatorBase());
			if (WindowDesignerControl != null)
			{
				WindowDesignerControl.RemoveReportEventsHandlers();
			}
		}
		if (PropertyPad.Grid != null)
		{
			((VisualHint.SmartPropertyGrid.PropertyGrid)(object)PropertyPad.Grid).SelectedObject = null;
		}
		if (WindowGeneralOptionsPanel.ShowPropertiesToolbar || ReportGeneralOptionsPanel.ShowPropertiesToolbar)
		{
			ISelectionService selectionService = (ISelectionService)base.Host.GetService(typeof(ISelectionService));
			selectionService.SelectionChanged -= SelectionChangedHandler;
		}
		if (toolbarUpdateTimer != null)
		{
			toolbarUpdateTimer.Tick -= UpdateWindowToolbar;
			toolbarUpdateTimer.Dispose();
			toolbarUpdateTimer = null;
		}
		if (LargeDesignAreaPanel != null)
		{
			LargeDesignAreaPanel.Dispose();
			m_largeDesignAreaPanel = null;
		}
		RemoveSideBarItem(IsReportDesigner ? "Report" : "Window");
		if (IsReportDesigner)
		{
			if (ReportDesignerControl != null)
			{
				ReportDesignerControl.DisposeUndoEngine();
			}
		}
		else if (WindowDesignerControl != null)
		{
			WindowDesignerControl.DisposeUndoEngine();
		}
		if (WindowKeyHandler != null)
		{
			System.Windows.Forms.Application.RemoveMessageFilter(WindowKeyHandler);
		}
	}

	public override void MergeFormChanges()
	{
		if (!failedDesignerInitialize && IsDirty)
		{
			bool flag = ((ICanBeDirty)viewContent).IsDirty;
			LoggingService.Info((object)"Merging form changes...");
			Save();
			LoggingService.Info((object)"Finished merging form changes");
			((ICanBeDirty)viewContent).IsDirty = flag;
		}
	}

	public virtual void Save()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Invalid comparison between Unknown and I4
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		Cursor.Current = Cursors.WaitCursor;
		string indentationString = Tab.GetIndentationString(((TextEditorControlBase)base.TextEditorControl).Document);
		string text = (IsReportDesigner ? ReportDesignerControl.ReportToFile(IndentStyle, indentationString) : ClaWindowManager.GetWindowText(base.Host, IndentStyle, indentationString));
		int num = ReportEndLine;
		if (ControlContainerDecl != null)
		{
			num = (m_IsTemplate ? ((TextEditorControlBase)base.TextEditorControl).ActiveTextAreaControl.Caret.Line : (ControlContainerDecl.Pos.Line - 1));
		}
		LineSegment lineSegment = ((TextEditorControlBase)base.TextEditorControl).Document.GetLineSegment(num);
		int totalNumberOfLines = ((TextEditorControlBase)base.TextEditorControl).Document.TotalNumberOfLines;
		LineSegment lineSegment2 = ((TextEditorControlBase)base.TextEditorControl).Document.GetLineSegment((ReportEndLine < totalNumberOfLines) ? ReportEndLine : (totalNumberOfLines - 1));
		((TextEditorControlBase)base.TextEditorControl).Document.UndoStack.StartUndoGroup();
		((TextEditorControlBase)base.TextEditorControl).Document.Replace(lineSegment.Offset, lineSegment2.Offset + lineSegment2.Length - lineSegment.Offset, text);
		int totalNumberOfLines2 = ((TextEditorControlBase)base.TextEditorControl).Document.TotalNumberOfLines;
		ReportEndLine = ReportEndLine + totalNumberOfLines2 - totalNumberOfLines;
		if ((int)IndentStyle == 2)
		{
			((TextEditorControlBase)base.TextEditorControl).Document.FormattingStrategy.IndentLines(((TextEditorControlBase)base.TextEditorControl).ActiveTextAreaControl.TextArea, num, ReportEndLine);
		}
		((TextEditorControlBase)base.TextEditorControl).Document.UndoStack.EndUndoGroup();
		((TextEditorControlBase)base.TextEditorControl).ActiveTextAreaControl.Caret.Position = new TextLocation(0, num);
		((TextEditorControlBase)base.TextEditorControl).ActiveTextAreaControl.ScrollTo((ReportEndLine - num > 10) ? (num + 10) : ReportEndLine);
		SetDirty(dirty: false);
		((Control)(object)((TextEditorControlBase)base.TextEditorControl).ActiveTextAreaControl).Focus();
		Cursor.Current = Cursors.Default;
		ParserService.UpdateFileOnNextParserStep(ControlContainerDecl.CurrentModule.Name);
		IsDirty = false;
	}

	public override void Dispose()
	{
		if (m_BuildSideTab != null)
		{
			m_BuildSideTab.Dispose();
		}
		IsDirty = false;
		base.Dispose();
	}

	public override Properties CreateMemento()
	{
		return null;
	}

	public override void SetMemento(Properties memento)
	{
	}

	public bool SelectRootComponent()
	{
		if (base.Host != null && PropertyPad.Grid != null)
		{
			((VisualHint.SmartPropertyGrid.PropertyGrid)(object)PropertyPad.Grid).SelectedObject = base.Host.RootComponent;
			BaseDesignerControl.ExpandCompositeProperties((PropertyGridSV)(object)PropertyPad.Grid);
		}
		return true;
	}

	public bool BackToSource()
	{
		if (OnBackClick())
		{
			CloseDesigner();
			return true;
		}
		return false;
	}

	public void SaveAndExit()
	{
		if (!IsDirty)
		{
			if (IsReportDesigner)
			{
				if (ReportDesignerControl != null && ReportDesignerControl.IsSaveRequiredIfNotDirty)
				{
					bool flag = (((ICanBeDirty)viewContent).IsDirty = true);
					IsDirty = flag;
				}
			}
			else if (WindowDesignerControl != null && WindowDesignerControl.IsSaveRequiredIfNotDirty)
			{
				bool flag3 = (((ICanBeDirty)viewContent).IsDirty = true);
				IsDirty = flag3;
			}
		}
		CloseDesigner();
	}

	public bool ExitNotSave()
	{
		bool flag = true;
		if (IsDirty)
		{
			switch (MessageBox.Show("Are you sure you want to cancel?", "Exit from" + (IsReportDesigner ? " Report Designer?" : " Window Designer?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				IsDirty = false;
				((ICanBeDirty)viewContent).IsDirty = m_IsTextEditorDirty;
				flag = true;
				break;
			case DialogResult.No:
				flag = false;
				break;
			}
		}
		if (flag)
		{
			CloseDesigner();
		}
		return flag;
	}

	protected override void UpdatePropertyPadSelection(ISelectionService selectionService)
	{
		if (IsReportDesigner)
		{
			if (ReportDesignerControl != null)
			{
				ReportDesignerControl.UpdatePropertyPadSelection(selectionService);
				if (PropertyPad.Grid != null)
				{
					BaseDesignerControl.ExpandCompositeProperties((PropertyGridSV)(object)PropertyPad.Grid);
				}
			}
		}
		else if (WindowDesignerControl != null)
		{
			WindowDesignerControl.UpdatePropertyPadSelection(selectionService);
			if (PropertyPad.Grid != null)
			{
				BaseDesignerControl.ExpandCompositeProperties((PropertyGridSV)(object)PropertyPad.Grid);
			}
		}
		else
		{
			base.UpdatePropertyPadSelection(selectionService);
		}
	}

	void IClipboardHandler.Copy()
	{
		if (IsPropertybarFocused())
		{
			DoPropertybarAction(StandardCommands.Copy);
			return;
		}
		PreCopyActions();
		Copy();
		PostCopyActions();
	}

	void IClipboardHandler.Paste()
	{
		if (IsPropertybarFocused())
		{
			DoPropertybarAction(StandardCommands.Paste);
			return;
		}
		PrePasteActions();
		Paste();
		PostPasteActions();
	}

	void IClipboardHandler.Delete()
	{
		bool isRefreshRequired = false;
		if (PreDeleteActions(ref isRefreshRequired))
		{
			Delete();
		}
		PostDeleteActions();
		if (isRefreshRequired)
		{
			RefreshPads();
		}
	}

	void IClipboardHandler.SelectAll()
	{
		SelectAll();
	}

	void IClipboardHandler.Cut()
	{
		if (IsPropertybarFocused())
		{
			DoPropertybarAction(StandardCommands.Cut);
			return;
		}
		PreCutActions();
		Cut();
		PostCutActions();
	}

	public virtual bool RefreshPads()
	{
		return true;
	}

	public ControlContainer ParseControlString(string template)
	{
		return ParseControlString(template, IsReportDesigner);
	}

	public static ControlContainer ParseControlString(string template, bool isReportDesigner)
	{
		if (string.IsNullOrEmpty(template))
		{
			return null;
		}
		string controlStringInContext = GetControlStringInContext(template, !isReportDesigner);
		ClarionType structType;
		CompilerResults compRes;
		return CommonIDEParser.ParseStructure("c:\\ClaReportDesigner_Dummy_.cln", controlStringInContext, 3, 0, extract: false, isWin: true, out structType, out compRes);
	}

	private static string GetControlStringInContext(string template, bool isWindow)
	{
		if (isWindow)
		{
			return " MEMBER\r\nW    WINDOW('Designer Requester'),AT(,,273,146),SYSTEM,GRAY,RESIZE,AUTO\r\n" + template + " END\r\n";
		}
		return " MEMBER\r\nReport  REPORT,AT(1000,2000,6250,7688), FONT('Arial', 10,,, CHARSET:ANSI),PRE(RPT), PAPER(PAPER:A4),THOUS\r\n  HEADER,  AT(1000,1000,6250,1000), USE(?HEADER), COLOR(COLOR:WINDOW)\r\n" + template + " END\r\n END\r\n";
	}

	public override void ShowTabOrder()
	{
		if (!IsTabOrderMode)
		{
			IDesignerHost designerHost = (IDesignerHost)base.DesignSurface.GetService(typeof(IDesignerHost));
			if (designerHost != null)
			{
				bool isWindow = !IsReportDesigner;
				tabOrder = new TabOrder(designerHost, isWindow);
			}
			IsTabOrderMode = true;
		}
	}

	public override void HideTabOrder()
	{
		if (IsTabOrderMode)
		{
			if (tabOrder != null)
			{
				tabOrder.Dispose();
				tabOrder = null;
			}
			IsTabOrderMode = false;
		}
	}
}

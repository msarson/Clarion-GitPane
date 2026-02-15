using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Clarion.ASL;
using Clarion.GEN;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TextEditor;
using SoftVelocity.CWPInvoke;
using SoftVelocity.Common.ClarionEditor;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.Controls;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.DataDictionary.Design;
using SoftVelocity.DataDictionary.FileSchemaEditor;
using SoftVelocity.DataDictionary.Schema;
using SoftVelocity.Generator.Commands;
using SoftVelocity.Generator.Editor;
using SoftVelocity.Generator.PWEE;

namespace SoftVelocity.Generator.UI;

public class ApplicationMainWindowControl_ViewContent : CWControl_ViewContent, IAppViewContentEvents, ITextEditorControlProvider, IPrintable, IPositionable, IFileSchemaProvider, IFileSchemaPadController, IAppTitleManager, IPweeProvider
{
	internal class APPPrintDocument : IDisposable
	{
		private Font printFont;

		private int totalPrintRecords;

		private int totalPrintedRecords;

		private int pageCount;

		private Application APP;

		private PrintDocument _PrintDocument;

		private Queue<string> bufferedLines = new Queue<string>();

		private int linesPerPageCount;

		private float linesPerPage;

		private Procedure[] Procedures;

		private List<string> calledProcedures = new List<string>();

		private bool isDisposed;

		private bool notfirstYet = true;

		public PrintDocument PrintDocument => _PrintDocument;

		public APPPrintDocument(Application pAPP)
		{
			printFont = new Font("Courier New", 12f);
			APP = pAPP;
			_PrintDocument = new PrintDocument();
			_PrintDocument.PrintPage += OnDctPrintPage;
			_PrintDocument.BeginPrint += OnDctBeginPrint;
			_PrintDocument.EndPrint += OnDctEndPrint;
		}

		public void Dispose()
		{
			if (!isDisposed)
			{
				_PrintDocument.PrintPage -= OnDctPrintPage;
				_PrintDocument.BeginPrint -= OnDctBeginPrint;
				_PrintDocument.EndPrint -= OnDctEndPrint;
				printFont = null;
				totalPrintRecords = 0;
				totalPrintedRecords = 0;
				pageCount = 0;
				APP = null;
				_PrintDocument = null;
				bufferedLines.Clear();
				bufferedLines = null;
			}
			isDisposed = true;
		}

		private void OnDctEndPrint(object sender, PrintEventArgs e)
		{
		}

		private void OnDctBeginPrint(object sender, PrintEventArgs e)
		{
			Procedures = APP.Procedures;
			totalPrintRecords = Procedures.Length;
			totalPrintedRecords = 0;
			pageCount = 0;
			linesPerPage = 0f;
			linesPerPageCount = 0;
		}

		private void PrintTextLine(string text, PrintPageEventArgs e)
		{
			if (text == null)
			{
				text = "";
			}
			if ((float)linesPerPageCount < linesPerPage)
			{
				e.Graphics.DrawString(text, printFont, Brushes.Black, e.MarginBounds.Left, (float)e.MarginBounds.Top + (float)linesPerPageCount * printFont.GetHeight(e.Graphics), new StringFormat());
				linesPerPageCount++;
			}
			else
			{
				bufferedLines.Enqueue(text);
			}
		}

		private void OnDctPrintPage(object sender, PrintPageEventArgs e)
		{
			pageCount++;
			linesPerPageCount = 0;
			string text = null;
			text = "Page # " + pageCount + " - File Name: " + APP.FileName;
			if (linesPerPage == 0f)
			{
				linesPerPage = (float)e.MarginBounds.Height / printFont.GetHeight(e.Graphics);
			}
			PrintTextLine(text, e);
			PrintTextLine("------------------------------------------------------------", e);
			PrintTextLine("", e);
			while (bufferedLines.Count > 0 && (float)linesPerPageCount < linesPerPage)
			{
				PrintTextLine(bufferedLines.Dequeue(), e);
			}
			while ((float)linesPerPageCount < linesPerPage && totalPrintedRecords < totalPrintRecords)
			{
				Procedure firstProcedures;
				if (notfirstYet)
				{
					notfirstYet = false;
					firstProcedures = APP.FirstProcedures;
					if (firstProcedures != null)
					{
						PrintProc(firstProcedures, 0, e, null);
					}
				}
				totalPrintedRecords++;
				firstProcedures = Procedures[totalPrintedRecords - 1];
				if (!calledProcedures.Contains(firstProcedures.Name))
				{
					PrintProc(firstProcedures, 0, e, null);
				}
			}
			if (totalPrintedRecords < totalPrintRecords || bufferedLines.Count > 0)
			{
				e.HasMorePages = true;
			}
			else
			{
				e.HasMorePages = false;
			}
		}

		private void PrintProc(Procedure currentProc, int indet, PrintPageEventArgs e, List<string> callchain)
		{
			if (callchain == null)
			{
				callchain = new List<string>();
			}
			string text = "";
			text = ((indet <= 0) ? "" : new string(' ', indet * 4));
			if (!calledProcedures.Contains(currentProc.Name))
			{
				calledProcedures.Add(currentProc.Name);
			}
			text += currentProc.Name;
			if (callchain.Contains(currentProc.Name))
			{
				text += " (recursive)";
			}
			PrintTextLine(text, e);
			if (!callchain.Contains(currentProc.Name))
			{
				callchain.Add(currentProc.Name);
				Procedure[] array = currentProc.CalledProcedures;
				foreach (Procedure currentProc2 in array)
				{
					PrintProc(currentProc2, indet + 1, e, callchain);
				}
				callchain.Remove(currentProc.Name);
			}
		}
	}

	private struct EmbedInfo
	{
		public string App;

		public string Module;

		public string Procedure;

		public string Embed;

		public string Title;

		public EmbedInfo(string infoString)
		{
			App = "";
			Module = "";
			Procedure = "";
			Embed = "";
			Title = "";
			Match match = Regex.Match(infoString, "Section://(?<app>[^/]+)/(?<module>[^/]+)/?(?<procedure>[^/]+)? Embed://(?<embed>.+)", RegexOptions.None);
			if (match.Success)
			{
				Group obj = match.Groups["app"];
				Group obj2 = match.Groups["module"];
				Group obj3 = match.Groups["procedure"];
				Group obj4 = match.Groups["embed"];
				if (obj.Success)
				{
					App = obj.Value;
				}
				if (obj2.Success)
				{
					Module = obj2.Value;
				}
				if (obj4.Success)
				{
					Embed = obj4.Value;
				}
				if (obj3.Success)
				{
					Procedure = obj3.Value;
					Title = "(" + Module + ") - " + Procedure + " - " + Embed;
				}
				else
				{
					Title = "(" + Module + ") - " + Embed;
				}
			}
		}
	}

	private struct EditorInfo
	{
		public IEditorDetails Generator;

		public string Name;

		public bool ReadOnly;

		public uint InitialLine;

		public bool EditingErrors;

		public bool EditingStructure;
	}

	private Application app;

	private SharpDevelopTextAreaControl backgroundEditor;

	private IGeneratorEditorDialog currentEditorDialog;

	private LoadingAppMessagePanel _LoadingAppMessagePanel;

	private AppHeaderLabel _AppHeaderLabel;

	private WaitPanel _WaitPanel;

	private bool? appIsReadOnly = null;

	private ApplicationContainer _ApplicationContainer
	{
		get
		{
			return (ApplicationContainer)_Container;
		}
		set
		{
			_Container = value;
		}
	}

	public override Control Control => _ApplicationContainer;

	public string Language
	{
		get
		{
			if (app != null)
			{
				return app.Language;
			}
			return string.Empty;
		}
	}

	public Application App => app;

	protected override int InstID
	{
		get
		{
			if (app == null)
			{
				return base.InstID;
			}
			return app.InstID;
		}
	}

	TextEditorControl ITextEditorControlProvider.TextEditorControl
	{
		get
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (((AbstractBaseViewContent)this).WorkbenchWindow != null && (object)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent != this && ((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent is ITextEditorControlProvider)
			{
				return ((ITextEditorControlProvider)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent).TextEditorControl;
			}
			return (TextEditorControl)(object)backgroundEditor;
		}
	}

	PrintDocument IPrintable.PrintDocument
	{
		get
		{
			if (App == null)
			{
				return null;
			}
			APPPrintDocument aPPPrintDocument = new APPPrintDocument(App);
			return aPPPrintDocument.PrintDocument;
		}
	}

	public UIBindingInterfaceKind ActiveWindowInterface
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (_Container != null)
			{
				CWWindow viewControl = _Container._ViewControl;
				if (viewControl != null)
				{
					return viewControl.UIKind;
				}
			}
			return (UIBindingInterfaceKind)0;
		}
	}

	public override bool IsDirty
	{
		get
		{
			if (app != null)
			{
				return app.IsDirty;
			}
			return ((AbstractViewContent)this).IsDirty;
		}
		set
		{
			((AbstractViewContent)this).IsDirty = value;
		}
	}

	public string HeaderTitle
	{
		get
		{
			if (_AppHeaderLabel != null)
			{
				return _AppHeaderLabel.HeaderTitle;
			}
			return "";
		}
	}

	public override bool IsReadOnly
	{
		get
		{
			if (!appIsReadOnly.HasValue)
			{
				if (string.IsNullOrEmpty(((AbstractViewContent)this).FileName))
				{
					return false;
				}
				appIsReadOnly = (File.GetAttributes(((AbstractViewContent)this).FileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
			}
			return appIsReadOnly.Value;
		}
	}

	public FileSchema FileSchema
	{
		get
		{
			if (app != null)
			{
				return app.FileSchema;
			}
			return null;
		}
	}

	public bool ShowKeys => false;

	public DictionaryItemClipboardFormat ClipboardFormat => DictionaryItemClipboardFormat.Simple;

	public bool DisplayColumns => true;

	public bool DisplayKeys => false;

	int IPositionable.Line
	{
		get
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (((AbstractBaseViewContent)this).WorkbenchWindow != null && (object)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent != this && ((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent is IPositionable)
			{
				return ((IPositionable)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent).Line;
			}
			return 0;
		}
	}

	int IPositionable.Column
	{
		get
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (((AbstractBaseViewContent)this).WorkbenchWindow != null && (object)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent != this && ((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent is IPositionable)
			{
				return ((IPositionable)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent).Column;
			}
			return 0;
		}
	}

	public override void Dispose()
	{
		if (backgroundEditor != null)
		{
			((Component)(object)backgroundEditor).Dispose();
			backgroundEditor = null;
		}
		base.Dispose();
	}

	protected override void DoWorkbenchWindowChanged()
	{
		CreateLoadingAppMessagePanel();
	}

	protected override void DoCloseEvent()
	{
		if (app != null)
		{
			app.SetIsOnApptree(value: false);
			app.Closed -= app_Closing;
			app.IsDirtyChanged -= app_IsDirtyChanged;
			app.IsBusyChanged -= app_IsBusyChanged;
			app = null;
		}
		if (_ApplicationContainer != null)
		{
			_ApplicationContainer.OnWorkbench_ViewClosed();
			_ApplicationContainer = null;
		}
		if (_AppHeaderLabel != null)
		{
			_AppHeaderLabel.Dispose();
			_AppHeaderLabel = null;
		}
		if (_WaitPanel != null)
		{
			_WaitPanel.Dispose();
			_WaitPanel = null;
		}
	}

	internal void DoClosingEvent(CancelEventArgs e)
	{
		switch (MessageBox.Show(WorkbenchSingleton.MainForm, "Do you want to save your changes?", "Save changes in " + ((AbstractViewContent)this).TitleName + "?", MessageBoxButtons.YesNoCancel))
		{
		case DialogResult.Yes:
			if (app.Save())
			{
				app.CloseEditSession();
			}
			else
			{
				e.Cancel = true;
			}
			break;
		case DialogResult.No:
			app.CloseEditSession();
			break;
		default:
			e.Cancel = true;
			break;
		}
	}

	public override void Load(string fileName)
	{
		if (ApplicationService.IsTemplateRegistryOpen)
		{
			ApplicationService.ThrowError(fileName, GeneratorError.RegistryInEdit);
			return;
		}
		Application application = ApplicationService.FetchApplication(fileName);
		if (application == null)
		{
			ApplicationService.ThrowError(fileName, GeneratorError.AppLoadFailed);
		}
		else if (application.InEdit)
		{
			ApplicationService.ThrowError(fileName, GeneratorError.AppIsAlreadyLoaded);
		}
		LoadApp(application);
	}

	public void LoadApp(Application _app)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		if (_app == null)
		{
			ApplicationService.ThrowError("", GeneratorError.AppLoadFailed);
		}
		app = _app;
		_ApplicationContainer = new ApplicationContainer(this);
		_ApplicationContainer.Location = new Point(0, 0);
		_ApplicationContainer.Dock = DockStyle.Fill;
		_ApplicationContainer.Visible = false;
		backgroundEditor = new SharpDevelopTextAreaControl();
		((AbstractViewContent)this).IsDirty = false;
		((AbstractViewContent)this).FileName = app.FileName;
		((AbstractViewContent)this).TitleName = Path.GetFileName(((AbstractViewContent)this).FileName);
		((AbstractViewContent)this).UntitledName = "";
		((TextEditorControlBase)backgroundEditor).Document.TextContent = string.Empty;
		((TextEditorControlBase)backgroundEditor).FileName = ((AbstractViewContent)this).FileName;
		Load();
	}

	private void Load()
	{
		System.Windows.Forms.Application.DoEvents();
		CWDialogService.Instance.CreateHost += Win32WindowOpen;
		CWDialogService.Instance.ValidateView += ValidateObject;
		app.IsDirtyChanged += app_IsDirtyChanged;
		app.IsBusyChanged += app_IsBusyChanged;
		FileSchemaPad.EnsurePadExist();
		if (!app.Edit())
		{
			CWDialogService.Instance.CreateHost -= Win32WindowOpen;
			CWDialogService.Instance.ValidateView -= ValidateObject;
			_ApplicationContainer = null;
			string fileName = app.FileName;
			app = null;
			ApplicationService.ThrowError(fileName, GeneratorError.AppLoadFailed);
		}
		else
		{
			ApplicationService.GenerationStarting += ApplicationService_GenerationStarting;
			ApplicationService.GenerationEnded += ApplicationService_GenerationEnded;
			app.SetIsOnApptree(value: true);
		}
	}

	private void ApplicationService_GenerationStarting(object sender, GenerationStartEventArgs e)
	{
		CreateWaitPanel();
		ShowWaitPanel();
	}

	private void ShowWaitPanel()
	{
		if (_WaitPanel != null)
		{
			_WaitPanel.DelayStart = true;
			_WaitPanel.ShowWaitPanel();
		}
	}

	private void HideWaitPanel()
	{
		if (_WaitPanel != null)
		{
			_WaitPanel.HideWaitPanel();
		}
	}

	private void ApplicationService_GenerationEnded(object sender, GenerationEndEventArgs e)
	{
		HideWaitPanel();
	}

	public override void Save(string fileName)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		((AbstractViewContent)this).OnSaving((EventArgs)null);
		bool flag = false;
		if (app != null)
		{
			flag = app.SaveAs(fileName);
			((AbstractViewContent)this).OnSaved(new SaveEventArgs(flag));
			if (flag)
			{
				((AbstractViewContent)this).IsDirty = false;
				((AbstractViewContent)this).FileName = app.FileName;
				((AbstractViewContent)this).TitleName = Path.GetFileName(((AbstractViewContent)this).FileName);
			}
		}
	}

	public override void Save()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		((AbstractViewContent)this).OnSaving((EventArgs)null);
		bool flag = false;
		if (app != null)
		{
			flag = app.Save();
			((AbstractViewContent)this).OnSaved(new SaveEventArgs(flag));
		}
		RefreshSortApplicationsPadAsyncMenuCommand.DoRun();
	}

	internal override void AllControlsClosed()
	{
		CWDialogService.Instance.CreateHost -= Win32WindowOpen;
		CWDialogService.Instance.ValidateView -= ValidateObject;
		ApplicationService.GenerationStarting -= ApplicationService_GenerationStarting;
		ApplicationService.GenerationEnded -= ApplicationService_GenerationEnded;
		base.AllControlsClosed();
		RefreshSortApplicationsPadAsyncMenuCommand.DoRun();
	}

	protected override void OnSecondaryContentsSaving()
	{
		ISecondaryViewContent val = null;
		bool flag = false;
		foreach (ISecondaryViewContent secondaryViewContent in ((AbstractViewContent)this).SecondaryViewContents)
		{
			if (secondaryViewContent is CommonGenEditor && !flag)
			{
				val = secondaryViewContent;
				continue;
			}
			secondaryViewContent.NotifyBeforeSave();
			if (secondaryViewContent is CommonClarionGenDesignerView)
			{
				flag = true;
				if (val != null)
				{
					val.NotifyBeforeSave();
				}
			}
		}
	}

	public IGeneratorDialog OpenWindowFormatter(string name, IFormatter generator)
	{
		System.Windows.Forms.Application.DoEvents();
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<IFormatter, string, IGeneratorDialog>((Func<IFormatter, string, IGeneratorDialog>)OpenStructureFormatterInternal, generator, app.Language);
		}
		return OpenStructureFormatterInternal(generator, app.Language);
	}

	public IGeneratorDialog OpenReportFormatter(string name, IFormatter generator)
	{
		System.Windows.Forms.Application.DoEvents();
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<IFormatter, string, IGeneratorDialog>((Func<IFormatter, string, IGeneratorDialog>)OpenStructureFormatterInternal, generator, app.Language);
		}
		return OpenStructureFormatterInternal(generator, app.Language);
	}

	private IGeneratorDialog OpenStructureFormatterInternal(IFormatter generator, string language)
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent == null)
		{
			return null;
		}
		IViewContent viewContent = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent;
		CommonClarionGenDesignerView commonClarionGenDesignerView = null;
		for (int i = 0; i < viewContent.SecondaryViewContents.Count; i++)
		{
			if (viewContent.SecondaryViewContents[i] is CommonClarionGenDesignerView)
			{
				commonClarionGenDesignerView = (CommonClarionGenDesignerView)(object)viewContent.SecondaryViewContents[i];
				if (commonClarionGenDesignerView.IsAppGenDesigner)
				{
					break;
				}
			}
		}
		if (commonClarionGenDesignerView == null)
		{
			return null;
		}
		string fileContent = "\tMEMBER\r\n" + generator.Data;
		Cursor.Current = Cursors.WaitCursor;
		ClarionType structType;
		CompilerResults compRes;
		ReportDeclaration rcd = CommonIDEParser.ParseStructure("c:\\AppGen__DummyFile_.clw", fileContent, 2, 1, extract: false, language.Equals("Clarion", StringComparison.InvariantCultureIgnoreCase), out structType, out compRes);
		Cursor.Current = Cursors.Default;
		app.SuspendGenerate();
		generator.DialogClosed += OnDialogClosed;
		if (!commonClarionGenDesignerView.ShowDesigner(rcd, compRes, structType != ClarionType.REPORT, structType != ClarionType.APPLICATION, generator))
		{
			return null;
		}
		return commonClarionGenDesignerView;
	}

	private void OnDialogClosed(object sender, EventArgs e)
	{
		if (sender != null && sender is IGenerator)
		{
			((IGenerator)sender).DialogClosed -= OnDialogClosed;
		}
		if (app != null)
		{
			app.ResumeGenerate();
			return;
		}
		throw new Exception("The app is null");
	}

	public IGeneratorEditorDialog OpenWindowReportEditor(string name, IEmbedEditorDetails generator)
	{
		return OpenEditor(new EditorInfo
		{
			Generator = generator,
			Name = name,
			InitialLine = 0u,
			ReadOnly = false,
			EditingErrors = false,
			EditingStructure = true
		});
	}

	public IGeneratorEditorDialog OpenEmbedEditor(string name, IEmbedEditorDetails generator)
	{
		return OpenEditor(new EditorInfo
		{
			Generator = generator,
			Name = name,
			InitialLine = 0u,
			ReadOnly = false,
			EditingErrors = App.CallingEditError,
			EditingStructure = false
		});
	}

	public IGeneratorEditorDialog OpenFileEditor(string name, bool readOnly, uint initialLine, IEditorDetails generator)
	{
		return OpenEditor(new EditorInfo
		{
			Generator = generator,
			Name = name,
			InitialLine = initialLine,
			ReadOnly = readOnly,
			EditingErrors = App.CallingEditError,
			EditingStructure = false
		});
	}

	public IGeneratorEditorDialog OpenPwee(IPweeDetails generator)
	{
		return OpenEditor(new EditorInfo
		{
			Generator = generator,
			Name = string.Empty,
			InitialLine = 0u,
			ReadOnly = false,
			EditingErrors = App.CallingEditError,
			EditingStructure = false
		});
	}

	private IGeneratorEditorDialog OpenEditor(EditorInfo einfo)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<EditorInfo, IGeneratorEditorDialog>((Func<EditorInfo, IGeneratorEditorDialog>)OpenEditor, einfo);
		}
		System.Windows.Forms.Application.DoEvents();
		if (einfo.EditingErrors && ((int)ActiveWindowInterface != 6 || ((object)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent != this && !(((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent is CommonClarionEditor))))
		{
			MessageService.ShowMessage(ResourceService.GetString("Clarion.Generator.Error.SaveUncommittedChanges"));
			einfo.Generator.InformDialogClosed();
			if (App != null)
			{
				App.CallingEditError = false;
			}
			return null;
		}
		if (currentEditorDialog != null && !currentEditorDialog.TryClose())
		{
			einfo.Generator.InformDialogClosed();
			return null;
		}
		currentEditorDialog = null;
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent == null)
		{
			return null;
		}
		currentEditorDialog = OpenEditorThreaded(einfo);
		if (currentEditorDialog != null)
		{
			einfo.Generator.DialogClosed += EditorRequesterDialogClosed;
			if (einfo.Generator is IPweeDetails && !App.CallingEditError)
			{
				einfo.Generator.DialogClosed += OnDialogClosed;
				App.SuspendGenerate();
			}
		}
		else
		{
			einfo.Generator.InformDialogClosed();
			if (App != null)
			{
				App.CallingEditError = false;
			}
		}
		return currentEditorDialog;
	}

	private void EditorRequesterDialogClosed(object sender, EventArgs e)
	{
		if (sender != null && sender is IGenerator)
		{
			((IGenerator)sender).DialogClosed -= EditorRequesterDialogClosed;
		}
		if (App != null)
		{
			App.CallingEditError = false;
		}
		currentEditorDialog = null;
	}

	private IGeneratorEditorDialog OpenEditorThreaded(EditorInfo einfo)
	{
		IViewContent viewContent = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent;
		CommonGenEditor commonGenEditor = null;
		for (int i = 0; i < viewContent.SecondaryViewContents.Count; i++)
		{
			if (viewContent.SecondaryViewContents[i] is CommonGenEditor)
			{
				commonGenEditor = (CommonGenEditor)(object)viewContent.SecondaryViewContents[i];
				break;
			}
		}
		if (commonGenEditor == null)
		{
			return null;
		}
		if (einfo.Generator is IEmbedEditorDetails)
		{
			IProject iPrj = null;
			if (app.IsOnSolution)
			{
				iPrj = app.GetProjectServiceProject();
			}
			if (!commonGenEditor.ShowEditor((IEmbedEditorDetails)einfo.Generator, iPrj))
			{
				return null;
			}
			EmbedInfo embedInfo = new EmbedInfo(einfo.Name);
			if (einfo.EditingStructure)
			{
				AppendHeaderTitle("Structure Editor");
			}
			else if (string.IsNullOrEmpty(embedInfo.Procedure))
			{
				SetHeaderTitle("Embed Editor - " + embedInfo.Title);
			}
			else
			{
				SetHeaderTitle(embedInfo.Procedure + " - Embed Editor - (" + embedInfo.Module + ") - " + embedInfo.Embed);
			}
			commonGenEditor.EditorClosed += view_EditorClosed;
			return commonGenEditor;
		}
		if (einfo.Generator is IPweeDetails)
		{
			IProject iPrj2 = null;
			if (app.IsOnSolution)
			{
				iPrj2 = app.GetProjectServiceProject();
			}
			if (!commonGenEditor.ShowEditor((IPweeDetails)einfo.Generator, iPrj2))
			{
				return null;
			}
			try
			{
				if (einfo.Generator.Schema != null)
				{
					if (string.IsNullOrEmpty(einfo.Generator.Schema.ProcedureName))
					{
						SetHeaderTitle("Embeditor - (" + einfo.Generator.Schema.ModuleName + ")");
					}
					else
					{
						SetHeaderTitle(einfo.Generator.Schema.ProcedureName + " - Embeditor - (" + einfo.Generator.Schema.ModuleName + ")");
					}
				}
				else
				{
					SetHeaderTitle("Embeditor");
				}
			}
			catch
			{
				SetHeaderTitle("Embeditor - " + ((IPweeDetails)einfo.Generator).Module);
			}
			commonGenEditor.EditorClosed += view_EditorClosed;
			return commonGenEditor;
		}
		int num = 0;
		int num2 = (int)(einfo.InitialLine - 1);
		if (einfo.Generator.Errors != null && einfo.Generator.Errors.Length > 0)
		{
			num2 = einfo.Generator.Errors[0].Line - 1;
			num = einfo.Generator.Errors[0].Column - 1;
		}
		FileService.OpenFile(einfo.Name);
		IViewContent val = FileService.JumpToFilePosition(einfo.Name, num2, num);
		WorkbenchSingleton.SafeThreadAsyncCall<IViewContent>((Action<IViewContent>)SetEditorFocus, val);
		return null;
	}

	private void SetEditorFocus(IViewContent vc)
	{
		WorkbenchSingleton.MainForm.Select();
		WorkbenchSingleton.MainForm.Focus();
		((IBaseViewContent)vc).Control.Focus();
	}

	private void view_EditorClosed(object sender, EventArgs e)
	{
		RemoveCurrentHeaderTitle();
	}

	private void Win32WindowOpen(UINetBinding CWObj, UIBindingInterfaceKind kind)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (ValidObject(CWObj))
		{
			OpenGeneratorWindow(CWObj, kind);
		}
	}

	private void ValidateObject(UINetBinding CWObj, ref IViewContent content)
	{
		if (ValidObject(CWObj))
		{
			content = (IViewContent)(object)this;
		}
	}

	public void OpenGeneratorWindow(UINetBinding CWObj, UIBindingInterfaceKind kind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if ((int)kind == 6)
		{
			app.Closed += app_Closing;
			_ApplicationContainer.ApplicationWindowOpened += OnApplicationWindowOpened;
		}
		System.Windows.Forms.Application.DoEvents();
		((AbstractBaseViewContent)this).WorkbenchWindow.SelectWindow();
		_ApplicationContainer.OpenGeneratorWindow(CWObj, kind);
	}

	private void OnApplicationWindowOpened(object sender, EventArgs e)
	{
		if (_ApplicationContainer != null)
		{
			_ApplicationContainer.ApplicationWindowOpened -= OnApplicationWindowOpened;
			System.Windows.Forms.Application.DoEvents();
			if (_LoadingAppMessagePanel != null)
			{
				CloseLoadingAppMessagePanel();
				CreateAppHeaderLabel();
				SetHeaderTitle(app.Name);
				((AbstractBaseViewContent)this).WorkbenchWindow.SelectWindow();
			}
			ApplicationService.CanOpenEditor = true;
		}
	}

	private void app_Closing(object sender, ApplicationEventArgs e)
	{
		if (_ApplicationContainer != null)
		{
			_ApplicationContainer.ForceCancel();
		}
	}

	private void app_IsDirtyChanged(bool isDirtyValue)
	{
		((AbstractViewContent)this).IsDirty = isDirtyValue;
	}

	private void app_IsBusyChanged(bool isBusyValue)
	{
		if (isBusyValue)
		{
			CreateWaitPanel();
			ShowWaitPanel();
		}
		else
		{
			HideWaitPanel();
		}
	}

	private void CreateLoadingAppMessagePanel()
	{
		if (_LoadingAppMessagePanel == null && ((AbstractBaseViewContent)this).WorkbenchWindow != null)
		{
			_LoadingAppMessagePanel = new LoadingAppMessagePanel();
			if (((AbstractBaseViewContent)this).WorkbenchWindow is Form form)
			{
				form.Controls.Add(_LoadingAppMessagePanel);
			}
			_LoadingAppMessagePanel.ShowWaitPanel();
		}
	}

	private void CloseLoadingAppMessagePanel()
	{
		if (_LoadingAppMessagePanel != null)
		{
			_LoadingAppMessagePanel.HideWaitPanel();
			_LoadingAppMessagePanel.Dispose();
			_LoadingAppMessagePanel = null;
		}
	}

	private void CreateAppHeaderLabel()
	{
		if (_AppHeaderLabel == null && ((AbstractBaseViewContent)this).WorkbenchWindow != null)
		{
			_AppHeaderLabel = new AppHeaderLabel();
			if (((AbstractBaseViewContent)this).WorkbenchWindow is Form form)
			{
				form.Controls.Add(_AppHeaderLabel);
			}
		}
	}

	private void CreateWaitPanel()
	{
		if (_WaitPanel == null && ((AbstractBaseViewContent)this).WorkbenchWindow != null)
		{
			_WaitPanel = new WaitPanel();
			_WaitPanel.AlphaBlend = AlphaBlendType.Transparent;
			_WaitPanel.Dock = DockStyle.Fill;
			_WaitPanel.Location = new Point(0, 0);
			if (((AbstractBaseViewContent)this).WorkbenchWindow is Form form)
			{
				form.Controls.Add(_WaitPanel);
				form.Controls.SetChildIndex(_WaitPanel, 0);
			}
		}
	}

	public void RemoveCurrentHeaderTitle()
	{
		if (_AppHeaderLabel != null)
		{
			_AppHeaderLabel.RemoveCurrentHeaderTitle();
		}
	}

	public void SetHeaderTitle(string title)
	{
		if (_AppHeaderLabel != null)
		{
			_AppHeaderLabel.SetHeaderTitle(title);
		}
	}

	public void ReplaceHeaderTitle(string title)
	{
		if (_AppHeaderLabel != null)
		{
			_AppHeaderLabel.ReplaceHeaderTitle(title);
		}
	}

	public void AppendHeaderTitle(string title)
	{
		if (_AppHeaderLabel != null)
		{
			_AppHeaderLabel.AppendHeaderTitle(title);
		}
	}

	public override INavigationPoint BuildNavPoint()
	{
		return null;
	}

	void IPositionable.JumpTo(int line, int column)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (((AbstractBaseViewContent)this).WorkbenchWindow != null && (object)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent != this && ((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent is IPositionable)
		{
			((IPositionable)((AbstractBaseViewContent)this).WorkbenchWindow.ActiveViewContent).JumpTo(line, column);
		}
	}

	internal IGeneratorDialog OpenFormDesigner(string name, AppgenSymbols appsymbols)
	{
		MessageBox.Show(appsymbols.ClarionVersion);
		return null;
	}

	internal void OpenSelectProcedure()
	{
		if (app != null)
		{
			using (SelectProcedures selectProcedures = new SelectProcedures())
			{
				selectProcedures.Init("Select Procedure to Export", app);
				selectProcedures.ShowDialog();
			}
		}
	}

	public void EditVariables(IPweeDetails details)
	{
		GeneratorBindingService.GetBinding("Clarion")?.OpenPwee(details);
	}
}

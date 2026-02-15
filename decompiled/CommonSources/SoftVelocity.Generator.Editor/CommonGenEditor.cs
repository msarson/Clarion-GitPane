using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Clarion.ASL;
using Clarion.Core;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common;
using SoftVelocity.Common.ClarionEditor;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.DataDictionary.Schema;
using SoftVelocity.Generator.PWEE;

namespace SoftVelocity.Generator.Editor;

public abstract class CommonGenEditor : CommonClarionEditor, ISecondaryViewContent, IBaseViewContent, IDisposable, IGeneratorEditorDialog, IGeneratorDialog, IBackToSourceCompatible, IFormatter, IGenerator
{
	private class ThreadParameters
	{
		public bool CancelThread;

		public string PWEEBeforeText;

		public string PWEEAfterText;

		public int CurrentLineNum;

		public ThreadParameters()
		{
			CancelThread = false;
			PWEEBeforeText = null;
			PWEEAfterText = null;
			CurrentLineNum = -1;
		}
	}

	private IEmbedEditorDetails embedIface;

	private IPweeDetails pweeIface;

	private IViewContent viewContent;

	private Control control;

	private Panel panel;

	private ToolStrip toolbar;

	private string fileName = string.Empty;

	private string fullRealName = string.Empty;

	private IGeneratorDialog childView;

	private CustomPweeLine currentEmbed;

	private int lastCaretLine = -1;

	private HighlightColor roColor;

	private HighlightColor embedColor;

	private string backgroundPWEEBeforeText;

	private string backgroundPWEEAfterText;

	private int backgroundLineNumOffset;

	private IProject project;

	private GenBookmarkFactory bookmarkFactory;

	private ProgressNotificationTaskInstance _monitor;

	private Thread embedEditorCCWorkerThread;

	private ThreadParameters threadparameters;

	private Thread moduleParsingWorkerThread;

	private bool alreadyShown;

	private static Regex endOfLine;

	private bool errorShown;

	private int mainViewIndex;

	private int lastStructureStartLine;

	private int lastStructureEndLine;

	private ProgressNotificationTaskInstance monitor
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			if (_monitor == null)
			{
				_monitor = new ProgressNotificationTaskInstance("Parsing");
			}
			return _monitor;
		}
	}

	public override string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
		}
	}

	public override Control Control => control;

	public IEmbedEditorDetails EmbedEditorDetails => embedIface;

	public IPweeDetails PweeEditorDetails => pweeIface;

	public bool IsPwee => PweeEditorDetails != null;

	protected IViewContent ViewContent => viewContent;

	public bool IsOnFirstEmbed
	{
		get
		{
			if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.CustomLineManager is PweeLineManager pweeLineManager && currentEmbed != null && pweeLineManager.FirstEmbed != null)
			{
				return ((CustomLine)currentEmbed).StartLineNr <= ((CustomLine)pweeLineManager.FirstEmbed).StartLineNr;
			}
			return true;
		}
	}

	public bool IsOnLastEmbed
	{
		get
		{
			if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.CustomLineManager is PweeLineManager pweeLineManager && currentEmbed != null && pweeLineManager.LastEmbed != null)
			{
				return ((CustomLine)currentEmbed).StartLineNr >= ((CustomLine)pweeLineManager.LastEmbed).StartLineNr;
			}
			return true;
		}
	}

	public bool IsOnFirstFilledEmbed
	{
		get
		{
			if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.CustomLineManager is PweeLineManager pweeLineManager && currentEmbed != null)
			{
				CustomPweeLine firstFilledEmbed = pweeLineManager.FirstFilledEmbed;
				if (firstFilledEmbed != null)
				{
					return ((CustomLine)currentEmbed).StartLineNr <= ((CustomLine)firstFilledEmbed).StartLineNr;
				}
			}
			return true;
		}
	}

	public bool IsOnLastFilledEmbed
	{
		get
		{
			if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.CustomLineManager is PweeLineManager pweeLineManager && currentEmbed != null)
			{
				CustomPweeLine lastFilledEmbed = pweeLineManager.LastFilledEmbed;
				if (lastFilledEmbed != null)
				{
					return ((CustomLine)currentEmbed).StartLineNr >= ((CustomLine)lastFilledEmbed).StartLineNr;
				}
			}
			return true;
		}
	}

	public override bool CanShowStructureDesigner
	{
		get
		{
			if (embedIface != null)
			{
				return embedIface.Text.IsData;
			}
			if (pweeIface != null)
			{
				IDocument document = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document;
				if (document.CustomLineManager is PweeLineManager pweeLineManager)
				{
					int line = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.Caret.Line;
					CustomPweeLine customLine = pweeLineManager.GetCustomLine(line);
					if (customLine != null && customLine.PweePart is IPweeEmbedPoint)
					{
						return ((IPweeEmbedPoint)customLine.PweePart).Text.IsData;
					}
				}
			}
			return false;
		}
	}

	protected override string ParseableText
	{
		get
		{
			string backgroundPWEEText = BackgroundPWEEText;
			if (!string.IsNullOrEmpty(backgroundPWEEText))
			{
				return backgroundPWEEText;
			}
			return base.ParseableText;
		}
	}

	public bool Visible => false;

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

	public string Data => string.Empty;

	public List<ITemplateClass> Templates => new List<ITemplateClass>();

	public FileSchema Schema => null;

	public string AppName => null;

	public string BackgroundPWEEText
	{
		get
		{
			if (backgroundPWEEBeforeText != null && backgroundPWEEAfterText != null)
			{
				StringBuilder stringBuilder = new StringBuilder(backgroundPWEEBeforeText);
				stringBuilder.Append(((TextEditorDisplayBindingWrapper)this).Text);
				stringBuilder.Append(backgroundPWEEAfterText);
				return stringBuilder.ToString();
			}
			return null;
		}
	}

	public int BackgroundLineNumOffset => backgroundLineNumOffset;

	public event EventHandler EditorClosed;

	public event DialogClosedEventHandler DisplayEmbedsDialogClosed;

	public event EventHandler DialogClosed;

	public abstract void InitializeEditor();

	protected abstract string CreateUniqueFileName();

	public void GotoPrevEmbed()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.CustomLineManager is PweeLineManager pweeLineManager)
		{
			CustomPweeLine prevEmbed = pweeLineManager.GetPrevEmbed(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Line);
			if (prevEmbed != null)
			{
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Position = new TextLocation(0, ((CustomLine)prevEmbed).StartLineNr);
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.CenterViewOn(((CustomLine)prevEmbed).StartLineNr, (int)(0.3 * (double)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.TextArea.TextView.VisibleLineCount));
			}
		}
	}

	public void GotoNextEmbed()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.CustomLineManager is PweeLineManager pweeLineManager)
		{
			CustomPweeLine nextEmbed = pweeLineManager.GetNextEmbed(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Line);
			if (nextEmbed != null)
			{
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Position = new TextLocation(0, ((CustomLine)nextEmbed).StartLineNr);
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.CenterViewOn(((CustomLine)nextEmbed).StartLineNr, (int)(0.3 * (double)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.TextArea.TextView.VisibleLineCount));
			}
		}
	}

	public void GotoPrevFilledEmbed()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.CustomLineManager is PweeLineManager pweeLineManager)
		{
			CustomPweeLine prevFilledEmbed = pweeLineManager.GetPrevFilledEmbed(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Line);
			if (prevFilledEmbed != null)
			{
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Position = new TextLocation(0, ((CustomLine)prevFilledEmbed).StartLineNr);
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.CenterViewOn(((CustomLine)prevFilledEmbed).StartLineNr, (int)(0.3 * (double)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.TextArea.TextView.VisibleLineCount));
			}
		}
	}

	public void GotoNextFilledEmbed()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.CustomLineManager is PweeLineManager pweeLineManager)
		{
			CustomPweeLine nextFilledEmbed = pweeLineManager.GetNextFilledEmbed(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Line);
			if (nextFilledEmbed != null)
			{
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Position = new TextLocation(0, ((CustomLine)nextFilledEmbed).StartLineNr);
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.CenterViewOn(((CustomLine)nextFilledEmbed).StartLineNr, (int)(0.3 * (double)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.TextArea.TextView.VisibleLineCount));
			}
		}
	}

	static CommonGenEditor()
	{
		endOfLine = new Regex("(\\n\\r?)|(\\r\\n?)", RegexOptions.Multiline | RegexOptions.Compiled);
		endOfLine.Match(string.Empty);
	}

	protected CommonGenEditor(IViewContent viewContent)
	{
		this.viewContent = viewContent;
		panel = new Panel();
		((TextEditorDisplayBindingWrapper)this).Control.Dock = DockStyle.Fill;
		panel.Controls.Add(((TextEditorDisplayBindingWrapper)this).Control);
		toolbar = ToolbarService.CreateToolStrip((object)this, "/SoftVelocity/Clarion/ToolBar/EmbedEditor", new string[0]);
		toolbar.ShowItemToolTips = true;
		toolbar.Dock = DockStyle.Top;
		toolbar.GripStyle = ToolStripGripStyle.Hidden;
		toolbar.Stretch = true;
		panel.Controls.Add(toolbar);
		panel.GotFocus += panel_GotFocus;
		control = panel;
		InitializeEditor();
		bookmarkFactory = new GenBookmarkFactory(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.BookmarkManager);
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.BookmarkManager.Factory = (IBookmarkFactory)(object)bookmarkFactory;
		((AbstractBaseViewContent)this).WorkbenchWindowChanged += OnWorkbenchWindowChanged;
	}

	private void OnWorkbenchWindowChanged(object sender, EventArgs e)
	{
		if (((AbstractBaseViewContent)this).WorkbenchWindow != null)
		{
			((AbstractBaseViewContent)this).WorkbenchWindowChanged -= OnWorkbenchWindowChanged;
			((AbstractBaseViewContent)this).WorkbenchWindow.CloseEvent += OnWorkbenchWindowClose;
		}
	}

	private void OnWorkbenchWindowClose(object sender, EventArgs e)
	{
		((AbstractBaseViewContent)this).WorkbenchWindow.CloseEvent -= OnWorkbenchWindowClose;
		viewContent = null;
	}

	private void panel_GotFocus(object sender, EventArgs e)
	{
		((TextEditorDisplayBindingWrapper)this).Control.Focus();
	}

	public override void Dispose()
	{
		viewContent = null;
		project = null;
		if (panel != null)
		{
			panel.Controls.Clear();
			toolbar.Dispose();
			panel.Dispose();
		}
		base.Dispose();
		if (embedIface != null)
		{
			embedIface.InformDialogClosed();
			embedIface = null;
		}
		if (pweeIface != null)
		{
			pweeIface.InformDialogClosed();
			pweeIface = null;
		}
		childView = null;
		if (_monitor != null)
		{
			ProgressNotificationTaskInstance val = _monitor;
			_monitor = null;
			val.Dispose();
			val = null;
		}
	}

	public virtual bool ShowEditor(IEmbedEditorDetails genIface, IProject iPrj)
	{
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		if (alreadyShown)
		{
			return false;
		}
		alreadyShown = true;
		embedIface = genIface;
		project = iPrj;
		if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).Document.FormattingStrategy is ClaCommonFormattingStrategy claCommonFormattingStrategy)
		{
			claCommonFormattingStrategy.MinimalIndent = 0;
		}
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).Document.TextContent = string.Empty;
		try
		{
			((TextEditorDisplayBindingWrapper)this).Text = EmbedEditorDetails.Text.Text;
		}
		catch
		{
		}
		if (((TextEditorDisplayBindingWrapper)this).Text == null)
		{
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).Document.TextContent = string.Empty;
		}
		((AbstractViewContent)this).IsDirty = false;
		fileName = string.Empty;
		((AbstractViewContent)this).TitleName = null;
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).FileName = string.Empty;
		bookmarkFactory.FileName = viewContent.FileName;
		fullRealName = GetFullRealName(embedIface.Module);
		backgroundPWEEBeforeText = null;
		backgroundPWEEAfterText = null;
		if (genIface.Errors != null && genIface.Errors.Length > 0)
		{
			int num = genIface.SelectedErrorIndex - 1;
			if (num < 0 || num > genIface.Errors.Length - 1)
			{
				num = 0;
			}
			int num2 = genIface.Errors[num].Line - 1;
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.CenterViewOn(num2, (int)(0.3 * (double)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.TextArea.TextView.VisibleLineCount));
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.Caret.Position = new TextLocation(genIface.Errors[num].Column - 1, num2);
		}
		bool flag = SwitchView();
		if (flag)
		{
			if (CodeCompletionOptions.EnableCodeCompletion)
			{
				embedEditorCCWorkerThread = new Thread(EmbedEditorCCThread);
				threadparameters = new ThreadParameters();
				embedEditorCCWorkerThread.IsBackground = true;
				embedEditorCCWorkerThread.Start(threadparameters);
			}
			Task.NewCommentTagTaskEvent = (EventHandler<NewCommentTagTaskEventArgs>)Delegate.Combine(Task.NewCommentTagTaskEvent, new EventHandler<NewCommentTagTaskEventArgs>(NewTagCommentTask));
		}
		return flag;
	}

	private void EmbedEditorCCThreadCompleted()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		if (threadparameters != null && !threadparameters.CancelThread)
		{
			backgroundPWEEBeforeText = threadparameters.PWEEBeforeText;
			backgroundPWEEAfterText = threadparameters.PWEEAfterText;
			backgroundLineNumOffset = threadparameters.CurrentLineNum;
			threadparameters = null;
			embedEditorCCWorkerThread = null;
			if (backgroundPWEEBeforeText == null || backgroundPWEEAfterText == null)
			{
				backgroundPWEEBeforeText = null;
				backgroundPWEEAfterText = null;
			}
			fileName = CreateUniqueFileName();
			((AbstractViewContent)this).TitleName = fileName;
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).FileName = fileName;
			AppGenEditorsService.RegisterPweeFile(fileName, fullRealName, project);
			DebuggerService.ViewContentOpened((object)null, new ViewContentEventArgs(viewContent));
		}
	}

	private void EmbedEditorCCThread(object parameter)
	{
		ThreadParameters threadParameters = (ThreadParameters)parameter;
		try
		{
			monitor.BeginTask("Preparing code completion...", 100, false);
			Commands.AttachToClarion();
			if (threadParameters.CancelThread)
			{
				return;
			}
			if (EmbedEditorDetails.PWEEAvailable)
			{
				IPweePart[] parts = EmbedEditorDetails.Parts;
				IPweePart start = EmbedEditorDetails.Start;
				if (threadParameters.CancelThread || parts == null || start == null)
				{
					return;
				}
				if (string.IsNullOrEmpty(fullRealName) || ParserService.GetParseInformationIfExist(fullRealName) != null)
				{
					monitor.WorkDone = 50;
				}
				else
				{
					monitor.WorkDone = 33;
					ParserService.ParseFile(fullRealName);
					if (threadParameters.CancelThread)
					{
						return;
					}
					monitor.WorkDone = 66;
				}
				StringBuilder stringBuilder = new StringBuilder();
				int curLine = 0;
				int selectedLine = -1;
				int selectedOffset = -1;
				CreatePweeText(parts, start, stringBuilder, null, ref curLine, ref selectedLine, ref selectedOffset);
				threadParameters.CurrentLineNum = selectedLine;
				threadParameters.PWEEBeforeText = stringBuilder.ToString(0, selectedOffset);
				int num = selectedOffset + ((TextEditorDisplayBindingWrapper)this).Text.Length;
				if (num > stringBuilder.Length)
				{
					num = stringBuilder.Length;
				}
				threadParameters.PWEEAfterText = stringBuilder.ToString(num, stringBuilder.Length - num);
				if (threadParameters.CancelThread)
				{
					return;
				}
				monitor.WorkDone = 100;
			}
			if (!threadParameters.CancelThread)
			{
				((Control)(object)((TextEditorDisplayBindingWrapper)this).TextEditorControl).BeginInvoke((Delegate)new MethodInvoker(EmbedEditorCCThreadCompleted));
			}
		}
		finally
		{
			if (!threadParameters.CancelThread)
			{
				monitor.Done();
			}
		}
	}

	public virtual bool ShowEditor(IPweeDetails genIface, IProject iPrj)
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		if (alreadyShown)
		{
			return false;
		}
		alreadyShown = true;
		pweeIface = genIface;
		project = iPrj;
		Cursor.Current = Cursors.WaitCursor;
		if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).Document.FormattingStrategy is ClaCommonFormattingStrategy claCommonFormattingStrategy)
		{
			claCommonFormattingStrategy.MinimalIndent = 1;
		}
		int num = FillPweeEditor();
		if (num != -1)
		{
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.CenterViewOn(num, (int)(0.3 * (double)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.TextArea.TextView.VisibleLineCount));
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.Caret.Line = num;
		}
		Caret_PositionChanged(null, null);
		fileName = CreateUniqueFileName();
		((AbstractViewContent)this).TitleName = fileName;
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).FileName = fileName;
		bookmarkFactory.FileName = viewContent.FileName;
		fullRealName = GetFullRealName(pweeIface.Module);
		((AbstractViewContent)this).IsDirty = false;
		Cursor.Current = Cursors.Default;
		bool flag = SwitchView();
		if (flag)
		{
			AppGenEditorsService.RegisterPweeFile(fileName, fullRealName, project);
			DebuggerService.ViewContentOpened((object)null, new ViewContentEventArgs(viewContent));
			if (!string.IsNullOrEmpty(fullRealName) && ParserService.GetParseInformationIfExist(fullRealName) == null)
			{
				StartModuleReparsingThread(reportProgress: true);
			}
			Task.NewCommentTagTaskEvent = (EventHandler<NewCommentTagTaskEventArgs>)Delegate.Combine(Task.NewCommentTagTaskEvent, new EventHandler<NewCommentTagTaskEventArgs>(NewTagCommentTask));
		}
		return flag;
	}

	protected void StartModuleReparsingThread(bool reportProgress)
	{
		if (moduleParsingWorkerThread == null && embedEditorCCWorkerThread == null)
		{
			moduleParsingWorkerThread = new Thread(ModuleParsingThread);
			moduleParsingWorkerThread.IsBackground = true;
			moduleParsingWorkerThread.Start(reportProgress);
		}
	}

	private void ModuleParsingThreadCompleted()
	{
		moduleParsingWorkerThread = null;
	}

	private void ModuleParsingThread(object parameter)
	{
		if (string.IsNullOrEmpty(fullRealName))
		{
			return;
		}
		bool flag = parameter == null || (bool)parameter;
		try
		{
			if (flag)
			{
				monitor.BeginTask($"Parsing {Path.GetFileName(fullRealName)}...", 100, false);
				monitor.WorkDone = 33;
			}
			ParserService.ParseFile(fullRealName);
			((Control)(object)((TextEditorDisplayBindingWrapper)this).TextEditorControl).BeginInvoke((Delegate)new MethodInvoker(ModuleParsingThreadCompleted));
		}
		catch (Exception)
		{
		}
		finally
		{
			if (flag)
			{
				monitor.Done();
			}
		}
	}

	private int FillPweeEditor()
	{
		IDocument document = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document;
		PweeLineManager pweeLineManager = document.CustomLineManager as PweeLineManager;
		if (pweeLineManager != null)
		{
			pweeLineManager.DisableDocumentMonitor = true;
		}
		roColor = document.HighlightingStrategy.GetColorFor("GeneratedCode");
		embedColor = document.HighlightingStrategy.GetColorFor("Default");
		StringBuilder stringBuilder = new StringBuilder();
		int curLine = 0;
		int selectedLine = -1;
		int selectedOffset = -1;
		CreatePweeText(pweeIface.Parts, pweeIface.Start, stringBuilder, pweeLineManager, ref curLine, ref selectedLine, ref selectedOffset);
		document.TextContent = stringBuilder.ToString();
		if (pweeLineManager != null)
		{
			pweeLineManager.AddCustomLine(null, document.TotalNumberOfLines - 1, document.TotalNumberOfLines - 1, roColor.BackgroundColor, readOnly: true);
			pweeLineManager.DisableDocumentMonitor = false;
			currentEmbed = null;
			lastCaretLine = -1;
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.PositionChanged += Caret_PositionChanged;
		}
		return selectedLine;
	}

	private void Caret_PositionChanged(object sender, EventArgs e)
	{
		if (lastCaretLine == ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.Caret.Line)
		{
			return;
		}
		lastCaretLine = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.Caret.Line;
		if (currentEmbed == null || lastCaretLine < ((CustomLine)currentEmbed).StartLineNr || lastCaretLine > ((CustomLine)currentEmbed).EndLineNr)
		{
			IDocument document = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document;
			if (document.CustomLineManager is PweeLineManager pweeLineManager)
			{
				currentEmbed = pweeLineManager.GetCustomLine(lastCaretLine);
			}
			ToolbarService.UpdateToolbar(toolbar);
		}
	}

	private string GetFullRealName(string fName)
	{
		RedirectionFile val = CommonClarionProject.CurrentRedirectionFile(project, IsWin);
		if (!string.IsNullOrEmpty(fName) && val.Exists(fName, project.Directory))
		{
			return val.OpenName(fName, project.Directory);
		}
		return string.Empty;
	}

	private bool SwitchView()
	{
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
		((AbstractBaseViewContent)this).WorkbenchWindow.SwitchView(num);
		ToolbarService.UpdateToolbar(toolbar);
		return true;
	}

	private void CreatePweeText(IPweePart[] parts, IPweePart startPart, StringBuilder sb, PweeLineManager lm, ref int curLine, ref int selectedLine, ref int selectedOffset)
	{
		int num = curLine;
		foreach (IPweePart pweePart in parts)
		{
			if (pweePart.IsText)
			{
				IPweeText pweeText = (IPweeText)pweePart;
				if (pweeText.IsLiteral)
				{
					if (pweePart == startPart)
					{
						selectedLine = num;
						selectedOffset = sb.Length;
					}
					AppendPartText(sb, 0, pweeText.Text.Text, ref curLine, addCR: false);
					lm?.AddCustomLine(pweeText, num, curLine - 1, roColor.BackgroundColor, readOnly: true);
					num = curLine;
					continue;
				}
				IPweeEmbedPoint pweeEmbedPoint = (IPweeEmbedPoint)pweeText;
				if (!string.IsNullOrEmpty(pweeEmbedPoint.Header))
				{
					AppendPartText(sb, (int)pweeEmbedPoint.Text.Column, pweeEmbedPoint.Header, ref curLine, addCR: true);
					lm?.AddCustomLine(null, num, curLine - 1, roColor.BackgroundColor, readOnly: true);
					num = curLine;
				}
				if (pweePart == startPart)
				{
					selectedLine = num;
					selectedOffset = sb.Length;
				}
				AppendPartText(sb, (int)pweeEmbedPoint.Text.Column, pweeText.Text.Text, ref curLine, addCR: true);
				lm?.AddCustomLine(pweeEmbedPoint, num, curLine - 1, embedColor.BackgroundColor, readOnly: false);
				num = curLine;
			}
			else
			{
				IPweeSection pweeSection = (IPweeSection)pweePart;
				if (pweePart == startPart)
				{
					selectedLine = num;
					selectedOffset = sb.Length;
				}
				if (!string.IsNullOrEmpty(pweeSection.Header))
				{
					AppendPartText(sb, (int)(pweeSection.Indentation + 1), pweeSection.Header, ref curLine, addCR: true);
					lm?.AddCustomLine(null, num, curLine - 1, roColor.BackgroundColor, readOnly: true);
				}
				CreatePweeText(pweeSection.Parts, startPart, sb, lm, ref curLine, ref selectedLine, ref selectedOffset);
				num = curLine;
				if (!string.IsNullOrEmpty(pweeSection.Footer))
				{
					AppendPartText(sb, (int)(pweeSection.Indentation + 1), pweeSection.Footer, ref curLine, addCR: true);
					lm?.AddCustomLine(null, num, curLine - 1, roColor.BackgroundColor, readOnly: true);
					num = curLine;
				}
			}
		}
	}

	private static void AppendPartText(StringBuilder content, int indent, string text, ref int endLine, bool addCR)
	{
		endLine += endOfLine.Matches(text).Count;
		if (indent > 1)
		{
			content.Append(new string(' ', indent - 1));
			text = text.Replace("\r\n", "\r\n" + new string(' ', indent - 1));
		}
		content.Append(text);
		if (addCR)
		{
			endLine++;
			content.Append("\r\n");
		}
	}

	public virtual void CloseEditor()
	{
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Expected O, but got Unknown
		if (errorShown)
		{
			StatusBarService.SetMessage(string.Empty, false);
		}
		Cursor current = Cursor.Current;
		Cursor.Current = Cursors.WaitCursor;
		bookmarkFactory.FileName = null;
		Task.NewCommentTagTaskEvent = (EventHandler<NewCommentTagTaskEventArgs>)Delegate.Remove(Task.NewCommentTagTaskEvent, new EventHandler<NewCommentTagTaskEventArgs>(NewTagCommentTask));
		List<Task> list = new List<Task>();
		foreach (Task commentTask in TaskService.CommentTasks)
		{
			if (fileName.Equals(commentTask.FileName, StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add(commentTask);
			}
		}
		foreach (Task item in list)
		{
			TaskService.Remove(item);
		}
		if (embedEditorCCWorkerThread != null)
		{
			threadparameters.CancelThread = true;
			threadparameters = null;
			embedEditorCCWorkerThread.Join(2000);
			if (embedEditorCCWorkerThread.IsAlive)
			{
				try
				{
					embedEditorCCWorkerThread.Abort();
				}
				catch
				{
				}
			}
			embedEditorCCWorkerThread = null;
			monitor.Done();
		}
		if (moduleParsingWorkerThread != null)
		{
			moduleParsingWorkerThread.Join(2000);
			if (moduleParsingWorkerThread.IsAlive)
			{
				try
				{
					moduleParsingWorkerThread.Abort();
				}
				catch
				{
				}
			}
			moduleParsingWorkerThread = null;
		}
		IDocument document = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document;
		document.CustomLineManager.Clear();
		document.BookmarkManager.Clear();
		document.TextContent = string.Empty;
		backgroundPWEEBeforeText = null;
		backgroundPWEEAfterText = null;
		backgroundLineNumOffset = 0;
		project = null;
		if (!string.IsNullOrEmpty(fileName))
		{
			ParserService.ClearParseInformation(fileName);
			AppGenEditorsService.RemovePweeFile(fileName);
		}
		NavigationService.ClearFileHistory(viewContent.FileName);
		DebuggerService.ViewContentClosed((object)null, new ViewContentEventArgs(viewContent));
		fileName = string.Empty;
		((AbstractViewContent)this).IsDirty = false;
		((AbstractViewContent)this).TitleName = null;
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).FileName = string.Empty;
		fullRealName = string.Empty;
		currentEmbed = null;
		lastCaretLine = -1;
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.PositionChanged -= Caret_PositionChanged;
		if (embedIface != null)
		{
			embedIface.InformDialogClosed();
			embedIface = null;
		}
		if (pweeIface != null)
		{
			pweeIface.InformDialogClosed();
			pweeIface = null;
		}
		Cursor.Current = current;
		alreadyShown = false;
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.SwitchView(MainViewIndex);
		if (this.EditorClosed != null)
		{
			this.EditorClosed(null, null);
			this.EditorClosed = null;
		}
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).Document.TextContent = string.Empty;
		((AbstractViewContent)this).IsDirty = false;
	}

	public override void Selected()
	{
		((AbstractBaseViewContent)this).Selected();
		((IBaseViewContent)viewContent).WorkbenchWindow.ClosingEvent += WorkbenchWindow_ClosingEvent;
		if (childView != null)
		{
			childView = null;
		}
	}

	public override void Deselected()
	{
		((AbstractBaseViewContent)this).Deselected();
		((IBaseViewContent)viewContent).WorkbenchWindow.ClosingEvent -= WorkbenchWindow_ClosingEvent;
	}

	public override void Save()
	{
		if (!((AbstractViewContent)this).IsDirty)
		{
			return;
		}
		if (embedIface != null)
		{
			embedIface.Data = ((TextEditorDisplayBindingWrapper)this).Text;
		}
		else if (pweeIface != null)
		{
			IDocument document = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document;
			if (document.CustomLineManager is PweeLineManager pweeLineManager)
			{
				foreach (CustomLine customLine in pweeLineManager.CustomLines)
				{
					if (!(customLine is CustomPweeLine { Dirty: not false } customPweeLine) || !(customPweeLine.PweePart is IPweeEmbedPoint))
					{
						continue;
					}
					LineSegment lineSegment = document.GetLineSegment(((CustomLine)customPweeLine).StartLineNr);
					LineSegment lineSegment2 = document.GetLineSegment(((CustomLine)customPweeLine).EndLineNr);
					string text = document.GetText(lineSegment.Offset, lineSegment2.Offset + lineSegment2.Length - lineSegment.Offset);
					IPweeEmbedPoint pweeEmbedPoint = (IPweeEmbedPoint)customPweeLine.PweePart;
					uint column = pweeEmbedPoint.Text.Column;
					if (column > 1 && !string.IsNullOrEmpty(text))
					{
						string[] array = text.Split(new string[1] { "\r\n" }, StringSplitOptions.None);
						int num = 0;
						for (int i = 0; i < array.Length; i++)
						{
							if (!string.IsNullOrEmpty(array[i]))
							{
								num = 0;
								for (int length = array[i].Length; length > num && array[i][num] == ' ' && num < column - 1; num++)
								{
								}
								array[i] = array[i].Substring(num);
							}
						}
						pweeEmbedPoint.Data = string.Join("\r\n", array);
					}
					else
					{
						pweeEmbedPoint.Data = text;
					}
					customPweeLine.Dirty = false;
				}
			}
		}
		((AbstractViewContent)this).IsDirty = false;
	}

	public void ShowDesigner(CommonClarionGenDesignerView view, ReportDeclaration structure, CompilerResults cr, bool isWindowDesigner, bool isWindowWindow, bool isTemplate)
	{
		childView = view;
		if (isTemplate)
		{
			lastStructureStartLine = (lastStructureEndLine = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Line);
		}
		else
		{
			lastStructureStartLine = structure.BodyReg.Line - 1;
			lastStructureEndLine = structure.BodyReg.LineEnd - 1;
		}
		view.ShowDesigner(structure, cr, isWindowDesigner, isWindowWindow, this);
	}

	protected override void ParseInformationUpdatedInvoked(ParseInformation parseInfo)
	{
		if (IsPwee)
		{
			base.ParseInformationUpdatedInvoked(parseInfo);
		}
	}

	public override INavigationPoint BuildNavPoint()
	{
		int line = ((TextEditorDisplayBindingWrapper)this).Line;
		LineSegment lineSegment = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).Document.GetLineSegment(line);
		string text = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).Document.GetText((ISegment)(object)lineSegment);
		return (INavigationPoint)(object)new GenTextNavigationPoint(viewContent.FileName, line, ((TextEditorDisplayBindingWrapper)this).Column, text);
	}

	private void NewTagCommentTask(object sender, NewCommentTagTaskEventArgs e)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (e.Task == null && fileName.Equals(e.FileName, StringComparison.InvariantCultureIgnoreCase))
		{
			TagComment tagComment = e.TagComment;
			string appFileName = viewContent.FileName;
			string text = fileName;
			string description = tagComment.Key + tagComment.CommentString;
			DomRegion region = tagComment.Region;
			int column = ((DomRegion)(ref region)).BeginColumn - 1;
			DomRegion region2 = tagComment.Region;
			e.Task = (Task)(object)new GenTagCommentTask(appFileName, text, description, column, ((DomRegion)(ref region2)).BeginLine - 1, (TaskType)3);
		}
	}

	public bool TryClose()
	{
		if (childView != null && !childView.TryClose())
		{
			return false;
		}
		return BackToSource();
	}

	public bool HaveChanges()
	{
		if (childView != null && childView.HaveChanges())
		{
			return true;
		}
		return ((AbstractViewContent)this).IsDirty;
	}

	public void Discard()
	{
		if (childView != null)
		{
			childView.Discard();
		}
		((AbstractViewContent)this).IsDirty = false;
		TryClose();
	}

	public void GoTo(int errorInfoIndex)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<int>((Action<int>)GoTo, errorInfoIndex);
		}
		else if (errorInfoIndex > -1)
		{
			int lColumn = 0;
			int lLine = 0;
			if (PweeEditorDetails != null)
			{
				FindErrorInPWEE(PweeEditorDetails.Parts, errorInfoIndex, ref lLine, ref lColumn);
			}
			else if (EmbedEditorDetails != null)
			{
				EditorBuildError[] errors = EmbedEditorDetails.Errors;
				foreach (EditorBuildError editorBuildError in errors)
				{
					if (editorBuildError.ErrorInfoIndex == errorInfoIndex)
					{
						lColumn = editorBuildError.Column - 1;
						lLine = editorBuildError.Line - 1;
						break;
					}
				}
			}
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.CenterViewOn(lLine, (int)(0.3 * (double)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.TextArea.TextView.VisibleLineCount));
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).textAreaControl).ActiveTextAreaControl.JumpTo(lLine, lColumn);
		}
		else
		{
			((AbstractBaseViewContent)this).WorkbenchWindow.SelectWindow();
			WorkbenchSingleton.Workbench.ShowView((IViewContent)(object)this);
		}
	}

	private bool FindErrorInPWEE(IPweePart[] parts, int errorInfoIndex, ref int lLine, ref int lColumn)
	{
		foreach (IPweePart pweePart in parts)
		{
			if (pweePart.IsText)
			{
				if (((IPweeText)pweePart).IsLiteral)
				{
					continue;
				}
				IPweeEmbedPoint pweeEmbedPoint = (IPweeEmbedPoint)pweePart;
				if (pweeEmbedPoint.Errors == null)
				{
					continue;
				}
				foreach (EditorBuildError error in pweeEmbedPoint.Errors)
				{
					if (error.ErrorInfoIndex != errorInfoIndex)
					{
						continue;
					}
					lColumn = error.Column - 1;
					lLine = error.Line - 1;
					IDocument document = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document;
					if (document.CustomLineManager is PweeLineManager pweeLineManager)
					{
						foreach (CustomPweeLine customLine in pweeLineManager.CustomLines)
						{
							if (customLine.PweePart == pweeEmbedPoint)
							{
								lLine += ((CustomLine)customLine).StartLineNr;
								break;
							}
						}
					}
					return true;
				}
			}
			else
			{
				IPweeSection pweeSection = (IPweeSection)pweePart;
				if (FindErrorInPWEE(pweeSection.Parts, errorInfoIndex, ref lLine, ref lColumn))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void NotifyAfterSave(bool successful)
	{
	}

	public void NotifyBeforeSave()
	{
		if (((AbstractViewContent)this).IsDirty)
		{
			((AbstractViewContent)this).Save();
		}
	}

	public void NotifyFileNameChanged()
	{
	}

	public bool BackToSource()
	{
		if (OnBackClick())
		{
			if (((AbstractViewContent)this).IsDirty)
			{
				((AbstractViewContent)this).Save();
			}
			CloseEditor();
			return true;
		}
		return false;
	}

	private bool OnBackClick()
	{
		if (((AbstractViewContent)this).IsDirty)
		{
			switch (MessageBox.Show(ResourceService.GetString("MainWindow.SaveChangesMessage"), ResourceService.GetString("MainWindow.SaveChangesMessageHeader") + " Embed Editor?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				return true;
			case DialogResult.No:
				((AbstractViewContent)this).IsDirty = false;
				return true;
			case DialogResult.Cancel:
				return false;
			}
		}
		return true;
	}

	public void SaveAndExit()
	{
		//IL_0011: Expected O, but got Unknown
		if (((AbstractViewContent)this).IsDirty)
		{
			try
			{
				((AbstractViewContent)this).Save();
			}
			catch (ParsingException ex)
			{
				ParsingException ex2 = ex;
				errorShown = true;
				StatusBarService.SetMessage("Error: " + ((Exception)(object)ex2).Message, true);
				((TextEditorDisplayBindingWrapper)this).JumpTo(ex2.Line, ex2.Column);
				return;
			}
		}
		CloseEditor();
	}

	public bool ExitNotSave()
	{
		bool flag = true;
		if (((AbstractViewContent)this).IsDirty)
		{
			switch (MessageBox.Show("Are you sure you want to cancel?", "Exit from embed editor?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				((AbstractViewContent)this).IsDirty = false;
				flag = true;
				break;
			case DialogResult.No:
				flag = false;
				break;
			}
		}
		if (flag)
		{
			CloseEditor();
		}
		return flag;
	}

	private void WorkbenchWindow_ClosingEvent(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		if (WorkbenchSingleton.Workbench.ActiveContent == this)
		{
			BackToSource();
		}
	}

	public void SetData(string value, bool save)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Invalid comparison between Unknown and I4
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		int totalNumberOfLines = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.TotalNumberOfLines;
		if (lastStructureStartLine >= totalNumberOfLines)
		{
			lastStructureStartLine = totalNumberOfLines - 1;
		}
		if (lastStructureEndLine >= totalNumberOfLines)
		{
			lastStructureEndLine = totalNumberOfLines - 1;
		}
		LineSegment lineSegment = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.GetLineSegment(lastStructureStartLine);
		LineSegment lineSegment2 = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.GetLineSegment(lastStructureEndLine);
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.Replace(lineSegment.Offset, lineSegment2.Offset + lineSegment2.Length - lineSegment.Offset, value);
		int totalNumberOfLines2 = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.TotalNumberOfLines;
		lastStructureEndLine = lastStructureEndLine + totalNumberOfLines2 - totalNumberOfLines;
		if ((int)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.TextEditorProperties.IndentStyle == 2)
		{
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.FormattingStrategy.IndentLines(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.TextArea, lastStructureStartLine, lastStructureEndLine);
		}
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Position = new TextLocation(0, lastStructureStartLine);
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.ScrollTo((lastStructureEndLine - lastStructureStartLine > 10) ? (lastStructureStartLine + 10) : lastStructureEndLine);
	}

	public List<ITemplateClass> TranslateControl(GeneratorControl controlType)
	{
		return new List<ITemplateClass>();
	}

	public IPopulatedTemplate PopulateTemplate(IControlTemplate template)
	{
		throw new NotSupportedException("The method PopulateTemplate is not supported.");
	}

	public IPopulatedTemplate PopulateTemplate(IControlTemplate template, IPopulatedTemplate baseTemplate)
	{
		throw new NotSupportedException("The method PopulateTemplate is not supported.");
	}

	public void DeleteTemplate(uint id)
	{
	}

	public IPopulatedTemplate UndeleteTemplate()
	{
		throw new NotSupportedException("The method UndeleteTemplate is not supported.");
	}

	public bool DisplayActionDialog(uint instance)
	{
		return true;
	}

	public bool DisplayEmbedsDialog(uint instance)
	{
		return true;
	}

	public void InformDialogClosed()
	{
		if (this.DialogClosed != null)
		{
			this.DialogClosed(null, null);
			this.DialogClosed = null;
		}
	}
}

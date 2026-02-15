using System;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class TextEditorDisplayBindingWrapper : AbstractViewContent, IMementoCapable, IPrintable, IEditable, IUndoHandler, IPositionable, ITextEditorControlProvider, IParseInformationListener, IClipboardHandler, IContextHelpProvider
{
	public sealed class FileChangeWatcher : IDisposable
	{
		private FileSystemWatcher watcher;

		private bool wasChangedExternally;

		private string fileName;

		private AbstractViewContent viewContent;

		public static bool DetectExternalChangesOption
		{
			get
			{
				return PropertyService.Get("SharpDevelop.FileChangeWatcher.DetectExternalChanges", defaultValue: true);
			}
			set
			{
				PropertyService.Set("SharpDevelop.FileChangeWatcher.DetectExternalChanges", value);
			}
		}

		public static bool AutoLoadExternalChangesOption
		{
			get
			{
				return PropertyService.Get("SharpDevelop.FileChangeWatcher.AutoLoadExternalChanges", defaultValue: true);
			}
			set
			{
				PropertyService.Set("SharpDevelop.FileChangeWatcher.AutoLoadExternalChanges", value);
			}
		}

		public FileChangeWatcher(AbstractViewContent viewContent)
		{
			this.viewContent = viewContent;
			WorkbenchSingleton.MainForm.Activated += GotFocusEvent;
		}

		public void Dispose()
		{
			WorkbenchSingleton.MainForm.Activated -= GotFocusEvent;
			if (watcher != null)
			{
				watcher.Dispose();
			}
		}

		public void Disable()
		{
			if (watcher != null)
			{
				watcher.EnableRaisingEvents = false;
			}
		}

		public void SetWatcher(string fileName)
		{
			this.fileName = fileName;
			if (!DetectExternalChangesOption || !File.Exists(fileName))
			{
				return;
			}
			try
			{
				if (watcher == null)
				{
					watcher = new FileSystemWatcher();
					watcher.SynchronizingObject = WorkbenchSingleton.MainForm;
					watcher.Changed += OnFileChangedEvent;
				}
				else
				{
					watcher.EnableRaisingEvents = false;
				}
				watcher.Path = Path.GetDirectoryName(fileName);
				watcher.Filter = Path.GetFileName(fileName);
				watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.LastWrite;
				watcher.EnableRaisingEvents = true;
			}
			catch (PlatformNotSupportedException)
			{
				if (watcher != null)
				{
					watcher.Dispose();
				}
				watcher = null;
			}
		}

		private void OnFileChangedEvent(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Deleted)
			{
				wasChangedExternally = true;
				if (WorkbenchSingleton.Workbench.IsActiveWindow)
				{
					WorkbenchSingleton.SafeThreadAsyncCall(GotFocusEvent, this, EventArgs.Empty);
				}
			}
		}

		private void GotFocusEvent(object sender, EventArgs e)
		{
			if (!wasChangedExternally)
			{
				return;
			}
			wasChangedExternally = false;
			string text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor.TextEditorDisplayBinding.FileAlteredMessage}", new string[1, 2] { 
			{
				"File",
				Path.GetFullPath(fileName)
			} });
			if ((AutoLoadExternalChangesOption && !viewContent.IsDirty) || MessageBox.Show(text, StringParser.Parse("${res:MainWindow.DialogName}"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				if (File.Exists(fileName))
				{
					viewContent.Load(fileName);
				}
			}
			else
			{
				viewContent.IsDirty = true;
			}
		}
	}

	public SharpDevelopTextAreaControl textAreaControl;

	private FileChangeWatcher watcher;

	private Properties storedMemento;

	public TextEditorControl TextEditorControl => textAreaControl;

	public bool EnableUndo => textAreaControl.EnableUndo;

	public bool EnableRedo => textAreaControl.EnableRedo;

	public string Text
	{
		get
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				return WorkbenchSingleton.SafeThreadFunction(GetText);
			}
			return GetText();
		}
		set
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				WorkbenchSingleton.SafeThreadCall(SetText, value);
			}
			else
			{
				SetText(value);
			}
		}
	}

	public PrintDocument PrintDocument => textAreaControl.PrintDocument;

	public override Control Control => textAreaControl;

	public override string TabPageText => "${res:FormsDesigner.DesignTabPages.SourceTabPage}";

	public override string UntitledName
	{
		get
		{
			return base.UntitledName;
		}
		set
		{
			base.UntitledName = value;
			textAreaControl.FileName = value;
			ForceFoldingUpdate();
		}
	}

	public override bool IsReadOnly => textAreaControl.IsReadOnly;

	public Properties StoredMemento => storedMemento;

	public override string FileName
	{
		set
		{
			if (Path.GetExtension(FileName) != Path.GetExtension(value) && textAreaControl.Document.HighlightingStrategy != null)
			{
				textAreaControl.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategyForFile(value);
				textAreaControl.Refresh();
			}
			base.FileName = value;
			base.TitleName = Path.GetFileName(value);
		}
	}

	public int Line => textAreaControl.ActiveTextAreaControl.Caret.Line;

	public int Column => textAreaControl.ActiveTextAreaControl.Caret.Column;

	public bool EnableCut => textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.EnableCut;

	public bool EnableCopy => textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.EnableCopy;

	public bool EnablePaste => textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.EnablePaste;

	public bool EnableDelete => textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.EnableDelete;

	public bool EnableSelectAll => textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.EnableSelectAll;

	private string GetText()
	{
		return textAreaControl.Document.TextContent;
	}

	private void SetText(string value)
	{
		textAreaControl.Document.Replace(0, textAreaControl.Document.TextLength, value);
	}

	protected override void OnFileNameChanged(EventArgs e)
	{
		base.OnFileNameChanged(e);
		textAreaControl.FileName = base.FileName;
		watcher.SetWatcher(textAreaControl.FileName);
	}

	public void Undo()
	{
		textAreaControl.Undo();
	}

	public void Redo()
	{
		textAreaControl.Redo();
	}

	protected virtual SharpDevelopTextAreaControl CreateSharpDevelopTextAreaControl()
	{
		return new SharpDevelopTextAreaControl();
	}

	public TextEditorDisplayBindingWrapper()
	{
		textAreaControl = CreateSharpDevelopTextAreaControl();
		textAreaControl.RightToLeft = RightToLeft.No;
		textAreaControl.Document.DocumentChanged += TextAreaChangedEvent;
		textAreaControl.ActiveTextAreaControl.Caret.CaretModeChanged += CaretModeChanged;
		textAreaControl.ActiveTextAreaControl.Enter += CaretUpdate;
		textAreaControl.ActiveTextAreaControl.Caret.PositionChanged += CaretUpdate;
		textAreaControl.Document.UndoStack.ActionRedone += UndoActionComplete;
		textAreaControl.Document.UndoStack.ActionUndone += UndoActionComplete;
		watcher = new FileChangeWatcher(this);
	}

	private void UndoActionComplete(object sender, EventArgs e)
	{
		if (textAreaControl.Document.UndoStack.IsDocumentUnchanged() && IsDirty)
		{
			IsDirty = false;
		}
	}

	public void ShowHelp()
	{
		TextArea textArea = textAreaControl.ActiveTextAreaControl.TextArea;
		IDocument document = textArea.Document;
		IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(textArea.MotherTextEditorControl.FileName);
		if (expressionFinder == null)
		{
			return;
		}
		LineSegment lineSegment = document.GetLineSegment(textArea.Caret.Line);
		string textContent = document.TextContent;
		ExpressionResult expressionResult = expressionFinder.FindFullExpression(textContent, lineSegment.Offset + textArea.Caret.Column);
		string expression = expressionResult.Expression;
		if (expression != null && expression.Length > 0)
		{
			ResolveResult resolveResult = ParserService.Resolve(expressionResult, textArea.Caret.Line + 1, textArea.Caret.Column + 1, textAreaControl.FileName, textContent);
			if (resolveResult is TypeResolveResult typeResolveResult)
			{
				HelpProvider.ShowHelp(typeResolveResult.ResolvedClass);
			}
			if (resolveResult is MemberResolveResult memberResolveResult)
			{
				HelpProvider.ShowHelp(memberResolveResult.ResolvedMember);
			}
		}
	}

	private void TextAreaChangedEvent(object sender, DocumentEventArgs e)
	{
		IsDirty = true;
		NavigationService.ContentChanging(textAreaControl, e);
	}

	public override void RedrawContent()
	{
		textAreaControl.OptionsChanged();
		textAreaControl.Refresh();
	}

	public override void Dispose()
	{
		if (IsUntitled)
		{
			ParserService.ClearParseInformation(UntitledName);
		}
		watcher.Dispose();
		textAreaControl.Dispose();
		base.Dispose();
	}

	public override void Save(string fileName)
	{
		OnSaving(EventArgs.Empty);
		watcher.Disable();
		if (!textAreaControl.CanSaveWithCurrentEncoding() && MessageService.AskQuestion("The file cannot be saved with the current encoding " + textAreaControl.Encoding.EncodingName + " without losing data.\nDo you want to save it using UTF-8 instead?"))
		{
			textAreaControl.Encoding = Encoding.UTF8;
		}
		textAreaControl.SaveFile(fileName);
		if (fileName != FileName)
		{
			ParserService.ClearParseInformation(FileName ?? UntitledName);
			FileName = fileName;
			ParserService.ParseViewContent(this);
		}
		TitleName = Path.GetFileName(fileName);
		IsDirty = false;
		textAreaControl.Document.UndoStack.DocumentSaved();
		watcher.SetWatcher(FileName);
		OnSaved(new SaveEventArgs(successful: true));
	}

	public override void Load(string fileName)
	{
		textAreaControl.IsReadOnly = (File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
		bool autodetectEncoding = true;
		textAreaControl.LoadFile(fileName, autoLoadHighlighting: true, autodetectEncoding);
		FileName = fileName;
		TitleName = Path.GetFileName(fileName);
		IsDirty = false;
		watcher.SetWatcher(fileName);
		foreach (SDBookmark bookmark in ICSharpCode.SharpDevelop.Bookmarks.BookmarkManager.GetBookmarks(fileName))
		{
			bookmark.Document = textAreaControl.Document;
			textAreaControl.Document.BookmarkManager.AddMark(bookmark);
		}
		ForceFoldingUpdate();
	}

	public Properties CreateMemento()
	{
		Properties properties = new Properties();
		properties.Set("CaretOffset", textAreaControl.ActiveTextAreaControl.Caret.Offset);
		properties.Set("VisibleLine", textAreaControl.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine);
		properties.Set("HighlightingLanguage", textAreaControl.Document.HighlightingStrategy.Name);
		properties.Set("Foldings", textAreaControl.Document.FoldingManager.SerializeToString());
		foreach (ISecondaryViewContent secondaryViewContent in base.SecondaryViewContents)
		{
			if (secondaryViewContent is IMementoCapable)
			{
				Properties properties2 = ((IMementoCapable)secondaryViewContent).CreateMemento();
				if (properties2 != null)
				{
					properties.Set(secondaryViewContent.TabPageText, properties2);
				}
			}
		}
		return properties;
	}

	public void SetMemento(Properties memento)
	{
		storedMemento = memento;
		textAreaControl.ActiveTextAreaControl.Caret.Position = textAreaControl.Document.OffsetToPosition(Math.Min(textAreaControl.Document.TextLength, Math.Max(0, memento.Get("CaretOffset", textAreaControl.ActiveTextAreaControl.Caret.Offset))));
		if (textAreaControl.Document.HighlightingStrategy.Name != memento.Get("HighlightingLanguage", textAreaControl.Document.HighlightingStrategy.Name))
		{
			IHighlightingStrategy highlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(memento.Get("HighlightingLanguage", textAreaControl.Document.HighlightingStrategy.Name));
			if (highlightingStrategy != null)
			{
				textAreaControl.Document.HighlightingStrategy = highlightingStrategy;
			}
		}
		textAreaControl.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = memento.Get("VisibleLine", 0);
		textAreaControl.Document.FoldingManager.DeserializeFromString(memento.Get("Foldings", ""));
		foreach (ISecondaryViewContent secondaryViewContent in base.SecondaryViewContents)
		{
			if (secondaryViewContent is IMementoCapable)
			{
				((IMementoCapable)secondaryViewContent).SetMemento(memento.Get(secondaryViewContent.TabPageText) as Properties);
			}
		}
	}

	public override INavigationPoint BuildNavPoint()
	{
		int line = Line;
		LineSegment lineSegment = textAreaControl.Document.GetLineSegment(line);
		string text = textAreaControl.Document.GetText(lineSegment);
		return new TextNavigationPoint(FileName, line, Column, text);
	}

	private void CaretUpdate(object sender, EventArgs e)
	{
		CaretChanged(null, null);
		CaretModeChanged(null, null);
	}

	private void CaretChanged(object sender, EventArgs e)
	{
		TextAreaControl activeTextAreaControl = textAreaControl.ActiveTextAreaControl;
		int line = activeTextAreaControl.Caret.Line;
		int column = activeTextAreaControl.Caret.Column;
		StatusBarService.SetCaretPosition(activeTextAreaControl.TextArea.TextView.GetVisualColumn(line, column), line, column);
		NavigationService.Log(BuildNavPoint());
	}

	private void CaretModeChanged(object sender, EventArgs e)
	{
		StatusBarService.SetInsertMode(textAreaControl.ActiveTextAreaControl.Caret.CaretMode == CaretMode.InsertMode);
	}

	public void JumpTo(int line, int column)
	{
		textAreaControl.ActiveTextAreaControl.JumpTo(line, column);
		WorkbenchSingleton.SafeThreadAsyncCall(delegate
		{
			textAreaControl.ActiveTextAreaControl.CenterViewOn(line, (int)(0.3 * (double)textAreaControl.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount));
		});
	}

	public void ForceFoldingUpdate()
	{
		if (textAreaControl.TextEditorProperties.EnableFolding)
		{
			string text = textAreaControl.FileName;
			ParseInformation parseInformation = ParserService.GetParseInformation(text);
			if (parseInformation == null)
			{
				parseInformation = ParserService.ParseFile(text, textAreaControl.Document.TextContent, updateCommentTags: false);
			}
			textAreaControl.Document.FoldingManager.UpdateFoldings(text, parseInformation);
			UpdateClassMemberBookmarks(parseInformation);
		}
	}

	public void ParseInformationUpdated(ParseInformation parseInfo)
	{
		if (textAreaControl.TextEditorProperties.EnableFolding)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(ParseInformationUpdatedInvoked, parseInfo);
		}
	}

	private void ParseInformationUpdatedInvoked(ParseInformation parseInfo)
	{
		try
		{
			textAreaControl.Document.FoldingManager.UpdateFoldings(TitleName, parseInfo);
			UpdateClassMemberBookmarks(parseInfo);
			textAreaControl.ActiveTextAreaControl.TextArea.Refresh(textAreaControl.ActiveTextAreaControl.TextArea.FoldMargin);
			textAreaControl.ActiveTextAreaControl.TextArea.Refresh(textAreaControl.ActiveTextAreaControl.TextArea.IconBarMargin);
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}

	protected virtual void UpdateClassMemberBookmarks(ParseInformation parseInfo)
	{
		ICSharpCode.TextEditor.Document.BookmarkManager bookmarkManager = textAreaControl.Document.BookmarkManager;
		bookmarkManager.RemoveMarks(IsClassMemberBookmark);
		if (parseInfo == null)
		{
			return;
		}
		foreach (IClass @class in parseInfo.MostRecentCompilationUnit.Classes)
		{
			AddClassMemberBookmarks(bookmarkManager, @class);
		}
	}

	protected virtual void AddClassMemberBookmarks(ICSharpCode.TextEditor.Document.BookmarkManager bm, IClass c)
	{
		if (c.IsSynthetic)
		{
			return;
		}
		if (!c.Region.IsEmpty)
		{
			bm.AddMark(new ClassBookmark(textAreaControl.Document, c));
		}
		foreach (IClass innerClass in c.InnerClasses)
		{
			AddClassMemberBookmarks(bm, innerClass);
		}
		foreach (IMethod method in c.Methods)
		{
			if (!method.Region.IsEmpty && !method.IsSynthetic)
			{
				bm.AddMark(new MethodBookmark(textAreaControl.Document, method));
			}
		}
		foreach (IProperty property in c.Properties)
		{
			if (!property.Region.IsEmpty && !property.IsSynthetic)
			{
				bm.AddMark(new PropertyBookmark(textAreaControl.Document, property));
			}
		}
		foreach (IField field in c.Fields)
		{
			if (!field.Region.IsEmpty && !field.IsSynthetic)
			{
				bm.AddMark(new FieldBookmark(textAreaControl.Document, field));
			}
		}
		foreach (IEvent @event in c.Events)
		{
			if (!@event.Region.IsEmpty && !@event.IsSynthetic)
			{
				bm.AddMark(new EventBookmark(textAreaControl.Document, @event));
			}
		}
	}

	protected virtual bool IsClassMemberBookmark(Bookmark b)
	{
		if (!(b is ClassMemberBookmark))
		{
			return b is ClassBookmark;
		}
		return true;
	}

	public void SelectAll()
	{
		textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.SelectAll(null, null);
	}

	public void Delete()
	{
		textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.Delete(null, null);
	}

	public void Paste()
	{
		textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.Paste(null, null);
	}

	public void Copy()
	{
		textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.Copy(null, null);
	}

	public void Cut()
	{
		textAreaControl.ActiveTextAreaControl.TextArea.ClipboardHandler.Cut(null, null);
	}
}

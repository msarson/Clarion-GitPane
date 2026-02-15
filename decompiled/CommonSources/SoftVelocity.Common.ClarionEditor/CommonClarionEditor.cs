using System;
using System.CodeDom.Compiler;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.ClarionNet.CommonProperties;
using SoftVelocity.Common.Bookmarks;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.ClarionEditor;

public abstract class CommonClarionEditor : TextEditorDisplayBindingWrapper, IClipboardHandler, IParseInformationListener, IParseableContent, IHasPropertyContainer, IStructureDesignerCompatible
{
	public const string CommonEditorProperty = "ClarionEditor";

	private bool dotDisableParsing;

	private int dotLinePosition;

	private int dotColumnPosition;

	private bool showBlockIndentDialog;

	private bool needReparsing;

	private object lockObject = new object();

	private PropertyContainer propertyContainer = new PropertyContainer();

	public abstract bool ShowBookmarksRefactoringMenu { get; }

	public abstract bool IsWin { get; }

	public abstract string ClassBookmarkContextMenuPath { get; }

	public abstract string MemberBookmarkContextMenuPath { get; }

	public abstract bool CanShowStructureDesigner { get; }

	public bool ShowBlockIndentDialog => showBlockIndentDialog;

	public override IWorkbenchWindow WorkbenchWindow
	{
		get
		{
			return ((AbstractBaseViewContent)this).WorkbenchWindow;
		}
		set
		{
			if (((AbstractBaseViewContent)this).WorkbenchWindow != null)
			{
				((AbstractBaseViewContent)this).WorkbenchWindow.WindowSelected -= WorkBenchWindowSelected;
			}
			((AbstractBaseViewContent)this).WorkbenchWindow = value;
			if (((AbstractBaseViewContent)this).WorkbenchWindow != null)
			{
				((AbstractBaseViewContent)this).WorkbenchWindow.WindowSelected += WorkBenchWindowSelected;
			}
		}
	}

	string IParseableContent.ParseableContentName => ParseableContentName;

	protected virtual string ParseableContentName
	{
		get
		{
			if (dotDisableParsing)
			{
				return null;
			}
			if (!((AbstractViewContent)this).IsUntitled)
			{
				return ((AbstractViewContent)this).FileName;
			}
			return ((AbstractViewContent)this).UntitledName;
		}
	}

	string IParseableContent.ParseableText => ParseableText;

	protected virtual string ParseableText => ((TextEditorDisplayBindingWrapper)this).Text;

	bool IClipboardHandler.EnableCut => ((TextEditorDisplayBindingWrapper)this).EnableCut;

	bool IClipboardHandler.EnableCopy => ((TextEditorDisplayBindingWrapper)this).EnableCopy;

	bool IClipboardHandler.EnablePaste => ((TextEditorDisplayBindingWrapper)this).EnablePaste;

	bool IClipboardHandler.EnableDelete => ((TextEditorDisplayBindingWrapper)this).EnableDelete;

	bool IClipboardHandler.EnableSelectAll => ((TextEditorDisplayBindingWrapper)this).EnableSelectAll;

	public PropertyContainer PropertyContainer => propertyContainer;

	public abstract string GetTemplatesFileName();

	protected CommonClarionEditor()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		ParserService.ParseInformationUpdated += new ParseInformationEventHandler(OnParseInformationUpdated);
		PropertyService.PropertyChanged += new PropertyChangedEventHandler(PropertyService_PropertyChanged);
		((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.Caret.PositionChanged += Caret_PositionChanged;
		Properties val = PropertyService.Get<Properties>("ClarionEditor", new Properties());
		showBlockIndentDialog = val.Get<bool>("ShowBlockIndentDialog", true);
		TextSplitter.LettersPerLine = val.Get<int>("LineOfCodeWidth", TextSplitter.DefaultLettersPerLine);
	}

	private void Caret_PositionChanged(object sender, EventArgs e)
	{
		if (dotDisableParsing && (((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.Caret.Line != dotLinePosition || ((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.Caret.Column != dotColumnPosition))
		{
			dotDisableParsing = false;
		}
	}

	private void PropertyService_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		object newValue = e.NewValue;
		Properties val = (Properties)((newValue is Properties) ? newValue : null);
		if (val != null && e.Key == "ClarionEditor")
		{
			showBlockIndentDialog = val.Get<bool>("ShowBlockIndentDialog", true);
		}
	}

	public override void Dispose()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		if (((TextEditorControlBase)base.textAreaControl).Document.FormattingStrategy is ClaCommonFormattingStrategy)
		{
			ClaCommonFormattingStrategy claCommonFormattingStrategy = (ClaCommonFormattingStrategy)(object)((TextEditorControlBase)base.textAreaControl).Document.FormattingStrategy;
			claCommonFormattingStrategy.DisposeParser(((TextEditorControlBase)base.textAreaControl).Document);
		}
		if (((AbstractBaseViewContent)this).WorkbenchWindow != null)
		{
			((AbstractBaseViewContent)this).WorkbenchWindow.WindowSelected -= WorkBenchWindowSelected;
		}
		ParserService.ParseInformationUpdated -= new ParseInformationEventHandler(OnParseInformationUpdated);
		PropertyService.PropertyChanged -= new PropertyChangedEventHandler(PropertyService_PropertyChanged);
		((TextEditorDisplayBindingWrapper)this).Dispose();
	}

	protected string GetText()
	{
		return ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.TextContent;
	}

	protected void SetText(string value)
	{
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.TextContent = value;
	}

	public void ForceFoldingUpdate()
	{
		if (((TextEditorControlBase)base.textAreaControl).TextEditorProperties.EnableFolding)
		{
			string fileName = ((TextEditorControlBase)base.textAreaControl).FileName;
			ParseInformation val = ParserService.GetParseInformation(fileName);
			if (val == null)
			{
				val = ParserService.ParseFile(fileName, ((TextEditorControlBase)base.textAreaControl).Document.TextContent, false);
			}
			((TextEditorControlBase)base.textAreaControl).Document.FoldingManager.UpdateFoldings(fileName, (object)val);
			((TextEditorDisplayBindingWrapper)this).UpdateClassMemberBookmarks(val);
		}
	}

	private void OnParseInformationUpdated(object sender, ParseInformationEventArgs e)
	{
		string text = ((((TextEditorControlBase)base.textAreaControl).FileName != null) ? ((TextEditorControlBase)base.textAreaControl).FileName.ToUpperInvariant() : string.Empty);
		if (!(e.FileName.ToUpperInvariant() == text))
		{
			return;
		}
		lock (lockObject)
		{
			if (needReparsing)
			{
				needReparsing = false;
				WorkbenchSingleton.SafeThreadAsyncCall<ParseInformation>((Action<ParseInformation>)ParseInformationUpdatedInvoked, e.ParseInformation);
			}
		}
	}

	public void ParseInformationUpdated(ParseInformation parseInfo)
	{
		if (((TextEditorControlBase)base.textAreaControl).TextEditorProperties.EnableFolding)
		{
			WorkbenchSingleton.SafeThreadAsyncCall<ParseInformation>((Action<ParseInformation>)ParseInformationUpdatedInvoked, parseInfo);
		}
	}

	protected virtual void ParseInformationUpdatedInvoked(ParseInformation parseInfo)
	{
		if (((AbstractBaseViewContent)this).WorkbenchWindow != null)
		{
			try
			{
				int firstVisibleLine = ((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine;
				((TextEditorControlBase)base.textAreaControl).Document.FoldingManager.UpdateFoldings(((AbstractViewContent)this).TitleName, (object)parseInfo);
				((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = firstVisibleLine;
				((TextEditorDisplayBindingWrapper)this).UpdateClassMemberBookmarks(parseInfo);
				((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.TextArea.Refresh((AbstractMargin)(object)((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.TextArea.TextView);
				((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.TextArea.Refresh((AbstractMargin)(object)((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.TextArea.FoldMargin);
				((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.TextArea.Refresh((AbstractMargin)(object)((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.TextArea.IconBarMargin);
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
	}

	protected override void UpdateClassMemberBookmarks(ParseInformation parseInfo)
	{
		BookmarkManager bookmarkManager = ((TextEditorControlBase)base.textAreaControl).Document.BookmarkManager;
		bookmarkManager.RemoveMarks((Predicate<Bookmark>)((TextEditorDisplayBindingWrapper)this).IsClassMemberBookmark);
		if (parseInfo == null)
		{
			return;
		}
		string text = ((AbstractViewContent)this).FileName ?? ((AbstractViewContent)this).TitleName ?? ((AbstractViewContent)this).UntitledName;
		text = text.ToUpperInvariant();
		foreach (IClass @class in parseInfo.MostRecentCompilationUnit.Classes)
		{
			AddClassMemberBookmarks(bookmarkManager, @class, text);
		}
	}

	protected virtual void AddClassMemberBookmarks(BookmarkManager bm, IClass c, string curFileName)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (((IDecoration)c).IsSynthetic || !(c is ClaClass))
		{
			return;
		}
		string fileName = ((ClaClass)(object)c).ClaRegion.FileName;
		int totalNumberOfLines = ((TextEditorControlBase)base.textAreaControl).Document.TotalNumberOfLines;
		DomRegion region = c.Region;
		if (!((DomRegion)(ref region)).IsEmpty && fileName.Equals(curFileName, StringComparison.InvariantCultureIgnoreCase))
		{
			DomRegion region2 = c.Region;
			if (((DomRegion)(ref region2)).BeginLine < totalNumberOfLines)
			{
				ClaClassBookmark claClassBookmark = new ClaClassBookmark(((TextEditorControlBase)base.textAreaControl).Document, c, ShowBookmarksRefactoringMenu);
				claClassBookmark.ContextMenuPath = ClassBookmarkContextMenuPath;
				bm.AddMark((Bookmark)(object)claClassBookmark);
			}
		}
		foreach (IClass innerClass in c.InnerClasses)
		{
			AddClassMemberBookmarks(bm, innerClass, curFileName);
		}
		foreach (IMethod method in c.Methods)
		{
			AddMethodBookmark(bm, method, curFileName);
		}
		foreach (IProperty property in c.Properties)
		{
			if (!((IDecoration)property).IsSynthetic && property is ClaProperty { ClaRegion: var claRegion } claProperty)
			{
				if (!claRegion.IsEmpty && claProperty.ClaRegion.FileName.Equals(curFileName, StringComparison.InvariantCultureIgnoreCase) && claProperty.ClaRegion.BeginLine < totalNumberOfLines)
				{
					ClaMemberBookmark claMemberBookmark = new ClaPropertyBookmark(((TextEditorControlBase)base.textAreaControl).Document, property, ShowBookmarksRefactoringMenu);
					claMemberBookmark.ContextMenuPath = MemberBookmarkContextMenuPath;
					bm.AddMark((Bookmark)(object)claMemberBookmark);
				}
				if (claProperty.Getter != null)
				{
					AddMethodBookmark(bm, (IMethod)(object)claProperty.Getter, curFileName);
				}
				if (claProperty.Setter != null)
				{
					AddMethodBookmark(bm, (IMethod)(object)claProperty.Setter, curFileName);
				}
			}
		}
		foreach (IField field in c.Fields)
		{
			if (!((IDecoration)field).IsSynthetic && field is ClaField { ClaRegion: { IsEmpty: false }, ClaRegion: var claRegion3 } claField && claRegion3.FileName.Equals(curFileName, StringComparison.InvariantCultureIgnoreCase) && claField.ClaRegion.BeginLine < totalNumberOfLines)
			{
				ClaMemberBookmark claMemberBookmark2 = new ClaFieldBookmark(((TextEditorControlBase)base.textAreaControl).Document, field, ShowBookmarksRefactoringMenu);
				claMemberBookmark2.ContextMenuPath = MemberBookmarkContextMenuPath;
				bm.AddMark((Bookmark)(object)claMemberBookmark2);
			}
		}
		foreach (IEvent @event in c.Events)
		{
			if (!((IDecoration)@event).IsSynthetic && @event is ClaEvent { ClaRegion: { IsEmpty: false }, ClaRegion: var claRegion5 } claEvent && claRegion5.FileName.Equals(curFileName, StringComparison.InvariantCultureIgnoreCase) && claEvent.ClaRegion.BeginLine < totalNumberOfLines)
			{
				ClaMemberBookmark claMemberBookmark3 = new ClaEventBookmark(((TextEditorControlBase)base.textAreaControl).Document, @event, ShowBookmarksRefactoringMenu);
				claMemberBookmark3.ContextMenuPath = MemberBookmarkContextMenuPath;
				bm.AddMark((Bookmark)(object)claMemberBookmark3);
			}
		}
	}

	private void AddMethodBookmark(BookmarkManager bm, IMethod m, string curFileName)
	{
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		if (((IDecoration)m).IsSynthetic)
		{
			return;
		}
		int totalNumberOfLines = ((TextEditorControlBase)base.textAreaControl).Document.TotalNumberOfLines;
		if (!(m is ClaMethod { ClaRegion: var claRegion } claMethod))
		{
			return;
		}
		if (!claRegion.IsEmpty && claMethod.ClaRegion.FileName.Equals(curFileName, StringComparison.InvariantCultureIgnoreCase) && claMethod.ClaRegion.BeginLine < totalNumberOfLines)
		{
			ClaMemberBookmark claMemberBookmark = new ClaMethodBookmark(((TextEditorControlBase)base.textAreaControl).Document, m, ShowBookmarksRefactoringMenu);
			claMemberBookmark.ContextMenuPath = MemberBookmarkContextMenuPath;
			bm.AddMark((Bookmark)(object)claMemberBookmark);
		}
		if (claMethod.ClaBodyRegion.IsEmpty || !claMethod.ClaBodyRegion.FileName.Equals(curFileName, StringComparison.InvariantCultureIgnoreCase) || claMethod.ClaBodyRegion.DeclBeginLine >= totalNumberOfLines)
		{
			return;
		}
		if (!claMethod.IsInline)
		{
			ClaMemberBookmark claMemberBookmark2 = new ClaMethodBookmark(((TextEditorControlBase)base.textAreaControl).Document, m, claMethod.ClaBodyRegion.DeclBeginLine, ShowBookmarksRefactoringMenu);
			claMemberBookmark2.ContextMenuPath = MemberBookmarkContextMenuPath;
			bm.AddMark((Bookmark)(object)claMemberBookmark2);
		}
		foreach (IField localVariable in claMethod.LocalVariables)
		{
			DomRegion region = ((IMember)localVariable).Region;
			if (((DomRegion)(ref region)).BeginLine < totalNumberOfLines)
			{
				ClaMemberBookmark claMemberBookmark3 = new ClaFieldBookmark(((TextEditorControlBase)base.textAreaControl).Document, localVariable, ShowBookmarksRefactoringMenu);
				claMemberBookmark3.ContextMenuPath = MemberBookmarkContextMenuPath;
				bm.AddMark((Bookmark)(object)claMemberBookmark3);
			}
		}
		foreach (ClaRoutine routine in claMethod.Routines)
		{
			AddMethodBookmark(bm, (IMethod)(object)routine, curFileName);
		}
		foreach (IMethod localMethod in claMethod.LocalMethods)
		{
			AddMethodBookmark(bm, localMethod, curFileName);
		}
		foreach (IClass localType in claMethod.LocalTypes)
		{
			AddClassMemberBookmarks(bm, localType, curFileName);
		}
	}

	protected override bool IsClassMemberBookmark(Bookmark b)
	{
		if (!(b is ClaMemberBookmark))
		{
			return b is ClaClassBookmark;
		}
		return true;
	}

	public void MarkForReparsing()
	{
		lock (lockObject)
		{
			needReparsing = true;
		}
	}

	private void WorkBenchWindowSelected(object sender, EventArgs e)
	{
		lock (lockObject)
		{
			if (needReparsing)
			{
				string text = ((AbstractViewContent)this).FileName ?? ((AbstractViewContent)this).TitleName ?? ((AbstractViewContent)this).UntitledName;
				ParserService.EnqueueForParsing(text);
			}
		}
	}

	public void DotKeyPressed()
	{
		dotDisableParsing = true;
		dotLinePosition = ((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.Caret.Line;
		dotColumnPosition = ((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.Caret.Column + 1;
	}

	void IClipboardHandler.Copy()
	{
		((TextEditorDisplayBindingWrapper)this).Copy();
	}

	void IClipboardHandler.Paste()
	{
		if (SmartFormatterOptions.General.FormatTextAfterPastingSeveralLines && ((TextEditorControlBase)base.textAreaControl).Document.FormattingStrategy is ClaCommonFormattingStrategy)
		{
			ClaCommonFormattingStrategy claCommonFormattingStrategy = (ClaCommonFormattingStrategy)(object)((TextEditorControlBase)base.textAreaControl).Document.FormattingStrategy;
			claCommonFormattingStrategy.Pasting = true;
			((TextEditorDisplayBindingWrapper)this).Paste();
			int lineNum = -1;
			int lineCount = -1;
			claCommonFormattingStrategy.GetPastedLines(ref lineNum, ref lineCount);
			claCommonFormattingStrategy.Pasting = false;
			if (lineNum != -1 && lineCount > 1)
			{
				((DefaultFormattingStrategy)claCommonFormattingStrategy).IndentLines(((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.TextArea, lineNum, lineNum + lineCount - 1);
			}
		}
		else
		{
			((TextEditorDisplayBindingWrapper)this).Paste();
		}
	}

	void IClipboardHandler.Delete()
	{
		((TextEditorDisplayBindingWrapper)this).Delete();
	}

	void IClipboardHandler.SelectAll()
	{
		((TextEditorDisplayBindingWrapper)this).SelectAll();
	}

	void IClipboardHandler.Cut()
	{
		((TextEditorDisplayBindingWrapper)this).Cut();
	}

	public virtual ReportDeclaration ParseStructure(string fileName, string fileContent, int line, int column, out ClarionType structureType, out CompilerResults cr)
	{
		return CommonIDEParser.ParseStructure(fileName, fileContent, line, column, extract: false, IsWin, out structureType, out cr);
	}

	public virtual string GetContentForDesigner()
	{
		return ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.TextContent;
	}
}

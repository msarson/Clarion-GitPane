using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using ICSharpCode.TextEditor.Gui.InsightWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class SharpDevelopTextAreaControl : TextEditorControl
{
	private static readonly string contextMenuPath = "/SharpDevelop/ViewContent/DefaultTextEditor/ContextMenu";

	private static readonly string editActionsPath = "/AddIns/DefaultTextEditor/EditActions";

	private static readonly string formatingStrategyPath = "/AddIns/DefaultTextEditor/Formatter";

	private static readonly string advancedHighlighterPath = "/AddIns/DefaultTextEditor/AdvancedHighlighter";

	private QuickClassBrowserPanel quickClassBrowserPanel;

	private ErrorDrawer errorDrawer;

	private IAdvancedHighlighter advancedHighlighter;

	private static ICodeCompletionBinding[] codeCompletionBindings;

	protected InsightWindow insightWindow;

	protected AbstractCompletionWindow codeCompletionWindow;

	private bool inHandleKeyPress;

	protected virtual string ContextMenuPath => contextMenuPath;

	public QuickClassBrowserPanel QuickClassBrowserPanel => quickClassBrowserPanel;

	public static ICodeCompletionBinding[] CodeCompletionBindings
	{
		get
		{
			if (codeCompletionBindings == null)
			{
				try
				{
					codeCompletionBindings = (ICodeCompletionBinding[])AddInTree.GetTreeNode("/AddIns/DefaultTextEditor/CodeCompletion").BuildChildItems(null).ToArray(typeof(ICodeCompletionBinding));
				}
				catch (TreePathNotFoundException)
				{
					codeCompletionBindings = new ICodeCompletionBinding[0];
				}
			}
			return codeCompletionBindings;
		}
	}

	public bool InsightWindowVisible => insightWindow != null;

	public SharpDevelopTextAreaControl()
	{
		base.Document.FoldingManager.FoldingStrategy = new ParserFoldingStrategy();
		base.Document.BookmarkManager.Factory = new SDBookmarkFactory(base.Document.BookmarkManager);
		base.Document.BookmarkManager.Added += BookmarkAdded;
		base.Document.BookmarkManager.Removed += BookmarkRemoved;
		base.Document.LineCountChanged += BookmarkLineCountChanged;
		GenerateEditActions();
		base.TextEditorProperties = SharpDevelopTextEditorProperties.Instance;
	}

	private void BookmarkAdded(object sender, ICSharpCode.TextEditor.Document.BookmarkEventArgs e)
	{
		if (e.Bookmark is SDBookmark bookmark)
		{
			ICSharpCode.SharpDevelop.Bookmarks.BookmarkManager.AddMark(bookmark);
		}
	}

	private void BookmarkRemoved(object sender, ICSharpCode.TextEditor.Document.BookmarkEventArgs e)
	{
		if (e.Bookmark is SDBookmark bookmark)
		{
			ICSharpCode.SharpDevelop.Bookmarks.BookmarkManager.RemoveMark(bookmark);
		}
	}

	private void BookmarkLineCountChanged(object sender, LineCountChangeEventArgs e)
	{
		foreach (Bookmark mark in base.Document.BookmarkManager.Marks)
		{
			if (mark.LineNumber >= e.LineStart && mark is SDBookmark sDBookmark)
			{
				sDBookmark.RaiseLineNumberChanged();
			}
		}
	}

	protected override void InitializeTextAreaControl(TextAreaControl newControl)
	{
		base.InitializeTextAreaControl(newControl);
		newControl.ShowContextMenu += delegate(object sender, MouseEventArgs e)
		{
			MenuService.ShowContextMenu(this, ContextMenuPath, (Control)sender, e.X, e.Y);
		};
		newControl.TextArea.KeyEventHandler += HandleKeyPress;
		newControl.TextArea.ClipboardHandler.CopyText += ClipboardHandlerCopyText;
		newControl.MouseWheel += TextAreaMouseWheel;
		newControl.DoHandleMousewheel = false;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			if (errorDrawer != null)
			{
				errorDrawer.Dispose();
				errorDrawer = null;
			}
			if (quickClassBrowserPanel != null)
			{
				quickClassBrowserPanel.Dispose();
				quickClassBrowserPanel = null;
			}
			if (advancedHighlighter != null)
			{
				advancedHighlighter.Dispose();
				advancedHighlighter = null;
			}
			CloseCodeCompletionWindow(this, EventArgs.Empty);
			CloseInsightWindow(this, EventArgs.Empty);
		}
	}

	protected virtual void CloseCodeCompletionWindow(object sender, EventArgs e)
	{
		if (codeCompletionWindow != null)
		{
			codeCompletionWindow.Closed -= CloseCodeCompletionWindow;
			codeCompletionWindow.Dispose();
			codeCompletionWindow = null;
		}
	}

	protected void CloseInsightWindow(object sender, EventArgs e)
	{
		if (insightWindow != null)
		{
			insightWindow.Closed -= CloseInsightWindow;
			insightWindow.Dispose();
			insightWindow = null;
		}
	}

	private void TextAreaMouseWheel(object sender, MouseEventArgs e)
	{
		TextAreaControl textAreaControl = (TextAreaControl)sender;
		if (insightWindow != null && !insightWindow.IsDisposed && insightWindow.Visible)
		{
			insightWindow.HandleMouseWheel(e);
		}
		else if (codeCompletionWindow != null && !codeCompletionWindow.IsDisposed && codeCompletionWindow.Visible)
		{
			codeCompletionWindow.HandleMouseWheel(e);
		}
		else
		{
			textAreaControl.HandleMouseWheel(e);
		}
	}

	private void ClipboardHandlerCopyText(object sender, CopyTextEventArgs e)
	{
		SideBarView.PutInClipboardRing(e.Text);
	}

	public override void OptionsChanged()
	{
		base.OptionsChanged();
		if (!(base.TextEditorProperties is SharpDevelopTextEditorProperties sharpDevelopTextEditorProperties))
		{
			return;
		}
		if (!sharpDevelopTextEditorProperties.ShowQuickClassBrowserPanel)
		{
			RemoveQuickClassBrowserPanel();
		}
		else
		{
			ActivateQuickClassBrowserOnDemand();
		}
		if (sharpDevelopTextEditorProperties.UnderlineErrors)
		{
			if (errorDrawer == null)
			{
				errorDrawer = new ErrorDrawer(this);
			}
		}
		else if (errorDrawer != null)
		{
			errorDrawer.Dispose();
			errorDrawer = null;
		}
	}

	private void GenerateEditActions()
	{
		try
		{
			IEditAction[] array = (IEditAction[])AddInTree.GetTreeNode(editActionsPath).BuildChildItems(this).ToArray(typeof(IEditAction));
			IEditAction[] array2 = array;
			foreach (IEditAction editAction in array2)
			{
				Keys[] keys = editAction.Keys;
				foreach (Keys key in keys)
				{
					editactions[key] = editAction;
				}
			}
		}
		catch (TreePathNotFoundException)
		{
			LoggingService.Warn("EditAction " + editActionsPath + " doesn't exists in the AddInTree");
		}
	}

	protected virtual void RemoveQuickClassBrowserPanel()
	{
		if (quickClassBrowserPanel != null)
		{
			base.Controls.Remove(quickClassBrowserPanel);
			quickClassBrowserPanel.Dispose();
			quickClassBrowserPanel = null;
			textAreaPanel.BorderStyle = BorderStyle.None;
		}
	}

	protected virtual void ShowQuickClassBrowserPanel()
	{
		if (quickClassBrowserPanel == null)
		{
			quickClassBrowserPanel = new QuickClassBrowserPanel(this);
			base.Controls.Add(quickClassBrowserPanel);
			textAreaPanel.BorderStyle = BorderStyle.Fixed3D;
		}
	}

	public void ActivateQuickClassBrowserOnDemand()
	{
		if (base.TextEditorProperties is SharpDevelopTextEditorProperties { ShowQuickClassBrowserPanel: not false } && base.FileName != null)
		{
			if (ParserService.GetParser(base.FileName) != null)
			{
				ShowQuickClassBrowserPanel();
			}
			else
			{
				RemoveQuickClassBrowserPanel();
			}
		}
	}

	protected override void OnFileNameChanged(EventArgs e)
	{
		base.OnFileNameChanged(e);
		((SDBookmarkFactory)base.Document.BookmarkManager.Factory).ChangeFilename(base.FileName);
		ActivateQuickClassBrowserOnDemand();
	}

	protected virtual bool HandleKeyPress(char ch)
	{
		if (inHandleKeyPress)
		{
			return false;
		}
		inHandleKeyPress = true;
		try
		{
			if (codeCompletionWindow != null && !codeCompletionWindow.IsDisposed)
			{
				if (codeCompletionWindow.ProcessKeyEvent(ch))
				{
					return true;
				}
				if (codeCompletionWindow != null && !codeCompletionWindow.IsDisposed)
				{
					return false;
				}
			}
			if (CodeCompletionOptions.EnableCodeCompletion)
			{
				ICodeCompletionBinding[] array = CodeCompletionBindings;
				foreach (ICodeCompletionBinding codeCompletionBinding in array)
				{
					if (codeCompletionBinding.HandleKeyPress(this, ch))
					{
						return false;
					}
				}
			}
			if (ch == ' ' && SharpDevelopTextEditorProperties.Instance.AutoInsertTemplates)
			{
				string wordBeforeCaret = GetWordBeforeCaret();
				if (wordBeforeCaret != null)
				{
					CodeTemplateGroup templateGroupPerFilename = CodeTemplateLoader.GetTemplateGroupPerFilename(base.FileName);
					if (templateGroupPerFilename != null)
					{
						foreach (CodeTemplate template in templateGroupPerFilename.Templates)
						{
							if (template.Shortcut == wordBeforeCaret)
							{
								if (wordBeforeCaret.Length > 0)
								{
									int offset = DeleteWordBeforeCaret();
									ActiveTextAreaControl.TextArea.Caret.Position = base.Document.OffsetToPosition(offset);
								}
								InsertTemplate(template);
								return true;
							}
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			LogException(ex);
		}
		finally
		{
			inHandleKeyPress = false;
		}
		return false;
	}

	public virtual void ShowInsightWindow(IInsightDataProvider insightDataProvider)
	{
		if (insightWindow == null || insightWindow.IsDisposed)
		{
			insightWindow = new InsightWindow((Form)WorkbenchSingleton.Workbench, this);
			insightWindow.Closed += CloseInsightWindow;
		}
		insightWindow.AddInsightDataProvider(insightDataProvider, base.FileName);
		insightWindow.ShowInsightWindow();
	}

	public void ShowCompletionWindow(ICompletionDataProvider completionDataProvider, char ch)
	{
		codeCompletionWindow = CreateCompletionWindow(completionDataProvider, ch);
		if (codeCompletionWindow != null)
		{
			codeCompletionWindow.Closed += CloseCodeCompletionWindow;
		}
	}

	protected virtual AbstractCompletionWindow CreateCompletionWindow(ICompletionDataProvider completionDataProvider, char ch)
	{
		CodeCompletionWindow.CompletionOptions completionOptions = CodeCompletionWindow.CompletionOptions.ShowDeclarationWindow;
		if (CodeCompletionOptions.ShrinkListWhenTyping)
		{
			completionOptions |= CodeCompletionWindow.CompletionOptions.FilterListOnTyping;
		}
		if (CodeCompletionOptions.NewLineOnEnterAfterFullWord)
		{
			completionOptions |= CodeCompletionWindow.CompletionOptions.ProceedEnterKeyAfterFullWord;
		}
		if (CodeCompletionOptions.CompleteOnInsertionKey)
		{
			completionOptions |= CodeCompletionWindow.CompletionOptions.CompleteOnInsertionKey;
		}
		return CodeCompletionWindow.ShowCompletionWindow((Form)WorkbenchSingleton.Workbench, this, base.FileName, completionDataProvider, ch, completionOptions);
	}

	private void LogException(Exception ex)
	{
		MessageService.ShowError(ex);
	}

	public string GetWordBeforeCaret()
	{
		int num = TextUtilities.FindPrevWordStart(base.Document, ActiveTextAreaControl.TextArea.Caret.Offset);
		return base.Document.GetText(num, ActiveTextAreaControl.TextArea.Caret.Offset - num);
	}

	public int DeleteWordBeforeCaret()
	{
		int num = TextUtilities.FindPrevWordStart(base.Document, ActiveTextAreaControl.TextArea.Caret.Offset);
		base.Document.Remove(num, ActiveTextAreaControl.TextArea.Caret.Offset - num);
		return num;
	}

	public void InsertTemplate(CodeTemplate template)
	{
		string text = string.Empty;
		base.Document.UndoStack.StartUndoGroup();
		if (base.ActiveTextAreaControl.TextArea.SelectionManager.HasSomethingSelected)
		{
			text = base.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
			ActiveTextAreaControl.TextArea.Caret.Position = ActiveTextAreaControl.TextArea.SelectionManager.SelectionCollection[0].StartPosition;
			base.ActiveTextAreaControl.TextArea.SelectionManager.RemoveSelectedText();
		}
		string text2 = StringParser.Parse(template.Text, new string[1, 2] { { "Selection", text } });
		int num = text2.IndexOf('|');
		if (num >= 0)
		{
			text2 = text2.Remove(num, 1);
		}
		else
		{
			num = text2.Length;
		}
		int offset = ActiveTextAreaControl.TextArea.Caret.Offset;
		BeginUpdate();
		int line = ActiveTextAreaControl.TextArea.Caret.Line;
		base.Document.Insert(offset, text2);
		ActiveTextAreaControl.TextArea.Caret.Position = base.Document.OffsetToPosition(offset + num);
		int num2 = base.Document.OffsetToPosition(offset + text2.Length).Y;
		IndentStyle indentStyle = base.TextEditorProperties.IndentStyle;
		base.TextEditorProperties.IndentStyle = IndentStyle.Smart;
		Console.WriteLine("Indent between {0} and {1}", line, num2);
		base.Document.FormattingStrategy.IndentLines(ActiveTextAreaControl.TextArea, line, num2);
		base.Document.UndoStack.EndUndoGroup();
		EndUpdate();
		base.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
		base.Document.CommitUpdate();
		base.TextEditorProperties.IndentStyle = indentStyle;
	}

	protected override void OnReloadHighlighting(object sender, EventArgs e)
	{
		base.OnReloadHighlighting(sender, e);
		InitializeAdvancedHighlighter();
	}

	public void InitializeAdvancedHighlighter()
	{
		if (advancedHighlighter != null)
		{
			advancedHighlighter.Dispose();
			advancedHighlighter = null;
		}
		string path = advancedHighlighterPath + "/" + base.Document.HighlightingStrategy.Name;
		if (AddInTree.ExistsTreeNode(path))
		{
			IList<IAdvancedHighlighter> list = AddInTree.BuildItems<IAdvancedHighlighter>(path, this);
			if (list != null && list.Count > 0)
			{
				advancedHighlighter = list[0];
				advancedHighlighter.Initialize(this);
				base.Document.HighlightingStrategy = new AdvancedHighlightingStrategy((DefaultHighlightingStrategy)base.Document.HighlightingStrategy, advancedHighlighter);
			}
		}
	}

	public void InitializeFormatter()
	{
		string path = formatingStrategyPath + "/" + base.Document.HighlightingStrategy.Name;
		if (AddInTree.ExistsTreeNode(path))
		{
			IFormattingStrategy[] array = (IFormattingStrategy[])AddInTree.GetTreeNode(path).BuildChildItems(this).ToArray(typeof(IFormattingStrategy));
			if (array != null && array.Length > 0)
			{
				base.Document.FormattingStrategy = array[0];
			}
		}
	}

	public override string GetRangeDescription(int selectedItem, int itemCount)
	{
		StringParser.Properties["CurrentMethodNumber"] = selectedItem.ToString("##");
		StringParser.Properties["NumberOfTotalMethods"] = itemCount.ToString("##");
		return StringParser.Parse("${res:ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor.InsightWindow.NumberOfText}");
	}
}

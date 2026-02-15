using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Debugging;

public static class DebuggerService
{
	private const string ToolTipProviderAddInTreePath = "/SharpDevelop/ViewContent/DefaultTextEditor/ToolTips";

	private static IDebugger currentDebugger;

	private static DebuggerDescriptor[] debuggers;

	private static string oldLayoutConfiguration;

	private static MessageViewCategory debugCategory;

	private static DebuggerGridControl oldToolTipControl;

	public static IDebugger CurrentDebugger
	{
		get
		{
			if (currentDebugger == null)
			{
				currentDebugger = GetCompatibleDebugger();
				currentDebugger.DebugStarted += OnDebugStarted;
				currentDebugger.DebugStopped += OnDebugStopped;
			}
			return currentDebugger;
		}
	}

	public static DebuggerDescriptor Descriptor
	{
		get
		{
			GetDescriptors();
			if (debuggers.Length > 0)
			{
				return debuggers[0];
			}
			return null;
		}
	}

	public static bool IsDebuggerLoaded => currentDebugger != null;

	public static IList<BreakpointBookmark> Breakpoints
	{
		get
		{
			List<BreakpointBookmark> list = new List<BreakpointBookmark>();
			foreach (SDBookmark bookmark in ICSharpCode.SharpDevelop.Bookmarks.BookmarkManager.Bookmarks)
			{
				if (bookmark is BreakpointBookmark item)
				{
					list.Add(item);
				}
			}
			return list.AsReadOnly();
		}
	}

	private static bool CanCloseOldToolTip
	{
		get
		{
			if (oldToolTipControl != null)
			{
				return oldToolTipControl.AllowClose;
			}
			return false;
		}
	}

	public static event EventHandler DebugStarted;

	public static event EventHandler DebugStopped;

	public static event EventHandler<BreakpointBookmarkEventArgs> BreakPointChanged;

	public static event EventHandler<BreakpointBookmarkEventArgs> BreakPointAdded;

	public static event EventHandler<BreakpointBookmarkEventArgs> BreakPointRemoved;

	static DebuggerService()
	{
		oldLayoutConfiguration = "Default";
		debugCategory = null;
		ProjectService.SolutionLoaded += delegate
		{
			ProjectService.OpenSolution.Preferences.StartupProjectChanged += StartupProjectChanged;
			ClearCurrentDebugger();
		};
		ProjectService.SolutionClosing += delegate
		{
			ProjectService.OpenSolution.Preferences.StartupProjectChanged -= StartupProjectChanged;
			currentDebugger = null;
		};
		WorkbenchSingleton.WorkbenchCreated += WorkspaceCreated;
		ICSharpCode.SharpDevelop.Bookmarks.BookmarkManager.Added += BookmarkAdded;
		ICSharpCode.SharpDevelop.Bookmarks.BookmarkManager.Removed += BookmarkRemoved;
	}

	public static bool ClearCurrentDebugger()
	{
		ClearDebugMessages();
		currentDebugger = null;
		return true;
	}

	private static void StartupProjectChanged(object sender, EventArgs e)
	{
		currentDebugger = null;
	}

	private static void GetDescriptors()
	{
		if (debuggers == null)
		{
			debuggers = (DebuggerDescriptor[])AddInTree.BuildItems("/SharpDevelop/Services/DebuggerService/Debugger", null, throwOnNotFound: false).ToArray(typeof(DebuggerDescriptor));
		}
	}

	private static IDebugger GetCompatibleDebugger()
	{
		GetDescriptors();
		IProject project = null;
		if (ProjectService.OpenSolution != null)
		{
			project = ProjectService.OpenSolution.StartupProject;
		}
		DebuggerDescriptor[] array = debuggers;
		foreach (DebuggerDescriptor debuggerDescriptor in array)
		{
			if (debuggerDescriptor.Debugger != null && debuggerDescriptor.Debugger.CanDebug(project))
			{
				return debuggerDescriptor.Debugger;
			}
		}
		return new DefaultDebugger();
	}

	private static void OnDebugStarted(object sender, EventArgs e)
	{
		WorkbenchSingleton.Workbench.WorkbenchLayout.StoreConfiguration();
		oldLayoutConfiguration = LayoutConfiguration.CurrentLayoutName;
		LayoutConfiguration.CurrentLayoutName = "Debug";
		ClearDebugMessages();
		if (DebuggerService.DebugStarted != null)
		{
			DebuggerService.DebugStarted(null, e);
		}
	}

	private static void OnDebugStopped(object sender, EventArgs e)
	{
		CurrentLineBookmark.Remove();
		WorkbenchSingleton.Workbench.WorkbenchLayout.StoreConfiguration();
		LayoutConfiguration.CurrentLayoutName = oldLayoutConfiguration;
		if (DebuggerService.DebugStopped != null)
		{
			DebuggerService.DebugStopped(null, e);
		}
	}

	private static void EnsureDebugCategory()
	{
		if (debugCategory == null)
		{
			debugCategory = new MessageViewCategory("Debug", "${res:MainWindow.Windows.OutputWindow.DebugCategory}");
			CompilerMessageView compilerMessageView = (CompilerMessageView)WorkbenchSingleton.Workbench.GetPad(typeof(CompilerMessageView)).PadContent;
			compilerMessageView.AddCategory(debugCategory);
		}
	}

	public static void ClearDebugMessages()
	{
		EnsureDebugCategory();
		debugCategory.ClearText();
	}

	public static void PrintDebugMessage(string msg)
	{
		EnsureDebugCategory();
		debugCategory.AppendText(msg);
	}

	private static void OnBreakPointChanged(BreakpointBookmarkEventArgs e)
	{
		if (DebuggerService.BreakPointChanged != null)
		{
			DebuggerService.BreakPointChanged(null, e);
		}
	}

	private static void OnBreakPointAdded(BreakpointBookmarkEventArgs e)
	{
		if (DebuggerService.BreakPointAdded != null)
		{
			DebuggerService.BreakPointAdded(null, e);
		}
	}

	private static void OnBreakPointRemoved(BreakpointBookmarkEventArgs e)
	{
		if (DebuggerService.BreakPointRemoved != null)
		{
			DebuggerService.BreakPointRemoved(null, e);
		}
	}

	private static void BookmarkAdded(object sender, ICSharpCode.SharpDevelop.Bookmarks.BookmarkEventArgs e)
	{
		if (e.Bookmark is BreakpointBookmark breakpointBookmark)
		{
			breakpointBookmark.LineNumberChanged += BookmarkChanged;
			OnBreakPointAdded(new BreakpointBookmarkEventArgs(breakpointBookmark));
		}
	}

	private static void BookmarkRemoved(object sender, ICSharpCode.SharpDevelop.Bookmarks.BookmarkEventArgs e)
	{
		if (e.Bookmark is BreakpointBookmark breakpointBookmark)
		{
			breakpointBookmark.RemoveMarker();
			OnBreakPointRemoved(new BreakpointBookmarkEventArgs(breakpointBookmark));
		}
	}

	private static void BookmarkChanged(object sender, EventArgs e)
	{
		if (sender is BreakpointBookmark breakpointBookmark)
		{
			OnBreakPointChanged(new BreakpointBookmarkEventArgs(breakpointBookmark));
		}
	}

	public static void ToggleBreakpointAt(IDocument document, string fileName, int lineNumber)
	{
		for (int i = 0; i < document.BookmarkManager.Marks.Count; i++)
		{
			Bookmark bookmark = document.BookmarkManager.Marks[i];
			if (bookmark is BreakpointBookmark breakpointBookmark && breakpointBookmark.LineNumber == lineNumber)
			{
				document.BookmarkManager.RemoveMark(bookmark);
				return;
			}
		}
		string text = document.GetText(document.GetLineSegment(lineNumber));
		foreach (char c in text)
		{
			if (!char.IsWhiteSpace(c))
			{
				document.BookmarkManager.AddMark(new BreakpointBookmark(fileName, document, lineNumber));
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, lineNumber));
				document.CommitUpdate();
				break;
			}
		}
	}

	private static void WorkspaceCreated(object sender, EventArgs args)
	{
		WorkbenchSingleton.Workbench.ViewOpened += ViewContentOpened;
		WorkbenchSingleton.Workbench.ViewClosed += ViewContentClosed;
	}

	public static void ViewContentOpened(object sender, ViewContentEventArgs e)
	{
		if (e.Content is ITextEditorControlProvider)
		{
			TextArea textArea = ((ITextEditorControlProvider)e.Content).TextEditorControl.ActiveTextAreaControl.TextArea;
			textArea.IconBarMargin.MouseDown += IconBarMouseDown;
			textArea.ToolTipRequest += TextAreaToolTipRequest;
			textArea.MouseLeave += TextAreaMouseLeave;
		}
	}

	public static void ViewContentClosed(object sender, ViewContentEventArgs e)
	{
		if (e.Content is ITextEditorControlProvider)
		{
			TextArea textArea = ((ITextEditorControlProvider)e.Content).TextEditorControl.ActiveTextAreaControl.TextArea;
			textArea.IconBarMargin.MouseDown -= IconBarMouseDown;
			textArea.ToolTipRequest -= TextAreaToolTipRequest;
			textArea.MouseLeave -= TextAreaMouseLeave;
		}
	}

	public static void RemoveCurrentLineMarker()
	{
		CurrentLineBookmark.Remove();
	}

	public static void JumpToCurrentLine(string SourceFullFilename, int StartLine, int StartColumn, int EndLine, int EndColumn)
	{
		IViewContent viewContent = FileService.JumpToFilePosition(SourceFullFilename, StartLine - 1, StartColumn - 1);
		if (viewContent.WorkbenchWindow != null && viewContent.WorkbenchWindow.ActiveViewContent != viewContent)
		{
			viewContent.WorkbenchWindow.SwitchView(0);
		}
		CurrentLineBookmark.SetPosition(viewContent, StartLine, StartColumn, EndLine, EndColumn);
	}

	private static void IconBarMouseDown(AbstractMargin iconBar, Point mousepos, MouseButtons mouseButtons)
	{
		if (mouseButtons == MouseButtons.Left)
		{
			Rectangle drawingPosition = iconBar.TextArea.TextView.DrawingPosition;
			TextLocation logicalPosition = iconBar.TextArea.TextView.GetLogicalPosition(0, mousepos.Y - drawingPosition.Top);
			if (logicalPosition.Y >= 0 && logicalPosition.Y < iconBar.TextArea.Document.TotalNumberOfLines)
			{
				ToggleBreakpointAt(iconBar.TextArea.Document, iconBar.TextArea.MotherTextEditorControl.FileName, logicalPosition.Y);
				iconBar.TextArea.Refresh(iconBar);
			}
		}
	}

	public static void TextAreaToolTipRequest(object sender, ToolTipRequestEventArgs e)
	{
		DebuggerGridControl debuggerGridControl = null;
		try
		{
			TextArea textArea = (TextArea)sender;
			if (e.ToolTipShown || (oldToolTipControl != null && !oldToolTipControl.AllowClose) || !CodeCompletionOptions.TooltipsEnabled || (CodeCompletionOptions.TooltipsOnlyWhenDebugging && (currentDebugger == null || !currentDebugger.IsDebugging)) || !e.InDocument)
			{
				return;
			}
			ToolTipInfo toolTipInfo = null;
			try
			{
				foreach (ITextAreaToolTipProvider item in AddInTree.BuildItems<ITextAreaToolTipProvider>("/SharpDevelop/ViewContent/DefaultTextEditor/ToolTips", null, throwOnNotFound: false))
				{
					if ((toolTipInfo = item.GetToolTipInfo(textArea, e)) != null)
					{
						break;
					}
				}
			}
			catch
			{
			}
			if (toolTipInfo != null)
			{
				debuggerGridControl = toolTipInfo.ToolTipControl as DebuggerGridControl;
				if (toolTipInfo.ToolTipText != null)
				{
					e.ShowToolTip(toolTipInfo.ToolTipText);
				}
			}
			CloseOldToolTip();
			debuggerGridControl?.ShowForm(textArea, e.LogicalPosition);
			oldToolTipControl = debuggerGridControl;
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
		finally
		{
			if (debuggerGridControl == null && CanCloseOldToolTip)
			{
				CloseOldToolTip();
			}
		}
	}

	private static void CloseOldToolTip()
	{
		if (oldToolTipControl != null)
		{
			oldToolTipControl.FindForm()?.Close();
			oldToolTipControl = null;
		}
	}

	private static void TextAreaMouseLeave(object source, EventArgs e)
	{
		if (CanCloseOldToolTip && !oldToolTipControl.IsMouseOver)
		{
			CloseOldToolTip();
		}
	}

	internal static ToolTipInfo GetToolTipInfo(TextArea textArea, ToolTipRequestEventArgs e)
	{
		TextLocation logicalPosition = e.LogicalPosition;
		IDocument document = textArea.Document;
		IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(textArea.MotherTextEditorControl.FileName);
		if (expressionFinder == null)
		{
			return null;
		}
		if (logicalPosition.Line > document.TotalNumberOfLines)
		{
			return null;
		}
		LineSegment lineSegment = document.GetLineSegment(logicalPosition.Y);
		if (logicalPosition.X > lineSegment.Length - 1)
		{
			return null;
		}
		string textContent = document.TextContent;
		ExpressionResult expressionResult = expressionFinder.FindFullExpression(textContent, lineSegment.Offset + logicalPosition.X);
		string text = (expressionResult.Expression ?? "").Trim();
		if (text.Length > 0)
		{
			ResolveResult result = ParserService.Resolve(expressionResult, logicalPosition.Y + 1, logicalPosition.X + 1, textArea.MotherTextEditorControl.FileName, textContent);
			bool debuggerCanShowValue;
			string text2 = GetText(result, text, out debuggerCanShowValue);
			if (text2 != null)
			{
				if (Control.ModifierKeys == Keys.Control)
				{
					text2 = "expr: " + expressionResult.ToString() + "\n" + text2;
				}
				else if (debuggerCanShowValue && currentDebugger != null)
				{
					return new ToolTipInfo(currentDebugger.GetTooltipControl(expressionResult.Expression));
				}
				return new ToolTipInfo(text2);
			}
		}
		return null;
	}

	private static string GetText(ResolveResult result, string expression, out bool debuggerCanShowValue)
	{
		debuggerCanShowValue = false;
		if (result == null)
		{
			if (Control.ModifierKeys != Keys.Control)
			{
				return null;
			}
			return "";
		}
		if (result is MixedResolveResult)
		{
			return GetText(((MixedResolveResult)result).PrimaryResult, expression, out debuggerCanShowValue);
		}
		IAmbience currentAmbience = AmbienceService.CurrentAmbience;
		currentAmbience.ConversionFlags = ConversionFlags.StandardConversionFlags | ConversionFlags.ShowAccessibility;
		if (result is MemberResolveResult)
		{
			return GetMemberText(currentAmbience, ((MemberResolveResult)result).ResolvedMember, expression, out debuggerCanShowValue);
		}
		if (result is LocalResolveResult)
		{
			LocalResolveResult localResolveResult = (LocalResolveResult)result;
			currentAmbience.ConversionFlags = ConversionFlags.UseFullyQualifiedNames | ConversionFlags.QualifiedNamesOnlyForReturnTypes | ConversionFlags.ShowReturnType;
			StringBuilder stringBuilder = new StringBuilder();
			if (localResolveResult.IsParameter)
			{
				stringBuilder.Append("parameter ");
			}
			else
			{
				stringBuilder.Append("local variable ");
			}
			stringBuilder.Append(currentAmbience.Convert(localResolveResult.Field));
			if (currentDebugger != null)
			{
				string valueAsString = currentDebugger.GetValueAsString(localResolveResult.Field.Name);
				if (valueAsString != null)
				{
					debuggerCanShowValue = true;
					stringBuilder.Append(" = ");
					stringBuilder.Append(valueAsString);
				}
			}
			string documentation = localResolveResult.Field.Documentation;
			if (documentation != null && documentation.Length > 0)
			{
				stringBuilder.Append('\n');
				stringBuilder.Append(CodeCompletionData.GetDocumentation(documentation));
			}
			return stringBuilder.ToString();
		}
		if (result is NamespaceResolveResult)
		{
			return "namespace " + ((NamespaceResolveResult)result).Name;
		}
		if (result is TypeResolveResult)
		{
			IClass resolvedClass = ((TypeResolveResult)result).ResolvedClass;
			if (resolvedClass != null)
			{
				return GetMemberText(currentAmbience, resolvedClass, expression, out debuggerCanShowValue);
			}
			return currentAmbience.Convert(result.ResolvedType);
		}
		if (result is MethodResolveResult)
		{
			MethodResolveResult methodResolveResult = result as MethodResolveResult;
			IMethod methodIfSingleOverload = methodResolveResult.GetMethodIfSingleOverload();
			if (methodIfSingleOverload != null)
			{
				return GetMemberText(currentAmbience, methodIfSingleOverload, expression, out debuggerCanShowValue);
			}
			return "Overload of " + currentAmbience.Convert(methodResolveResult.ContainingType) + "." + methodResolveResult.Name;
		}
		if (result is TextResolveResult)
		{
			return ((TextResolveResult)result).Text;
		}
		if (Control.ModifierKeys == Keys.Control)
		{
			if (result.ResolvedType != null)
			{
				return "expression of type " + currentAmbience.Convert(result.ResolvedType);
			}
			return "ResolveResult without ResolvedType";
		}
		return null;
	}

	private static string GetMemberText(IAmbience ambience, IDecoration member, string expression, out bool debuggerCanShowValue)
	{
		bool flag = false;
		debuggerCanShowValue = false;
		StringBuilder stringBuilder = new StringBuilder();
		if (member is IField)
		{
			stringBuilder.Append(ambience.Convert(member as IField));
			flag = true;
		}
		else if (member is IProperty)
		{
			stringBuilder.Append(ambience.Convert(member as IProperty));
			flag = true;
		}
		else if (member is IEvent)
		{
			stringBuilder.Append(ambience.Convert(member as IEvent));
		}
		else if (member is IMethod)
		{
			stringBuilder.Append(ambience.Convert(member as IMethod));
		}
		else if (member is IClass)
		{
			stringBuilder.Append(ambience.Convert(member as IClass));
		}
		else
		{
			stringBuilder.Append("unknown member ");
			stringBuilder.Append(member.ToString());
		}
		if (flag && currentDebugger != null)
		{
			LoggingService.Info("asking debugger for value of '" + expression + "'");
			string valueAsString = currentDebugger.GetValueAsString(expression);
			if (valueAsString != null)
			{
				debuggerCanShowValue = true;
				stringBuilder.Append(" = ");
				stringBuilder.Append(valueAsString);
			}
		}
		string documentation = member.Documentation;
		if (documentation != null && documentation.Length > 0)
		{
			stringBuilder.Append('\n');
			stringBuilder.Append(CodeCompletionData.GetDocumentation(documentation));
		}
		return stringBuilder.ToString();
	}
}

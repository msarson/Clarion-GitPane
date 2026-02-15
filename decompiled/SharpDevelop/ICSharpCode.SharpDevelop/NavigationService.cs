using System;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class NavigationService
{
	private static LinkedList<INavigationPoint> history;

	private static LinkedListNode<INavigationPoint> currentNode;

	private static bool loggingSuspended;

	private static LinkedList<IWorkbenchWindow> windowsHistory;

	private static IWorkbenchWindow lastSelectedWindow;

	public static bool CanNavigateBack
	{
		get
		{
			if (currentNode != history.First)
			{
				return currentNode != null;
			}
			return false;
		}
	}

	public static bool CanNavigateForwards
	{
		get
		{
			if (currentNode != history.Last)
			{
				return currentNode != null;
			}
			return false;
		}
	}

	public static int Count => history.Count;

	public static INavigationPoint CurrentPosition
	{
		get
		{
			if (currentNode != null)
			{
				return currentNode.Value;
			}
			return null;
		}
		set
		{
			Log(value);
		}
	}

	public static bool IsLogging => !loggingSuspended;

	public static ICollection<INavigationPoint> Points => new List<INavigationPoint>(history);

	public static event EventHandler HistoryChanged;

	static NavigationService()
	{
		history = new LinkedList<INavigationPoint>();
		windowsHistory = new LinkedList<IWorkbenchWindow>();
		WorkbenchSingleton.WorkbenchCreated += WorkbenchCreatedHandler;
		FileService.FileRenamed += FileService_FileRenamed;
		ProjectService.SolutionClosed += ProjectService_SolutionClosed;
	}

	public static void ContentChanging(object sender, EventArgs e)
	{
		foreach (INavigationPoint item in history)
		{
			item.ContentChanging(sender, e);
		}
	}

	private static void Log(IWorkbenchWindow window)
	{
		if (window != null)
		{
			Log(window.ViewContent);
		}
	}

	private static void Log(IViewContent vc)
	{
		if (vc != null)
		{
			Log(vc.BuildNavPoint());
		}
	}

	public static void Log(INavigationPoint pointToLog)
	{
		if (!loggingSuspended)
		{
			LogInternal(pointToLog);
		}
	}

	private static void LogInternal(INavigationPoint p)
	{
		if (p != null && p.FileName != null && !(p.FileName == string.Empty))
		{
			if (currentNode == null)
			{
				currentNode = history.AddFirst(p);
			}
			else if (p.Equals(currentNode.Value))
			{
				currentNode.Value = p;
			}
			else
			{
				currentNode = history.AddAfter(currentNode, p);
			}
			OnHistoryChanged();
		}
	}

	public static INavigationPoint Log()
	{
		return null;
	}

	public static void ClearHistory()
	{
		ClearHistory(clearCurrentPosition: false);
	}

	public static void ClearHistory(bool clearCurrentPosition)
	{
		INavigationPoint currentPosition = CurrentPosition;
		history.Clear();
		currentNode = null;
		if (!clearCurrentPosition)
		{
			LogInternal(currentPosition);
		}
		OnHistoryChanged();
	}

	public static void ClearFileHistory(string fileName)
	{
		for (LinkedListNode<INavigationPoint> linkedListNode = history.First; linkedListNode != null; linkedListNode = ((linkedListNode == null) ? history.First : linkedListNode.Next))
		{
			if (linkedListNode.Value.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
			{
				LinkedListNode<INavigationPoint> node = linkedListNode;
				linkedListNode = linkedListNode.Previous;
				history.Remove(node);
			}
		}
		currentNode = history.First;
		OnHistoryChanged();
	}

	public static void Go(int delta)
	{
		if (delta == 0)
		{
			return;
		}
		if (0 > delta)
		{
			while (0 > delta && currentNode != history.First)
			{
				currentNode = currentNode.Previous;
				delta++;
			}
		}
		else
		{
			while (0 < delta && currentNode != history.Last)
			{
				currentNode = currentNode.Next;
				delta--;
			}
		}
		SyncViewWithModel();
	}

	public static void Go(INavigationPoint target)
	{
		if (target != null)
		{
			LinkedListNode<INavigationPoint> linkedListNode = history.Find(target);
			if (linkedListNode != null)
			{
				currentNode = linkedListNode;
			}
			else
			{
				LoggingService.ErrorFormatted("Logging additional point: {0}", target);
				LogInternal(target);
			}
			SyncViewWithModel();
		}
	}

	private static void SyncViewWithModel()
	{
		SuspendLogging();
		if (CurrentPosition != null)
		{
			CurrentPosition.JumpTo();
		}
		ResumeLogging();
	}

	public static void SuspendLogging()
	{
		loggingSuspended = true;
	}

	public static void ResumeLogging()
	{
		loggingSuspended = false;
	}

	private static void WorkbenchCreatedHandler(object sender, EventArgs e)
	{
		WorkbenchSingleton.Workbench.ViewOpened += ViewContentOpened;
		WorkbenchSingleton.Workbench.ViewClosed += ViewContentClosed;
	}

	private static void ViewContentOpened(object sender, ViewContentEventArgs e)
	{
		e.Content.WorkbenchWindow.WindowSelected += WorkBenchWindowSelected;
		windowsHistory.Remove(e.Content.WorkbenchWindow);
		windowsHistory.AddFirst(e.Content.WorkbenchWindow);
	}

	private static void ViewContentClosed(object sender, ViewContentEventArgs e)
	{
		e.Content.WorkbenchWindow.WindowSelected -= WorkBenchWindowSelected;
		windowsHistory.Remove(e.Content.WorkbenchWindow);
		if (windowsHistory.Count > 0)
		{
			windowsHistory.First.Value.SelectWindow();
		}
	}

	private static void WorkBenchWindowSelected(object sender, EventArgs e)
	{
		try
		{
			IWorkbenchWindow workbenchWindow = sender as IWorkbenchWindow;
			if (workbenchWindow != lastSelectedWindow)
			{
				windowsHistory.Remove(workbenchWindow);
				windowsHistory.AddFirst(workbenchWindow);
				Log(workbenchWindow);
				lastSelectedWindow = workbenchWindow;
			}
		}
		catch (Exception ex)
		{
			LoggingService.ErrorFormatted("{0}:\n{1}", ex.Message, ex.StackTrace);
			throw;
		}
	}

	private static void FileService_FileRenamed(object sender, FileRenameEventArgs e)
	{
		foreach (INavigationPoint item in history)
		{
			if (item.FileName.Equals(e.SourceFile))
			{
				item.FileNameChanged(e.TargetFile);
			}
		}
	}

	private static void ProjectService_SolutionClosed(object sender, EventArgs e)
	{
		ClearHistory(clearCurrentPosition: true);
		lastSelectedWindow = null;
	}

	private static void OnHistoryChanged()
	{
		if (NavigationService.HistoryChanged != null)
		{
			NavigationService.HistoryChanged(CurrentPosition, EventArgs.Empty);
		}
	}
}

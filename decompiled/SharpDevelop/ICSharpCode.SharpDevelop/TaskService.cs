using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class TaskService
{
	private static List<Task> tasks;

	private static Dictionary<TaskType, int> taskCount;

	private static MessageViewCategory buildMessageViewCategory;

	private static bool inUpdate;

	public static MessageViewCategory BuildMessageViewCategory => buildMessageViewCategory;

	public static int TaskCount => tasks.Count - GetCount(TaskType.Comment);

	public static IEnumerable<Task> Tasks
	{
		get
		{
			foreach (Task task in tasks)
			{
				if (task.TaskType != TaskType.Comment)
				{
					yield return task;
				}
			}
		}
	}

	public static IEnumerable<Task> CommentTasks
	{
		get
		{
			foreach (Task task in tasks)
			{
				if (task.TaskType == TaskType.Comment)
				{
					yield return task;
				}
			}
		}
	}

	public static bool SomethingWentWrong => GetCount(TaskType.Error) + GetCount(TaskType.Warning) > 0;

	public static bool InUpdate
	{
		get
		{
			return inUpdate;
		}
		set
		{
			if (inUpdate != value)
			{
				inUpdate = value;
				if (TaskService.InUpdateChanged != null)
				{
					TaskService.InUpdateChanged(null, EventArgs.Empty);
				}
			}
		}
	}

	public static event TaskEventHandler Added;

	public static event TaskEventHandler Removed;

	public static event EventHandler Cleared;

	public static event EventHandler InUpdateChanged;

	public static int GetCount(TaskType type)
	{
		if (!taskCount.ContainsKey(type))
		{
			return 0;
		}
		return taskCount[type];
	}

	public static bool HasCriticalErrors(bool treatWarningsAsErrors)
	{
		if (treatWarningsAsErrors)
		{
			return SomethingWentWrong;
		}
		return GetCount(TaskType.Error) > 0;
	}

	public static string GetLastErrorDescription()
	{
		foreach (Task task in tasks)
		{
			if (task.TaskType == TaskType.Error)
			{
				return task.Description;
			}
		}
		return "";
	}

	static TaskService()
	{
		tasks = new List<Task>();
		taskCount = new Dictionary<TaskType, int>();
		buildMessageViewCategory = new MessageViewCategory("Build", "${res:MainWindow.Windows.OutputWindow.BuildCategory}");
		FileService.FileRenamed += CheckFileRename;
		FileService.FileRemoved += CheckFileRemove;
		ProjectService.SolutionClosed += ProjectServiceSolutionClosed;
	}

	private static void ProjectServiceSolutionClosed(object sender, EventArgs e)
	{
		Clear();
	}

	private static void CheckFileRemove(object sender, FileEventArgs e)
	{
		for (int i = 0; i < tasks.Count; i++)
		{
			Task task = tasks[i];
			if (FileUtility.IsEqualFileName(task.FileName, e.FileName))
			{
				Remove(task);
				i--;
			}
		}
	}

	private static void CheckFileRename(object sender, FileRenameEventArgs e)
	{
		for (int i = 0; i < tasks.Count; i++)
		{
			Task task = tasks[i];
			if (FileUtility.IsEqualFileName(task.FileName, e.SourceFile))
			{
				Remove(task);
				task.FileName = Path.GetFullPath(e.TargetFile);
				Add(task);
				i--;
			}
		}
	}

	public static void Clear()
	{
		taskCount.Clear();
		tasks.Clear();
		OnCleared(EventArgs.Empty);
	}

	public static void ClearExceptCommentTasks()
	{
		List<Task> list = new List<Task>(CommentTasks);
		Clear();
		foreach (Task item in list)
		{
			Add(item);
		}
	}

	public static void Add(Task task)
	{
		tasks.Add(task);
		if (!taskCount.ContainsKey(task.TaskType))
		{
			taskCount[task.TaskType] = 1;
		}
		else
		{
			taskCount[task.TaskType]++;
		}
		OnAdded(new TaskEventArgs(task));
	}

	public static void AddRange(IEnumerable<Task> tasks)
	{
		foreach (Task task in tasks)
		{
			Add(task);
		}
	}

	public static void Remove(Task task)
	{
		if (tasks.Contains(task))
		{
			tasks.Remove(task);
			taskCount[task.TaskType]--;
			OnRemoved(new TaskEventArgs(task));
		}
	}

	public static void UpdateCommentTags(string fileName, List<TagComment> tagComments)
	{
		if (fileName != null && tagComments != null)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(UpdateCommentTagsInvoked, fileName, tagComments);
		}
	}

	private static void UpdateCommentTagsInvoked(string fileName, List<TagComment> tagComments)
	{
		List<Task> list = new List<Task>();
		foreach (TagComment tagComment in tagComments)
		{
			list.Add(Task.NewCommentTagTask(fileName, tagComment));
		}
		List<Task> list2 = new List<Task>();
		foreach (Task commentTask in CommentTasks)
		{
			if (FileUtility.IsEqualFileName(commentTask.FileName, fileName))
			{
				list2.Add(commentTask);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j] != null && list[i].Line == list2[j].Line && list[i].Column == list2[j].Column && list[i].Description == list2[j].Description)
				{
					list[i] = null;
					list2[j] = null;
					break;
				}
			}
		}
		foreach (Task item in list)
		{
			if (item != null)
			{
				Add(item);
			}
		}
		foreach (Task item2 in list2)
		{
			if (item2 != null)
			{
				Remove(item2);
			}
		}
	}

	private static void OnCleared(EventArgs e)
	{
		if (TaskService.Cleared != null)
		{
			TaskService.Cleared(null, e);
		}
	}

	private static void OnAdded(TaskEventArgs e)
	{
		if (TaskService.Added != null)
		{
			TaskService.Added(null, e);
		}
	}

	private static void OnRemoved(TaskEventArgs e)
	{
		if (TaskService.Removed != null)
		{
			TaskService.Removed(null, e);
		}
	}
}

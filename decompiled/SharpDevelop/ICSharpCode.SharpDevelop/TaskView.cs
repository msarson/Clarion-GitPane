using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class TaskView : ListView
{
	private class TaskViewSorter : IComparer
	{
		private int sortCol = -1;

		private SortOrder sortOrder = SortOrder.Ascending;

		public TaskViewSorter(int col, SortOrder order)
		{
			sortCol = col;
			sortOrder = order;
		}

		protected int CompareLineNumbers(ListViewItem a, ListViewItem b)
		{
			return ((Task)a.Tag).Line.CompareTo(((Task)b.Tag).Line);
		}

		protected int CompareAsText(ListViewItem a, ListViewItem b, TaskViewCols col)
		{
			return CompareAsText(a, b, (int)col);
		}

		protected int CompareAsText(ListViewItem a, ListViewItem b, int col)
		{
			return a.SubItems[col].Text.CompareTo(b.SubItems[col].Text);
		}

		public int Compare(object x, object y)
		{
			ListViewItem a = (ListViewItem)x;
			ListViewItem b = (ListViewItem)y;
			int num = 0;
			if (sortCol == 1)
			{
				num = CompareLineNumbers(a, b);
			}
			else
			{
				num = CompareAsText(a, b, sortCol);
				if (num == 0)
				{
					if (sortCol == 4)
					{
						num = CompareAsText(a, b, TaskViewCols.File);
						if (num == 0)
						{
							num = CompareLineNumbers(a, b);
						}
					}
					else if (sortCol == 3)
					{
						num = CompareLineNumbers(a, b);
					}
				}
			}
			if (sortOrder == SortOrder.Descending)
			{
				return -num;
			}
			return num;
		}
	}

	private ColumnHeader columnType = new ColumnHeader();

	private ColumnHeader columnLine = new ColumnHeader();

	private ColumnHeader columnDescription = new ColumnHeader();

	private ColumnHeader columnFile = new ColumnHeader();

	private ColumnHeader columnPath = new ColumnHeader();

	private ToolTip taskToolTip = new ToolTip();

	private Properties props;

	private int widthChangingCount;

	private bool manuallySettingWidth;

	private bool _ColumnWidthChanged;

	private bool forceResize;

	private int originalWidth;

	private ListViewItem currentListViewItem;

	private int currentSortColumn = -1;

	private SortOrder currentSortOrder = SortOrder.Ascending;

	private TaskViewSorter mySorter;

	public Task SelectedTask
	{
		get
		{
			if (base.FocusedItem == null)
			{
				return null;
			}
			return (Task)base.FocusedItem.Tag;
		}
	}

	internal bool NextValid
	{
		get
		{
			if (base.Items.Count > 0)
			{
				if (base.FocusedItem != null && base.FocusedItem.Selected)
				{
					return base.FocusedItem.Index < base.Items.Count - 1;
				}
				return true;
			}
			return false;
		}
	}

	internal bool PreviousValid
	{
		get
		{
			if (base.Items.Count > 0)
			{
				if (base.FocusedItem != null && base.FocusedItem.Selected)
				{
					return base.FocusedItem.Index > 0;
				}
				return true;
			}
			return false;
		}
	}

	public bool TaskIsSelected => base.FocusedItem != null;

	public IEnumerable<Task> SelectedTasks
	{
		get
		{
			foreach (ListViewItem item in base.SelectedItems)
			{
				yield return (Task)item.Tag;
			}
		}
	}

	private Properties Props
	{
		get
		{
			if (props == null)
			{
				props = PropertyService.GetSubProperties("TaskViewControls", base.Name, "Columns");
			}
			return props;
		}
	}

	public List<Task> Tasks
	{
		get
		{
			List<Task> list = new List<Task>();
			foreach (ListViewItem item in base.Items)
			{
				list.Add((Task)item.Tag);
			}
			return list;
		}
	}

	private void DoPosition(bool up)
	{
		Select();
		SendKeys.Send(up ? "{UP}" : "{DOWN}");
		SendKeys.Send("~");
		Application.DoEvents();
		foreach (int selectedIndex in base.SelectedIndices)
		{
			base.Items[selectedIndex].Selected = false;
		}
		if (base.FocusedItem != null)
		{
			base.FocusedItem.Selected = true;
		}
		OnItemActivate(EventArgs.Empty);
	}

	private void SetToItem(int item)
	{
		ListViewItem listViewItem = base.Items[item];
		listViewItem.Selected = true;
		listViewItem.Focused = true;
		if (base.Items.Count > 1 && item > 0)
		{
			SendKeys.Send("{UP}");
			SendKeys.Send("{DOWN}");
			Application.DoEvents();
		}
		OnItemActivate(EventArgs.Empty);
	}

	internal void SelectNext()
	{
		if (base.Items.Count > 0)
		{
			if (base.FocusedItem == null || !base.FocusedItem.Selected)
			{
				SetToItem(0);
			}
			else if (base.FocusedItem.Index < base.Items.Count - 1)
			{
				DoPosition(up: false);
			}
		}
	}

	internal void SelectPrevious()
	{
		if (base.Items.Count > 0)
		{
			if (base.FocusedItem == null || !base.FocusedItem.Selected)
			{
				SetToItem(base.Items.Count - 1);
			}
			else if (base.FocusedItem.Index > 0)
			{
				DoPosition(up: true);
			}
		}
	}

	public void CopySelectionToClipboard()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Task selectedTask in SelectedTasks)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(selectedTask.Description);
			if (string.IsNullOrEmpty(selectedTask.FileName))
			{
				continue;
			}
			stringBuilder.Append(" - ");
			stringBuilder.Append(selectedTask.FileName);
			if (selectedTask.Line >= 0)
			{
				stringBuilder.Append(':');
				stringBuilder.Append(selectedTask.Line + 1);
				if (selectedTask.Column > 0)
				{
					stringBuilder.Append(',');
					stringBuilder.Append(selectedTask.Column + 1);
				}
			}
		}
		ClipboardWrapper.SetText(stringBuilder.ToString());
	}

	public void SelectAll()
	{
		BeginUpdate();
		try
		{
			foreach (ListViewItem item in base.Items)
			{
				item.Selected = true;
			}
		}
		finally
		{
			EndUpdate();
		}
	}

	private TaskView()
		: this("TaskView")
	{
	}

	public TaskView(string listName)
	{
		base.Name = listName;
		Font = FontService.GetFont(FontService.FontType.ListControls);
		RefreshColumnNames();
		base.Columns.Add(columnType);
		base.Columns.Add(columnLine);
		base.Columns.Add(columnDescription);
		base.Columns.Add(columnFile);
		base.Columns.Add(columnPath);
		base.FullRowSelect = true;
		base.HideSelection = false;
		base.AutoArrange = true;
		base.Alignment = ListViewAlignment.Left;
		base.View = View.Details;
		Dock = DockStyle.Fill;
		base.GridLines = true;
		base.Activation = ItemActivation.OneClick;
		base.ColumnWidthChanged += OnColumnWidthChanged;
		base.SmallImageList = (base.LargeImageList = new ImageList
		{
			ColorDepth = ColorDepth.Depth32Bit,
			Images = 
			{
				(Image)ResourceService.GetBitmap("Icons.16x16.Error"),
				(Image)ResourceService.GetBitmap("Icons.16x16.Warning"),
				(Image)ResourceService.GetBitmap("Icons.16x16.Information"),
				(Image)ResourceService.GetBitmap("Icons.16x16.Question")
			}
		});
		taskToolTip.InitialDelay = 500;
		taskToolTip.ReshowDelay = 100;
		taskToolTip.AutoPopDelay = 5000;
		SortBy(TaskViewCols.Path);
	}

	protected override void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
	{
		if (!manuallySettingWidth)
		{
			widthChangingCount++;
		}
		base.OnColumnWidthChanging(e);
	}

	protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
	{
		if (!manuallySettingWidth)
		{
			if (widthChangingCount == 1)
			{
				SetDefaultColumnWidths();
			}
			_ColumnWidthChanged = true;
		}
		widthChangingCount = 0;
		base.OnColumnWidthChanged(e);
	}

	private void OnColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
	{
		_ColumnWidthChanged = true;
	}

	public void AutoResizeColumnWidths()
	{
		if (base.Width != 0 && originalWidth != base.Width)
		{
			SetDefaultColumnWidths();
			originalWidth = base.Width;
		}
	}

	protected void SetDefaultColumnWidths()
	{
		if (!manuallySettingWidth)
		{
			manuallySettingWidth = true;
			BeginUpdate();
			AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.HeaderSize);
			AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
			int num = base.Width - columnType.Width - columnLine.Width;
			columnFile.Width = num * 15 / 100;
			columnPath.Width = num * 15 / 100;
			columnDescription.Width = num - columnFile.Width - columnPath.Width - 5;
			EndUpdate();
			manuallySettingWidth = false;
		}
	}

	public void RestoreColumnWidths()
	{
		if (Props.Count == 0)
		{
			forceResize = true;
			return;
		}
		SetDefaultColumnWidths();
		manuallySettingWidth = true;
		BeginUpdate();
		columnDescription.Width = Props.Get("Description", columnDescription.Width);
		columnFile.Width = Props.Get("File", columnFile.Width);
		columnPath.Width = Props.Get("Path", columnPath.Width);
		EndUpdate();
		manuallySettingWidth = false;
	}

	public void StoreColumnWidths()
	{
		StoreColumnWidths(force: false);
	}

	public void StoreColumnWidths(bool force)
	{
		if (force || _ColumnWidthChanged)
		{
			Props.Set("Description", columnDescription.Width);
			Props.Set("File", columnFile.Width);
			Props.Set("Path", columnPath.Width);
			_ColumnWidthChanged = false;
		}
	}

	public void RefreshColumnNames()
	{
		columnType.Text = "!";
		columnLine.Text = ResourceService.GetString("CompilerResultView.LineText");
		columnDescription.Text = ResourceService.GetString("CompilerResultView.DescriptionText");
		columnFile.Text = ResourceService.GetString("CompilerResultView.FileText");
		columnPath.Text = ResourceService.GetString("Global.Path");
	}

	protected override void OnResize(EventArgs e)
	{
		if (forceResize)
		{
			SetDefaultColumnWidths();
			forceResize = false;
		}
		base.OnResize(e);
	}

	protected override void OnColumnClick(ColumnClickEventArgs e)
	{
		SortBy(e.Column);
		base.OnColumnClick(e);
	}

	protected override void OnItemActivate(EventArgs e)
	{
		base.OnItemActivate(e);
		if (base.FocusedItem != null)
		{
			SelectedTask.JumpToPosition();
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		ListViewItem itemAt = GetItemAt(e.X, e.Y);
		if (itemAt == currentListViewItem)
		{
			return;
		}
		if (itemAt != null)
		{
			Task task = (Task)itemAt.Tag;
			string text = task.Description;
			if (text != null)
			{
				text = text.Replace("\t", "    ");
			}
			taskToolTip.SetToolTip(this, text);
			taskToolTip.Active = true;
		}
		else
		{
			taskToolTip.RemoveAll();
			taskToolTip.Active = false;
		}
		currentListViewItem = itemAt;
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 123 && base.SelectedItems.Count > 0)
		{
			long num = m.LParam.ToInt64();
			int num2 = (short)(num & 0xFFFF);
			int num3 = (short)((num & 0xFFFF0000u) >> 16);
			Point point;
			if (num2 == -1 && num3 == -1)
			{
				point = base.SelectedItems[0].Bounds.Location;
				point.X += 30;
				point.Y += 4;
			}
			else
			{
				point = PointToClient(new Point(num2, num3));
			}
			string text = ((Task)base.SelectedItems[0].Tag).ContextMenuAddInTreeEntry;
			for (int i = 1; i < base.SelectedItems.Count; i++)
			{
				string contextMenuAddInTreeEntry = ((Task)base.SelectedItems[i].Tag).ContextMenuAddInTreeEntry;
				if (contextMenuAddInTreeEntry != text)
				{
					text = "/SharpDevelop/Pads/ErrorList/TaskContextMenu";
					break;
				}
			}
			MenuService.ShowContextMenu(this, text, this, point.X, point.Y);
		}
		base.WndProc(ref m);
	}

	public void ClearTasks()
	{
		base.Items.Clear();
	}

	public void AddTask(Task task)
	{
		string fileName = task.FileName;
		string text = task.FileName;
		try
		{
			fileName = Path.GetFileName(fileName);
		}
		catch (Exception)
		{
		}
		try
		{
			text = Path.GetDirectoryName(text);
		}
		catch (Exception)
		{
		}
		ListViewItem listViewItem = new ListViewItem(new string[5]
		{
			string.Empty,
			(task.Line + 1).ToString(),
			FormatDescription(task.Description),
			fileName,
			text
		});
		int imageIndex = (listViewItem.StateImageIndex = (int)task.TaskType);
		listViewItem.ImageIndex = imageIndex;
		listViewItem.Tag = task;
		int num = 0;
		int num2 = base.Items.Count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num) / 2;
			if (mySorter.Compare(listViewItem, base.Items[num3]) > 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		base.Items.Insert(num, listViewItem);
	}

	private string FormatDescription(string description)
	{
		if (!string.IsNullOrEmpty(description))
		{
			string text = description.TrimEnd();
			text = text.Replace("\r", " ");
			text = text.Replace("\t", " ");
			return text.Replace("\n", " ");
		}
		return "";
	}

	public void RemoveTask(Task task)
	{
		for (int i = 0; i < base.Items.Count; i++)
		{
			if ((Task)base.Items[i].Tag == task)
			{
				base.Items.RemoveAt(i);
				break;
			}
		}
	}

	public void UpdateResults(IEnumerable<Task> taskSet)
	{
		BeginUpdate();
		ClearTasks();
		foreach (Task item in taskSet)
		{
			AddTask(item);
		}
		EndUpdate();
	}

	private void SortBy(TaskViewCols col)
	{
		SortBy((int)col);
	}

	private void SortBy(int col)
	{
		if (col == currentSortColumn)
		{
			if (currentSortOrder == SortOrder.Ascending)
			{
				currentSortOrder = SortOrder.Descending;
			}
			else
			{
				currentSortOrder = SortOrder.Ascending;
			}
		}
		else
		{
			currentSortColumn = col;
			currentSortOrder = SortOrder.Ascending;
		}
		mySorter = new TaskViewSorter(currentSortColumn, currentSortOrder);
		base.ListViewItemSorter = mySorter;
		base.ListViewItemSorter = null;
	}
}

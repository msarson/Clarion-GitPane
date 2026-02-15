using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class FileList : ListView
{
	public class FileListItem : ListViewItem
	{
		private string fullname;

		public string FullName
		{
			get
			{
				return fullname;
			}
			set
			{
				fullname = value;
			}
		}

		public FileListItem(string fullname)
			: base(Path.GetFileName(fullname))
		{
			this.fullname = fullname;
			base.ImageIndex = IconManager.GetIndexForFile(fullname);
		}
	}

	private FileSystemWatcher watcher;

	public FileList()
	{
		new ResourceManager("ProjectComponentResources", GetType().Module.Assembly);
		base.Columns.Add(ResourceService.GetString("CompilerResultView.FileText"), 100, HorizontalAlignment.Left);
		base.Columns.Add(ResourceService.GetString("MainWindow.Windows.FileScout.Size"), -2, HorizontalAlignment.Right);
		base.Columns.Add(ResourceService.GetString("MainWindow.Windows.FileScout.LastModified"), -2, HorizontalAlignment.Left);
		try
		{
			watcher = new FileSystemWatcher();
		}
		catch
		{
		}
		if (watcher != null)
		{
			watcher.NotifyFilter = NotifyFilters.FileName;
			watcher.EnableRaisingEvents = false;
			watcher.Renamed += fileRenamed;
			watcher.Deleted += fileDeleted;
			watcher.Created += fileCreated;
			watcher.Changed += fileChanged;
		}
		base.HideSelection = false;
		base.GridLines = true;
		base.LabelEdit = true;
		base.SmallImageList = IconManager.List;
		base.HeaderStyle = ColumnHeaderStyle.Nonclickable;
		base.View = View.Details;
		base.Alignment = ListViewAlignment.Left;
	}

	private void fileDeleted(object sender, FileSystemEventArgs e)
	{
		Action method = delegate
		{
			foreach (FileListItem item in base.Items)
			{
				if (item.FullName.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase))
				{
					base.Items.Remove(item);
					break;
				}
			}
		};
		WorkbenchSingleton.SafeThreadAsyncCall(method);
	}

	private void fileChanged(object sender, FileSystemEventArgs e)
	{
		Action method = delegate
		{
			foreach (FileListItem item in base.Items)
			{
				if (item.FullName.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase))
				{
					FileInfo fileInfo = new FileInfo(e.FullPath);
					try
					{
						item.SubItems[1].Text = Math.Round((double)fileInfo.Length / 1024.0) + " KB";
						item.SubItems[2].Text = fileInfo.LastWriteTime.ToString();
						break;
					}
					catch (IOException)
					{
						break;
					}
				}
			}
		};
		WorkbenchSingleton.SafeThreadAsyncCall(method);
	}

	private void fileCreated(object sender, FileSystemEventArgs e)
	{
		Action method = delegate
		{
			FileInfo fileInfo = new FileInfo(e.FullPath);
			ListViewItem listViewItem = base.Items.Add(new FileListItem(e.FullPath));
			try
			{
				listViewItem.SubItems.Add(Math.Round((double)fileInfo.Length / 1024.0) + " KB");
				listViewItem.SubItems.Add(fileInfo.LastWriteTime.ToString());
			}
			catch (IOException)
			{
			}
		};
		WorkbenchSingleton.SafeThreadAsyncCall(method);
	}

	private void fileRenamed(object sender, RenamedEventArgs e)
	{
		Action method = delegate
		{
			foreach (FileListItem item in base.Items)
			{
				if (item.FullName.Equals(e.OldFullPath, StringComparison.OrdinalIgnoreCase))
				{
					item.FullName = e.FullPath;
					item.Text = e.Name;
					break;
				}
			}
		};
		WorkbenchSingleton.SafeThreadAsyncCall(method);
	}

	private void renameFile(object sender, EventArgs e)
	{
		if (base.SelectedItems.Count == 1)
		{
			base.SelectedItems[0].BeginEdit();
		}
	}

	private void deleteFiles(object sender, EventArgs e)
	{
		string text = "";
		IEnumerator enumerator = base.SelectedItems.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				FileListItem fileListItem = (FileListItem)enumerator.Current;
				text = fileListItem.FullName;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		if (!MessageService.AskQuestion(StringParser.Parse("${res:ProjectComponent.ContextMenu.Delete.Question}", new string[1, 2] { { "FileName", text } }), "${Global.Delete}"))
		{
			return;
		}
		foreach (FileListItem selectedItem in base.SelectedItems)
		{
			try
			{
				File.Delete(selectedItem.FullName);
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex, "Couldn't delete file '" + Path.GetFileName(selectedItem.FullName) + "'");
				break;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		GetItemAt(PointToScreen(new Point(e.X, e.Y)).X, PointToScreen(new Point(e.X, e.Y)).Y);
		if (e.Button == MouseButtons.Right)
		{
			_ = base.SelectedItems.Count;
			_ = 0;
		}
	}

	protected override void OnAfterLabelEdit(LabelEditEventArgs e)
	{
		base.OnAfterLabelEdit(e);
		if (e.Label == null || !FileService.CheckFileName(e.Label))
		{
			e.CancelEdit = true;
			return;
		}
		string fullName = ((FileListItem)base.Items[e.Item]).FullName;
		string text = Path.Combine(Path.GetDirectoryName(fullName), e.Label);
		if (FileService.RenameFile(fullName, text, isDirectory: false))
		{
			((FileListItem)base.Items[e.Item]).FullName = text;
		}
		else
		{
			e.CancelEdit = true;
		}
	}

	public void ShowFilesInPath(string path)
	{
		base.Items.Clear();
		string[] files;
		try
		{
			if (!Directory.Exists(path))
			{
				return;
			}
			files = Directory.GetFiles(path);
		}
		catch (Exception)
		{
			return;
		}
		watcher.Path = path;
		watcher.EnableRaisingEvents = true;
		string[] array = files;
		foreach (string text in array)
		{
			FileInfo fileInfo = new FileInfo(text);
			ListViewItem listViewItem = base.Items.Add(new FileListItem(text));
			listViewItem.SubItems.Add(Math.Round((double)fileInfo.Length / 1024.0) + " KB");
			listViewItem.SubItems.Add(fileInfo.LastWriteTime.ToString());
		}
		EndUpdate();
	}
}

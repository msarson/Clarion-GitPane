using System.Collections.Generic;
using System.IO;

namespace Clarion.Core.Redirection;

public class FileWatcherInRedFile : SoftEventHandler
{
	private List<FileSystemWatcher> watchers;

	private bool reload = true;

	internal bool Reload
	{
		get
		{
			return reload;
		}
		set
		{
			reload = value;
		}
	}

	private void ReloadFile(object o, FileSystemEventArgs e)
	{
		reload = true;
	}

	public FileWatcherInRedFile()
	{
		watchers = new List<FileSystemWatcher>();
	}

	internal void AddWatcher(FileSystemWatcher watcher)
	{
		watchers.Add(watcher);
		watcher.Changed += ReloadFile;
	}

	public override void Detach()
	{
		foreach (FileSystemWatcher watcher in watchers)
		{
			watcher.Changed -= ReloadFile;
		}
	}
}

using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    /// <summary>
    /// GitPanePad - File System Watchers
    /// </summary>
    public partial class GitPanePad
    {
        #region File System Watchers

        private void StartFileWatcher(string directory)
        {
            StopFileWatcher();

            try
            {
                fileWatcher = new System.IO.FileSystemWatcher(directory);
                fileWatcher.IncludeSubdirectories = true;
                fileWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite 
                    | System.IO.NotifyFilters.FileName 
                    | System.IO.NotifyFilters.DirectoryName
                    | System.IO.NotifyFilters.Size;

                // Filter out .git folder changes to reduce noise
                fileWatcher.Changed += OnFileSystemChanged;
                fileWatcher.Created += OnFileSystemChanged;
                fileWatcher.Deleted += OnFileSystemChanged;
                fileWatcher.Renamed += OnFileSystemChanged;

                fileWatcher.EnableRaisingEvents = true;
                
                // Watch .git/config for remote changes
                string gitDir = System.IO.Path.Combine(directory, ".git");
                if (System.IO.Directory.Exists(gitDir))
                {
                    gitConfigWatcher = new System.IO.FileSystemWatcher(gitDir);
                    gitConfigWatcher.Filter = "config";
                    gitConfigWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
                    gitConfigWatcher.Changed += OnGitConfigChanged;
                    gitConfigWatcher.EnableRaisingEvents = true;
                }
            }
            catch
            {
                // Silently fail if we can't watch (maybe permissions issue)
                fileWatcher = null;
                gitConfigWatcher = null;
            }
        }

        private void StopFileWatcher()
        {
            if (fileWatcher != null)
            {
                fileWatcher.EnableRaisingEvents = false;
                fileWatcher.Dispose();
                fileWatcher = null;
            }
            
            if (gitConfigWatcher != null)
            {
                gitConfigWatcher.EnableRaisingEvents = false;
                gitConfigWatcher.Dispose();
                gitConfigWatcher = null;
            }
        }

        private void OnFileSystemChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            // Ignore .git folder changes
            if (e.FullPath.Contains("\\.git\\"))
                return;

            // Debounce: atomically replace any existing timer to avoid race between
            // concurrent FileSystemWatcher callbacks firing on ThreadPool threads.
            var old = System.Threading.Interlocked.Exchange(ref debounceTimer,
                new System.Threading.Timer(state =>
                {
                    if (contentPanel.InvokeRequired)
                    {
                        try { contentPanel.Invoke(new Action(RefreshFileList)); }
                        catch { /* control disposed */ }
                    }
                    else
                    {
                        RefreshFileList();
                    }
                }, null, 500, System.Threading.Timeout.Infinite));
            old?.Dispose();
        }

        private void OnGitConfigChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            // Debounce: atomically replace any existing timer.
            var old = System.Threading.Interlocked.Exchange(ref configDebounceTimer,
                new System.Threading.Timer(state =>
                {
                    if (contentPanel.InvokeRequired)
                    {
                        try { contentPanel.Invoke(new Action(UpdateMenuStates)); }
                        catch { /* control disposed */ }
                    }
                    else
                    {
                        UpdateMenuStates();
                    }
                }, null, 500, System.Threading.Timeout.Infinite));
            old?.Dispose();
        }

        #endregion
    }
}

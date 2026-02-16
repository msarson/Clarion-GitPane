using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    public partial class GitPanePad : AbstractPadContent
    {
        #region Fields and Properties

        private GitRepository gitRepo;
        private System.IO.FileSystemWatcher fileWatcher;
        private System.IO.FileSystemWatcher gitConfigWatcher;
        private System.Threading.Timer debounceTimer;
        private System.Threading.Timer configDebounceTimer;

        public override Control Control => contentPanel;

        #endregion

        #region Constructor and Initialization

        public GitPanePad()
        {
            InitializeUI();
            UpdateStatus();

            // Subscribe to solution events
            ProjectService.SolutionLoaded += OnSolutionChanged;
            ProjectService.SolutionClosed += OnSolutionClosed;
        }

        #endregion

        #region Main Status and Updates

        private void UpdateStatus()
        {
            if (ProjectService.OpenSolution != null)
            {
                string solutionDir = ProjectService.OpenSolution.Directory;
                gitRepo = new GitRepository(solutionDir);

                if (gitRepo.IsRepository())
                {
                    string repoName = gitRepo.GetRepositoryName();
                    string currentBranch = gitRepo.GetCurrentBranch();

                    // Update title to include repo name and path
                    UpdatePadTitle($"{repoName} - {solutionDir}");

                    branchValueLabel.Text = currentBranch ?? "unknown";

                    // FIX: Hide non-repo controls, show repo UI
                    statusLabel.Visible = false;
                    initRepoButton.Visible = false;
                    toolStrip.Visible = true;
                    mainSplitter.Visible = true;
                    commitPanel.Visible = true;
                    
                    branchValueLabel.Visible = true;
                    branchSelectButton.Visible = true;
                    refreshButton.Visible = true;
                    
                    // Check remote status
                    UpdateRemoteStatus();
                    
                    // Show commit workflow controls
                    stagedGroupBox.Visible = true;
                    unstagedGroupBox.Visible = true;
                    commitMessageLabel.Visible = true;
                    commitMessageBox.Visible = true;
                    commitButton.Visible = true;
                    commitPushButton.Visible = true;
                    
                    // FIX: Set splitter distance after controls are visible
                    if (mainSplitter.Height > 0)
                    {
                        mainSplitter.SplitterDistance = mainSplitter.Height / 2;
                    }
                    
                    RefreshFileList();
                    StartFileWatcher(solutionDir);
                }
                else
                {
                    UpdatePadTitle($"Not a Git repository - {solutionDir}");
                    HideCommitControls();
                    StopFileWatcher();
                    statusLabel.Text = "Not a Git repository";
                    statusLabel.Visible = true;
                    initRepoButton.Visible = true; // Show init button
                }
            }
            else
            {
                UpdatePadTitle("Git - No solution opened");
                HideCommitControls();
                StopFileWatcher();
                statusLabel.Text = "No solution opened";
                statusLabel.Visible = true;
                initRepoButton.Visible = false; // Hide init button - no solution
            }
        }

        private void HideCommitControls()
        {
            branchValueLabel.Visible = false;
            branchSelectButton.Visible = false;
            remoteLabel.Visible = false;
            removeRemoteButton.Visible = false;
            addRemoteButton.Visible = false;
            createGitHubRepoButton.Visible = false;
            initRepoButton.Visible = false;
            refreshButton.Visible = false;
            stagedGroupBox.Visible = false;
            unstagedGroupBox.Visible = false;
            commitMessageLabel.Visible = false;
            commitMessageBox.Visible = false;
            commitButton.Visible = false;
            commitPushButton.Visible = false;
            pushButton.Visible = false;
        }

        private void RefreshFileList()
        {
            if (gitRepo == null)
                return;

            // Check for unpushed commits and show/hide push button
            UpdatePushButtonVisibility();

            // Get staged files
            stagedListBox.Items.Clear();
            var stagedFiles = gitRepo.GetStagedFiles();
            foreach (var file in stagedFiles)
            {
                stagedListBox.Items.Add(file, false);
            }
            stagedGroupBox.Text = $"Staged Files ({stagedFiles.Length})";

            // Get unstaged files
            unstagedListBox.Items.Clear();
            var unstagedFiles = gitRepo.GetUnstagedFiles();
            var untrackedFiles = gitRepo.GetUntrackedFiles();
            
            foreach (var file in unstagedFiles)
            {
                unstagedListBox.Items.Add(file, false);
            }
            
            // Add untracked files with U prefix
            foreach (var file in untrackedFiles)
            {
                unstagedListBox.Items.Add($"U\t{file}", false);
            }
            
            unstagedGroupBox.Text = $"Unstaged Files ({unstagedFiles.Length + untrackedFiles.Length})";
        }

        private void UpdatePushButtonVisibility()
        {
            if (gitRepo == null || !gitRepo.HasRemote())
            {
                pushButton.Visible = false;
                return;
            }

            int unpushedCount = gitRepo.GetUnpushedCommitsCount();
            if (unpushedCount > 0)
            {
                pushButton.Text = $"Push ({unpushedCount})";
                pushButton.Visible = true;
            }
            else
            {
                pushButton.Visible = false;
            }
        }

        #endregion

        #region Event Handlers - Basic Actions

        private void OnRefreshClick(object sender, EventArgs e)
        {
            RefreshFileList();
            UpdateRemoteStatus(); // Check if remote still exists
        }

        private void OnStageSelectedClick(object sender, EventArgs e)
        {
            if (gitRepo == null || unstagedListBox.CheckedItems.Count == 0)
                return;

            foreach (var item in unstagedListBox.CheckedItems)
            {
                var parts = item.ToString().Split('\t');
                var filePath = parts.Length > 1 ? parts[1] : item.ToString();
                gitRepo.StageFile(filePath);
            }

            RefreshFileList();
        }

        private void OnUnstageSelectedClick(object sender, EventArgs e)
        {
            if (gitRepo == null || stagedListBox.CheckedItems.Count == 0)
                return;

            foreach (var item in stagedListBox.CheckedItems)
            {
                var parts = item.ToString().Split('\t');
                var filePath = parts.Length > 1 ? parts[1] : item.ToString();
                gitRepo.UnstageFile(filePath);
            }

            RefreshFileList();
        }

        private void OnStageAllClick(object sender, EventArgs e)
        {
            if (gitRepo != null && gitRepo.StageAllFiles())
            {
                RefreshFileList();
            }
            else
            {
                MessageBox.Show("Failed to stage all files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnUnstageAllClick(object sender, EventArgs e)
        {
            if (gitRepo != null && gitRepo.UnstageAllFiles())
            {
                RefreshFileList();
            }
            else
            {
                MessageBox.Show("Failed to unstage all files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnUnstagedListBoxMouseDown(object sender, MouseEventArgs e)
        {
            // Select item on right-click before showing context menu
            if (e.Button == MouseButtons.Right)
            {
                int index = unstagedListBox.IndexFromPoint(e.Location);
                if (index >= 0 && index < unstagedListBox.Items.Count)
                {
                    unstagedListBox.SelectedIndex = index;
                }
            }
        }

        private void OnUnstagedContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Update menu items text based on selected file
            if (unstagedListBox.SelectedItem == null)
            {
                e.Cancel = true; // Don't show menu if nothing selected
                return;
            }

            var item = unstagedListBox.SelectedItem.ToString();
            var parts = item.Split('\t');
            var filePath = parts.Length > 1 ? parts[1] : item;

            // Update extension menu item text
            string extension = System.IO.Path.GetExtension(filePath);
            if (!string.IsNullOrEmpty(extension))
            {
                ((ToolStripMenuItem)unstagedContextMenu.Items[1]).Text = $"Ignore file type (*{extension})";
            }
            else
            {
                ((ToolStripMenuItem)unstagedContextMenu.Items[1]).Text = "Ignore file type (no extension)";
                ((ToolStripMenuItem)unstagedContextMenu.Items[1]).Enabled = false;
            }

            // Update directory menu item
            string directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                string displayDir = directory.Length > 20 ? "..." + directory.Substring(directory.Length - 20) : directory;
                ((ToolStripMenuItem)unstagedContextMenu.Items[2]).Text = $"Ignore directory ({displayDir}/)";
                ((ToolStripMenuItem)unstagedContextMenu.Items[2]).Enabled = true;
            }
            else
            {
                ((ToolStripMenuItem)unstagedContextMenu.Items[2]).Text = "Ignore directory (root)";
                ((ToolStripMenuItem)unstagedContextMenu.Items[2]).Enabled = false;
            }
        }

        #endregion

        #region Event Handlers - Context Menu (.gitignore)

        private void OnIgnoreFileClick(object sender, EventArgs e)
        {
            if (unstagedListBox.SelectedItem == null)
                return;

            var item = unstagedListBox.SelectedItem.ToString();
            var parts = item.Split('\t');
            var filePath = parts.Length > 1 ? parts[1] : item;

            if (gitRepo.AddToGitignore(filePath))
            {
                MessageBox.Show($"Added '{filePath}' to .gitignore", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshFileList();
            }
            else
            {
                MessageBox.Show("Failed to update .gitignore", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnIgnoreExtensionClick(object sender, EventArgs e)
        {
            if (unstagedListBox.SelectedItem == null)
                return;

            var item = unstagedListBox.SelectedItem.ToString();
            var parts = item.Split('\t');
            var filePath = parts.Length > 1 ? parts[1] : item;

            string extension = System.IO.Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension))
            {
                MessageBox.Show("File has no extension to ignore.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string pattern = $"*{extension}";
            if (gitRepo.AddToGitignore(pattern))
            {
                MessageBox.Show($"Added '{pattern}' to .gitignore", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshFileList();
            }
            else
            {
                MessageBox.Show("Failed to update .gitignore", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnIgnoreDirectoryClick(object sender, EventArgs e)
        {
            if (unstagedListBox.SelectedItem == null)
                return;

            var item = unstagedListBox.SelectedItem.ToString();
            var parts = item.Split('\t');
            var filePath = parts.Length > 1 ? parts[1] : item;

            string directory = System.IO.Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
            {
                MessageBox.Show("File is in root directory.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Normalize path separators to forward slashes for .gitignore
            string pattern = directory.Replace('\\', '/') + "/";
            if (gitRepo.AddToGitignore(pattern))
            {
                MessageBox.Show($"Added '{pattern}' to .gitignore", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshFileList();
            }
            else
            {
                MessageBox.Show("Failed to update .gitignore", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region UI Layout and Resize

        private void UpdatePadTitle(string title)
        {
            // Try to update the pad title through the parent form
            var parent = contentPanel.Parent;
            while (parent != null)
            {
                if (parent is Form)
                {
                    ((Form)parent).Text = title;
                    break;
                }
                parent = parent.Parent;
            }
        }

        #endregion



        #region Solution Event Handlers

        private void OnSolutionChanged(object sender, SolutionEventArgs e)
        {
            UpdateStatus();
        }

        private void OnSolutionClosed(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        public override void Dispose()
        {
            ProjectService.SolutionLoaded -= OnSolutionChanged;
            ProjectService.SolutionClosed -= OnSolutionClosed;
            StopFileWatcher();
            debounceTimer?.Dispose();
            contentPanel?.Dispose();
            base.Dispose();
        }

        #endregion
    }
}

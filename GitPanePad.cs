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
                    historyButton.Visible = true;
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
            // Hide main UI containers
            toolStrip.Visible = false;
            mainSplitter.Visible = false;
            
            // Hide individual controls
            branchValueLabel.Visible = false;
            branchSelectButton.Visible = false;
            historyButton.Visible = false;
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
            
            // Update button states after refreshing lists
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                // No repo - disable all git operation buttons
                stageSelectedButton.Enabled = false;
                stageAllButton.Enabled = false;
                discardSelectedButton.Enabled = false;
                discardAllButton.Enabled = false;
                unstageSelectedButton.Enabled = false;
                unstageAllButton.Enabled = false;
                commitButton.Enabled = false;
                commitPushButton.Enabled = false;
                pushButton.Enabled = false;
                branchSelectButton.Enabled = false;
                historyButton.Enabled = false;
                refreshButton.Enabled = false;
                return;
            }
            
            // Enable refresh, history, and branch buttons when repo exists
            refreshButton.Enabled = true;
            historyButton.Enabled = true;
            branchSelectButton.Enabled = true;
            
            // Stage buttons - enabled when unstaged files exist or are checked
            int unstagedCount = unstagedListBox.Items.Count;
            int unstagedCheckedCount = unstagedListBox.CheckedItems.Count;
            stageSelectedButton.Enabled = unstagedCheckedCount > 0;
            stageAllButton.Enabled = unstagedCount > 0;
            
            // Discard buttons - same logic as stage buttons (work on unstaged files)
            discardSelectedButton.Enabled = unstagedCheckedCount > 0;
            discardAllButton.Enabled = unstagedCount > 0;
            
            // Unstage buttons - enabled when staged files exist or are checked
            int stagedCount = stagedListBox.Items.Count;
            int stagedCheckedCount = stagedListBox.CheckedItems.Count;
            unstageSelectedButton.Enabled = stagedCheckedCount > 0;
            unstageAllButton.Enabled = stagedCount > 0;
            
            // Commit message box - only enabled when there are staged files
            commitMessageBox.Enabled = stagedCount > 0;
            
            // Commit buttons - enabled when staged files exist AND commit message is not empty
            bool hasCommitMessage = !string.IsNullOrWhiteSpace(commitMessageBox.Text);
            bool canCommit = stagedCount > 0 && hasCommitMessage;
            commitButton.Enabled = canCommit;
            
            // Commit & Push - same as commit, plus requires remote
            bool hasRemote = gitRepo.HasRemote();
            commitPushButton.Enabled = canCommit && hasRemote;
            
            // Push button visibility/enabled already handled by UpdatePushButtonVisibility()
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

        private void OnHistoryClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
                return;
            
            using (var dialog = new GitHistoryDialog(gitRepo))
            {
                dialog.ShowDialog(this.Control.FindForm());
            }
        }

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

        private void OnDiscardSelectedClick(object sender, EventArgs e)
        {
            if (gitRepo == null || unstagedListBox.CheckedItems.Count == 0)
                return;

            var fileCount = unstagedListBox.CheckedItems.Count;
            var result = MessageBox.Show(
                $"Are you sure you want to discard changes to {fileCount} file(s)?\n\nThis action cannot be undone.",
                "Confirm Discard",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            foreach (var item in unstagedListBox.CheckedItems)
            {
                var parts = item.ToString().Split('\t');
                var filePath = parts.Length > 1 ? parts[1] : item.ToString();
                gitRepo.DiscardFile(filePath);
            }

            RefreshFileList();
        }

        private void OnDiscardAllClick(object sender, EventArgs e)
        {
            if (gitRepo == null)
                return;

            var result = MessageBox.Show(
                "Are you sure you want to discard ALL unstaged changes?\n\nThis will:\n- Discard all changes to tracked files\n- Remove all untracked files\n\nThis action cannot be undone.",
                "Confirm Discard All",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            if (gitRepo.DiscardAllFiles())
            {
                RefreshFileList();
            }
            else
            {
                MessageBox.Show("Failed to discard all changes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnStagedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // ItemCheck fires before the check state is updated, so we need to delay the update
            contentPanel.BeginInvoke(new Action(() => UpdateButtonStates()));
        }

        private void OnUnstagedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // ItemCheck fires before the check state is updated, so we need to delay the update
            contentPanel.BeginInvoke(new Action(() => UpdateButtonStates()));
        }

        private void OnCommitMessageBox_TextChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
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

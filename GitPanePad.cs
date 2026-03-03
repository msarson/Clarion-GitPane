using System;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    public partial class GitPanePad : AbstractPadContent
    {
        #region Fields and Properties

        private GitRepository gitRepo;
        private TemplateManager templateManager;
        private System.IO.FileSystemWatcher fileWatcher;
        private System.IO.FileSystemWatcher gitConfigWatcher;
        private System.Threading.Timer debounceTimer;
        private System.Threading.Timer configDebounceTimer;
        private System.Windows.Forms.Timer _syncTimer;

        public override Control Control => contentPanel;

        #endregion

        #region Constructor and Initialization

        public GitPanePad()
        {
            using (var stream = Assembly.GetExecutingAssembly()
                       .GetManifestResourceStream("GitPane.Resources.GitPaneIcon.png"))
            {
                if (stream != null)
                    ResourceService.RegisterNeutralImages(
                        new EmbeddedIconManager("GitPane.GitPaneIcon", new Bitmap(stream)));
            }

            // Check Git availability first
            if (!GitRepository.IsGitAvailable())
            {
                ShowGitNotInstalledMessage();
            }
            
            // Initialize template manager
            InitializeTemplateManager();
            
            InitializeUI();
            UpdateStatus();

            // Subscribe to solution events
            ProjectService.SolutionLoaded += OnSolutionChanged;
            ProjectService.SolutionClosed += OnSolutionClosed;
        }

        private void InitializeTemplateManager()
        {
            try
            {
                // Get the add-in directory path
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string addInPath = System.IO.Path.GetDirectoryName(assemblyPath);
                
                templateManager = new TemplateManager(addInPath);
                templateManager.EnsureDefaultTemplates();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to initialize template manager: " + ex.Message,
                    "Template System Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ShowGitNotInstalledMessage()
        {
            var result = MessageBox.Show(
                "Git is not installed or not found in your system PATH.\n\n" +
                "GitPane requires Git to be installed to function.\n\n" +
                "Would you like to open the Git download page?",
                "Git Not Found",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start("https://git-scm.com/downloads");
                }
                catch
                {
                    MessageBox.Show("Please visit https://git-scm.com/downloads to download Git.",
                        "Git Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        #region Main Status and Updates

        private void UpdateStatus()
        {
            // Check if Git is available first
            if (!GitRepository.IsGitAvailable())
            {
                statusLabel.Text = "Git not installed - Please install Git to use this pane";
                statusLabel.ForeColor = Color.Red;
                HideCommitControls();
                UpdateMenuStates();
                return;
            }
            
            if (ProjectService.OpenSolution != null)
            {
                string solutionDir = ProjectService.OpenSolution.Directory;
                gitRepo = new GitRepository(solutionDir);

                if (gitRepo.IsRepository())
                {
                    string repoName = gitRepo.GetRepositoryName();
                    string currentBranch = gitRepo.GetCurrentBranch();

                    // Update title: show branch immediately with "checking…" then update async
                    UpdatePadTitle(BuildPaneTitle(repoName, currentBranch, null));
                    TriggerSyncCheck(gitRepo, repoName, currentBranch);
                    StartSyncTimer(gitRepo, repoName, currentBranch);

                    // FIX: Hide non-repo controls, show repo UI
                    statusLabel.Visible = false;
                    initRepoButton.Visible = false;
                    toolStrip.Visible = true;
                    mainSplitter.Visible = true;
                    commitPanel.Visible = true;
                    
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
                    UpdateMenuStates(); // Update menu items based on repo state
                }
                else
                {
                    UpdatePadTitle($"Not a Git repository - {solutionDir}");
                    HideCommitControls();
                    StopFileWatcher();
                    StopSyncTimer();
                    statusLabel.Text = "Not a Git repository";
                    statusLabel.Visible = true;
                    initRepoButton.Visible = false; // Hide button - use menu instead
                    toolStrip.Visible = true; // Show menu bar for Initialize and Templates access
                    UpdateMenuStates(); // Update menu items for non-repo state
                }
            }
            else
            {
                UpdatePadTitle("Git - No solution opened");
                HideCommitControls();
                StopFileWatcher();
                StopSyncTimer();
                statusLabel.Text = "No solution opened";
                statusLabel.Visible = true;
                initRepoButton.Visible = false; // Hide init button - no solution
                toolStrip.Visible = true; // Show menu bar for Templates access
                UpdateMenuStates(); // Update menu items for no solution state
            }
        }

        private void HideCommitControls()
        {
            // Hide main UI containers
            toolStrip.Visible = false;
            mainSplitter.Visible = false;
            
            // Hide individual controls
            initRepoButton.Visible = false;
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

            // Refresh stash panel
            RefreshStashPanel();
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
                return;
            }

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

        private void UpdateMenuStates()
        {
            bool gitAvailable = GitRepository.IsGitAvailable();
            bool hasRepo = gitAvailable && gitRepo != null && gitRepo.IsRepository();
            bool hasSolution = ProjectService.OpenSolution != null;
            bool hasRemote = hasRepo && gitRepo.HasRemote();
            bool hasGitHubCLI = GitRepository.IsGitHubCLIAvailable();
            
            // Disable everything if Git is not available
            if (!gitAvailable)
            {
                initRepoMenuItem.Visible = false;
                fetchMenuItem.Enabled = false;
                pullMenuItem.Enabled = false;
                pushMenuItem.Enabled = false;
                viewOnRemoteMenuItem.Enabled = false;
                createGitHubMenuItem.Enabled = false;
                addRemoteMenuItem.Enabled = false;
                removeRemoteMenuItem.Enabled = false;
                switchBranchMenuItem.Enabled = false;
                createBranchMenuItem.Enabled = false;
                deleteBranchMenuItem.Enabled = false;
                mergeBranchMenuItem.Enabled = false;
                return;
            }
            
            // Show/hide entire menus based on repository state
            // If no repo: show top-level Initialize button and Help menu only
            // If repo: show all normal menus
            initRepoMenuButton.Visible = hasSolution && !hasRepo;
            fileMenu.Visible = hasRepo;
            repoMenu.Visible = hasRepo;
            branchMenu.Visible = hasRepo;
            viewMenu.Visible = hasRepo;
            helpMenu.Visible = true;

            // File menu items (when visible)
            initRepoMenuItem.Visible = false; // Not needed - using top-level button now
            openExternalMenuItem.Visible = hasRepo;
            
            // Show open file items only if files exist
            if (hasRepo && hasSolution)
            {
                string solutionDir = ProjectService.OpenSolution.Directory;
                openGitignoreMenuItem.Visible = System.IO.File.Exists(System.IO.Path.Combine(solutionDir, ".gitignore"));
                openGitattributesMenuItem.Visible = System.IO.File.Exists(System.IO.Path.Combine(solutionDir, ".gitattributes"));
            }
            else
            {
                openGitignoreMenuItem.Visible = false;
                openGitattributesMenuItem.Visible = false;
            }
            
            // Repository menu items
            applyTemplateMenuItem.Enabled = hasRepo;
            fetchMenuItem.Enabled = hasRemote;
            pullMenuItem.Enabled = hasRemote;
            pushMenuItem.Enabled = hasRemote;
            viewOnRemoteMenuItem.Enabled = hasRemote;
            createGitHubMenuItem.Enabled = hasRepo && !hasRemote && hasGitHubCLI; // Requires gh CLI
            addRemoteMenuItem.Enabled = hasRepo && !hasRemote;
            removeRemoteMenuItem.Enabled = hasRemote;
            
            // Branch menu items
            switchBranchMenuItem.Enabled = hasRepo;
            createBranchMenuItem.Enabled = hasRepo;
            deleteBranchMenuItem.Enabled = hasRepo;
            mergeBranchMenuItem.Enabled = hasRepo;
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

        private void RefreshStashPanel()
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                stashGroupBox.Visible = false;
                return;
            }

            var entries = gitRepo.GetStashEntries();
            stashListView.Items.Clear();

            if (entries.Length == 0)
            {
                stashGroupBox.Visible = false;
                return;
            }

            foreach (var s in entries)
            {
                var item = new ListViewItem(s.Ref);
                item.SubItems.Add(s.Message);
                item.SubItems.Add(s.Relative);
                item.Tag = s.Index;
                stashListView.Items.Add(item);
            }

            stashGroupBox.Text    = $"Stashes ({entries.Length})";
            stashGroupBox.Visible = true;
            UpdateStashButtons();
        }

        private void OnStashSelectionChanged(object sender, EventArgs e)
        {
            UpdateStashButtons();
        }

        private void UpdateStashButtons()
        {
            bool selected = stashListView.SelectedItems.Count > 0;
            applyStashButton.Enabled = selected;
            popStashButton.Enabled   = selected;
            dropStashButton.Enabled  = selected;
        }

        private void OnNewStashClick(object sender, EventArgs e)
        {
            if (gitRepo == null) return;
            string msg = PromptSingleLine("New Stash", "Stash message (optional):");
            if (msg == null) return; // cancelled
            if (!gitRepo.StashChanges(msg))
                MessageBox.Show("Failed to create stash.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            RefreshFileList();
        }

        private void OnApplyStashClick(object sender, EventArgs e)
        {
            if (gitRepo == null || stashListView.SelectedItems.Count == 0) return;
            int idx = (int)stashListView.SelectedItems[0].Tag;
            if (!gitRepo.ApplyStash(idx))
                MessageBox.Show("Failed to apply stash. You may have conflicts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            RefreshFileList();
        }

        private void OnPopStashClick(object sender, EventArgs e)
        {
            if (gitRepo == null || stashListView.SelectedItems.Count == 0) return;
            int idx = (int)stashListView.SelectedItems[0].Tag;
            if (!gitRepo.PopStash(idx))
                MessageBox.Show("Failed to pop stash. You may have conflicts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            RefreshFileList();
        }

        private void OnDropStashClick(object sender, EventArgs e)
        {
            if (gitRepo == null || stashListView.SelectedItems.Count == 0) return;
            int idx = (int)stashListView.SelectedItems[0].Tag;
            string stashRef = stashListView.SelectedItems[0].Text;
            if (MessageBox.Show($"Permanently drop '{stashRef}'?", "Drop Stash",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                != DialogResult.Yes) return;
            if (!gitRepo.DropStash(idx))
                MessageBox.Show("Failed to drop stash.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            RefreshFileList();
        }

        /// <summary>Shows a small single-line input prompt. Returns null if user cancelled.</summary>
        private string PromptSingleLine(string title, string labelText)
        {
            using (var dlg = new Form())
            {
                dlg.Text            = title;
                dlg.Size            = new Size(400, 130);
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;

                var lbl = new Label { Text = labelText, Location = new Point(10, 12), AutoSize = true };
                var tb  = new TextBox { Location = new Point(10, 32), Width = 360 };
                var ok  = new Button { Text = "OK",     Location = new Point(195, 62), Width = 80,
                                       DialogResult = DialogResult.OK };
                var cn  = new Button { Text = "Cancel", Location = new Point(285, 62), Width = 80,
                                       DialogResult = DialogResult.Cancel };
                dlg.Controls.AddRange(new Control[] { lbl, tb, ok, cn });
                dlg.AcceptButton = ok;
                dlg.CancelButton = cn;

                return dlg.ShowDialog() == DialogResult.OK ? tb.Text : null;
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
            if (gitRepo == null)
                return;

            // Disable button to prevent double-clicks
            stageAllButton.Enabled = false;
            statusLabel.Text = "Staging all files...";
            statusLabel.Visible = true;

            // Run Stage All in background thread
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                var success = gitRepo.StageAllFiles();
                
                // Marshal back to UI thread
                if (Control.InvokeRequired)
                {
                    Control.Invoke(new System.Action(delegate
                    {
                        statusLabel.Visible = false;
                        stageAllButton.Enabled = true;
                        
                        if (success)
                        {
                            RefreshFileList();
                        }
                        else
                        {
                            MessageBox.Show("Failed to stage all files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                }
            });
        }

        private void OnUnstageAllClick(object sender, EventArgs e)
        {
            if (gitRepo == null)
                return;

            // Disable button to prevent double-clicks
            unstageAllButton.Enabled = false;
            statusLabel.Text = "Unstaging all files...";
            statusLabel.Visible = true;

            // Run Unstage All in background thread
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                var success = gitRepo.UnstageAllFiles();
                
                // Marshal back to UI thread
                if (Control.InvokeRequired)
                {
                    Control.Invoke(new System.Action(delegate
                    {
                        statusLabel.Visible = false;
                        unstageAllButton.Enabled = true;
                        
                        if (success)
                        {
                            RefreshFileList();
                        }
                        else
                        {
                            MessageBox.Show("Failed to unstage all files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                }
            });
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

            // Disable button to prevent double-clicks
            discardAllButton.Enabled = false;
            statusLabel.Text = "Discarding all changes...";
            statusLabel.Visible = true;

            // Run Discard All in background thread
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                var success = gitRepo.DiscardAllFiles();
                
                // Marshal back to UI thread
                if (Control.InvokeRequired)
                {
                    Control.Invoke(new System.Action(delegate
                    {
                        statusLabel.Visible = false;
                        discardAllButton.Enabled = true;
                        
                        if (success)
                        {
                            RefreshFileList();
                        }
                        else
                        {
                            MessageBox.Show("Failed to discard all changes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                }
            });
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
                ((ToolStripMenuItem)unstagedContextMenu.Items[1]).Enabled = true;
            }
            else
            {
                ((ToolStripMenuItem)unstagedContextMenu.Items[1]).Text = "Ignore file type (no extension)";
                ((ToolStripMenuItem)unstagedContextMenu.Items[1]).Enabled = false;
            }

            // Build directory hierarchy submenu
            var directoryMenuItem = (ToolStripMenuItem)unstagedContextMenu.Items[2];
            directoryMenuItem.DropDownItems.Clear();
            
            string directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                directoryMenuItem.Enabled = true;
                
                // Build path hierarchy from deepest to root
                var pathParts = directory.Replace("\\", "/").Split('/');
                
                for (int i = pathParts.Length; i > 0; i--)
                {
                    string partialPath = string.Join("/", pathParts.Take(i));
                    string displayPath = pathParts[i - 1]; // Show just the folder name
                    string fullPath = partialPath + "/";
                    
                    // Show full relative path in tooltip
                    var menuItem = new ToolStripMenuItem($"{displayPath}/");
                    menuItem.Tag = fullPath;
                    menuItem.ToolTipText = fullPath;
                    menuItem.Click += OnIgnoreDirectoryClick;
                    
                    directoryMenuItem.DropDownItems.Add(menuItem);
                }
            }
            else
            {
                directoryMenuItem.Enabled = false;
                directoryMenuItem.DropDownItems.Add(new ToolStripMenuItem("(file is in root directory)") { Enabled = false });
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
            // Get the directory path from the menu item's Tag
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null || menuItem.Tag == null)
                return;
            
            string pattern = menuItem.Tag.ToString();
            
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

        #region Event Handlers - Menu Items

        // File menu
        private void OnOpenGitHubDesktopClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string repoPath = gitRepo.GetWorkingDirectory();
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "github",
                    Arguments = repoPath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open GitHub Desktop.\n\nMake sure GitHub Desktop is installed.\n\nError: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnOpenGitKrakenClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string repoPath = gitRepo.GetWorkingDirectory();
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "gitkraken",
                    Arguments = $"--path \"{GitRepository.EscapeGitArg(repoPath)}\"",
                    UseShellExecute = false
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open GitKraken.\n\nMake sure GitKraken is installed.\n\nError: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnClosePaneClick(object sender, EventArgs e)
        {
            // Close the pad by closing its parent form
            var parentForm = this.Control.FindForm();
            if (parentForm != null)
            {
                parentForm.Close();
            }
        }

        private void OnOpenGitignoreClick(object sender, EventArgs e)
        {
            if (ProjectService.OpenSolution == null || gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string gitignorePath = System.IO.Path.Combine(ProjectService.OpenSolution.Directory, ".gitignore");
            if (!System.IO.File.Exists(gitignorePath))
            {
                MessageBox.Show(".gitignore file does not exist in the repository.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                FileService.OpenFile(gitignorePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open .gitignore:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnOpenGitattributesClick(object sender, EventArgs e)
        {
            if (ProjectService.OpenSolution == null || gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string gitattributesPath = System.IO.Path.Combine(ProjectService.OpenSolution.Directory, ".gitattributes");
            if (!System.IO.File.Exists(gitattributesPath))
            {
                MessageBox.Show(".gitattributes file does not exist in the repository.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                FileService.OpenFile(gitattributesPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open .gitattributes:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Repository menu
        private void OnFetchClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!gitRepo.HasRemote())
            {
                MessageBox.Show("No remote configured for this repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = gitRepo.Fetch();
            if (result.ExitCode == 0)
            {
                MessageBox.Show("Fetch completed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshFileList();
            }
            else
            {
                // Check for authentication errors
                var errorText = result.Error + "\n" + result.Output;
                if (errorText.Contains("Authentication failed") || 
                    errorText.Contains("Invalid username or token") ||
                    errorText.Contains("Password authentication is not supported"))
                {
                    MessageBox.Show(
                        "Fetch failed: Git could not authenticate with the remote server.\n\n" +
                        "Why this happens:\n" +
                        "Other Git tools may have credentials stored and supply them automatically. " +
                        "GitPane uses Git directly and cannot prompt for credentials.\n\n" +
                        "Solutions for GitHub:\n" +
                        "• Run 'gh auth login' once (easiest)\n" +
                        "• Use SSH instead of HTTPS\n" +
                        "• Use a Personal Access Token (PAT) as your password\n" +
                        "• Install Git Credential Manager\n\n" +
                        $"Git error:\n{errorText}",
                        "Authentication Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Fetch failed.\n\nError: {result.Error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnPullClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!gitRepo.HasRemote())
            {
                MessageBox.Show("No remote configured for this repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gitRepo.HasUncommittedChanges())
            {
                var result = MessageBox.Show(
                    "You have uncommitted changes. Pull may fail or cause conflicts.\n\nDo you want to continue?",
                    "Uncommitted Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result != DialogResult.Yes)
                    return;
            }

            var pullResult = gitRepo.Pull();
            if (pullResult.ExitCode == 0)
            {
                MessageBox.Show($"Pull completed successfully.\n\n{pullResult.Output}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshFileList();
            }
            else
            {
                // Check for authentication errors
                var errorText = pullResult.Error + "\n" + pullResult.Output;
                if (errorText.Contains("Authentication failed") || 
                    errorText.Contains("Invalid username or token") ||
                    errorText.Contains("Password authentication is not supported"))
                {
                    MessageBox.Show(
                        "Pull failed: Git could not authenticate with the remote server.\n\n" +
                        "Why this happens:\n" +
                        "Other Git tools may have credentials stored and supply them automatically. " +
                        "GitPane uses Git directly and cannot prompt for credentials.\n\n" +
                        "Solutions for GitHub:\n" +
                        "• Run 'gh auth login' once (easiest - GitHub CLI will handle credentials)\n" +
                        "• Use SSH instead of HTTPS\n" +
                        "• Use a Personal Access Token (PAT) as your password\n" +
                        "• Install Git Credential Manager to store credentials\n\n" +
                        $"Git error:\n{errorText}",
                        "Authentication Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Pull failed.\n\nError: {pullResult.Error}\n\n{pullResult.Output}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnViewOnRemoteClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!gitRepo.HasRemote())
            {
                MessageBox.Show("No remote configured for this repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string remoteUrl = gitRepo.GetRemoteUrl();
            if (string.IsNullOrEmpty(remoteUrl))
            {
                MessageBox.Show("Could not retrieve remote URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Convert SSH URLs to HTTPS for browser
            string browserUrl = remoteUrl;
            if (browserUrl.StartsWith("git@"))
            {
                // Convert git@github.com:user/repo.git to https://github.com/user/repo
                browserUrl = browserUrl.Replace("git@", "https://");
                browserUrl = browserUrl.Replace(".com:", ".com/");
            }
            
            // Remove .git suffix
            if (browserUrl.EndsWith(".git"))
            {
                browserUrl = browserUrl.Substring(0, browserUrl.Length - 4);
            }

            try
            {
                // Only allow http/https to prevent file://, javascript: etc. from executing
                if (!browserUrl.StartsWith("https://") && !browserUrl.StartsWith("http://"))
                {
                    MessageBox.Show("The remote URL could not be converted to a safe browser URL.\n\nOnly HTTPS and HTTP URLs can be opened.",
                        "Cannot Open URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                System.Diagnostics.Process.Start(browserUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser.\n\nURL: {browserUrl}\n\nError: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Branch menu
        private void OnCreateBranchClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Simple input dialog for branch name
            var branchName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter new branch name:",
                "Create Branch",
                "");

            if (string.IsNullOrWhiteSpace(branchName))
                return;

            // Validate branch name - allow only git-safe characters
            if (!System.Text.RegularExpressions.Regex.IsMatch(branchName, @"^[a-zA-Z0-9/_\-\.]+$")
                || branchName.Contains("..") || branchName.StartsWith(".") || branchName.EndsWith("/"))
            {
                MessageBox.Show(
                    "Invalid branch name. Use only letters, numbers, hyphens, underscores, dots and forward slashes.\n" +
                    "Branch names cannot contain spaces, quotes, or other special characters.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = gitRepo.CreateBranch(branchName, checkout: true);
            if (result.ExitCode == 0)
            {
                MessageBox.Show($"Branch '{branchName}' created and checked out.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus();
            }
            else
            {
                MessageBox.Show($"Failed to create branch.\n\nError: {result.Error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDeleteBranchClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var branches = gitRepo.GetAllBranchesWithInfo();
            var currentBranch = gitRepo.GetCurrentBranch();

            // Filter out current branch and remote branches
            var deletableBranches = new System.Collections.Generic.List<string>();
            foreach (var branch in branches)
            {
                if (!branch.IsCurrent && !branch.IsRemote)
                {
                    deletableBranches.Add(branch.Name);
                }
            }

            if (deletableBranches.Count == 0)
            {
                MessageBox.Show("No local branches available to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create BranchInfo array for dialog (only deletable branches)
            var deletableBranchInfos = new System.Collections.Generic.List<BranchInfo>();
            foreach (var branch in branches)
            {
                if (!branch.IsCurrent && !branch.IsRemote)
                {
                    deletableBranchInfos.Add(branch);
                }
            }

            // Show branch selector dialog
            using (var dialog = new BranchSelectorDialog(deletableBranchInfos.ToArray(), currentBranch))
            {
                dialog.Text = "Delete Branch";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var branchToDelete = dialog.SelectedBranch;
                    
                    var confirmResult = MessageBox.Show(
                        $"Are you sure you want to delete branch '{branchToDelete}'?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        var result = gitRepo.DeleteBranch(branchToDelete, force: false);
                        if (result.ExitCode == 0)
                        {
                            MessageBox.Show($"Branch '{branchToDelete}' deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            UpdateStatus();
                        }
                        else
                        {
                            // Try force delete if normal delete failed
                            var forceResult = MessageBox.Show(
                                $"Failed to delete branch.\n\n{result.Error}\n\nDo you want to force delete?",
                                "Delete Failed",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);

                            if (forceResult == DialogResult.Yes)
                            {
                                var forceDeleteResult = gitRepo.DeleteBranch(branchToDelete, force: true);
                                if (forceDeleteResult.ExitCode == 0)
                                {
                                    MessageBox.Show($"Branch '{branchToDelete}' force deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    UpdateStatus();
                                }
                                else
                                {
                                    MessageBox.Show($"Failed to force delete branch.\n\nError: {forceDeleteResult.Error}", 
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnMergeBranchClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No Git repository is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gitRepo.HasUncommittedChanges())
            {
                MessageBox.Show("You have uncommitted changes. Please commit or discard them before merging.", 
                    "Uncommitted Changes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var branches = gitRepo.GetAllBranchesWithInfo();
            var currentBranch = gitRepo.GetCurrentBranch();

            // Filter out current branch
            var mergeableBranches = new System.Collections.Generic.List<BranchInfo>();
            foreach (var branch in branches)
            {
                if (!branch.IsCurrent)
                {
                    mergeableBranches.Add(branch);
                }
            }

            if (mergeableBranches.Count == 0)
            {
                MessageBox.Show("No branches available to merge.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show branch selector dialog
            using (var dialog = new BranchSelectorDialog(mergeableBranches.ToArray(), currentBranch))
            {
                dialog.Text = "Merge Branch";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var branchToMerge = dialog.SelectedBranch;
                    
                    var result = gitRepo.MergeBranch(branchToMerge);
                    if (result.ExitCode == 0)
                    {
                        MessageBox.Show($"Branch '{branchToMerge}' merged into '{currentBranch}'.\n\n{result.Output}", 
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshFileList();
                    }
                    else
                    {
                        // Check for conflicts first
                        var conflictedFiles = gitRepo.GetConflictedFiles();
                        if (conflictedFiles.Length > 0)
                        {
                            using (var conflictDialog = new MergeConflictDialog(
                                gitRepo, gitRepo.GetWorkingDirectory(), branchToMerge, conflictedFiles))
                            {
                                var dr = conflictDialog.ShowDialog();
                                if (dr == DialogResult.OK)
                                {
                                    MessageBox.Show($"Merge of '{branchToMerge}' completed successfully.",
                                        "Merge Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (dr == DialogResult.Abort)
                                {
                                    MessageBox.Show($"Merge of '{branchToMerge}' was aborted.",
                                        "Merge Aborted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Merge failed.\n\nError: {result.Error}\n\n{result.Output}", 
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        RefreshFileList();
                    }
                }
            }
        }

        // View menu
        private void OnShowToolbarClick(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                toolStrip.Visible = menuItem.Checked;
            }
        }

        private void OnShowCommitAreaClick(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                // Toggle visibility of commit container
                if (commitContainer != null)
                {
                    commitContainer.Visible = menuItem.Checked;
                }
            }
        }

        private void OnResetLayoutClick(object sender, EventArgs e)
        {
            // Reset splitter distances to default
            if (mainSplitter.Height > 0)
            {
                mainSplitter.SplitterDistance = mainSplitter.Height / 2;
            }
            if (lowerSplitter.Height > 0)
            {
                lowerSplitter.SplitterDistance = (lowerSplitter.Height * 2) / 3;
            }
        }

        // Help menu
        private void OnAboutClick(object sender, EventArgs e)
        {
            MessageBox.Show(
                "GitPane - Git Integration for Clarion IDE\n\n" +
                "A visual Git client integrated into the Clarion IDE.\n\n" +
                "Features:\n" +
                "- Stage, unstage, and commit changes\n" +
                "- Branch management\n" +
                "- Remote operations\n" +
                "- Commit history viewer\n" +
                "- GitHub integration",
                "About GitPane",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnManageTemplatesClick(object sender, EventArgs e)
        {
            if (templateManager == null)
            {
                MessageBox.Show(
                    "Template manager is not available.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var dialog = new TemplateManagerDialog(templateManager);
            dialog.ShowDialog();
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

        /// <summary>
        /// Builds the pane title: "Git · branch · ✓ up to date — repoName"
        /// Pass null for sync to show "checking…"
        /// </summary>
        private void TriggerSyncCheck(GitRepository repo, string repoName, string branch)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                var sync = repo.GetSyncStatus();
                contentPanel.BeginInvoke(new Action(() =>
                    UpdatePadTitle(BuildPaneTitle(repoName, branch, sync))));
            });
        }

        private void StartSyncTimer(GitRepository repo, string repoName, string branch)
        {
            StopSyncTimer();
            _syncTimer = new System.Windows.Forms.Timer { Interval = 60000 }; // 1 minute
            _syncTimer.Tick += (s, e) =>
            {
                // Re-read branch name each tick in case it changed
                string currentBranch = repo.GetCurrentBranch();
                TriggerSyncCheck(repo, repoName, currentBranch);
            };
            _syncTimer.Start();
        }

        private void StopSyncTimer()
        {
            if (_syncTimer != null)
            {
                _syncTimer.Stop();
                _syncTimer.Dispose();
                _syncTimer = null;
            }
        }

        private static string BuildPaneTitle(string repoName, string branch, GitRepository.SyncInfo sync)
        {
            string branchPart = branch ?? "unknown";

            string statusPart;
            if (sync == null)
            {
                statusPart = "checking\u2026"; // checking…, shown before async result
            }
            else if (sync.Behind == -1) // sentinel for "no upstream"
            {
                statusPart = "\u2717 no remote"; // ✗
            }
            else if (sync.Behind == 0 && sync.Ahead == 0)
            {
                statusPart = "\u2713 up to date"; // ✓
            }
            else
            {
                var sb = new System.Text.StringBuilder();
                if (sync.Behind > 0) sb.Append($"\u26a0 {sync.Behind} behind"); // ⚠
                if (sync.Behind > 0 && sync.Ahead > 0) sb.Append(" \u00b7 "); // ·
                if (sync.Ahead  > 0) sb.Append($"\u2191{sync.Ahead} ahead");   // ↑
                statusPart = sb.ToString();
            }

            return $"Git \u00b7 {branchPart} \u00b7 {statusPart} \u2014 {repoName}";
        }



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
            StopSyncTimer();
            debounceTimer?.Dispose();
            configDebounceTimer?.Dispose();
            contentPanel?.Dispose();
            base.Dispose();
        }

        #endregion

        private sealed class EmbeddedIconManager : System.Resources.ResourceManager
        {
            private readonly string _key;
            private readonly Bitmap _bitmap;
            public EmbeddedIconManager(string key, Bitmap bitmap) : base(key, Assembly.GetExecutingAssembly()) { _key = key; _bitmap = bitmap; }
            public override object GetObject(string name) => name == _key ? _bitmap : null;
            public override object GetObject(string name, System.Globalization.CultureInfo culture) => GetObject(name);
        }
    }
}

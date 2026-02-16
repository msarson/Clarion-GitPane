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

        private Panel contentPanel;
        
        // Toolbar controls
        private Label branchValueLabel;
        private ToolStripButton branchSelectButton;
        private Label remoteLabel;
        private ToolStripButton removeRemoteButton;
        private ToolStripButton addRemoteButton;
        private ToolStripButton createGitHubRepoButton;
        private ToolStripButton refreshButton;
        
        private Button initRepoButton;
        private Label statusLabel;
        
        // Commit workflow controls
        private GroupBox stagedGroupBox;
        private CheckedListBox stagedListBox;
        private Button unstageSelectedButton;
        private Button unstageAllButton;
        
        private GroupBox unstagedGroupBox;
        private CheckedListBox unstagedListBox;
        private ContextMenuStrip unstagedContextMenu;
        private Button stageSelectedButton;
        private Button stageAllButton;
        
        private Label commitMessageLabel;
        private TextBox commitMessageBox;
        private Button commitButton;
        private Button commitPushButton;
        private Button pushButton;
        
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
            
            // Layout now handled by Anchor/Dock properties - no manual resize needed
        }

        private void InitializeUI()
        {
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill; // Let the IDE handle sizing automatically
            contentPanel.BackColor = SystemColors.Window;
            contentPanel.Padding = new Padding(10);
            contentPanel.AutoScroll = true; // Enable auto-scroll as fallback
            // Removed manual resize handlers - let Anchor/Dock properties handle layout

            // ToolStrip for top controls - professional layout
            ToolStrip toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            
            // Branch info
            ToolStripLabel branchToolLabel = new ToolStripLabel("Branch:");
            branchValueLabel = new Label();
            branchValueLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);
            branchValueLabel.ForeColor = Color.DarkBlue;
            branchValueLabel.AutoSize = true;
            ToolStripControlHost branchValueHost = new ToolStripControlHost(branchValueLabel);
            
            branchSelectButton = new ToolStripButton("...");
            branchSelectButton.Click += OnBranchSelectClick;
            
            // Separator
            ToolStripSeparator separator1 = new ToolStripSeparator();
            
            // Remote info
            remoteLabel = new Label();
            remoteLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 8F);
            remoteLabel.ForeColor = SystemColors.GrayText;
            remoteLabel.AutoSize = true;
            remoteLabel.MaximumSize = new Size(200, 0);
            ToolStripControlHost remoteLabelHost = new ToolStripControlHost(remoteLabel);
            
            // Remote buttons
            removeRemoteButton = new ToolStripButton("×");
            removeRemoteButton.Font = new Font(SystemFonts.DefaultFont.FontFamily, 10F, FontStyle.Bold);
            removeRemoteButton.Click += OnRemoveRemoteClick;
            removeRemoteButton.Visible = false;
            
            addRemoteButton = new ToolStripButton("Add Remote");
            addRemoteButton.Click += OnAddRemoteClick;
            addRemoteButton.Visible = false;
            
            createGitHubRepoButton = new ToolStripButton("Create on GitHub");
            createGitHubRepoButton.Click += OnCreateGitHubRepoClick;
            createGitHubRepoButton.Visible = false;
            
            // Separator
            ToolStripSeparator separator2 = new ToolStripSeparator();
            
            // Refresh button
            refreshButton = new ToolStripButton("Refresh");
            refreshButton.Click += OnRefreshClick;
            
            // Add items to toolbar
            toolStrip.Items.Add(branchToolLabel);
            toolStrip.Items.Add(branchValueHost);
            toolStrip.Items.Add(branchSelectButton);
            toolStrip.Items.Add(separator1);
            toolStrip.Items.Add(remoteLabelHost);
            toolStrip.Items.Add(removeRemoteButton);
            toolStrip.Items.Add(addRemoteButton);
            toolStrip.Items.Add(createGitHubRepoButton);
            toolStrip.Items.Add(separator2);
            toolStrip.Items.Add(refreshButton);
            
            // Bottom panel for commit controls - Docked to bottom
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 100;

            // Staged files section - Docked to top
            stagedGroupBox = new GroupBox();
            stagedGroupBox.Text = "Staged Files (0)";
            stagedGroupBox.Dock = DockStyle.Top;
            stagedGroupBox.Height = 180;

            stagedListBox = new CheckedListBox();
            stagedListBox.Location = new Point(10, 20);
            stagedListBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            stagedListBox.Height = 125;
            stagedListBox.Font = new Font("Courier New", 8F);
            stagedListBox.CheckOnClick = true;

            unstageSelectedButton = new Button();
            unstageSelectedButton.Text = "Unstage Selected";
            unstageSelectedButton.Location = new Point(10, 150);
            unstageSelectedButton.Width = 120;
            unstageSelectedButton.Height = 25;
            unstageSelectedButton.Click += OnUnstageSelectedClick;
            unstageSelectedButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            unstageAllButton = new Button();
            unstageAllButton.Text = "Unstage All";
            unstageAllButton.Location = new Point(135, 150);
            unstageAllButton.Width = 100;
            unstageAllButton.Height = 25;
            unstageAllButton.Click += OnUnstageAllClick;
            unstageAllButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            stagedGroupBox.Controls.Add(stagedListBox);
            stagedGroupBox.Controls.Add(unstageSelectedButton);
            stagedGroupBox.Controls.Add(unstageAllButton);

            // Unstaged files section - Fills remaining space
            unstagedGroupBox = new GroupBox();
            unstagedGroupBox.Text = "Unstaged Files (0)";
            unstagedGroupBox.Dock = DockStyle.Fill;

            unstagedListBox = new CheckedListBox();
            unstagedListBox.Location = new Point(10, 20);
            unstagedListBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            unstagedListBox.Font = new Font("Courier New", 8F);
            unstagedListBox.CheckOnClick = true;
            unstagedListBox.MouseDown += OnUnstagedListBoxMouseDown;

            // Context menu for unstaged files
            unstagedContextMenu = new ContextMenuStrip();
            var ignoreFileItem = new ToolStripMenuItem("Ignore this file");
            ignoreFileItem.Click += OnIgnoreFileClick;
            var ignoreExtensionItem = new ToolStripMenuItem("Ignore file type (*.[ext])");
            ignoreExtensionItem.Click += OnIgnoreExtensionClick;
            var ignoreDirectoryItem = new ToolStripMenuItem("Ignore directory");
            ignoreDirectoryItem.Click += OnIgnoreDirectoryClick;
            unstagedContextMenu.Items.Add(ignoreFileItem);
            unstagedContextMenu.Items.Add(ignoreExtensionItem);
            unstagedContextMenu.Items.Add(ignoreDirectoryItem);
            unstagedContextMenu.Opening += OnUnstagedContextMenuOpening;
            unstagedListBox.ContextMenuStrip = unstagedContextMenu;

            stageSelectedButton = new Button();
            stageSelectedButton.Text = "Stage Selected";
            stageSelectedButton.Location = new Point(10, 140);
            stageSelectedButton.Width = 120;
            stageSelectedButton.Height = 25;
            stageSelectedButton.Click += OnStageSelectedClick;
            stageSelectedButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            stageAllButton = new Button();
            stageAllButton.Text = "Stage All";
            stageAllButton.Location = new Point(135, 140);
            stageAllButton.Width = 100;
            stageAllButton.Height = 25;
            stageAllButton.Click += OnStageAllClick;
            stageAllButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            unstagedGroupBox.Controls.Add(unstagedListBox);
            unstagedGroupBox.Controls.Add(stageSelectedButton);
            unstagedGroupBox.Controls.Add(stageAllButton);

            // Commit message section - Goes in bottom panel
            commitMessageLabel = new Label();
            commitMessageLabel.Text = "Commit Message:";
            commitMessageLabel.Location = new Point(0, 0);
            commitMessageLabel.AutoSize = true;

            commitMessageBox = new TextBox();
            commitMessageBox.Multiline = true;
            commitMessageBox.Location = new Point(0, 20);
            commitMessageBox.Size = new Size(450, 40);
            commitMessageBox.ScrollBars = ScrollBars.Vertical;
            commitMessageBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Commit buttons - Goes in bottom panel
            commitButton = new Button();
            commitButton.Text = "Commit";
            commitButton.Location = new Point(0, 65);
            commitButton.Width = 80;
            commitButton.Height = 28;
            commitButton.Click += OnCommitClick;

            commitPushButton = new Button();
            commitPushButton.Text = "Commit && Push";
            commitPushButton.Location = new Point(85, 65);
            commitPushButton.Width = 110;
            commitPushButton.Height = 28;
            commitPushButton.Click += OnCommitPushClick;

            // Push button (for unpushed commits)
            pushButton = new Button();
            pushButton.Text = "Push";
            pushButton.Location = new Point(200, 65);
            pushButton.Width = 80;
            pushButton.Height = 28;
            pushButton.Click += OnPushClick;
            pushButton.Visible = false;
            
            bottomPanel.Controls.Add(commitMessageLabel);
            bottomPanel.Controls.Add(commitMessageBox);
            bottomPanel.Controls.Add(commitButton);
            bottomPanel.Controls.Add(commitPushButton);
            bottomPanel.Controls.Add(pushButton);

            // Status label (for non-repo message)
            statusLabel = new Label();
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F);
            statusLabel.ForeColor = SystemColors.GrayText;
            statusLabel.Location = new Point(10, 10);

            // Initialize Repo button (for non-repo directories)
            initRepoButton = new Button();
            initRepoButton.Text = "Initialize Git Repository";
            initRepoButton.Location = new Point(10, 40);
            initRepoButton.Width = 180;
            initRepoButton.Height = 30;
            initRepoButton.Click += OnInitRepoClick;
            initRepoButton.Visible = false;

            // Add controls to contentPanel in correct docking order
            // For docking: Bottom first, then Top (in REVERSE order), then Fill
            contentPanel.Controls.Add(bottomPanel);      // Dock.Bottom - at bottom
            contentPanel.Controls.Add(unstagedGroupBox); // Dock.Fill - fills middle
            contentPanel.Controls.Add(stagedGroupBox);   // Dock.Top - below toolbar
            contentPanel.Controls.Add(toolStrip);        // Dock.Top - at very top
            contentPanel.Controls.Add(statusLabel);
            contentPanel.Controls.Add(initRepoButton);
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

                    branchValueLabel.Visible = true;
                    branchSelectButton.Visible = true;
                    refreshButton.Visible = true;
                    statusLabel.Visible = false;
                    
                    // Check remote status
                    UpdateRemoteStatus();
                    
                    // Show commit workflow controls
                    stagedGroupBox.Visible = true;
                    unstagedGroupBox.Visible = true;
                    commitMessageLabel.Visible = true;
                    commitMessageBox.Visible = true;
                    commitButton.Visible = true;
                    commitPushButton.Visible = true;
                    
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

using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    public class GitPanePad : AbstractPadContent
    {
        private Panel contentPanel;
        private Label branchLabel;
        private Label branchValueLabel;
        private Button branchSelectButton;
        private Label remoteLabel;
        private Button addRemoteButton;
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
        private Button refreshButton;
        
        private GitRepository gitRepo;
        private System.IO.FileSystemWatcher fileWatcher;
        private System.Threading.Timer debounceTimer;

        public override Control Control => contentPanel;

        public GitPanePad()
        {
            InitializeUI();
            UpdateStatus();

            // Subscribe to solution events
            ProjectService.SolutionLoaded += OnSolutionChanged;
            ProjectService.SolutionClosed += OnSolutionClosed;
            
            // Ensure initial layout is applied
            contentPanel.Layout += OnPanelLayout;
        }

        private void InitializeUI()
        {
            contentPanel = new Panel();
            contentPanel.BackColor = SystemColors.Window;
            contentPanel.Padding = new Padding(10);
            contentPanel.AutoScroll = true;
            contentPanel.Resize += OnPanelResize;
            contentPanel.SizeChanged += OnPanelResize;

            // Branch label
            branchLabel = new Label();
            branchLabel.AutoSize = true;
            branchLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F);
            branchLabel.ForeColor = SystemColors.ControlText;
            branchLabel.Location = new Point(10, 10);
            branchLabel.Text = "Branch:";

            // Branch value label
            branchValueLabel = new Label();
            branchValueLabel.AutoSize = true;
            branchValueLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);
            branchValueLabel.ForeColor = Color.DarkBlue;
            branchValueLabel.Location = new Point(60, 10);

            // Branch select button
            branchSelectButton = new Button();
            branchSelectButton.Text = "...";
            branchSelectButton.Width = 30;
            branchSelectButton.Height = 23;
            branchSelectButton.Location = new Point(200, 7);
            branchSelectButton.Click += OnBranchSelectClick;

            // Remote label
            remoteLabel = new Label();
            remoteLabel.AutoSize = true;
            remoteLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 8F);
            remoteLabel.ForeColor = SystemColors.GrayText;
            remoteLabel.Location = new Point(240, 12);
            remoteLabel.Text = "";

            // Add Remote button
            addRemoteButton = new Button();
            addRemoteButton.Text = "Add Remote Origin";
            addRemoteButton.Location = new Point(240, 7);
            addRemoteButton.Width = 130;
            addRemoteButton.Height = 23;
            addRemoteButton.Click += OnAddRemoteClick;
            addRemoteButton.Visible = false;

            // Refresh button
            refreshButton = new Button();
            refreshButton.Text = "Refresh";
            refreshButton.Location = new Point(380, 7);
            refreshButton.Width = 70;
            refreshButton.Height = 23;
            refreshButton.Click += OnRefreshClick;

            // Staged files section
            stagedGroupBox = new GroupBox();
            stagedGroupBox.Text = "Staged Files (0)";
            stagedGroupBox.Location = new Point(10, 40);
            stagedGroupBox.Size = new Size(460, 170);

            stagedListBox = new CheckedListBox();
            stagedListBox.Location = new Point(10, 20);
            stagedListBox.Size = new Size(440, 115);
            stagedListBox.Font = new Font("Courier New", 8F);
            stagedListBox.CheckOnClick = true;

            unstageSelectedButton = new Button();
            unstageSelectedButton.Text = "Unstage Selected";
            unstageSelectedButton.Location = new Point(10, 140);
            unstageSelectedButton.Width = 120;
            unstageSelectedButton.Height = 25;
            unstageSelectedButton.Click += OnUnstageSelectedClick;

            unstageAllButton = new Button();
            unstageAllButton.Text = "Unstage All";
            unstageAllButton.Location = new Point(135, 140);
            unstageAllButton.Width = 100;
            unstageAllButton.Height = 25;
            unstageAllButton.Click += OnUnstageAllClick;

            stagedGroupBox.Controls.Add(stagedListBox);
            stagedGroupBox.Controls.Add(unstageSelectedButton);
            stagedGroupBox.Controls.Add(unstageAllButton);

            // Unstaged files section
            unstagedGroupBox = new GroupBox();
            unstagedGroupBox.Text = "Unstaged Files (0)";
            unstagedGroupBox.Location = new Point(10, 220);
            unstagedGroupBox.Size = new Size(460, 170);

            unstagedListBox = new CheckedListBox();
            unstagedListBox.Location = new Point(10, 20);
            unstagedListBox.Size = new Size(440, 115);
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

            stageAllButton = new Button();
            stageAllButton.Text = "Stage All";
            stageAllButton.Location = new Point(135, 140);
            stageAllButton.Width = 100;
            stageAllButton.Height = 25;
            stageAllButton.Click += OnStageAllClick;

            unstagedGroupBox.Controls.Add(unstagedListBox);
            unstagedGroupBox.Controls.Add(stageSelectedButton);
            unstagedGroupBox.Controls.Add(stageAllButton);

            // Commit message section
            commitMessageLabel = new Label();
            commitMessageLabel.Text = "Commit Message:";
            commitMessageLabel.Location = new Point(10, 400);
            commitMessageLabel.AutoSize = true;

            commitMessageBox = new TextBox();
            commitMessageBox.Multiline = true;
            commitMessageBox.Location = new Point(10, 420);
            commitMessageBox.Size = new Size(460, 60);
            commitMessageBox.ScrollBars = ScrollBars.Vertical;

            // Commit buttons
            commitButton = new Button();
            commitButton.Text = "Commit";
            commitButton.Location = new Point(10, 490);
            commitButton.Width = 80;
            commitButton.Height = 28;
            commitButton.Click += OnCommitClick;

            commitPushButton = new Button();
            commitPushButton.Text = "Commit && Push";
            commitPushButton.Location = new Point(95, 490);
            commitPushButton.Width = 110;
            commitPushButton.Height = 28;
            commitPushButton.Click += OnCommitPushClick;

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

            contentPanel.Controls.Add(branchLabel);
            contentPanel.Controls.Add(branchValueLabel);
            contentPanel.Controls.Add(branchSelectButton);
            contentPanel.Controls.Add(remoteLabel);
            contentPanel.Controls.Add(addRemoteButton);
            contentPanel.Controls.Add(refreshButton);
            contentPanel.Controls.Add(stagedGroupBox);
            contentPanel.Controls.Add(unstagedGroupBox);
            contentPanel.Controls.Add(commitMessageLabel);
            contentPanel.Controls.Add(commitMessageBox);
            contentPanel.Controls.Add(commitButton);
            contentPanel.Controls.Add(commitPushButton);
            contentPanel.Controls.Add(statusLabel);
            contentPanel.Controls.Add(initRepoButton);
        }

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

                    branchLabel.Visible = true;
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
            branchLabel.Visible = false;
            branchValueLabel.Visible = false;
            branchSelectButton.Visible = false;
            remoteLabel.Visible = false;
            addRemoteButton.Visible = false;
            initRepoButton.Visible = false;
            refreshButton.Visible = false;
            stagedGroupBox.Visible = false;
            unstagedGroupBox.Visible = false;
            commitMessageLabel.Visible = false;
            commitMessageBox.Visible = false;
            commitButton.Visible = false;
            commitPushButton.Visible = false;
        }

        private void RefreshFileList()
        {
            if (gitRepo == null)
                return;

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

        private void OnCommitClick(object sender, EventArgs e)
        {
            PerformCommit(false);
        }

        private void OnCommitPushClick(object sender, EventArgs e)
        {
            PerformCommit(true);
        }

        private void PerformCommit(bool andPush)
        {
            var message = commitMessageBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("Please enter a commit message.", "Commit Message Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (stagedListBox.Items.Count == 0)
            {
                MessageBox.Show("No files staged for commit.", "Nothing to Commit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (gitRepo.CommitChanges(message))
            {
                commitMessageBox.Clear();
                
                if (andPush)
                {
                    if (gitRepo.PushChanges())
                    {
                        MessageBox.Show("Committed and pushed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Committed locally but push failed. You can push manually later.", "Partial Success", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Committed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                RefreshFileList();
                UpdateStatus();
            }
            else
            {
                MessageBox.Show("Failed to commit changes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateRemoteStatus()
        {
            if (gitRepo == null)
                return;

            bool hasRemote = gitRepo.HasRemote();
            
            if (hasRemote)
            {
                string remoteUrl = gitRepo.GetRemoteUrl();
                if (!string.IsNullOrEmpty(remoteUrl))
                {
                    // Show shortened remote URL
                    string displayUrl = remoteUrl;
                    if (displayUrl.Length > 40)
                        displayUrl = displayUrl.Substring(0, 37) + "...";
                    
                    remoteLabel.Text = $"Remote: {displayUrl}";
                    remoteLabel.Visible = true;
                }
                addRemoteButton.Visible = false;
            }
            else
            {
                remoteLabel.Text = "Local repository only";
                remoteLabel.Visible = true;
                addRemoteButton.Visible = true;
            }
        }

        private void OnAddRemoteClick(object sender, EventArgs e)
        {
            // Prompt for remote URL
            var urlDialog = new Form();
            urlDialog.Text = "Add Remote Origin";
            urlDialog.Width = 500;
            urlDialog.Height = 160;
            urlDialog.StartPosition = FormStartPosition.CenterParent;
            urlDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            urlDialog.MaximizeBox = false;
            urlDialog.MinimizeBox = false;

            var label = new Label();
            label.Text = "Enter remote URL (HTTPS or SSH):";
            label.Location = new Point(10, 10);
            label.AutoSize = true;
            urlDialog.Controls.Add(label);

            var urlBox = new TextBox();
            urlBox.Location = new Point(10, 35);
            urlBox.Width = 460;
            urlDialog.Controls.Add(urlBox);

            var exampleLabel = new Label();
            exampleLabel.Text = "Example: https://github.com/user/repo.git";
            exampleLabel.Location = new Point(10, 60);
            exampleLabel.AutoSize = true;
            exampleLabel.ForeColor = SystemColors.GrayText;
            urlDialog.Controls.Add(exampleLabel);

            var okButton = new Button();
            okButton.Text = "Add";
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(310, 85);
            okButton.Width = 75;
            urlDialog.Controls.Add(okButton);

            var cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(395, 85);
            cancelButton.Width = 75;
            urlDialog.Controls.Add(cancelButton);

            urlDialog.AcceptButton = okButton;
            urlDialog.CancelButton = cancelButton;

            if (urlDialog.ShowDialog() == DialogResult.OK)
            {
                string url = urlBox.Text.Trim();
                
                if (string.IsNullOrEmpty(url))
                {
                    MessageBox.Show("URL cannot be empty.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Basic validation
                if (!url.StartsWith("http://") && !url.StartsWith("https://") && 
                    !url.StartsWith("git@") && !url.StartsWith("ssh://"))
                {
                    MessageBox.Show("URL must start with http://, https://, git@, or ssh://", 
                        "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (gitRepo.AddRemote("origin", url))
                {
                    MessageBox.Show("Remote origin added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateRemoteStatus();
                    UpdateStatus(); // Refresh to update repo name if it changed
                }
                else
                {
                    MessageBox.Show("Failed to add remote. The remote may already exist or the URL is invalid.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnInitRepoClick(object sender, EventArgs e)
        {
            if (ProjectService.OpenSolution == null)
                return;

            string solutionDir = ProjectService.OpenSolution.Directory;
            
            var result = MessageBox.Show(
                $"Initialize a new Git repository in:\n{solutionDir}\n\nThis will create a .git folder. Continue?",
                "Initialize Git Repository",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (gitRepo != null && gitRepo.InitializeRepository())
                {
                    MessageBox.Show("Git repository initialized successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatus(); // Refresh UI to show repo controls
                }
                else
                {
                    MessageBox.Show("Failed to initialize Git repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

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

        private void OnPanelResize(object sender, EventArgs e)
        {
            // Dynamically reposition controls based on panel width
            int panelWidth = contentPanel.Width;
            
            // Skip if panel hasn't been sized yet
            if (panelWidth <= 0)
                return;
                
            int margin = 10;
            int rightMargin = 20; // Extra padding on right since IDE has no border
            int availableWidth = panelWidth - margin - rightMargin;

            // Position top-right controls (remoteLabel, addRemoteButton, refreshButton)
            // Work backwards from right edge with proper margin
            refreshButton.Left = panelWidth - rightMargin - refreshButton.Width;

            if (addRemoteButton.Visible)
            {
                addRemoteButton.Left = refreshButton.Left - margin - addRemoteButton.Width;
            }
            
            if (remoteLabel.Visible)
            {
                // Position remote label to the left of refresh button
                remoteLabel.Left = refreshButton.Left - margin - remoteLabel.Width;
            }

            // Adjust anchored control widths to respect right margin
            stagedGroupBox.Width = availableWidth;
            unstagedGroupBox.Width = availableWidth;
            commitMessageBox.Width = availableWidth;
            
            // Adjust list boxes inside group boxes
            stagedListBox.Width = availableWidth - 20; // 20 = internal padding
            unstagedListBox.Width = availableWidth - 20;
        }

        private void OnPanelLayout(object sender, LayoutEventArgs e)
        {
            // Trigger resize logic when panel layout changes
            OnPanelResize(sender, EventArgs.Empty);
        }

        private void OnBranchSelectClick(object sender, EventArgs e)
        {
            if (gitRepo == null)
                return;

            var branches = gitRepo.GetAllBranchesWithInfo();
            var currentBranch = gitRepo.GetCurrentBranch();

            using (var dialog = new BranchSelectorDialog(branches, currentBranch))
            {
                if (dialog.ShowDialog() == DialogResult.OK && dialog.SelectedBranch != null)
                {
                    string selectedBranch = dialog.SelectedBranch;

                    // Don't switch if already on this branch
                    if (selectedBranch == currentBranch)
                        return;

                    // Don't allow switching from detached HEAD entries
                    if (selectedBranch.StartsWith("HEAD"))
                        return;

                    // Check for uncommitted changes
                    if (gitRepo.HasUncommittedChanges())
                    {
                        if (!HandleUncommittedChanges(selectedBranch))
                            return; // User cancelled or operation failed
                    }
                    else
                    {
                        // No uncommitted changes, just confirm and switch
                        if (MessageBox.Show(
                            $"Switch to branch '{selectedBranch}'?",
                            "Switch Branch",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            if (gitRepo.CheckoutBranch(selectedBranch))
                            {
                                // Check if there are stashes after switching
                                CheckForStashesToRestore();
                                MessageBox.Show($"Switched to branch '{selectedBranch}'", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                UpdateStatus();
                            }
                            else
                            {
                                MessageBox.Show($"Failed to switch to branch '{selectedBranch}'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private bool HandleUncommittedChanges(string targetBranch)
        {
            var changesStatus = gitRepo.GetUncommittedChangesStatus();
            var changes = changesStatus.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            using (var dialog = new Form())
            {
                dialog.Text = "Uncommitted Changes";
                dialog.Size = new Size(600, 450);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var infoLabel = new Label();
                infoLabel.Text = $"You have {changes.Length} uncommitted change(s). What would you like to do?";
                infoLabel.Location = new Point(10, 10);
                infoLabel.AutoSize = true;
                infoLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);

                var selectAllCheckBox = new CheckBox();
                selectAllCheckBox.Text = "Select All";
                selectAllCheckBox.Location = new Point(10, 35);
                selectAllCheckBox.AutoSize = true;
                selectAllCheckBox.Checked = true;

                var changesListBox = new CheckedListBox();
                changesListBox.Location = new Point(10, 60);
                changesListBox.Size = new Size(560, 280);
                changesListBox.Font = new Font("Courier New", 9F);
                changesListBox.CheckOnClick = true;
                
                foreach (var change in changes)
                {
                    changesListBox.Items.Add(change, true);
                }

                selectAllCheckBox.CheckedChanged += (s, e) =>
                {
                    for (int i = 0; i < changesListBox.Items.Count; i++)
                    {
                        changesListBox.SetItemChecked(i, selectAllCheckBox.Checked);
                    }
                };

                var stashButton = new Button();
                stashButton.Text = "Stash";
                stashButton.Location = new Point(10, 350);
                stashButton.Width = 90;
                stashButton.Click += (s, ev) => { dialog.Tag = "stash"; dialog.Close(); };

                var commitButton = new Button();
                commitButton.Text = "Commit";
                commitButton.Location = new Point(105, 350);
                commitButton.Width = 90;
                commitButton.Click += (s, ev) => { dialog.Tag = "commit"; dialog.Close(); };

                var commitPushButton = new Button();
                commitPushButton.Text = "Commit && Push";
                commitPushButton.Location = new Point(200, 350);
                commitPushButton.Width = 110;
                commitPushButton.Click += (s, ev) => { dialog.Tag = "commitpush"; dialog.Close(); };

                var discardButton = new Button();
                discardButton.Text = "Discard";
                discardButton.Location = new Point(315, 350);
                discardButton.Width = 75;
                discardButton.ForeColor = Color.Red;
                discardButton.Click += (s, ev) => 
                { 
                    if (MessageBox.Show("Are you sure you want to discard all changes? This cannot be undone!", 
                        "Confirm Discard", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        dialog.Tag = "discard"; 
                        dialog.Close(); 
                    }
                };

                var cancelButton = new Button();
                cancelButton.Text = "Cancel";
                cancelButton.Location = new Point(395, 350);
                cancelButton.Width = 75;
                cancelButton.Click += (s, ev) => { dialog.Tag = "cancel"; dialog.Close(); };

                var helpLabel = new Label();
                helpLabel.Text = "Note: All operations apply to all files (selection is for reference only)";
                helpLabel.Location = new Point(10, 380);
                helpLabel.AutoSize = true;
                helpLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 7.5F);
                helpLabel.ForeColor = Color.Gray;

                dialog.Controls.Add(infoLabel);
                dialog.Controls.Add(selectAllCheckBox);
                dialog.Controls.Add(changesListBox);
                dialog.Controls.Add(stashButton);
                dialog.Controls.Add(commitButton);
                dialog.Controls.Add(commitPushButton);
                dialog.Controls.Add(discardButton);
                dialog.Controls.Add(cancelButton);
                dialog.Controls.Add(helpLabel);

                dialog.ShowDialog();

                string action = dialog.Tag as string;

                if (action == "stash")
                {
                    if (gitRepo.StashChanges($"Before switching to {targetBranch}"))
                    {
                        if (gitRepo.CheckoutBranch(targetBranch))
                        {
                            // Don't prompt for stash restore right after stashing - user just wanted to switch
                            MessageBox.Show($"Changes stashed and switched to branch '{targetBranch}'", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            UpdateStatus();
                            return true;
                        }
                    }
                    MessageBox.Show("Failed to stash changes or switch branch.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (action == "commit" || action == "commitpush")
                {
                    var commitMsg = PromptForCommitMessage();
                    if (string.IsNullOrWhiteSpace(commitMsg))
                        return false; // User cancelled

                    if (gitRepo.CommitChanges(commitMsg))
                    {
                        if (action == "commitpush")
                        {
                            if (!gitRepo.PushChanges())
                            {
                                MessageBox.Show("Committed locally but push failed. You can push manually later.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }

                        if (gitRepo.CheckoutBranch(targetBranch))
                        {
                            // Check if there are stashes after switching
                            CheckForStashesToRestore();
                            MessageBox.Show($"Changes committed and switched to branch '{targetBranch}'", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            UpdateStatus();
                            return true;
                        }
                    }
                    MessageBox.Show("Failed to commit changes or switch branch.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (action == "discard")
                {
                    if (gitRepo.DiscardChanges())
                    {
                        if (gitRepo.CheckoutBranch(targetBranch))
                        {
                            // Check if there are stashes after switching
                            CheckForStashesToRestore();
                            MessageBox.Show($"Changes discarded and switched to branch '{targetBranch}'", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            UpdateStatus();
                            return true;
                        }
                    }
                    MessageBox.Show("Failed to discard changes or switch branch.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return false; // Cancelled
            }
        }

        private string PromptForCommitMessage()
        {
            using (var dialog = new Form())
            {
                dialog.Text = "Commit Message";
                dialog.Size = new Size(450, 200);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var label = new Label();
                label.Text = "Enter commit message:";
                label.Location = new Point(10, 10);
                label.AutoSize = true;

                var textBox = new TextBox();
                textBox.Multiline = true;
                textBox.Location = new Point(10, 35);
                textBox.Size = new Size(410, 80);
                textBox.ScrollBars = ScrollBars.Vertical;

                var okButton = new Button();
                okButton.Text = "OK";
                okButton.Location = new Point(255, 125);
                okButton.Width = 80;
                okButton.Click += (s, e) => { dialog.DialogResult = DialogResult.OK; dialog.Close(); };

                var cancelButton = new Button();
                cancelButton.Text = "Cancel";
                cancelButton.Location = new Point(340, 125);
                cancelButton.Width = 80;
                cancelButton.Click += (s, e) => { dialog.DialogResult = DialogResult.Cancel; dialog.Close(); };

                dialog.Controls.Add(label);
                dialog.Controls.Add(textBox);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                if (dialog.ShowDialog() == DialogResult.OK)
                    return textBox.Text;
                
                return null;
            }
        }

        private void CheckForStashesToRestore()
        {
            if (!gitRepo.HasStashes())
                return;

            var stashes = gitRepo.GetStashList();
            var currentBranch = gitRepo.GetCurrentBranch();

            using (var dialog = new Form())
            {
                dialog.Text = "Stashed Changes Available";
                dialog.Size = new Size(500, 350);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var label = new Label();
                label.Text = $"There are stashed changes available on '{currentBranch}'.\nWould you like to restore them?";
                label.Location = new Point(10, 10);
                label.AutoSize = true;

                var listBox = new ListBox();
                listBox.Location = new Point(10, 50);
                listBox.Size = new Size(460, 200);
                listBox.Font = new Font("Courier New", 8F);
                
                for (int i = 0; i < stashes.Length; i++)
                {
                    listBox.Items.Add($"[{i}] {stashes[i]}");
                }
                
                if (stashes.Length > 0)
                    listBox.SelectedIndex = 0;

                var applyButton = new Button();
                applyButton.Text = "Apply";
                applyButton.Location = new Point(10, 260);
                applyButton.Width = 90;
                applyButton.Click += (s, ev) => { dialog.Tag = "apply"; dialog.Close(); };

                var popButton = new Button();
                popButton.Text = "Pop";
                popButton.Location = new Point(105, 260);
                popButton.Width = 90;
                popButton.Click += (s, ev) => { dialog.Tag = "pop"; dialog.Close(); };

                var dropButton = new Button();
                dropButton.Text = "Drop";
                dropButton.Location = new Point(200, 260);
                dropButton.Width = 90;
                dropButton.ForeColor = Color.Red;
                dropButton.Click += (s, ev) => 
                { 
                    if (MessageBox.Show("Are you sure you want to permanently delete this stash?", 
                        "Confirm Drop", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        dialog.Tag = "drop"; 
                        dialog.Close(); 
                    }
                };

                var cancelButton = new Button();
                cancelButton.Text = "Not Now";
                cancelButton.Location = new Point(295, 260);
                cancelButton.Width = 90;
                cancelButton.Click += (s, ev) => { dialog.Tag = "cancel"; dialog.Close(); };

                var helpLabel = new Label();
                helpLabel.Text = "Apply: Keep stash for later | Pop: Apply and remove | Drop: Delete stash";
                helpLabel.Location = new Point(10, 290);
                helpLabel.AutoSize = true;
                helpLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 7.5F);
                helpLabel.ForeColor = Color.Gray;

                dialog.Controls.Add(label);
                dialog.Controls.Add(listBox);
                dialog.Controls.Add(applyButton);
                dialog.Controls.Add(popButton);
                dialog.Controls.Add(dropButton);
                dialog.Controls.Add(cancelButton);
                dialog.Controls.Add(helpLabel);

                dialog.ShowDialog();

                string action = dialog.Tag as string;
                int selectedIndex = listBox.SelectedIndex >= 0 ? listBox.SelectedIndex : 0;

                if (action == "apply")
                {
                    if (gitRepo.ApplyStash(selectedIndex))
                    {
                        MessageBox.Show("Stash applied successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateStatus();
                    }
                    else
                    {
                        MessageBox.Show("Failed to apply stash. You may have conflicts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (action == "pop")
                {
                    if (gitRepo.PopStash(selectedIndex))
                    {
                        MessageBox.Show("Stash popped successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateStatus();
                    }
                    else
                    {
                        MessageBox.Show("Failed to pop stash. You may have conflicts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (action == "drop")
                {
                    if (gitRepo.DropStash(selectedIndex))
                    {
                        MessageBox.Show("Stash dropped successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to drop stash.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
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
            debounceTimer?.Dispose();
            contentPanel?.Dispose();
            base.Dispose();
        }

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
            }
            catch
            {
                // Silently fail if we can't watch (maybe permissions issue)
                fileWatcher = null;
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
        }

        private void OnFileSystemChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            // Ignore .git folder changes
            if (e.FullPath.Contains("\\.git\\"))
                return;

            // Debounce the refresh - only refresh after 500ms of no changes
            if (debounceTimer != null)
                debounceTimer.Dispose();

            debounceTimer = new System.Threading.Timer(state =>
            {
                if (contentPanel.InvokeRequired)
                {
                    try
                    {
                        contentPanel.Invoke(new Action(RefreshFileList));
                    }
                    catch
                    {
                        // Ignore if control is disposed
                    }
                }
                else
                {
                    RefreshFileList();
                }
            }, null, 500, System.Threading.Timeout.Infinite);
        }
    }
}

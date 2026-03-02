using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    /// <summary>
    /// GitPanePad - Designer (UI Construction)
    /// This file contains control declarations and UI initialization code.
    /// Resembles a WinForms .Designer.cs but is human-maintainable.
    /// </summary>
    partial class GitPanePad
    {
        #region Control Declarations
        
        private Panel contentPanel;
        
        // Menu and toolbar controls (MenuStrip removed - caused reparenting issues in SharpDevelop host)
        private ToolStrip toolStrip;
        private ToolStripDropDownButton branchDropDown;
        
        // Menu dropdowns for visibility control
        private ToolStripDropDownButton fileMenu;
        private ToolStripDropDownButton repoMenu;
        private ToolStripDropDownButton branchMenu;
        private ToolStripDropDownButton viewMenu;
        private ToolStripDropDownButton helpMenu;
        private ToolStripButton initRepoMenuButton;
        
        // Menu items that need context-aware visibility
        private ToolStripMenuItem initRepoMenuItem;
        private ToolStripMenuItem openExternalMenuItem;
        private ToolStripMenuItem openGitignoreMenuItem;
        private ToolStripMenuItem openGitattributesMenuItem;
        private ToolStripMenuItem applyTemplateMenuItem;
        private ToolStripMenuItem fetchMenuItem;
        private ToolStripMenuItem pullMenuItem;
        private ToolStripMenuItem pushMenuItem;
        private ToolStripMenuItem viewOnRemoteMenuItem;
        private ToolStripMenuItem createGitHubMenuItem;
        private ToolStripMenuItem addRemoteMenuItem;
        private ToolStripMenuItem removeRemoteMenuItem;
        private ToolStripMenuItem switchBranchMenuItem;
        private ToolStripMenuItem createBranchMenuItem;
        private ToolStripMenuItem deleteBranchMenuItem;
        private ToolStripMenuItem mergeBranchMenuItem;
        
        // Main layout containers
        private SplitContainer mainSplitter;
        private SplitContainer lowerSplitter;
        private Panel commitContainer;
        private Panel commitPanel;
        
        private Button initRepoButton;
        private Label statusLabel;
        
        // Commit workflow controls
        private GroupBox stagedGroupBox;
        private CheckedListBox stagedListBox;
        private ToolStrip stagedToolStrip;
        private ToolStripButton unstageSelectedButton;
        private ToolStripButton unstageAllButton;
        
        private GroupBox unstagedGroupBox;
        private CheckedListBox unstagedListBox;
        private ContextMenuStrip unstagedContextMenu;
        private ToolStrip unstagedToolStrip;
        private ToolStripButton stageSelectedButton;
        private ToolStripButton stageAllButton;
        private ToolStripButton discardSelectedButton;
        private ToolStripButton discardAllButton;
        
        private Panel topSpacerPanel;
        private Label commitMessageLabel;
        private TextBox commitMessageBox;
        private ToolStrip commitToolStrip;
        private ToolStripButton commitButton;
        private ToolStripButton commitPushButton;
        private ToolStripButton pushButton;

        // Stash panel
        private GroupBox stashGroupBox;
        private ListView stashListView;
        private ToolStrip stashToolStrip;
        private ToolStripButton newStashButton;
        private ToolStripButton applyStashButton;
        private ToolStripButton popStashButton;
        private ToolStripButton dropStashButton;
        
        #endregion
        
        #region UI Initialization
        
        private void InitializeUI()
        {
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = SystemColors.Window;
            contentPanel.Padding = new Padding(10);
            contentPanel.AutoScroll = false; // FIX: Disable AutoScroll - incompatible with Dock.Fill SplitContainer

            // ToolStrip for menu and toolbar - MenuStrip removed due to SharpDevelop host reparenting issues
            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Padding = new Padding(5, 2, 5, 6); // Extra bottom padding to separate from staged group
            
            // Top-level Initialize Repository button (visible when no repo)
            initRepoMenuButton = new ToolStripButton("Initialize Repository");
            initRepoMenuButton.Click += OnInitRepoClick;
            initRepoMenuButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            initRepoMenuButton.AutoToolTip = false;
            initRepoMenuButton.ToolTipText = "Create a new Git repository in the current solution directory";
            
            // File menu
            fileMenu = new ToolStripDropDownButton("File");
            fileMenu.AutoToolTip = false;
            initRepoMenuItem = new ToolStripMenuItem("Initialize Repository...");
            initRepoMenuItem.Click += OnInitRepoClick;
            openExternalMenuItem = new ToolStripMenuItem("Open in External Tool");
            var openGitHubDesktopMenuItem = new ToolStripMenuItem("GitHub Desktop");
            openGitHubDesktopMenuItem.Click += OnOpenGitHubDesktopClick;
            openGitHubDesktopMenuItem.ToolTipText = "Open this repository in GitHub Desktop";
            var openGitKrakenMenuItem = new ToolStripMenuItem("GitKraken");
            openGitKrakenMenuItem.Click += OnOpenGitKrakenClick;
            openGitKrakenMenuItem.ToolTipText = "Open this repository in GitKraken";
            openExternalMenuItem.DropDownItems.Add(openGitHubDesktopMenuItem);
            openExternalMenuItem.DropDownItems.Add(openGitKrakenMenuItem);
            
            openGitignoreMenuItem = new ToolStripMenuItem("Open .gitignore");
            openGitignoreMenuItem.Click += OnOpenGitignoreClick;
            openGitignoreMenuItem.ToolTipText = "Open .gitignore file in the IDE editor";
            
            openGitattributesMenuItem = new ToolStripMenuItem("Open .gitattributes");
            openGitattributesMenuItem.Click += OnOpenGitattributesClick;
            openGitattributesMenuItem.ToolTipText = "Open .gitattributes file in the IDE editor";
            
            var closePaneMenuItem = new ToolStripMenuItem("Close Git Pane");
            closePaneMenuItem.Click += OnClosePaneClick;
            closePaneMenuItem.ToolTipText = "Hide the Git pane";
            fileMenu.DropDownItems.Add(initRepoMenuItem);
            fileMenu.DropDownItems.Add(openExternalMenuItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(openGitignoreMenuItem);
            fileMenu.DropDownItems.Add(openGitattributesMenuItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(closePaneMenuItem);
            
            // Repository menu
            repoMenu = new ToolStripDropDownButton("Repository");
            repoMenu.AutoToolTip = false;
            var refreshMenuItem = new ToolStripMenuItem("Refresh");
            refreshMenuItem.Click += OnRefreshClick;
            refreshMenuItem.ToolTipText = "Refresh file list and repository status";
            fetchMenuItem = new ToolStripMenuItem("Fetch");
            fetchMenuItem.Click += OnFetchClick;
            fetchMenuItem.ToolTipText = "Fetch changes from remote (doesn't merge)";
            pullMenuItem = new ToolStripMenuItem("Pull");
            pullMenuItem.Click += OnPullClick;
            pullMenuItem.ToolTipText = "Pull and merge changes from remote";
            pushMenuItem = new ToolStripMenuItem("Push");
            pushMenuItem.Click += OnPushClick;
            pushMenuItem.ToolTipText = "Push committed changes to remote";
            var historyMenuItem = new ToolStripMenuItem("View History...");
            historyMenuItem.Click += OnHistoryClick;
            historyMenuItem.ToolTipText = "View commit history and diffs";
            createGitHubMenuItem = new ToolStripMenuItem("Create GitHub Repository...");
            createGitHubMenuItem.Click += OnCreateGitHubRepoClick;
            createGitHubMenuItem.ToolTipText = "Create a new GitHub repository and set it as remote";
            addRemoteMenuItem = new ToolStripMenuItem("Add Remote...");
            addRemoteMenuItem.Click += OnAddRemoteClick;
            addRemoteMenuItem.ToolTipText = "Add a remote repository URL";
            removeRemoteMenuItem = new ToolStripMenuItem("Remove Remote...");
            removeRemoteMenuItem.Click += OnRemoveRemoteClick;
            removeRemoteMenuItem.ToolTipText = "Remove the remote repository connection";
            repoMenu.DropDownItems.Add(refreshMenuItem);
            repoMenu.DropDownItems.Add(fetchMenuItem);
            repoMenu.DropDownItems.Add(pullMenuItem);
            repoMenu.DropDownItems.Add(pushMenuItem);
            repoMenu.DropDownItems.Add(historyMenuItem);
            repoMenu.DropDownItems.Add(new ToolStripSeparator());
            applyTemplateMenuItem = new ToolStripMenuItem("Apply .gitignore/.gitattributes Template...");
            applyTemplateMenuItem.Click += OnApplyTemplateClick;
            applyTemplateMenuItem.ToolTipText = "Apply .gitignore or .gitattributes template to existing repository";
            repoMenu.DropDownItems.Add(applyTemplateMenuItem);
            repoMenu.DropDownItems.Add(new ToolStripSeparator());
            viewOnRemoteMenuItem = new ToolStripMenuItem("View on Remote");
            viewOnRemoteMenuItem.Click += OnViewOnRemoteClick;
            viewOnRemoteMenuItem.ToolTipText = "Open this repository in your web browser (GitHub/GitLab)";
            repoMenu.DropDownItems.Add(viewOnRemoteMenuItem);
            repoMenu.DropDownItems.Add(new ToolStripSeparator());
            repoMenu.DropDownItems.Add(createGitHubMenuItem);
            repoMenu.DropDownItems.Add(addRemoteMenuItem);
            repoMenu.DropDownItems.Add(removeRemoteMenuItem);
            
            // Branch menu
            branchMenu = new ToolStripDropDownButton("Branch");
            branchMenu.AutoToolTip = false;
            switchBranchMenuItem = new ToolStripMenuItem("Switch Branch...");
            switchBranchMenuItem.Click += OnBranchSelectClick;
            switchBranchMenuItem.ToolTipText = "Switch to a different branch";
            createBranchMenuItem = new ToolStripMenuItem("Create Branch...");
            createBranchMenuItem.Click += OnCreateBranchClick;
            createBranchMenuItem.ToolTipText = "Create a new branch from current HEAD";
            deleteBranchMenuItem = new ToolStripMenuItem("Delete Branch...");
            deleteBranchMenuItem.Click += OnDeleteBranchClick;
            deleteBranchMenuItem.ToolTipText = "Delete a local branch";
            mergeBranchMenuItem = new ToolStripMenuItem("Merge...");
            mergeBranchMenuItem.Click += OnMergeBranchClick;
            mergeBranchMenuItem.ToolTipText = "Merge another branch into current branch";
            branchMenu.DropDownItems.Add(switchBranchMenuItem);
            branchMenu.DropDownItems.Add(createBranchMenuItem);
            branchMenu.DropDownItems.Add(deleteBranchMenuItem);
            branchMenu.DropDownItems.Add(new ToolStripSeparator());
            branchMenu.DropDownItems.Add(mergeBranchMenuItem);
            
            // View menu
            viewMenu = new ToolStripDropDownButton("View");
            viewMenu.AutoToolTip = false;
            var showToolbarMenuItem = new ToolStripMenuItem("Show Toolbar");
            showToolbarMenuItem.CheckOnClick = true;
            showToolbarMenuItem.Checked = true;
            showToolbarMenuItem.Click += OnShowToolbarClick;
            showToolbarMenuItem.ToolTipText = "Toggle toolbar visibility";
            var showCommitAreaMenuItem = new ToolStripMenuItem("Show Commit Area");
            showCommitAreaMenuItem.CheckOnClick = true;
            showCommitAreaMenuItem.Checked = true;
            showCommitAreaMenuItem.Click += OnShowCommitAreaClick;
            showCommitAreaMenuItem.ToolTipText = "Toggle commit message area visibility";
            var resetLayoutMenuItem = new ToolStripMenuItem("Reset Split Layout");
            resetLayoutMenuItem.Click += OnResetLayoutClick;
            resetLayoutMenuItem.ToolTipText = "Reset splitter positions to default";
            viewMenu.DropDownItems.Add(showToolbarMenuItem);
            viewMenu.DropDownItems.Add(showCommitAreaMenuItem);
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(resetLayoutMenuItem);
            
            // Help menu
            helpMenu = new ToolStripDropDownButton("Help");
            helpMenu.AutoToolTip = false;
            var manageTemplatesMenuItem = new ToolStripMenuItem("Manage .gitignore/.gitattributes Templates...");
            manageTemplatesMenuItem.Click += OnManageTemplatesClick;
            manageTemplatesMenuItem.ToolTipText = "Manage .gitignore and .gitattributes templates";
            var aboutMenuItem = new ToolStripMenuItem("About GitPane");
            aboutMenuItem.Click += OnAboutClick;
            aboutMenuItem.ToolTipText = "View GitPane version and information";
            helpMenu.DropDownItems.Add(manageTemplatesMenuItem);
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add(aboutMenuItem);
            
            // Add menu dropdowns to ToolStrip
            toolStrip.Items.Add(initRepoMenuButton);
            toolStrip.Items.Add(fileMenu);
            toolStrip.Items.Add(repoMenu);
            toolStrip.Items.Add(branchMenu);
            toolStrip.Items.Add(viewMenu);
            toolStrip.Items.Add(helpMenu);
            toolStrip.Items.Add(new ToolStripSeparator());
            
            // Branch dropdown button (with separator for visual grouping)
            branchDropDown = new ToolStripDropDownButton();
            branchDropDown.Text = "No branch";
            branchDropDown.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);
            branchDropDown.ForeColor = Color.DarkBlue;
            branchDropDown.DisplayStyle = ToolStripItemDisplayStyle.Text;
            branchDropDown.ToolTipText = "Switch branches or manage branch options";
            branchDropDown.DropDownOpening += OnBranchDropDownOpening;
            
            // Add branch dropdown to toolbar
            toolStrip.Items.Add(branchDropDown);
            
            // Inert spacer panel for WinForms docking stability
            topSpacerPanel = new Panel();
            topSpacerPanel.Dock = DockStyle.Top;
            topSpacerPanel.Height = 6;
            
            // Commit container - holds editor area and buttons in lower splitter
            commitContainer = new Panel();
            commitContainer.Dock = DockStyle.Fill;
            commitContainer.Padding = new Padding(10);
            
            // Commit panel - editor area (label + textbox)
            commitPanel = new Panel();
            commitPanel.Dock = DockStyle.Fill;

            // Main horizontal splitter - staged vs (unstaged + commit)
            mainSplitter = new SplitContainer();
            mainSplitter.Dock = DockStyle.Fill;
            mainSplitter.Orientation = Orientation.Horizontal;
            // Don't set SplitterDistance here - will be set dynamically after layout
            
            // Lower horizontal splitter - unstaged vs commit
            lowerSplitter = new SplitContainer();
            lowerSplitter.Dock = DockStyle.Fill;
            lowerSplitter.Orientation = Orientation.Horizontal;
            // Don't set SplitterDistance here - will be set dynamically after layout
            
            // Staged files section - in top panel of splitter
            stagedGroupBox = new GroupBox();
            stagedGroupBox.Text = "Staged Files (0)";
            stagedGroupBox.Dock = DockStyle.Fill;

            // ToolStrip for staged file actions
            stagedToolStrip = new ToolStrip();
            stagedToolStrip.Dock = DockStyle.Bottom;
            stagedToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            stagedToolStrip.Padding = new Padding(5, 2, 5, 2);

            stagedListBox = new CheckedListBox();
            stagedListBox.Dock = DockStyle.Fill;
            stagedListBox.Font = new Font("Courier New", 8F);
            stagedListBox.CheckOnClick = true;
            stagedListBox.ItemCheck += OnStagedListBox_ItemCheck;

            unstageSelectedButton = new ToolStripButton("Unstage Selected");
            unstageSelectedButton.Click += OnUnstageSelectedClick;
            unstageSelectedButton.ToolTipText = "Unstage selected files (remove from commit)";

            unstageAllButton = new ToolStripButton("Unstage All");
            unstageAllButton.Click += OnUnstageAllClick;
            unstageAllButton.ToolTipText = "Unstage all files (remove all from commit)";

            stagedToolStrip.Items.Add(unstageSelectedButton);
            stagedToolStrip.Items.Add(unstageAllButton);

            // FIX: Add Fill FIRST, then Bottom (z-order matters in WinForms)
            stagedGroupBox.Controls.Add(stagedListBox);     // Fill first
            stagedGroupBox.Controls.Add(stagedToolStrip);   // Bottom last
            
            // Add staged groupbox to top panel of splitter
            mainSplitter.Panel1.Controls.Add(stagedGroupBox);

            // Unstaged files section - in bottom panel of splitter
            unstagedGroupBox = new GroupBox();
            unstagedGroupBox.Text = "Unstaged Files (0)";
            unstagedGroupBox.Dock = DockStyle.Fill;

            // ToolStrip for unstaged file actions
            unstagedToolStrip = new ToolStrip();
            unstagedToolStrip.Dock = DockStyle.Bottom;
            unstagedToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            unstagedToolStrip.Padding = new Padding(5, 2, 5, 2);

            unstagedListBox = new CheckedListBox();
            unstagedListBox.Dock = DockStyle.Fill;
            unstagedListBox.Font = new Font("Courier New", 8F);
            unstagedListBox.CheckOnClick = true;
            unstagedListBox.ItemCheck += OnUnstagedListBox_ItemCheck;
            unstagedListBox.MouseDown += OnUnstagedListBoxMouseDown;

            // Context menu for unstaged files
            unstagedContextMenu = new ContextMenuStrip();
            var ignoreFileItem = new ToolStripMenuItem("Ignore this file");
            ignoreFileItem.Click += OnIgnoreFileClick;
            var ignoreExtensionItem = new ToolStripMenuItem("Ignore file type (*.[ext])");
            ignoreExtensionItem.Click += OnIgnoreExtensionClick;
            var ignoreDirectoryItem = new ToolStripMenuItem("Ignore directory");
            // Note: Directory submenu items will be populated dynamically in OnUnstagedContextMenuOpening
            unstagedContextMenu.Items.Add(ignoreFileItem);
            unstagedContextMenu.Items.Add(ignoreExtensionItem);
            unstagedContextMenu.Items.Add(ignoreDirectoryItem);
            unstagedContextMenu.Opening += OnUnstagedContextMenuOpening;
            unstagedListBox.ContextMenuStrip = unstagedContextMenu;

            stageSelectedButton = new ToolStripButton("Stage Selected");
            stageSelectedButton.Click += OnStageSelectedClick;
            stageSelectedButton.ToolTipText = "Stage selected files for commit";

            stageAllButton = new ToolStripButton("Stage All");
            stageAllButton.Click += OnStageAllClick;
            stageAllButton.ToolTipText = "Stage all unstaged files for commit";

            ToolStripSeparator unstagedSeparator = new ToolStripSeparator();
            unstagedSeparator.Margin = new Padding(5, 0, 5, 0);

            discardSelectedButton = new ToolStripButton("Discard Selected");
            discardSelectedButton.Click += OnDiscardSelectedClick;
            discardSelectedButton.ToolTipText = "Discard changes to selected files (cannot be undone)";

            discardAllButton = new ToolStripButton("Discard All");
            discardAllButton.Click += OnDiscardAllClick;
            discardAllButton.ToolTipText = "Discard all unstaged changes (cannot be undone)";

            unstagedToolStrip.Items.Add(stageSelectedButton);
            unstagedToolStrip.Items.Add(stageAllButton);
            unstagedToolStrip.Items.Add(unstagedSeparator);
            unstagedToolStrip.Items.Add(discardSelectedButton);
            unstagedToolStrip.Items.Add(discardAllButton);

            // FIX: Add Fill FIRST, then Bottom (z-order matters in WinForms)
            unstagedGroupBox.Controls.Add(unstagedListBox);     // Fill first
            unstagedGroupBox.Controls.Add(unstagedToolStrip);   // Bottom last
            
            // Add unstaged groupbox to top panel of lower splitter
            lowerSplitter.Panel1.Controls.Add(unstagedGroupBox);

            // Commit message section - using Dock instead of Anchor for reliable positioning
            commitMessageLabel = new Label();
            commitMessageLabel.Text = "Commit Message:";
            commitMessageLabel.Dock = DockStyle.Top;
            commitMessageLabel.Height = 20;
            commitMessageLabel.TextAlign = ContentAlignment.MiddleLeft;

            commitMessageBox = new TextBox();
            commitMessageBox.Multiline = true;
            commitMessageBox.Dock = DockStyle.Fill;
            commitMessageBox.ScrollBars = ScrollBars.Vertical;
            commitMessageBox.TextChanged += OnCommitMessageBox_TextChanged;

            // ToolStrip for commit buttons - docked to bottom of commit container
            commitToolStrip = new ToolStrip();
            commitToolStrip.Dock = DockStyle.Bottom;
            commitToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            commitToolStrip.Padding = new Padding(5, 2, 5, 2);

            // Commit buttons
            commitButton = new ToolStripButton("Commit");
            commitButton.Click += OnCommitClick;
            commitButton.ToolTipText = "Commit staged files to local repository";

            commitPushButton = new ToolStripButton("Commit && Push");
            commitPushButton.Click += OnCommitPushClick;
            commitPushButton.ToolTipText = "Commit staged files and push to remote";

            // Push button (for unpushed commits)
            pushButton = new ToolStripButton("Push");
            pushButton.Click += OnPushClick;
            pushButton.Visible = false;
            pushButton.ToolTipText = "Push committed changes to remote repository";
            
            commitToolStrip.Items.Add(commitButton);
            commitToolStrip.Items.Add(commitPushButton);
            commitToolStrip.Items.Add(pushButton);
            
            // Add to commit panel (editor area) - label on top, textbox fills
            commitPanel.Controls.Add(commitMessageBox);   // Fill
            commitPanel.Controls.Add(commitMessageLabel); // Top (added last for z-order)
            
            // Add editor area (Fill) and toolbar (Bottom) to commit container
            commitContainer.Controls.Add(commitPanel);    // Fill first
            commitContainer.Controls.Add(commitToolStrip);    // Bottom last
            
            // Add commit container to bottom panel of lower splitter
            lowerSplitter.Panel2.Controls.Add(commitContainer);
            
            // Add lower splitter to bottom panel of main splitter
            mainSplitter.Panel2.Controls.Add(lowerSplitter);

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

            // Stash management panel - docked to bottom, hidden when no stashes
            stashGroupBox = new GroupBox();
            stashGroupBox.Text = "Stashes (0)";
            stashGroupBox.Dock = DockStyle.Bottom;
            stashGroupBox.Height = 130;
            stashGroupBox.Visible = false;

            stashListView = new ListView();
            stashListView.Dock = DockStyle.Fill;
            stashListView.View = View.Details;
            stashListView.FullRowSelect = true;
            stashListView.MultiSelect = false;
            stashListView.Font = new Font("Courier New", 8F);
            stashListView.Columns.Add("Ref", 80);
            stashListView.Columns.Add("Message", 260);
            stashListView.Columns.Add("When", 100);
            stashListView.SelectedIndexChanged += OnStashSelectionChanged;

            stashToolStrip = new ToolStrip();
            stashToolStrip.Dock = DockStyle.Bottom;
            stashToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            stashToolStrip.Padding = new Padding(5, 2, 5, 2);

            newStashButton   = new ToolStripButton("New Stash");
            applyStashButton = new ToolStripButton("Apply");
            popStashButton   = new ToolStripButton("Pop");
            dropStashButton  = new ToolStripButton("Drop");
            dropStashButton.ForeColor = Color.DarkRed;

            applyStashButton.Enabled = false;
            popStashButton.Enabled   = false;
            dropStashButton.Enabled  = false;

            newStashButton.Click   += OnNewStashClick;
            applyStashButton.Click += OnApplyStashClick;
            popStashButton.Click   += OnPopStashClick;
            dropStashButton.Click  += OnDropStashClick;

            stashToolStrip.Items.Add(newStashButton);
            stashToolStrip.Items.Add(new ToolStripSeparator());
            stashToolStrip.Items.Add(applyStashButton);
            stashToolStrip.Items.Add(popStashButton);
            stashToolStrip.Items.Add(dropStashButton);

            stashGroupBox.Controls.Add(stashListView);
            stashGroupBox.Controls.Add(stashToolStrip);

            // Add controls to contentPanel in correct docking order
            // Dock.Fill first, then Dock.Top in reverse visual order (last added = top position)
            contentPanel.Controls.Add(mainSplitter);   // Dock.Fill - fills remaining space
            contentPanel.Controls.Add(stashGroupBox);  // Dock.Bottom - stash panel at bottom
            contentPanel.Controls.Add(topSpacerPanel); // Dock.Top - visual separator (appears below toolStrip)
            contentPanel.Controls.Add(toolStrip);      // Dock.Top - combined menu and toolbar
            contentPanel.Controls.Add(statusLabel);
            contentPanel.Controls.Add(initRepoButton);
        }
        
        #endregion
    }
}

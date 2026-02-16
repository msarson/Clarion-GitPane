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
        
        // Menu and toolbar controls
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private Label branchValueLabel;
        private ToolStripButton branchSelectButton;
        
        // Menu items that need context-aware visibility
        private ToolStripMenuItem initRepoMenuItem;
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
        
        private Label commitMessageLabel;
        private TextBox commitMessageBox;
        private ToolStrip commitToolStrip;
        private ToolStripButton commitButton;
        private ToolStripButton commitPushButton;
        private ToolStripButton pushButton;
        
        #endregion
        
        #region UI Initialization
        
        private void InitializeUI()
        {
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = SystemColors.Window;
            contentPanel.Padding = new Padding(10);
            contentPanel.AutoScroll = false; // FIX: Disable AutoScroll - incompatible with Dock.Fill SplitContainer

            // MenuStrip for main menu
            menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Top;
            
            // File menu
            var fileMenu = new ToolStripMenuItem("&File");
            initRepoMenuItem = new ToolStripMenuItem("Initialize Repository...");
            initRepoMenuItem.Click += OnInitRepoClick;
            var openExternalMenu = new ToolStripMenuItem("Open in External Tool");
            var openGitHubDesktopMenuItem = new ToolStripMenuItem("GitHub Desktop");
            openGitHubDesktopMenuItem.Click += OnOpenGitHubDesktopClick;
            var openGitKrakenMenuItem = new ToolStripMenuItem("GitKraken");
            openGitKrakenMenuItem.Click += OnOpenGitKrakenClick;
            openExternalMenu.DropDownItems.Add(openGitHubDesktopMenuItem);
            openExternalMenu.DropDownItems.Add(openGitKrakenMenuItem);
            var closePaneMenuItem = new ToolStripMenuItem("Close Git Pane");
            closePaneMenuItem.Click += OnClosePaneClick;
            fileMenu.DropDownItems.Add(initRepoMenuItem);
            fileMenu.DropDownItems.Add(openExternalMenu);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(closePaneMenuItem);
            
            // Repository menu
            var repoMenu = new ToolStripMenuItem("&Repository");
            var refreshMenuItem = new ToolStripMenuItem("Refresh");
            refreshMenuItem.Click += OnRefreshClick;
            fetchMenuItem = new ToolStripMenuItem("Fetch");
            fetchMenuItem.Click += OnFetchClick;
            pullMenuItem = new ToolStripMenuItem("Pull");
            pullMenuItem.Click += OnPullClick;
            pushMenuItem = new ToolStripMenuItem("Push");
            pushMenuItem.Click += OnPushClick;
            var historyMenuItem = new ToolStripMenuItem("View History...");
            historyMenuItem.Click += OnHistoryClick;
            createGitHubMenuItem = new ToolStripMenuItem("Create GitHub Repository...");
            createGitHubMenuItem.Click += OnCreateGitHubRepoClick;
            addRemoteMenuItem = new ToolStripMenuItem("Add Remote...");
            addRemoteMenuItem.Click += OnAddRemoteClick;
            removeRemoteMenuItem = new ToolStripMenuItem("Remove Remote...");
            removeRemoteMenuItem.Click += OnRemoveRemoteClick;
            repoMenu.DropDownItems.Add(refreshMenuItem);
            repoMenu.DropDownItems.Add(fetchMenuItem);
            repoMenu.DropDownItems.Add(pullMenuItem);
            repoMenu.DropDownItems.Add(pushMenuItem);
            repoMenu.DropDownItems.Add(historyMenuItem);
            repoMenu.DropDownItems.Add(new ToolStripSeparator());
            viewOnRemoteMenuItem = new ToolStripMenuItem("View on Remote");
            viewOnRemoteMenuItem.Click += OnViewOnRemoteClick;
            repoMenu.DropDownItems.Add(viewOnRemoteMenuItem);
            repoMenu.DropDownItems.Add(new ToolStripSeparator());
            repoMenu.DropDownItems.Add(createGitHubMenuItem);
            repoMenu.DropDownItems.Add(addRemoteMenuItem);
            repoMenu.DropDownItems.Add(removeRemoteMenuItem);
            
            // Branch menu
            var branchMenu = new ToolStripMenuItem("&Branch");
            switchBranchMenuItem = new ToolStripMenuItem("Switch Branch...");
            switchBranchMenuItem.Click += OnBranchSelectClick;
            createBranchMenuItem = new ToolStripMenuItem("Create Branch...");
            createBranchMenuItem.Click += OnCreateBranchClick;
            deleteBranchMenuItem = new ToolStripMenuItem("Delete Branch...");
            deleteBranchMenuItem.Click += OnDeleteBranchClick;
            mergeBranchMenuItem = new ToolStripMenuItem("Merge...");
            mergeBranchMenuItem.Click += OnMergeBranchClick;
            branchMenu.DropDownItems.Add(switchBranchMenuItem);
            branchMenu.DropDownItems.Add(createBranchMenuItem);
            branchMenu.DropDownItems.Add(deleteBranchMenuItem);
            branchMenu.DropDownItems.Add(new ToolStripSeparator());
            branchMenu.DropDownItems.Add(mergeBranchMenuItem);
            
            // View menu
            var viewMenu = new ToolStripMenuItem("&View");
            var showToolbarMenuItem = new ToolStripMenuItem("Show Toolbar");
            showToolbarMenuItem.CheckOnClick = true;
            showToolbarMenuItem.Checked = true;
            showToolbarMenuItem.Click += OnShowToolbarClick;
            var showCommitAreaMenuItem = new ToolStripMenuItem("Show Commit Area");
            showCommitAreaMenuItem.CheckOnClick = true;
            showCommitAreaMenuItem.Checked = true;
            showCommitAreaMenuItem.Click += OnShowCommitAreaClick;
            var resetLayoutMenuItem = new ToolStripMenuItem("Reset Split Layout");
            resetLayoutMenuItem.Click += OnResetLayoutClick;
            viewMenu.DropDownItems.Add(showToolbarMenuItem);
            viewMenu.DropDownItems.Add(showCommitAreaMenuItem);
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(resetLayoutMenuItem);
            
            // Help menu
            var helpMenu = new ToolStripMenuItem("&Help");
            var aboutMenuItem = new ToolStripMenuItem("About GitPane");
            aboutMenuItem.Click += OnAboutClick;
            helpMenu.DropDownItems.Add(aboutMenuItem);
            
            // Add menus to MenuStrip
            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(repoMenu);
            menuStrip.Items.Add(branchMenu);
            menuStrip.Items.Add(viewMenu);
            menuStrip.Items.Add(helpMenu);

            // ToolStrip for top controls - professional layout
            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Padding = new Padding(5, 2, 5, 2);
            
            // Branch info
            ToolStripLabel branchToolLabel = new ToolStripLabel("Branch:");
            branchValueLabel = new Label();
            branchValueLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);
            branchValueLabel.ForeColor = Color.DarkBlue;
            branchValueLabel.AutoSize = true;
            branchValueLabel.Margin = new Padding(0, 0, 5, 0);
            ToolStripControlHost branchValueHost = new ToolStripControlHost(branchValueLabel);
            
            branchSelectButton = new ToolStripButton("...");
            branchSelectButton.Click += OnBranchSelectClick;
            branchSelectButton.Margin = new Padding(0, 1, 3, 2);
            
            // Add items to toolbar (simple: just branch info)
            toolStrip.Items.Add(branchToolLabel);
            toolStrip.Items.Add(branchValueHost);
            toolStrip.Items.Add(branchSelectButton);
            
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

            unstageAllButton = new ToolStripButton("Unstage All");
            unstageAllButton.Click += OnUnstageAllClick;

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
            ignoreDirectoryItem.Click += OnIgnoreDirectoryClick;
            unstagedContextMenu.Items.Add(ignoreFileItem);
            unstagedContextMenu.Items.Add(ignoreExtensionItem);
            unstagedContextMenu.Items.Add(ignoreDirectoryItem);
            unstagedContextMenu.Opening += OnUnstagedContextMenuOpening;
            unstagedListBox.ContextMenuStrip = unstagedContextMenu;

            stageSelectedButton = new ToolStripButton("Stage Selected");
            stageSelectedButton.Click += OnStageSelectedClick;

            stageAllButton = new ToolStripButton("Stage All");
            stageAllButton.Click += OnStageAllClick;

            ToolStripSeparator unstagedSeparator = new ToolStripSeparator();
            unstagedSeparator.Margin = new Padding(5, 0, 5, 0);

            discardSelectedButton = new ToolStripButton("Discard Selected");
            discardSelectedButton.Click += OnDiscardSelectedClick;

            discardAllButton = new ToolStripButton("Discard All");
            discardAllButton.Click += OnDiscardAllClick;

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

            commitPushButton = new ToolStripButton("Commit && Push");
            commitPushButton.Click += OnCommitPushClick;

            // Push button (for unpushed commits)
            pushButton = new ToolStripButton("Push");
            pushButton.Click += OnPushClick;
            pushButton.Visible = false;
            
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

            // Add controls to contentPanel in correct docking order
            // Fill first, then Top (multiple Top items dock in reverse order)
            contentPanel.Controls.Add(mainSplitter);  // Dock.Fill - nested splitters (staged / unstaged / commit)
            contentPanel.Controls.Add(toolStrip);     // Dock.Top - toolbar
            contentPanel.Controls.Add(menuStrip);     // Dock.Top - menu (added last, appears at top)
            contentPanel.Controls.Add(statusLabel);
            contentPanel.Controls.Add(initRepoButton);
        }
        
        #endregion
    }
}

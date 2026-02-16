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
        
        // Toolbar controls
        private ToolStrip toolStrip;
        private Label branchValueLabel;
        private ToolStripButton branchSelectButton;
        private Label remoteLabel;
        private ToolStripButton removeRemoteButton;
        private ToolStripButton addRemoteButton;
        private ToolStripButton createGitHubRepoButton;
        private ToolStripButton historyButton;
        private ToolStripButton refreshButton;
        
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
            
            // Separator
            ToolStripSeparator separator1 = new ToolStripSeparator();
            separator1.Margin = new Padding(5, 0, 5, 0);
            
            // Remote info
            remoteLabel = new Label();
            remoteLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 8F);
            remoteLabel.ForeColor = SystemColors.GrayText;
            remoteLabel.AutoSize = true;
            remoteLabel.MaximumSize = new Size(300, 0);
            remoteLabel.Margin = new Padding(0, 0, 5, 0);
            ToolStripControlHost remoteLabelHost = new ToolStripControlHost(remoteLabel);
            
            // Remote buttons
            removeRemoteButton = new ToolStripButton("×");
            removeRemoteButton.Font = new Font(SystemFonts.DefaultFont.FontFamily, 10F, FontStyle.Bold);
            removeRemoteButton.Click += OnRemoveRemoteClick;
            removeRemoteButton.Visible = false;
            removeRemoteButton.Margin = new Padding(0, 1, 3, 2);
            
            addRemoteButton = new ToolStripButton("Add Remote");
            addRemoteButton.Click += OnAddRemoteClick;
            addRemoteButton.Visible = false;
            addRemoteButton.Margin = new Padding(0, 1, 3, 2);
            
            createGitHubRepoButton = new ToolStripButton("Create on GitHub");
            createGitHubRepoButton.Click += OnCreateGitHubRepoClick;
            createGitHubRepoButton.Visible = false;
            createGitHubRepoButton.Margin = new Padding(0, 1, 3, 2);
            
            // Separator
            ToolStripSeparator separator2 = new ToolStripSeparator();
            separator2.Margin = new Padding(5, 0, 5, 0);
            
            // History button
            historyButton = new ToolStripButton("History");
            historyButton.Click += OnHistoryClick;
            historyButton.Margin = new Padding(0, 1, 3, 2);
            
            // Refresh button
            refreshButton = new ToolStripButton("Refresh");
            refreshButton.Click += OnRefreshClick;
            refreshButton.Margin = new Padding(0, 1, 3, 2);
            
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
            toolStrip.Items.Add(historyButton);
            toolStrip.Items.Add(refreshButton);
            
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
            // Fill first, then Top
            contentPanel.Controls.Add(mainSplitter);  // Dock.Fill - nested splitters (staged / unstaged / commit)
            contentPanel.Controls.Add(toolStrip);     // Dock.Top - toolbar at top
            contentPanel.Controls.Add(statusLabel);
            contentPanel.Controls.Add(initRepoButton);
        }
        
        #endregion
    }
}

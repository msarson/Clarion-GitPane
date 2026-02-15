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
        private Label repoInfoLabel;
        private Label branchLabel;
        private ComboBox branchComboBox;
        private Label statusLabel;
        private GitRepository gitRepo;

        public override Control Control => contentPanel;

        public GitPanePad()
        {
            InitializeUI();
            UpdateStatus();

            // Subscribe to solution events
            ProjectService.SolutionLoaded += OnSolutionChanged;
            ProjectService.SolutionClosed += OnSolutionClosed;
        }

        private void InitializeUI()
        {
            contentPanel = new Panel();
            contentPanel.BackColor = SystemColors.Window;
            contentPanel.Padding = new Padding(10);
            contentPanel.AutoScroll = true;

            // Repository name label
            repoInfoLabel = new Label();
            repoInfoLabel.AutoSize = true;
            repoInfoLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 11F, FontStyle.Bold);
            repoInfoLabel.ForeColor = SystemColors.ControlText;
            repoInfoLabel.Location = new Point(10, 10);

            // Branch label
            branchLabel = new Label();
            branchLabel.AutoSize = true;
            branchLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F);
            branchLabel.ForeColor = SystemColors.ControlText;
            branchLabel.Location = new Point(10, 35);
            branchLabel.Text = "Branch:";

            // Branch dropdown
            branchComboBox = new ComboBox();
            branchComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            branchComboBox.Location = new Point(60, 32);
            branchComboBox.Width = 200;
            branchComboBox.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F);
            branchComboBox.SelectedIndexChanged += OnBranchSelected;

            // Status label
            statusLabel = new Label();
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F);
            statusLabel.ForeColor = SystemColors.GrayText;
            statusLabel.Location = new Point(10, 65);

            contentPanel.Controls.Add(repoInfoLabel);
            contentPanel.Controls.Add(branchLabel);
            contentPanel.Controls.Add(branchComboBox);
            contentPanel.Controls.Add(statusLabel);
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

                    repoInfoLabel.Text = $"Repository: {repoName}";
                    statusLabel.Text = $"Path: {solutionDir}";

                    // Populate branch dropdown
                    branchComboBox.SelectedIndexChanged -= OnBranchSelected;
                    branchComboBox.Items.Clear();
                    
                    var branches = gitRepo.GetAllBranches();
                    foreach (var branch in branches)
                    {
                        branchComboBox.Items.Add(branch);
                    }

                    // Select current branch
                    if (currentBranch != null && !currentBranch.StartsWith("HEAD"))
                    {
                        int index = branchComboBox.Items.IndexOf(currentBranch);
                        if (index >= 0)
                            branchComboBox.SelectedIndex = index;
                    }
                    else
                    {
                        // Detached HEAD state - show in label
                        branchComboBox.Items.Insert(0, currentBranch ?? "unknown");
                        branchComboBox.SelectedIndex = 0;
                    }

                    branchComboBox.SelectedIndexChanged += OnBranchSelected;

                    repoInfoLabel.Visible = true;
                    branchLabel.Visible = true;
                    branchComboBox.Visible = true;
                    statusLabel.Visible = true;
                }
                else
                {
                    repoInfoLabel.Text = "Not a Git repository";
                    branchLabel.Visible = false;
                    branchComboBox.Visible = false;
                    statusLabel.Text = $"Path: {solutionDir}";
                    repoInfoLabel.Visible = true;
                    statusLabel.Visible = true;
                }
            }
            else
            {
                repoInfoLabel.Text = "No solution opened";
                repoInfoLabel.Visible = true;
                branchLabel.Visible = false;
                branchComboBox.Visible = false;
                statusLabel.Visible = false;
            }
        }

        private void OnBranchSelected(object sender, EventArgs e)
        {
            if (branchComboBox.SelectedItem == null || gitRepo == null)
                return;

            string selectedBranch = branchComboBox.SelectedItem.ToString();
            string currentBranch = gitRepo.GetCurrentBranch();

            // Don't switch if already on this branch
            if (selectedBranch == currentBranch)
                return;

            // Don't allow switching from detached HEAD entries
            if (selectedBranch.StartsWith("HEAD"))
                return;

            if (MessageBox.Show(
                $"Switch to branch '{selectedBranch}'?",
                "Switch Branch",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (gitRepo.CheckoutBranch(selectedBranch))
                {
                    MessageBox.Show($"Switched to branch '{selectedBranch}'", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatus();
                }
                else
                {
                    MessageBox.Show($"Failed to switch to branch '{selectedBranch}'.\n\nYou may have uncommitted changes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus(); // Refresh to restore correct selection
                }
            }
            else
            {
                UpdateStatus(); // Restore previous selection if user cancels
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
            contentPanel?.Dispose();
            base.Dispose();
        }
    }
}

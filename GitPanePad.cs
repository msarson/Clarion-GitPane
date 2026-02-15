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
        private Label branchValueLabel;
        private Button branchSelectButton;
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

            // Branch value label
            branchValueLabel = new Label();
            branchValueLabel.AutoSize = true;
            branchValueLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);
            branchValueLabel.ForeColor = Color.DarkBlue;
            branchValueLabel.Location = new Point(60, 35);

            // Branch select button
            branchSelectButton = new Button();
            branchSelectButton.Text = "...";
            branchSelectButton.Width = 30;
            branchSelectButton.Height = 23;
            branchSelectButton.Location = new Point(200, 32);
            branchSelectButton.Click += OnBranchSelectClick;

            // Status label
            statusLabel = new Label();
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F);
            statusLabel.ForeColor = SystemColors.GrayText;
            statusLabel.Location = new Point(10, 65);

            contentPanel.Controls.Add(repoInfoLabel);
            contentPanel.Controls.Add(branchLabel);
            contentPanel.Controls.Add(branchValueLabel);
            contentPanel.Controls.Add(branchSelectButton);
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
                    branchValueLabel.Text = currentBranch ?? "unknown";
                    statusLabel.Text = $"Path: {solutionDir}";

                    repoInfoLabel.Visible = true;
                    branchLabel.Visible = true;
                    branchValueLabel.Visible = true;
                    branchSelectButton.Visible = true;
                    statusLabel.Visible = true;
                }
                else
                {
                    repoInfoLabel.Text = "Not a Git repository";
                    branchLabel.Visible = false;
                    branchValueLabel.Visible = false;
                    branchSelectButton.Visible = false;
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
                branchValueLabel.Visible = false;
                branchSelectButton.Visible = false;
                statusLabel.Visible = false;
            }
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
                        }
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
            contentPanel?.Dispose();
            base.Dispose();
        }
    }
}

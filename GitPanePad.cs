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
            branchLabel.ForeColor = Color.DarkBlue;
            branchLabel.Location = new Point(10, 35);

            // Status label
            statusLabel = new Label();
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F);
            statusLabel.ForeColor = SystemColors.GrayText;
            statusLabel.Location = new Point(10, 60);

            contentPanel.Controls.Add(repoInfoLabel);
            contentPanel.Controls.Add(branchLabel);
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
                    string branch = gitRepo.GetCurrentBranch();

                    repoInfoLabel.Text = $"Repository: {repoName}";
                    branchLabel.Text = $"Branch: {branch ?? "unknown"}";
                    statusLabel.Text = $"Path: {solutionDir}";

                    repoInfoLabel.Visible = true;
                    branchLabel.Visible = true;
                }
                else
                {
                    repoInfoLabel.Text = "Not a Git repository";
                    branchLabel.Visible = false;
                    statusLabel.Text = $"Path: {solutionDir}";
                    repoInfoLabel.Visible = true;
                }
                statusLabel.Visible = true;
            }
            else
            {
                repoInfoLabel.Text = "No solution opened";
                repoInfoLabel.Visible = true;
                branchLabel.Visible = false;
                statusLabel.Visible = false;
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

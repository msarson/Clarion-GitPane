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
            
            var message = $"You have uncommitted changes:\n\n{changesStatus}\n\nWhat would you like to do?";
            
            using (var dialog = new Form())
            {
                dialog.Text = "Uncommitted Changes";
                dialog.Size = new Size(500, 400);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var textBox = new TextBox();
                textBox.Multiline = true;
                textBox.ReadOnly = true;
                textBox.ScrollBars = ScrollBars.Both;
                textBox.Location = new Point(10, 10);
                textBox.Size = new Size(460, 240);
                textBox.Text = message;
                textBox.Font = new Font("Courier New", 9F);

                var stashButton = new Button();
                stashButton.Text = "Stash";
                stashButton.Location = new Point(10, 260);
                stashButton.Width = 90;
                stashButton.Click += (s, ev) => { dialog.Tag = "stash"; dialog.Close(); };

                var commitButton = new Button();
                commitButton.Text = "Commit";
                commitButton.Location = new Point(105, 260);
                commitButton.Width = 90;
                commitButton.Click += (s, ev) => { dialog.Tag = "commit"; dialog.Close(); };

                var commitPushButton = new Button();
                commitPushButton.Text = "Commit && Push";
                commitPushButton.Location = new Point(200, 260);
                commitPushButton.Width = 110;
                commitPushButton.Click += (s, ev) => { dialog.Tag = "commitpush"; dialog.Close(); };

                var discardButton = new Button();
                discardButton.Text = "Discard";
                discardButton.Location = new Point(315, 260);
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
                cancelButton.Location = new Point(395, 260);
                cancelButton.Width = 75;
                cancelButton.Click += (s, ev) => { dialog.Tag = "cancel"; dialog.Close(); };

                dialog.Controls.Add(textBox);
                dialog.Controls.Add(stashButton);
                dialog.Controls.Add(commitButton);
                dialog.Controls.Add(commitPushButton);
                dialog.Controls.Add(discardButton);
                dialog.Controls.Add(cancelButton);

                dialog.ShowDialog();

                string action = dialog.Tag as string;

                if (action == "stash")
                {
                    if (gitRepo.StashChanges($"Before switching to {targetBranch}"))
                    {
                        if (gitRepo.CheckoutBranch(targetBranch))
                        {
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

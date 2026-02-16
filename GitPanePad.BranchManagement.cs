using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    /// <summary>
    /// GitPanePad - Branch Management
    /// </summary>
    public partial class GitPanePad
    {
        #region Branch Management

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
                            var pushResult = gitRepo.PushChanges();
                            if (pushResult.ExitCode != 0)
                            {
                                var errorText = pushResult.Error + "\n" + pushResult.Output;
                                if (errorText.Contains("Authentication failed") || 
                                    errorText.Contains("Invalid username or token") ||
                                    errorText.Contains("Password authentication is not supported"))
                                {
                                    MessageBox.Show("Committed locally but push failed (authentication error). You can push manually later.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show("Committed locally but push failed. You can push manually later.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
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

        #endregion
    }
}

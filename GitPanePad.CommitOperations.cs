using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    /// <summary>
    /// GitPanePad - Commit Operations
    /// </summary>
    public partial class GitPanePad
    {
        #region Event Handlers - Commit and Push

        private void OnCommitClick(object sender, EventArgs e)
        {
            PerformCommit(false);
        }

        private void OnCommitPushClick(object sender, EventArgs e)
        {
            PerformCommit(true);
        }

        private void OnPushClick(object sender, EventArgs e)
        {
            if (gitRepo == null)
                return;

            int unpushedCount = gitRepo.GetUnpushedCommitsCount();
            
            var result = MessageBox.Show(
                $"Push {unpushedCount} local commit{(unpushedCount > 1 ? "s" : "")} to remote?",
                "Push Commits",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (gitRepo.PushChanges())
                {
                    MessageBox.Show("Commits pushed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshFileList(); // Update push button visibility
                }
                else
                {
                    MessageBox.Show("Failed to push commits. Check your connection and credentials.", "Push Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
                // Check if there are unstaged files to stage
                if (unstagedListBox.Items.Count > 0)
                {
                    var result = MessageBox.Show(
                        "No files are staged for commit.\n\nWould you like to stage all files and commit?",
                        "Nothing Staged",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        // Stage all files first
                        if (!gitRepo.StageAllFiles())
                        {
                            MessageBox.Show("Failed to stage files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        
                        // Refresh to update staged list
                        RefreshFileList();
                        
                        // Continue with commit (check again after staging)
                        if (stagedListBox.Items.Count == 0)
                        {
                            MessageBox.Show("No files to commit.", "Nothing to Commit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    else
                    {
                        return; // User cancelled
                    }
                }
                else
                {
                    MessageBox.Show("No files to commit.", "Nothing to Commit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
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

        #endregion
    }
}

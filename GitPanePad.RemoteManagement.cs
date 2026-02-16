using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    /// <summary>
    /// GitPanePad - Remote Management
    /// </summary>
    public partial class GitPanePad
    {
        #region Remote Management

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
                    UpdateMenuStates();
                    UpdateStatus(); // Refresh to update repo name if it changed
                }
                else
                {
                    MessageBox.Show("Failed to add remote. The remote may already exist or the URL is invalid.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnRemoveRemoteClick(object sender, EventArgs e)
        {
            if (gitRepo == null)
                return;

            string remoteUrl = gitRepo.GetRemoteUrl();
            
            var result = MessageBox.Show(
                $"Remove remote 'origin'?\n\nURL: {remoteUrl}\n\nThis will remove the remote from your local Git config.\nThe remote repository on GitHub/Bitbucket will NOT be deleted.",
                "Remove Remote",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (gitRepo.RemoveRemote("origin"))
                {
                    MessageBox.Show("Remote 'origin' removed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateMenuStates();
                    UpdateStatus(); // Refresh to update repo name
                }
                else
                {
                    MessageBox.Show("Failed to remove remote.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
    }
}

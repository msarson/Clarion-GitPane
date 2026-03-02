using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    /// <summary>
    /// GitPanePad - Repository Dialogs
    /// </summary>
    public partial class GitPanePad
    {
        #region Dialogs - Remote and GitHub Creation

        private void OnCreateGitHubRepoClick(object sender, EventArgs e)
        {
            if (gitRepo == null)
                return;

            // Create dialog for GitHub repo creation
            var repoDialog = new Form();
            repoDialog.Text = "Create GitHub Repository";
            repoDialog.Width = 450;
            repoDialog.Height = 265;
            repoDialog.StartPosition = FormStartPosition.CenterParent;
            repoDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            repoDialog.MaximizeBox = false;
            repoDialog.MinimizeBox = false;

            var nameLabel = new Label();
            nameLabel.Text = "Repository name:";
            nameLabel.Location = new Point(10, 15);
            nameLabel.AutoSize = true;
            repoDialog.Controls.Add(nameLabel);

            var nameBox = new TextBox();
            nameBox.Location = new Point(10, 35);
            nameBox.Width = 410;
            // Default to current folder name
            nameBox.Text = System.IO.Path.GetFileName(gitRepo == null ? "" : ProjectService.OpenSolution.Directory);
            repoDialog.Controls.Add(nameBox);

            var descLabel = new Label();
            descLabel.Text = "Description (optional):";
            descLabel.Location = new Point(10, 65);
            descLabel.AutoSize = true;
            repoDialog.Controls.Add(descLabel);

            var descBox = new TextBox();
            descBox.Location = new Point(10, 85);
            descBox.Width = 410;
            repoDialog.Controls.Add(descBox);

            var visibilityLabel = new Label();
            visibilityLabel.Text = "Visibility:";
            visibilityLabel.Location = new Point(10, 115);
            visibilityLabel.AutoSize = true;
            repoDialog.Controls.Add(visibilityLabel);

            var publicRadio = new RadioButton();
            publicRadio.Text = "Public";
            publicRadio.Location = new Point(10, 135);
            publicRadio.AutoSize = true;
            repoDialog.Controls.Add(publicRadio);

            var privateRadio = new RadioButton();
            privateRadio.Text = "Private";
            privateRadio.Location = new Point(100, 135);
            privateRadio.Checked = true; // Default to private
            privateRadio.AutoSize = true;
            repoDialog.Controls.Add(privateRadio);

            var readmeCheckbox = new CheckBox();
            readmeCheckbox.Text = "Create README.md";
            readmeCheckbox.Location = new Point(10, 165);
            readmeCheckbox.Checked = true; // Default to creating README
            readmeCheckbox.AutoSize = true;
            repoDialog.Controls.Add(readmeCheckbox);

            var createButton = new Button();
            createButton.Text = "Create";
            createButton.DialogResult = DialogResult.OK;
            createButton.Location = new Point(250, 195);
            createButton.Width = 80;
            repoDialog.Controls.Add(createButton);

            var cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(340, 195);
            cancelButton.Width = 80;
            repoDialog.Controls.Add(cancelButton);

            repoDialog.AcceptButton = createButton;
            repoDialog.CancelButton = cancelButton;

            if (repoDialog.ShowDialog() == DialogResult.OK)
            {
                string repoName = nameBox.Text.Trim();
                string description = descBox.Text.Trim();
                bool isPrivate = privateRadio.Checked;
                bool createReadme = readmeCheckbox.Checked;

                if (string.IsNullOrEmpty(repoName))
                {
                    MessageBox.Show("Repository name cannot be empty.", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create README.md if requested and doesn't exist
                if (createReadme)
                {
                    string readmePath = System.IO.Path.Combine(ProjectService.OpenSolution.Directory, "README.md");
                    if (!System.IO.File.Exists(readmePath))
                    {
                        try
                        {
                            string readmeContent = $"# {repoName}\n\n{description}\n";
                            System.IO.File.WriteAllText(readmePath, readmeContent);
                            
                            // Stage and commit the README
                            gitRepo.StageFile("README.md");
                            gitRepo.CommitChanges("Add README.md");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to create README.md: {ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }

                // Check if there are commits to push
                bool hasCommits = gitRepo.HasCommits();

                // Show progress message
                var progressForm = new Form();
                progressForm.Text = "Creating Repository...";
                progressForm.Width = 300;
                progressForm.Height = 100;
                progressForm.StartPosition = FormStartPosition.CenterParent;
                progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                progressForm.ControlBox = false;
                var progressLabel = new Label();
                progressLabel.Text = "Creating repository on GitHub...\nPlease wait.";
                progressLabel.Location = new Point(20, 20);
                progressLabel.AutoSize = true;
                progressForm.Controls.Add(progressLabel);
                progressForm.Show();
                progressForm.Refresh();

                // Create repo
                var result = gitRepo.CreateGitHubRepo(repoName, isPrivate, description);
                progressForm.Close();

                if (result.ExitCode == 0)
                {
                    UpdateMenuStates();
                    UpdateStatus();
                    
                    // Offer to push if there are commits
                    if (hasCommits)
                    {
                        var pushPrompt = MessageBox.Show(
                            $"Repository '{repoName}' created successfully!\n\nYou have local commits. Push them to GitHub now?",
                            "Push Commits?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        
                        if (pushPrompt == DialogResult.Yes)
                        {
                            progressForm = new Form();
                            progressForm.Text = "Pushing...";
                            progressForm.Width = 300;
                            progressForm.Height = 100;
                            progressForm.StartPosition = FormStartPosition.CenterParent;
                            progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                            progressForm.ControlBox = false;
                            progressLabel = new Label();
                            progressLabel.Text = "Pushing commits to GitHub...\nPlease wait.";
                            progressLabel.Location = new Point(20, 20);
                            progressLabel.AutoSize = true;
                            progressForm.Controls.Add(progressLabel);
                            progressForm.Show();
                            progressForm.Refresh();
                            
                            var pushResult = gitRepo.PushChanges();
                            progressForm.Close();
                            
                            if (pushResult.ExitCode == 0)
                            {
                                MessageBox.Show("Commits pushed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                var errorText = pushResult.Error + "\n" + pushResult.Output;
                                if (errorText.Contains("Authentication failed") || 
                                    errorText.Contains("Invalid username or token") ||
                                    errorText.Contains("Password authentication is not supported"))
                                {
                                    MessageBox.Show(
                                        "Push failed: Git could not authenticate with the remote server.\n\n" +
                                        "Why this happens:\n" +
                                        "Other Git tools may have credentials stored. GitPane uses Git directly " +
                                        "and cannot prompt for credentials.\n\n" +
                                        "Solution: Run 'gh auth login' once, or configure SSH/PAT.",
                                        "Authentication Required",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show($"Failed to push commits.\n\n{pushResult.Error}\n\nYou may need to run 'git push -u origin main' manually.", "Push Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Repository '{repoName}' created successfully on GitHub!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    string errorMsg = !string.IsNullOrEmpty(result.Error) ? result.Error : result.Output;
                    MessageBox.Show($"Failed to create repository:\n\n{errorMsg}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnInitRepoClick(object sender, EventArgs e)
        {
            if (ProjectService.OpenSolution == null)
                return;

            if (templateManager == null)
            {
                MessageBox.Show("Template manager is not available.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string solutionDir = ProjectService.OpenSolution.Directory;
            
            // Show initialize dialog with template selection
            var dialog = new InitializeRepositoryDialog(templateManager, solutionDir);
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var result = gitRepo.InitializeRepository();
                
                // Check for dubious ownership error
                if (result.ExitCode != 0 && result.Error.Contains("dubious ownership"))
                {
                    if (HandleDubiousOwnershipError(result.Error, solutionDir, dialog))
                        return; // Error was handled and user chose to retry or cancel
                }
                
                if (result.ExitCode == 0)
                {
                    bool gitignoreSuccess = true;
                    bool gitattributesSuccess = true;
                    
                    // Create .gitignore if template selected
                    if (dialog.SelectedGitignoreTemplate != null)
                    {
                        gitignoreSuccess = gitRepo.CreateGitignoreFile(dialog.SelectedGitignoreTemplate.Content);
                    }

                    // Create .gitattributes if template selected
                    if (dialog.SelectedGitattributesTemplate != null)
                    {
                        gitattributesSuccess = gitRepo.CreateGitattributesFile(dialog.SelectedGitattributesTemplate.Content);
                    }

                    if (gitignoreSuccess && gitattributesSuccess)
                    {
                        MessageBox.Show("Git repository initialized successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string warnings = "Git repository initialized, but:\n";
                        if (!gitignoreSuccess)
                            warnings += "- Failed to create .gitignore file\n";
                        if (!gitattributesSuccess)
                            warnings += "- Failed to create .gitattributes file\n";
                        
                        MessageBox.Show(warnings, "Partial Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    
                    UpdateStatus(); // Refresh UI to show repo controls
                }
                else
                {
                    string errorMsg = "Failed to initialize Git repository.\n\n";
                    
                    if (!string.IsNullOrEmpty(result.Error))
                    {
                        errorMsg += "Error: " + result.Error;
                    }
                    else if (!string.IsNullOrEmpty(result.Output))
                    {
                        errorMsg += "Output: " + result.Output;
                    }
                    else
                    {
                        errorMsg += "Git may not be installed or not available in PATH.\n";
                        errorMsg += "Please ensure Git is installed and try again.";
                    }
                    
                    MessageBox.Show(errorMsg, "Initialization Failed", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool HandleDubiousOwnershipError(string errorMessage, string directory, InitializeRepositoryDialog initDialog)
        {
            var dialogResult = MessageBox.Show(
                "Git detected a directory ownership issue:\n\n" +
                "This directory is located in a shared/public folder and is owned by Administrators, " +
                "but you are running as a different user. Git blocks this for security reasons.\n\n" +
                "GitPane can add this directory to Git's safe directory list to allow operations.\n\n" +
                "Directory: " + directory + "\n\n" +
                "Do you want to add this directory to the safe list and retry?",
                "Git Security Issue",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            if (dialogResult == DialogResult.Yes)
            {
                // Add to safe.directory
                var addResult = GitRepository.ExecuteGitCommand(
                    $"config --global --add safe.directory \"{GitRepository.EscapeGitArg(directory.Replace("\\", "/"))}\"",
                    ""
                );

                if (addResult.ExitCode == 0)
                {
                    // Retry initialization
                    MessageBox.Show(
                        "Directory added to safe list successfully.\n\nRetrying initialization...",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Retry the initialization
                    var retryResult = gitRepo.InitializeRepository();
                    
                    if (retryResult.ExitCode == 0)
                    {
                        // Create template files
                        bool gitignoreSuccess = true;
                        bool gitattributesSuccess = true;
                        
                        if (initDialog.SelectedGitignoreTemplate != null)
                        {
                            gitignoreSuccess = gitRepo.CreateGitignoreFile(initDialog.SelectedGitignoreTemplate.Content);
                        }

                        if (initDialog.SelectedGitattributesTemplate != null)
                        {
                            gitattributesSuccess = gitRepo.CreateGitattributesFile(initDialog.SelectedGitattributesTemplate.Content);
                        }

                        if (gitignoreSuccess && gitattributesSuccess)
                        {
                            MessageBox.Show("Git repository initialized successfully!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string warnings = "Git repository initialized, but:\n";
                            if (!gitignoreSuccess)
                                warnings += "- Failed to create .gitignore file\n";
                            if (!gitattributesSuccess)
                                warnings += "- Failed to create .gitattributes file\n";
                            
                            MessageBox.Show(warnings, "Partial Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        
                        UpdateStatus();
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(
                            "Retry failed:\n\n" + retryResult.Error,
                            "Initialization Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Failed to add directory to safe list:\n\n" + addResult.Error,
                        "Configuration Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return true;
                }
            }
            else if (dialogResult == DialogResult.No)
            {
                // Show the full error message
                MessageBox.Show(
                    "Initialization cancelled.\n\nFull error:\n\n" + errorMessage,
                    "Initialization Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return true;
            }
            
            // DialogResult.Cancel - user cancelled
            return true;
        }

        private void OnApplyTemplateClick(object sender, EventArgs e)
        {
            if (gitRepo == null || !gitRepo.IsRepository())
            {
                MessageBox.Show("No repository initialized.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (templateManager == null)
            {
                MessageBox.Show("Template manager is not available.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string repoDir = gitRepo.GetWorkingDirectory();
            var dialog = new ApplyTemplateDialog(templateManager, repoDir);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                bool success = true;
                string message = "";

                // Apply gitignore template
                if (dialog.GitignoreAction != ApplyTemplateDialog.TemplateAction.Skip && 
                    dialog.SelectedGitignoreTemplate != null)
                {
                    bool result = false;
                    switch (dialog.GitignoreAction)
                    {
                        case ApplyTemplateDialog.TemplateAction.Replace:
                            result = gitRepo.ReplaceGitignoreFile(dialog.SelectedGitignoreTemplate.Content);
                            message += result ? ".gitignore replaced (backup created)\n" : ".gitignore replace failed\n";
                            break;
                        case ApplyTemplateDialog.TemplateAction.Merge:
                            result = gitRepo.MergeGitignoreFile(dialog.SelectedGitignoreTemplate.Content, 
                                dialog.SelectedGitignoreTemplate.Name);
                            message += result ? ".gitignore merged\n" : ".gitignore merge failed\n";
                            break;
                    }
                    success &= result;
                }

                // Apply gitattributes template
                if (dialog.GitattributesAction != ApplyTemplateDialog.TemplateAction.Skip && 
                    dialog.SelectedGitattributesTemplate != null)
                {
                    bool result = false;
                    switch (dialog.GitattributesAction)
                    {
                        case ApplyTemplateDialog.TemplateAction.Replace:
                            result = gitRepo.ReplaceGitattributesFile(dialog.SelectedGitattributesTemplate.Content);
                            message += result ? ".gitattributes replaced (backup created)\n" : ".gitattributes replace failed\n";
                            break;
                        case ApplyTemplateDialog.TemplateAction.Merge:
                            result = gitRepo.MergeGitattributesFile(dialog.SelectedGitattributesTemplate.Content, 
                                dialog.SelectedGitattributesTemplate.Name);
                            message += result ? ".gitattributes merged\n" : ".gitattributes merge failed\n";
                            break;
                    }
                    success &= result;
                }

                if (success)
                {
                    MessageBox.Show(message + "\nTemplates applied successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshFileList(); // Refresh to show new/modified files
                }
                else
                {
                    MessageBox.Show(message + "\nSome operations failed.", "Partial Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion
    }
}

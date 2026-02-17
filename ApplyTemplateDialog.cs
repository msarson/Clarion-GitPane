using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GitPane
{
    public class ApplyTemplateDialog : Form
    {
        private Label infoLabel;
        
        // Gitignore section
        private Label gitignoreLabel;
        private ComboBox gitignoreComboBox;
        private Label gitignoreDescLabel;
        private RadioButton gitignoreSkipRadio;
        private RadioButton gitignoreReplaceRadio;
        private RadioButton gitignoreMergeRadio;
        private Label gitignoreExistsLabel;
        
        // Gitattributes section
        private Label gitattributesLabel;
        private ComboBox gitattributesComboBox;
        private Label gitattributesDescLabel;
        private RadioButton gitattributesSkipRadio;
        private RadioButton gitattributesReplaceRadio;
        private RadioButton gitattributesMergeRadio;
        private Label gitattributesExistsLabel;
        
        private Button okButton;
        private Button cancelButton;

        private TemplateManager templateManager;
        private string repositoryPath;
        private bool gitignoreExists;
        private bool gitattributesExists;

        public GitTemplate SelectedGitignoreTemplate { get; private set; }
        public GitTemplate SelectedGitattributesTemplate { get; private set; }
        public TemplateAction GitignoreAction { get; private set; }
        public TemplateAction GitattributesAction { get; private set; }

        public enum TemplateAction
        {
            Skip,
            Replace,
            Merge
        }

        public ApplyTemplateDialog(TemplateManager manager, string repoPath)
        {
            templateManager = manager;
            repositoryPath = repoPath;
            
            CheckExistingFiles();
            InitializeUI();
            LoadTemplates();
        }

        private void CheckExistingFiles()
        {
            gitignoreExists = File.Exists(Path.Combine(repositoryPath, ".gitignore"));
            gitattributesExists = File.Exists(Path.Combine(repositoryPath, ".gitattributes"));
        }

        private void InitializeUI()
        {
            this.Text = "Apply Templates";
            this.Size = new Size(580, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 15;

            // Info label
            infoLabel = new Label
            {
                Text = "Apply templates to your repository. You can apply one, both, or neither.",
                Location = new Point(15, yPos),
                Size = new Size(540, 30),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(infoLabel);
            yPos += 40;

            // === Gitignore Section ===
            gitignoreLabel = new Label
            {
                Text = ".gitignore Template:",
                Location = new Point(15, yPos),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            this.Controls.Add(gitignoreLabel);
            yPos += 25;

            gitignoreComboBox = new ComboBox
            {
                Location = new Point(15, yPos),
                Size = new Size(540, 23),
                Font = new Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gitignoreComboBox.SelectedIndexChanged += OnGitignoreSelectionChanged;
            this.Controls.Add(gitignoreComboBox);
            yPos += 28;

            gitignoreDescLabel = new Label
            {
                Location = new Point(15, yPos),
                Size = new Size(540, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            this.Controls.Add(gitignoreDescLabel);
            yPos += 25;

            // Existing file warning
            if (gitignoreExists)
            {
                gitignoreExistsLabel = new Label
                {
                    Text = "⚠ .gitignore already exists in repository",
                    Location = new Point(15, yPos),
                    Size = new Size(300, 20),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.DarkOrange
                };
                this.Controls.Add(gitignoreExistsLabel);
                yPos += 25;

                // Action radio buttons
                gitignoreSkipRadio = new RadioButton
                {
                    Text = "Skip (don't modify existing file)",
                    Location = new Point(35, yPos),
                    Size = new Size(250, 20),
                    Checked = true
                };
                this.Controls.Add(gitignoreSkipRadio);
                yPos += 25;

                gitignoreReplaceRadio = new RadioButton
                {
                    Text = "Replace (backup existing to .gitignore.backup)",
                    Location = new Point(35, yPos),
                    Size = new Size(350, 20)
                };
                this.Controls.Add(gitignoreReplaceRadio);
                yPos += 25;

                gitignoreMergeRadio = new RadioButton
                {
                    Text = "Merge (append template content to existing)",
                    Location = new Point(35, yPos),
                    Size = new Size(350, 20)
                };
                this.Controls.Add(gitignoreMergeRadio);
                yPos += 30;
            }
            else
            {
                // No existing file - will just create
                yPos += 10;
            }

            // === Gitattributes Section ===
            gitattributesLabel = new Label
            {
                Text = ".gitattributes Template:",
                Location = new Point(15, yPos),
                Size = new Size(170, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            this.Controls.Add(gitattributesLabel);
            yPos += 25;

            gitattributesComboBox = new ComboBox
            {
                Location = new Point(15, yPos),
                Size = new Size(540, 23),
                Font = new Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gitattributesComboBox.SelectedIndexChanged += OnGitattributesSelectionChanged;
            this.Controls.Add(gitattributesComboBox);
            yPos += 28;

            gitattributesDescLabel = new Label
            {
                Location = new Point(15, yPos),
                Size = new Size(540, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            this.Controls.Add(gitattributesDescLabel);
            yPos += 25;

            // Existing file warning
            if (gitattributesExists)
            {
                gitattributesExistsLabel = new Label
                {
                    Text = "⚠ .gitattributes already exists in repository",
                    Location = new Point(15, yPos),
                    Size = new Size(300, 20),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.DarkOrange
                };
                this.Controls.Add(gitattributesExistsLabel);
                yPos += 25;

                // Action radio buttons
                gitattributesSkipRadio = new RadioButton
                {
                    Text = "Skip (don't modify existing file)",
                    Location = new Point(35, yPos),
                    Size = new Size(250, 20),
                    Checked = true
                };
                this.Controls.Add(gitattributesSkipRadio);
                yPos += 25;

                gitattributesReplaceRadio = new RadioButton
                {
                    Text = "Replace (backup existing to .gitattributes.backup)",
                    Location = new Point(35, yPos),
                    Size = new Size(380, 20)
                };
                this.Controls.Add(gitattributesReplaceRadio);
                yPos += 25;

                gitattributesMergeRadio = new RadioButton
                {
                    Text = "Merge (append template content to existing)",
                    Location = new Point(35, yPos),
                    Size = new Size(350, 20)
                };
                this.Controls.Add(gitattributesMergeRadio);
                yPos += 30;
            }

            // Buttons
            okButton = new Button
            {
                Text = "Apply",
                Location = new Point(419, yPos + 10),
                Size = new Size(65, 28),
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9F)
            };
            okButton.Click += OnApply;
            this.Controls.Add(okButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(490, yPos + 10),
                Size = new Size(65, 28),
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private void LoadTemplates()
        {
            // Load gitignore templates
            var gitignoreTemplates = templateManager.GetTemplates(TemplateType.Gitignore);
            gitignoreComboBox.Items.Add("(None)");
            
            foreach (var template in gitignoreTemplates)
            {
                gitignoreComboBox.Items.Add(template);
                if (template.IsDefault)
                {
                    gitignoreComboBox.SelectedItem = template;
                }
            }
            
            if (gitignoreComboBox.SelectedIndex == -1)
            {
                gitignoreComboBox.SelectedIndex = 0;
            }

            // Load gitattributes templates
            var gitattributesTemplates = templateManager.GetTemplates(TemplateType.Gitattributes);
            gitattributesComboBox.Items.Add("(None)");
            
            foreach (var template in gitattributesTemplates)
            {
                gitattributesComboBox.Items.Add(template);
                if (template.IsDefault)
                {
                    gitattributesComboBox.SelectedItem = template;
                }
            }
            
            if (gitattributesComboBox.SelectedIndex == -1)
            {
                gitattributesComboBox.SelectedIndex = 0;
            }

            gitignoreComboBox.DisplayMember = "Name";
            gitattributesComboBox.DisplayMember = "Name";
        }

        private void OnGitignoreSelectionChanged(object sender, EventArgs e)
        {
            if (gitignoreComboBox.SelectedItem is GitTemplate template)
            {
                gitignoreDescLabel.Text = template.Description;
                SelectedGitignoreTemplate = template;
            }
            else
            {
                gitignoreDescLabel.Text = "No .gitignore will be applied";
                SelectedGitignoreTemplate = null;
            }
        }

        private void OnGitattributesSelectionChanged(object sender, EventArgs e)
        {
            if (gitattributesComboBox.SelectedItem is GitTemplate template)
            {
                gitattributesDescLabel.Text = template.Description;
                SelectedGitattributesTemplate = template;
            }
            else
            {
                gitattributesDescLabel.Text = "No .gitattributes will be applied";
                SelectedGitattributesTemplate = null;
            }
        }

        private void OnApply(object sender, EventArgs e)
        {
            // Determine gitignore action
            if (SelectedGitignoreTemplate == null)
            {
                GitignoreAction = TemplateAction.Skip;
            }
            else if (gitignoreExists)
            {
                if (gitignoreReplaceRadio.Checked)
                    GitignoreAction = TemplateAction.Replace;
                else if (gitignoreMergeRadio.Checked)
                    GitignoreAction = TemplateAction.Merge;
                else
                    GitignoreAction = TemplateAction.Skip;
            }
            else
            {
                GitignoreAction = TemplateAction.Replace; // Create new
            }

            // Determine gitattributes action
            if (SelectedGitattributesTemplate == null)
            {
                GitattributesAction = TemplateAction.Skip;
            }
            else if (gitattributesExists)
            {
                if (gitattributesReplaceRadio.Checked)
                    GitattributesAction = TemplateAction.Replace;
                else if (gitattributesMergeRadio.Checked)
                    GitattributesAction = TemplateAction.Merge;
                else
                    GitattributesAction = TemplateAction.Skip;
            }
            else
            {
                GitattributesAction = TemplateAction.Replace; // Create new
            }

            // Validate at least one action selected
            if (GitignoreAction == TemplateAction.Skip && GitattributesAction == TemplateAction.Skip)
            {
                MessageBox.Show("No templates selected to apply.", "Nothing to Apply",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.None;
                return;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GitPane
{
    public class InitializeRepositoryDialog : Form
    {
        private TextBox pathTextBox;
        private Label gitignoreLabel;
        private ComboBox gitignoreComboBox;
        private Label gitignoreDescLabel;
        private Label gitattributesLabel;
        private ComboBox gitattributesComboBox;
        private Label gitattributesDescLabel;
        private Button okButton;
        private Button cancelButton;

        private TemplateManager templateManager;
        private string repositoryPath;

        public GitTemplate SelectedGitignoreTemplate { get; private set; }
        public GitTemplate SelectedGitattributesTemplate { get; private set; }

        public InitializeRepositoryDialog(TemplateManager manager, string path)
        {
            templateManager = manager;
            repositoryPath = path;
            
            InitializeUI();
            LoadTemplates();
        }

        private void InitializeUI()
        {
            this.Text = "Initialize Git Repository";
            this.Size = new Size(550, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Path label
            var pathInfoLabel = new Label
            {
                Text = "Initialize Git repository in:",
                Location = new Point(15, 15),
                Size = new Size(500, 20),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(pathInfoLabel);

            // Path textbox (read-only)
            pathTextBox = new TextBox
            {
                Text = repositoryPath,
                Location = new Point(15, 38),
                Size = new Size(505, 23),
                Font = new Font("Segoe UI", 9F),
                ReadOnly = true,
                BackColor = SystemColors.Control
            };
            this.Controls.Add(pathTextBox);

            // Gitignore section
            gitignoreLabel = new Label
            {
                Text = ".gitignore Template:",
                Location = new Point(15, 75),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            this.Controls.Add(gitignoreLabel);

            gitignoreComboBox = new ComboBox
            {
                Location = new Point(15, 98),
                Size = new Size(505, 23),
                Font = new Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gitignoreComboBox.SelectedIndexChanged += OnGitignoreSelectionChanged;
            this.Controls.Add(gitignoreComboBox);

            gitignoreDescLabel = new Label
            {
                Location = new Point(15, 125),
                Size = new Size(505, 30),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            this.Controls.Add(gitignoreDescLabel);

            // Gitattributes section
            gitattributesLabel = new Label
            {
                Text = ".gitattributes Template:",
                Location = new Point(15, 160),
                Size = new Size(170, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            this.Controls.Add(gitattributesLabel);

            gitattributesComboBox = new ComboBox
            {
                Location = new Point(15, 183),
                Size = new Size(505, 23),
                Font = new Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gitattributesComboBox.SelectedIndexChanged += OnGitattributesSelectionChanged;
            this.Controls.Add(gitattributesComboBox);

            gitattributesDescLabel = new Label
            {
                Location = new Point(15, 210),
                Size = new Size(505, 30),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            this.Controls.Add(gitattributesDescLabel);

            // Buttons
            okButton = new Button
            {
                Text = "Initialize",
                Location = new Point(364, 250),
                Size = new Size(75, 28),
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(okButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(445, 250),
                Size = new Size(75, 28),
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
                gitignoreComboBox.SelectedIndex = 0; // Select "(None)" if no default
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
                gitattributesComboBox.SelectedIndex = 0; // Select "(None)" if no default
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
                gitignoreDescLabel.Text = "No .gitignore file will be created";
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
                gitattributesDescLabel.Text = "No .gitattributes file will be created";
                SelectedGitattributesTemplate = null;
            }
        }
    }
}

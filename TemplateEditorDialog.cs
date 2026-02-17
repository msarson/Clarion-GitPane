using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitPane
{
    public class TemplateEditorDialog : Form
    {
        private TextBox nameTextBox;
        private TextBox descriptionTextBox;
        private TextBox contentTextBox;
        private Label characterCountLabel;
        private CheckBox isDefaultCheckBox;
        private Button saveButton;
        private Button cancelButton;
        
        private GitTemplate template;
        private TemplateType templateType;
        private bool isNewTemplate;

        public GitTemplate Template => template;

        public TemplateEditorDialog(GitTemplate existingTemplate, TemplateType type, bool isNew = false)
        {
            template = existingTemplate ?? new GitTemplate();
            templateType = type;
            isNewTemplate = isNew;
            
            InitializeUI();
            LoadTemplate();
        }

        private void InitializeUI()
        {
            string typeName = templateType == TemplateType.Gitignore ? ".gitignore" : ".gitattributes";
            this.Text = (isNewTemplate ? "New " : "Edit ") + typeName + " Template";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Name label and textbox
            var nameLabel = new Label
            {
                Text = "Name:",
                Location = new Point(12, 15),
                Size = new Size(80, 20)
            };
            this.Controls.Add(nameLabel);

            nameTextBox = new TextBox
            {
                Location = new Point(100, 12),
                Size = new Size(470, 23),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(nameTextBox);

            // Description label and textbox
            var descriptionLabel = new Label
            {
                Text = "Description:",
                Location = new Point(12, 45),
                Size = new Size(80, 20)
            };
            this.Controls.Add(descriptionLabel);

            descriptionTextBox = new TextBox
            {
                Location = new Point(100, 42),
                Size = new Size(470, 23),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(descriptionTextBox);

            // Content label
            var contentLabel = new Label
            {
                Text = "Content:",
                Location = new Point(12, 75),
                Size = new Size(80, 20)
            };
            this.Controls.Add(contentLabel);

            // Content textbox
            contentTextBox = new TextBox
            {
                Location = new Point(12, 98),
                Size = new Size(558, 300),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9F),
                WordWrap = false,
                AcceptsReturn = true,
                AcceptsTab = true
            };
            contentTextBox.TextChanged += OnContentChanged;
            this.Controls.Add(contentTextBox);

            // Character count label
            characterCountLabel = new Label
            {
                Location = new Point(12, 403),
                Size = new Size(300, 20),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F)
            };
            this.Controls.Add(characterCountLabel);

            // Is Default checkbox
            isDefaultCheckBox = new CheckBox
            {
                Text = "Set as default template",
                Location = new Point(12, 428),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(isDefaultCheckBox);

            // Buttons
            saveButton = new Button
            {
                Text = "Save",
                Location = new Point(414, 425),
                Size = new Size(75, 28),
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9F)
            };
            saveButton.Click += OnSave;
            this.Controls.Add(saveButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(495, 425),
                Size = new Size(75, 28),
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(cancelButton);

            this.AcceptButton = saveButton;
            this.CancelButton = cancelButton;
        }

        private void LoadTemplate()
        {
            nameTextBox.Text = template.Name;
            descriptionTextBox.Text = template.Description;
            contentTextBox.Text = template.Content;
            isDefaultCheckBox.Checked = template.IsDefault;
            
            UpdateCharacterCount();
        }

        private void OnContentChanged(object sender, EventArgs e)
        {
            UpdateCharacterCount();
        }

        private void UpdateCharacterCount()
        {
            int lines = contentTextBox.Lines.Length;
            int chars = contentTextBox.Text.Length;
            characterCountLabel.Text = $"{lines} lines, {chars} characters";
        }

        private void OnSave(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Template name is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrWhiteSpace(contentTextBox.Text))
            {
                var result = MessageBox.Show(
                    "Template content is empty. Save anyway?", 
                    "Empty Content", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (result != DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.None;
                    return;
                }
            }

            // Save to template object
            template.Name = nameTextBox.Text.Trim();
            template.Description = descriptionTextBox.Text.Trim();
            template.Content = contentTextBox.Text;
            template.IsDefault = isDefaultCheckBox.Checked;
        }
    }
}

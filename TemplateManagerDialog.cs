using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GitPane
{
    public class TemplateManagerDialog : Form
    {
        private TemplateManager templateManager;
        private TabControl tabControl;
        
        // Gitignore tab controls
        private ListView gitignoreListView;
        private Button gitignoreAddButton;
        private Button gitignoreEditButton;
        private Button gitignoreDeleteButton;
        private Button gitignoreSetDefaultButton;
        
        // Gitattributes tab controls
        private ListView gitattributesListView;
        private Button gitattributesAddButton;
        private Button gitattributesEditButton;
        private Button gitattributesDeleteButton;
        private Button gitattributesSetDefaultButton;
        
        private Button openFolderButton;
        private Button closeButton;

        public TemplateManagerDialog(TemplateManager manager)
        {
            templateManager = manager;
            InitializeUI();
            LoadTemplates();
        }

        private void InitializeUI()
        {
            this.Text = "Manage .gitignore/.gitattributes Templates";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Tab control
            tabControl = new TabControl
            {
                Location = new Point(12, 12),
                Size = new Size(660, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Create tabs
            var gitignoreTab = new TabPage(".gitignore Templates");
            var gitattributesTab = new TabPage(".gitattributes Templates");

            // Setup gitignore tab
            SetupGitignoreTab(gitignoreTab);
            
            // Setup gitattributes tab
            SetupGitattributesTab(gitattributesTab);

            tabControl.TabPages.Add(gitignoreTab);
            tabControl.TabPages.Add(gitattributesTab);
            
            this.Controls.Add(tabControl);

            // Bottom buttons
            openFolderButton = new Button
            {
                Text = "Open Templates Folder",
                Location = new Point(12, 420),
                Size = new Size(150, 28),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9F)
            };
            openFolderButton.Click += OnOpenFolder;
            this.Controls.Add(openFolderButton);

            closeButton = new Button
            {
                Text = "Close",
                Location = new Point(597, 420),
                Size = new Size(75, 28),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(closeButton);

            this.AcceptButton = closeButton;
        }

        private void SetupGitignoreTab(TabPage tab)
        {
            // ListView
            gitignoreListView = new ListView
            {
                Location = new Point(10, 10),
                Size = new Size(540, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9F)
            };
            gitignoreListView.Columns.Add("Name", 150);
            gitignoreListView.Columns.Add("Description", 300);
            gitignoreListView.Columns.Add("Default", 70);
            gitignoreListView.SelectedIndexChanged += OnGitignoreSelectionChanged;
            gitignoreListView.DoubleClick += OnGitignoreEdit;
            tab.Controls.Add(gitignoreListView);

            // Buttons panel
            int buttonX = 560;
            int buttonY = 10;
            int buttonWidth = 80;
            int buttonHeight = 28;
            int buttonSpacing = 35;

            gitignoreAddButton = new Button
            {
                Text = "Add",
                Location = new Point(buttonX, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F)
            };
            gitignoreAddButton.Click += OnGitignoreAdd;
            tab.Controls.Add(gitignoreAddButton);

            gitignoreEditButton = new Button
            {
                Text = "Edit",
                Location = new Point(buttonX, buttonY + buttonSpacing),
                Size = new Size(buttonWidth, buttonHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitignoreEditButton.Click += OnGitignoreEdit;
            tab.Controls.Add(gitignoreEditButton);

            gitignoreDeleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(buttonX, buttonY + buttonSpacing * 2),
                Size = new Size(buttonWidth, buttonHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitignoreDeleteButton.Click += OnGitignoreDelete;
            tab.Controls.Add(gitignoreDeleteButton);

            gitignoreSetDefaultButton = new Button
            {
                Text = "Set Default",
                Location = new Point(buttonX, buttonY + buttonSpacing * 3),
                Size = new Size(buttonWidth, buttonHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitignoreSetDefaultButton.Click += OnGitignoreSetDefault;
            tab.Controls.Add(gitignoreSetDefaultButton);
        }

        private void SetupGitattributesTab(TabPage tab)
        {
            // ListView
            gitattributesListView = new ListView
            {
                Location = new Point(10, 10),
                Size = new Size(540, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9F)
            };
            gitattributesListView.Columns.Add("Name", 150);
            gitattributesListView.Columns.Add("Description", 300);
            gitattributesListView.Columns.Add("Default", 70);
            gitattributesListView.SelectedIndexChanged += OnGitattributesSelectionChanged;
            gitattributesListView.DoubleClick += OnGitattributesEdit;
            tab.Controls.Add(gitattributesListView);

            // Buttons panel
            int buttonX = 560;
            int buttonY = 10;
            int buttonWidth = 80;
            int buttonHeight = 28;
            int buttonSpacing = 35;

            gitattributesAddButton = new Button
            {
                Text = "Add",
                Location = new Point(buttonX, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F)
            };
            gitattributesAddButton.Click += OnGitattributesAdd;
            tab.Controls.Add(gitattributesAddButton);

            gitattributesEditButton = new Button
            {
                Text = "Edit",
                Location = new Point(buttonX, buttonY + buttonSpacing),
                Size = new Size(buttonWidth, buttonHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitattributesEditButton.Click += OnGitattributesEdit;
            tab.Controls.Add(gitattributesEditButton);

            gitattributesDeleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(buttonX, buttonY + buttonSpacing * 2),
                Size = new Size(buttonWidth, buttonHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitattributesDeleteButton.Click += OnGitattributesDelete;
            tab.Controls.Add(gitattributesDeleteButton);

            gitattributesSetDefaultButton = new Button
            {
                Text = "Set Default",
                Location = new Point(buttonX, buttonY + buttonSpacing * 3),
                Size = new Size(buttonWidth, buttonHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitattributesSetDefaultButton.Click += OnGitattributesSetDefault;
            tab.Controls.Add(gitattributesSetDefaultButton);
        }

        private void LoadTemplates()
        {
            LoadGitignoreTemplates();
            LoadGitattributesTemplates();
        }

        private void LoadGitignoreTemplates()
        {
            gitignoreListView.Items.Clear();
            var templates = templateManager.GetTemplates(TemplateType.Gitignore);
            
            foreach (var template in templates)
            {
                var item = new ListViewItem(template.Name);
                item.SubItems.Add(template.Description);
                item.SubItems.Add(template.IsDefault ? "Yes" : "");
                item.Tag = template;
                
                if (template.IsDefault)
                {
                    item.Font = new Font(item.Font, FontStyle.Bold);
                }
                
                gitignoreListView.Items.Add(item);
            }
        }

        private void LoadGitattributesTemplates()
        {
            gitattributesListView.Items.Clear();
            var templates = templateManager.GetTemplates(TemplateType.Gitattributes);
            
            foreach (var template in templates)
            {
                var item = new ListViewItem(template.Name);
                item.SubItems.Add(template.Description);
                item.SubItems.Add(template.IsDefault ? "Yes" : "");
                item.Tag = template;
                
                if (template.IsDefault)
                {
                    item.Font = new Font(item.Font, FontStyle.Bold);
                }
                
                gitattributesListView.Items.Add(item);
            }
        }

        // Gitignore event handlers
        private void OnGitignoreSelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = gitignoreListView.SelectedItems.Count > 0;
            gitignoreEditButton.Enabled = hasSelection;
            gitignoreDeleteButton.Enabled = hasSelection;
            gitignoreSetDefaultButton.Enabled = hasSelection;
        }

        private void OnGitignoreAdd(object sender, EventArgs e)
        {
            var newTemplate = new GitTemplate();
            var dialog = new TemplateEditorDialog(newTemplate, TemplateType.Gitignore, isNew: true);
            
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                templateManager.SaveTemplate(newTemplate, TemplateType.Gitignore);
                LoadGitignoreTemplates();
            }
        }

        private void OnGitignoreEdit(object sender, EventArgs e)
        {
            if (gitignoreListView.SelectedItems.Count == 0)
                return;

            var selectedItem = gitignoreListView.SelectedItems[0];
            var template = selectedItem.Tag as GitTemplate;
            
            if (template != null)
            {
                var dialog = new TemplateEditorDialog(template, TemplateType.Gitignore, isNew: false);
                
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    templateManager.SaveTemplate(template, TemplateType.Gitignore);
                    LoadGitignoreTemplates();
                }
            }
        }

        private void OnGitignoreDelete(object sender, EventArgs e)
        {
            if (gitignoreListView.SelectedItems.Count == 0)
                return;

            var selectedItem = gitignoreListView.SelectedItems[0];
            var template = selectedItem.Tag as GitTemplate;
            
            if (template != null)
            {
                var templates = templateManager.GetTemplates(TemplateType.Gitignore);
                if (templates.Count <= 1)
                {
                    MessageBox.Show("Cannot delete the last template.", "Delete Template",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Delete template '{template.Name}'?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    templateManager.DeleteTemplate(template.Id, TemplateType.Gitignore);
                    LoadGitignoreTemplates();
                }
            }
        }

        private void OnGitignoreSetDefault(object sender, EventArgs e)
        {
            if (gitignoreListView.SelectedItems.Count == 0)
                return;

            var selectedItem = gitignoreListView.SelectedItems[0];
            var template = selectedItem.Tag as GitTemplate;
            
            if (template != null && !template.IsDefault)
            {
                template.IsDefault = true;
                templateManager.SaveTemplate(template, TemplateType.Gitignore);
                LoadGitignoreTemplates();
            }
        }

        // Gitattributes event handlers
        private void OnGitattributesSelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = gitattributesListView.SelectedItems.Count > 0;
            gitattributesEditButton.Enabled = hasSelection;
            gitattributesDeleteButton.Enabled = hasSelection;
            gitattributesSetDefaultButton.Enabled = hasSelection;
        }

        private void OnGitattributesAdd(object sender, EventArgs e)
        {
            var newTemplate = new GitTemplate();
            var dialog = new TemplateEditorDialog(newTemplate, TemplateType.Gitattributes, isNew: true);
            
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                templateManager.SaveTemplate(newTemplate, TemplateType.Gitattributes);
                LoadGitattributesTemplates();
            }
        }

        private void OnGitattributesEdit(object sender, EventArgs e)
        {
            if (gitattributesListView.SelectedItems.Count == 0)
                return;

            var selectedItem = gitattributesListView.SelectedItems[0];
            var template = selectedItem.Tag as GitTemplate;
            
            if (template != null)
            {
                var dialog = new TemplateEditorDialog(template, TemplateType.Gitattributes, isNew: false);
                
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    templateManager.SaveTemplate(template, TemplateType.Gitattributes);
                    LoadGitattributesTemplates();
                }
            }
        }

        private void OnGitattributesDelete(object sender, EventArgs e)
        {
            if (gitattributesListView.SelectedItems.Count == 0)
                return;

            var selectedItem = gitattributesListView.SelectedItems[0];
            var template = selectedItem.Tag as GitTemplate;
            
            if (template != null)
            {
                var templates = templateManager.GetTemplates(TemplateType.Gitattributes);
                if (templates.Count <= 1)
                {
                    MessageBox.Show("Cannot delete the last template.", "Delete Template",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Delete template '{template.Name}'?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    templateManager.DeleteTemplate(template.Id, TemplateType.Gitattributes);
                    LoadGitattributesTemplates();
                }
            }
        }

        private void OnGitattributesSetDefault(object sender, EventArgs e)
        {
            if (gitattributesListView.SelectedItems.Count == 0)
                return;

            var selectedItem = gitattributesListView.SelectedItems[0];
            var template = selectedItem.Tag as GitTemplate;
            
            if (template != null && !template.IsDefault)
            {
                template.IsDefault = true;
                templateManager.SaveTemplate(template, TemplateType.Gitattributes);
                LoadGitattributesTemplates();
            }
        }

        private void OnOpenFolder(object sender, EventArgs e)
        {
            try
            {
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string addInPath = System.IO.Path.GetDirectoryName(assemblyPath);
                string templatesPath = System.IO.Path.Combine(addInPath, "templates");
                
                if (System.IO.Directory.Exists(templatesPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", templatesPath);
                }
                else
                {
                    MessageBox.Show("Templates folder not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open templates folder: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

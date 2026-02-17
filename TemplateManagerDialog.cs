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
        private Button gitignoreNewButton;
        private Button gitignoreImportButton;
        private Button gitignoreEditButton;
        private Button gitignoreDeleteButton;
        private Button gitignoreSetDefaultButton;
        
        // Gitattributes tab controls
        private ListView gitattributesListView;
        private Button gitattributesNewButton;
        private Button gitattributesImportButton;
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
            // Bottom panel for buttons
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45
            };
            tab.Controls.Add(bottomPanel);

            // ListView
            gitignoreListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9F)
            };
            gitignoreListView.Columns.Add("Name", 150);
            gitignoreListView.Columns.Add("Description", 400);
            gitignoreListView.Columns.Add("Default", 70);
            gitignoreListView.SelectedIndexChanged += OnGitignoreSelectionChanged;
            gitignoreListView.DoubleClick += OnGitignoreEdit;
            tab.Controls.Add(gitignoreListView);

            // Buttons in bottom panel
            int buttonX = 10;
            int buttonY = 8;
            int buttonWidth = 80;
            int buttonHeight = 28;
            int buttonSpacing = 90;

            gitignoreNewButton = new Button
            {
                Text = "New",
                Location = new Point(buttonX, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F)
            };
            gitignoreNewButton.Click += OnGitignoreNew;
            bottomPanel.Controls.Add(gitignoreNewButton);

            gitignoreImportButton = new Button
            {
                Text = "Import",
                Location = new Point(buttonX + buttonSpacing, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F)
            };
            gitignoreImportButton.Click += OnGitignoreImport;
            bottomPanel.Controls.Add(gitignoreImportButton);

            gitignoreEditButton = new Button
            {
                Text = "Edit",
                Location = new Point(buttonX + buttonSpacing * 2, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitignoreEditButton.Click += OnGitignoreEdit;
            bottomPanel.Controls.Add(gitignoreEditButton);

            gitignoreDeleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(buttonX + buttonSpacing * 3, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitignoreDeleteButton.Click += OnGitignoreDelete;
            bottomPanel.Controls.Add(gitignoreDeleteButton);

            gitignoreSetDefaultButton = new Button
            {
                Text = "Set Default",
                Location = new Point(buttonX + buttonSpacing * 4, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitignoreSetDefaultButton.Click += OnGitignoreSetDefault;
            bottomPanel.Controls.Add(gitignoreSetDefaultButton);
        }

        private void SetupGitattributesTab(TabPage tab)
        {
            // Bottom panel for buttons
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45
            };
            tab.Controls.Add(bottomPanel);

            // ListView
            gitattributesListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9F)
            };
            gitattributesListView.Columns.Add("Name", 150);
            gitattributesListView.Columns.Add("Description", 400);
            gitattributesListView.Columns.Add("Default", 70);
            gitattributesListView.SelectedIndexChanged += OnGitattributesSelectionChanged;
            gitattributesListView.DoubleClick += OnGitattributesEdit;
            tab.Controls.Add(gitattributesListView);

            // Buttons in bottom panel
            int buttonX = 10;
            int buttonY = 8;
            int buttonWidth = 80;
            int buttonHeight = 28;
            int buttonSpacing = 90;

            gitattributesNewButton = new Button
            {
                Text = "New",
                Location = new Point(buttonX, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F)
            };
            gitattributesNewButton.Click += OnGitattributesNew;
            bottomPanel.Controls.Add(gitattributesNewButton);

            gitattributesImportButton = new Button
            {
                Text = "Import",
                Location = new Point(buttonX + buttonSpacing, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F)
            };
            gitattributesImportButton.Click += OnGitattributesImport;
            bottomPanel.Controls.Add(gitattributesImportButton);

            gitattributesEditButton = new Button
            {
                Text = "Edit",
                Location = new Point(buttonX + buttonSpacing * 2, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitattributesEditButton.Click += OnGitattributesEdit;
            bottomPanel.Controls.Add(gitattributesEditButton);

            gitattributesDeleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(buttonX + buttonSpacing * 3, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitattributesDeleteButton.Click += OnGitattributesDelete;
            bottomPanel.Controls.Add(gitattributesDeleteButton);

            gitattributesSetDefaultButton = new Button
            {
                Text = "Set Default",
                Location = new Point(buttonX + buttonSpacing * 4, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            gitattributesSetDefaultButton.Click += OnGitattributesSetDefault;
            bottomPanel.Controls.Add(gitattributesSetDefaultButton);
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

        private void OnGitignoreNew(object sender, EventArgs e)
        {
            var newTemplate = new GitTemplate();
            var dialog = new TemplateEditorDialog(newTemplate, TemplateType.Gitignore, isNew: true);
            
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                templateManager.SaveTemplate(newTemplate, TemplateType.Gitignore);
                LoadGitignoreTemplates();
            }
        }

        private void OnGitignoreImport(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Gitignore files (.gitignore)|.gitignore|All files (*.*)|*.*";
                openFileDialog.Title = "Import .gitignore File";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string content = System.IO.File.ReadAllText(openFileDialog.FileName);
                        string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                        
                        var newTemplate = new GitTemplate
                        {
                            Name = "Imported " + fileName,
                            Description = "Imported from " + openFileDialog.FileName,
                            Content = content
                        };
                        
                        var dialog = new TemplateEditorDialog(newTemplate, TemplateType.Gitignore, isNew: true);
                        
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            templateManager.SaveTemplate(newTemplate, TemplateType.Gitignore);
                            LoadGitignoreTemplates();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to import file: {ex.Message}", "Import Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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

        private void OnGitattributesNew(object sender, EventArgs e)
        {
            var newTemplate = new GitTemplate();
            var dialog = new TemplateEditorDialog(newTemplate, TemplateType.Gitattributes, isNew: true);
            
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                templateManager.SaveTemplate(newTemplate, TemplateType.Gitattributes);
                LoadGitattributesTemplates();
            }
        }

        private void OnGitattributesImport(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Gitattributes files (.gitattributes)|.gitattributes|All files (*.*)|*.*";
                openFileDialog.Title = "Import .gitattributes File";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string content = System.IO.File.ReadAllText(openFileDialog.FileName);
                        string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                        
                        var newTemplate = new GitTemplate
                        {
                            Name = "Imported " + fileName,
                            Description = "Imported from " + openFileDialog.FileName,
                            Content = content
                        };
                        
                        var dialog = new TemplateEditorDialog(newTemplate, TemplateType.Gitattributes, isNew: true);
                        
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            templateManager.SaveTemplate(newTemplate, TemplateType.Gitattributes);
                            LoadGitattributesTemplates();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to import file: {ex.Message}", "Import Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class SolutionConfigurationEditor : Form
{
	private sealed class EditTag
	{
		public static readonly EditTag Instance = new EditTag();

		public override string ToString()
		{
			return "<Edit>";
		}
	}

	private IContainer components;

	private DataGridView grid;

	private ComboBox configurationComboBox;

	private ComboBox platformComboBox;

	private Button okButton;

	private Panel panel2;

	private DataGridViewComboBoxColumn platformColumn;

	private DataGridViewComboBoxColumn configurationColumn;

	private DataGridViewTextBoxColumn projectNameColumn;

	private Label label1;

	private Label label2;

	private Panel panel1;

	private Solution solution;

	private bool inUpdate;

	private int configurationComboBoxEditIndex;

	private int platformComboBoxEditIndex;

	private ComboBox gridEditingControl;

	public ComboBox GridEditingControl
	{
		get
		{
			return gridEditingControl;
		}
		set
		{
			if (gridEditingControl != value)
			{
				if (gridEditingControl != null)
				{
					gridEditingControl.SelectedIndexChanged -= GridEditingControlSelectedIndexChanged;
				}
				gridEditingControl = value;
				if (gridEditingControl != null)
				{
					gridEditingControl.SelectedIndexChanged += GridEditingControlSelectedIndexChanged;
				}
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.panel1 = new System.Windows.Forms.Panel();
		this.platformComboBox = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		this.configurationComboBox = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.grid = new System.Windows.Forms.DataGridView();
		this.projectNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.configurationColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.platformColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.panel2 = new System.Windows.Forms.Panel();
		this.okButton = new System.Windows.Forms.Button();
		this.panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grid).BeginInit();
		this.panel2.SuspendLayout();
		base.SuspendLayout();
		this.panel1.Controls.Add(this.platformComboBox);
		this.panel1.Controls.Add(this.label2);
		this.panel1.Controls.Add(this.configurationComboBox);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
		this.panel1.Location = new System.Drawing.Point(0, 0);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(504, 37);
		this.panel1.TabIndex = 0;
		this.platformComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.platformComboBox.FormattingEnabled = true;
		this.platformComboBox.Location = new System.Drawing.Point(326, 6);
		this.platformComboBox.Name = "platformComboBox";
		this.platformComboBox.Size = new System.Drawing.Size(121, 21);
		this.platformComboBox.TabIndex = 3;
		this.platformComboBox.SelectedIndexChanged += new System.EventHandler(PlatformComboBoxSelectedIndexChanged);
		this.label2.Location = new System.Drawing.Point(265, 9);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(64, 23);
		this.label2.TabIndex = 2;
		this.label2.Text = "${res:Dialog.ProjectOptions.Platform}:";
		this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.configurationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.configurationComboBox.FormattingEnabled = true;
		this.configurationComboBox.Location = new System.Drawing.Point(138, 6);
		this.configurationComboBox.Name = "configurationComboBox";
		this.configurationComboBox.Size = new System.Drawing.Size(121, 21);
		this.configurationComboBox.TabIndex = 1;
		this.configurationComboBox.SelectedIndexChanged += new System.EventHandler(ConfigurationComboBoxSelectedIndexChanged);
		this.label1.Location = new System.Drawing.Point(3, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(138, 23);
		this.label1.TabIndex = 0;
		this.label1.Text = "${res:Dialog.Options.CombineOptions.Configurations.SolutionConfiguration}";
		this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.grid.AllowUserToAddRows = false;
		this.grid.AllowUserToDeleteRows = false;
		this.grid.AllowUserToResizeRows = false;
		this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.grid.Columns.AddRange(this.projectNameColumn, this.configurationColumn, this.platformColumn);
		this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
		this.grid.Location = new System.Drawing.Point(0, 37);
		this.grid.Name = "grid";
		this.grid.Size = new System.Drawing.Size(504, 192);
		this.grid.TabIndex = 1;
		this.grid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(GridCellValueChanged);
		this.grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(GridDataError);
		this.grid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(GridEditingControlShowing);
		this.projectNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.projectNameColumn.HeaderText = "${res:Dialog.SelectReferenceDialog.ProjectReferencePanel.NameHeader}";
		this.projectNameColumn.Name = "projectNameColumn";
		this.projectNameColumn.ReadOnly = true;
		this.configurationColumn.HeaderText = "${res:Dialog.Options.CombineOptions.Configurations.ConfigurationColumnHeader}";
		this.configurationColumn.Name = "configurationColumn";
		this.platformColumn.HeaderText = "${res:Dialog.Options.CombineOptions.Configurations.PlatformColumnHeader}";
		this.platformColumn.Name = "platformColumn";
		this.panel2.Controls.Add(this.okButton);
		this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panel2.Location = new System.Drawing.Point(0, 229);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(504, 30);
		this.panel2.TabIndex = 2;
		this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Location = new System.Drawing.Point(417, 3);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(75, 23);
		this.okButton.TabIndex = 0;
		this.okButton.Text = "${res:Global.OKButtonText}";
		this.okButton.UseVisualStyleBackColor = true;
		base.AcceptButton = this.okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(504, 259);
		base.Controls.Add(this.grid);
		base.Controls.Add(this.panel2);
		base.Controls.Add(this.panel1);
		this.MinimumSize = new System.Drawing.Size(457, 145);
		base.Name = "SolutionConfigurationEditor";
		base.ShowInTaskbar = false;
		this.Text = "${res:Dialog.Options.CombineOptions.Configurations.ConfigurationEditor}";
		this.panel1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grid).EndInit();
		this.panel2.ResumeLayout(false);
		base.ResumeLayout(false);
	}

	public SolutionConfigurationEditor()
	{
		solution = ProjectService.OpenSolution;
		if (solution == null)
		{
			throw new Exception("A solution must be opened");
		}
		InitializeComponent();
		Text = StringParser.Parse(Text);
		label1.Text = StringParser.Parse(label1.Text);
		label2.Text = StringParser.Parse(label2.Text);
		okButton.Text = StringParser.Parse(okButton.Text);
		projectNameColumn.HeaderText = StringParser.Parse(projectNameColumn.HeaderText);
		configurationColumn.HeaderText = StringParser.Parse(configurationColumn.HeaderText);
		platformColumn.HeaderText = StringParser.Parse(platformColumn.HeaderText);
		inUpdate = true;
		UpdateAvailableSolutionConfigurationPlatforms();
		foreach (IProject project in solution.Projects)
		{
			DataGridViewRow dataGridViewRow = grid.Rows[grid.Rows.Add()];
			dataGridViewRow.Tag = project;
			dataGridViewRow.Cells[0].Value = project.Name;
		}
		UpdateGrid();
	}

	private void UpdateAvailableSolutionConfigurationPlatforms()
	{
		SetItems(configurationComboBox.Items, solution.GetConfigurationNames());
		SetItems(platformComboBox.Items, solution.GetPlatformNames());
		SelectElement(configurationComboBox, solution.Preferences.ActiveConfiguration);
		SelectElement(platformComboBox, solution.Preferences.ActivePlatform);
		configurationComboBoxEditIndex = configurationComboBox.Items.Add("<Edit>");
		platformComboBoxEditIndex = platformComboBox.Items.Add("<Edit>");
	}

	private void SetItems(IList items, IEnumerable<string> elements)
	{
		items.Clear();
		foreach (string element in elements)
		{
			items.Add(element);
		}
	}

	private void SelectElement(ComboBox box, string itemName)
	{
		box.SelectedIndex = box.Items.IndexOf(itemName);
	}

	private void SelectElement(DataGridViewComboBoxCell box, string itemName)
	{
		if (box.Items.IndexOf(itemName) == -1)
		{
			if (itemName == "Any CPU" && box.Items.IndexOf("AnyCPU") >= 0)
			{
				box.Value = "AnyCPU";
			}
			else
			{
				box.Value = box.Items[0];
			}
		}
		else
		{
			box.Value = itemName;
		}
	}

	private void UpdateGrid()
	{
		inUpdate = true;
		Dictionary<IProject, Solution.ProjectConfigurationPlatformMatching> dictionary = new Dictionary<IProject, Solution.ProjectConfigurationPlatformMatching>();
		foreach (Solution.ProjectConfigurationPlatformMatching activeConfigurationsAndPlatformsForProject in solution.GetActiveConfigurationsAndPlatformsForProjects(configurationComboBox.Text, platformComboBox.Text))
		{
			dictionary[activeConfigurationsAndPlatformsForProject.Project] = activeConfigurationsAndPlatformsForProject;
		}
		foreach (DataGridViewRow item in (IEnumerable)grid.Rows)
		{
			IProject project = (IProject)item.Tag;
			if (!dictionary.TryGetValue(project, out var value))
			{
				value = new Solution.ProjectConfigurationPlatformMatching(project, project.ActiveConfiguration, project.ActivePlatform, null);
			}
			DataGridViewComboBoxCell dataGridViewComboBoxCell = (DataGridViewComboBoxCell)item.Cells[1];
			dataGridViewComboBoxCell.Tag = value;
			SetItems(dataGridViewComboBoxCell.Items, project.ConfigurationNames);
			SelectElement(dataGridViewComboBoxCell, value.Configuration);
			dataGridViewComboBoxCell.Items.Add(EditTag.Instance);
			DataGridViewComboBoxCell dataGridViewComboBoxCell2 = (DataGridViewComboBoxCell)item.Cells[2];
			dataGridViewComboBoxCell2.Tag = value;
			SetItems(dataGridViewComboBoxCell2.Items, project.PlatformNames);
			SelectElement(dataGridViewComboBoxCell2, value.Platform);
			dataGridViewComboBoxCell2.Items.Add(EditTag.Instance);
		}
		inUpdate = false;
	}

	private void ConfigurationComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		if (inUpdate)
		{
			return;
		}
		inUpdate = true;
		if (configurationComboBox.SelectedIndex == configurationComboBoxEditIndex)
		{
			using (Form form = new EditAvailableConfigurationsDialog(solution, editPlatforms: false))
			{
				form.ShowDialog(this);
			}
			UpdateAvailableSolutionConfigurationPlatforms();
		}
		UpdateGrid();
	}

	private void PlatformComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		if (inUpdate)
		{
			return;
		}
		inUpdate = true;
		if (platformComboBox.SelectedIndex == platformComboBoxEditIndex)
		{
			using (Form form = new EditAvailableConfigurationsDialog(solution, editPlatforms: true))
			{
				form.ShowDialog(this);
			}
			UpdateAvailableSolutionConfigurationPlatforms();
		}
		UpdateGrid();
	}

	private void GridDataError(object sender, DataGridViewDataErrorEventArgs e)
	{
		e.ThrowException = true;
	}

	private void GridCellValueChanged(object sender, DataGridViewCellEventArgs e)
	{
		if (inUpdate || e.RowIndex < 0)
		{
			return;
		}
		DataGridViewRow dataGridViewRow = grid.Rows[e.RowIndex];
		DataGridViewCell dataGridViewCell = dataGridViewRow.Cells[e.ColumnIndex];
		if (dataGridViewCell.Tag is Solution.ProjectConfigurationPlatformMatching projectConfigurationPlatformMatching)
		{
			if (e.ColumnIndex == configurationColumn.Index)
			{
				projectConfigurationPlatformMatching.Configuration = dataGridViewCell.Value.ToString();
			}
			else
			{
				projectConfigurationPlatformMatching.Platform = dataGridViewCell.Value.ToString();
			}
			if (projectConfigurationPlatformMatching.Platform == "AnyCPU")
			{
				projectConfigurationPlatformMatching.Platform = "Any CPU";
			}
			if (projectConfigurationPlatformMatching.SolutionItem == null)
			{
				projectConfigurationPlatformMatching.SolutionItem = solution.CreateMatchingItem(configurationComboBox.Text, platformComboBox.Text, projectConfigurationPlatformMatching.Project, "");
			}
			projectConfigurationPlatformMatching.SetProjectConfigurationPlatform(solution.GetProjectConfigurationsSection(), projectConfigurationPlatformMatching.Configuration, projectConfigurationPlatformMatching.Platform);
		}
	}

	private void GridEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
	{
		GridEditingControl = e.Control as ComboBox;
	}

	private void GridEditingControlSelectedIndexChanged(object sender, EventArgs e)
	{
		if (gridEditingControl.SelectedItem == EditTag.Instance && grid.CurrentCell is DataGridViewComboBoxCell { Tag: Solution.ProjectConfigurationPlatformMatching tag } dataGridViewComboBoxCell)
		{
			inUpdate = true;
			using (Form form = new EditAvailableConfigurationsDialog(tag.Project, dataGridViewComboBoxCell.ColumnIndex != configurationColumn.Index))
			{
				form.ShowDialog(this);
			}
			grid.EndEdit();
			inUpdate = true;
			grid.EndEdit();
			grid.CurrentCell = grid.Rows[dataGridViewComboBoxCell.RowIndex].Cells[0];
			dataGridViewComboBoxCell.Value = null;
			UpdateAvailableSolutionConfigurationPlatforms();
			UpdateGrid();
		}
	}
}

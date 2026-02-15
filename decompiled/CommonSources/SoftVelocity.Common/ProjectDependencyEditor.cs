using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CommonSources.Properties;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Common;

public class ProjectDependencyEditor : Form
{
	private List<IProject> projectsListByName = new List<IProject>();

	private List<IProject> projectsListByDependency = new List<IProject>();

	private ProjectDependencyEditorHelper depComp = new ProjectDependencyEditorHelper();

	private IProject firstProject;

	private bool loadingData;

	private bool selectingByCode;

	private IContainer components;

	private ListBox projectsListListBox;

	private CheckedListBox dependencyListListBox;

	private ComboBox projectsComboBox;

	private Label sortedListLabel;

	private Button buttonAccept;

	private Button buttonCancel;

	private TableLayoutPanel tableLayoutPanel1;

	private TableLayoutPanel tableLayoutPanel2;

	private TableLayoutPanel tableLayoutPanel3;

	private Button buttonDown;

	private Button buttonUp;

	private List<IProject> ProjectsListByName => projectsListByName;

	private List<IProject> ProjectsListByDependency => projectsListByDependency;

	private IProject FirstProject
	{
		set
		{
			firstProject = value;
		}
	}

	public static void Open()
	{
		Open(null);
	}

	public static void OpenFromSelected()
	{
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		if (selectedNode == null || selectedNode is ISolutionFolderNode)
		{
			Open(null);
			return;
		}
		IProject project = selectedNode.Project;
		Open(project);
	}

	private static void Open(IProject selected)
	{
		if (ProjectService.OpenSolution == null || ProjectBrowserPad.Instance.ProjectBrowserControl.RootNode == null)
		{
			return;
		}
		using ProjectDependencyEditor projectDependencyEditor = new ProjectDependencyEditor();
		projectDependencyEditor.FirstProject = selected;
		if (projectDependencyEditor.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			ProjectService.OpenSolution.Save();
		}
	}

	private ProjectDependencyEditor()
	{
		InitializeComponent();
		Text = ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.FormTitle");
		sortedListLabel.Text = ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.SortedListLabel");
		buttonAccept.Text = ResourceService.GetString("Global.OKButtonText");
		buttonCancel.Text = ResourceService.GetString("Global.CancelButtonText");
		projectsListListBox.ItemHeight = dependencyListListBox.ItemHeight;
		FileAttributes attributes = File.GetAttributes(ProjectService.OpenSolution.FileName);
		if ((attributes & FileAttributes.ReadOnly) != 0)
		{
			buttonAccept.Enabled = false;
		}
	}

	private void ProjectDependencyEditor_Load(object sender, EventArgs e)
	{
		selectingByCode = true;
		ProjectDependencyEditorHelper.InitData();
		projectsListByDependency = new List<IProject>(ProjectService.OpenSolution.Projects);
		projectsListByName = new List<IProject>(ProjectService.OpenSolution.Projects);
		projectsListByName.Sort((IProject a1, IProject a2) => ((ISolutionFolder)a1).Name.CompareTo(((ISolutionFolder)a2).Name));
		foreach (IProject item in projectsListByName)
		{
			projectsComboBox.Items.Add(((ISolutionFolder)item).Name);
		}
		selectingByCode = false;
		RefreshProjectsList();
		if (projectsListListBox.Items.Count > 0)
		{
			if (firstProject == null)
			{
				projectsComboBox.SelectedIndex = 0;
			}
			else
			{
				SelectProjectInListByName(firstProject);
				SelectProjectInListByDependency(firstProject);
			}
		}
		firstProject = null;
	}

	private void RefreshProjectsList()
	{
		if (loadingData)
		{
			return;
		}
		selectingByCode = true;
		depComp.Refresh();
		projectsListByDependency.Sort(depComp);
		projectsListListBox.SuspendLayout();
		projectsListListBox.Items.Clear();
		foreach (IProject item in projectsListByDependency)
		{
			projectsListListBox.Items.Add(((ISolutionFolder)item).Name);
		}
		projectsListListBox.ResumeLayout();
		selectingByCode = false;
	}

	private void RefreshDependencyList()
	{
		IProject val = projectsListByName[projectsComboBox.SelectedIndex];
		if (val != null)
		{
			RefreshDependencyList(val);
			SelectProjectInListByDependency(val);
		}
	}

	private void RefreshDependencyList(IProject proj)
	{
		loadingData = true;
		dependencyListListBox.SuspendLayout();
		dependencyListListBox.Items.Clear();
		foreach (IProject item in projectsListByName)
		{
			if (((ISolutionFolder)proj).IdGuid.Equals(((ISolutionFolder)item).IdGuid, StringComparison.OrdinalIgnoreCase))
			{
				dependencyListListBox.Items.Add(((ISolutionFolder)item).Name, CheckState.Unchecked);
			}
			else if (ProjectDependencyEditorHelper.IsParentProject(proj, item))
			{
				if (ProjectDependencyEditorHelper.IsCircularRefence(proj, item))
				{
					dependencyListListBox.Items.Add(((ISolutionFolder)item).Name, CheckState.Indeterminate);
				}
				else
				{
					dependencyListListBox.Items.Add(((ISolutionFolder)item).Name, CheckState.Checked);
				}
			}
			else
			{
				dependencyListListBox.Items.Add(((ISolutionFolder)item).Name, CheckState.Unchecked);
			}
		}
		dependencyListListBox.ResumeLayout();
		loadingData = false;
	}

	private void dependencyListListBox_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		if (loadingData)
		{
			return;
		}
		IProject val = projectsListByName[e.Index];
		if (val == null)
		{
			return;
		}
		IProject val2 = projectsListByName[projectsComboBox.SelectedIndex];
		if (val2 == null)
		{
			return;
		}
		if (((ISolutionFolder)val2).IdGuid.Equals(((ISolutionFolder)val).IdGuid, StringComparison.OrdinalIgnoreCase))
		{
			MessageBox.Show(WorkbenchSingleton.MainForm, ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MessageIsCircularReference"), ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MessageTitle"), MessageBoxButtons.OK);
			e.NewValue = e.CurrentValue;
		}
		else if (e.CurrentValue == CheckState.Checked || e.CurrentValue == CheckState.Indeterminate)
		{
			if (ProjectDependencyEditorHelper.IsReferenced(val2, val))
			{
				MessageBox.Show(WorkbenchSingleton.MainForm, string.Format(ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MessageIsReference"), ((ISolutionFolder)val).Name, ((ISolutionFolder)val2).Name), ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MessageTitle"), MessageBoxButtons.OK);
				e.NewValue = e.CurrentValue;
			}
			else
			{
				ProjectDependencyEditorHelper.RemoveTempDependency(val2, val);
				RefreshProjectsList();
			}
		}
		else if (ProjectDependencyEditorHelper.IsCircularRefence(val2, val))
		{
			MessageBox.Show(WorkbenchSingleton.MainForm, ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MessageIsCircularReference"), ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MessageTitle"), MessageBoxButtons.OK);
			e.NewValue = e.CurrentValue;
		}
		else
		{
			ProjectDependencyEditorHelper.AddTempDependency(val2, val);
			RefreshProjectsList();
		}
	}

	private void projectsComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (projectsComboBox.SelectedIndex > -1)
		{
			RefreshDependencyList();
		}
	}

	private void projectsListListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (projectsListListBox.SelectedIndex > -1)
		{
			IProject p = projectsListByDependency[projectsListListBox.SelectedIndex];
			SelectProjectInListByName(p);
		}
	}

	private void SelectProjectInListByName(IProject p)
	{
		if (selectingByCode)
		{
			return;
		}
		selectingByCode = true;
		int num = 0;
		foreach (IProject item in projectsListByName)
		{
			if (!(((ISolutionFolder)item).IdGuid == ((ISolutionFolder)p).IdGuid))
			{
				num++;
				continue;
			}
			break;
		}
		if (projectsComboBox.Items.Count >= num + 1)
		{
			projectsComboBox.SelectedIndex = num;
		}
		selectingByCode = false;
	}

	private void SelectProjectInListByDependency(IProject p)
	{
		if (selectingByCode)
		{
			return;
		}
		selectingByCode = true;
		int num = 0;
		foreach (IProject item in projectsListByDependency)
		{
			if (!(((ISolutionFolder)item).IdGuid == ((ISolutionFolder)p).IdGuid))
			{
				num++;
				continue;
			}
			break;
		}
		if (projectsListListBox.Items.Count >= num + 1)
		{
			projectsListListBox.SelectedIndex = num;
		}
		selectingByCode = false;
	}

	private void buttonAccept_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void buttonCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void ProjectDependencyEditor_FormClosed(object sender, FormClosedEventArgs e)
	{
		if (base.DialogResult == DialogResult.OK)
		{
			ProjectDependencyEditorHelper.UpdateDependenciesOnSolution();
		}
		else
		{
			ProjectDependencyEditorHelper.FinishData();
		}
	}

	private void projectsListListBox_DrawItem(object sender, DrawItemEventArgs e)
	{
		e.DrawBackground();
		e.Graphics.DrawString(projectsListListBox.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
		e.DrawFocusRectangle();
	}

	private void buttonDown_Click(object sender, EventArgs e)
	{
		int selectedIndex = projectsListListBox.SelectedIndex;
		if (moveItemDown(selectedIndex))
		{
			RefreshProjectsList();
			RefreshDependencyList();
			projectsListListBox.SelectedIndex = selectedIndex + 1;
		}
	}

	private void buttonUp_Click(object sender, EventArgs e)
	{
		int selectedIndex = projectsListListBox.SelectedIndex - 1;
		if (moveItemDown(selectedIndex))
		{
			RefreshProjectsList();
			RefreshDependencyList();
			projectsListListBox.SelectedIndex = selectedIndex;
		}
	}

	private bool moveItemDown(int selectedIndex)
	{
		IProject val = projectsListByDependency[selectedIndex];
		if (val != null && selectedIndex > -1 && selectedIndex < projectsListByDependency.Count - 1)
		{
			IProject val2 = projectsListByDependency[selectedIndex + 1];
			if (!ProjectDependencyEditorHelper.IsCircularRefence(val, val2))
			{
				ProjectDependencyEditorHelper.AddTempDependency(val, val2);
				return true;
			}
			if (ProjectDependencyEditorHelper.IsReferenced(val2, val))
			{
				MessageBox.Show(WorkbenchSingleton.MainForm, ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MessageIsCircularReference"), ResourceService.GetString("SoftVelocity.Common.ProjectDependencyEditor.MessageTitle"), MessageBoxButtons.OK);
				return false;
			}
			ProjectDependencyEditorHelper.RemoveTempDependency(val2, val);
			ProjectDependencyEditorHelper.AddTempDependency(val, val2);
			return true;
		}
		return false;
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
		this.projectsListListBox = new System.Windows.Forms.ListBox();
		this.dependencyListListBox = new System.Windows.Forms.CheckedListBox();
		this.projectsComboBox = new System.Windows.Forms.ComboBox();
		this.sortedListLabel = new System.Windows.Forms.Label();
		this.buttonAccept = new System.Windows.Forms.Button();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
		this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
		this.buttonDown = new System.Windows.Forms.Button();
		this.buttonUp = new System.Windows.Forms.Button();
		this.tableLayoutPanel1.SuspendLayout();
		this.tableLayoutPanel2.SuspendLayout();
		this.tableLayoutPanel3.SuspendLayout();
		base.SuspendLayout();
		this.projectsListListBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.projectsListListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.projectsListListBox.FormattingEnabled = true;
		this.projectsListListBox.ItemHeight = 17;
		this.projectsListListBox.Location = new System.Drawing.Point(331, 48);
		this.projectsListListBox.Margin = new System.Windows.Forms.Padding(4);
		this.projectsListListBox.Name = "projectsListListBox";
		this.projectsListListBox.Size = new System.Drawing.Size(317, 310);
		this.projectsListListBox.TabIndex = 0;
		this.projectsListListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(projectsListListBox_DrawItem);
		this.projectsListListBox.SelectedIndexChanged += new System.EventHandler(projectsListListBox_SelectedIndexChanged);
		this.dependencyListListBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dependencyListListBox.FormattingEnabled = true;
		this.dependencyListListBox.Location = new System.Drawing.Point(7, 48);
		this.dependencyListListBox.Margin = new System.Windows.Forms.Padding(4);
		this.dependencyListListBox.Name = "dependencyListListBox";
		this.dependencyListListBox.Size = new System.Drawing.Size(316, 310);
		this.dependencyListListBox.TabIndex = 4;
		this.dependencyListListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(dependencyListListBox_ItemCheck);
		this.projectsComboBox.Dock = System.Windows.Forms.DockStyle.Top;
		this.projectsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.projectsComboBox.FormattingEnabled = true;
		this.projectsComboBox.Location = new System.Drawing.Point(7, 7);
		this.projectsComboBox.Margin = new System.Windows.Forms.Padding(4);
		this.projectsComboBox.Name = "projectsComboBox";
		this.projectsComboBox.Size = new System.Drawing.Size(316, 24);
		this.projectsComboBox.TabIndex = 5;
		this.projectsComboBox.SelectedIndexChanged += new System.EventHandler(projectsComboBox_SelectedIndexChanged);
		this.sortedListLabel.AutoSize = true;
		this.sortedListLabel.Location = new System.Drawing.Point(3, 8);
		this.sortedListLabel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
		this.sortedListLabel.Name = "sortedListLabel";
		this.sortedListLabel.Size = new System.Drawing.Size(196, 17);
		this.sortedListLabel.TabIndex = 6;
		this.sortedListLabel.Text = "Sorted Projects in Build Order";
		this.buttonAccept.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.buttonAccept.Location = new System.Drawing.Point(154, 11);
		this.buttonAccept.Name = "buttonAccept";
		this.buttonAccept.Size = new System.Drawing.Size(78, 24);
		this.buttonAccept.TabIndex = 7;
		this.buttonAccept.Text = "&Accept";
		this.buttonAccept.UseVisualStyleBackColor = true;
		this.buttonAccept.Click += new System.EventHandler(buttonAccept_Click);
		this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(238, 11);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(78, 24);
		this.buttonCancel.TabIndex = 8;
		this.buttonCancel.Text = "&Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
		this.tableLayoutPanel1.ColumnCount = 2;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.Controls.Add(this.projectsComboBox, 0, 0);
		this.tableLayoutPanel1.Controls.Add(this.projectsListListBox, 1, 1);
		this.tableLayoutPanel1.Controls.Add(this.dependencyListListBox, 0, 1);
		this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 2);
		this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
		this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
		this.tableLayoutPanel1.RowCount = 3;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.Size = new System.Drawing.Size(655, 410);
		this.tableLayoutPanel1.TabIndex = 9;
		this.tableLayoutPanel2.ColumnCount = 2;
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 1, 0);
		this.tableLayoutPanel2.Controls.Add(this.buttonAccept, 0, 0);
		this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.tableLayoutPanel2.Location = new System.Drawing.Point(330, 366);
		this.tableLayoutPanel2.Name = "tableLayoutPanel2";
		this.tableLayoutPanel2.RowCount = 1;
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.Size = new System.Drawing.Size(319, 38);
		this.tableLayoutPanel2.TabIndex = 7;
		this.tableLayoutPanel3.ColumnCount = 3;
		this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel3.Controls.Add(this.sortedListLabel, 0, 0);
		this.tableLayoutPanel3.Controls.Add(this.buttonDown, 1, 0);
		this.tableLayoutPanel3.Controls.Add(this.buttonUp, 2, 0);
		this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel3.Location = new System.Drawing.Point(330, 6);
		this.tableLayoutPanel3.Name = "tableLayoutPanel3";
		this.tableLayoutPanel3.RowCount = 1;
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel3.Size = new System.Drawing.Size(319, 35);
		this.tableLayoutPanel3.TabIndex = 8;
		this.buttonDown.Image = CommonSources.Properties.Resources.arrowdown;
		this.buttonDown.Location = new System.Drawing.Point(246, 3);
		this.buttonDown.Name = "buttonDown";
		this.buttonDown.Size = new System.Drawing.Size(32, 29);
		this.buttonDown.TabIndex = 7;
		this.buttonDown.UseVisualStyleBackColor = true;
		this.buttonDown.Click += new System.EventHandler(buttonDown_Click);
		this.buttonUp.Image = CommonSources.Properties.Resources.arrowup;
		this.buttonUp.Location = new System.Drawing.Point(284, 3);
		this.buttonUp.Name = "buttonUp";
		this.buttonUp.Size = new System.Drawing.Size(32, 29);
		this.buttonUp.TabIndex = 8;
		this.buttonUp.UseVisualStyleBackColor = true;
		this.buttonUp.Click += new System.EventHandler(buttonUp_Click);
		base.AcceptButton = this.buttonAccept;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.buttonCancel;
		base.ClientSize = new System.Drawing.Size(655, 410);
		base.Controls.Add(this.tableLayoutPanel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "ProjectDependencyEditor";
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Project Dependency Editor";
		base.Load += new System.EventHandler(ProjectDependencyEditor_Load);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(ProjectDependencyEditor_FormClosed);
		this.tableLayoutPanel1.ResumeLayout(false);
		this.tableLayoutPanel2.ResumeLayout(false);
		this.tableLayoutPanel3.ResumeLayout(false);
		this.tableLayoutPanel3.PerformLayout();
		base.ResumeLayout(false);
	}
}

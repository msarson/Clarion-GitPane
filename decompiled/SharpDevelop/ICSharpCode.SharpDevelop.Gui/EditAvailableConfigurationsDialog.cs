using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class EditAvailableConfigurationsDialog : Form
{
	private IContainer components;

	private Button addButton;

	private Button renameButton;

	private Button removeButton;

	private ListBox listBox;

	private Button okButton;

	private Solution solution;

	private IProject project;

	private bool editPlatforms;

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
		this.listBox = new System.Windows.Forms.ListBox();
		this.okButton = new System.Windows.Forms.Button();
		this.removeButton = new System.Windows.Forms.Button();
		this.renameButton = new System.Windows.Forms.Button();
		this.addButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.listBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.listBox.FormattingEnabled = true;
		this.listBox.IntegralHeight = false;
		this.listBox.Location = new System.Drawing.Point(12, 12);
		this.listBox.Name = "listBox";
		this.listBox.Size = new System.Drawing.Size(204, 93);
		this.listBox.TabIndex = 0;
		this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Location = new System.Drawing.Point(222, 111);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(75, 23);
		this.okButton.TabIndex = 1;
		this.okButton.Text = "${res:Global.OKButtonText}";
		this.okButton.UseVisualStyleBackColor = true;
		this.removeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.removeButton.Location = new System.Drawing.Point(222, 12);
		this.removeButton.Name = "removeButton";
		this.removeButton.Size = new System.Drawing.Size(75, 23);
		this.removeButton.TabIndex = 2;
		this.removeButton.Text = "${res:Global.RemoveButtonText}";
		this.removeButton.UseVisualStyleBackColor = true;
		this.removeButton.Click += new System.EventHandler(RemoveButtonClick);
		this.renameButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.renameButton.Location = new System.Drawing.Point(222, 41);
		this.renameButton.Name = "renameButton";
		this.renameButton.Size = new System.Drawing.Size(75, 23);
		this.renameButton.TabIndex = 3;
		this.renameButton.Text = "${res:Global.RenameButtonText}";
		this.renameButton.UseVisualStyleBackColor = true;
		this.renameButton.Click += new System.EventHandler(RenameButtonClick);
		this.addButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.addButton.Location = new System.Drawing.Point(222, 70);
		this.addButton.Name = "addButton";
		this.addButton.Size = new System.Drawing.Size(75, 23);
		this.addButton.TabIndex = 4;
		this.addButton.Text = "${res:Global.AddButtonText}...";
		this.addButton.UseVisualStyleBackColor = true;
		this.addButton.Click += new System.EventHandler(AddButtonClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(309, 146);
		base.Controls.Add(this.addButton);
		base.Controls.Add(this.renameButton);
		base.Controls.Add(this.removeButton);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.listBox);
		this.MinimumSize = new System.Drawing.Size(230, 165);
		base.Name = "EditAvailableConfigurationsDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "EditAvailableConfigurationsDialog";
		base.ResumeLayout(false);
	}

	private EditAvailableConfigurationsDialog()
	{
		InitializeComponent();
		foreach (Control control in base.Controls)
		{
			control.Text = StringParser.Parse(control.Text);
		}
	}

	public EditAvailableConfigurationsDialog(Solution solution, bool editPlatforms)
		: this()
	{
		this.solution = solution;
		this.editPlatforms = editPlatforms;
		InitList();
		if (editPlatforms)
		{
			Text = "Edit Solution Platforms";
		}
		else
		{
			Text = "Edit Solution Configurations";
		}
	}

	public EditAvailableConfigurationsDialog(IProject project, bool editPlatforms)
		: this()
	{
		this.project = project;
		solution = project.ParentSolution;
		this.editPlatforms = editPlatforms;
		InitList();
		if (editPlatforms)
		{
			Text = "Edit Project Platforms";
		}
		else
		{
			Text = "Edit Project Configurations";
		}
	}

	private void InitList()
	{
		if (project != null)
		{
			if (editPlatforms)
			{
				ShowEntries(project.PlatformNames, project.ActivePlatform);
			}
			else
			{
				ShowEntries(project.ConfigurationNames, project.ActiveConfiguration);
			}
		}
		else if (editPlatforms)
		{
			ShowEntries(solution.GetPlatformNames(), solution.Preferences.ActivePlatform);
		}
		else
		{
			ShowEntries(solution.GetConfigurationNames(), solution.Preferences.ActiveConfiguration);
		}
	}

	private void ShowEntries(IEnumerable<string> list, string activeItem)
	{
		string[] array = Linq.ToArray(list);
		listBox.Items.Clear();
		listBox.Items.AddRange(array);
		if (listBox.Items.Count == 0)
		{
			throw new Exception("There must be at least one configuration/platform");
		}
		listBox.SelectedIndex = Math.Max(Array.IndexOf(array, activeItem), 0);
	}

	private void RemoveButtonClick(object sender, EventArgs e)
	{
		if (listBox.Items.Count == 1)
		{
			MessageService.ShowMessage("You cannot delete all configurations/platforms.");
		}
		string text = listBox.SelectedItem.ToString();
		if (MessageService.AskQuestionFormatted("Do you really want to remove '{0}'?", new string[1] { text }))
		{
			if (project != null)
			{
				Remove(project, text, editPlatforms);
			}
			else
			{
				Remove(solution, text, editPlatforms);
			}
			InitList();
		}
	}

	private static void Remove(IProject project, string name, bool isPlatform)
	{
		if (isPlatform)
		{
			project.ParentSolution.RemoveProjectPlatform(project, name);
		}
		else
		{
			project.ParentSolution.RemoveProjectConfiguration(project, name);
		}
	}

	private static void Remove(Solution solution, string name, bool isPlatform)
	{
		if (isPlatform)
		{
			solution.RemoveSolutionPlatform(name);
		}
		else
		{
			solution.RemoveSolutionConfiguration(name);
		}
	}

	private void RenameButtonClick(object sender, EventArgs e)
	{
		string text = listBox.SelectedItem.ToString();
		string newName = MessageService.ShowInputBox("${res:SharpDevelop.Refactoring.Rename}", "Enter the new name:", text);
		if (string.IsNullOrEmpty(newName) || newName == text || !EnsureCorrectName(ref newName))
		{
			return;
		}
		if (project != null)
		{
			Rename(project, text, newName);
		}
		else
		{
			if (editPlatforms)
			{
				solution.RenameSolutionPlatform(text, newName);
				if (solution.Preferences.ActivePlatform == text)
				{
					solution.Preferences.ActivePlatform = newName;
				}
			}
			else
			{
				solution.RenameSolutionConfiguration(text, newName);
				if (solution.Preferences.ActiveConfiguration == text)
				{
					solution.Preferences.ActiveConfiguration = newName;
				}
			}
			foreach (IProject project in solution.Projects)
			{
				Rename(project, text, newName);
			}
		}
		InitList();
	}

	private void Rename(IProject project, string oldName, string newName)
	{
		if (editPlatforms)
		{
			if (!project.PlatformNames.Contains(newName))
			{
				solution.RenameProjectPlatform(project, oldName, newName);
			}
		}
		else if (!project.ConfigurationNames.Contains(newName))
		{
			solution.RenameProjectConfiguration(project, oldName, newName);
		}
	}

	private bool EnsureCorrectName(ref string newName)
	{
		newName = newName.Trim();
		if (editPlatforms && string.Equals(newName, "AnyCPU", StringComparison.InvariantCultureIgnoreCase))
		{
			newName = "Any CPU";
		}
		foreach (string item in listBox.Items)
		{
			if (string.Equals(item, newName, StringComparison.InvariantCultureIgnoreCase))
			{
				MessageService.ShowMessage("Duplicate name.");
				return false;
			}
		}
		if (MSBuildInternals.Escape(newName) != newName || !FileUtility.IsValidDirectoryName(newName) || newName.Contains("'"))
		{
			MessageService.ShowMessage("The name was invalid.");
			return false;
		}
		return true;
	}

	private void AddButtonClick(object sender, EventArgs e)
	{
		using AddNewConfigurationDialog addNewConfigurationDialog = new AddNewConfigurationDialog(availableSourceItems: (project != null) ? ((!editPlatforms) ? project.ConfigurationNames : project.PlatformNames) : ((!editPlatforms) ? solution.GetConfigurationNames() : solution.GetPlatformNames()), solution: project == null, editPlatforms: editPlatforms, checkNameValid: (string name) => EnsureCorrectName(ref name));
		if (addNewConfigurationDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		string newName = addNewConfigurationDialog.NewName;
		if (!EnsureCorrectName(ref newName))
		{
			return;
		}
		if (project != null)
		{
			if (project is IProjectAllowChangeConfigurations projectAllowChangeConfigurations)
			{
				if (editPlatforms)
				{
					projectAllowChangeConfigurations.AddProjectPlatform(newName, addNewConfigurationDialog.CopyFrom);
				}
				else
				{
					projectAllowChangeConfigurations.AddProjectConfiguration(newName, addNewConfigurationDialog.CopyFrom);
				}
			}
		}
		else if (editPlatforms)
		{
			solution.AddSolutionPlatform(newName, addNewConfigurationDialog.CopyFrom, addNewConfigurationDialog.CreateInAllProjects);
		}
		else
		{
			solution.AddSolutionConfiguration(newName, addNewConfigurationDialog.CopyFrom, addNewConfigurationDialog.CreateInAllProjects);
		}
		InitList();
	}
}

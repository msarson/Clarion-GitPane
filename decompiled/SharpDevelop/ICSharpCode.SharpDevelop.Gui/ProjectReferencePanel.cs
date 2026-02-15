using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class ProjectReferencePanel : ListView, IReferencePanel
{
	private ISelectReferenceDialog selectDialog;

	private IProject callingProject;

	public ProjectReferencePanel(ISelectReferenceDialog selectDialog, IProject callingProject)
	{
		this.selectDialog = selectDialog;
		this.callingProject = callingProject;
		ColumnHeader value = new ColumnHeader
		{
			Text = ResourceService.GetString("Dialog.SelectReferenceDialog.ProjectReferencePanel.NameHeader"),
			Width = 170
		};
		base.Columns.Add(value);
		ColumnHeader value2 = new ColumnHeader
		{
			Text = ResourceService.GetString("Dialog.SelectReferenceDialog.ProjectReferencePanel.DirectoryHeader"),
			Width = 290
		};
		base.Columns.Add(value2);
		base.View = View.Details;
		Dock = DockStyle.Fill;
		base.FullRowSelect = true;
		EventHandler value3 = delegate
		{
			AddReference();
		};
		base.ItemActivate += value3;
		PopulateListView();
	}

	public void AddReference()
	{
		foreach (ListViewItem selectedItem in base.SelectedItems)
		{
			IProject project = (IProject)selectedItem.Tag;
			LanguageBindingService.GetBindingPerLanguageName(project.Language);
			selectDialog.AddReference(ReferenceType.Project, project.Name, project.OutputAssemblyFullPath, project);
		}
	}

	private void PopulateListView()
	{
		if (ProjectService.OpenSolution == null)
		{
			return;
		}
		foreach (IProject project in ProjectService.OpenSolution.Projects)
		{
			if (project != callingProject)
			{
				ListViewItem listViewItem = new ListViewItem(new string[2] { project.Name, project.Directory });
				listViewItem.Tag = project;
				base.Items.Add(listViewItem);
			}
		}
	}
}

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project.Commands;

namespace ICSharpCode.SharpDevelop.Project;

public class ProjectNode : DirectoryNode
{
	private IProject project;

	private bool isStartupProject;

	public override bool Visible => true;

	public override IProject Project => project;

	public override string RelativePath => "";

	public override string Directory
	{
		get
		{
			return project.Directory;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override DataObject DragDropDataObject => new DataObject(this);

	public override bool EnableDelete => true;

	public override bool EnableCopy => false;

	public override bool EnableCut
	{
		get
		{
			if (base.IsEditing)
			{
				return false;
			}
			return true;
		}
	}

	public ProjectNode(IProject project)
	{
		sortOrder = 1;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/ProjectNode";
		this.project = project;
		base.Text = project.Name;
		autoClearNodes = false;
		if (project is MissingProject)
		{
			base.OpenedImage = (base.ClosedImage = "ProjectBrowser.MissingProject");
			ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/MissingProjectNode";
		}
		else if (project is UnknownProject)
		{
			base.OpenedImage = (base.ClosedImage = "ProjectBrowser.ProjectWarning");
			ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/UnknownProjectNode";
		}
		else
		{
			base.OpenedImage = (base.ClosedImage = IconService.GetImageForProjectType(project.Language));
		}
		base.Tag = project;
		if (project.ParentSolution != null)
		{
			project.ParentSolution.Preferences.StartupProjectChanged += OnStartupProjectChanged;
			OnStartupProjectChanged(null, null);
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		if (project.ParentSolution != null)
		{
			project.ParentSolution.Preferences.StartupProjectChanged -= OnStartupProjectChanged;
		}
	}

	private void OnStartupProjectChanged(object sender, EventArgs e)
	{
		bool flag = project == project.ParentSolution.Preferences.StartupProject;
		if (flag != isStartupProject)
		{
			isStartupProject = flag;
			drawDefault = !isStartupProject;
			if (base.TreeView != null)
			{
				base.TreeView.Invalidate(base.Bounds);
			}
		}
	}

	protected override int MeasureItemWidth(DrawTreeNodeEventArgs e)
	{
		if (isStartupProject)
		{
			return MeasureTextWidth(e.Graphics, base.Text, ExtTreeNode.BoldDefaultFont);
		}
		return base.MeasureItemWidth(e);
	}

	protected override void DrawForeground(DrawTreeNodeEventArgs e, float x)
	{
		if (isStartupProject)
		{
			DrawForegroundExpandImg(e, ref x);
			DrawForegroundIcon(e, ref x);
			DrawText(e, base.Text, SystemBrushes.WindowText, ExtTreeNode.BoldDefaultFont, ref x);
		}
		else
		{
			base.DrawForeground(e, x);
		}
	}

	public override void ActivateItem()
	{
		if (project is UnknownProject && base.Nodes.Count == 0)
		{
			FileService.OpenFile(project.FileName);
		}
	}

	public override void ShowProperties()
	{
		ViewProjectOptions.ShowProjectOptions(project);
	}

	public override void Delete()
	{
		ProjectService.RemoveSolutionFolder(Project.IdGuid);
		ProjectService.SaveSolution();
	}

	public override void Copy()
	{
		throw new NotSupportedException();
	}

	public override void Cut()
	{
		DoPerformCut = true;
		ClipboardWrapper.SetDataObject(new DataObject(typeof(ISolutionFolder).ToString(), project.IdGuid));
	}

	public override void AfterLabelEdit(string newName)
	{
		RenameProject(project, newName);
		base.Text = project.Name;
	}

	public static void RenameProject(IProject project, string newName)
	{
		if (project.Name == newName || !FileService.CheckFileName(newName))
		{
			return;
		}
		string text = Path.Combine(project.Directory, newName + Path.GetExtension(project.FileName));
		if (!FileService.RenameFile(project.FileName, text, isDirectory: false))
		{
			return;
		}
		if (project.AssemblyName == project.Name)
		{
			project.AssemblyName = newName;
		}
		if (File.Exists(project.FileName + ".user"))
		{
			FileService.RenameFile(project.FileName + ".user", text + ".user", isDirectory: false);
		}
		foreach (IProject project2 in ProjectService.OpenSolution.Projects)
		{
			foreach (ProjectItem item in project2.Items)
			{
				if (item.ItemType == ItemType.ProjectReference)
				{
					ProjectReferenceProjectItem projectReferenceProjectItem = (ProjectReferenceProjectItem)item;
					if (projectReferenceProjectItem.ReferencedProject == project)
					{
						projectReferenceProjectItem.ProjectName = newName;
						projectReferenceProjectItem.Include = FileUtility.GetRelativePath(project2.Directory, text);
					}
				}
			}
		}
		project.FileName = text;
		project.Name = newName;
		ProjectService.SaveSolution();
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
	}

	public virtual void AddNewItemsToProject()
	{
		new AddNewItemsToProject().Run();
	}
}

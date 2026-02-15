using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddExitingProjectToSolution : AbstractMenuCommand
{
	public static void AddProject(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName) && ProjectBrowserPad.Instance.SolutionNode != null)
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.SelectedNode = ProjectBrowserPad.Instance.SolutionNode;
			AddProject(new string[1] { fileName });
		}
	}

	public static void AddProject(string[] fileNames)
	{
		if (fileNames.Length <= 0 || ProjectBrowserPad.Instance.SolutionNode == null)
		{
			return;
		}
		AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		if (abstractProjectBrowserTreeNode == null || (abstractProjectBrowserTreeNode != null && !(abstractProjectBrowserTreeNode is ISolutionFolderNode)))
		{
			abstractProjectBrowserTreeNode = ProjectBrowserPad.Instance.SolutionNode;
		}
		ISolutionFolderNode solutionFolderNode = abstractProjectBrowserTreeNode as ISolutionFolderNode;
		if (abstractProjectBrowserTreeNode == null)
		{
			return;
		}
		if (solutionFolderNode is SolutionFolderNode && solutionFolderNode.Folder.IdGuid == "{2150E333-8FDC-42A3-9474-1A3956D46DE8}" && ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.Nodes.Count > 0)
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.SelectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.Nodes[0];
			abstractProjectBrowserTreeNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
			solutionFolderNode = abstractProjectBrowserTreeNode as ISolutionFolderNode;
		}
		if (solutionFolderNode != null)
		{
			foreach (string fileName in fileNames)
			{
				AddProject(solutionFolderNode, fileName);
			}
			ProjectService.SaveSolution();
		}
	}

	public static void AddProject(ISolutionFolderNode solutionFolderNode, string fileName)
	{
		AddProject(solutionFolderNode, LanguageBindingService.LoadProject(solutionFolderNode.Solution, fileName, Path.GetFileNameWithoutExtension(fileName)));
	}

	public static void AddProject(ISolutionFolderNode solutionFolderNode, IProject newProject)
	{
		if (newProject != null)
		{
			newProject.Location = FileUtility.GetRelativePath(solutionFolderNode.Solution.Directory, newProject.FileName);
			if (ProjectService.AddProject(solutionFolderNode, newProject))
			{
				NodeBuilders.AddProjectNode((TreeNode)solutionFolderNode, newProject).EnsureVisible();
				solutionFolderNode.Solution.ApplySolutionConfigurationAndPlatformToProjects();
			}
		}
	}

	public override void Run()
	{
		if (ProjectBrowserPad.Instance.ProjectBrowserControl.RootNode == null)
		{
			return;
		}
		if (ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode == null)
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.SelectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.RootNode;
		}
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		openFileDialog.AddExtension = true;
		openFileDialog.Filter = ProjectService.GetAllProjectsFilter(this);
		openFileDialog.Multiselect = true;
		openFileDialog.CheckFileExists = true;
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			AddProject(openFileDialog.FileNames);
		}
	}
}

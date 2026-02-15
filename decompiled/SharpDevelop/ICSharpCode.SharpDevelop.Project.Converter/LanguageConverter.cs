using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Project.Commands;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project.Converter;

public abstract class LanguageConverter : AbstractMenuCommand
{
	protected StringBuilder conversionLog;

	public abstract string TargetLanguageName { get; }

	protected virtual void AfterConversion(IProject targetProject)
	{
	}

	protected virtual IProject CreateProject(string targetProjectDirectory, IProject sourceProject)
	{
		ProjectCreateInformation projectCreateInformation = new ProjectCreateInformation();
		projectCreateInformation.Solution = sourceProject.ParentSolution;
		projectCreateInformation.ProjectBasePath = targetProjectDirectory;
		projectCreateInformation.ProjectName = sourceProject.Name + ".Converted";
		projectCreateInformation.RootNamespace = sourceProject.RootNamespace;
		LanguageBindingDescriptor codonPerLanguageName = LanguageBindingService.GetCodonPerLanguageName(TargetLanguageName);
		if (codonPerLanguageName == null || codonPerLanguageName.Binding == null)
		{
			throw new InvalidOperationException("Cannot get Language Binding for " + TargetLanguageName);
		}
		projectCreateInformation.OutputProjectFileName = Path.GetFullPath(Path.Combine(targetProjectDirectory, projectCreateInformation.ProjectName + codonPerLanguageName.ProjectFileExtension));
		return codonPerLanguageName.Binding.CreateProject(projectCreateInformation);
	}

	protected virtual void ConvertFile(FileProjectItem sourceItem, FileProjectItem targetItem)
	{
		if (!File.Exists(targetItem.FileName))
		{
			File.Copy(sourceItem.FileName, targetItem.FileName);
		}
	}

	protected virtual void CopyProperties(IProject sourceProject, IProject targetProject)
	{
		MSBuildBasedProject mSBuildBasedProject = sourceProject as MSBuildBasedProject;
		MSBuildBasedProject mSBuildBasedProject2 = targetProject as MSBuildBasedProject;
		if (mSBuildBasedProject == null || mSBuildBasedProject2 == null)
		{
			return;
		}
		lock (mSBuildBasedProject.SyncRoot)
		{
			lock (mSBuildBasedProject2.SyncRoot)
			{
				mSBuildBasedProject2.MSBuildProject.RemoveAllPropertyGroups();
				foreach (BuildPropertyGroup propertyGroup in mSBuildBasedProject.MSBuildProject.PropertyGroups)
				{
					if (propertyGroup.IsImported)
					{
						continue;
					}
					BuildPropertyGroup buildPropertyGroup2 = mSBuildBasedProject2.MSBuildProject.AddNewPropertyGroup(insertAtEndOfProject: false);
					buildPropertyGroup2.Condition = propertyGroup.Condition;
					foreach (BuildProperty item in propertyGroup)
					{
						BuildProperty buildProperty2 = buildPropertyGroup2.AddNewProperty(item.Name, item.Value);
						buildProperty2.Condition = item.Condition;
					}
				}
				mSBuildBasedProject2.SetProperty("ProjectGuid", mSBuildBasedProject2.IdGuid);
			}
		}
	}

	protected void FixProperty(MSBuildBasedProject project, string propertyName, Converter<string, string> method)
	{
		lock (project.SyncRoot)
		{
			foreach (BuildProperty allProperty in project.GetAllProperties(propertyName))
			{
				allProperty.Value = method(allProperty.Value);
			}
		}
	}

	protected virtual void FixExtensionOfExtraProperties(FileProjectItem item, string sourceExtension, string targetExtension)
	{
		sourceExtension = sourceExtension.ToLowerInvariant();
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		foreach (string metadataName in item.MetadataNames)
		{
			if (!"Include".Equals(metadataName, StringComparison.OrdinalIgnoreCase))
			{
				string metadata = item.GetMetadata(metadataName);
				if (metadata.ToLowerInvariant().EndsWith(sourceExtension))
				{
					list.Add(new KeyValuePair<string, string>(metadataName, metadata));
				}
			}
		}
		foreach (KeyValuePair<string, string> item2 in list)
		{
			item.SetMetadata(item2.Key, Path.ChangeExtension(item2.Value, targetExtension));
		}
	}

	protected virtual void CopyItems(IProject sourceProject, IProject targetProject)
	{
		if (sourceProject == null)
		{
			throw new ArgumentNullException("sourceProject");
		}
		if (targetProject == null)
		{
			throw new ArgumentNullException("targetProject");
		}
		if (!(targetProject is IProjectItemListProvider projectItemListProvider))
		{
			throw new ArgumentNullException("targetProjectItems");
		}
		foreach (ProjectItem item in sourceProject.Items)
		{
			if (item is FileProjectItem fileProjectItem && FileUtility.IsBaseDirectory(sourceProject.Directory, fileProjectItem.FileName))
			{
				FileProjectItem fileProjectItem2 = new FileProjectItem(targetProject, fileProjectItem.ItemType);
				fileProjectItem.CopyMetadataTo(fileProjectItem2);
				fileProjectItem2.Include = fileProjectItem.Include;
				if (File.Exists(fileProjectItem.FileName))
				{
					if (!Directory.Exists(Path.GetDirectoryName(fileProjectItem2.FileName)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(fileProjectItem2.FileName));
					}
					ConvertFile(fileProjectItem, fileProjectItem2);
				}
				projectItemListProvider.AddProjectItem(fileProjectItem2);
			}
			else
			{
				projectItemListProvider.AddProjectItem(item.CloneFor(targetProject));
			}
		}
	}

	public override void Run()
	{
		conversionLog = new StringBuilder();
		string text = ResourceService.GetString("ICSharpCode.SharpDevelop.Commands.Convert.ProjectConverter");
		conversionLog.AppendLine(text);
		conversionLog.Append('=', text.Length);
		conversionLog.AppendLine();
		conversionLog.AppendLine();
		MSBuildBasedProject mSBuildBasedProject = ProjectService.CurrentProject as MSBuildBasedProject;
		string text2 = mSBuildBasedProject.Directory + ".ConvertedTo" + TargetLanguageName;
		if (Directory.Exists(text2))
		{
			MessageService.ShowMessageFormatted(text, "${res:ICSharpCode.SharpDevelop.Commands.Convert.TargetAlreadyExists}", text2);
			return;
		}
		conversionLog.Append(ResourceService.GetString("ICSharpCode.SharpDevelop.Commands.Convert.SourceDirectory")).Append(": ");
		conversionLog.AppendLine(mSBuildBasedProject.Directory);
		conversionLog.Append(ResourceService.GetString("ICSharpCode.SharpDevelop.Commands.Convert.TargetDirectory")).Append(": ");
		conversionLog.AppendLine(text2);
		Directory.CreateDirectory(text2);
		IProject project = CreateProject(text2, mSBuildBasedProject);
		CopyProperties(mSBuildBasedProject, project);
		conversionLog.AppendLine();
		CopyItems(mSBuildBasedProject, project);
		conversionLog.AppendLine();
		AfterConversion(project);
		conversionLog.AppendLine(ResourceService.GetString("ICSharpCode.SharpDevelop.Commands.Convert.ConversionComplete"));
		project.Save();
		project.Dispose();
		TreeNode treeNode = ProjectBrowserPad.Instance.SelectedNode;
		if (treeNode == null)
		{
			treeNode = ProjectBrowserPad.Instance.SolutionNode;
		}
		while (treeNode != null)
		{
			if (treeNode is ISolutionFolderNode)
			{
				AddExitingProjectToSolution.AddProject((ISolutionFolderNode)treeNode, project.FileName);
				ProjectService.SaveSolution();
				break;
			}
			treeNode = treeNode.Parent;
		}
		IWorkbenchWindow workbenchWindow = FileService.NewFile(ResourceService.GetString("ICSharpCode.SharpDevelop.Commands.Convert.ConversionResults"), "Text", conversionLog.ToString());
		if (workbenchWindow != null)
		{
			workbenchWindow.ViewContent.IsDirty = false;
		}
	}
}

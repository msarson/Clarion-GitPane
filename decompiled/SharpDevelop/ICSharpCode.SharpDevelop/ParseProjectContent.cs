using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class ParseProjectContent : DefaultProjectContent
{
	protected IProject project;

	protected bool initializing;

	private static Queue<MethodInvoker> callAfterAddComReference = new Queue<MethodInvoker>();

	private static bool buildingComReference;

	private int languageDefaultImportCount = -1;

	public override object Project => project;

	internal static ParseProjectContent CreateUninitalized(IProject project)
	{
		ParseProjectContent parseProjectContent = new ParseProjectContent();
		parseProjectContent.project = project;
		parseProjectContent.Language = project.LanguageProperties;
		parseProjectContent.initializing = true;
		IProjectContent mscorlib = ParserService.GetRegistryForReference(new ReferenceProjectItem(project, "mscorlib")).Mscorlib;
		parseProjectContent.AddReferencedContent(mscorlib);
		return parseProjectContent;
	}

	public override string ToString()
	{
		return $"[{GetType().Name}: {project.Name}]";
	}

	protected internal virtual void Initialize1()
	{
		ICollection<ProjectItem> items = project.Items;
		ProjectService.ProjectItemAdded += OnProjectItemAdded;
		ProjectService.ProjectItemRemoved += OnProjectItemRemoved;
		UpdateDefaultImports(items);
		foreach (ProjectItem item in items)
		{
			if (!initializing)
			{
				return;
			}
			if (item.ItemType == ItemType.Reference || item.ItemType == ItemType.ProjectReference || item.ItemType == ItemType.COMReference)
			{
				AddReference(item as ReferenceProjectItem, updateInterDependencies: false);
			}
		}
		UpdateReferenceInterDependencies();
		OnReferencedContentsChanged(EventArgs.Empty);
	}

	internal void ReInitialize1()
	{
		lock (base.ReferencedContents)
		{
			base.ReferencedContents.Clear();
			AddReferencedContent(ParserService.GetRegistryForReference(new ReferenceProjectItem(project, "mscorlib")).Mscorlib);
		}
		ProjectService.ProjectItemAdded -= OnProjectItemAdded;
		ProjectService.ProjectItemRemoved -= OnProjectItemRemoved;
		initializing = true;
		Initialize1();
		initializing = false;
	}

	private void UpdateReferenceInterDependencies()
	{
		IProjectContent[] array;
		lock (base.ReferencedContents)
		{
			array = new IProjectContent[base.ReferencedContents.Count];
			base.ReferencedContents.CopyTo(array, 0);
		}
		IProjectContent[] array2 = array;
		foreach (IProjectContent projectContent in array2)
		{
			if (projectContent is ReflectionProjectContent)
			{
				((ReflectionProjectContent)projectContent).InitializeReferences();
			}
		}
	}

	protected virtual void AddReference(ReferenceProjectItem reference, bool updateInterDependencies)
	{
		try
		{
			if (!string.IsNullOrEmpty(reference.Include))
			{
				AddReferencedContent(ParserService.GetProjectContentForReference(reference));
				if (updateInterDependencies)
				{
					UpdateReferenceInterDependencies();
				}
				OnReferencedContentsChanged(EventArgs.Empty);
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}

	private void AddReference(object state)
	{
		AddReference((ReferenceProjectItem)state, updateInterDependencies: true);
	}

	protected virtual void OnProjectItemAdded(object sender, ProjectItemEventArgs e)
	{
		if (e.Project != project)
		{
			return;
		}
		ReferenceProjectItem reference = e.ProjectItem as ReferenceProjectItem;
		if (reference != null)
		{
			if (reference.ItemType == ItemType.COMReference)
			{
				MethodInvoker methodInvoker = delegate
				{
					project.Save();
					TaskService.BuildMessageViewCategory.AppendText("\n${res:MainWindow.CompilerMessages.CreatingCOMInteropAssembly}\n");
					BuildCallback callback = delegate
					{
						ThreadPool.QueueUserWorkItem(AddReference, reference);
						lock (callAfterAddComReference)
						{
							if (callAfterAddComReference.Count > 0)
							{
								callAfterAddComReference.Dequeue()();
							}
							else
							{
								buildingComReference = false;
							}
						}
					};
					project.StartBuild(new BuildOptions(BuildTarget.ResolveComReferences, callback));
				};
				lock (callAfterAddComReference)
				{
					if (buildingComReference)
					{
						callAfterAddComReference.Enqueue(methodInvoker);
					}
					else
					{
						buildingComReference = true;
						methodInvoker();
					}
				}
			}
			else
			{
				ParserService.RefreshProjectContentForReference(reference);
				ThreadPool.QueueUserWorkItem(AddReference, reference);
			}
		}
		if (e.ProjectItem.ItemType == ItemType.Import)
		{
			UpdateDefaultImports(project.Items);
		}
		else if (e.ProjectItem.ItemType == ItemType.Compile && File.Exists(e.ProjectItem.FileName))
		{
			ParserService.EnqueueForParsing(e.ProjectItem.FileName);
		}
	}

	protected virtual void OnProjectItemRemoved(object sender, ProjectItemEventArgs e)
	{
		if (e.Project != project)
		{
			return;
		}
		if (e.ProjectItem is ReferenceProjectItem item)
		{
			try
			{
				IProjectContent existingProjectContentForReference = ParserService.GetExistingProjectContentForReference(item);
				if (existingProjectContentForReference != null)
				{
					lock (base.ReferencedContents)
					{
						base.ReferencedContents.Remove(existingProjectContentForReference);
					}
					OnReferencedContentsChanged(EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
		LoggingService.Info("OnProjectItemRemoved: " + e.ProjectItem.FileName);
		if (e.ProjectItem.ItemType == ItemType.Import)
		{
			UpdateDefaultImports(project.Items);
		}
		else if (e.ProjectItem.ItemType == ItemType.Compile)
		{
			ParserService.ClearParseInformation(e.ProjectItem.FileName);
		}
	}

	private void UpdateDefaultImports(ICollection<ProjectItem> items)
	{
		if (languageDefaultImportCount < 0)
		{
			languageDefaultImportCount = ((base.DefaultImports != null) ? base.DefaultImports.Usings.Count : 0);
		}
		if (languageDefaultImportCount == 0)
		{
			base.DefaultImports = null;
		}
		else
		{
			while (base.DefaultImports.Usings.Count > languageDefaultImportCount)
			{
				base.DefaultImports.Usings.RemoveAt(languageDefaultImportCount);
			}
		}
		foreach (ProjectItem item in items)
		{
			if (item.ItemType == ItemType.Import)
			{
				if (base.DefaultImports == null)
				{
					base.DefaultImports = new DefaultUsing(this);
				}
				base.DefaultImports.Usings.Add(item.Include);
			}
		}
	}

	internal int GetInitializationWorkAmount()
	{
		return project.Items.Count;
	}

	internal void ReInitialize2()
	{
		if (!initializing)
		{
			initializing = true;
			Initialize2();
		}
	}

	protected internal virtual void Initialize2()
	{
		if (!initializing)
		{
			return;
		}
		int workDone = StatusBarService.ProgressMonitor.GetWorkDone("Parsing");
		ParseableFileContentEnumerator parseableFileContentEnumerator = new ParseableFileContentEnumerator(project);
		try
		{
			StatusBarService.ProgressMonitor.SetTaskTextAndWork("Parsing", "${res:ICSharpCode.SharpDevelop.Internal.ParserService.Parsing} " + project.Name + "...", workDone);
			IProjectContent[] array;
			lock (base.ReferencedContents)
			{
				array = new IProjectContent[base.ReferencedContents.Count];
				base.ReferencedContents.CopyTo(array, 0);
			}
			IProjectContent[] array2 = array;
			foreach (IProjectContent projectContent in array2)
			{
				if (projectContent is ReflectionProjectContent)
				{
					((ReflectionProjectContent)projectContent).InitializeReferences();
				}
			}
			while (parseableFileContentEnumerator.MoveNext())
			{
				int index = parseableFileContentEnumerator.Index;
				if (index % 5 == 2)
				{
					StatusBarService.ProgressMonitor.SetWorkDone("Parsing", workDone + index);
				}
				ParserService.ParseFile(this, parseableFileContentEnumerator.CurrentFileName, parseableFileContentEnumerator.CurrentFileContent, updateCommentTags: true);
				if (!initializing)
				{
					break;
				}
			}
		}
		finally
		{
			initializing = false;
			StatusBarService.ProgressMonitor.SetWorkDone("Parsing", workDone + parseableFileContentEnumerator.ItemCount);
			parseableFileContentEnumerator.Dispose();
		}
	}

	public override void Dispose()
	{
		ProjectService.ProjectItemAdded -= OnProjectItemAdded;
		ProjectService.ProjectItemRemoved -= OnProjectItemRemoved;
		initializing = false;
		base.Dispose();
	}
}

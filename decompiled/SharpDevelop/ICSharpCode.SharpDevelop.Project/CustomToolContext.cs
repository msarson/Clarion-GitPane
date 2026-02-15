using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Threading;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class CustomToolContext
{
	private IProject project;

	private IProgressNotificationCenter progressMonitor;

	private string outputNamespace;

	internal bool RunningSeparateThread;

	private static object lockObject = new object();

	private static volatile MessageViewCategory customToolMessageView;

	public IProject Project => project;

	public string OutputNamespace
	{
		get
		{
			return outputNamespace;
		}
		set
		{
			outputNamespace = value;
		}
	}

	internal static MessageViewCategory StaticMessageView
	{
		get
		{
			if (customToolMessageView == null)
			{
				lock (lockObject)
				{
					if (customToolMessageView == null)
					{
						customToolMessageView = new MessageViewCategory("Custom Tool");
						CompilerMessageView.Instance.AddCategory(customToolMessageView);
					}
				}
			}
			return customToolMessageView;
		}
	}

	public MessageViewCategory MessageView => StaticMessageView;

	public IProgressNotificationCenter ProgressMonitor
	{
		get
		{
			return progressMonitor;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			progressMonitor = value;
		}
	}

	public CustomToolContext(IProject project)
		: this(project, new DummyProgressMonitor())
	{
	}

	public CustomToolContext(IProject project, IProgressNotificationCenter progressMonitor)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		this.project = project;
		ProgressMonitor = progressMonitor;
	}

	public void RunAsync(Action action)
	{
		RunningSeparateThread = true;
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
			finally
			{
				CustomToolsService.NotifyAsyncFinish(this);
			}
		});
	}

	public string GetOutputFileName(FileProjectItem baseItem, string additionalExtension)
	{
		if (baseItem == null)
		{
			throw new ArgumentNullException("baseItem");
		}
		if (baseItem.Project != project)
		{
			throw new ArgumentException("baseItem is not from project this CustomToolContext belongs to");
		}
		string text = null;
		if (project.LanguageProperties.CodeDomProvider != null)
		{
			text = project.LanguageProperties.CodeDomProvider.FileExtension;
		}
		if (string.IsNullOrEmpty(text))
		{
			if (string.IsNullOrEmpty(additionalExtension))
			{
				text = ".unknown";
			}
			else
			{
				text = additionalExtension;
				additionalExtension = "";
			}
		}
		if (!text.StartsWith("."))
		{
			text = "." + text;
		}
		return Path.ChangeExtension(baseItem.FileName, additionalExtension + text);
	}

	public FileProjectItem EnsureOutputFileIsInProject(FileProjectItem baseItem, string outputFileName)
	{
		WorkbenchSingleton.AssertMainThread();
		FileProjectItem fileProjectItem = project.FindFile(outputFileName);
		if (fileProjectItem == null)
		{
			fileProjectItem = new FileProjectItem(project, ItemType.Compile);
			fileProjectItem.FileName = outputFileName;
			fileProjectItem.DependentUpon = Path.GetFileName(baseItem.FileName);
			ProjectService.AddProjectItem(project, fileProjectItem);
			project.Save();
			ProjectBrowserPad.Instance.ProjectBrowserControl.RefreshView();
		}
		return fileProjectItem;
	}

	public void WriteCodeDomToFile(FileProjectItem baseItem, string outputFileName, CodeCompileUnit ccu)
	{
		WorkbenchSingleton.AssertMainThread();
		CodeDomProvider codeDomProvider = project.LanguageProperties.CodeDomProvider;
		CodeGeneratorOptions createCodeGeneratorOptions = new CodeDOMGeneratorUtility().CreateCodeGeneratorOptions;
		string codeOutput;
		using (StringWriter stringWriter = new StringWriter())
		{
			if (codeDomProvider == null)
			{
				stringWriter.WriteLine("No CodeDom provider was found for this language.");
			}
			else
			{
				codeDomProvider.GenerateCodeFromCompileUnit(ccu, stringWriter, createCodeGeneratorOptions);
			}
			codeOutput = stringWriter.ToString();
		}
		FileUtility.ObservedSave(delegate(string fileName)
		{
			File.WriteAllText(fileName, codeOutput, Encoding.UTF8);
		}, outputFileName, FileErrorPolicy.Inform);
		EnsureOutputFileIsInProject(baseItem, outputFileName);
		ParserService.EnqueueForParsing(outputFileName, codeOutput);
	}

	public void GenerateCodeDomAsync(FileProjectItem baseItem, string outputFileName, Func<CodeCompileUnit> func)
	{
		RunAsync(delegate
		{
			CodeCompileUnit arg = func();
			WorkbenchSingleton.SafeThreadAsyncCall(WriteCodeDomToFile, baseItem, outputFileName, arg);
		});
	}
}

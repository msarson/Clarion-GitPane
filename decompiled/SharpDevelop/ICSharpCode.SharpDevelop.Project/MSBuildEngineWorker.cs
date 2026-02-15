using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using ICSharpCode.Core;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class MSBuildEngineWorker
{
	private class SharpDevelopLogger : ILogger
	{
		private MSBuildEngineWorker worker;

		private BuildResults results;

		private string activeTaskName;

		private LoggerVerbosity verbosity = LoggerVerbosity.Minimal;

		private string parameters;

		public LoggerVerbosity Verbosity
		{
			get
			{
				return verbosity;
			}
			set
			{
				verbosity = value;
			}
		}

		public string Parameters
		{
			get
			{
				return parameters;
			}
			set
			{
				parameters = value;
			}
		}

		public SharpDevelopLogger(MSBuildEngineWorker worker)
		{
			this.worker = worker;
			results = worker.buildRun.currentResults;
		}

		private void AppendText(string text)
		{
			worker.OutputText(text + "\r\n");
		}

		internal void FlushCurrentError()
		{
			if (worker.currentErrorOrWarning != null)
			{
				AppendText(worker.currentErrorOrWarning.ToString());
				worker.currentErrorOrWarning = null;
			}
		}

		internal void OnProjectStarted(object sender, ProjectStartedEventArgs e)
		{
			worker.projectFiles.Push(e.ProjectFile);
		}

		internal void OnProjectFinished(object sender, ProjectFinishedEventArgs e)
		{
			FlushCurrentError();
			if (worker.projectFiles.Count > 0)
			{
				worker.projectFiles.Pop();
			}
		}

		internal void OnTaskStarted(object sender, TaskStartedEventArgs e)
		{
			activeTaskName = e.TaskName;
			if (MSBuildEngine.CompileTaskNames.Contains(e.TaskName.ToLowerInvariant()))
			{
				AppendText("${res:MainWindow.CompilerMessages.CompileVerb} " + Path.GetFileNameWithoutExtension(e.ProjectFile));
			}
		}

		internal void OnTaskFinished(object sender, TaskFinishedEventArgs e)
		{
			FlushCurrentError();
		}

		internal void OnError(object sender, BuildErrorEventArgs e)
		{
			AppendError(e.File, e.LineNumber, e.ColumnNumber, e.Code, e.Message, isWarning: false);
		}

		internal void OnWarning(object sender, BuildWarningEventArgs e)
		{
			AppendError(e.File, e.LineNumber, e.ColumnNumber, e.Code, e.Message, isWarning: true);
		}

		internal void OnMessage(object sender, BuildMessageEventArgs e)
		{
			bool flag = false;
			switch (e.Importance)
			{
			case MessageImportance.Low:
				flag = Verbosity == LoggerVerbosity.Detailed || Verbosity == LoggerVerbosity.Diagnostic;
				break;
			case MessageImportance.Normal:
				flag = Verbosity == LoggerVerbosity.Detailed || Verbosity == LoggerVerbosity.Diagnostic || Verbosity == LoggerVerbosity.Normal;
				break;
			case MessageImportance.High:
				flag = Verbosity != LoggerVerbosity.Quiet;
				break;
			}
			if (flag)
			{
				AppendText(e.Message);
			}
		}

		private void AppendError(string file, int lineNumber, int columnNumber, string code, string message, bool isWarning)
		{
			if (string.Equals(file, activeTaskName, StringComparison.InvariantCultureIgnoreCase))
			{
				file = "";
			}
			else if (FileUtility.IsValidFileName(file))
			{
				bool flag = file == Path.GetFileNameWithoutExtension(file);
				if (!string.IsNullOrEmpty(worker.CurrentProjectFile))
				{
					try
					{
						file = Path.Combine(Path.GetDirectoryName(worker.CurrentProjectFile), file);
					}
					catch
					{
						file = "";
					}
				}
				if (flag && !File.Exists(file))
				{
					file = "";
				}
			}
			FlushCurrentError();
			BuildError buildError = new BuildError(file, lineNumber, columnNumber, code, message);
			buildError.IsWarning = isWarning;
			results.Add(buildError);
			worker.currentErrorOrWarning = buildError;
			if (!isWarning)
			{
				worker.buildRun.currentResults.Result = BuildResultCode.Error;
			}
		}

		public void Initialize(IEventSource eventSource)
		{
			verbosity = (LoggerVerbosity)Enum.Parse(typeof(LoggerVerbosity), PropertyService.Get("SharpDevelop.LoggerVerbosity", LoggerVerbosity.Minimal.ToString()));
			eventSource.ProjectStarted += OnProjectStarted;
			eventSource.ProjectFinished += OnProjectFinished;
			eventSource.TaskStarted += OnTaskStarted;
			eventSource.TaskFinished += OnTaskFinished;
			eventSource.ErrorRaised += OnError;
			eventSource.WarningRaised += OnWarning;
			eventSource.MessageRaised += OnMessage;
		}

		public void Shutdown()
		{
		}
	}

	private MSBuildEngine parentEngine;

	private MSBuildEngine.BuildRun buildRun;

	private Engine engine;

	private SharpDevelopLogger logger;

	private bool outputAcquired;

	private StringBuilder cachedOutput;

	private BuildError currentErrorOrWarning;

	private Stack<string> projectFiles = new Stack<string>();

	public BuildError CurrentErrorOrWarning => currentErrorOrWarning;

	public string CurrentProjectFile
	{
		get
		{
			if (projectFiles.Count == 0)
			{
				return null;
			}
			return projectFiles.Peek();
		}
	}

	internal MSBuildEngineWorker(MSBuildEngine parentEngine, MSBuildEngine.BuildRun buildRun)
	{
		this.parentEngine = parentEngine;
		this.buildRun = buildRun;
		engine = buildRun.CreateEngine();
		logger = new SharpDevelopLogger(this);
		engine.RegisterLogger(logger);
		foreach (IMSBuildAdditionalLogger additionalMSBuildLogger in MSBuildEngine.AdditionalMSBuildLoggers)
		{
			engine.RegisterLogger(additionalMSBuildLogger.CreateLogger(this));
		}
	}

	private static void AppendDir(StringBuilder args, string dir)
	{
		if (dir.EndsWith("\\"))
		{
			args.Append(dir.Substring(0, dir.Length - 1));
		}
		else
		{
			args.Append(dir);
		}
	}

	private string CommandLineArguments(MSBuildEngine.ProjectToBuild ptb)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (ptb.file != null)
		{
			stringBuilder.Append(" Project \"");
			stringBuilder.Append(ptb.file);
			stringBuilder.Append("\"");
		}
		stringBuilder.Append(" Configuration \"");
		stringBuilder.Append(parentEngine.Configuration);
		stringBuilder.Append("\" Platform \"");
		stringBuilder.Append(parentEngine.Platform);
		stringBuilder.Append("\" Target \"");
		stringBuilder.Append(ptb.targets);
		stringBuilder.Append("\" SolutionDir \"");
		AppendDir(stringBuilder, buildRun.Solution.Directory);
		stringBuilder.Append("\" SolutionFileName \"");
		stringBuilder.Append(buildRun.Solution.FileName);
		stringBuilder.Append("\" Verbosity \"");
		stringBuilder.Append(logger.Verbosity.ToString());
		stringBuilder.Append("\" BinPath \"");
		AppendDir(stringBuilder, engine.BinPath);
		stringBuilder.Append("\"");
		stringBuilder.Append(" AdditionalProperties ");
		stringBuilder.Append(buildRun.BuildOptions.AdditionalProperties.Count + 1);
		stringBuilder.Append(" \"ConfigDir\" \"");
		AppendDir(stringBuilder, PropertyService.ConfigDirectory);
		stringBuilder.Append("\"");
		if (buildRun.BuildOptions.AdditionalProperties != null && buildRun.BuildOptions.AdditionalProperties.Count > 0)
		{
			foreach (KeyValuePair<string, string> additionalProperty in buildRun.BuildOptions.AdditionalProperties)
			{
				stringBuilder.Append(" \"");
				stringBuilder.Append(additionalProperty.Key);
				stringBuilder.Append("\" \"");
				AppendDir(stringBuilder, additionalProperty.Value);
				stringBuilder.Append("\"");
			}
		}
		if (MSBuildEngine.AdditionalTargetFiles.Count > 0)
		{
			stringBuilder.Append(" AdditionalTargetFiles ");
			stringBuilder.Append(MSBuildEngine.AdditionalTargetFiles.Count);
			foreach (string additionalTargetFile in MSBuildEngine.AdditionalTargetFiles)
			{
				stringBuilder.Append(" \"");
				stringBuilder.Append(additionalTargetFile);
				stringBuilder.Append("\"");
			}
		}
		if (MSBuildEngine.MSBuildProperties.Count > 0)
		{
			stringBuilder.Append(" MSBuildProperties ");
			stringBuilder.Append(MSBuildEngine.MSBuildProperties.Count);
			foreach (KeyValuePair<string, string> mSBuildProperty in MSBuildEngine.MSBuildProperties)
			{
				stringBuilder.Append(" \"");
				stringBuilder.Append(mSBuildProperty.Key);
				stringBuilder.Append("\" \"");
				AppendDir(stringBuilder, mSBuildProperty.Value);
				stringBuilder.Append("\"");
			}
		}
		AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/MSBuildEngine/AdditionalProperties", throwOnNotFound: false);
		if (treeNode != null)
		{
			stringBuilder.Append(" AddinPropertiesStart ");
			foreach (Codon codon in treeNode.Codons)
			{
				object obj = codon.BuildItem(null, new ArrayList());
				if (obj != null)
				{
					bool flag = !codon.Properties.Get("text", "").Contains("$(");
					stringBuilder.Append(" \"");
					stringBuilder.Append(codon.Id);
					stringBuilder.Append("\" \"");
					stringBuilder.Append(obj.ToString());
					stringBuilder.Append("\" ");
					stringBuilder.Append(flag.ToString());
					stringBuilder.Append(" ");
				}
			}
			stringBuilder.Append(" AddinPropertiesStop ");
		}
		return stringBuilder.ToString();
	}

	private bool ProcessMessage(Process p, char messageType)
	{
		bool result = false;
		string text = null;
		string text2 = null;
		string text3 = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		string text4 = null;
		string text5 = null;
		string text6 = null;
		switch (messageType)
		{
		case '1':
			logger.OnProjectStarted(null, new ProjectStartedEventArgs(null, null, p.StandardOutput.ReadLine(), null, null, null));
			break;
		case '2':
			logger.OnProjectFinished(null, null);
			break;
		case '3':
			logger.OnTaskStarted(null, new TaskStartedEventArgs(null, null, p.StandardOutput.ReadLine(), null, p.StandardOutput.ReadLine()));
			break;
		case '4':
			logger.OnTaskFinished(null, null);
			break;
		case '5':
			text = null;
			text2 = p.StandardOutput.ReadLine();
			text3 = p.StandardOutput.ReadLine();
			num = 0;
			int.TryParse(p.StandardOutput.ReadLine(), out num);
			num2 = 0;
			int.TryParse(p.StandardOutput.ReadLine(), out num2);
			num3 = 0;
			num4 = 0;
			text4 = p.StandardOutput.ReadLine();
			text5 = null;
			text6 = null;
			logger.OnError(null, new BuildErrorEventArgs(text, text2, text3, num, num2, num3, num4, text4, text5, text6));
			break;
		case '6':
			text = null;
			text2 = p.StandardOutput.ReadLine();
			text3 = p.StandardOutput.ReadLine();
			num = 0;
			int.TryParse(p.StandardOutput.ReadLine(), out num);
			num2 = 0;
			int.TryParse(p.StandardOutput.ReadLine(), out num2);
			num3 = 0;
			num4 = 0;
			text4 = p.StandardOutput.ReadLine();
			text5 = null;
			text6 = null;
			logger.OnWarning(null, new BuildWarningEventArgs(text, text2, text3, num, num2, num3, num4, text4, text5, text6));
			break;
		case '7':
			logger.OnMessage(null, new BuildMessageEventArgs(p.StandardOutput.ReadLine(), null, null, MessageImportance.High));
			break;
		case '8':
			bool.TryParse(p.StandardOutput.ReadLine(), out result);
			break;
		}
		return result;
	}

	private bool ProcessMessages(Process p)
	{
		bool result = false;
		int num;
		do
		{
			num = p.StandardOutput.Read();
			if (num != -1)
			{
				result = ProcessMessage(p, (char)num);
			}
		}
		while (num != -1);
		return result;
	}

	internal bool Build(MSBuildEngine.ProjectToBuild ptb)
	{
		if (ProjectService.CancelSemaphore.WaitOne(1, exitContext: false))
		{
			ProjectService.CancelSemaphore.Release();
			return false;
		}
		LoggingService.Debug("Run MSBuild on " + ptb.file);
		bool flag = false;
		if (PropertyService.Get("SharpDevelop.BuildAsProcess", defaultValue: true))
		{
			string location = Assembly.GetEntryAssembly().Location;
			location = FileUtility.Combine(Path.GetPathRoot(location), Path.GetDirectoryName(location), "PrjServer.exe");
			ProcessStartInfo processStartInfo = new ProcessStartInfo(location, CommandLineArguments(ptb));
			processStartInfo.UseShellExecute = false;
			processStartInfo.RedirectStandardInput = true;
			processStartInfo.RedirectStandardOutput = true;
			Process process = Process.Start(processStartInfo);
			if (process != null)
			{
				flag = ProcessMessages(process);
			}
		}
		else
		{
			engine.GlobalProperties.SetProperty("ConfigDir", PropertyService.ConfigDirectory);
			if (!string.IsNullOrEmpty(ptb.configuration))
			{
				engine.GlobalProperties.SetProperty("Configuration", ptb.configuration);
			}
			if (!string.IsNullOrEmpty(ptb.platform))
			{
				engine.GlobalProperties.SetProperty("Platform", ptb.platform);
			}
			Microsoft.Build.BuildEngine.Project project = buildRun.LoadProject(engine, ptb.file);
			if (project == null)
			{
				LoggingService.Debug("Error loading " + ptb.file);
				return false;
			}
			foreach (string additionalTargetFile in MSBuildEngine.AdditionalTargetFiles)
			{
				project.AddNewImport(additionalTargetFile, null);
			}
			flag = ((!string.IsNullOrEmpty(ptb.targets)) ? engine.BuildProject(project, ptb.targets.Split(';')) : engine.BuildProject(project));
		}
		logger.FlushCurrentError();
		ReleaseOutput();
		LoggingService.Debug("MSBuild on " + ptb.file + " finished " + (flag ? "successfully" : "with error"));
		return flag;
	}

	public void OutputText(string text)
	{
		if (!outputAcquired && cachedOutput == null)
		{
			outputAcquired = buildRun.TryAquireOutputLock();
			if (!outputAcquired)
			{
				cachedOutput = new StringBuilder();
			}
		}
		if (outputAcquired)
		{
			parentEngine.MessageView.AppendText(text);
		}
		else
		{
			cachedOutput.Append(text);
		}
	}

	private void ReleaseOutput()
	{
		if (cachedOutput != null)
		{
			buildRun.EnqueueTextForAppendWhenOutputLockIsReleased(cachedOutput.ToString());
			cachedOutput = null;
		}
		if (outputAcquired)
		{
			buildRun.ReleaseOutputLock();
		}
	}
}

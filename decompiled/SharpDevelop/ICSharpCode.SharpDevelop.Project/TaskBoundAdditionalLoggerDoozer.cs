using System;
using System.Collections;
using ICSharpCode.Core;
using Microsoft.Build.Framework;

namespace ICSharpCode.SharpDevelop.Project;

public class TaskBoundAdditionalLoggerDoozer : IDoozer
{
	private class TaskBoundAdditionalLoggerDescriptor : IMSBuildAdditionalLogger
	{
		internal string taskname;

		internal string classname;

		internal AddIn addIn;

		public TaskBoundAdditionalLoggerDescriptor(Codon codon)
		{
			classname = codon.Properties["class"];
			taskname = codon.Properties["taskname"];
			addIn = codon.AddIn;
		}

		public ILogger CreateLogger(MSBuildEngineWorker engineWorker)
		{
			return new TaskBoundAdditionalLogger(this, engineWorker);
		}
	}

	private class TaskBoundAdditionalLogger : ILogger
	{
		private TaskBoundAdditionalLoggerDescriptor desc;

		private MSBuildEngineWorker engineWorker;

		private ILogger baseLogger;

		private bool isActive;

		private LoggerVerbosity verbosity = LoggerVerbosity.Minimal;

		private string parameters;

		private IEventSource eventSource;

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

		public TaskBoundAdditionalLogger(TaskBoundAdditionalLoggerDescriptor desc, MSBuildEngineWorker engineWorker)
		{
			this.desc = desc;
			this.engineWorker = engineWorker;
		}

		private void CreateBaseLogger()
		{
			if (baseLogger == null)
			{
				object obj = desc.addIn.CreateObject(desc.classname);
				baseLogger = obj as ILogger;
				if (obj is IMSBuildAdditionalLogger iMSBuildAdditionalLogger)
				{
					baseLogger = iMSBuildAdditionalLogger.CreateLogger(engineWorker);
				}
			}
		}

		private void OnTaskStarted(object sender, TaskStartedEventArgs e)
		{
			if (desc.taskname.Equals(e.TaskName, StringComparison.InvariantCultureIgnoreCase))
			{
				CreateBaseLogger();
				if (baseLogger != null)
				{
					baseLogger.Initialize(eventSource);
					isActive = true;
				}
			}
		}

		private void OnTaskFinished(object sender, TaskFinishedEventArgs e)
		{
			if (isActive)
			{
				baseLogger.Shutdown();
				isActive = false;
			}
		}

		public void Initialize(IEventSource eventSource)
		{
			this.eventSource = eventSource;
			eventSource.TaskStarted += OnTaskStarted;
			eventSource.TaskFinished += OnTaskFinished;
		}

		public void Shutdown()
		{
			OnTaskFinished(null, null);
			if (eventSource != null)
			{
				eventSource.TaskStarted -= OnTaskStarted;
				eventSource.TaskFinished -= OnTaskFinished;
				eventSource = null;
			}
		}
	}

	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		return new TaskBoundAdditionalLoggerDescriptor(codon);
	}
}

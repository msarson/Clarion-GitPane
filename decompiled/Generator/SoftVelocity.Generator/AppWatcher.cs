using System;
using System.IO;
using Clarion.GEN;
using ICSharpCode.Core;
using Microsoft.Build.Framework;
using SoftVelocity.BinaryToText;

namespace SoftVelocity.Generator;

internal class AppWatcher : BinaryFileWatcher
{
	private static AppWatcher self;

	private bool noExport;

	private bool errorOccured;

	internal static AppWatcher Instance
	{
		get
		{
			if (self == null)
			{
				self = new AppWatcher();
			}
			return self;
		}
	}

	internal bool CanExport
	{
		get
		{
			return !noExport;
		}
		set
		{
			noExport = !value;
		}
	}

	public override IBinaryWatcher WatcherDetails => (IBinaryWatcher)(object)new AppWatcherDetails();

	protected override bool AllowThreadedExport => false;

	private AppWatcher()
	{
	}

	internal void Startup()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		ApplicationService.ApplicationLoading += ApplicationLoading;
		FileUtility.FileLoading += new FileNameCancelEventHandler(ApplicationFileLoading);
	}

	private void ApplicationLoading(object sender, ApplicationLoadingEventArgs e)
	{
		((BinaryFileWatcher)this).LoadBinaryFile(e.FullPath);
	}

	private void ApplicationFileLoading(object sender, FileNameCancelEventArgs e)
	{
		if (Path.GetExtension(((FileNameEventArgs)e).FileName).Equals(".app", StringComparison.OrdinalIgnoreCase))
		{
			((BinaryFileWatcher)this).LoadBinaryFile(((FileNameEventArgs)e).FileName);
		}
	}

	private void GeneratorErrorOccured(object sender, BuildErrorEventArgs args)
	{
		errorOccured = true;
	}

	protected override string TextToBinary(string binaryFile, string textFile)
	{
		errorOccured = false;
		ApplicationService.ErrorOccured = (EventHandler<BuildErrorEventArgs>)Delegate.Combine(ApplicationService.ErrorOccured, new EventHandler<BuildErrorEventArgs>(GeneratorErrorOccured));
		Win32App win32App = null;
		try
		{
			win32App = ApplicationService.NewAppFromTxa(binaryFile, textFile);
		}
		finally
		{
			ApplicationService.ErrorOccured = (EventHandler<BuildErrorEventArgs>)Delegate.Remove(ApplicationService.ErrorOccured, new EventHandler<BuildErrorEventArgs>(GeneratorErrorOccured));
		}
		if (errorOccured)
		{
			win32App = null;
		}
		if (win32App == null)
		{
			return string.Format(ResourceService.GetString("Clarion.Generator.ImportExport.Error.LoadFailed"), textFile);
		}
		return null;
	}

	protected override string BinaryToText(string binaryFile, string textFile)
	{
		if (noExport)
		{
			return null;
		}
		if (ApplicationService.IsTemplateRegistryOpen)
		{
			return null;
		}
		Application application = ApplicationService.FindApplication(binaryFile);
		if (application != null && application.InEdit && application.CanGenerate)
		{
			bool flag = ((BinaryFileWatcher)this).IgnoreFile(binaryFile);
			ApplicationService.SetText($"AutoExport on Save {binaryFile}");
			bool flag2 = application.ExportAll(textFile);
			if (flag)
			{
				((BinaryFileWatcher)this).WatchFile(binaryFile);
			}
			if (!flag2)
			{
				return string.Format(ResourceService.GetString("Clarion.Generator.ImportExport.Error.ExportAppFailed"), binaryFile, textFile);
			}
		}
		return null;
	}

	protected override void Dispose(bool disposing)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		try
		{
			ApplicationService.ApplicationLoading -= ApplicationLoading;
			FileUtility.FileLoading -= new FileNameCancelEventHandler(ApplicationFileLoading);
		}
		finally
		{
			((BinaryFileWatcher)this).Dispose(disposing);
		}
	}
}

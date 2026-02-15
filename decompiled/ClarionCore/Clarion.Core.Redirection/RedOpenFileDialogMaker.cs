using System;
using Clarion.Core.Options;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common;
using SoftVelocity.Ide.Core;

namespace Clarion.Core.Redirection;

internal class RedOpenFileDialogMaker : IFileDialogHelper
{
	private static bool hookedUp;

	private static void VesionChanged(string newVersion, bool forWindows)
	{
		FileDialog.ClearDirectoryCache();
	}

	public SoftVelocity.Ide.Core.OpenFileDialog OpenFileDialogMaker(bool forWindows)
	{
		if (!hookedUp)
		{
			Versions.VersionChanged = (Versions.VersionChangingDelegate)Delegate.Combine(Versions.VersionChanged, new Versions.VersionChangingDelegate(VesionChanged));
			hookedUp = true;
		}
		if (ProjectService.CurrentProject != null && ProjectService.CurrentProject.ActivePlatform == "Win32")
		{
			forWindows = true;
		}
		else if (!forWindows && !ClarionAddins.DotNetPresent)
		{
			forWindows = true;
		}
		else if (forWindows && !ClarionAddins.Win32Present)
		{
			forWindows = false;
		}
		return new OpenFileDialog(RedirectionFile.GetActiveRedirectionFile(forWindows), useRelativePaths: false);
	}

	public void SaveDirectory(string extension, string dir)
	{
		OpenFileDialog.SetStartingDir(extension, dir);
		FileDialog.SaveDirectory(extension, dir);
	}
}

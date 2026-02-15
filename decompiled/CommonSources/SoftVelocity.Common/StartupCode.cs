using System;
using System.Reflection;
using System.Windows.Forms;
using Clarion;
using Clarion.ASL;
using Clarion.Base;
using Clarion.Core.Options;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.CWPInvoke;
using SoftVelocity.Common.Project.Commands;

namespace SoftVelocity.Common;

internal class StartupCode : AbstractCommand
{
	internal class Callback : IASLInit
	{
		internal Callback()
		{
		}

		public virtual bool Error(string applet, string cause)
		{
			string format = ResourceService.GetString("Clarion.Init.NoLoad");
			MessageBox.Show(string.Format(format, new string[2] { applet, cause }), ResourceService.GetString("Clarion.Init.Failed"));
			return false;
		}

		public virtual bool Error(string applet, AppletLoadError code)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Invalid comparison between Unknown and I4
			string text = (((int)code != 1) ? "Clarion.Init.DllNotLoaded" : "Clarion.Init.NoEntryPoint");
			return Error(applet, ResourceService.GetString(text));
		}
	}

	internal static bool Called;

	public override void Run()
	{
		if (!Called)
		{
			CFile.InitRTL();
			Commands.AttachToClarion();
			ResourceService.RegisterStrings("CommonSources.Resources.Init.String", Assembly.GetExecutingAssembly());
			if (Commands.InitASL((IASLInit)(object)new Callback(), (IWinOptions)(object)new WinOptions(), (IDEType)2) != 0)
			{
				Environment.Exit(1);
			}
			RedirectionFile.ThrowErrorOnLoadFailure = false;
			CWDialogService.Start();
			WorkbenchSingleton.WorkbenchCreated += WorkbenchSingleton_WorkbenchCreated;
			ProjectService.StartBuild += ClarionSolutionService.StartBuild;
			CancelBuild.Startup();
			Called = true;
		}
	}

	private void SetVersion(bool ver)
	{
		string activeVersion = Versions.GetActiveVersion(ver);
		if (!string.IsNullOrEmpty(activeVersion))
		{
			Versions.SetActiveVersion(activeVersion, ver);
		}
	}

	private void WorkbenchSingleton_WorkbenchCreated(object sender, EventArgs e)
	{
		SetVersion(ver: true);
		SetVersion(ver: false);
		if (ProjectService.OpenSolution == null)
		{
			WorkbenchSingleton.Workbench.SetProjectTitle((IProject)null);
		}
		WorkbenchSingleton.WorkbenchCreated -= WorkbenchSingleton_WorkbenchCreated;
	}
}

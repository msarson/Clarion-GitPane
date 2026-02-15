using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common;

namespace SoftVelocity.Generator.Commands;

internal abstract class AbstractRunProjectMenuCommand : AbstractMenuCommand
{
	private bool _useDebug;

	private bool _fallbackToStartUp;

	public bool UseDebug
	{
		get
		{
			return _useDebug;
		}
		set
		{
			_useDebug = value;
		}
	}

	public bool FallbackToStartUp
	{
		get
		{
			return _fallbackToStartUp;
		}
		set
		{
			_fallbackToStartUp = value;
		}
	}

	internal static void RunCurrentProject(IProject project, bool useDebug, bool fallbackToStartUp)
	{
		RunCurrentProject runCurrentProject = new RunCurrentProject();
		runCurrentProject.UseDebug = useDebug;
		runCurrentProject.FallbackToStartUp = fallbackToStartUp;
		runCurrentProject.DoRun(project);
	}

	protected void DoRun(IProject project)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected I4, but got Unknown
		RedirectionFile val = CommonClarionProject.CurrentRedirectionFile(project);
		if (project is CompilableProject)
		{
			try
			{
				CompilableProject val2 = (CompilableProject)project;
				string text = null;
				StartAction startAction = val2.StartAction;
				switch ((int)startAction)
				{
				case 1:
					text = val.OpenName(val2.StartProgram, "");
					_ = val2.StartArguments;
					break;
				case 2:
					text = val2.StartArguments;
					break;
				case 0:
					if (Path.GetExtension(((AbstractProject)val2).OutputAssemblyFullPath).ToUpperInvariant() == ".EXE")
					{
						text = val.OpenName(((AbstractProject)val2).OutputAssemblyFullPath, ((AbstractProject)val2).Directory);
						_ = val2.StartArguments;
						Path.GetDirectoryName(text);
					}
					break;
				}
				if (text != null)
				{
					project.Start(UseDebug);
				}
				else if (FallbackToStartUp && ProjectService.OpenSolution != null && ProjectService.OpenSolution.StartupProject != null && ((ISolutionFolder)project).IdGuid != ((ISolutionFolder)ProjectService.OpenSolution.StartupProject).IdGuid)
				{
					DoRun(ProjectService.OpenSolution.StartupProject);
				}
				return;
			}
			catch (FileNotFoundException ex)
			{
				MessageBox.Show(string.Format(ResourceService.GetString("Clarion.Generator.FileNotFoundExceptionMessage"), ex.FileName), ResourceService.GetString("Clarion.Generator.FileNotFoundExceptionCaption"));
				return;
			}
		}
		DoRun(project.OutputAssemblyFullPath, null, null);
	}

	protected void DoRun(string exeName, string arguments, string workingDir)
	{
		try
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = exeName;
			if (workingDir != null && Directory.Exists(workingDir))
			{
				processStartInfo.WorkingDirectory = workingDir;
			}
			if (!string.IsNullOrEmpty(arguments))
			{
				processStartInfo.Arguments = arguments;
			}
			if (UseDebug)
			{
				DebuggerService.CurrentDebugger.Start(processStartInfo);
			}
			else
			{
				DebuggerService.CurrentDebugger.StartWithoutDebugging(processStartInfo);
			}
		}
		catch (Win32Exception ex)
		{
			if (ex.ErrorCode == -2147467259)
			{
				MessageBox.Show(ex.Message, ResourceService.GetString("Clarion.Generator.FileNotFoundExceptionCaption"));
			}
		}
		catch (FileNotFoundException ex2)
		{
			MessageBox.Show(string.Format(ResourceService.GetString("Clarion.Generator.FileNotFoundExceptionMessage"), ex2.FileName), ResourceService.GetString("Clarion.Generator.FileNotFoundExceptionCaption"));
		}
		catch (Exception ex3)
		{
			MessageBox.Show(ex3.Message, ResourceService.GetString("Clarion.Generator.FileNotFoundExceptionCaption"));
		}
	}
}

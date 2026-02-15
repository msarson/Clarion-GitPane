using System;
using System.IO;
using Clarion.Core.Options;
using Clarion.GEN;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using Microsoft.Build.Framework;

namespace SoftVelocity.Generator;

internal class GeneratorRequestor : IRequestor, IErrorLog
{
	private bool _AllowClearErrors = true;

	public EventHandler<BuildErrorEventArgs> ErrorOccured;

	public IErrorLog ErrorLogger => this;

	public bool QuietMode => false;

	public bool UseWideDialogs
	{
		get
		{
			bool result = true;
			try
			{
				ClarionVersion versionActive = Versions.GetVersionActive(true);
				if (versionActive != null)
				{
					result = versionActive.SupportsWideAppgenDialogs;
				}
			}
			catch
			{
			}
			return result;
		}
	}

	internal bool AllowClearErrors
	{
		get
		{
			return _AllowClearErrors;
		}
		set
		{
		}
	}

	public void ClearErrors(bool canEditErrors, bool okToContinue)
	{
	}

	public void SetError(string msg)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall<string>((Action<string>)SetError, msg);
			return;
		}
		ApplicationService.SetTextNewLine(msg);
		DoErrorOccured(msg);
		TaskService.Add(new Task(string.Empty, "GEN: " + msg, 0, 0, (TaskType)0));
		if (ErrorListPad.Instance != null)
		{
			WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(((object)ErrorListPad.Instance).GetType().FullName);
		}
	}

	public void SetError(string msg, string fileName, uint line, uint column)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		if (Path.GetFileNameWithoutExtension(fileName) == "PWEE")
		{
			SetError(new Task(string.Empty, "TPL: " + msg, (int)(column - 1), (int)(line - 1), (TaskType)2), string.Format(ResourceService.GetString("Clarion.Generator.Error.Format.Complete"), msg, fileName, line, column));
		}
		else
		{
			SetError(new Task(fileName, "TPL: " + msg, (int)(column - 1), (int)(line - 1), (TaskType)0), string.Format(ResourceService.GetString("Clarion.Generator.Error.Format.Complete"), msg, fileName, line, column));
		}
	}

	private void SetError(Task tas, string text)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall<Task, string>((Action<Task, string>)SetError, tas, text);
			return;
		}
		TaskService.Add(tas);
		ApplicationService.SetTextNewLine(text);
		DoErrorOccured(text);
		if (ErrorListPad.Instance != null)
		{
			WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(((object)ErrorListPad.Instance).GetType().FullName);
		}
	}

	public void SetError(string msg, string fileName, uint offset)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall<string, string, uint>((Action<string, string, uint>)SetError, msg, fileName, offset);
			return;
		}
		if (Path.GetFileNameWithoutExtension(fileName) == "PWEE")
		{
			TaskService.Add(new Task(string.Empty, "TPL: " + msg, (int)offset, 0, (TaskType)2));
		}
		else
		{
			TaskService.Add(new Task(fileName, "TPL: " + msg, (int)offset, 0, (TaskType)0));
		}
		string textNewLine = string.Format(ResourceService.GetString("Clarion.Generator.Error.Format.Offset"), msg, fileName, offset);
		ApplicationService.SetTextNewLine(textNewLine);
		DoErrorOccured(msg, fileName, (int)offset);
		if (ErrorListPad.Instance != null)
		{
			WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(((object)ErrorListPad.Instance).GetType().FullName);
		}
	}

	public void SetError(GeneratorError err)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall<GeneratorError>((Action<GeneratorError>)SetError, err);
		}
		else if (err != GeneratorError.NoError)
		{
			string text = ResourceService.GetString("Clarion.Generator.Error." + err);
			ApplicationService.SetTextNewLine(text);
			DoErrorOccured(text);
			TaskService.Add(new Task(string.Empty, "GEN: " + text, 0, 0, (TaskType)0));
			if (ErrorListPad.Instance != null)
			{
				WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(((object)ErrorListPad.Instance).GetType().FullName);
			}
		}
	}

	public void StatusMessage(uint line, string msg)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall<uint, string>((Action<uint, string>)StatusMessage, line, msg);
		}
		else
		{
			Win32Generator.SetMessage((int)line, msg);
		}
	}

	public void Write(string msg)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall<string>((Action<string>)Write, msg);
		}
		else
		{
			ApplicationService.SetTextNewLine(msg);
		}
	}

	private void DoErrorOccured(string text)
	{
		if (ErrorOccured != null)
		{
			ErrorOccured(this, new BuildErrorEventArgs(null, null, null, 0, 0, 0, 0, text, null, null));
		}
	}

	private void DoErrorOccured(string text, string fileName, int offset)
	{
		if (ErrorOccured != null)
		{
			ErrorOccured(this, new BuildErrorEventArgs(null, null, fileName, offset, 0, 0, 0, text, null, null));
		}
	}
}

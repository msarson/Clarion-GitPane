using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Refactoring;

namespace ICSharpCode.SharpDevelop;

internal static class DomHostCallback
{
	internal static void Register()
	{
		HostCallback.GetParseInformation = ParserService.GetParseInformation;
		HostCallback.RenameMember = FindReferencesAndRenameHelper.RenameMember;
		HostCallback.ShowMessage = MessageService.ShowMessage;
		HostCallback.GetCurrentProjectContent = () => ParserService.CurrentProjectContent;
		HostCallback.ShowError = delegate(string message, Exception ex)
		{
			MessageService.ShowError(ex, message);
		};
		HostCallback.BeginAssemblyLoad = delegate
		{
		};
		HostCallback.FinishAssemblyLoad = ParserServiceProgressDone;
		HostCallback.ShowAssemblyLoadError = delegate(string fileName, string include, string message)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(ShowAssemblyLoadError, fileName, include, message);
		};
	}

	private static void ParserServiceProgressDone()
	{
	}

	private static void ShowAssemblyLoadError(string fileName, string include, string message)
	{
		WorkbenchSingleton.Workbench.GetPad(typeof(CompilerMessageView)).BringPadToFront();
		TaskService.BuildMessageViewCategory.AppendText(StringParser.Parse("${res:ICSharpCode.SharpDevelop.ErrorLoadingCodeCompletionInformation}", new string[2, 2]
		{
			{ "Assembly", include },
			{ "Filename", fileName }
		}) + "\r\n" + message + "\r\n");
	}
}

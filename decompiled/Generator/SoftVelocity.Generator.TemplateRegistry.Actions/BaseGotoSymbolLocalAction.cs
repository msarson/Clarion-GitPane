using System;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using SearchAndReplace;

namespace SoftVelocity.Generator.TemplateRegistry.Actions;

public abstract class BaseGotoSymbolLocalAction : BaseSymbolSearchLocalAction
{
	protected override void ExecuteSearch(string symbolToSearch, TextArea textArea)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		ProgressNotificationTaskInstance val = new ProgressNotificationTaskInstance("Searching %" + symbolToSearch, true);
		try
		{
			SearchReplaceManager.FindNext((IProgressNotificationTaskInstance)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}

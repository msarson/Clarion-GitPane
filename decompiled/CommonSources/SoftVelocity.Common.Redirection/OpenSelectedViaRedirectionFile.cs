using ICSharpCode.SharpDevelop.DefaultEditor.Commands;
using ICSharpCode.TextEditor.Actions;
using SoftVelocity.Common.Redirection.Action;

namespace SoftVelocity.Common.Redirection;

internal class OpenSelectedViaRedirectionFile : AbstractEditActionMenuCommand
{
	private bool _ForceEmptyDialog;

	internal bool ForceEmptyDialog
	{
		get
		{
			return _ForceEmptyDialog;
		}
		set
		{
			_ForceEmptyDialog = value;
		}
	}

	public override IEditAction EditAction
	{
		get
		{
			SoftVelocity.Common.Redirection.Action.OpenSelectedViaRedirectionFile openSelectedViaRedirectionFile = new SoftVelocity.Common.Redirection.Action.OpenSelectedViaRedirectionFile();
			openSelectedViaRedirectionFile.ForceEmptyDialog = ForceEmptyDialog;
			return (IEditAction)(object)openSelectedViaRedirectionFile;
		}
	}
}

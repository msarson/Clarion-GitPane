using System;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public interface IDialogPanel
{
	object CustomizationObject { get; set; }

	Control Control { get; }

	bool EnableFinish { get; }

	event EventHandler EnableFinishChanged;

	bool ReceiveDialogMessage(DialogMessage message);

	bool ExistControlWithText(string textToSearch, TreeViewLocator.SearchFoundEventArgs controlsToSelect);
}

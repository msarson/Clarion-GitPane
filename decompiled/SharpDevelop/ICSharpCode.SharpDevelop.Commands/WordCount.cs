using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class WordCount : AbstractMenuCommand
{
	public override void Run()
	{
		using WordCountDialog wordCountDialog = new WordCountDialog();
		wordCountDialog.Owner = (Form)WorkbenchSingleton.Workbench;
		wordCountDialog.ShowDialog(WorkbenchSingleton.MainForm);
	}
}

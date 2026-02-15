using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class ShowHtmlHelp : AbstractMenuCommand
{
	public override void Run()
	{
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		FileInfo fileInfo = new FileInfo(entryAssembly.Location);
		string text = Path.Combine(fileInfo.DirectoryName, "ClarionHelp.chm");
		if (File.Exists(text))
		{
			Help.ShowHelp(WorkbenchSingleton.helpHost, text);
		}
		else
		{
			MessageService.ShowWarning("${res:MainWindow.Windows.HtmlHelp.NotFound} " + text);
		}
	}
}

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public class FormWithHelp : Form
{
	public FormWithHelp()
	{
		Font = FontService.GetFont(FontService.FontType.Dialogs);
	}

	public static bool DoF1(Type t)
	{
		string parameter = t.FullName.Replace('.', '_') + ".htm";
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		FileInfo fileInfo = new FileInfo(entryAssembly.Location);
		string text = Path.Combine(fileInfo.DirectoryName, "ClarionHelp.chm");
		if (File.Exists(text))
		{
			Help.ShowHelp(WorkbenchSingleton.helpHost, text, HelpNavigator.Topic, parameter);
		}
		else
		{
			MessageService.ShowWarning("${res:MainWindow.Windows.HtmlHelp.NotFound} " + text);
		}
		return true;
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.F1)
		{
			return DoF1(GetType());
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}
}

using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public abstract class BaseSharpDevelopUserControl : XmlUserControl
{
	public BaseSharpDevelopUserControl()
	{
	}

	protected override void SetupXmlLoader()
	{
		xmlLoader.StringValueFilter = new SharpDevelopStringValueFilter();
		xmlLoader.PropertyValueCreator = new SharpDevelopPropertyValueCreator();
	}

	protected override void InitializeXmlComponents()
	{
		base.AutoScaleMode = AutoScaleMode.Inherit;
		base.InitializeXmlComponents();
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.F1)
		{
			string text = GetType().FullName.Replace('.', '_') + ".htm";
			text = text.Replace('+', '_');
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			FileInfo fileInfo = new FileInfo(entryAssembly.Location);
			string text2 = Path.Combine(fileInfo.DirectoryName, "ClarionHelp.chm");
			if (File.Exists(text2))
			{
				Help.ShowHelp(WorkbenchSingleton.helpHost, text2, HelpNavigator.Topic, text);
			}
			else
			{
				MessageService.ShowWarning("${res:MainWindow.Windows.HtmlHelp.NotFound} " + text2);
			}
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	public void SetEnabledStatus(bool enabled, params string[] controlNames)
	{
		foreach (string text in controlNames)
		{
			Control control = ControlDictionary[text];
			if (control == null)
			{
				MessageService.ShowError(text + " not found!");
			}
			else
			{
				control.Enabled = enabled;
			}
		}
	}
}

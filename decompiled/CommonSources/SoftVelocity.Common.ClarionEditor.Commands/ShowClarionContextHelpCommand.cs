using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.FormDesigner;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class ShowClarionContextHelpCommand : AbstractMenuCommand
{
	private const string _ClarionHelpFile = "ClarionHelp.chm";

	private const string _ClarionReportDesignedHelp = "ReportDesigner.htm";

	private const string _ClarionWindowDesignerHelp = "WindowDesignerWin32.htm";

	private const string _ClarionFormDesignerHelp = "WindowDesignerDotNet.htm";

	public override void Run()
	{
		string parameter = string.Empty;
		bool flag = true;
		string path = "ClarionHelp.chm";
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
		{
			IBaseViewContent val = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent;
			if (val == null)
			{
				val = (IBaseViewContent)(object)WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent;
			}
			if (val != null)
			{
				if (val is IHasClarionContextHelpSupport)
				{
					IHasClarionContextHelpSupport hasClarionContextHelpSupport = (IHasClarionContextHelpSupport)val;
					parameter = hasClarionContextHelpSupport.HelpText;
					flag = hasClarionContextHelpSupport.HelpTextIsKeyword;
				}
				else if (val is CommonClarionEditor)
				{
					TextAreaControl activeTextAreaControl = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)(CommonClarionEditor)(object)val).TextEditorControl).ActiveTextAreaControl;
					string text = null;
					if (activeTextAreaControl.SelectionManager.HasSomethingSelected)
					{
						text = activeTextAreaControl.SelectionManager.SelectedText;
					}
					else
					{
						LineSegment lineSegment = activeTextAreaControl.Document.GetLineSegment(activeTextAreaControl.Caret.Line);
						TextWord word = lineSegment.GetWord(activeTextAreaControl.Caret.Column);
						if (word != null)
						{
							text = word.Word;
						}
					}
					if (!string.IsNullOrEmpty(text))
					{
						Regex regex = new Regex("^[A-Za-z_]\\w*$", RegexOptions.IgnoreCase);
						if (regex.IsMatch(text))
						{
							parameter = text;
						}
					}
				}
				else if (val is CommonClarionDesignerView)
				{
					if (WorkbenchSingleton.Workbench.ActiveContent is PropertyPad)
					{
						object activeContent = WorkbenchSingleton.Workbench.ActiveContent;
						PropertyPad val2 = (PropertyPad)((activeContent is PropertyPad) ? activeContent : null);
						parameter = val2.GetSelectedPropertyName();
						flag = true;
					}
					else
					{
						CommonClarionDesignerView commonClarionDesignerView = (CommonClarionDesignerView)(object)val;
						switch ((ClaDesignerGenerator.FormDesignerModeenum)(object)commonClarionDesignerView.InternalState)
						{
						case ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner:
							parameter = "ReportDesigner.htm";
							flag = false;
							break;
						case ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner:
							parameter = "WindowDesignerWin32.htm";
							flag = false;
							break;
						}
					}
				}
				else if (val is FormsDesignerViewContent)
				{
					parameter = "WindowDesignerDotNet.htm";
					flag = false;
				}
				if (val is IHasMyOwnContextHelpSupport)
				{
					path = ((IHasMyOwnContextHelpSupport)val).FullHelpFileName;
				}
			}
		}
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		FileInfo fileInfo = new FileInfo(entryAssembly.Location);
		string text2 = Path.Combine(fileInfo.DirectoryName, path);
		if (File.Exists(text2))
		{
			if (flag)
			{
				Help.ShowHelp(WorkbenchSingleton.helpHost, text2, HelpNavigator.KeywordIndex, parameter);
			}
			else
			{
				Help.ShowHelp(WorkbenchSingleton.helpHost, text2, HelpNavigator.Topic, parameter);
			}
		}
		else
		{
			MessageService.ShowWarning("${res:MainWindow.Windows.HtmlHelp.NotFound} " + text2);
		}
	}
}

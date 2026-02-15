using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Internal.ExternalTool;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Widgets;

namespace ICSharpCode.SharpDevelop.Commands;

public class ToolMenuBuilder : ISubmenuBuilder
{
	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		List<ToolStripItem> list = new List<ToolStripItem>();
		for (int i = 0; i < ToolLoader.Tool.Count; i++)
		{
			if (ToolLoader.Tool[i].AddTopSeparator)
			{
				list.Add(new MenuSeparator());
			}
			MenuCommand menuCommand = new MenuCommand(ToolLoader.Tool[i].ToString(), ToolEvt);
			menuCommand.Description = "Start tool " + string.Join(string.Empty, ToolLoader.Tool[i].ToString().Split('&'));
			if (!string.IsNullOrEmpty(ToolLoader.Tool[i].ShortcutKeys))
			{
				menuCommand.ShortcutKeys = ShortcutKeyStringHelper.ParseShortcut(ToolLoader.Tool[i].ShortcutKeys);
			}
			list.Add(menuCommand);
		}
		return list.ToArray();
	}

	private void ProcessExitEvent(object sender, EventArgs e)
	{
		Process process = (Process)sender;
		string text = process.StandardOutput.ReadToEnd();
		TaskService.BuildMessageViewCategory.AppendText(text + Environment.NewLine + "${res:XML.MainMenu.ToolMenu.ExternalTools.ExitedWithCode} " + process.ExitCode + Environment.NewLine);
	}

	private void ToolEvt(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		for (int i = 0; i < ToolLoader.Tool.Count; i++)
		{
			if (!(menuCommand.Text == ToolLoader.Tool[i].ToString()))
			{
				continue;
			}
			ExternalTool externalTool = ToolLoader.Tool[i];
			string text = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow?.ViewContent.FileName;
			StringParser.Properties["ItemPath"] = ((text == null) ? string.Empty : text);
			StringParser.Properties["ItemDir"] = ((text == null) ? string.Empty : Path.GetDirectoryName(text));
			StringParser.Properties["ItemFileName"] = ((text == null) ? string.Empty : Path.GetFileName(text));
			StringParser.Properties["ItemExt"] = ((text == null) ? string.Empty : Path.GetExtension(text));
			StringParser.Properties["CurLine"] = "0";
			StringParser.Properties["CurCol"] = "0";
			StringParser.Properties["CurText"] = "0";
			string text2 = ((ProjectService.CurrentProject == null) ? null : ProjectService.CurrentProject.OutputAssemblyFullPath);
			StringParser.Properties["TargetPath"] = ((text2 == null) ? string.Empty : text2);
			StringParser.Properties["TargetDir"] = ((text2 == null) ? string.Empty : Path.GetDirectoryName(text2));
			StringParser.Properties["TargetName"] = ((text2 == null) ? string.Empty : Path.GetFileName(text2));
			StringParser.Properties["TargetExt"] = ((text2 == null) ? string.Empty : Path.GetExtension(text2));
			string text3 = ((ProjectService.CurrentProject == null) ? null : ProjectService.CurrentProject.FileName);
			StringParser.Properties["ProjectDir"] = ((text3 == null) ? null : Path.GetDirectoryName(text3));
			StringParser.Properties["ProjectFileName"] = ((text3 == null) ? null : text3);
			string text4 = ((ProjectService.OpenSolution == null) ? null : ProjectService.OpenSolution.FileName);
			StringParser.Properties["CombineDir"] = ((text4 == null) ? null : Path.GetDirectoryName(text4));
			StringParser.Properties["CombineFileName"] = ((text4 == null) ? null : text4);
			StringParser.Properties["StartupPath"] = Application.StartupPath;
			string text5 = StringParser.Parse(externalTool.Command);
			string text6 = StringParser.Parse(externalTool.Arguments);
			if (externalTool.PromptForArguments)
			{
				using InputBox inputBox = new InputBox();
				inputBox.Text = externalTool.MenuCommand;
				inputBox.Label.Text = ResourceService.GetString("XML.MainMenu.ToolMenu.ExternalTools.EnterArguments");
				inputBox.TextBox.Text = text6;
				if (inputBox.ShowDialog() != DialogResult.OK)
				{
					break;
				}
				text6 = inputBox.TextBox.Text;
			}
			try
			{
				ProcessStartInfo processStartInfo = ((text6 != null && text6.Length != 0 && text6.Trim('"', ' ').Length != 0) ? new ProcessStartInfo(text5, text6) : new ProcessStartInfo(text5));
				processStartInfo.WorkingDirectory = StringParser.Parse(externalTool.InitialDirectory);
				if (externalTool.UseOutputPad)
				{
					processStartInfo.UseShellExecute = false;
					processStartInfo.RedirectStandardOutput = true;
				}
				Process process = new Process();
				process.EnableRaisingEvents = true;
				process.StartInfo = processStartInfo;
				if (externalTool.UseOutputPad)
				{
					process.Exited += ProcessExitEvent;
				}
				process.Start();
				break;
			}
			catch (Exception ex)
			{
				MessageService.ShowError("${res:XML.MainMenu.ToolMenu.ExternalTools.ExecutionFailed} '" + text5 + " " + text6 + "'\n" + ex.Message);
				break;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Commands;

public class StartWorkbenchCommand
{
	private class FormKeyHandler : IMessageFilter
	{
		private const int keyPressedMessage = 256;

		private string oldLayout = "Default";

		private bool SelectActiveWorkbenchWindow()
		{
			bool result = false;
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
			{
				if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent.Control != null)
				{
					if (!WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent.Control.ContainsFocus && Form.ActiveForm == WorkbenchSingleton.MainForm)
					{
						WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent.Control.Focus();
						result = true;
					}
				}
				else if (Form.ActiveForm == WorkbenchSingleton.MainForm)
				{
					WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.SelectWindow();
					result = true;
				}
			}
			return result;
		}

		private bool PadHasFocusAndSkipsEscape()
		{
			foreach (PadDescriptor item in WorkbenchSingleton.Workbench.PadContentCollection)
			{
				if (item.HasFocus)
				{
					return !item.WantsEscape;
				}
			}
			return false;
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg != 256)
			{
				return false;
			}
			switch ((Keys)((uint)m.WParam.ToInt32() | (uint)Control.ModifierKeys))
			{
			case Keys.Escape:
				if (PadHasFocusAndSkipsEscape() && !MenuService.IsContextMenuOpen && SelectActiveWorkbenchWindow())
				{
					return true;
				}
				return false;
			case Keys.Escape | Keys.Shift:
				if (LayoutConfiguration.CurrentLayoutName == "Plain")
				{
					LayoutConfiguration.CurrentLayoutName = oldLayout;
				}
				else
				{
					WorkbenchSingleton.Workbench.WorkbenchLayout.StoreConfiguration();
					oldLayout = LayoutConfiguration.CurrentLayoutName;
					LayoutConfiguration.CurrentLayoutName = "Plain";
				}
				SelectActiveWorkbenchWindow();
				return true;
			default:
				return false;
			}
		}
	}

	private const string workbenchMemento = "WorkbenchMemento";

	public void Run(IList<string> fileList)
	{
		Form form = (Form)WorkbenchSingleton.Workbench;
		form.Show();
		bool flag = false;
		foreach (string file in fileList)
		{
			flag = true;
			try
			{
				IProjectLoader projectLoader = ProjectService.GetProjectLoader(file);
				if (projectLoader != null)
				{
					FileUtility.ObservedLoad(projectLoader.Load, file);
				}
				else
				{
					FileService.OpenFile(file);
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex, "unable to open file " + file);
			}
		}
		if (!flag && PropertyService.Get("SharpDevelop.LoadPrevProjectOnStartup", defaultValue: false))
		{
			IList<RecentOpen.RecentOpenDescription> recentsFromCategory = FileService.RecentOpen.GetRecentsFromCategory(RecentOpen.defaultTypeProjects);
			if (recentsFromCategory.Count > 0)
			{
				ProjectService.LoadSolution(recentsFromCategory[0].FileName);
				flag = true;
			}
		}
		if (!flag)
		{
			ShowStartPageCommand.DoRun();
			foreach (ICommand item in AddInTree.BuildItems("/Workspace/AutostartNothingLoaded", null, throwOnNotFound: false))
			{
				item.Run();
			}
		}
		form.Focus();
		ParserService.StartParserThread();
		Application.AddMessageFilter(new FormKeyHandler());
		Application.Run(form);
		try
		{
			PropertyService.Set("WorkbenchMemento", WorkbenchSingleton.Workbench.CreateMemento());
		}
		catch (Exception ex2)
		{
			MessageService.ShowError(ex2, "Exception while saving workbench state.");
		}
	}
}

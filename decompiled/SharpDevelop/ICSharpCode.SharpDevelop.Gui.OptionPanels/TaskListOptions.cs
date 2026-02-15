using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class TaskListOptions : AbstractOptionPanel
{
	private const string taskListView = "taskListView";

	private const string nameTextBox = "nameTextBox";

	private const string changeButton = "changeButton";

	private const string removeButton = "removeButton";

	private const string addButton = "addButton";

	private ListView taskList;

	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.TaskListOptions.xfrm"));
		string[] array = PropertyService.Get("SharpDevelop.TaskListTokens", ParserService.DefaultTaskListTokens);
		taskList = (ListView)ControlDictionary["taskListView"];
		taskList.BeginUpdate();
		string[] array2 = array;
		foreach (string text in array2)
		{
			taskList.Items.Add(text);
		}
		taskList.EndUpdate();
		taskList.SelectedIndexChanged += TaskListViewSelectedIndexChanged;
		ControlDictionary["changeButton"].Click += ChangeButtonClick;
		ControlDictionary["removeButton"].Click += RemoveButtonClick;
		ControlDictionary["addButton"].Click += AddButtonClick;
		TaskListViewSelectedIndexChanged(this, EventArgs.Empty);
	}

	public override bool StorePanelContents()
	{
		List<string> list = new List<string>();
		foreach (ListViewItem item in taskList.Items)
		{
			string text = item.Text.Trim();
			if (text.Length > 0)
			{
				list.Add(text);
			}
		}
		PropertyService.Set("SharpDevelop.TaskListTokens", list.ToArray());
		return true;
	}

	private void AddButtonClick(object sender, EventArgs e)
	{
		string text = ControlDictionary["nameTextBox"].Text;
		foreach (ListViewItem item in ((ListView)ControlDictionary["taskListView"]).Items)
		{
			if (item.Text == text)
			{
				return;
			}
		}
		((ListView)ControlDictionary["taskListView"]).Items.Add(new ListViewItem(text));
	}

	private void ChangeButtonClick(object sender, EventArgs e)
	{
		((ListView)ControlDictionary["taskListView"]).SelectedItems[0].Text = ControlDictionary["nameTextBox"].Text;
	}

	private void RemoveButtonClick(object sender, EventArgs e)
	{
		((ListView)ControlDictionary["taskListView"]).Items.Remove(((ListView)ControlDictionary["taskListView"]).SelectedItems[0]);
	}

	private void TaskListViewSelectedIndexChanged(object sender, EventArgs e)
	{
		if (((ListView)ControlDictionary["taskListView"]).SelectedItems.Count > 0)
		{
			ControlDictionary["nameTextBox"].Text = ((ListView)ControlDictionary["taskListView"]).SelectedItems[0].Text;
			ControlDictionary["changeButton"].Enabled = true;
			ControlDictionary["removeButton"].Enabled = true;
		}
		else
		{
			ControlDictionary["nameTextBox"].Text = string.Empty;
			ControlDictionary["changeButton"].Enabled = false;
			ControlDictionary["removeButton"].Enabled = false;
		}
	}
}

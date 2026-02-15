using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.Gui;

public class GacReferencePanel : UserControl, IReferencePanel
{
	private class ColumnSorter : IComparer
	{
		private int column;

		private bool asc = true;

		public int CurrentColumn
		{
			get
			{
				return column;
			}
			set
			{
				if (column == value)
				{
					asc = !asc;
				}
				else
				{
					column = value;
				}
			}
		}

		public int Compare(object x, object y)
		{
			ListViewItem listViewItem = (ListViewItem)x;
			ListViewItem listViewItem2 = (ListViewItem)y;
			int num = string.Compare(listViewItem.SubItems[CurrentColumn].Text, listViewItem2.SubItems[CurrentColumn].Text);
			if (asc)
			{
				return num;
			}
			return num * -1;
		}
	}

	protected ListView listView;

	private CheckBox chooseSpecificVersionCheckBox;

	private ISelectReferenceDialog selectDialog;

	private ColumnSorter sorter;

	private ListViewItem[] fullItemList;

	private ListViewItem[] shortItemList;

	public GacReferencePanel(ISelectReferenceDialog selectDialog)
	{
		listView = new ListView();
		sorter = new ColumnSorter();
		listView.ListViewItemSorter = sorter;
		this.selectDialog = selectDialog;
		ColumnHeader value = new ColumnHeader
		{
			Text = ResourceService.GetString("Dialog.SelectReferenceDialog.GacReferencePanel.ReferenceHeader"),
			Width = 180
		};
		listView.Columns.Add(value);
		listView.Sorting = SortOrder.Ascending;
		ColumnHeader value2 = new ColumnHeader
		{
			Text = ResourceService.GetString("Dialog.SelectReferenceDialog.GacReferencePanel.VersionHeader"),
			Width = 70
		};
		listView.Columns.Add(value2);
		ColumnHeader value3 = new ColumnHeader
		{
			Text = ResourceService.GetString("Global.Path"),
			Width = 100
		};
		listView.Columns.Add(value3);
		listView.View = View.Details;
		listView.FullRowSelect = true;
		ListView obj = listView;
		EventHandler value4 = delegate
		{
			AddReference();
		};
		obj.ItemActivate += value4;
		listView.ColumnClick += columnClick;
		listView.Dock = DockStyle.Fill;
		Dock = DockStyle.Fill;
		base.Controls.Add(listView);
		chooseSpecificVersionCheckBox = new CheckBox();
		chooseSpecificVersionCheckBox.Dock = DockStyle.Top;
		chooseSpecificVersionCheckBox.Text = StringParser.Parse("${res:Dialog.SelectReferenceDialog.GacReferencePanel.ChooseSpecificAssemblyVersion}");
		base.Controls.Add(chooseSpecificVersionCheckBox);
		chooseSpecificVersionCheckBox.CheckedChanged += delegate
		{
			listView.Items.Clear();
			if (chooseSpecificVersionCheckBox.Checked)
			{
				listView.Items.AddRange(fullItemList);
			}
			else
			{
				listView.Items.AddRange(shortItemList);
			}
		};
		PrintCache();
	}

	private void columnClick(object sender, ColumnClickEventArgs e)
	{
		if (e.Column < 2)
		{
			sorter.CurrentColumn = e.Column;
			listView.Sort();
		}
	}

	public void AddReference()
	{
		foreach (ListViewItem selectedItem in listView.SelectedItems)
		{
			selectDialog.AddReference(ReferenceType.Gac, selectedItem.Text, chooseSpecificVersionCheckBox.Checked ? selectedItem.Tag.ToString() : selectedItem.Text, null);
		}
	}

	private void PrintCache()
	{
		List<ListViewItem> itemList = GetCacheContent();
		fullItemList = itemList.ToArray();
		itemList.RemoveAll((ListViewItem item) => itemList.Exists((ListViewItem listViewItem) => string.Equals(item.Text, listViewItem.Text, StringComparison.OrdinalIgnoreCase) && new Version(item.SubItems[1].Text) < new Version(listViewItem.SubItems[1].Text)));
		shortItemList = itemList.ToArray();
		listView.Items.AddRange(shortItemList);
	}

	protected virtual List<ListViewItem> GetCacheContent()
	{
		List<ListViewItem> list = new List<ListViewItem>();
		foreach (GacInterop.AssemblyListEntry assembly in GacInterop.GetAssemblyList())
		{
			ListViewItem listViewItem = new ListViewItem(new string[2] { assembly.Name, assembly.Version });
			listViewItem.Tag = assembly.FullName;
			list.Add(listViewItem);
		}
		return list;
	}
}

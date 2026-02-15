using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class COMReferencePanel : ListView, IReferencePanel
{
	private ISelectReferenceDialog selectDialog;

	private bool populated;

	public COMReferencePanel(ISelectReferenceDialog selectDialog)
	{
		this.selectDialog = selectDialog;
		base.Sorting = SortOrder.Ascending;
		ColumnHeader value = new ColumnHeader
		{
			Text = ResourceService.GetString("Global.Name"),
			Width = 240
		};
		base.Columns.Add(value);
		ColumnHeader value2 = new ColumnHeader
		{
			Text = ResourceService.GetString("Global.Path"),
			Width = 200
		};
		base.Columns.Add(value2);
		base.View = View.Details;
		Dock = DockStyle.Fill;
		base.FullRowSelect = true;
		EventHandler value3 = delegate
		{
			AddReference();
		};
		base.ItemActivate += value3;
	}

	public void AddReference()
	{
		foreach (ListViewItem selectedItem in base.SelectedItems)
		{
			TypeLibrary typeLibrary = (TypeLibrary)selectedItem.Tag;
			selectDialog.AddReference(ReferenceType.Typelib, typeLibrary.Name, typeLibrary.Path, typeLibrary);
		}
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (!populated && base.Visible)
		{
			populated = true;
			PopulateListView();
		}
	}

	private void PopulateListView()
	{
		foreach (TypeLibrary library in TypeLibrary.Libraries)
		{
			ListViewItem listViewItem = new ListViewItem(new string[2] { library.Description, library.Path });
			listViewItem.Tag = library;
			base.Items.Add(listViewItem);
		}
	}
}

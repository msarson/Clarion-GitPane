using System.ComponentModel;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

[ProvideProperty("DoLocate", typeof(ListView))]
public class ListViewLocator : ListLocatorBase
{
	private ListView listToSearch;

	private ListViewItem foundItem;

	[Category("Search Behavior")]
	[Description("ListView to search.\r\nIf no tree is selected you can select the locator from the tree it self.\r\nThis option is to allow the locator to work in multiple tree at the same time.")]
	public ListView ListToSearch
	{
		get
		{
			return listToSearch;
		}
		set
		{
			if (listToSearch != null)
			{
				listToSearch.KeyPress -= base.KeyPressOnTree;
			}
			listToSearch = value;
			if (listToSearch != null)
			{
				listToSearch.KeyPress += base.KeyPressOnTree;
			}
		}
	}

	public override bool HasListToLocate => ListToSearch != null;

	public override bool ListHasItems
	{
		get
		{
			if (HasListToLocate)
			{
				return ListToSearch.Items.Count > 0;
			}
			return false;
		}
	}

	public override bool Search(string text, bool fromTop)
	{
		if (ListHasItems)
		{
			return Search(ListToSearch, text.ToLower(), fromTop);
		}
		return false;
	}

	protected override bool CanLocateAtControl(object extendee)
	{
		return extendee is ListView;
	}

	[Category("Search Behavior")]
	[DefaultValue("")]
	[Description("Sets the locator for this List")]
	public bool GetDoLocate(Control extendee)
	{
		return ListToSearch == extendee;
	}

	public void SetDoLocate(Control extendee, object value)
	{
		if (!(extendee is ListView listView))
		{
			return;
		}
		if (value == null || !bool.Parse(value.ToString()))
		{
			if (ListToSearch == extendee)
			{
				ListToSearch = null;
			}
		}
		else
		{
			ListToSearch = listView;
		}
	}

	private bool Search(ListView list, string textToSearch, bool fromTop)
	{
		if (fromTop)
		{
			foundItem = null;
		}
		SearchListViewItem(foundItem, textToSearch);
		if (foundItem == null)
		{
			return false;
		}
		return true;
	}

	private void SearchListViewItem(ListViewItem beginingItem, string textToSearch)
	{
		foreach (ListViewItem selectedItem in listToSearch.SelectedItems)
		{
			selectedItem.Selected = false;
		}
		int num = -1;
		if (beginingItem != null)
		{
			num = beginingItem.Index;
		}
		foundItem = null;
		if (base.InString)
		{
			foreach (ListViewItem item in listToSearch.Items)
			{
				if (item.Index > num && item.Text.ToLower().Contains(textToSearch))
				{
					foundItem = item;
					break;
				}
			}
		}
		else
		{
			foreach (ListViewItem item2 in listToSearch.Items)
			{
				if (item2.Index > num && item2.Text.ToLower().StartsWith(textToSearch))
				{
					foundItem = item2;
					break;
				}
			}
		}
		if (foundItem != null)
		{
			foundItem.Selected = true;
			ListToSearch.FocusedItem = foundItem;
			int startidx = 0;
			int endidx = 0;
			GetIndexes(listToSearch, out startidx, out endidx);
			if (foundItem.Index < startidx || foundItem.Index > endidx)
			{
				listToSearch.TopItem = foundItem;
			}
		}
	}

	private void GetIndexes(ListView vv, out int startidx, out int endidx)
	{
		ListViewItem itemAt = vv.GetItemAt(vv.ClientRectangle.X + 6, vv.ClientRectangle.Y + 6);
		ListViewItem itemAt2 = vv.GetItemAt(vv.ClientRectangle.X + 6, vv.ClientRectangle.Bottom - 10);
		startidx = vv.Items.IndexOf(itemAt);
		endidx = vv.Items.IndexOf(itemAt2);
		if (endidx == -1)
		{
			endidx = vv.Items.Count;
		}
	}
}

using System.ComponentModel;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

[ProvideProperty("DoLocate", typeof(PropertyGrid))]
public class PropertyGridLocator : ListLocatorBase
{
	private PropertyGrid propertyGridToSearch;

	[Category("Search Behavior")]
	[Description("Tree to search.\r\nIf no tree is selected you can select the locator from the tree it self.\r\nThis option is to allow the locator to work in multiple tree at the same time.")]
	public PropertyGrid PropertyGridToSearch
	{
		get
		{
			return propertyGridToSearch;
		}
		set
		{
			propertyGridToSearch = value;
		}
	}

	public override bool HasListToLocate => PropertyGridToSearch != null;

	public override bool ListHasItems
	{
		get
		{
			if (HasListToLocate)
			{
				return true;
			}
			return false;
		}
	}

	protected override void ExpandAll()
	{
		if (PropertyGridToSearch != null)
		{
			PropertyGridToSearch.ExpandAllGridItems();
		}
	}

	protected override void ContractAll()
	{
		if (PropertyGridToSearch != null)
		{
			PropertyGridToSearch.CollapseAllGridItems();
		}
	}

	private GridItem GetNextNode(GridItem currentItem)
	{
		GridItem gridItem = currentItem.Parent;
		if (gridItem != null)
		{
			bool flag = false;
			foreach (GridItem gridItem2 in gridItem.GridItems)
			{
				if (flag)
				{
					return gridItem2;
				}
				if (!flag && gridItem2 == currentItem)
				{
					flag = true;
				}
			}
		}
		return null;
	}

	private bool Search(PropertyGrid propGrid, string textToSearch)
	{
		GridItem selectedGridItem = propGrid.SelectedGridItem;
		while (selectedGridItem.Parent == null)
		{
			selectedGridItem = selectedGridItem.Parent;
		}
		selectedGridItem = Search(selectedGridItem, selectedGridItem, textToSearch.ToUpper(), skipFirst: true);
		if (selectedGridItem != null)
		{
			ExpandNode(selectedGridItem);
			propGrid.SelectedGridItem = selectedGridItem;
			return true;
		}
		return false;
	}

	private void ExpandNode(GridItem node)
	{
		if (node.Parent != null)
		{
			ExpandNode(node.Parent);
		}
		if (node.Expandable)
		{
			node.Expanded = true;
		}
	}

	private GridItem Search(GridItem origin, GridItem node, string textToSearch, bool skipFirst)
	{
		if (node != null)
		{
			if (!skipFirst && ((base.InString && node.Label.ToUpper().Contains(textToSearch)) || (!base.InString && node.Label.ToUpper().StartsWith(textToSearch))))
			{
				return node;
			}
			GridItem gridItem = null;
			foreach (GridItem gridItem4 in node.GridItems)
			{
				gridItem = Search(origin, gridItem4, textToSearch, skipFirst: false);
				if (gridItem != null)
				{
					return gridItem;
				}
			}
			gridItem = Search(origin, GetNextNode(node), textToSearch, skipFirst: false);
			if (gridItem != null)
			{
				return gridItem;
			}
			if (node.Parent != null && node == origin)
			{
				GridItem gridItem2 = null;
				GridItem gridItem3 = node;
				while (gridItem2 == null && gridItem3 != null)
				{
					gridItem3 = gridItem3.Parent;
					if (gridItem3 != null)
					{
						gridItem2 = GetNextNode(gridItem3);
					}
				}
				if (gridItem2 != null)
				{
					return Search(gridItem2, gridItem2, textToSearch, skipFirst: false);
				}
			}
		}
		return null;
	}

	private GridItem Search(GridItem root, string textToSearch)
	{
		if ((base.InString && root.Label.ToUpper().Contains(textToSearch)) || (!base.InString && root.Label.ToUpper().StartsWith(textToSearch)))
		{
			return root;
		}
		foreach (GridItem gridItem2 in root.GridItems)
		{
			GridItem gridItem = Search(gridItem2, textToSearch);
			if (gridItem != null)
			{
				return gridItem;
			}
		}
		return null;
	}

	[DefaultValue("")]
	[Category("Search Behavior")]
	[Description("Sets the locator for this PropertyGrid")]
	public bool GetDoLocate(Control extendee)
	{
		return PropertyGridToSearch == extendee;
	}

	public void SetDoLocate(Control extendee, object value)
	{
		if (!(extendee is PropertyGrid propertyGrid))
		{
			return;
		}
		if (value == null || !bool.Parse(value.ToString()))
		{
			if (PropertyGridToSearch == extendee)
			{
				PropertyGridToSearch = null;
			}
		}
		else
		{
			PropertyGridToSearch = propertyGrid;
		}
	}

	public override bool Search(string text, bool fromTop)
	{
		if (HasListToLocate)
		{
			return Search(propertyGridToSearch, text);
		}
		return false;
	}

	protected override bool CanLocateAtControl(object extendee)
	{
		return extendee is PropertyGrid;
	}
}

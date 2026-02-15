using System.Collections;

namespace ICSharpCode.SharpDevelop.Gui;

public class TemplateCategoryComparer : IComparer
{
	public int Compare(object x, object y)
	{
		ICategory category = x as ICategory;
		ICategory category2 = y as ICategory;
		if (category.SortOrder != -1 && category2.SortOrder != -1)
		{
			if (category.SortOrder > category2.SortOrder)
			{
				return 1;
			}
			if (category.SortOrder < category2.SortOrder)
			{
				return -1;
			}
		}
		else
		{
			if (category.SortOrder != -1)
			{
				return -1;
			}
			if (category2.SortOrder != -1)
			{
				return 1;
			}
		}
		return string.Compare(category.Name, category2.Name);
	}
}

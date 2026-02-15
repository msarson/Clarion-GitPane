using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class WebReferenceChanges
{
	private List<ProjectItem> newItems = new List<ProjectItem>();

	private List<ProjectItem> itemsRemoved = new List<ProjectItem>();

	public List<ProjectItem> NewItems => newItems;

	public List<ProjectItem> ItemsRemoved => itemsRemoved;

	public bool Changed
	{
		get
		{
			if (itemsRemoved.Count <= 0)
			{
				return newItems.Count > 0;
			}
			return true;
		}
	}
}

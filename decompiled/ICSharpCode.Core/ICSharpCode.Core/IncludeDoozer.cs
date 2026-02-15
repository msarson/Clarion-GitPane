using System.Collections;

namespace ICSharpCode.Core;

public class IncludeDoozer : IDoozer
{
	private class IncludeReturnItem : IBuildItemsModifier
	{
		private string path;

		private object caller;

		public IncludeReturnItem(object caller, string path)
		{
			this.caller = caller;
			this.path = path;
		}

		public void Apply(IList items)
		{
			try
			{
				AddInTreeNode treeNode = AddInTree.GetTreeNode(path);
				foreach (object item in treeNode.BuildChildItems(caller))
				{
					items.Add(item);
				}
			}
			catch (TreePathNotFoundException)
			{
				MessageService.ShowError("IncludeDoozer: AddinTree-Path not found: " + path);
			}
		}
	}

	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		string text = codon.Properties["item"];
		string text2 = codon.Properties["path"];
		if (text != null && text.Length > 0)
		{
			return AddInTree.BuildItem(text, caller);
		}
		if (text2 != null && text2.Length > 0)
		{
			return new IncludeReturnItem(caller, text2);
		}
		MessageService.ShowMessage("<Include> requires the attribute 'item' (to include one item) or the attribute 'path' (to include multiple items)");
		return null;
	}
}

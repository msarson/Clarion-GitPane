using System.Collections.Generic;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class BinaryProjectGeneratorLoader
{
	public static IBinaryProjectGenerator GetGenerator(string treePath)
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode(treePath);
		List<BinaryProjectGeneratorDescriptor> list = treeNode.BuildChildItems<BinaryProjectGeneratorDescriptor>(null);
		if (list.Count > 0)
		{
			return list[0].Generator;
		}
		return null;
	}
}

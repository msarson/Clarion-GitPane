using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop;

public class BinaryFileGeneratorLoader
{
	public static bool Run(string treePath, FileTemplate fileTemplate)
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode(treePath);
		List<BinaryFileGeneratorDescriptor> list = treeNode.BuildChildItems<BinaryFileGeneratorDescriptor>(null);
		if (list.Count > 0)
		{
			IBinaryFileGenerator generator = list[0].Generator;
			if (generator != null)
			{
				return generator.GenerateFiles(fileTemplate);
			}
		}
		return false;
	}
}

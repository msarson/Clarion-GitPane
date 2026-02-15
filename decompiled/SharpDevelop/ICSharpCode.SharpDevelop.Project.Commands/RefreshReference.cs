using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class RefreshReference : AbstractMenuCommand
{
	public override void Run()
	{
		if (Owner is ReferenceNode { ReferenceProjectItem: { } referenceProjectItem })
		{
			ParserService.RefreshProjectContentForReference(referenceProjectItem);
		}
	}
}

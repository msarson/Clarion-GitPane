using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Commands;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ShowBufferOptions : AbstractMenuCommand
{
	public override void Run()
	{
		OptionsCommand.ShowTabbedOptions(ResourceService.GetString("Dialog.Options.BufferOptions"), AddInTree.GetTreeNode("/SharpDevelop/ViewContent/DefaultTextEditor/OptionsDialog"));
	}
}

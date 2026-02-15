using ICSharpCode.Core;

namespace SearchAndReplace;

public class Replace : AbstractMenuCommand
{
	public override void Run()
	{
		Find.SetSearchPattern();
		SearchAndReplaceDialog.ShowSingleInstance(SearchAndReplaceMode.Replace);
	}
}

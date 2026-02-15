using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class EnableCodeCompletion : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return CodeCompletionOptions.EnableCodeCompletion;
		}
		set
		{
			CodeCompletionOptions.EnableCodeCompletion = value;
		}
	}
}

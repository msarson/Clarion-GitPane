namespace ICSharpCode.Core;

public interface ITextBoxCommand : ICommand
{
	bool IsEnabled { get; set; }
}

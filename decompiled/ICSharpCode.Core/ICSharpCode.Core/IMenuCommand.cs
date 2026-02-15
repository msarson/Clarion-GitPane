namespace ICSharpCode.Core;

public interface IMenuCommand : ICommand
{
	bool IsEnabled { get; set; }
}

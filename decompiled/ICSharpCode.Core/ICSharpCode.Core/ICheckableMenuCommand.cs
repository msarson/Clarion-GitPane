namespace ICSharpCode.Core;

public interface ICheckableMenuCommand : IMenuCommand, ICommand
{
	bool IsChecked { get; set; }
}

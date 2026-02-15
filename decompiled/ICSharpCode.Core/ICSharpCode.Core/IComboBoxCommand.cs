namespace ICSharpCode.Core;

public interface IComboBoxCommand : ICommand
{
	bool IsEnabled { get; set; }
}

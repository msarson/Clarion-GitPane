namespace ICSharpCode.Core;

public abstract class AbstractCheckableMenuCommand : AbstractMenuCommand, ICheckableMenuCommand, IMenuCommand, ICommand
{
	private bool isChecked;

	public virtual bool IsChecked
	{
		get
		{
			return isChecked;
		}
		set
		{
			isChecked = value;
		}
	}

	public override void Run()
	{
		IsChecked = !IsChecked;
	}
}

namespace ICSharpCode.Core;

public abstract class AbstractMenuCommand : AbstractCommand, IMenuCommand, ICommand
{
	private bool isEnabled = true;

	public virtual bool IsEnabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			isEnabled = value;
		}
	}
}

namespace ICSharpCode.Core;

public abstract class AbstractComboBoxCommand : AbstractCommand, IComboBoxCommand, ICommand
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

	public override void Run()
	{
	}
}

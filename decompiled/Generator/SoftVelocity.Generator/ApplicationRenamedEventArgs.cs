namespace SoftVelocity.Generator;

public class ApplicationRenamedEventArgs : ApplicationEventArgs
{
	private string _OldName;

	public string OldName => _OldName;

	public ApplicationRenamedEventArgs(Application application, string oldName)
		: base(application)
	{
		_OldName = oldName;
	}
}

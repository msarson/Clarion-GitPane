namespace SoftVelocity.Generator;

public class ApplicationGeneratingEventArgs : ApplicationCancelEventArgs
{
	private PosGenerationAction _posGenAction;

	public PosGenerationAction posGenAction => _posGenAction;

	public ApplicationGeneratingEventArgs(Application application, PosGenerationAction posGenAction)
		: base(application)
	{
		_posGenAction = posGenAction;
	}
}

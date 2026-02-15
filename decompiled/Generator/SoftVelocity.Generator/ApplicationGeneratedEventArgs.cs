namespace SoftVelocity.Generator;

public class ApplicationGeneratedEventArgs : ApplicationEventArgs
{
	private PosGenerationAction _posGenAction;

	private bool _Generated;

	public PosGenerationAction posGenAction => _posGenAction;

	public bool Generated => _Generated;

	public ApplicationGeneratedEventArgs(Application applications, PosGenerationAction posGenAction, bool generated)
		: base(applications)
	{
		_posGenAction = posGenAction;
		_Generated = generated;
	}
}

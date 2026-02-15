using System.Collections.Generic;

namespace SoftVelocity.Generator;

public class GenerationEndEventArgs : ApplicationsEventArgs
{
	private PosGenerationAction _posGenAction;

	private bool _NoErrors;

	public PosGenerationAction posGenAction => _posGenAction;

	public bool NoErrors => _NoErrors;

	public GenerationEndEventArgs(IEnumerable<Application> generatedApplications, PosGenerationAction posGenAction, bool noErrors)
		: base(generatedApplications)
	{
		_posGenAction = posGenAction;
		_NoErrors = noErrors;
	}
}

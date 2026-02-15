using System.Collections.Generic;

namespace SoftVelocity.Generator;

public class GenerationStartEventArgs : ApplicationsCancelEventArgs
{
	private PosGenerationAction _posGenAction;

	public PosGenerationAction posGenAction => _posGenAction;

	public GenerationStartEventArgs(IEnumerable<Application> applications, PosGenerationAction posGenAction)
		: base(applications)
	{
		_posGenAction = posGenAction;
	}
}

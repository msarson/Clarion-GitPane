namespace SoftVelocity.Generator.PWEE;

public interface IPweeDetails : IEditorDetails, IGenerator
{
	IPweePart[] Parts { get; }

	IPweePart Start { get; }

	string Module { get; }
}

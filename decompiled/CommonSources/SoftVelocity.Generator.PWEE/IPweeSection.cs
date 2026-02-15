namespace SoftVelocity.Generator.PWEE;

public interface IPweeSection : IPweePart
{
	IPweePart[] Parts { get; }

	string Header { get; }

	string Footer { get; }

	uint Indentation { get; }
}

namespace SoftVelocity.Generator.PWEE;

public interface IPweePart
{
	bool IsText { get; }

	bool TryGet(IPweePart toFind, out IPweePart result);
}

namespace SoftVelocity.Generator.PWEE;

public interface IPweeText : IPweePart
{
	ITextSection Text { get; }

	uint Priority { get; }

	bool IsLiteral { get; }
}

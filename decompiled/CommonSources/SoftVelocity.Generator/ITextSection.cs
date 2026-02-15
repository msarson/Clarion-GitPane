namespace SoftVelocity.Generator;

public interface ITextSection
{
	bool IsData { get; }

	uint Column { get; }

	string Text { get; }
}

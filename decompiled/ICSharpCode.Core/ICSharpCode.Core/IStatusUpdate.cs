namespace ICSharpCode.Core;

public interface IStatusUpdate
{
	string CodonId { get; }

	void UpdateStatus();

	void UpdateText();
}

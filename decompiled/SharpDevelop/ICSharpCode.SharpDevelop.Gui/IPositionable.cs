namespace ICSharpCode.SharpDevelop.Gui;

public interface IPositionable
{
	int Line { get; }

	int Column { get; }

	void JumpTo(int line, int column);
}

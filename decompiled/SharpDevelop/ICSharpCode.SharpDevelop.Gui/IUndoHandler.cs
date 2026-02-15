namespace ICSharpCode.SharpDevelop.Gui;

public interface IUndoHandler
{
	bool EnableUndo { get; }

	bool EnableRedo { get; }

	void Undo();

	void Redo();
}

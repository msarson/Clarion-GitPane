namespace ICSharpCode.SharpDevelop.Internal.Undo;

public interface IUndoableOperation
{
	void Undo();

	void Redo();
}

namespace ICSharpCode.Core;

public interface IMementoCapable
{
	Properties CreateMemento();

	void SetMemento(Properties memento);
}

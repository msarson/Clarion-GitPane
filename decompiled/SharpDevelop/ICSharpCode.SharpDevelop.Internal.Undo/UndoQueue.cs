using System;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop.Internal.Undo;

internal class UndoQueue : IUndoableOperation
{
	private List<IUndoableOperation> undolist = new List<IUndoableOperation>();

	public UndoQueue(UndoStack stack, int numops)
	{
		if (stack == null)
		{
			throw new ArgumentNullException("stack");
		}
		for (int i = 0; i < numops; i++)
		{
			if (stack._UndoStack.Count > 0)
			{
				undolist.Add(stack._UndoStack.Pop());
			}
		}
	}

	public void Undo()
	{
		for (int i = 0; i < undolist.Count; i++)
		{
			undolist[i].Undo();
		}
	}

	public void Redo()
	{
		for (int num = undolist.Count - 1; num >= 0; num--)
		{
			undolist[num].Redo();
		}
	}
}

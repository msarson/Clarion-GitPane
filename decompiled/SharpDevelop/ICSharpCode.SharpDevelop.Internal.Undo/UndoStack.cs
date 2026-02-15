using System;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop.Internal.Undo;

public class UndoStack
{
	private Stack<IUndoableOperation> undostack = new Stack<IUndoableOperation>();

	private Stack<IUndoableOperation> redostack = new Stack<IUndoableOperation>();

	public bool AcceptChanges = true;

	internal Stack<IUndoableOperation> _UndoStack => undostack;

	public bool CanUndo => undostack.Count > 0;

	public bool CanRedo => redostack.Count > 0;

	public event EventHandler ActionUndone;

	public event EventHandler ActionRedone;

	public void UndoLast(int x)
	{
		undostack.Push(new UndoQueue(this, x));
	}

	public void Undo()
	{
		if (undostack.Count > 0)
		{
			IUndoableOperation undoableOperation = undostack.Pop();
			redostack.Push(undoableOperation);
			undoableOperation.Undo();
			OnActionUndone();
		}
	}

	public void Redo()
	{
		if (redostack.Count > 0)
		{
			IUndoableOperation undoableOperation = redostack.Pop();
			undostack.Push(undoableOperation);
			undoableOperation.Redo();
			OnActionRedone();
		}
	}

	public void Push(IUndoableOperation operation)
	{
		if (operation == null)
		{
			throw new ArgumentNullException("UndoStack.Push(UndoableOperation operation) : operation can't be null");
		}
		if (AcceptChanges)
		{
			undostack.Push(operation);
			ClearRedoStack();
		}
	}

	public void ClearRedoStack()
	{
		redostack.Clear();
	}

	public void ClearAll()
	{
		undostack.Clear();
		redostack.Clear();
	}

	protected void OnActionUndone()
	{
		if (this.ActionUndone != null)
		{
			this.ActionUndone(null, null);
		}
	}

	protected void OnActionRedone()
	{
		if (this.ActionRedone != null)
		{
			this.ActionRedone(null, null);
		}
	}
}

using System;

namespace SoftVelocity.Common;

public class EntityWithHistory
{
	protected UndoRedoManager _HistoryManager;

	protected UndoRedoManager HistoryManager
	{
		get
		{
			return _HistoryManager;
		}
		set
		{
			if (_HistoryManager == null)
			{
				_HistoryManager = value;
				_HistoryManager.StateChanged += HistoryManager_StateChanged;
			}
		}
	}

	public bool CanUndo => HistoryManager.CanUndo;

	public bool CanRedo => HistoryManager.CanRedo;

	public event EventHandler UndoStateChanged;

	public EntityWithHistory()
	{
		HistoryManager = new UndoRedoManager();
	}

	private void HistoryManager_StateChanged(object sender, EventArgs e)
	{
		if (this.UndoStateChanged != null)
		{
			this.UndoStateChanged(this, null);
		}
	}

	public void Undo()
	{
		HistoryManager.DoUndo();
	}

	public void Redo()
	{
		HistoryManager.Redo();
	}

	public void UndoAll()
	{
		HistoryManager.UndoAll();
	}

	public void RedoAll()
	{
		HistoryManager.RedoAll();
	}
}

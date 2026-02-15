using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common;

public class UndoRedoManager : IUndoHandler
{
	public delegate void UndoRedoExecutedEventHandler(object sender, object target);

	private class HistoryTransactionCommand : HistoryCommand
	{
		private List<HistoryCommand> commands = new List<HistoryCommand>();

		private bool commited;

		public HistoryTransactionCommand(object target)
			: base(target)
		{
		}

		protected override void Cleaning()
		{
			foreach (HistoryCommand command in commands)
			{
				command.Clean();
			}
			commands.Clear();
			commands = null;
			base.Cleaning();
		}

		public void Commit()
		{
			commited = true;
		}

		public void Rollback()
		{
			if (!commited)
			{
				UnExecute();
				commands.Clear();
			}
		}

		public void AddCommand(HistoryCommand command)
		{
			if (!commited)
			{
				commands.Add(command);
			}
		}

		protected override void DoExecute()
		{
			foreach (HistoryCommand command in commands)
			{
				command.Execute();
			}
		}

		protected override void DoUnExecute()
		{
			foreach (HistoryCommand command in commands)
			{
				command.UnExecute();
			}
		}
	}

	private Stack<HistoryCommand> redoStack = new Stack<HistoryCommand>();

	private Stack<HistoryCommand> undoStack = new Stack<HistoryCommand>();

	private bool onProcess;

	private HistoryTransactionCommand transaction;

	private bool onAll;

	public bool EnableRedo
	{
		get
		{
			if (redoStack.Count <= 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool EnableUndo
	{
		get
		{
			if (undoStack.Count <= 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool CanUndo => EnableUndo;

	public bool CanRedo => EnableRedo;

	public event EventHandler StateChanged;

	public event UndoRedoExecutedEventHandler UndoRedoExecuted;

	private void OnStateChanged()
	{
		if (this.StateChanged != null)
		{
			this.StateChanged(this, EventArgs.Empty);
		}
	}

	private void OnUndoRedoExecuted(object target)
	{
		if (this.UndoRedoExecuted != null)
		{
			this.UndoRedoExecuted(this, target);
		}
	}

	public void AddCommand(HistoryCommand command)
	{
		if (!onProcess)
		{
			if (transaction != null)
			{
				transaction.AddCommand(command);
			}
			else
			{
				undoStack.Push(command);
			}
			OnStateChanged();
		}
	}

	public void Undo()
	{
		DoUndo();
	}

	public bool DoUndo()
	{
		if (undoStack.Count > 0)
		{
			HistoryCommand historyCommand = undoStack.Pop();
			if (historyCommand != null)
			{
				onProcess = true;
				historyCommand.UnExecute();
				onProcess = false;
				redoStack.Push(historyCommand);
				OnUndoRedoExecuted(historyCommand.Target);
				if (!onAll)
				{
					OnStateChanged();
				}
				return true;
			}
		}
		return false;
	}

	public void Redo()
	{
		DoRedo();
	}

	public bool DoRedo()
	{
		if (redoStack.Count > 0)
		{
			HistoryCommand historyCommand = redoStack.Pop();
			if (historyCommand != null)
			{
				onProcess = true;
				historyCommand.Execute();
				onProcess = false;
				undoStack.Push(historyCommand);
				OnUndoRedoExecuted(historyCommand.Target);
				if (!onAll)
				{
					OnStateChanged();
				}
				return true;
			}
		}
		return false;
	}

	public void StartTransaction(object target)
	{
		if (transaction == null)
		{
			transaction = new HistoryTransactionCommand(target);
		}
	}

	public void Commit()
	{
		if (transaction != null)
		{
			transaction.Commit();
			HistoryTransactionCommand command = transaction;
			transaction = null;
			AddCommand(command);
		}
	}

	public void Rollback()
	{
		if (transaction != null)
		{
			transaction.Rollback();
			transaction = null;
		}
	}

	public void UndoAll()
	{
		onAll = true;
		while (DoUndo())
		{
		}
		onAll = false;
		OnStateChanged();
	}

	public void RedoAll()
	{
		onAll = true;
		while (DoRedo())
		{
		}
		onAll = false;
		OnStateChanged();
	}

	public void CleanHistory()
	{
		foreach (HistoryCommand item in undoStack)
		{
			item.Clean();
		}
		foreach (HistoryCommand item2 in redoStack)
		{
			item2.Clean();
		}
		undoStack.Clear();
		redoStack.Clear();
		OnStateChanged();
	}
}

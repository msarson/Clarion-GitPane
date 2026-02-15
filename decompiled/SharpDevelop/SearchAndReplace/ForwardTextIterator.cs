using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class ForwardTextIterator : ITextIterator
{
	private enum TextIteratorState
	{
		Resetted,
		Iterating,
		Done
	}

	private ProvidedDocumentInformation info;

	private TextIteratorState state;

	private ITextBufferStrategy textBuffer;

	private int endOffset;

	private int oldOffset = -1;

	private int position;

	private bool stopAtEnd;

	private bool doCircularSearch = true;

	public ITextBufferStrategy TextBuffer => textBuffer;

	public char Current => state switch
	{
		TextIteratorState.Resetted => throw new InvalidOperationException("Call moveAhead first"), 
		TextIteratorState.Iterating => textBuffer.GetCharAt(Position), 
		TextIteratorState.Done => throw new InvalidOperationException("TextIterator is at the end"), 
		_ => throw new InvalidOperationException("unknown text iterator state"), 
	};

	public IDocument Document => info.Document;

	public int Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	public void ResetCaret()
	{
		if (info.CurrentOffset < position)
		{
			oldOffset = -1;
		}
	}

	public ForwardTextIterator(ProvidedDocumentInformation info)
	{
		this.info = info;
		textBuffer = info.TextBuffer;
		position = info.CurrentOffset;
		endOffset = info.EndOffset;
		doCircularSearch = PropertyService.Get("CircularSearch", true, "SearchAndReplaceProperties");
		Reset();
	}

	public char GetCharRelative(int offset)
	{
		if (state != TextIteratorState.Iterating)
		{
			throw new InvalidOperationException();
		}
		int offset2 = (Position + (1 + Math.Abs(offset) / textBuffer.Length) * textBuffer.Length + offset) % textBuffer.Length;
		return textBuffer.GetCharAt(offset2);
	}

	public bool MoveAhead(int numChars)
	{
		switch (state)
		{
		case TextIteratorState.Resetted:
			if (textBuffer.Length == 0)
			{
				state = TextIteratorState.Done;
				return false;
			}
			Position = endOffset;
			state = TextIteratorState.Iterating;
			return true;
		case TextIteratorState.Done:
			return false;
		case TextIteratorState.Iterating:
		{
			if (oldOffset == -1 && textBuffer.Length == endOffset)
			{
				Position--;
			}
			if (oldOffset != -1 && Position == endOffset - 1 && textBuffer.Length == endOffset)
			{
				state = TextIteratorState.Done;
				return false;
			}
			int num = (Position + numChars) % textBuffer.Length;
			if (!doCircularSearch && num < Position && MessageBox.Show((Form)WorkbenchSingleton.Workbench, ResourceService.GetString("Dialog.NewProject.SearchReplace.SearchEndOfFile"), ResourceService.GetString("Dialog.NewProject.SearchReplace.SearchEndOfFile.Title"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) != DialogResult.OK)
			{
				state = TextIteratorState.Done;
				return false;
			}
			Position = num;
			bool flag = (oldOffset != -1 && (oldOffset > Position || oldOffset < endOffset) && Position >= endOffset) || (stopAtEnd && Position + numChars >= textBuffer.Length);
			if (oldOffset != -1 && oldOffset == endOffset - 1 && textBuffer.Length == endOffset)
			{
				flag = true;
			}
			oldOffset = Position;
			if (flag)
			{
				state = TextIteratorState.Done;
				return false;
			}
			return true;
		}
		default:
			throw new Exception("Unknown text iterator state");
		}
	}

	public void InformReplace(int offset, int length, int newLength)
	{
		if (offset <= endOffset)
		{
			endOffset = endOffset - length + newLength;
		}
		if (offset <= Position)
		{
			Position = Position - length + newLength;
		}
		if (offset <= oldOffset)
		{
			oldOffset = oldOffset - length + newLength;
		}
	}

	public void Reset()
	{
		if (endOffset == position && position == 0)
		{
			stopAtEnd = true;
		}
		else
		{
			stopAtEnd = false;
		}
		state = TextIteratorState.Resetted;
		Position = endOffset;
		oldOffset = -1;
	}

	public override string ToString()
	{
		return $"[ForwardTextIterator: Position={Position}, endOffset={endOffset}, state={state}]";
	}
}

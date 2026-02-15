using System.Windows.Forms;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common.Redirection.Action;

public class OpenSelectedViaRedirectionFile : AbstractEditAction
{
	private bool _ForceEmptyDialog;

	internal bool ForceEmptyDialog
	{
		get
		{
			return _ForceEmptyDialog;
		}
		set
		{
			_ForceEmptyDialog = value;
		}
	}

	public override void Execute(TextArea textArea)
	{
		string text = textArea.SelectionManager.SelectedText;
		if (string.IsNullOrEmpty(text))
		{
			text = GetFileNameOnCaret(textArea);
		}
		if (!string.IsNullOrEmpty(text))
		{
			RedirectionFile val = CommonClarionProject.CurrentRedirectionFile(null);
			bool flag = PropertyService.Get<bool>("DisplayRedDialog", true, "ClarionEditor", new string[0]);
			if (!flag && !val.Exists(text, RedirectionFile.CurrentDirectory))
			{
				MessageBox.Show(string.Format(ResourceService.GetString("Clarion.Gui.OpenViaRedirection.FileNotFound"), text), ResourceService.GetString("Clarion.Gui.OpenViaRedirection.FileNotFound.Title"));
				flag = true;
			}
			if (flag)
			{
				OpenViaRedirectionFile openViaRedirectionFile = new OpenViaRedirectionFile();
				openViaRedirectionFile.DefaultFileName = text;
				((AbstractCommand)openViaRedirectionFile).Run();
			}
			else
			{
				FileService.OpenFile(val.OpenName(text, RedirectionFile.CurrentDirectory));
			}
		}
		else if (PropertyService.Get<bool>("OpenEmptyRedDialogIfNotSelected", false, "ClarionEditor", new string[0]) || ForceEmptyDialog)
		{
			OpenViaRedirectionFile openViaRedirectionFile2 = new OpenViaRedirectionFile();
			((AbstractCommand)openViaRedirectionFile2).Run();
		}
	}

	protected string GetFileNameOnCaret(TextArea textArea)
	{
		string text = string.Empty;
		if (textArea != null)
		{
			TextEditorControl motherTextEditorControl = textArea.MotherTextEditorControl;
			if (motherTextEditorControl != null)
			{
				IDocument document = ((TextEditorControlBase)motherTextEditorControl).Document;
				if (document != null)
				{
					int num = document.GetLineNumberForOffset(((TextEditorControlBase)motherTextEditorControl).ActiveTextAreaControl.Caret.Offset) + 1;
					int num2 = ((TextEditorControlBase)motherTextEditorControl).ActiveTextAreaControl.Caret.Offset - document.GetLineSegment(num - 1).Offset;
					LineSegment lineSegment = document.GetLineSegment(num - 1);
					if (lineSegment != null)
					{
						TextWord word = lineSegment.GetWord(num2);
						if (word != null)
						{
							text = word.Word;
							if (text.Length == 0 || (text.Length == 1 && "/ *=+-<>,#!@$%|?".Contains(text)))
							{
								return string.Empty;
							}
							int length = word.Length;
							int offset = word.Offset;
							bool flag = false;
							if (length == 1 && text == "." && offset > 0)
							{
								word = lineSegment.GetWord(offset - 1);
								if (word == null)
								{
									return string.Empty;
								}
								text = word.Word;
								length = word.Length;
								offset = word.Offset;
							}
							if (offset > 0)
							{
								TextWord word2 = lineSegment.GetWord(offset - 1);
								if (word2 == null)
								{
									flag = true;
								}
								else if (word2.Word == "." && offset > 1)
								{
									word2 = lineSegment.GetWord(offset - 2);
									text = word2.Word + '.' + text;
									flag = false;
								}
								else if (word2.IsWhiteSpace || "/ *=+-<>,#!@$%|.?".Contains(word2.Word.Substring(0, 1)))
								{
									flag = true;
								}
							}
							else
							{
								flag = true;
							}
							if (flag && length > 0)
							{
								TextWord word3 = lineSegment.GetWord(offset + length + 1);
								if (word3 != null)
								{
									text = text + '.' + word3.Word;
								}
							}
						}
					}
				}
			}
		}
		if (text == ".")
		{
			return string.Empty;
		}
		return text;
	}
}

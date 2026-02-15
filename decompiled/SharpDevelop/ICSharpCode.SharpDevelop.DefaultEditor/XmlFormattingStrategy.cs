using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor;

public class XmlFormattingStrategy : DefaultFormattingStrategy
{
	public override void FormatLine(TextArea textArea, int lineNr, int caretOffset, char charTyped)
	{
		textArea.Document.UndoStack.StartUndoGroup();
		try
		{
			if (charTyped == '>')
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int num = Math.Min(caretOffset - 2, textArea.Document.TextLength - 1); num >= 0; num--)
				{
					char charAt = textArea.Document.GetCharAt(num);
					if (charAt == '<')
					{
						string text = stringBuilder.ToString().Trim();
						if (text.StartsWith("/") || text.EndsWith("/"))
						{
							break;
						}
						bool flag = true;
						try
						{
							XmlDocument xmlDocument = new XmlDocument();
							xmlDocument.LoadXml(textArea.Document.TextContent);
						}
						catch (Exception)
						{
							flag = false;
						}
						if (!flag)
						{
							StringBuilder stringBuilder2 = new StringBuilder();
							int num2 = text.Length - 1;
							while (num2 >= 0 && !char.IsWhiteSpace(text[num2]))
							{
								stringBuilder2.Append(text[num2]);
								num2--;
							}
							string text2 = stringBuilder2.ToString();
							if (text2.Length > 0 && !text2.StartsWith("!") && !text2.StartsWith("?"))
							{
								textArea.Document.Insert(caretOffset, "</" + text2 + ">");
							}
						}
						break;
					}
					stringBuilder.Append(charAt);
				}
			}
		}
		catch (Exception)
		{
		}
		if (charTyped == '\n')
		{
			textArea.Caret.Column = IndentLine(textArea, lineNr);
		}
		textArea.Document.UndoStack.EndUndoGroup();
	}

	protected override int SmartIndentLine(TextArea textArea, int lineNr)
	{
		if (lineNr <= 0)
		{
			return AutoIndentLine(textArea, lineNr);
		}
		try
		{
			TryIndent(textArea, lineNr, lineNr);
			return GetIndentation(textArea, lineNr).Length;
		}
		catch (XmlException)
		{
			return AutoIndentLine(textArea, lineNr);
		}
	}

	public override void IndentLines(TextArea textArea, int begin, int end)
	{
		textArea.Document.UndoStack.StartUndoGroup();
		try
		{
			TryIndent(textArea, begin, end);
		}
		catch (XmlException ex)
		{
			LoggingService.Debug(ex.ToString());
		}
		finally
		{
			textArea.Document.UndoStack.EndUndoGroup();
		}
	}

	private void TryIndent(TextArea textArea, int begin, int end)
	{
		string text = "";
		Stack stack = new Stack();
		IDocument document = textArea.Document;
		string indentationString = Tab.GetIndentationString(document);
		int i = begin;
		bool flag = false;
		XmlNodeType xmlNodeType = XmlNodeType.XmlDeclaration;
		using StringReader input = new StringReader(document.TextContent);
		XmlTextReader xmlTextReader = new XmlTextReader(input);
		xmlTextReader.XmlResolver = null;
		while (xmlTextReader.Read())
		{
			if (flag)
			{
				flag = false;
				text = ((stack.Count != 0) ? ((string)stack.Pop()) : "");
			}
			if (xmlTextReader.NodeType == XmlNodeType.EndElement)
			{
				text = ((stack.Count != 0) ? ((string)stack.Pop()) : "");
			}
			while (xmlTextReader.LineNumber > i && i <= end)
			{
				if (xmlNodeType == XmlNodeType.CDATA || xmlNodeType == XmlNodeType.Comment)
				{
					i++;
					continue;
				}
				LineSegment lineSegment = document.GetLineSegment(i);
				string text2 = document.GetText(lineSegment);
				string text3 = ((!(text2.Trim() == ">")) ? (text + text2.Trim()) : ((string)stack.Peek() + text2.Trim()));
				if (text3 != text2)
				{
					document.Replace(lineSegment.Offset, lineSegment.Length, text3);
				}
				i++;
			}
			if (xmlTextReader.LineNumber > end)
			{
				break;
			}
			flag = xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.IsEmptyElement;
			string text4 = null;
			if (xmlTextReader.NodeType == XmlNodeType.Element)
			{
				stack.Push(text);
				if (xmlTextReader.LineNumber < begin)
				{
					text = GetIndentation(textArea, xmlTextReader.LineNumber - 1);
				}
				text4 = ((xmlTextReader.Name.Length >= 16) ? (text + indentationString) : (text + new string(' ', 2 + xmlTextReader.Name.Length)));
				text += indentationString;
			}
			xmlNodeType = xmlTextReader.NodeType;
			if (xmlTextReader.NodeType != XmlNodeType.Element || !xmlTextReader.HasAttributes)
			{
				continue;
			}
			int lineNumber = xmlTextReader.LineNumber;
			xmlTextReader.MoveToAttribute(0);
			if (xmlTextReader.LineNumber != lineNumber)
			{
				text4 = text;
			}
			xmlTextReader.MoveToAttribute(xmlTextReader.AttributeCount - 1);
			for (; xmlTextReader.LineNumber > i && i <= end; i++)
			{
				LineSegment lineSegment2 = document.GetLineSegment(i);
				string text5 = document.GetText(lineSegment2);
				string text6 = text4 + text5.Trim();
				if (text6 != text5)
				{
					document.Replace(lineSegment2.Offset, lineSegment2.Length, text6);
				}
			}
		}
		xmlTextReader.Close();
	}
}

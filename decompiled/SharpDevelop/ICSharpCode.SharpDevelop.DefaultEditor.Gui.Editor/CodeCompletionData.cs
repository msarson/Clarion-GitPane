using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class CodeCompletionData : ICompletionData, IComparable
{
	private IAmbience ambience;

	private int imageIndex;

	private int overloads;

	private string text;

	private string description;

	private string documentation;

	private IClass c;

	private IMember member;

	private bool convertedDocumentation;

	private double priority;

	private string dotnetName;

	internal static Regex whitespace = new Regex("\\s+");

	public IClass Class => c;

	public IMember Member => member;

	public int Overloads
	{
		get
		{
			return overloads;
		}
		set
		{
			overloads = value;
		}
	}

	public double Priority
	{
		get
		{
			return priority;
		}
		set
		{
			priority = value;
		}
	}

	public int ImageIndex
	{
		get
		{
			return imageIndex;
		}
		set
		{
			imageIndex = value;
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	public string Description
	{
		get
		{
			if (description == null)
			{
				description = LazyConvertDescription();
			}
			if (description.Length == 0 && (documentation == null || documentation.Length == 0))
			{
				return string.Empty;
			}
			if (!convertedDocumentation && documentation != null)
			{
				convertedDocumentation = true;
				documentation = GetDocumentation(documentation);
			}
			return description + ((overloads > 0) ? (" " + StringParser.Parse("${res:ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor.CodeCompletionData.OverloadsCounter}", new string[1, 2] { 
			{
				"NumOverloads",
				overloads.ToString()
			} })) : string.Empty) + "\n" + documentation;
		}
		set
		{
			description = value;
		}
	}

	private string LazyConvertDescription()
	{
		if (c != null)
		{
			return ambience.Convert(c);
		}
		if (member is IMethod)
		{
			return ambience.Convert((IMethod)member);
		}
		if (member is IField)
		{
			return ambience.Convert((IField)member);
		}
		if (member is IProperty)
		{
			return ambience.Convert((IProperty)member);
		}
		if (member is IEvent)
		{
			return ambience.Convert((IEvent)member);
		}
		return string.Empty;
	}

	private void GetPriority(string dotnetName)
	{
		this.dotnetName = dotnetName;
		priority = CodeCompletionDataUsageCache.GetPriority(dotnetName, incrementShowCount: true);
	}

	public CodeCompletionData(string s, int imageIndex)
	{
		ambience = AmbienceService.CurrentAmbience;
		description = (documentation = string.Empty);
		text = s;
		this.imageIndex = imageIndex;
		GetPriority(s);
	}

	public CodeCompletionData(IClass c)
	{
		ambience = AmbienceService.CurrentAmbience;
		this.c = c;
		imageIndex = ClassBrowserIconService.GetIcon(c);
		ambience.ConversionFlags = ConversionFlags.None;
		text = ambience.Convert(c);
		ambience.ConversionFlags = ConversionFlags.UseFullyQualifiedNames | ConversionFlags.ShowModifiers | ConversionFlags.ShowReturnType;
		documentation = c.Documentation;
		GetPriority(c.DotNetName);
	}

	public CodeCompletionData(IMethod method)
	{
		member = method;
		ambience = AmbienceService.CurrentAmbience;
		ambience.ConversionFlags = ConversionFlags.ShowParameterNames | ConversionFlags.ShowModifiers | ConversionFlags.ShowReturnType;
		imageIndex = ClassBrowserIconService.GetIcon(method);
		text = method.Name;
		documentation = method.Documentation;
		GetPriority(method.DotNetName);
	}

	public CodeCompletionData(IField field)
	{
		member = field;
		ambience = AmbienceService.CurrentAmbience;
		ambience.ConversionFlags = ConversionFlags.ShowParameterNames | ConversionFlags.ShowModifiers | ConversionFlags.ShowReturnType;
		imageIndex = ClassBrowserIconService.GetIcon(field);
		text = field.Name;
		documentation = field.Documentation;
		GetPriority(field.DotNetName);
	}

	public CodeCompletionData(IProperty property)
	{
		member = property;
		ambience = AmbienceService.CurrentAmbience;
		ambience.ConversionFlags = ConversionFlags.ShowParameterNames | ConversionFlags.ShowModifiers | ConversionFlags.ShowReturnType;
		imageIndex = ClassBrowserIconService.GetIcon(property);
		text = property.Name;
		documentation = property.Documentation;
		GetPriority(property.DotNetName);
	}

	public CodeCompletionData(IEvent e)
	{
		member = e;
		ambience = AmbienceService.CurrentAmbience;
		ambience.ConversionFlags = ConversionFlags.ShowParameterNames | ConversionFlags.ShowModifiers | ConversionFlags.ShowReturnType;
		imageIndex = ClassBrowserIconService.GetIcon(e);
		text = e.Name;
		documentation = e.Documentation;
		GetPriority(e.DotNetName);
	}

	public virtual bool InsertAction(TextArea textArea, char ch)
	{
		if (dotnetName != null)
		{
			CodeCompletionDataUsageCache.IncrementUsage(dotnetName);
		}
		if (c != null && text.Length > c.Name.Length)
		{
			textArea.InsertString(text.Substring(0, c.Name.Length + 1));
			TextLocation position = textArea.Caret.Position;
			int num = text.IndexOf(',');
			TextLocation position2;
			if (num < 0)
			{
				textArea.InsertString(text.Substring(c.Name.Length + 1));
				position2 = textArea.Caret.Position;
				position2.X--;
			}
			else
			{
				textArea.InsertString(text.Substring(c.Name.Length + 1, num - c.Name.Length - 1));
				position2 = textArea.Caret.Position;
				textArea.InsertString(text.Substring(num));
			}
			textArea.Caret.Position = position;
			textArea.SelectionManager.SetSelection(position, position2);
			if (!char.IsLetterOrDigit(ch))
			{
				return true;
			}
		}
		else
		{
			textArea.InsertString(text);
		}
		return false;
	}

	public static string GetDocumentation(string doc)
	{
		StringReader input = new StringReader("<docroot>" + doc + "</docroot>");
		XmlTextReader xmlTextReader = new XmlTextReader(input);
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			xmlTextReader.Read();
			do
			{
				if (xmlTextReader.NodeType == XmlNodeType.Element)
				{
					switch (xmlTextReader.Name.ToLowerInvariant())
					{
					case "filterpriority":
						xmlTextReader.Skip();
						break;
					case "remarks":
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append("Remarks:");
						stringBuilder.Append(Environment.NewLine);
						break;
					case "example":
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append("Example:");
						stringBuilder.Append(Environment.NewLine);
						break;
					case "exception":
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append(GetCref(xmlTextReader["cref"]));
						stringBuilder.Append(": ");
						break;
					case "returns":
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append("Returns: ");
						break;
					case "see":
						stringBuilder.Append(GetCref(xmlTextReader["cref"]));
						stringBuilder.Append(xmlTextReader["langword"]);
						break;
					case "seealso":
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append("See also: ");
						stringBuilder.Append(GetCref(xmlTextReader["cref"]));
						break;
					case "paramref":
						stringBuilder.Append(xmlTextReader["name"]);
						break;
					case "param":
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append(whitespace.Replace(xmlTextReader["name"].Trim(), " "));
						stringBuilder.Append(": ");
						break;
					case "value":
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append("Value: ");
						stringBuilder.Append(Environment.NewLine);
						break;
					case "br":
					case "para":
						stringBuilder.Append(Environment.NewLine);
						break;
					}
				}
				else if (xmlTextReader.NodeType == XmlNodeType.Text)
				{
					stringBuilder.Append(whitespace.Replace(xmlTextReader.Value, " "));
				}
			}
			while (xmlTextReader.Read());
		}
		catch (Exception ex)
		{
			LoggingService.Debug("Invalid XML documentation: " + ex.Message);
			return doc;
		}
		return stringBuilder.ToString();
	}

	private static string GetCref(string cref)
	{
		if (cref == null || cref.Trim().Length == 0)
		{
			return "";
		}
		if (cref.Length < 2)
		{
			return cref;
		}
		if (cref.Substring(1, 1) == ":")
		{
			return cref.Substring(2, cref.Length - 2);
		}
		return cref;
	}

	public int CompareTo(object obj)
	{
		if (obj == null || !(obj is CodeCompletionData))
		{
			return -1;
		}
		return text.CompareTo(((CodeCompletionData)obj).text);
	}
}

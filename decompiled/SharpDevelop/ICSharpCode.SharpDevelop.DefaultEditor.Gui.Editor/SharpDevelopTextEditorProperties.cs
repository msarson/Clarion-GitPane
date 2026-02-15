using System;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.OptionPanels;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public sealed class SharpDevelopTextEditorProperties : ITextEditorProperties
{
	private static SharpDevelopTextEditorProperties textEditorProperties;

	private Properties properties;

	private FontContainer fontContainer;

	private bool useCustomLine;

	public static SharpDevelopTextEditorProperties Instance
	{
		get
		{
			if (textEditorProperties == null)
			{
				textEditorProperties = new SharpDevelopTextEditorProperties();
			}
			return textEditorProperties;
		}
	}

	public int TabIndent
	{
		get
		{
			return properties.Get("TabIndent", 4);
		}
		set
		{
			properties.Set("TabIndent", value);
		}
	}

	public int IndentationSize
	{
		get
		{
			return properties.Get("IndentationSize", 4);
		}
		set
		{
			properties.Set("IndentationSize", value);
		}
	}

	public IndentStyle IndentStyle
	{
		get
		{
			return properties.Get("IndentStyle", IndentStyle.Smart);
		}
		set
		{
			properties.Set("IndentStyle", value);
		}
	}

	public DocumentSelectionMode DocumentSelectionMode
	{
		get
		{
			return properties.Get("DocumentSelectionMode", DocumentSelectionMode.Normal);
		}
		set
		{
			properties.Set("DocumentSelectionMode", value);
		}
	}

	public bool ShowQuickClassBrowserPanel
	{
		get
		{
			return properties.Get("ShowQuickClassBrowserPanel", defaultValue: true);
		}
		set
		{
			properties.Set("ShowQuickClassBrowserPanel", value);
		}
	}

	public bool AllowCaretBeyondEOL
	{
		get
		{
			return properties.Get("CursorBehindEOL", defaultValue: false);
		}
		set
		{
			properties.Set("CursorBehindEOL", value);
		}
	}

	public bool UnderlineErrors
	{
		get
		{
			return properties.Get("ShowErrors", defaultValue: true);
		}
		set
		{
			properties.Set("ShowErrors", value);
		}
	}

	public bool ShowMatchingBracket
	{
		get
		{
			return properties.Get("ShowBracketHighlight", defaultValue: true);
		}
		set
		{
			properties.Set("ShowBracketHighlight", value);
		}
	}

	public bool ShowLineNumbers
	{
		get
		{
			return properties.Get("ShowLineNumbers", defaultValue: true);
		}
		set
		{
			properties.Set("ShowLineNumbers", value);
		}
	}

	public bool ShowSpaces
	{
		get
		{
			return properties.Get("ShowSpaces", defaultValue: false);
		}
		set
		{
			properties.Set("ShowSpaces", value);
		}
	}

	public bool ShowTabs
	{
		get
		{
			return properties.Get("ShowTabs", defaultValue: false);
		}
		set
		{
			properties.Get("ShowTabs", value);
		}
	}

	public bool ShowEOLMarker
	{
		get
		{
			return properties.Get("ShowEOLMarkers", defaultValue: false);
		}
		set
		{
			properties.Set("ShowEOLMarkers", value);
		}
	}

	public bool ShowInvalidLines
	{
		get
		{
			return properties.Get("ShowInvalidLines", defaultValue: false);
		}
		set
		{
			properties.Set("ShowInvalidLines", value);
		}
	}

	public bool IsIconBarVisible
	{
		get
		{
			return properties.Get("IconBarVisible", defaultValue: true);
		}
		set
		{
			properties.Set("IconBarVisible", value);
		}
	}

	public bool EnableFolding
	{
		get
		{
			return properties.Get("EnableFolding", defaultValue: true);
		}
		set
		{
			properties.Set("EnableFolding", value);
		}
	}

	public bool ShowHorizontalRuler
	{
		get
		{
			return properties.Get("ShowHRuler", defaultValue: false);
		}
		set
		{
			properties.Set("ShowHRuler", value);
		}
	}

	public bool ShowVerticalRuler
	{
		get
		{
			return properties.Get("ShowVRuler", defaultValue: false);
		}
		set
		{
			properties.Set("ShowVRuler", value);
		}
	}

	public bool ConvertTabsToSpaces
	{
		get
		{
			return properties.Get("TabsToSpaces", defaultValue: false);
		}
		set
		{
			properties.Set("TabsToSpaces", value);
		}
	}

	public bool MouseWheelScrollDown
	{
		get
		{
			return properties.Get("MouseWheelScrollDown", defaultValue: true);
		}
		set
		{
			properties.Set("MouseWheelScrollDown", value);
		}
	}

	public bool MouseWheelTextZoom
	{
		get
		{
			return properties.Get("MouseWheelTextZoom", defaultValue: true);
		}
		set
		{
			properties.Set("MouseWheelTextZoom", value);
		}
	}

	public bool HideMouseCursor
	{
		get
		{
			return properties.Get("HideMouseCursor", defaultValue: false);
		}
		set
		{
			properties.Set("HideMouseCursor", value);
		}
	}

	public bool CutCopyWholeLine
	{
		get
		{
			return properties.Get("CutCopyWholeLine", defaultValue: true);
		}
		set
		{
			properties.Set("CutCopyWholeLine", value);
		}
	}

	public Encoding Encoding
	{
		get
		{
			try
			{
				return Encoding.GetEncoding(EncodingCodePage);
			}
			catch
			{
				EncodingCodePage = 0;
				return Encoding.GetEncoding(0);
			}
		}
		set
		{
			EncodingCodePage = value.CodePage;
		}
	}

	public int EncodingCodePage
	{
		get
		{
			return properties.Get("Encoding", 0);
		}
		set
		{
			properties.Set("Encoding", value);
		}
	}

	public int VerticalRulerRow
	{
		get
		{
			return properties.Get("VRulerRow", 80);
		}
		set
		{
			properties.Set("VRulerRow", value);
		}
	}

	public LineViewerStyle LineViewerStyle
	{
		get
		{
			return properties.Get("LineViewerStyle", LineViewerStyle.None);
		}
		set
		{
			properties.Set("LineViewerStyle", value);
		}
	}

	public string LineTerminator
	{
		get
		{
			return PropertyService.Get("SharpDevelop.LineTerminatorStyle", LineTerminatorStyle.Windows) switch
			{
				LineTerminatorStyle.Windows => "\r\n", 
				LineTerminatorStyle.Macintosh => "\r", 
				_ => "\n", 
			};
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool AutoInsertCurlyBracket
	{
		get
		{
			return properties.Get("AutoInsertCurlyBracket", defaultValue: true);
		}
		set
		{
			properties.Set("AutoInsertCurlyBracket", value);
		}
	}

	public bool AutoInsertTemplates
	{
		get
		{
			return properties.Get("AutoInsertTemplates", defaultValue: true);
		}
		set
		{
			properties.Set("AutoInsertTemplates", value);
		}
	}

	public Font Font
	{
		get
		{
			return fontContainer.DefaultFont;
		}
		set
		{
			properties.Set("DefaultFont", value.ToString());
			fontContainer.DefaultFont = value;
		}
	}

	public FontContainer FontContainer => fontContainer;

	public BracketMatchingStyle BracketMatchingStyle
	{
		get
		{
			return properties.Get("BracketMatchingStyle", BracketMatchingStyle.After);
		}
		set
		{
			properties.Set("BracketMatchingStyle", value);
		}
	}

	public bool UseCustomLine
	{
		get
		{
			return useCustomLine;
		}
		set
		{
			useCustomLine = value;
		}
	}

	public TextRenderingHint TextRenderingHint
	{
		get
		{
			return properties.Get("TextRenderingHint", TextRenderingHint.SystemDefault);
		}
		set
		{
			LoggingService.Debug("Setting TextRenderingHint to " + value);
			properties.Set("TextRenderingHint", value);
		}
	}

	public bool CircularSearch
	{
		get
		{
			return PropertyService.Get("CircularSearch", true, "SearchAndReplaceProperties");
		}
		set
		{
			LoggingService.Debug("Setting CircularSearch to " + value);
			PropertyService.Set("CircularSearch", value, "SearchAndReplaceProperties");
		}
	}

	private SharpDevelopTextEditorProperties()
	{
		properties = PropertyService.Get("TextEditorSettings", new Properties());
		fontContainer = new FontContainer(FontService.GetFont(FontService.FontType.TextEditor));
		properties.PropertyChanged += CheckFontChange;
	}

	private void CheckFontChange(object sender, PropertyChangedEventArgs e)
	{
		if (e.Key == "DefaultFont")
		{
			fontContainer.DefaultFont = FontContainer.ParseFont(e.NewValue.ToString());
		}
	}
}

using System;
using System.ComponentModel;
using System.Text;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using VisualHint.SmartPropertyGrid;

namespace SoftVelocity.Common;

[DefaultProperty("PreferredColumn")]
public class SmartFormatterOptions : IDisposable
{
	private const string GeneralCategory = "General";

	private const string CodeStatementsCategory = "Code Statements";

	public const int DontIndentCommentsOnFirstColPropertyId = 100;

	public const int FormatBlockAfterEndOnlyInCodePropertyId = 101;

	public static string AutoformatterProperty = "ClarionEditor.Autoformatter";

	private static SmartFormatterOptions defaultOptions;

	private string indentString = "\t";

	private int indentSize = 4;

	private int preferredColumn = 21;

	private bool enableEnteredLineFormatting = true;

	private bool indentStatementsFromCODE = true;

	private bool treatStatementEndsWithColonAsLabel;

	private bool indentComments;

	private bool formatTextAfterPastingSeveralLines;

	private bool dontIndentCommentsOnFirstCol = true;

	private bool setProgramToPreferredCol = true;

	private bool setNamespaceToPreferredCol = true;

	private bool setUsingToPreferredCol;

	private bool setMapToPreferredCol = true;

	private bool setPragmaToPreferredCol;

	private bool indentOfFromCase;

	private int continuousLineIndentMultiplier = 1;

	private bool formatBlockAfterEnd;

	private bool formatBlockAfterEndOnlyInCode = true;

	private bool savingProperties;

	[Browsable(false)]
	public static SmartFormatterOptions General
	{
		get
		{
			if (defaultOptions == null)
			{
				defaultOptions = new SmartFormatterOptions();
				defaultOptions.Initialize();
			}
			return defaultOptions;
		}
	}

	[Browsable(false)]
	public string IndentString => indentString;

	[Browsable(false)]
	public int IndentSize => indentSize;

	[SortedCategory("General", 0)]
	[DefaultValue(21)]
	[DisplayName("Preferred column")]
	[PropertyValidator(typeof(PropertyValidatorMinMax), new object[] { 0, 80 })]
	[SortedProperty(0)]
	[Description("If the keyword should not be indented relative to the parent it will be placed to the preferred column. The real value may be different as keyword indentation will contain an integer number of tabs. Values range is 0 - 80.")]
	public int PreferredColumn
	{
		get
		{
			return preferredColumn;
		}
		set
		{
			preferredColumn = value;
			ValidatePreferredColumn();
		}
	}

	[SortedCategory("General", 0)]
	[DefaultValue(1)]
	[SortedProperty(1)]
	[DisplayName("Continuous line indent multiplier")]
	[Description("Indicates how the next line will be indented relatively to the current if current line contains line continuation character ( | ). Values range is 0 - 10.")]
	[PropertyValidator(typeof(PropertyValidatorMinMax), new object[] { 0, 10 })]
	public int ContinuousLineIndentMultiplier
	{
		get
		{
			return continuousLineIndentMultiplier;
		}
		set
		{
			continuousLineIndentMultiplier = value;
			ValidateContinuousLineIndentMultiplier();
		}
	}

	[DisplayName("Enable entered line formatting")]
	[SortedCategory("General", 0)]
	[SortedProperty(2)]
	[DefaultValue(true)]
	[Description("Indents a line after the Enter key is pressed at the end of the line.")]
	[PropertyValueDisplayedAs(new string[] { "", "" })]
	public bool EnableEnteredLineFormatting
	{
		get
		{
			return enableEnteredLineFormatting;
		}
		set
		{
			enableEnteredLineFormatting = value;
		}
	}

	[DefaultValue(false)]
	[SortedCategory("General", 0)]
	[SortedProperty(3)]
	[DisplayName("Format text after pasting several lines")]
	[Description("Auto format (e.g., indent according to your settings) pasted text if several lines are pasted.")]
	[PropertyValueDisplayedAs(new string[] { "", "" })]
	public bool FormatTextAfterPastingSeveralLines
	{
		get
		{
			return formatTextAfterPastingSeveralLines;
		}
		set
		{
			formatTextAfterPastingSeveralLines = value;
		}
	}

	[SortedCategory("General", 0)]
	[DisplayName("Indent comments")]
	[Description("Indicates that any commented text will be treated as other non-commented text based on the current settings.")]
	[PropertyValueDisplayedAs(new string[] { "", "" })]
	[SortedProperty(4)]
	[DefaultValue(false)]
	public bool IndentComments
	{
		get
		{
			return indentComments;
		}
		set
		{
			indentComments = value;
		}
	}

	[PropertyValueDisplayedAs(new string[] { "", "" })]
	[DisplayName("Don't indent comments started on first column")]
	[PropertyId(100)]
	[SortedCategory("General", 0)]
	[DefaultValue(true)]
	[SortedProperty(5)]
	[Description("Indicates that any single line comment that started on first column will not be indented even if IndentComments is enabled.")]
	public bool DontIndentCommentsOnFirstCol
	{
		get
		{
			return dontIndentCommentsOnFirstCol;
		}
		set
		{
			dontIndentCommentsOnFirstCol = value;
		}
	}

	[SortedProperty(0)]
	[DisplayName("Indent statements from CODE")]
	[Description("Allows indentation to be positioned relative to the location of the CODE statement.")]
	[PropertyValueDisplayedAs(new string[] { "", "" })]
	[DefaultValue(true)]
	[SortedCategory("Code Statements", 1)]
	public bool IndentStatementsFromCODE
	{
		get
		{
			return indentStatementsFromCODE;
		}
		set
		{
			indentStatementsFromCODE = value;
		}
	}

	[SortedCategory("Code Statements", 1)]
	[SortedProperty(1)]
	[Description("Controls how OF, OROF and ELSE statements will be indented inside CASE.")]
	[PropertyValueDisplayedAs(new string[] { "", "" })]
	[DefaultValue(false)]
	[DisplayName("Indent OF, OROF and ELSE statements from CASE")]
	public bool IndentOfFromCase
	{
		get
		{
			return indentOfFromCase;
		}
		set
		{
			indentOfFromCase = value;
		}
	}

	[PropertyValueDisplayedAs(new string[] { "", "" })]
	[SortedProperty(2)]
	[Description("This option tells the editor to treat expressions that ends with a colon ( : ) as a statement label in the CODE section.")]
	[SortedCategory("Code Statements", 1)]
	[DefaultValue(false)]
	[DisplayName("Treat statement ends with colon as label")]
	public bool TreatStatementEndsWithColonAsLabel
	{
		get
		{
			return treatStatementEndsWithColonAsLabel;
		}
		set
		{
			treatStatementEndsWithColonAsLabel = value;
		}
	}

	[SortedProperty(3)]
	[DisplayName("Format block of code after END")]
	[Description("Auto format (e.g., indent according to your settings) a block of code when enter is pressed after END keyword.")]
	[PropertyValueDisplayedAs(new string[] { "", "" })]
	[DefaultValue(false)]
	[SortedCategory("Code Statements", 1)]
	public bool FormatBlockAfterEnd
	{
		get
		{
			return formatBlockAfterEnd;
		}
		set
		{
			formatBlockAfterEnd = value;
		}
	}

	[PropertyValueDisplayedAs(new string[] { "", "" })]
	[PropertyId(101)]
	[DisplayName("Format block of code after END only in CODE section")]
	[Description("Auto format (e.g., indent according to your settings) a block of code inside CODE section when enter is pressed after END keyword.")]
	[SortedCategory("Code Statements", 1)]
	[DefaultValue(true)]
	[SortedProperty(4)]
	public bool FormatBlockAfterEndOnlyInCode
	{
		get
		{
			return formatBlockAfterEndOnlyInCode;
		}
		set
		{
			formatBlockAfterEndOnlyInCode = value;
		}
	}

	[DisplayName("Indent PROGRAM/MEMBER statements to")]
	[Description("Controls how PROGRAM and MEMBER statements will be indented.")]
	[PropertyValueDisplayedAs(new string[] { "Preferred column", "One indent from the left" })]
	[PropertyLook(typeof(PropertyRadioButtonLook))]
	[PropertyFeel("radiobutton")]
	[DefaultValue(true)]
	[SortedProperty(5)]
	[SortedCategory("Code Statements", 1)]
	public bool SetProgramToPreferredCol
	{
		get
		{
			return setProgramToPreferredCol;
		}
		set
		{
			setProgramToPreferredCol = value;
		}
	}

	[DefaultValue(true)]
	[Description("Controls how NAMESPACE statement will be indented.")]
	[ClaNetOnly]
	[SortedProperty(6)]
	[DisplayName("Indent NAMESPACE statement to")]
	[PropertyValueDisplayedAs(new string[] { "Preferred column", "One indent from the left" })]
	[PropertyLook(typeof(PropertyRadioButtonLook))]
	[PropertyFeel("radiobutton")]
	[SortedCategory("Code Statements", 1)]
	public bool SetNamespaceToPreferredCol
	{
		get
		{
			return setNamespaceToPreferredCol;
		}
		set
		{
			setNamespaceToPreferredCol = value;
		}
	}

	[PropertyLook(typeof(PropertyRadioButtonLook))]
	[ClaNetOnly]
	[PropertyValueDisplayedAs(new string[] { "Preferred column", "One indent from the left" })]
	[PropertyFeel("radiobutton")]
	[DefaultValue(false)]
	[Description("Controls how USING statement will be indented.")]
	[SortedCategory("Code Statements", 1)]
	[SortedProperty(7)]
	[DisplayName("Indent USING statement to")]
	public bool SetUsingToPreferredCol
	{
		get
		{
			return setUsingToPreferredCol;
		}
		set
		{
			setUsingToPreferredCol = value;
		}
	}

	[DefaultValue(true)]
	[PropertyFeel("radiobutton")]
	[SortedCategory("Code Statements", 1)]
	[SortedProperty(8)]
	[DisplayName("Indent MAP statement to")]
	[Description("Controls how MAP statement will be indented.")]
	[PropertyValueDisplayedAs(new string[] { "Preferred column", "One indent from the left" })]
	[PropertyLook(typeof(PropertyRadioButtonLook))]
	public bool SetMapToPreferredCol
	{
		get
		{
			return setMapToPreferredCol;
		}
		set
		{
			setMapToPreferredCol = value;
		}
	}

	[SortedCategory("Code Statements", 1)]
	[Description("Controls how PRAGMA and SECTION statements will be indented.")]
	[PropertyLook(typeof(PropertyRadioButtonLook))]
	[PropertyValueDisplayedAs(new string[] { "Preferred column", "One indent from the left" })]
	[DisplayName("Indent PRAGMA and SECTION statements to")]
	[DefaultValue(false)]
	[PropertyFeel("radiobutton")]
	[SortedProperty(9)]
	public bool SetPragmaToPreferredCol
	{
		get
		{
			return setPragmaToPreferredCol;
		}
		set
		{
			setPragmaToPreferredCol = value;
		}
	}

	public void Initialize()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		Properties val = PropertyService.Get<Properties>("ICSharpCode.TextEditor.Document.Document.DefaultDocumentAggregatorProperties", new Properties());
		val.PropertyChanged += new PropertyChangedEventHandler(TextEditorPropertyChanged);
		SetTextEditorOptions();
		PropertyService.PropertyChanged += new PropertyChangedEventHandler(FormatterPropertyChanged);
		val = PropertyService.Get<Properties>(AutoformatterProperty, new Properties());
		SetFormatterOptions(val);
	}

	public void SaveProperties()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		savingProperties = true;
		Properties val = PropertyService.Get<Properties>(AutoformatterProperty, new Properties());
		val.Set<int>("PreferredColumn", preferredColumn);
		val.Set<bool>("EnableEnteredLineFormatting", enableEnteredLineFormatting);
		val.Set<bool>("IndentStatementsFromCODE", indentStatementsFromCODE);
		val.Set<bool>("TreatStatementEndsWithColonAsLabel", treatStatementEndsWithColonAsLabel);
		val.Set<bool>("IndentComments", indentComments);
		val.Set<bool>("FormatTextAfterPastingSeveralLines", formatTextAfterPastingSeveralLines);
		val.Set<bool>("DontIndentCommentsOnFirstCol", dontIndentCommentsOnFirstCol);
		val.Set<bool>("SetProgramToPreferredCol", setProgramToPreferredCol);
		val.Set<bool>("SetNamespaceToPreferredCol", setNamespaceToPreferredCol);
		val.Set<bool>("SetUsingToPreferredCol", setUsingToPreferredCol);
		val.Set<bool>("SetMapToPreferredCol", setMapToPreferredCol);
		val.Set<bool>("SetPragmaToPreferredCol", setPragmaToPreferredCol);
		val.Set<bool>("IndentOfFromCase", indentOfFromCase);
		val.Set<bool>("FormatBlockAfterEnd", formatBlockAfterEnd);
		val.Set<bool>("FormatBlockAfterEndOnlyInCode", formatBlockAfterEndOnlyInCode);
		val.Set<int>("ContinuousLineIndentMultiplier", continuousLineIndentMultiplier);
		PropertyService.Set<Properties>(AutoformatterProperty, val);
		savingProperties = false;
	}

	private void TextEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.Key == "TabIndent" || e.Key == "TabsToSpaces")
		{
			SetTextEditorOptions();
		}
	}

	private void FormatterPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		if (e.Key == AutoformatterProperty && !savingProperties)
		{
			Properties formatterOptions = (Properties)e.NewValue;
			SetFormatterOptions(formatterOptions);
		}
	}

	private void SetTextEditorOptions()
	{
		indentString = GetIndentationString();
		indentSize = SharpDevelopTextEditorProperties.Instance.TabIndent;
	}

	private void SetFormatterOptions(Properties properties)
	{
		preferredColumn = properties.Get<int>("PreferredColumn", 21);
		ValidatePreferredColumn();
		enableEnteredLineFormatting = properties.Get<bool>("EnableEnteredLineFormatting", true);
		indentStatementsFromCODE = properties.Get<bool>("IndentStatementsFromCODE", true);
		treatStatementEndsWithColonAsLabel = properties.Get<bool>("TreatStatementEndsWithColonAsLabel", false);
		indentComments = properties.Get<bool>("IndentComments", false);
		formatTextAfterPastingSeveralLines = properties.Get<bool>("FormatTextAfterPastingSeveralLines", false);
		dontIndentCommentsOnFirstCol = properties.Get<bool>("DontIndentCommentsOnFirstCol", true);
		setProgramToPreferredCol = properties.Get<bool>("SetProgramToPreferredCol", true);
		setNamespaceToPreferredCol = properties.Get<bool>("SetNamespaceToPreferredCol", true);
		setUsingToPreferredCol = properties.Get<bool>("SetUsingToPreferredCol", false);
		setMapToPreferredCol = properties.Get<bool>("SetMapToPreferredCol", true);
		setPragmaToPreferredCol = properties.Get<bool>("SetPragmaToPreferredCol", false);
		indentOfFromCase = properties.Get<bool>("IndentOfFromCase", false);
		formatBlockAfterEnd = properties.Get<bool>("FormatBlockAfterEnd", false);
		formatBlockAfterEndOnlyInCode = properties.Get<bool>("FormatBlockAfterEndOnlyInCode", true);
		continuousLineIndentMultiplier = properties.Get<int>("ContinuousLineIndentMultiplier", 1);
	}

	private static string GetIndentationString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (SharpDevelopTextEditorProperties.Instance.ConvertTabsToSpaces)
		{
			int tabIndent = SharpDevelopTextEditorProperties.Instance.TabIndent;
			stringBuilder.Append(new string(' ', tabIndent));
		}
		else
		{
			stringBuilder.Append('\t');
		}
		return stringBuilder.ToString();
	}

	public void Dispose()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		Properties val = PropertyService.Get<Properties>("ICSharpCode.TextEditor.Document.Document.DefaultDocumentAggregatorProperties", new Properties());
		val.PropertyChanged -= new PropertyChangedEventHandler(TextEditorPropertyChanged);
		val = PropertyService.Get<Properties>(AutoformatterProperty, new Properties());
		val.PropertyChanged -= new PropertyChangedEventHandler(FormatterPropertyChanged);
	}

	private void ValidatePreferredColumn()
	{
		if (preferredColumn < 0)
		{
			preferredColumn = 0;
		}
		else if (preferredColumn > 80)
		{
			preferredColumn = 80;
		}
	}

	private void ValidateContinuousLineIndentMultiplier()
	{
		if (continuousLineIndentMultiplier < 0)
		{
			continuousLineIndentMultiplier = 0;
		}
		else if (continuousLineIndentMultiplier > 10)
		{
			continuousLineIndentMultiplier = 10;
		}
	}
}

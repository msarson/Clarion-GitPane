using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common;
using SoftVelocity.Common.ClarionEditor;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.Parser.Ast;

namespace SoftVelocity.Generator;

internal class ClarionTemplateEditorDisplayBindingWrapper : TextEditorDisplayBindingWrapper, IClipboardHandler, IHasClarionContextHelpSupport, IStructureDesignerCompatible
{
	private Timer timerTextChanged;

	private ComboBox functionsComboBox;

	private ComboBox functionTypesComboBox;

	private Panel panelComboBox;

	private Panel panel;

	private ClarionTemplateFolding _ClaFolding;

	private Font font = new Font("Arial", 8.25f);

	private StringFormat drawStringFormat = new StringFormat(StringFormatFlags.NoWrap);

	private bool refreshRequired;

	private bool lastChanged;

	private DateTime lastChangedTime = DateTime.Now;

	private DateTime startingChangedTime = DateTime.Now;

	private TimeSpan timeToWait = new TimeSpan(1000000L);

	private bool updatingDropContent;

	private bool updatingFoldings;

	private bool caretPositionChanging;

	private static Regex expWindow = new Regex("^\\??[A-Za-z_](\\w|:|\\.)*( |\\t)+WINDOW", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	private static Regex expReport = new Regex("^\\??[A-Za-z_](\\w|:|\\.)*( |\\t)+REPORT", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	public override Control Control => panel;

	public override IWorkbenchWindow WorkbenchWindow
	{
		get
		{
			return ((AbstractBaseViewContent)this).WorkbenchWindow;
		}
		set
		{
			if (((AbstractBaseViewContent)this).WorkbenchWindow != null)
			{
				((AbstractBaseViewContent)this).WorkbenchWindow.WindowSelected -= WorkBenchWindowSelected;
			}
			((AbstractBaseViewContent)this).WorkbenchWindow = value;
			if (((AbstractBaseViewContent)this).WorkbenchWindow != null)
			{
				((AbstractBaseViewContent)this).WorkbenchWindow.WindowSelected += WorkBenchWindowSelected;
			}
		}
	}

	private ClarionTemplateFolding ClaFolding
	{
		get
		{
			if (_ClaFolding == null && ((TextEditorControlBase)base.textAreaControl).Document.FoldingManager.FoldingStrategy != null && ((TextEditorControlBase)base.textAreaControl).Document.FoldingManager.FoldingStrategy is ClarionTemplateFolding)
			{
				_ClaFolding = (ClarionTemplateFolding)(object)((TextEditorControlBase)base.textAreaControl).Document.FoldingManager.FoldingStrategy;
			}
			return _ClaFolding;
		}
	}

	public string HelpText
	{
		get
		{
			string text = string.Empty;
			TextAreaControl activeTextAreaControl = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl;
			LineSegment lineSegment = activeTextAreaControl.Document.GetLineSegment(activeTextAreaControl.Caret.Line);
			TextWord word = lineSegment.GetWord(activeTextAreaControl.Caret.Column);
			if (word != null)
			{
				text = word.Word;
				int offset = word.Offset;
				if (offset > 0)
				{
					TextWord word2 = lineSegment.GetWord(offset - 1);
					if (word2 != null && (word2.Word == "#" || word2.Word == "%"))
					{
						text = word2.Word + text;
					}
				}
			}
			else if (functionTypesComboBox.SelectedIndex > -1)
			{
				text = "#" + functionTypesComboBox.Items[functionTypesComboBox.SelectedIndex];
			}
			return text;
		}
	}

	public bool HelpTextIsKeyword => true;

	public bool CanShowStructureDesigner
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			TextLocation position = ((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.Caret.Position;
			LineSegment lineSegment = ((TextEditorControlBase)base.textAreaControl).Document.GetLineSegment(((TextLocation)(ref position)).Line);
			string text = ((TextEditorControlBase)base.textAreaControl).Document.GetText((ISegment)(object)lineSegment);
			if (text.Trim() == string.Empty)
			{
				return true;
			}
			if (expWindow.IsMatch(text) || expReport.IsMatch(text))
			{
				return true;
			}
			return false;
		}
	}

	public bool IsWin => true;

	public ClarionTemplateEditorDisplayBindingWrapper()
	{
		InitializeComponents();
		((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.Caret.PositionChanged += Caret_PositionChanged;
		((TextEditorControlBase)base.textAreaControl).TextChanged += textAreaControl_TextChanged;
		timerTextChanged.Enabled = true;
		timerTextChanged.Start();
		timerTextChanged.Tick += timerTextChanged_Tick;
		panel = new Panel();
		((TextEditorDisplayBindingWrapper)this).Control.Dock = DockStyle.Fill;
		panel.Controls.Add(((TextEditorDisplayBindingWrapper)this).Control);
		panel.Controls.Add(panelComboBox);
		panel.GotFocus += panel_GotFocus;
	}

	private void panel_GotFocus(object sender, EventArgs e)
	{
		((TextEditorDisplayBindingWrapper)this).Control.Focus();
	}

	private void InitializeComponents()
	{
		panelComboBox = new Panel();
		functionsComboBox = new ComboBox();
		functionTypesComboBox = new ComboBox();
		timerTextChanged = new Timer();
		panelComboBox.SuspendLayout();
		functionsComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		functionsComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
		functionsComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
		functionsComboBox.DrawMode = DrawMode.OwnerDrawVariable;
		functionsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
		functionsComboBox.Location = new Point(162, 11);
		functionsComboBox.Name = "functionsComboBox";
		functionsComboBox.Size = new Size(295, 23);
		functionsComboBox.TabIndex = 1;
		functionsComboBox.DrawItem += functionsComboBox_DrawItem;
		functionsComboBox.SelectedIndexChanged += functionsComboBox_SelectedIndexChanged;
		functionsComboBox.DropDownClosed += functionsComboBox_DropDownClosed;
		functionTypesComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
		functionTypesComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
		functionTypesComboBox.DrawMode = DrawMode.OwnerDrawVariable;
		functionTypesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
		functionTypesComboBox.Location = new Point(9, 11);
		functionTypesComboBox.Name = "functionTypesComboBox";
		functionTypesComboBox.Size = new Size(147, 23);
		functionTypesComboBox.TabIndex = 2;
		functionTypesComboBox.DrawItem += functionTypesComboBox_DrawItem;
		functionTypesComboBox.SelectedIndexChanged += functionTypesComboBox_SelectedIndexChanged;
		functionTypesComboBox.DropDownClosed += functionTypesComboBox_DropDownClosed;
		panelComboBox.Location = new Point(0, 0);
		panelComboBox.Size = new Size(465, 42);
		panelComboBox.Name = "panelComboBox";
		panelComboBox.Controls.Add(functionTypesComboBox);
		panelComboBox.Controls.Add(functionsComboBox);
		panelComboBox.Dock = DockStyle.Top;
		panelComboBox.TabIndex = 3;
		panelComboBox.ResumeLayout();
	}

	public override void Dispose()
	{
		if (((AbstractBaseViewContent)this).WorkbenchWindow != null)
		{
			((AbstractBaseViewContent)this).WorkbenchWindow.WindowSelected -= WorkBenchWindowSelected;
		}
		if (panel != null)
		{
			panel.Controls.Clear();
			panel.Dispose();
		}
		((TextEditorControlBase)base.textAreaControl).TextChanged -= textAreaControl_TextChanged;
		timerTextChanged.Dispose();
		timerTextChanged = null;
		((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.Caret.PositionChanged -= Caret_PositionChanged;
		if (((TextEditorControlBase)base.textAreaControl).Document.FoldingManager.FoldingStrategy is ClarionTemplateFolding)
		{
			((ClarionTemplateFolding)(object)((TextEditorControlBase)base.textAreaControl).Document.FoldingManager.FoldingStrategy).Dispose();
		}
		((TextEditorControlBase)base.textAreaControl).Document.FoldingManager.FoldingStrategy = null;
		((TextEditorDisplayBindingWrapper)this).Dispose();
	}

	private void WorkBenchWindowSelected(object sender, EventArgs e)
	{
		foreach (ISecondaryViewContent secondaryViewContent in ((AbstractViewContent)this).SecondaryViewContents)
		{
			if (secondaryViewContent is CommonClarionDesignerView)
			{
				((CommonClarionDesignerView)(object)secondaryViewContent).ForceDesignerIndentation = true;
			}
		}
	}

	private void functionTypesComboBox_DrawItem(object sender, DrawItemEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		e.DrawBackground();
		if (e.Index >= 0)
		{
			string s = (string)comboBox.Items[e.Index];
			Rectangle rectangle = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
			Brush brush = SystemBrushes.WindowText;
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				brush = SystemBrushes.HighlightText;
			}
			e.Graphics.DrawString(s, font, brush, rectangle, drawStringFormat);
		}
		e.DrawFocusRectangle();
	}

	private void functionsComboBox_DrawItem(object sender, DrawItemEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		e.DrawBackground();
		if (e.Index >= 0)
		{
			ClarionTemplateParsedFunction clarionTemplateParsedFunction = (ClarionTemplateParsedFunction)comboBox.Items[e.Index];
			Rectangle rectangle = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
			Brush brush = SystemBrushes.WindowText;
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				brush = SystemBrushes.HighlightText;
			}
			e.Graphics.DrawString(clarionTemplateParsedFunction.ToString(), font, brush, rectangle, drawStringFormat);
		}
		e.DrawFocusRectangle();
	}

	private void functionsComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!functionsComboBox.DroppedDown)
		{
			if (!updatingDropContent && !caretPositionChanging && functionsComboBox.SelectedIndex > -1)
			{
				ClarionTemplateParsedFunction clarionTemplateParsedFunction = functionsComboBox.Items[functionsComboBox.SelectedIndex] as ClarionTemplateParsedFunction;
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.JumpTo(clarionTemplateParsedFunction.LineNumber, 0);
			}
		}
		else
		{
			refreshRequired = true;
		}
	}

	private void functionTypesComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (updatingDropContent || functionTypesComboBox.SelectedIndex <= -1)
		{
			return;
		}
		string functionType = functionTypesComboBox.Items[functionTypesComboBox.SelectedIndex] as string;
		updateFunctionListDropContent(functionType);
		if (!functionTypesComboBox.DroppedDown)
		{
			if (!caretPositionChanging)
			{
				ClarionTemplateParsedFunction clarionTemplateParsedFunction = functionsComboBox.Items[functionsComboBox.SelectedIndex] as ClarionTemplateParsedFunction;
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.JumpTo(clarionTemplateParsedFunction.LineNumber, 0);
			}
		}
		else
		{
			refreshRequired = true;
		}
	}

	private void timerTextChanged_Tick(object sender, EventArgs e)
	{
		if (lastChanged && !updatingFoldings && DateTime.Now.TimeOfDay - lastChangedTime.TimeOfDay > timeToWait)
		{
			ForceFoldingRefresh();
		}
	}

	public void ForceFoldingRefresh()
	{
		if (!updatingFoldings)
		{
			threadedUpdate();
		}
	}

	private void threadedUpdate()
	{
		updatingFoldings = true;
		((TextEditorControlBase)base.textAreaControl).Document.FoldingManager.UpdateFoldings((string)null, (object)null);
		lastChanged = false;
		updateFunctionTypesListDropContent();
		updatingFoldings = false;
	}

	private void textAreaControl_TextChanged(object sender, EventArgs e)
	{
		lastChanged = true;
		lastChangedTime = DateTime.Now;
	}

	private void Caret_PositionChanged(object sender, EventArgs e)
	{
		caretPositionChanging = true;
		int functionTypeIndex = -1;
		int functionIndex = -1;
		ClaFolding.GetIndexInListOfFunctions(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.Caret.Line, out functionTypeIndex, out functionIndex);
		if (functionTypesComboBox.Items.Count > 0)
		{
			functionTypesComboBox.SelectedIndex = functionTypeIndex;
			if (functionTypesComboBox.SelectedIndex > -1)
			{
				updatingDropContent = true;
				if (functionsComboBox.Items.Count > 0 && functionIndex < functionsComboBox.Items.Count)
				{
					functionsComboBox.SelectedIndex = functionIndex;
				}
				else
				{
					updateFunctionListDropContent(functionTypesComboBox.Items[functionTypesComboBox.SelectedIndex] as string);
					if (functionsComboBox.Items.Count > 0 && functionIndex < functionsComboBox.Items.Count)
					{
						functionsComboBox.SelectedIndex = functionIndex;
					}
				}
			}
		}
		updatingDropContent = false;
		caretPositionChanging = false;
	}

	private void updateFunctionTypesListDropContent()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (((AbstractBaseViewContent)this).Control.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(new Action(updateFunctionTypesListDropContent));
			return;
		}
		updatingDropContent = true;
		if (ClaFolding.ListOfFunctions().Count == 0)
		{
			functionTypesComboBox.Items.Clear();
			updateFunctionListDropContent(null);
			updatingDropContent = false;
			return;
		}
		string text = "";
		if (functionTypesComboBox.SelectedIndex > -1)
		{
			text = functionTypesComboBox.Items[functionTypesComboBox.SelectedIndex] as string;
		}
		functionTypesComboBox.Items.Clear();
		int num = -1;
		int num2 = -1;
		ClaFolding.SortListOfFunctions();
		foreach (KeyValuePair<string, List<ClarionTemplateParsedFunction>> item in ClaFolding.ListOfFunctionsSorted())
		{
			num++;
			functionTypesComboBox.Items.Add(item.Key);
			if (item.Key == text)
			{
				num2 = num;
			}
		}
		functionTypesComboBox.SelectedIndex = num2;
		if (num2 > -1)
		{
			updateFunctionListDropContent(functionTypesComboBox.Items[num2] as string);
		}
		else
		{
			updateFunctionListDropContent(null);
		}
		updatingDropContent = false;
	}

	private void updateFunctionListDropContent(string functionType)
	{
		updatingDropContent = true;
		if (ClaFolding.ListOfFunctions().Count == 0 || functionType == null)
		{
			functionsComboBox.Items.Clear();
			updatingDropContent = false;
			return;
		}
		string text = "";
		int num = -1;
		if (functionsComboBox.SelectedIndex >= 0)
		{
			ClarionTemplateParsedFunction clarionTemplateParsedFunction = functionsComboBox.Items[functionsComboBox.SelectedIndex] as ClarionTemplateParsedFunction;
			if (clarionTemplateParsedFunction.FunctionType == functionType)
			{
				text = clarionTemplateParsedFunction.ToString();
				num = clarionTemplateParsedFunction.LineNumber;
			}
		}
		functionsComboBox.Items.Clear();
		int num2 = 0;
		bool flag = false;
		foreach (ClarionTemplateParsedFunction item in ClaFolding.ListOfFunctions(functionType))
		{
			if (item.LineNumber <= num)
			{
				num2 = item.LineNumber;
				continue;
			}
			break;
		}
		ClaFolding.SortListOfFunctions();
		int num3 = 0;
		List<ClarionTemplateParsedFunction> list = ClaFolding.ListOfFunctionsSorted(functionType);
		foreach (ClarionTemplateParsedFunction item2 in list)
		{
			num3++;
			functionsComboBox.Items.Add(item2);
			if (!flag)
			{
				if (item2.ToString() == text)
				{
					flag = true;
					num2 = num3 - 1;
				}
				else if (num2 == item2.LineNumber)
				{
					num2 = num3 - 1;
				}
			}
		}
		functionsComboBox.SelectedIndex = num2;
		updatingDropContent = false;
	}

	private void functionsComboBox_DropDownClosed(object sender, EventArgs e)
	{
		if (refreshRequired)
		{
			refreshRequired = false;
			functionsComboBox_SelectedIndexChanged(null, null);
		}
	}

	private void functionTypesComboBox_DropDownClosed(object sender, EventArgs e)
	{
		if (refreshRequired)
		{
			refreshRequired = false;
			if (!caretPositionChanging)
			{
				ClarionTemplateParsedFunction clarionTemplateParsedFunction = functionsComboBox.Items[functionsComboBox.SelectedIndex] as ClarionTemplateParsedFunction;
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).ActiveTextAreaControl.JumpTo(clarionTemplateParsedFunction.LineNumber, 0);
			}
			functionTypesComboBox_SelectedIndexChanged(null, null);
		}
	}

	public ReportDeclaration ParseStructure(string fileName, string fileContent, int line, int column, out ClarionType structureType, out CompilerResults cr)
	{
		return CommonIDEParser.ParseStructure(fileName, fileContent, line, column, extract: true, IsWin, out structureType, out cr);
	}

	public string GetTemplatesFileName()
	{
		return "DEFAULTS.CLW";
	}

	public virtual string GetContentForDesigner()
	{
		LineSegment lineSegment = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.GetLineSegment(((TextEditorControlBase)base.textAreaControl).ActiveTextAreaControl.Caret.Line);
		return ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.GetText(lineSegment.Offset, ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)this).TextEditorControl).Document.TextLength - lineSegment.Offset);
	}
}

using System.Windows.Forms;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.OptionPanels;

public class CodeCompletionPanel : AbstractOptionPanel
{
	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.CodeCompletionOptionPanel.xfrm"));
		EnableCodeCompletionSettingsGroupBox();
		Get<CheckBox>("codeCompletionEnabled").CheckedChanged += delegate
		{
			EnableCodeCompletionSettingsGroupBox();
		};
		Get<CheckBox>("codeCompletionEnabled").Checked = CodeCompletionOptions.EnableCodeCompletion;
		Get<CheckBox>("useDataUsageCache").CheckedChanged += delegate
		{
			ControlDictionary["dataUsageCacheLabel1"].Enabled = Get<CheckBox>("useDataUsageCache").Checked;
			ControlDictionary["dataUsageCacheLabel2"].Enabled = Get<CheckBox>("useDataUsageCache").Checked;
			ControlDictionary["dataUsageCacheItemCountNumericUpDown"].Enabled = Get<CheckBox>("useDataUsageCache").Checked;
		};
		Get<CheckBox>("useDataUsageCache").Checked = CodeCompletionOptions.DataUsageCacheEnabled;
		Get<NumericUpDown>("dataUsageCacheItemCount").Value = CodeCompletionOptions.DataUsageCacheItemCount;
		ControlDictionary["clearDataUseCacheButton"].Click += delegate
		{
			CodeCompletionDataUsageCache.ResetCache();
		};
		Get<CheckBox>("useTooltips").CheckedChanged += delegate
		{
			ControlDictionary["useDebugTooltipsOnlyCheckBox"].Enabled = Get<CheckBox>("useTooltips").Checked;
		};
		Get<CheckBox>("useTooltips").Checked = CodeCompletionOptions.TooltipsEnabled;
		Get<CheckBox>("useDebugTooltipsOnly").Checked = CodeCompletionOptions.TooltipsOnlyWhenDebugging;
		Get<CheckBox>("completeWhenTyping").Checked = CodeCompletionOptions.CompleteWhenTyping;
		Get<CheckBox>("completeOnInsertionKey").Checked = CodeCompletionOptions.CompleteOnInsertionKey;
		Get<CheckBox>("proceedEnter").Checked = CodeCompletionOptions.NewLineOnEnterAfterFullWord;
		Get<CheckBox>("useKeywordCompletion").Checked = CodeCompletionOptions.KeywordCompletionEnabled;
		Get<CheckBox>("shrinkList").Checked = CodeCompletionOptions.ShrinkListWhenTyping;
		Get<CheckBox>("useInsight").CheckedChanged += delegate
		{
			ControlDictionary["refreshInsightOnCommaCheckBox"].Enabled = Get<CheckBox>("useInsight").Checked;
		};
		Get<CheckBox>("useInsight").Checked = CodeCompletionOptions.InsightEnabled;
		Get<CheckBox>("refreshInsightOnComma").Checked = CodeCompletionOptions.InsightRefreshOnComma;
	}

	public override bool StorePanelContents()
	{
		CodeCompletionOptions.EnableCodeCompletion = Get<CheckBox>("codeCompletionEnabled").Checked;
		CodeCompletionOptions.DataUsageCacheEnabled = Get<CheckBox>("useDataUsageCache").Checked;
		CodeCompletionOptions.DataUsageCacheItemCount = (int)Get<NumericUpDown>("dataUsageCacheItemCount").Value;
		CodeCompletionOptions.TooltipsEnabled = Get<CheckBox>("useTooltips").Checked;
		CodeCompletionOptions.TooltipsOnlyWhenDebugging = Get<CheckBox>("useDebugTooltipsOnly").Checked;
		CodeCompletionOptions.CompleteWhenTyping = Get<CheckBox>("completeWhenTyping").Checked;
		CodeCompletionOptions.CompleteOnInsertionKey = Get<CheckBox>("completeOnInsertionKey").Checked;
		CodeCompletionOptions.NewLineOnEnterAfterFullWord = Get<CheckBox>("proceedEnter").Checked;
		CodeCompletionOptions.ShrinkListWhenTyping = Get<CheckBox>("shrinkList").Checked;
		CodeCompletionOptions.KeywordCompletionEnabled = Get<CheckBox>("useKeywordCompletion").Checked;
		CodeCompletionOptions.InsightEnabled = Get<CheckBox>("useInsight").Checked;
		CodeCompletionOptions.InsightRefreshOnComma = Get<CheckBox>("refreshInsightOnComma").Checked;
		return base.StorePanelContents();
	}

	private void EnableCodeCompletionSettingsGroupBox()
	{
		ControlDictionary["groupBox"].Enabled = Get<CheckBox>("codeCompletionEnabled").Checked;
	}
}

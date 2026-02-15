using System;

namespace ICSharpCode.SharpDevelop;

public interface IWizardPanel : IDialogPanel
{
	string NextWizardPanelID { get; }

	bool IsLastPanel { get; }

	bool EnableNext { get; }

	bool EnablePrevious { get; }

	bool EnableCancel { get; }

	event EventHandler EnableNextChanged;

	event EventHandler NextWizardPanelIDChanged;

	event EventHandler IsLastPanelChanged;

	event EventHandler EnablePreviousChanged;

	event EventHandler EnableCancelChanged;

	event EventHandler FinishPanelRequested;
}

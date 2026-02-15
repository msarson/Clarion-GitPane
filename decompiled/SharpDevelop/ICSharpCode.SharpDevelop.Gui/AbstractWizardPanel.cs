using System;

namespace ICSharpCode.SharpDevelop.Gui;

public abstract class AbstractWizardPanel : AbstractOptionPanel, IWizardPanel, IDialogPanel
{
	private string nextWizardPanelID = string.Empty;

	private bool enablePrevious = true;

	private bool enableNext = true;

	private bool isLastPanel;

	private bool enableCancel = true;

	public string NextWizardPanelID
	{
		get
		{
			return nextWizardPanelID;
		}
		set
		{
			if (nextWizardPanelID != value)
			{
				nextWizardPanelID = value;
				OnNextWizardPanelIDChanged(EventArgs.Empty);
			}
		}
	}

	public bool IsLastPanel
	{
		get
		{
			return isLastPanel;
		}
		set
		{
			if (isLastPanel != value)
			{
				isLastPanel = value;
				OnIsLastPanelChanged(EventArgs.Empty);
			}
		}
	}

	public bool EnableNext
	{
		get
		{
			return enableNext;
		}
		set
		{
			if (enableNext != value)
			{
				enableNext = value;
				OnEnableNextChanged(EventArgs.Empty);
			}
		}
	}

	public bool EnablePrevious
	{
		get
		{
			return enablePrevious;
		}
		set
		{
			if (enablePrevious != value)
			{
				enablePrevious = value;
				OnEnablePreviousChanged(EventArgs.Empty);
			}
		}
	}

	public bool EnableCancel
	{
		get
		{
			return enableCancel;
		}
		set
		{
			if (enableCancel != value)
			{
				enableCancel = value;
				OnEnableCancelChanged(EventArgs.Empty);
			}
		}
	}

	public event EventHandler EnablePreviousChanged;

	public event EventHandler EnableNextChanged;

	public event EventHandler EnableCancelChanged;

	public event EventHandler NextWizardPanelIDChanged;

	public event EventHandler IsLastPanelChanged;

	public event EventHandler FinishPanelRequested;

	public AbstractWizardPanel()
	{
	}

	protected virtual void FinishPanel()
	{
		if (this.FinishPanelRequested != null)
		{
			this.FinishPanelRequested(this, EventArgs.Empty);
		}
	}

	protected virtual void OnEnableNextChanged(EventArgs e)
	{
		if (this.EnableNextChanged != null)
		{
			this.EnableNextChanged(this, e);
		}
	}

	protected virtual void OnEnablePreviousChanged(EventArgs e)
	{
		if (this.EnablePreviousChanged != null)
		{
			this.EnablePreviousChanged(this, e);
		}
	}

	protected virtual void OnEnableCancelChanged(EventArgs e)
	{
		if (this.EnableCancelChanged != null)
		{
			this.EnableCancelChanged(this, e);
		}
	}

	protected virtual void OnNextWizardPanelIDChanged(EventArgs e)
	{
		if (this.NextWizardPanelIDChanged != null)
		{
			this.NextWizardPanelIDChanged(this, e);
		}
	}

	protected virtual void OnIsLastPanelChanged(EventArgs e)
	{
		if (this.IsLastPanelChanged != null)
		{
			this.IsLastPanelChanged(this, e);
		}
	}
}

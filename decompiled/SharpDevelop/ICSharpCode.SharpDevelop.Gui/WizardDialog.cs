using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class WizardDialog : Form
{
	private StatusPanel statusPanel;

	private CurrentPanelPanel curPanel;

	private Panel dialogPanel = new Panel();

	private Stack idStack = new Stack();

	private ArrayList wizardPanels = new ArrayList();

	private int activePanelNumber;

	private EventHandler enableNextChangedHandler;

	private EventHandler enableCancelChangedHandler;

	private EventHandler nextWizardPanelIDChangedHandler;

	private EventHandler finishPanelHandler;

	private Label label1 = new Label();

	private Button backButton = new Button();

	private Button nextButton = new Button();

	private Button finishButton = new Button();

	private Button cancelButton = new Button();

	private Button helpButton = new Button();

	public ArrayList WizardPanels => wizardPanels;

	public int ActivePanelNumber => activePanelNumber;

	public IWizardPanel CurrentWizardPane => (IWizardPanel)((IDialogPanelDescriptor)wizardPanels[activePanelNumber]).DialogPanel;

	private bool CanFinish
	{
		get
		{
			for (int num = 0; num < wizardPanels.Count; num = GetSuccessorNumber(num))
			{
				IDialogPanelDescriptor dialogPanelDescriptor = (IDialogPanelDescriptor)wizardPanels[num];
				if (!dialogPanelDescriptor.DialogPanel.EnableFinish)
				{
					return false;
				}
			}
			return true;
		}
	}

	private int GetPanelNumber(string id)
	{
		for (int i = 0; i < wizardPanels.Count; i++)
		{
			IDialogPanelDescriptor dialogPanelDescriptor = (IDialogPanelDescriptor)wizardPanels[i];
			if (dialogPanelDescriptor.ID == id)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetSuccessorNumber(int curNr)
	{
		IWizardPanel wizardPanel = (IWizardPanel)((IDialogPanelDescriptor)wizardPanels[curNr]).DialogPanel;
		if (wizardPanel.IsLastPanel)
		{
			return wizardPanels.Count + 1;
		}
		int panelNumber = GetPanelNumber(wizardPanel.NextWizardPanelID);
		if (panelNumber < 0)
		{
			return curNr + 1;
		}
		return panelNumber;
	}

	private void CheckFinishedState(object sender, EventArgs e)
	{
		finishButton.Enabled = CanFinish;
	}

	private void AddNodes(object customizer, IEnumerable<IDialogPanelDescriptor> dialogPanelDescriptors)
	{
		foreach (IDialogPanelDescriptor dialogPanelDescriptor in dialogPanelDescriptors)
		{
			if (dialogPanelDescriptor.DialogPanel != null)
			{
				dialogPanelDescriptor.DialogPanel.EnableFinishChanged += CheckFinishedState;
				dialogPanelDescriptor.DialogPanel.CustomizationObject = customizer;
				wizardPanels.Add(dialogPanelDescriptor);
			}
			if (dialogPanelDescriptor.ChildDialogPanelDescriptors != null)
			{
				AddNodes(customizer, dialogPanelDescriptor.ChildDialogPanelDescriptors);
			}
		}
	}

	private void EnableCancelChanged(object sender, EventArgs e)
	{
		cancelButton.Enabled = CurrentWizardPane.EnableCancel;
	}

	private void EnableNextChanged(object sender, EventArgs e)
	{
		nextButton.Enabled = CurrentWizardPane.EnableNext && GetSuccessorNumber(activePanelNumber) < wizardPanels.Count;
		backButton.Enabled = CurrentWizardPane.EnablePrevious && idStack.Count > 0;
	}

	private void NextWizardPanelIDChanged(object sender, EventArgs e)
	{
		EnableNextChanged(null, null);
		finishButton.Enabled = CanFinish;
		statusPanel.Refresh();
	}

	private void ActivatePanel(int number)
	{
		if (CurrentWizardPane != null)
		{
			CurrentWizardPane.EnableNextChanged -= enableNextChangedHandler;
			CurrentWizardPane.EnableCancelChanged -= enableCancelChangedHandler;
			CurrentWizardPane.EnablePreviousChanged -= enableNextChangedHandler;
			CurrentWizardPane.NextWizardPanelIDChanged -= nextWizardPanelIDChangedHandler;
			CurrentWizardPane.IsLastPanelChanged -= nextWizardPanelIDChangedHandler;
			CurrentWizardPane.FinishPanelRequested -= finishPanelHandler;
		}
		activePanelNumber = number;
		if (CurrentWizardPane != null)
		{
			CurrentWizardPane.EnableNextChanged += enableNextChangedHandler;
			CurrentWizardPane.EnableCancelChanged += enableCancelChangedHandler;
			CurrentWizardPane.EnablePreviousChanged += enableNextChangedHandler;
			CurrentWizardPane.NextWizardPanelIDChanged += nextWizardPanelIDChangedHandler;
			CurrentWizardPane.IsLastPanelChanged += nextWizardPanelIDChangedHandler;
			CurrentWizardPane.FinishPanelRequested += finishPanelHandler;
		}
		EnableNextChanged(null, null);
		NextWizardPanelIDChanged(null, null);
		EnableCancelChanged(null, null);
		statusPanel.Refresh();
		curPanel.Refresh();
		dialogPanel.Controls.Clear();
		Control control = CurrentWizardPane.Control;
		control.Dock = DockStyle.Fill;
		dialogPanel.Controls.Add(control);
	}

	public WizardDialog(string title, object customizer, string treePath)
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode(treePath);
		Text = title;
		if (treeNode != null)
		{
			AddNodes(customizer, treeNode.BuildChildItems<IDialogPanelDescriptor>(this));
		}
		InitializeComponents();
		enableNextChangedHandler = EnableNextChanged;
		nextWizardPanelIDChangedHandler = NextWizardPanelIDChanged;
		enableCancelChangedHandler = EnableCancelChanged;
		finishPanelHandler = FinishPanelEvent;
		ActivatePanel(0);
	}

	private void FinishPanelEvent(object sender, EventArgs e)
	{
		AbstractWizardPanel abstractWizardPanel = (AbstractWizardPanel)CurrentWizardPane;
		bool isLastPanel = abstractWizardPanel.IsLastPanel;
		abstractWizardPanel.IsLastPanel = false;
		ShowNextPanelEvent(sender, e);
		abstractWizardPanel.IsLastPanel = isLastPanel;
	}

	private void ShowNextPanelEvent(object sender, EventArgs e)
	{
		int successorNumber = GetSuccessorNumber(ActivePanelNumber);
		if (CurrentWizardPane.ReceiveDialogMessage(DialogMessage.Next))
		{
			idStack.Push(activePanelNumber);
			ActivatePanel(successorNumber);
			CurrentWizardPane.ReceiveDialogMessage(DialogMessage.Activated);
		}
	}

	private void ShowPrevPanelEvent(object sender, EventArgs e)
	{
		if (CurrentWizardPane.ReceiveDialogMessage(DialogMessage.Prev))
		{
			ActivatePanel((int)idStack.Pop());
		}
	}

	private void FinishEvent(object sender, EventArgs e)
	{
		foreach (IDialogPanelDescriptor wizardPanel in wizardPanels)
		{
			if (!wizardPanel.DialogPanel.ReceiveDialogMessage(DialogMessage.Finish))
			{
				return;
			}
		}
		base.DialogResult = DialogResult.OK;
	}

	private void CancelEvent(object sender, EventArgs e)
	{
		foreach (IDialogPanelDescriptor wizardPanel in wizardPanels)
		{
			if (!wizardPanel.DialogPanel.ReceiveDialogMessage(DialogMessage.Cancel))
			{
				return;
			}
		}
		base.DialogResult = DialogResult.Cancel;
	}

	private void HelpEvent(object sender, EventArgs e)
	{
		CurrentWizardPane.ReceiveDialogMessage(DialogMessage.Help);
	}

	private void InitializeComponents()
	{
		SuspendLayout();
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		bool minimizeBox = (base.MaximizeBox = false);
		base.MinimizeBox = minimizeBox;
		base.Icon = null;
		base.ClientSize = new Size(640, 440);
		int num = 92;
		int num2 = 412;
		int num3 = base.Width - (num + 4) * 4 - 4;
		label1.Size = new Size(base.Width - 4, 1);
		label1.BorderStyle = BorderStyle.Fixed3D;
		label1.Location = new Point(2, 402);
		label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		base.Controls.Add(label1);
		backButton.Text = ResourceService.GetString("Global.BackButtonText");
		backButton.Location = new Point(num3, num2);
		backButton.ClientSize = new Size(num, 26);
		backButton.Click += ShowPrevPanelEvent;
		backButton.FlatStyle = FlatStyle.System;
		backButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		base.Controls.Add(backButton);
		nextButton.Text = ResourceService.GetString("Global.NextButtonText");
		nextButton.Location = new Point(num3 + num + 4, num2);
		nextButton.ClientSize = new Size(num, 26);
		nextButton.Click += ShowNextPanelEvent;
		nextButton.FlatStyle = FlatStyle.System;
		nextButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		base.Controls.Add(nextButton);
		finishButton.Text = ResourceService.GetString("Dialog.WizardDialog.FinishButton");
		finishButton.Location = new Point(num3 + 2 * (num + 4), num2);
		finishButton.ClientSize = new Size(num, 26);
		finishButton.Click += FinishEvent;
		finishButton.FlatStyle = FlatStyle.System;
		finishButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		base.Controls.Add(finishButton);
		cancelButton.Text = ResourceService.GetString("Global.CancelButtonText");
		cancelButton.Location = new Point(num3 + 3 * (num + 4), num2);
		cancelButton.ClientSize = new Size(num, 26);
		cancelButton.Click += CancelEvent;
		cancelButton.FlatStyle = FlatStyle.System;
		cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		base.Controls.Add(cancelButton);
		statusPanel = new StatusPanel(this);
		statusPanel.Location = new Point(2, 2);
		statusPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
		base.Controls.Add(statusPanel);
		curPanel = new CurrentPanelPanel(this);
		curPanel.Location = new Point(200, 2);
		curPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		base.Controls.Add(curPanel);
		dialogPanel.Location = new Point(200, 27);
		dialogPanel.Size = new Size(base.Width - 8 - statusPanel.Bounds.Right, label1.Location.Y - dialogPanel.Location.Y);
		dialogPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		base.Controls.Add(dialogPanel);
		ResumeLayout(performLayout: true);
	}
}

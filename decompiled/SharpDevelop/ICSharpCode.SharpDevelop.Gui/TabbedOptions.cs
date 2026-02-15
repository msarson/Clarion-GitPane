using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace ICSharpCode.SharpDevelop.Gui;

public class TabbedOptions : BaseSharpDevelopForm
{
	private ArrayList OptionPanels = new ArrayList();

	private Properties properties;

	private int taller;

	private void AcceptEvent(object sender, EventArgs e)
	{
		foreach (AbstractOptionPanel optionPanel in OptionPanels)
		{
			if (!optionPanel.ReceiveDialogMessage(DialogMessage.OK))
			{
				return;
			}
		}
		base.DialogResult = DialogResult.OK;
	}

	private void AddOptionPanels(IEnumerable<IDialogPanelDescriptor> dialogPanelDescriptors)
	{
		TabControl tabControl = (TabControl)base.ControlDictionary["optionPanelTabControl"];
		if (tabControl == null)
		{
			return;
		}
		taller = tabControl.Height;
		tabControl.SuspendLayout();
		foreach (IDialogPanelDescriptor dialogPanelDescriptor in dialogPanelDescriptors)
		{
			if (dialogPanelDescriptor != null && dialogPanelDescriptor.DialogPanel != null && dialogPanelDescriptor.DialogPanel.Control != null)
			{
				dialogPanelDescriptor.DialogPanel.CustomizationObject = properties;
				dialogPanelDescriptor.DialogPanel.Control.Dock = DockStyle.Fill;
				dialogPanelDescriptor.DialogPanel.ReceiveDialogMessage(DialogMessage.Activated);
				OptionPanels.Add(dialogPanelDescriptor.DialogPanel);
				TabPage tabPage = new TabPage(dialogPanelDescriptor.Label);
				tabPage.SuspendLayout();
				tabPage.UseVisualStyleBackColor = true;
				tabPage.Controls.Add(dialogPanelDescriptor.DialogPanel.Control);
				if (taller < dialogPanelDescriptor.DialogPanel.Control.Height)
				{
					taller = dialogPanelDescriptor.DialogPanel.Control.Height;
				}
				tabPage.ResumeLayout();
				tabPage.Refresh();
				tabControl.TabPages.Add(tabPage);
			}
			if (dialogPanelDescriptor.ChildDialogPanelDescriptors != null)
			{
				AddOptionPanels(dialogPanelDescriptor.ChildDialogPanelDescriptors);
			}
		}
		if (taller > tabControl.Height)
		{
			tabControl.Height = taller;
		}
		tabControl.ResumeLayout(performLayout: true);
		tabControl.Refresh();
	}

	public TabbedOptions(string dialogName, Properties properties, AddInTreeNode node)
	{
		this.properties = properties;
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.TabbedOptionsDialog.xfrm"));
		Text = dialogName;
		base.ControlDictionary["okButton"].Click += AcceptEvent;
		base.Icon = null;
		base.Owner = (Form)WorkbenchSingleton.Workbench;
		AutoSize = false;
		base.ResizeRedraw = true;
		SuspendLayout();
		AddOptionPanels(node.BuildChildItems<IDialogPanelDescriptor>(this));
		FormLocationHelper.Apply(this, "ICSharpCode.SharpDevelop.Gui.TabbedOptions.Location", isResizable: true);
		ResumeLayout();
		Refresh();
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project.Dialogs;

public class ProjectOptionsView : AbstractViewContent
{
	private List<IDialogPanelDescriptor> descriptors = new List<IDialogPanelDescriptor>();

	private TabControl tabControl = new TabControl();

	private IProject project;

	public IProject Project => project;

	public override string TitleName => project.Name;

	public override string FileName
	{
		get
		{
			return project.FileName;
		}
		set
		{
			OnTitleNameChanged(EventArgs.Empty);
		}
	}

	public override Control Control => tabControl;

	public ProjectOptionsView(AddInTreeNode node, IProject project)
	{
		this.project = project;
		tabControl.HandleCreated += TabControlHandleCreated;
		AddOptionPanels(node.BuildChildItems<IDialogPanelDescriptor>(this));
	}

	private void TabControlHandleCreated(object sender, EventArgs e)
	{
		tabControl.HandleCreated -= TabControlHandleCreated;
		tabControl.BeginInvoke(new MethodInvoker(DockControlsInPages));
	}

	private void DockControlsInPages()
	{
		foreach (TabPage tabPage in tabControl.TabPages)
		{
			foreach (Control control in tabPage.Controls)
			{
				control.Dock = DockStyle.Fill;
			}
		}
	}

	private void AddOptionPanels(IEnumerable<IDialogPanelDescriptor> dialogPanelDescriptors)
	{
		Properties properties = new Properties();
		properties.Set("Project", project);
		foreach (IDialogPanelDescriptor dialogPanelDescriptor in dialogPanelDescriptors)
		{
			descriptors.Add(dialogPanelDescriptor);
			if (dialogPanelDescriptor != null && dialogPanelDescriptor.DialogPanel != null && dialogPanelDescriptor.DialogPanel.Control != null)
			{
				dialogPanelDescriptor.DialogPanel.CustomizationObject = properties;
				dialogPanelDescriptor.DialogPanel.ReceiveDialogMessage(DialogMessage.Activated);
				if (dialogPanelDescriptor.DialogPanel is ICanBeDirty canBeDirty)
				{
					canBeDirty.DirtyChanged += PanelDirtyChanged;
				}
				TabPage tabPage = new TabPage(dialogPanelDescriptor.Label);
				tabPage.UseVisualStyleBackColor = true;
				tabPage.Controls.Add(dialogPanelDescriptor.DialogPanel.Control);
				tabControl.TabPages.Add(tabPage);
			}
			if (dialogPanelDescriptor.ChildDialogPanelDescriptors != null)
			{
				AddOptionPanels(dialogPanelDescriptor.ChildDialogPanelDescriptors);
			}
		}
		PanelDirtyChanged(null, null);
	}

	private void PanelDirtyChanged(object sender, EventArgs e)
	{
		bool flag = false;
		foreach (IDialogPanelDescriptor descriptor in descriptors)
		{
			if (descriptor != null && descriptor.DialogPanel is ICanBeDirty canBeDirty)
			{
				flag |= canBeDirty.IsDirty;
			}
		}
		IsDirty = flag;
	}

	public override void Load(string fileName)
	{
	}

	public override void Save(string fileName)
	{
		try
		{
			foreach (IDialogPanelDescriptor descriptor in descriptors)
			{
				if (!(descriptor.DialogPanel is ICanBeDirty { IsDirty: false }))
				{
					descriptor.DialogPanel.ReceiveDialogMessage(DialogMessage.OK);
				}
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex, "Error saving project options panel");
			return;
		}
		project.Save();
	}
}

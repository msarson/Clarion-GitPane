using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace ICSharpCode.SharpDevelop.Gui;

public class TreeViewOptions : BaseSharpDevelopForm
{
	private TreeViewLocator Locator;

	protected GradientHeaderPanel optionsPanelLabel;

	protected List<IDialogPanel> OptionPanels = new List<IDialogPanel>();

	protected Properties properties;

	protected Font plainFont;

	protected Font boldFont;

	private ExtTreeView _optionsTreeView;

	private Panel _optionControlPanel;

	protected bool onSelectingNode;

	private ExtTreeView optionsTreeView
	{
		get
		{
			if (_optionsTreeView == null)
			{
				_optionsTreeView = (ExtTreeView)base.ControlDictionary["optionsTreeView"];
			}
			return _optionsTreeView;
		}
	}

	private Panel optionControlPanel
	{
		get
		{
			if (_optionControlPanel == null)
			{
				_optionControlPanel = (Panel)base.ControlDictionary["optionControlPanel"];
			}
			return _optionControlPanel;
		}
	}

	public Properties Properties => properties;

	protected void AcceptEvent(object sender, EventArgs e)
	{
		foreach (IDialogPanel optionPanel in OptionPanels)
		{
			if (!optionPanel.ReceiveDialogMessage(DialogMessage.OK))
			{
				return;
			}
		}
		base.DialogResult = DialogResult.OK;
	}

	protected void CancelEvent(object sender, EventArgs e)
	{
		foreach (IDialogPanel optionPanel in OptionPanels)
		{
			optionPanel.ReceiveDialogMessage(DialogMessage.Cancel);
		}
		base.DialogResult = DialogResult.Cancel;
	}

	private void FormClosingEvent(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing)
		{
			CancelEvent(null, null);
		}
	}

	protected void SetOptionPanelTo(TreeNode node)
	{
		if (!(node.Tag is IDialogPanelDescriptor { DialogPanel: not null } dialogPanelDescriptor) || dialogPanelDescriptor.DialogPanel.Control == null)
		{
			return;
		}
		if (!OptionPanels.Contains(dialogPanelDescriptor.DialogPanel))
		{
			dialogPanelDescriptor.DialogPanel.CustomizationObject = properties;
			Size size = dialogPanelDescriptor.DialogPanel.Control.Size;
			dialogPanelDescriptor.DialogPanel.Control.AutoSize = false;
			dialogPanelDescriptor.DialogPanel.Control.Dock = DockStyle.Top;
			dialogPanelDescriptor.DialogPanel.Control.AutoSize = true;
			if (dialogPanelDescriptor.DialogPanel.Control.Size.Height == 0)
			{
				dialogPanelDescriptor.DialogPanel.Control.AutoSize = false;
				dialogPanelDescriptor.DialogPanel.Control.Size = size;
			}
			OptionPanels.Add(dialogPanelDescriptor.DialogPanel);
		}
		dialogPanelDescriptor.DialogPanel.ReceiveDialogMessage(DialogMessage.Activated);
		optionControlPanel.Controls.Clear();
		RightToLeftConverter.ConvertRecursive(dialogPanelDescriptor.DialogPanel.Control);
		optionControlPanel.Controls.Add(dialogPanelDescriptor.DialogPanel.Control);
		dialogPanelDescriptor.DialogPanel.Control.AutoSize = false;
		dialogPanelDescriptor.DialogPanel.Control.Dock = DockStyle.Top;
		optionsPanelLabel.Text = dialogPanelDescriptor.Label;
		optionControlPanel.AutoScroll = true;
	}

	protected void AddNodes(TreeNodeCollection nodes, IEnumerable<IDialogPanelDescriptor> dialogPanelDescriptors)
	{
		nodes.Clear();
		foreach (IDialogPanelDescriptor dialogPanelDescriptor in dialogPanelDescriptors)
		{
			ExtTreeNode extTreeNode = new ExtTreeNode();
			extTreeNode.Text = dialogPanelDescriptor.Label;
			extTreeNode.Tag = dialogPanelDescriptor;
			extTreeNode.NodeFont = plainFont;
			nodes.Add(extTreeNode);
			if (dialogPanelDescriptor.ChildDialogPanelDescriptors != null)
			{
				AddNodes(extTreeNode.Nodes, dialogPanelDescriptor.ChildDialogPanelDescriptors);
			}
		}
	}

	protected void SelectNode(object sender, TreeViewEventArgs e)
	{
		SetOptionPanelTo(optionsTreeView.SelectedNode);
	}

	public TreeViewOptions(Properties properties, AddInTreeNode node)
	{
		this.properties = properties;
		Text = StringParser.Parse("${res:Dialog.Options.TreeViewOptions.DialogName}");
		InitializeComponent();
		plainFont = new Font(optionsTreeView.Font, FontStyle.Regular);
		boldFont = new Font(optionsTreeView.Font, FontStyle.Bold);
		if (node != null)
		{
			AddNodes(optionsTreeView.Nodes, node.BuildChildItems<IDialogPanelDescriptor>(this));
		}
		FormPositionService.Instance.Apply(this, "OptionsDialog");
	}

	protected override void InitializeXmlComponents()
	{
		base.SizeGripStyle = SizeGripStyle.Show;
		base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
		base.AutoSizeMode = AutoSizeMode.GrowOnly;
		optionControlPanel.AutoSize = false;
		optionControlPanel.AutoScroll = true;
		base.InitializeXmlComponents();
	}

	protected void InitializeComponent()
	{
		base.Owner = (System.Windows.Forms.Form)ICSharpCode.SharpDevelop.Gui.WorkbenchSingleton.Workbench;
		base.SetupFromXmlStream(base.GetType().Assembly.GetManifestResourceStream("Resources.TreeViewOptionsDialog.xfrm"));
		base.Icon = null;
		this.optionsTreeView.ShowLines = false;
		this.optionsTreeView.FullNodeRowSelect = true;
		this.optionsTreeView.IsSorted = false;
		this.Locator = new ICSharpCode.SharpDevelop.Gui.TreeViewLocator();
		this.Locator.BackColor = System.Drawing.SystemColors.Control;
		this.Locator.Dock = System.Windows.Forms.DockStyle.Top;
		this.Locator.InString = true;
		this.Locator.IsTransparent = false;
		this.Locator.Location = new System.Drawing.Point(0, 0);
		this.Locator.Margin = new System.Windows.Forms.Padding(0);
		this.Locator.Name = "Locator";
		this.Locator.ShowBeginWithButton = false;
		this.Locator.TabIndex = 0;
		this.Locator.TreeToSearch = this.optionsTreeView;
		this.Locator.ObjectSerchRequested += new System.EventHandler<ICSharpCode.SharpDevelop.Gui.TreeViewLocator.SearchFoundEventArgs>(Locator_ObjectSerchRequested);
		base.ControlDictionary["treePanel"].Controls.Add(this.Locator);
		this.optionsPanelLabel = new ICSharpCode.SharpDevelop.Gui.GradientHeaderPanel();
		this.optionsPanelLabel.Font = new System.Drawing.Font("Tahoma", 14f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.optionsPanelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.optionsPanelLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.optionsPanelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
		base.ControlDictionary["headerPanel"].Controls.Add(this.optionsPanelLabel);
		base.ControlDictionary["okButton"].Click += new System.EventHandler(AcceptEvent);
		base.ControlDictionary["cancelButton"].Click += new System.EventHandler(CancelEvent);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FormClosingEvent);
		this.optionsTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(OnBeforeSelect);
		this.optionsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(OnAfterSelect);
		base.ResumeLayout(true);
	}

	protected void OnBeforeSelect(object sender, TreeViewCancelEventArgs e)
	{
		if (!onSelectingNode)
		{
			onSelectingNode = true;
			optionsTreeView.BeginUpdate();
			TreeNode treeNode = e.Node;
			while (treeNode.Nodes.Count > 0 && treeNode.FirstNode != null)
			{
				treeNode = treeNode.FirstNode;
			}
			if (treeNode != e.Node && treeNode.Parent != null && optionsTreeView.SelectedNode != null && treeNode.Parent != optionsTreeView.SelectedNode.Parent)
			{
				optionsTreeView.CollapseAll();
				treeNode.EnsureVisible();
			}
			onSelectingNode = false;
			if (treeNode != e.Node)
			{
				optionsTreeView.SelectedNode = treeNode;
			}
			optionsTreeView.EndUpdate();
		}
	}

	private void OnAfterSelect(object sender, TreeViewEventArgs e)
	{
		if (!onSelectingNode)
		{
			SetOptionPanelTo(optionsTreeView.SelectedNode);
		}
	}

	private void Locator_ObjectSerchRequested(object sender, TreeViewLocator.SearchFoundEventArgs e)
	{
		if (e != null && !e.Found && e.ObjectSearched != null && e.ObjectSearched is IDialogPanelDescriptor { DialogPanel: not null } dialogPanelDescriptor)
		{
			try
			{
				dialogPanelDescriptor.DialogPanel.ReceiveDialogMessage(DialogMessage.Activated);
				e.Found = dialogPanelDescriptor.DialogPanel.ExistControlWithText(e.SearchText, e);
				dialogPanelDescriptor.DialogPanel.ReceiveDialogMessage(DialogMessage.Cancel);
			}
			catch
			{
			}
		}
	}
}

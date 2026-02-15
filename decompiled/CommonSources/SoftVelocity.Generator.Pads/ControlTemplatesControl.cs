using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;
using SoftVelocity.ClarionNet.Designer;
using SoftVelocity.ClarionNet.Designer.SectionControls;
using SoftVelocity.ClarionNet.WindowDesigner;
using SoftVelocity.Common.FormDesigner;
using SoftVelocity.Generator.DragDrop;
using SoftVelocity.Generator.Editor;

namespace SoftVelocity.Generator.Pads;

public class ControlTemplatesControl : UserControl
{
	private IBaseViewContent previousViewContent;

	private TreeModel _TreeModel;

	private IContainer HostContainer;

	private IDesignerHost _Host;

	private IContainer components;

	private SplitContainer splitContainer;

	private Label textDescription;

	private TreeViewAdvBase m_treeAdvTpl;

	private NodeStateIcon nodeStateIcon1;

	private NodeTextBox nodeTextBox1;

	private Locator m_treeAdvTplLocator;

	private TableLayoutPanel tableLayoutPanel1;

	private CommonClarionGenDesignerView GenDesignerView
	{
		get
		{
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
			{
				return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as CommonClarionGenDesignerView;
			}
			return null;
		}
	}

	private Report ReportControl
	{
		get
		{
			if (CurrentHost.RootComponent is BaseDesignerControl baseDesignerControl)
			{
				return baseDesignerControl.ReportControl;
			}
			return null;
		}
	}

	private GeneralDesiner WindowControl => CurrentHost.RootComponent as GeneralDesiner;

	private IDesignerHost CurrentHost
	{
		get
		{
			if (_Host == null)
			{
				_Host = GetActiveDesigner();
			}
			if (_Host == null)
			{
				ClearHostAndServices();
				HostContainer = null;
			}
			else
			{
				HostContainer = _Host.Container;
			}
			return _Host;
		}
	}

	public bool IsTemplateSelected
	{
		get
		{
			if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null && ((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Tag != null && ((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Tag is Node node)
			{
				return node.Tag is IControlTemplate;
			}
			return false;
		}
	}

	public ControlTemplatesControl()
	{
		InitializeComponent();
		((Control)(object)m_treeAdvTpl).Font = FontService.GetFont((FontType)1);
		((TreeViewAdv)(object)m_treeAdvTpl).RowHeight = ((Control)(object)m_treeAdvTpl).Font.Height + 4;
		DisableAll();
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += Workbench_ActiveWorkbenchWindowChanged;
		RemoveSelection();
		base.ParentChanged += ControlTemplatesControl_ParentChanged;
	}

	private void ControlTemplatesControl_ParentChanged(object sender, EventArgs e)
	{
		_ = base.ParentForm;
	}

	private bool IsWindowReport()
	{
		CommonClarionGenDesignerView genDesignerView = GenDesignerView;
		if (genDesignerView == null)
		{
			return false;
		}
		return ((ClaDesignerGenerator.FormDesignerModeenum)(object)genDesignerView.InternalState & (ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner | ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)) > (ClaDesignerGenerator.FormDesignerModeenum)0;
	}

	private bool IsReport()
	{
		CommonClarionGenDesignerView genDesignerView = GenDesignerView;
		if (genDesignerView == null)
		{
			return false;
		}
		return ((ClaDesignerGenerator.FormDesignerModeenum)(object)genDesignerView.InternalState & ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner) > (ClaDesignerGenerator.FormDesignerModeenum)0;
	}

	private bool IsWindow()
	{
		CommonClarionGenDesignerView genDesignerView = GenDesignerView;
		if (genDesignerView == null)
		{
			return false;
		}
		return ((ClaDesignerGenerator.FormDesignerModeenum)(object)genDesignerView.InternalState & ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner) > (ClaDesignerGenerator.FormDesignerModeenum)0;
	}

	private void DesignerClosed()
	{
		DisableAll();
	}

	private void Workbench_ActiveWorkbenchWindowChanged(object sender, EventArgs e)
	{
		if (WorkbenchSingleton.Workbench != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is FormsDesignerViewContent)
		{
			previousViewContent = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent;
			if (!((FormsDesignerViewContent)(object)WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent).FailedDesignerInitialize)
			{
				EnableAll();
				_Host = CurrentHost;
				SetHostAndServices();
			}
		}
		else
		{
			DisableAll();
		}
	}

	private void EnableAll()
	{
		splitContainer.Visible = true;
		splitContainer.Enabled = true;
	}

	private void DisableAll()
	{
		splitContainer.Visible = false;
		ClearHostAndServices();
	}

	private void ClearHostAndServices()
	{
		_Host = null;
	}

	private void SetHostAndServices()
	{
		if (_Host != null)
		{
			_Host = GetActiveDesigner();
		}
	}

	private IDesignerHost GetActiveDesigner()
	{
		try
		{
			if (WorkbenchSingleton.Workbench != null && (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent != null) && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is FormsDesignerViewContent)
			{
				FormsDesignerViewContent formsDesignerViewContent = (FormsDesignerViewContent)(object)WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent;
				if (formsDesignerViewContent != null && formsDesignerViewContent.Host != null)
				{
					return formsDesignerViewContent.Host;
				}
			}
		}
		catch
		{
		}
		return null;
	}

	public bool RefreshTemplates(IFormatter iformatter)
	{
		IControlTemplate controlTemplate = ((((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null) ? (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Tag as IControlTemplate) : null);
		IPopulatedTemplate populatedTemplate = ((((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null) ? (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Tag as IPopulatedTemplate) : null);
		if (controlTemplate == null && populatedTemplate != null && ((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null)
		{
			controlTemplate = ((((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Parent == null) ? null : (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Parent.Tag as IControlTemplate));
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		try
		{
			empty = ((controlTemplate != null) ? controlTemplate.Name : string.Empty);
			empty2 = ((controlTemplate != null && controlTemplate.Hosts != null) ? controlTemplate.ParentName : string.Empty);
		}
		catch (Exception)
		{
			empty = (empty2 = string.Empty);
		}
		Node node = null;
		System.Windows.Forms.Application.DoEvents();
		_TreeModel = null;
		GC.Collect(3);
		if (iformatter == null)
		{
			return false;
		}
		SuspendLayout();
		((Control)(object)m_treeAdvTpl).SuspendLayout();
		((TreeViewAdv)(object)m_treeAdvTpl).Model = null;
		_TreeModel = new TreeModel();
		ICollection templates = iformatter.Templates;
		foreach (ITemplateClass item in templates)
		{
			Node node2 = new Node(item.Name);
			_TreeModel.Root.Nodes.Add(node2);
			foreach (IControlTemplate controlTemplate2 in item.ControlTemplates)
			{
				Node node3 = ((controlTemplate2.Hosts == null) ? new NodeSimpleLeaf(controlTemplate2.Name) : new Node(controlTemplate2.Name));
				node3.Tag = controlTemplate2;
				node2.Nodes.Add(node3);
				try
				{
					if (controlTemplate != null && empty == controlTemplate2.Name && ((controlTemplate2.Hosts == null && empty2 == string.Empty) || (controlTemplate2.Hosts != null && empty2 == controlTemplate2.ParentName)))
					{
						node = node3;
					}
				}
				catch (Exception)
				{
					node = null;
				}
				if (controlTemplate2.Hosts == null)
				{
					continue;
				}
				foreach (IPopulatedTemplate host in controlTemplate2.Hosts)
				{
					Node node4 = new NodeSimpleLeaf(StripProcName(host.Description));
					node4.Tag = host;
					node3.Nodes.Add(node4);
				}
			}
		}
		SortedTreeModel sortedTreeModel = new SortedTreeModel(_TreeModel);
		sortedTreeModel.Comparer = new NodeTextSorter();
		((TreeViewAdv)(object)m_treeAdvTpl).Model = sortedTreeModel;
		((TreeViewAdv)(object)m_treeAdvTpl).ExpandAll();
		if (node != null)
		{
			((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode = ((TreeViewAdv)(object)m_treeAdvTpl).FindNodeByTag((object)node);
		}
		((Control)(object)m_treeAdvTpl).ResumeLayout();
		ResumeLayout();
		return true;
	}

	private string StripProcName(string fullDesc)
	{
		string text = fullDesc;
		if (!string.IsNullOrEmpty(text))
		{
			int num = text.LastIndexOf('-');
			text = ((num <= 0) ? text.Trim() : text.Substring(0, num).Trim());
		}
		return text;
	}

	public bool TemplatePopulated(CommonClarionGenDesignerView dv, bool isRefresh)
	{
		if (dv == null)
		{
			dv = GenDesignerView;
		}
		if (dv != null && isRefresh)
		{
			RefreshTemplates(dv.FormatterRequester);
		}
		RemoveSelection();
		return true;
	}

	public bool RemoveSelection()
	{
		((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode = null;
		m_treeAdvTplLocator.ResetText();
		textDescription.Text = string.Empty;
		return true;
	}

	public SpecialDataObject GetDragDropDataObject()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		IControlTemplate controlTemplate = null;
		if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null)
		{
			if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Tag is Node node)
			{
				controlTemplate = node.Tag as IControlTemplate;
			}
		}
		else
		{
			controlTemplate = null;
		}
		IPopulatedTemplate popTemplate = null;
		if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null)
		{
			if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Tag is Node node2)
			{
				popTemplate = node2.Tag as IPopulatedTemplate;
			}
		}
		else
		{
			popTemplate = null;
		}
		if (controlTemplate == null && ((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null && ((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Level == 3)
		{
			if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Parent != null)
			{
				controlTemplate = null;
				if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Parent.Tag is Node node3)
				{
					controlTemplate = node3.Tag as IControlTemplate;
				}
			}
			else
			{
				controlTemplate = null;
			}
		}
		if (controlTemplate != null)
		{
			CommonClarionGenDesignerView genDesignerView = GenDesignerView;
			if (genDesignerView != null)
			{
				SpecialDataObject val = new SpecialDataObject();
				val.SetData((object)new DragDropDataObject(genDesignerView.FormatterRequester, controlTemplate, popTemplate));
				return val;
			}
		}
		return null;
	}

	private void m_treeAdvTpl_ItemDrag(object sender, ItemDragEventArgs e)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		IControlTemplate controlTemplate = null;
		if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null)
		{
			if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Tag is Node node)
			{
				controlTemplate = node.Tag as IControlTemplate;
			}
		}
		else
		{
			controlTemplate = null;
		}
		IPopulatedTemplate popTemplate = null;
		if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null)
		{
			if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Tag is Node node2)
			{
				popTemplate = node2.Tag as IPopulatedTemplate;
			}
		}
		else
		{
			popTemplate = null;
		}
		if (controlTemplate == null && ((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null && ((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Level == 3)
		{
			if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Parent != null)
			{
				controlTemplate = null;
				if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Parent.Tag is Node node3)
				{
					controlTemplate = node3.Tag as IControlTemplate;
				}
			}
			else
			{
				controlTemplate = null;
			}
		}
		if (controlTemplate != null)
		{
			CommonClarionGenDesignerView genDesignerView = GenDesignerView;
			if (genDesignerView != null)
			{
				SpecialDataObject val = new SpecialDataObject();
				val.SetData((object)new DragDropDataObject(genDesignerView.FormatterRequester, controlTemplate, popTemplate));
				DoDragDrop(val, DragDropEffects.All);
				RemoveSelection();
			}
		}
	}

	private void m_treeAdvTpl_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode = null;
			RemoveSelection();
		}
	}

	private void m_treeAdvTpl_SelectionChanged(object sender, EventArgs e)
	{
		if (CurrentHost == null)
		{
			return;
		}
		if (CurrentHost.GetService(typeof(IToolboxService)) is IToolboxService toolboxService)
		{
			toolboxService.SelectedToolboxItemUsed();
		}
		if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode != null)
		{
			if (((TreeViewAdv)(object)m_treeAdvTpl).SelectedNode.Tag is Node node)
			{
				if (node.Tag is IControlTemplate controlTemplate)
				{
					textDescription.Text = controlTemplate.Description;
				}
				else if (node.Tag is IPopulatedTemplate populatedTemplate)
				{
					textDescription.Text = populatedTemplate.Description;
				}
				else
				{
					textDescription.Text = string.Empty;
				}
			}
		}
		else
		{
			textDescription.Text = string.Empty;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		this.splitContainer = new System.Windows.Forms.SplitContainer();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.m_treeAdvTpl = new TreeViewAdvBase();
		this.nodeStateIcon1 = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
		this.nodeTextBox1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
		this.m_treeAdvTplLocator = new Aga.Controls.Tree.Locator();
		this.textDescription = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.splitContainer).BeginInit();
		this.splitContainer.Panel1.SuspendLayout();
		this.splitContainer.Panel2.SuspendLayout();
		this.splitContainer.SuspendLayout();
		this.tableLayoutPanel1.SuspendLayout();
		base.SuspendLayout();
		this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
		this.splitContainer.Location = new System.Drawing.Point(0, 0);
		this.splitContainer.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
		this.splitContainer.Name = "splitContainer";
		this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer.Panel1.Controls.Add(this.tableLayoutPanel1);
		this.splitContainer.Panel2.Controls.Add(this.textDescription);
		this.splitContainer.Size = new System.Drawing.Size(390, 600);
		this.splitContainer.SplitterDistance = 554;
		this.splitContainer.SplitterWidth = 5;
		this.splitContainer.TabIndex = 1;
		this.tableLayoutPanel1.ColumnCount = 1;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel1.Controls.Add((System.Windows.Forms.Control)(object)this.m_treeAdvTpl, 0, 1);
		this.tableLayoutPanel1.Controls.Add(this.m_treeAdvTplLocator, 0, 0);
		this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
		this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 2;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(390, 554);
		this.tableLayoutPanel1.TabIndex = 6;
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		((System.Windows.Forms.UserControl)(object)this.m_treeAdvTpl).AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).BackColor = System.Drawing.SystemColors.Window;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).DefaultToolTipProvider = null;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).DragDropMarkColor = System.Drawing.Color.Black;
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).Font = new System.Drawing.Font("Segoe UI", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).ForeColor = System.Drawing.SystemColors.WindowText;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).FullRowSelect = true;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).GoToLastWhenClickBelowLast = false;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).InactiveRowColor = System.Drawing.SystemColors.InactiveCaption;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).InactiveTextColor = System.Drawing.SystemColors.InactiveCaptionText;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).LineColor = System.Drawing.SystemColors.WindowText;
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).Location = new System.Drawing.Point(4, 57);
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).Margin = new System.Windows.Forms.Padding(4, 5, 9, 5);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).Model = null;
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).Name = "m_treeAdvTpl";
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).NodeControls.Add(this.nodeStateIcon1);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).NodeControls.Add(this.nodeTextBox1);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).RowHeight = 20;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).SelectedNode = null;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).SelectedRowColor = System.Drawing.SystemColors.Highlight;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).SelectedTextColor = System.Drawing.SystemColors.HighlightText;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).ShowLines = false;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).ShowPlusMinusTriangle = true;
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).Size = new System.Drawing.Size(377, 492);
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).TabIndex = 3;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).ThemedBar = false;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).ItemDrag += new System.Windows.Forms.ItemDragEventHandler(m_treeAdvTpl_ItemDrag);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl).SelectionChanged += new System.EventHandler(m_treeAdvTpl_SelectionChanged);
		((System.Windows.Forms.Control)(object)this.m_treeAdvTpl).MouseClick += new System.Windows.Forms.MouseEventHandler(m_treeAdvTpl_MouseClick);
		this.nodeStateIcon1.LeftMargin = 1;
		this.nodeStateIcon1.ParentColumn = null;
		this.nodeTextBox1.DataPropertyName = "Text";
		this.nodeTextBox1.EditEnabled = false;
		this.nodeTextBox1.IncrementalSearchEnabled = true;
		this.nodeTextBox1.LeftMargin = 3;
		this.nodeTextBox1.ParentColumn = null;
		this.m_treeAdvTplLocator.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_treeAdvTplLocator.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.m_treeAdvTplLocator.InString = true;
		this.m_treeAdvTplLocator.Location = new System.Drawing.Point(0, 0);
		this.m_treeAdvTplLocator.Margin = new System.Windows.Forms.Padding(0);
		this.m_treeAdvTplLocator.Name = "m_treeAdvTplLocator";
		this.m_treeAdvTplLocator.Size = new System.Drawing.Size(390, 52);
		this.m_treeAdvTplLocator.SuportExapandContractButtons = true;
		this.m_treeAdvTplLocator.TabIndex = 4;
		this.m_treeAdvTplLocator.TreeToSearch = (Aga.Controls.Tree.TreeViewAdv)(object)this.m_treeAdvTpl;
		this.textDescription.BackColor = System.Drawing.SystemColors.Control;
		this.textDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.textDescription.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textDescription.Location = new System.Drawing.Point(0, 0);
		this.textDescription.Name = "textDescription";
		this.textDescription.Size = new System.Drawing.Size(390, 41);
		this.textDescription.TabIndex = 3;
		base.AutoScaleDimensions = new System.Drawing.SizeF(9f, 20f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.Controls.Add(this.splitContainer);
		base.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		base.Name = "ControlTemplatesControl";
		base.Size = new System.Drawing.Size(390, 600);
		this.splitContainer.Panel1.ResumeLayout(false);
		this.splitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer).EndInit();
		this.splitContainer.ResumeLayout(false);
		this.tableLayoutPanel1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}

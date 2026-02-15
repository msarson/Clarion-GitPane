using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace SoftVelocity.Generator.Dialogs;

public class ControlTemplateDlg : Form
{
	private static uint m_DefLocationValue = uint.MaxValue;

	private static int m_XLocation = (int)m_DefLocationValue;

	private static int m_YLocation = (int)m_DefLocationValue;

	private static int m_WSize = (int)m_DefLocationValue;

	private static int m_HSize = (int)m_DefLocationValue;

	private ImageList imageList1;

	private bool m_isInitializing;

	private Button m_btnSelect;

	private Button m_btnCancel;

	private TextBox m_txtTpl;

	private TreeView m_treeTpl;

	private IContainer components;

	private IFormatter m_ifo;

	private IControlTemplate m_returnIControlTemplate;

	public IFormatter Formatter => m_ifo;

	public IControlTemplate ReturnIControlTemplate => m_returnIControlTemplate;

	protected override void OnMove(EventArgs e)
	{
		if (!m_isInitializing)
		{
			m_XLocation = base.Left;
			m_YLocation = base.Top;
		}
		base.OnMove(e);
	}

	protected override void OnResize(EventArgs e)
	{
		if (!m_isInitializing)
		{
			m_WSize = base.Width;
			m_HSize = base.Height;
		}
		base.OnResize(e);
	}

	public ControlTemplateDlg(IFormatter ifo)
	{
		m_ifo = ifo;
		m_isInitializing = true;
		InitializeComponent();
		imageList1.Images.Add(ResourceService.GetIcon("Icons.16x16.Leaf"));
		imageList1.Images.Add(ResourceService.GetIcon("Icons.16x16.Folder"));
		imageList1.Images.Add(ResourceService.GetIcon("Icons.16x16.FolderClosed"));
		if (m_XLocation != (int)m_DefLocationValue || m_YLocation != (int)m_DefLocationValue)
		{
			base.StartPosition = FormStartPosition.Manual;
			base.Location = new Point(m_XLocation, m_YLocation);
		}
		else
		{
			base.StartPosition = FormStartPosition.CenterScreen;
		}
		if (m_WSize != (int)m_DefLocationValue || m_HSize != (int)m_DefLocationValue)
		{
			base.Size = new Size(m_WSize, m_HSize);
		}
		m_isInitializing = false;
		InitNodes();
	}

	public bool InitNodes()
	{
		if (Formatter == null)
		{
			return false;
		}
		ICollection templates = Formatter.Templates;
		foreach (ITemplateClass item in templates)
		{
			TreeNode treeNode = new TreeNode(item.Name);
			m_treeTpl.Nodes.Add(treeNode);
			int imageIndex = (treeNode.SelectedImageIndex = 2);
			treeNode.ImageIndex = imageIndex;
			foreach (IControlTemplate controlTemplate in item.ControlTemplates)
			{
				TreeNode treeNode2 = new TreeNode(controlTemplate.Name);
				treeNode2.Tag = controlTemplate;
				int imageIndex2 = (treeNode2.SelectedImageIndex = 0);
				treeNode2.ImageIndex = imageIndex2;
				treeNode.Nodes.Add(treeNode2);
			}
		}
		m_treeTpl.ExpandAll();
		return true;
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
		this.components = new System.ComponentModel.Container();
		this.m_btnSelect = new System.Windows.Forms.Button();
		this.m_btnCancel = new System.Windows.Forms.Button();
		this.m_txtTpl = new System.Windows.Forms.TextBox();
		this.m_treeTpl = new System.Windows.Forms.TreeView();
		this.imageList1 = new System.Windows.Forms.ImageList(this.components);
		base.SuspendLayout();
		this.m_btnSelect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnSelect.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_btnSelect.Enabled = false;
		this.m_btnSelect.Location = new System.Drawing.Point(288, 8);
		this.m_btnSelect.Name = "m_btnSelect";
		this.m_btnSelect.Size = new System.Drawing.Size(75, 23);
		this.m_btnSelect.TabIndex = 2;
		this.m_btnSelect.Text = "&Select";
		this.m_btnSelect.Click += new System.EventHandler(m_btnSelect_Click);
		this.m_btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_btnCancel.Location = new System.Drawing.Point(288, 40);
		this.m_btnCancel.Name = "m_btnCancel";
		this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
		this.m_btnCancel.TabIndex = 3;
		this.m_btnCancel.Text = "&Cancel";
		this.m_txtTpl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_txtTpl.Location = new System.Drawing.Point(8, 8);
		this.m_txtTpl.Name = "m_txtTpl";
		this.m_txtTpl.Size = new System.Drawing.Size(272, 20);
		this.m_txtTpl.TabIndex = 0;
		this.m_txtTpl.TextChanged += new System.EventHandler(m_txtTpl_TextChanged);
		this.m_treeTpl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_treeTpl.FullRowSelect = true;
		this.m_treeTpl.HideSelection = false;
		this.m_treeTpl.ImageIndex = 0;
		this.m_treeTpl.ImageList = this.imageList1;
		this.m_treeTpl.Location = new System.Drawing.Point(8, 32);
		this.m_treeTpl.Name = "m_treeTpl";
		this.m_treeTpl.SelectedImageIndex = 0;
		this.m_treeTpl.Size = new System.Drawing.Size(272, 248);
		this.m_treeTpl.TabIndex = 1;
		this.m_treeTpl.DoubleClick += new System.EventHandler(m_treeTpl_DoubleClick);
		this.m_treeTpl.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(m_treeTpl_AfterSelect);
		this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
		this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
		this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
		base.AcceptButton = this.m_btnSelect;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		base.CancelButton = this.m_btnCancel;
		base.ClientSize = new System.Drawing.Size(368, 285);
		base.Controls.Add(this.m_treeTpl);
		base.Controls.Add(this.m_txtTpl);
		base.Controls.Add(this.m_btnCancel);
		base.Controls.Add(this.m_btnSelect);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ControlTemplateDlg";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.Text = "Select Control Template";
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private void m_treeTpl_AfterSelect(object sender, TreeViewEventArgs e)
	{
		if (e.Node.Parent != null)
		{
			m_btnSelect.Enabled = true;
		}
		else
		{
			m_btnSelect.Enabled = false;
		}
	}

	private void m_btnSelect_Click(object sender, EventArgs e)
	{
		m_returnIControlTemplate = m_treeTpl.SelectedNode.Tag as IControlTemplate;
	}

	private void m_treeTpl_DoubleClick(object sender, EventArgs e)
	{
		if (m_treeTpl.SelectedNode != null)
		{
			m_returnIControlTemplate = m_treeTpl.SelectedNode.Tag as IControlTemplate;
			if (m_returnIControlTemplate != null)
			{
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}
	}

	private void m_txtTpl_TextChanged(object sender, EventArgs e)
	{
		string value = m_txtTpl.Text.ToUpper();
		foreach (TreeNode node in m_treeTpl.Nodes)
		{
			foreach (TreeNode node2 in node.Nodes)
			{
				if (node2.Text.ToUpper().IndexOf(value) == 0)
				{
					m_treeTpl.SelectedNode = node2;
					return;
				}
			}
		}
	}
}

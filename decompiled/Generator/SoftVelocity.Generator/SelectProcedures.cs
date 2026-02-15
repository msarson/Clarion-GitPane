using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using Clarion.GEN;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator;

public class SelectProcedures : Form
{
	private ApplicationTreeModel treeModel;

	private List<string> selectedProcedures = new List<string>();

	private List<string> selectedModules = new List<string>();

	private IContainer components;

	private Button buttonSelect;

	private Button buttonClear;

	private TreeViewAdvBase applicationsTree;

	private Button buttonSelectAll;

	private Button buttonClearAll;

	private Button buttonAcceptForm;

	private Button buttonCancelForm;

	private TabControl tabControlSorting;

	private TabPage tabPageModule;

	private TabPage tabPageTemplate;

	private TabPage tabPageName;

	private TabPage tabPageCategory;

	private NodeCheckBox nodeCheckBox1;

	private NodeIcon nodeIcon1;

	private NodeTextBox nodeTextBox1;

	private ApplicationTreeModel TreeModel
	{
		set
		{
			treeModel = value;
			((TreeViewAdv)(object)applicationsTree).Model = treeModel;
		}
	}

	public List<string> SelectedProcedures => selectedProcedures;

	public List<string> SelectedModules => selectedModules;

	public SelectProcedures()
	{
		base.DialogResult = DialogResult.Cancel;
		InitializeComponent();
		Font = FontService.GetFont((FontType)0);
	}

	public void Init(string Title, Application app)
	{
		Text = Title;
		TreeModel = new ApplicationTreeModel(app);
		ChangeSorting(tabControlSorting.SelectedIndex);
		nodeCheckBox1.IsVisibleValueNeeded += treeModel.IsCheckBoxVisibleValueNeeded;
		nodeCheckBox1.CheckStateChanged += nodeCheckBox1_CheckStateChanged;
		nodeCheckBox1.ValueNeeded += nodeCheckBox1_ValueNeeded;
		nodeCheckBox1.ValuePushed += nodeCheckBox1_ValuePushed;
		nodeIcon1.ValueNeeded += treeModel.IconNeeded;
		nodeTextBox1.ValueNeeded += treeModel.TextNeeded;
	}

	private void nodeCheckBox1_CheckStateChanged(object sender, TreePathEventArgs e)
	{
	}

	private void nodeCheckBox1_ValuePushed(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag == null)
		{
			return;
		}
		CheckState checkState = CheckState.Checked;
		checkState = (e.Value.Equals(CheckState.Checked) ? CheckState.Checked : CheckState.Unchecked);
		string text = "";
		if (e.Node.Tag is Module)
		{
			Module module = (Module)e.Node.Tag;
			text = module.Name;
			if (checkState == CheckState.Checked)
			{
				if (!selectedModules.Contains(text))
				{
					selectedModules.Add(text);
				}
			}
			else if (selectedModules.Contains(text))
			{
				selectedModules.Remove(text);
			}
		}
		else if (e.Node.Tag is Procedure)
		{
			Procedure procedure = (Procedure)e.Node.Tag;
			text = procedure.Name;
			if (checkState == CheckState.Checked)
			{
				if (!selectedProcedures.Contains(text))
				{
					selectedProcedures.Add(text);
				}
			}
			else if (selectedProcedures.Contains(text))
			{
				selectedProcedures.Remove(text);
			}
		}
		else
		{
			e.Value = false;
		}
	}

	private void nodeCheckBox1_ValueNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag == null)
		{
			return;
		}
		string text = "";
		if (e.Node.Tag is Module)
		{
			text = ((Module)e.Node.Tag).Name;
			if (selectedModules.Contains(text))
			{
				e.Value = CheckState.Checked;
			}
			else
			{
				e.Value = CheckState.Unchecked;
			}
		}
		else if (e.Node.Tag is Procedure)
		{
			text = ((Procedure)e.Node.Tag).Name;
			if (selectedProcedures.Contains(text))
			{
				e.Value = CheckState.Checked;
			}
			else
			{
				e.Value = CheckState.Unchecked;
			}
		}
		else
		{
			e.Value = CheckState.Unchecked;
		}
	}

	private void nodeCheckBox1_IsVisibleValueNeeded(object sender, NodeControlValueEventArgs e)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	private void tabControlSorting_SelectedIndexChanged(object sender, EventArgs e)
	{
		ChangeSorting(tabControlSorting.SelectedIndex);
	}

	private void ChangeSorting(int selectedTab)
	{
		switch (selectedTab)
		{
		case 0:
			treeModel.TreeMode = AppTreeMode.AppModuleView;
			break;
		case 1:
			treeModel.TreeMode = AppTreeMode.AppTemplateView;
			break;
		case 2:
			treeModel.TreeMode = AppTreeMode.AppAlphaView;
			break;
		case 3:
			treeModel.TreeMode = AppTreeMode.AppCategoryView;
			break;
		}
	}

	private void buttonSelect_Click(object sender, EventArgs e)
	{
	}

	private void buttonClear_Click(object sender, EventArgs e)
	{
	}

	private void buttonSelectAll_Click(object sender, EventArgs e)
	{
	}

	private void buttonClearAll_Click(object sender, EventArgs e)
	{
	}

	private void buttonAcceptForm_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void buttonCancelForm_Click(object sender, EventArgs e)
	{
		Close();
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
		this.buttonSelect = new System.Windows.Forms.Button();
		this.buttonClear = new System.Windows.Forms.Button();
		this.applicationsTree = new TreeViewAdvBase();
		this.nodeCheckBox1 = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
		this.nodeIcon1 = new Aga.Controls.Tree.NodeControls.NodeIcon();
		this.nodeTextBox1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
		this.buttonSelectAll = new System.Windows.Forms.Button();
		this.buttonClearAll = new System.Windows.Forms.Button();
		this.buttonAcceptForm = new System.Windows.Forms.Button();
		this.buttonCancelForm = new System.Windows.Forms.Button();
		this.tabControlSorting = new System.Windows.Forms.TabControl();
		this.tabPageModule = new System.Windows.Forms.TabPage();
		this.tabPageTemplate = new System.Windows.Forms.TabPage();
		this.tabPageName = new System.Windows.Forms.TabPage();
		this.tabPageCategory = new System.Windows.Forms.TabPage();
		this.tabControlSorting.SuspendLayout();
		base.SuspendLayout();
		this.buttonSelect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonSelect.Location = new System.Drawing.Point(413, 54);
		this.buttonSelect.Name = "buttonSelect";
		this.buttonSelect.Size = new System.Drawing.Size(109, 27);
		this.buttonSelect.TabIndex = 0;
		this.buttonSelect.Text = "Select";
		this.buttonSelect.UseVisualStyleBackColor = true;
		this.buttonSelect.Click += new System.EventHandler(buttonSelect_Click);
		this.buttonClear.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonClear.Location = new System.Drawing.Point(413, 86);
		this.buttonClear.Name = "buttonClear";
		this.buttonClear.Size = new System.Drawing.Size(109, 27);
		this.buttonClear.TabIndex = 1;
		this.buttonClear.Text = "Clear";
		this.buttonClear.UseVisualStyleBackColor = true;
		this.buttonClear.Click += new System.EventHandler(buttonClear_Click);
		((System.Windows.Forms.Control)(object)this.applicationsTree).Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		((System.Windows.Forms.Control)(object)this.applicationsTree).BackColor = System.Drawing.SystemColors.Window;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).DefaultToolTipProvider = null;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).DragDropMarkColor = System.Drawing.Color.Black;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).FullRowSelect = true;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).LineColor = System.Drawing.SystemColors.ControlDark;
		((System.Windows.Forms.Control)(object)this.applicationsTree).Location = new System.Drawing.Point(12, 54);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).Model = null;
		((System.Windows.Forms.Control)(object)this.applicationsTree).Name = "applicationsTree";
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).NodeControls.Add(this.nodeCheckBox1);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).NodeControls.Add(this.nodeIcon1);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).NodeControls.Add(this.nodeTextBox1);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).RowHeight = 23;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationsTree).SelectedNode = null;
		((System.Windows.Forms.Control)(object)this.applicationsTree).Size = new System.Drawing.Size(385, 346);
		((System.Windows.Forms.Control)(object)this.applicationsTree).TabIndex = 2;
		this.nodeCheckBox1.LeftMargin = 0;
		this.nodeCheckBox1.ParentColumn = null;
		this.nodeCheckBox1.VirtualMode = true;
		this.nodeIcon1.LeftMargin = 1;
		this.nodeIcon1.ParentColumn = null;
		this.nodeIcon1.VirtualMode = true;
		this.nodeTextBox1.EditEnabled = false;
		this.nodeTextBox1.IncrementalSearchEnabled = true;
		this.nodeTextBox1.LeftMargin = 3;
		this.nodeTextBox1.ParentColumn = null;
		this.nodeTextBox1.VirtualMode = true;
		this.buttonSelectAll.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonSelectAll.Location = new System.Drawing.Point(413, 138);
		this.buttonSelectAll.Name = "buttonSelectAll";
		this.buttonSelectAll.Size = new System.Drawing.Size(109, 27);
		this.buttonSelectAll.TabIndex = 3;
		this.buttonSelectAll.Text = "Select All";
		this.buttonSelectAll.UseVisualStyleBackColor = true;
		this.buttonSelectAll.Click += new System.EventHandler(buttonSelectAll_Click);
		this.buttonClearAll.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonClearAll.Location = new System.Drawing.Point(413, 170);
		this.buttonClearAll.Name = "buttonClearAll";
		this.buttonClearAll.Size = new System.Drawing.Size(109, 27);
		this.buttonClearAll.TabIndex = 4;
		this.buttonClearAll.Text = "Clear All";
		this.buttonClearAll.UseVisualStyleBackColor = true;
		this.buttonClearAll.Click += new System.EventHandler(buttonClearAll_Click);
		this.buttonAcceptForm.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonAcceptForm.Location = new System.Drawing.Point(413, 223);
		this.buttonAcceptForm.Name = "buttonAcceptForm";
		this.buttonAcceptForm.Size = new System.Drawing.Size(109, 27);
		this.buttonAcceptForm.TabIndex = 5;
		this.buttonAcceptForm.Text = "Accept";
		this.buttonAcceptForm.UseVisualStyleBackColor = true;
		this.buttonAcceptForm.Click += new System.EventHandler(buttonAcceptForm_Click);
		this.buttonCancelForm.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonCancelForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancelForm.Location = new System.Drawing.Point(413, 254);
		this.buttonCancelForm.Name = "buttonCancelForm";
		this.buttonCancelForm.Size = new System.Drawing.Size(109, 27);
		this.buttonCancelForm.TabIndex = 6;
		this.buttonCancelForm.Text = "Cancel";
		this.buttonCancelForm.UseVisualStyleBackColor = true;
		this.buttonCancelForm.Click += new System.EventHandler(buttonCancelForm_Click);
		this.tabControlSorting.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tabControlSorting.Controls.Add(this.tabPageModule);
		this.tabControlSorting.Controls.Add(this.tabPageTemplate);
		this.tabControlSorting.Controls.Add(this.tabPageName);
		this.tabControlSorting.Controls.Add(this.tabPageCategory);
		this.tabControlSorting.Location = new System.Drawing.Point(12, 18);
		this.tabControlSorting.Name = "tabControlSorting";
		this.tabControlSorting.SelectedIndex = 0;
		this.tabControlSorting.Size = new System.Drawing.Size(385, 30);
		this.tabControlSorting.TabIndex = 7;
		this.tabControlSorting.SelectedIndexChanged += new System.EventHandler(tabControlSorting_SelectedIndexChanged);
		this.tabPageModule.Location = new System.Drawing.Point(4, 25);
		this.tabPageModule.Name = "tabPageModule";
		this.tabPageModule.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageModule.Size = new System.Drawing.Size(377, 1);
		this.tabPageModule.TabIndex = 0;
		this.tabPageModule.Text = "Module";
		this.tabPageModule.UseVisualStyleBackColor = true;
		this.tabPageTemplate.Location = new System.Drawing.Point(4, 25);
		this.tabPageTemplate.Name = "tabPageTemplate";
		this.tabPageTemplate.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageTemplate.Size = new System.Drawing.Size(377, 1);
		this.tabPageTemplate.TabIndex = 1;
		this.tabPageTemplate.Text = "Template";
		this.tabPageTemplate.UseVisualStyleBackColor = true;
		this.tabPageName.Location = new System.Drawing.Point(4, 25);
		this.tabPageName.Name = "tabPageName";
		this.tabPageName.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageName.Size = new System.Drawing.Size(377, 1);
		this.tabPageName.TabIndex = 2;
		this.tabPageName.Text = "Name";
		this.tabPageName.UseVisualStyleBackColor = true;
		this.tabPageCategory.Location = new System.Drawing.Point(4, 25);
		this.tabPageCategory.Name = "tabPageCategory";
		this.tabPageCategory.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageCategory.Size = new System.Drawing.Size(377, 1);
		this.tabPageCategory.TabIndex = 3;
		this.tabPageCategory.Text = "Category";
		this.tabPageCategory.UseVisualStyleBackColor = true;
		base.AcceptButton = this.buttonAcceptForm;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.buttonCancelForm;
		base.ClientSize = new System.Drawing.Size(534, 412);
		base.Controls.Add(this.tabControlSorting);
		base.Controls.Add(this.buttonCancelForm);
		base.Controls.Add(this.buttonAcceptForm);
		base.Controls.Add(this.buttonClearAll);
		base.Controls.Add(this.buttonSelectAll);
		base.Controls.Add((System.Windows.Forms.Control)(object)this.applicationsTree);
		base.Controls.Add(this.buttonClear);
		base.Controls.Add(this.buttonSelect);
		base.MinimizeBox = false;
		base.Name = "SelectProcedures";
		base.ShowInTaskbar = false;
		this.Text = "Select Procedures";
		this.tabControlSorting.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}

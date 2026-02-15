using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Widgets;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

internal class ShortcutEditorPanel : AbstractOptionPanel, IToolTipProvider
{
	private List<MenuShortcutService.CommandShortcut> data;

	private Dictionary<string, KeyValuePair<string, Bitmap>> codonsTextAndImages = new Dictionary<string, KeyValuePair<string, Bitmap>>();

	private TreeViewAdvBase commandList;

	private NodeTextBox commandNameNode;

	private NodeTextBox originalShortcutNode;

	private TreeColumn commandNameColumn;

	private TreeColumn commandOriginalShortcutColumn;

	private TreeColumn commandNewShortcutColumn;

	private NodeIcon commandIconNode;

	private NodeTextBox newShortcutNode;

	private Locator label1;

	private TreeColumn commandIconColumn;

	private Button buttonRestoreAll;

	private Label label2;

	private Label labelCodonId;

	private CheckBox checkBoxUseFullName;

	public ShortcutEditorPanel()
	{
		InitializeComponent();
		commandIconNode.ToolTipProvider = this;
		commandIconNode.ValueNeeded += commandIconNode_ValueNeeded;
		commandNameNode.ValueNeeded += commandNameNode_ValueNeeded;
		commandNameNode.ToolTipProvider = this;
		originalShortcutNode.ValueNeeded += originalShortcutNode_ValueNeeded;
		originalShortcutNode.ToolTipProvider = this;
		newShortcutNode.ValueNeeded += nodeGenericValue1_ValueNeeded;
		newShortcutNode.ValuePushed += nodeGenericValue1_ValuePushed;
		newShortcutNode.ToolTipProvider = this;
	}

	public override void LoadPanelContents()
	{
		LoadData();
		checkBoxUseFullName.Checked = MenuShortcutService.UseFullName;
		base.LoadPanelContents();
	}

	public override bool StorePanelContents()
	{
		MenuShortcutService.UseFullName = checkBoxUseFullName.Checked;
		UpdateData();
		return base.StorePanelContents();
	}

	private void LoadCodonList(AddInTreeNode addinNode)
	{
		if (addinNode == null)
		{
			return;
		}
		Bitmap bitmap = null;
		string text = null;
		string text2 = null;
		foreach (Codon codon in addinNode.Codons)
		{
			if (codon.Properties.Contains("type"))
			{
				text2 = codon.Properties["type"];
			}
			if (!codon.Name.Equals("MenuItem", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			if (codon.Properties.Contains("type"))
			{
				if (!codon.Properties.Contains("type"))
				{
					continue;
				}
				switch (text2)
				{
				case "Item":
				case "Command":
				case "CheckBox":
					break;
				default:
					continue;
				}
			}
			bitmap = null;
			if (codon.Properties.Contains("icon"))
			{
				try
				{
					bitmap = ResourceService.GetBitmap(codon.Properties["icon"]);
				}
				catch (ResourceNotFoundException)
				{
				}
			}
			text = StringParser.Parse(codon.Properties["label"]);
			string shortcutId = codon.ShortcutId;
			if (!codonsTextAndImages.ContainsKey(shortcutId))
			{
				codonsTextAndImages.Add(shortcutId, new KeyValuePair<string, Bitmap>(text, bitmap));
			}
		}
		foreach (AddInTreeNode value in addinNode.ChildNodes.Values)
		{
			LoadCodonList(value);
		}
	}

	public void LoadData()
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode(null, throwOnNotFound: false);
		LoadCodonList(treeNode);
		foreach (KeyValuePair<string, KeyValuePair<string, Bitmap>> codonsTextAndImage in codonsTextAndImages)
		{
			if (!MenuShortcutService.SetShortcutTextAndImage(codonsTextAndImage.Key, codonsTextAndImage.Value.Key, codonsTextAndImage.Value.Value))
			{
				_ = codonsTextAndImage.Key;
			}
		}
		if (codonsTextAndImages.Count > 0)
		{
			MenuShortcutService.ValidateCommands(new List<string>(codonsTextAndImages.Keys));
		}
		data = MenuShortcutService.GetCommandsCopy();
		commandList.Model = new TreeListAdapter(data);
	}

	public void UpdateData()
	{
		MenuShortcutService.SetCommandsValues(data.ToArray());
	}

	private void commandIconNode_ValueNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag != null)
		{
			e.Value = (e.Node.Tag as MenuShortcutService.CommandShortcut).Image;
		}
	}

	private void commandNameNode_ValueNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag != null)
		{
			e.Value = (e.Node.Tag as MenuShortcutService.CommandShortcut).Text;
		}
	}

	private void originalShortcutNode_ValueNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag != null)
		{
			e.Value = (e.Node.Tag as MenuShortcutService.CommandShortcut).OriginalShortcutKeys;
		}
	}

	private void nodeGenericValue1_ValueNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag != null)
		{
			e.Value = (e.Node.Tag as MenuShortcutService.CommandShortcut).NewShortcutKeys;
		}
	}

	private void nodeGenericValue1_ValuePushed(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag != null)
		{
			(e.Node.Tag as MenuShortcutService.CommandShortcut).NewShortcutKeys = e.Value.ToString();
		}
	}

	public string GetToolTip(TreeNodeAdv node, NodeControl nodeControl)
	{
		if (node != null && node.Tag is MenuShortcutService.CommandShortcut commandShortcut)
		{
			return commandShortcut.CodonId;
		}
		return string.Empty;
	}

	private void commandList_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
	{
		if (e.Node.Tag != null)
		{
			MenuShortcutService.CommandShortcut commandShortcut = e.Node.Tag as MenuShortcutService.CommandShortcut;
			string title = commandShortcut.Text;
			string shortcutString = commandShortcut.NewShortcutKeys;
			if (string.IsNullOrEmpty(shortcutString))
			{
				shortcutString = commandShortcut.OriginalShortcutKeys;
			}
			string text = shortcutString;
			if (ShortcutEditorForm.EditKey(title, ref shortcutString) && !text.Equals(shortcutString, StringComparison.OrdinalIgnoreCase))
			{
				MenuShortcutService.ChangeCommandShortcut(commandShortcut.CodonId, shortcutString);
				commandShortcut.NewShortcutKeys = shortcutString;
			}
		}
	}

	private void buttonRestoreAll_Click(object sender, EventArgs e)
	{
		if (MenuShortcutService.UseFullName != checkBoxUseFullName.Checked)
		{
			MenuShortcutService.UseFullName = checkBoxUseFullName.Checked;
		}
		else
		{
			MenuShortcutService.Restore();
		}
		LoadData();
	}

	private void commandList_SelectionChanged(object sender, EventArgs e)
	{
		if (commandList.SelectedNode != null)
		{
			MenuShortcutService.CommandShortcut commandShortcut = commandList.SelectedNode.Tag as MenuShortcutService.CommandShortcut;
			labelCodonId.Text = commandShortcut.CodonId;
		}
		else
		{
			labelCodonId.Text = "";
		}
	}

	private void checkBoxUseFullName_CheckedChanged(object sender, EventArgs e)
	{
	}

	private void InitializeComponent()
	{
		this.commandList = new ICSharpCode.SharpDevelop.Gui.TreeViewAdvBase();
		this.commandIconColumn = new Aga.Controls.Tree.TreeColumn();
		this.commandNameColumn = new Aga.Controls.Tree.TreeColumn();
		this.commandOriginalShortcutColumn = new Aga.Controls.Tree.TreeColumn();
		this.commandNewShortcutColumn = new Aga.Controls.Tree.TreeColumn();
		this.commandIconNode = new Aga.Controls.Tree.NodeControls.NodeIcon();
		this.commandNameNode = new Aga.Controls.Tree.NodeControls.NodeTextBox();
		this.originalShortcutNode = new Aga.Controls.Tree.NodeControls.NodeTextBox();
		this.newShortcutNode = new Aga.Controls.Tree.NodeControls.NodeTextBox();
		this.label1 = new Aga.Controls.Tree.Locator();
		this.buttonRestoreAll = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.labelCodonId = new System.Windows.Forms.Label();
		this.checkBoxUseFullName = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.commandList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.commandList.BackColor = System.Drawing.SystemColors.Window;
		this.commandList.Columns.Add(this.commandIconColumn);
		this.commandList.Columns.Add(this.commandNameColumn);
		this.commandList.Columns.Add(this.commandOriginalShortcutColumn);
		this.commandList.Columns.Add(this.commandNewShortcutColumn);
		this.commandList.DefaultToolTipProvider = null;
		this.commandList.DragDropMarkColor = System.Drawing.Color.Black;
		this.commandList.FullRowSelect = true;
		this.commandList.GoToLastWhenClickBelowLast = false;
		this.commandList.GridLineStyle = Aga.Controls.Tree.GridLineStyle.HorizontalAndVertical;
		this.commandList.InactiveRowColor = System.Drawing.SystemColors.ControlLight;
		this.commandList.LineColor = System.Drawing.SystemColors.ControlDark;
		this.commandList.Location = new System.Drawing.Point(3, 67);
		this.commandList.Model = null;
		this.commandList.Name = "commandList";
		this.commandList.NodeControls.Add(this.commandIconNode);
		this.commandList.NodeControls.Add(this.commandNameNode);
		this.commandList.NodeControls.Add(this.originalShortcutNode);
		this.commandList.NodeControls.Add(this.newShortcutNode);
		this.commandList.SelectedNode = null;
		this.commandList.SelectedRowColor = System.Drawing.SystemColors.Highlight;
		this.commandList.ShowLines = false;
		this.commandList.ShowNodeToolTips = true;
		this.commandList.ShowPlusMinus = false;
		this.commandList.Size = new System.Drawing.Size(437, 210);
		this.commandList.TabIndex = 4;
		this.commandList.UseColumns = true;
		this.commandList.NodeMouseDoubleClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(commandList_NodeMouseDoubleClick);
		this.commandList.SelectionChanged += new System.EventHandler(commandList_SelectionChanged);
		this.commandIconColumn.Header = "";
		this.commandIconColumn.SortOrder = System.Windows.Forms.SortOrder.None;
		this.commandIconColumn.TooltipText = null;
		this.commandIconColumn.Width = 30;
		this.commandNameColumn.Header = "Command Name";
		this.commandNameColumn.Sortable = true;
		this.commandNameColumn.SortOrder = System.Windows.Forms.SortOrder.Ascending;
		this.commandNameColumn.TooltipText = null;
		this.commandNameColumn.Width = 150;
		this.commandOriginalShortcutColumn.Header = "Original";
		this.commandOriginalShortcutColumn.SortOrder = System.Windows.Forms.SortOrder.None;
		this.commandOriginalShortcutColumn.TooltipText = null;
		this.commandOriginalShortcutColumn.Width = 122;
		this.commandNewShortcutColumn.Header = "User Defined";
		this.commandNewShortcutColumn.SortOrder = System.Windows.Forms.SortOrder.None;
		this.commandNewShortcutColumn.TooltipText = null;
		this.commandNewShortcutColumn.Width = 122;
		this.commandIconNode.LeftMargin = 1;
		this.commandIconNode.ParentColumn = this.commandIconColumn;
		this.commandIconNode.VirtualMode = true;
		this.commandNameNode.EditEnabled = false;
		this.commandNameNode.IncrementalSearchEnabled = true;
		this.commandNameNode.LeftMargin = 3;
		this.commandNameNode.ParentColumn = this.commandNameColumn;
		this.commandNameNode.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
		this.commandNameNode.VirtualMode = true;
		this.originalShortcutNode.EditEnabled = false;
		this.originalShortcutNode.IncrementalSearchEnabled = true;
		this.originalShortcutNode.LeftMargin = 3;
		this.originalShortcutNode.ParentColumn = this.commandOriginalShortcutColumn;
		this.originalShortcutNode.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
		this.originalShortcutNode.VirtualMode = true;
		this.newShortcutNode.EditEnabled = false;
		this.newShortcutNode.IncrementalSearchEnabled = true;
		this.newShortcutNode.LeftMargin = 3;
		this.newShortcutNode.ParentColumn = this.commandNewShortcutColumn;
		this.newShortcutNode.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
		this.newShortcutNode.VirtualMode = true;
		this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.label1.InString = true;
		this.label1.Location = new System.Drawing.Point(3, 36);
		this.label1.Margin = new System.Windows.Forms.Padding(0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(437, 29);
		this.label1.TabIndex = 5;
		this.label1.TreeToSearch = this.commandList;
		this.buttonRestoreAll.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.buttonRestoreAll.Location = new System.Drawing.Point(13, 361);
		this.buttonRestoreAll.Name = "buttonRestoreAll";
		this.buttonRestoreAll.Size = new System.Drawing.Size(118, 23);
		this.buttonRestoreAll.TabIndex = 6;
		this.buttonRestoreAll.Text = "Restore All";
		this.buttonRestoreAll.UseVisualStyleBackColor = true;
		this.buttonRestoreAll.Click += new System.EventHandler(buttonRestoreAll_Click);
		this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.label2.Location = new System.Drawing.Point(3, 324);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(437, 35);
		this.label2.TabIndex = 7;
		this.label2.Text = "*Note: The IDE needs to be restarted to activate the new Shortcuts.";
		this.labelCodonId.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelCodonId.AutoEllipsis = true;
		this.labelCodonId.BackColor = System.Drawing.SystemColors.ControlDark;
		this.labelCodonId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.labelCodonId.ForeColor = System.Drawing.Color.White;
		this.labelCodonId.Location = new System.Drawing.Point(3, 282);
		this.labelCodonId.Name = "labelCodonId";
		this.labelCodonId.Size = new System.Drawing.Size(437, 39);
		this.labelCodonId.TabIndex = 8;
		this.labelCodonId.Text = "None";
		this.checkBoxUseFullName.AutoSize = true;
		this.checkBoxUseFullName.Location = new System.Drawing.Point(6, 11);
		this.checkBoxUseFullName.Name = "checkBoxUseFullName";
		this.checkBoxUseFullName.Size = new System.Drawing.Size(288, 21);
		this.checkBoxUseFullName.TabIndex = 9;
		this.checkBoxUseFullName.Text = "Use full name as ID ( Parent Id + Item Id )";
		this.checkBoxUseFullName.UseVisualStyleBackColor = true;
		this.checkBoxUseFullName.CheckedChanged += new System.EventHandler(checkBoxUseFullName_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.Controls.Add(this.checkBoxUseFullName);
		base.Controls.Add(this.commandList);
		base.Controls.Add(this.labelCodonId);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.buttonRestoreAll);
		base.Controls.Add(this.label1);
		base.Name = "ShortcutEditorPanel";
		base.Size = new System.Drawing.Size(445, 391);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

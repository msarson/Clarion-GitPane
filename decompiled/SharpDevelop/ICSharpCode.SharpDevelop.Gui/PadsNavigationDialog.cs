using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class PadsNavigationDialog : Form
{
	private object sel;

	private IContainer components;

	private TableLayoutPanel tableLayoutPanel;

	private Panel ItemsPanel;

	private Panel DescriptionPanel;

	private Label ActiveFilesLabel;

	private Label ActiveToolsWindowsLabel;

	private Label DescriptionLabel;

	private PathLabel FilePathLabel;

	public PadsNavigationDialog(int selectedViewIndex)
	{
		SuspendLayout();
		InitializeComponent();
		int count = WorkbenchSingleton.Workbench.ViewContentCollection.Count;
		List<PadDescriptor> list = WorkbenchSingleton.Workbench.PadContentCollection.FindAll(VisiblePad);
		int num = Math.Max(count, list.Count);
		tableLayoutPanel.RowCount = num;
		for (int i = 0; i < num; i++)
		{
			tableLayoutPanel.RowStyles.Add(new RowStyle());
		}
		if (count > 0)
		{
			if (selectedViewIndex > count)
			{
				selectedViewIndex = 0;
			}
			for (int j = 0; j < count; j++)
			{
				RadioButton newRadioButton = GetNewRadioButton(WorkbenchSingleton.Workbench.ViewContentCollection[j]);
				tableLayoutPanel.Controls.Add(newRadioButton, 1, j);
				if (selectedViewIndex == j)
				{
					newRadioButton.Checked = true;
					newRadioButton.Select();
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			RadioButton newRadioButton2 = GetNewRadioButton(list[k]);
			tableLayoutPanel.Controls.Add(newRadioButton2, 0, k);
			if (count == 0 && k == 0)
			{
				newRadioButton2.Checked = true;
				newRadioButton2.Select();
			}
		}
		ResumeLayout(performLayout: false);
		if (num < 12)
		{
			base.Location = new Point(base.Location.X, base.Location.Y - num * 24 / 2);
			base.ClientSize = new Size(381, 180 + num * 24);
		}
		else
		{
			base.Location = new Point(base.Location.X, base.Location.Y - 144);
			base.ClientSize = new Size(381, 468);
		}
	}

	internal static bool VisiblePad(PadDescriptor pd)
	{
		return WorkbenchSingleton.Workbench.WorkbenchLayout.IsVisible(pd);
	}

	private RadioButton GetNewRadioButton(object o, string text, Image i)
	{
		FocuslessRadioButton focuslessRadioButton = new FocuslessRadioButton();
		focuslessRadioButton.Appearance = Appearance.Button;
		focuslessRadioButton.AutoEllipsis = true;
		focuslessRadioButton.FlatAppearance.BorderColor = SystemColors.GradientInactiveCaption;
		focuslessRadioButton.FlatAppearance.BorderSize = 0;
		focuslessRadioButton.FlatAppearance.CheckedBackColor = SystemColors.GradientInactiveCaption;
		focuslessRadioButton.FlatAppearance.MouseDownBackColor = SystemColors.GradientInactiveCaption;
		focuslessRadioButton.FlatAppearance.MouseOverBackColor = SystemColors.Control;
		focuslessRadioButton.FlatStyle = FlatStyle.Flat;
		focuslessRadioButton.Location = new Point(0, 0);
		focuslessRadioButton.Margin = new Padding(0, 0, 0, 0);
		focuslessRadioButton.Name = text;
		focuslessRadioButton.Tag = o;
		focuslessRadioButton.Size = new Size(162, 24);
		focuslessRadioButton.TabStop = false;
		focuslessRadioButton.Text = text;
		focuslessRadioButton.Font = new Font("Tahoma", 7.5f, FontStyle.Regular);
		focuslessRadioButton.Cursor = Cursors.Hand;
		focuslessRadioButton.TextImageRelation = TextImageRelation.ImageBeforeText;
		focuslessRadioButton.UseVisualStyleBackColor = true;
		focuslessRadioButton.Dock = DockStyle.Fill;
		if (i != null)
		{
			focuslessRadioButton.Image = i;
			focuslessRadioButton.ImageAlign = ContentAlignment.MiddleLeft;
			focuslessRadioButton.TextAlign = ContentAlignment.MiddleLeft;
		}
		focuslessRadioButton.PreviewKeyDown += PadsNavForm_PreviewKeyDown;
		focuslessRadioButton.KeyUp += PadsNavForm_KeyUp;
		focuslessRadioButton.CheckedChanged += vcRadioButton_CheckedChanged;
		focuslessRadioButton.Enter += vcRadioButton_Enter;
		focuslessRadioButton.MouseClick += vcRadioButton_MouseClick;
		return focuslessRadioButton;
	}

	private static void vcRadioButton_Enter(object sender, EventArgs e)
	{
		if (sender is RadioButton && !((RadioButton)sender).Checked)
		{
			((RadioButton)sender).Checked = true;
		}
	}

	private void vcRadioButton_MouseClick(object sender, MouseEventArgs e)
	{
		Close();
	}

	private void vcRadioButton_CheckedChanged(object sender, EventArgs e)
	{
		sel = ((RadioButton)sender).Tag;
		if (sel == null)
		{
			return;
		}
		if (sel is PadDescriptor)
		{
			DescriptionLabel.Text = StringParser.Parse(((PadDescriptor)sel).Title);
			FilePathLabel.Text = string.Empty;
			return;
		}
		DescriptionLabel.Text = StringParser.Parse(((IViewContent)sel).TitleName);
		if (!string.IsNullOrEmpty(((IViewContent)sel).FileName))
		{
			FilePathLabel.Text = ((IViewContent)sel).FileName;
		}
		else
		{
			FilePathLabel.Text = string.Empty;
		}
	}

	private RadioButton GetNewRadioButton(PadDescriptor pd)
	{
		if (!string.IsNullOrEmpty(pd.Icon))
		{
			return GetNewRadioButton(pd, StringParser.Parse(pd.Title), IconService.GetIcon(pd.Icon).ToBitmap());
		}
		return GetNewRadioButton(pd, StringParser.Parse(pd.Title), null);
	}

	private RadioButton GetNewRadioButton(IViewContent vc)
	{
		if (!string.IsNullOrEmpty(vc.FileName))
		{
			return GetNewRadioButton(vc, StringParser.Parse(vc.TitleName), IconService.GetIcon(IconService.GetImageForFile(vc.FileName)).ToBitmap());
		}
		return GetNewRadioButton(vc, StringParser.Parse(vc.TitleName), null);
	}

	private void PadsNavForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (!(sender is RadioButton) || !e.Control)
		{
			return;
		}
		if (e.KeyCode == Keys.Tab)
		{
			if (e.Shift)
			{
				int row = tableLayoutPanel.GetRow((Control)sender);
				int column = tableLayoutPanel.GetColumn((Control)sender);
				if (SelectNextControlInColumn(row, column, forward: false))
				{
					ProcessDialogKey(Keys.None);
				}
				else
				{
					ProcessDialogKey(Keys.Up);
				}
			}
			else
			{
				int row2 = tableLayoutPanel.GetRow((Control)sender);
				int column2 = tableLayoutPanel.GetColumn((Control)sender);
				if (SelectNextControlInColumn(row2, column2, forward: true))
				{
					ProcessDialogKey(Keys.None);
				}
				else
				{
					ProcessDialogKey(Keys.Down);
				}
			}
		}
		else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
		{
			ProcessDialogKey(e.KeyCode);
		}
		else
		{
			if (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
			{
				return;
			}
			int row3 = tableLayoutPanel.GetRow((Control)sender);
			int column3 = ((tableLayoutPanel.GetColumn((Control)sender) == 0) ? 1 : 0);
			object controlFromPosition = tableLayoutPanel.GetControlFromPosition(column3, row3);
			if (controlFromPosition == null)
			{
				for (int num = row3 - 1; num >= 0; num--)
				{
					controlFromPosition = tableLayoutPanel.GetControlFromPosition(column3, num);
					if (controlFromPosition != null)
					{
						break;
					}
				}
			}
			if (controlFromPosition != null)
			{
				((Control)controlFromPosition).Select();
				ProcessDialogKey(Keys.None);
			}
		}
	}

	private bool SelectNextControlInColumn(int row, int col, bool forward)
	{
		int rowCount = tableLayoutPanel.RowCount;
		object obj = null;
		if (forward)
		{
			if (row == rowCount - 1)
			{
				row = -1;
			}
			else if (tableLayoutPanel.GetControlFromPosition(col, row + 1) == null)
			{
				row = -1;
			}
			obj = tableLayoutPanel.GetControlFromPosition(col, row + 1);
		}
		else
		{
			if (row == 0)
			{
				row = rowCount;
			}
			for (int num = row - 1; num >= 0; num--)
			{
				obj = tableLayoutPanel.GetControlFromPosition(col, num);
				if (obj != null)
				{
					break;
				}
			}
		}
		if (obj != null)
		{
			((Control)obj).Select();
		}
		return obj != null;
	}

	private void PadsNavForm_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
		{
			Close();
		}
	}

	internal void ExecAction()
	{
		if (sel != null)
		{
			if (sel is PadDescriptor)
			{
				WorkbenchSingleton.MainForm.Activate();
				((PadDescriptor)sel).BringPadToFront();
				WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad((PadDescriptor)sel);
			}
			else if (sel is IViewContent)
			{
				((IViewContent)sel).WorkbenchWindow.SelectWindow();
			}
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
		this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.ItemsPanel = new System.Windows.Forms.Panel();
		this.DescriptionPanel = new System.Windows.Forms.Panel();
		this.FilePathLabel = new ICSharpCode.SharpDevelop.Gui.PathLabel();
		this.DescriptionLabel = new System.Windows.Forms.Label();
		this.ActiveFilesLabel = new System.Windows.Forms.Label();
		this.ActiveToolsWindowsLabel = new System.Windows.Forms.Label();
		this.ItemsPanel.SuspendLayout();
		this.DescriptionPanel.SuspendLayout();
		base.SuspendLayout();
		this.tableLayoutPanel.AutoScroll = true;
		this.tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.tableLayoutPanel.ColumnCount = 2;
		this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
		this.tableLayoutPanel.Name = "tableLayoutPanel";
		this.tableLayoutPanel.RowCount = 1;
		this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel.Size = new System.Drawing.Size(354, 49);
		this.tableLayoutPanel.TabIndex = 10;
		this.ItemsPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.ItemsPanel.Controls.Add(this.tableLayoutPanel);
		this.ItemsPanel.Location = new System.Drawing.Point(12, 41);
		this.ItemsPanel.Margin = new System.Windows.Forms.Padding(0);
		this.ItemsPanel.Name = "ItemsPanel";
		this.ItemsPanel.Size = new System.Drawing.Size(354, 49);
		this.ItemsPanel.TabIndex = 11;
		this.DescriptionPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.DescriptionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.DescriptionPanel.Controls.Add(this.FilePathLabel);
		this.DescriptionPanel.Controls.Add(this.DescriptionLabel);
		this.DescriptionPanel.Location = new System.Drawing.Point(12, 102);
		this.DescriptionPanel.Margin = new System.Windows.Forms.Padding(4);
		this.DescriptionPanel.Name = "DescriptionPanel";
		this.DescriptionPanel.Size = new System.Drawing.Size(352, 82);
		this.DescriptionPanel.TabIndex = 12;
		this.FilePathLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.FilePathLabel.Font = new System.Drawing.Font("Tahoma", 7.5f);
		this.FilePathLabel.Location = new System.Drawing.Point(8, 44);
		this.FilePathLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.FilePathLabel.Name = "FilePathLabel";
		this.FilePathLabel.Size = new System.Drawing.Size(336, 22);
		this.FilePathLabel.TabIndex = 0;
		this.FilePathLabel.UseCompatibleTextRendering = true;
		this.DescriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.DescriptionLabel.AutoEllipsis = true;
		this.DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 7.5f, System.Drawing.FontStyle.Bold);
		this.DescriptionLabel.Location = new System.Drawing.Point(8, 12);
		this.DescriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.DescriptionLabel.Name = "DescriptionLabel";
		this.DescriptionLabel.Size = new System.Drawing.Size(336, 22);
		this.DescriptionLabel.TabIndex = 0;
		this.ActiveFilesLabel.Font = new System.Drawing.Font("Tahoma", 7.5f, System.Drawing.FontStyle.Bold);
		this.ActiveFilesLabel.Location = new System.Drawing.Point(192, 12);
		this.ActiveFilesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.ActiveFilesLabel.Name = "ActiveFilesLabel";
		this.ActiveFilesLabel.Size = new System.Drawing.Size(172, 18);
		this.ActiveFilesLabel.TabIndex = 14;
		this.ActiveFilesLabel.Text = "Active Files";
		this.ActiveFilesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.ActiveToolsWindowsLabel.Font = new System.Drawing.Font("Tahoma", 7.5f, System.Drawing.FontStyle.Bold);
		this.ActiveToolsWindowsLabel.Location = new System.Drawing.Point(13, 12);
		this.ActiveToolsWindowsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.ActiveToolsWindowsLabel.Name = "ActiveToolsWindowsLabel";
		this.ActiveToolsWindowsLabel.Size = new System.Drawing.Size(171, 18);
		this.ActiveToolsWindowsLabel.TabIndex = 13;
		this.ActiveToolsWindowsLabel.Text = "Active Tool Windows";
		this.ActiveToolsWindowsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.ClientSize = new System.Drawing.Size(379, 197);
		base.ControlBox = false;
		base.Controls.Add(this.DescriptionPanel);
		base.Controls.Add(this.ActiveToolsWindowsLabel);
		base.Controls.Add(this.ActiveFilesLabel);
		base.Controls.Add(this.ItemsPanel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.KeyPreview = true;
		base.Margin = new System.Windows.Forms.Padding(4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "PadsNavigationDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.ItemsPanel.ResumeLayout(false);
		this.DescriptionPanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace ICSharpCode.SharpDevelop.Gui;

public class SelectProcessIdDialog : PositionedForm
{
	public class ListViewItemComparer : IComparer
	{
		private int col;

		private SortOrder order;

		public ListViewItemComparer()
		{
			col = 0;
			order = SortOrder.Ascending;
		}

		public ListViewItemComparer(int column, SortOrder order)
		{
			col = column;
			this.order = order;
		}

		public int Compare(object x, object y)
		{
			int num = -1;
			num = string.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
			if (order == SortOrder.Descending)
			{
				num *= -1;
			}
			return num;
		}
	}

	private int sortColumn = -1;

	private int m_nProcessId = -1;

	private IContainer components;

	private ListView listProcess;

	private Button butOK;

	private Button butCancel;

	private ColumnHeader PID;

	private ColumnHeader ProcessName;

	private ColumnHeader Time;

	private ColumnHeader MainWindowTitle;

	private CheckBox checkBoxwithWindows;

	public int SelectedProcessId => m_nProcessId;

	public SelectProcessIdDialog()
	{
		InitializeComponent();
		checkBoxwithWindows.Checked = true;
	}

	private void ProcessDlg_Load(object sender, EventArgs e)
	{
		LoadListView();
	}

	private void LoadListView()
	{
		listProcess.Items.Clear();
		Process[] processes = Process.GetProcesses();
		bool flag = true;
		Process[] array = processes;
		foreach (Process process in array)
		{
			try
			{
				if ((checkBoxwithWindows.Checked && !string.IsNullOrEmpty(process.MainWindowTitle)) || !checkBoxwithWindows.Checked)
				{
					ListViewItem listViewItem = new ListViewItem(new string[4]
					{
						process.ProcessName,
						process.MainWindowTitle,
						process.StartTime.ToShortTimeString(),
						process.Id.ToString()
					});
					listViewItem.Tag = process.Id;
					listProcess.Items.Add(listViewItem);
					if (flag)
					{
						listViewItem.Selected = true;
						flag = false;
					}
				}
			}
			catch (Exception)
			{
			}
		}
		listProcess.SetSortIcon(0, listProcess.Sorting);
	}

	private void SetSelectedProcessId()
	{
		try
		{
			m_nProcessId = (int)listProcess.SelectedItems[0].Tag;
		}
		catch (Exception)
		{
			m_nProcessId = -1;
		}
	}

	public static int GetProcessId()
	{
		using (SelectProcessIdDialog selectProcessIdDialog = new SelectProcessIdDialog())
		{
			if (selectProcessIdDialog.ShowDialog() == DialogResult.OK)
			{
				return selectProcessIdDialog.SelectedProcessId;
			}
		}
		return -1;
	}

	private void butOK_Click(object sender, EventArgs e)
	{
		SetSelectedProcessId();
	}

	private void listProcess_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		SetSelectedProcessId();
		if (m_nProcessId > 0)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void listProcess_ColumnClick(object sender, ColumnClickEventArgs e)
	{
		if (e.Column != sortColumn)
		{
			sortColumn = e.Column;
			listProcess.Sorting = SortOrder.Ascending;
		}
		else if (listProcess.Sorting == SortOrder.Ascending)
		{
			listProcess.Sorting = SortOrder.Descending;
		}
		else
		{
			listProcess.Sorting = SortOrder.Ascending;
		}
		listProcess.Sort();
		listProcess.ListViewItemSorter = new ListViewItemComparer(e.Column, listProcess.Sorting);
		listProcess.SetSortIcon(e.Column, listProcess.Sorting);
	}

	private void checkBoxwithWindows_CheckedChanged(object sender, EventArgs e)
	{
		LoadListView();
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
		this.listProcess = new System.Windows.Forms.ListView();
		this.ProcessName = new System.Windows.Forms.ColumnHeader();
		this.MainWindowTitle = new System.Windows.Forms.ColumnHeader();
		this.Time = new System.Windows.Forms.ColumnHeader();
		this.PID = new System.Windows.Forms.ColumnHeader();
		this.butOK = new System.Windows.Forms.Button();
		this.butCancel = new System.Windows.Forms.Button();
		this.checkBoxwithWindows = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.listProcess.Activation = System.Windows.Forms.ItemActivation.OneClick;
		this.listProcess.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.listProcess.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.ProcessName, this.MainWindowTitle, this.Time, this.PID });
		this.listProcess.FullRowSelect = true;
		this.listProcess.GridLines = true;
		this.listProcess.Location = new System.Drawing.Point(13, 13);
		this.listProcess.Margin = new System.Windows.Forms.Padding(2);
		this.listProcess.MultiSelect = false;
		this.listProcess.Name = "listProcess";
		this.listProcess.Size = new System.Drawing.Size(483, 339);
		this.listProcess.Sorting = System.Windows.Forms.SortOrder.Ascending;
		this.listProcess.TabIndex = 1;
		this.listProcess.UseCompatibleStateImageBehavior = false;
		this.listProcess.View = System.Windows.Forms.View.Details;
		this.listProcess.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(listProcess_ColumnClick);
		this.listProcess.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(listProcess_MouseDoubleClick);
		this.ProcessName.Text = "Process Name";
		this.ProcessName.Width = 113;
		this.MainWindowTitle.Text = "Window Title";
		this.MainWindowTitle.Width = 201;
		this.Time.Text = "Start Time";
		this.Time.Width = 102;
		this.PID.Text = "PID";
		this.PID.Width = 56;
		this.butOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.butOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.butOK.Location = new System.Drawing.Point(304, 368);
		this.butOK.Margin = new System.Windows.Forms.Padding(2);
		this.butOK.Name = "butOK";
		this.butOK.Size = new System.Drawing.Size(87, 29);
		this.butOK.TabIndex = 1;
		this.butOK.Text = "OK";
		this.butOK.UseVisualStyleBackColor = true;
		this.butOK.Click += new System.EventHandler(butOK_Click);
		this.butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.butCancel.Location = new System.Drawing.Point(409, 368);
		this.butCancel.Margin = new System.Windows.Forms.Padding(2);
		this.butCancel.Name = "butCancel";
		this.butCancel.Size = new System.Drawing.Size(87, 29);
		this.butCancel.TabIndex = 2;
		this.butCancel.Text = "Cancel";
		this.butCancel.UseVisualStyleBackColor = true;
		this.checkBoxwithWindows.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.checkBoxwithWindows.AutoSize = true;
		this.checkBoxwithWindows.Location = new System.Drawing.Point(16, 368);
		this.checkBoxwithWindows.Name = "checkBoxwithWindows";
		this.checkBoxwithWindows.Size = new System.Drawing.Size(202, 19);
		this.checkBoxwithWindows.TabIndex = 3;
		this.checkBoxwithWindows.Text = "Show only process with Windows";
		this.checkBoxwithWindows.UseVisualStyleBackColor = true;
		this.checkBoxwithWindows.CheckedChanged += new System.EventHandler(checkBoxwithWindows_CheckedChanged);
		base.AcceptButton = this.butOK;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.butCancel;
		base.ClientSize = new System.Drawing.Size(509, 412);
		base.Controls.Add(this.checkBoxwithWindows);
		base.Controls.Add(this.butCancel);
		base.Controls.Add(this.butOK);
		base.Controls.Add(this.listProcess);
		base.Margin = new System.Windows.Forms.Padding(2);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SelectProcessIdDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.Text = "Select Process";
		base.Load += new System.EventHandler(ProcessDlg_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

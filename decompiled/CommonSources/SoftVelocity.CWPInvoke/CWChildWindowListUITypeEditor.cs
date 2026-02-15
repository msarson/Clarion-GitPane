using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SoftVelocity.CWPInvoke;

internal class CWChildWindowListUITypeEditor : UITypeEditor
{
	private class ControlComp : Comparer<CWChildWindow>
	{
		public override int Compare(CWChildWindow x, CWChildWindow y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}

	private class ControlsListForm : Form
	{
		private class ControlDisplay
		{
			private CWChildWindow c;

			public ControlDisplay(CWChildWindow c)
			{
				this.c = c;
			}

			public override string ToString()
			{
				return c.Name;
			}

			public static implicit operator CWChildWindow(ControlDisplay cd)
			{
				return cd.c;
			}

			public static implicit operator ControlDisplay(CWChildWindow cd)
			{
				return new ControlDisplay(cd);
			}

			public static bool operator >(ControlDisplay x, ControlDisplay y)
			{
				if (x.c.Name.CompareTo(y.c.Name) > 0)
				{
					return true;
				}
				return false;
			}

			public static bool operator <(ControlDisplay x, ControlDisplay y)
			{
				if (x.c.Name.CompareTo(y.c.Name) < 0)
				{
					return true;
				}
				return false;
			}
		}

		private List<CWChildWindow> _sourceControls = new List<CWChildWindow>();

		private List<CWChildWindow> _selectedControls = new List<CWChildWindow>();

		private IContainer components;

		private ListBox sourceControlsList;

		private ListBox selectedControlsList;

		private Button buttonRemove;

		private Button buttonAdd;

		private Button buttonOk;

		private Button buttonCancel;

		private SplitContainer splitContainer1;

		private SplitContainer splitContainer2;

		private Label labelSelected;

		private Label labelSource;

		public CWChildWindow[] SourceControls
		{
			get
			{
				return _sourceControls.ToArray();
			}
			set
			{
				_sourceControls.Clear();
				_sourceControls.AddRange(value);
			}
		}

		public CWChildWindow[] SelectedControls
		{
			get
			{
				return _selectedControls.ToArray();
			}
			set
			{
				_selectedControls.Clear();
				_selectedControls.AddRange(value);
			}
		}

		public ControlsListForm()
		{
			InitializeComponent();
		}

		private void ControlsList_Load(object sender, EventArgs e)
		{
			if (_sourceControls == null)
			{
				return;
			}
			sourceControlsList.Items.Clear();
			selectedControlsList.Items.Clear();
			foreach (CWChildWindow sourceControl in _sourceControls)
			{
				if (!_selectedControls.Contains(sourceControl))
				{
					sourceControlsList.Items.Add(new ControlDisplay(sourceControl));
				}
				else
				{
					selectedControlsList.Items.Add(new ControlDisplay(sourceControl));
				}
			}
			if (selectedControlsList.Items.Count > 0)
			{
				selectedControlsList.SelectedIndex = 0;
			}
			if (sourceControlsList.Items.Count > 0)
			{
				sourceControlsList.SelectedIndex = 0;
			}
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			AddSelected();
		}

		private void AddSelected()
		{
			if (sourceControlsList.SelectedItems.Count == 0)
			{
				return;
			}
			int num = sourceControlsList.SelectedIndices[0];
			object obj = sourceControlsList.SelectedItems[0];
			sourceControlsList.Items.Remove(obj);
			selectedControlsList.SelectedIndex = selectedControlsList.Items.Add(obj);
			if (sourceControlsList.Items.Count > 0)
			{
				if (num == sourceControlsList.Items.Count)
				{
					sourceControlsList.SelectedIndex = num - 1;
				}
				else
				{
					sourceControlsList.SelectedIndex = num;
				}
			}
		}

		private void buttonRemove_Click(object sender, EventArgs e)
		{
			RemoveSelected();
		}

		private void RemoveSelected()
		{
			if (selectedControlsList.SelectedItems.Count == 0)
			{
				return;
			}
			int num = selectedControlsList.SelectedIndices[0];
			object obj = selectedControlsList.SelectedItems[0];
			selectedControlsList.Items.Remove(obj);
			sourceControlsList.SelectedIndex = sourceControlsList.Items.Add(obj);
			if (selectedControlsList.Items.Count > 0)
			{
				if (num == selectedControlsList.Items.Count)
				{
					selectedControlsList.SelectedIndex = num - 1;
				}
				else
				{
					selectedControlsList.SelectedIndex = num;
				}
			}
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			_selectedControls.Clear();
			foreach (object item2 in selectedControlsList.Items)
			{
				CWChildWindow item = (ControlDisplay)item2;
				_selectedControls.Add(item);
			}
			base.DialogResult = DialogResult.OK;
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
		}

		private void sourceControlsList_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && sourceControlsList.Items.Count > 0 && sourceControlsList.GetItemRectangle(sourceControlsList.SelectedIndex).Contains(e.Location))
			{
				AddSelected();
			}
		}

		private void selectedControlsList_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && selectedControlsList.Items.Count > 0 && selectedControlsList.GetItemRectangle(selectedControlsList.SelectedIndex).Contains(e.Location))
			{
				RemoveSelected();
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
			this.sourceControlsList = new System.Windows.Forms.ListBox();
			this.selectedControlsList = new System.Windows.Forms.ListBox();
			this.buttonRemove = new System.Windows.Forms.Button();
			this.buttonAdd = new System.Windows.Forms.Button();
			this.buttonOk = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.labelSource = new System.Windows.Forms.Label();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.labelSelected = new System.Windows.Forms.Label();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			base.SuspendLayout();
			this.sourceControlsList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.sourceControlsList.Location = new System.Drawing.Point(0, 26);
			this.sourceControlsList.Name = "sourceControlsList";
			this.sourceControlsList.Size = new System.Drawing.Size(323, 277);
			this.sourceControlsList.Sorted = true;
			this.sourceControlsList.TabIndex = 0;
			this.sourceControlsList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(sourceControlsList_MouseDoubleClick);
			this.selectedControlsList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.selectedControlsList.Location = new System.Drawing.Point(2, 26);
			this.selectedControlsList.Name = "selectedControlsList";
			this.selectedControlsList.Size = new System.Drawing.Size(340, 277);
			this.selectedControlsList.Sorted = true;
			this.selectedControlsList.TabIndex = 1;
			this.selectedControlsList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(selectedControlsList_MouseDoubleClick);
			this.buttonRemove.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonRemove.Location = new System.Drawing.Point(12, 126);
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.Size = new System.Drawing.Size(37, 23);
			this.buttonRemove.TabIndex = 2;
			this.buttonRemove.Text = "<-";
			this.buttonRemove.UseVisualStyleBackColor = true;
			this.buttonRemove.Click += new System.EventHandler(buttonRemove_Click);
			this.buttonAdd.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonAdd.Location = new System.Drawing.Point(12, 76);
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.Size = new System.Drawing.Size(37, 23);
			this.buttonAdd.TabIndex = 3;
			this.buttonAdd.Text = "->";
			this.buttonAdd.UseVisualStyleBackColor = true;
			this.buttonAdd.Click += new System.EventHandler(buttonAdd_Click);
			this.buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.buttonOk.Location = new System.Drawing.Point(585, 330);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(75, 23);
			this.buttonOk.TabIndex = 4;
			this.buttonOk.Text = "Ok";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new System.EventHandler(buttonOk_Click);
			this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(672, 329);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "&Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
			this.splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = new System.Drawing.Point(12, 12);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Panel1.Controls.Add(this.labelSource);
			this.splitContainer1.Panel1.Controls.Add(this.sourceControlsList);
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(735, 311);
			this.splitContainer1.SplitterDistance = 325;
			this.splitContainer1.TabIndex = 7;
			this.labelSource.AutoSize = true;
			this.labelSource.Location = new System.Drawing.Point(0, 0);
			this.labelSource.Name = "labelSource";
			this.labelSource.Size = new System.Drawing.Size(91, 13);
			this.labelSource.TabIndex = 8;
			this.labelSource.Text = "Available Controls";
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.IsSplitterFixed = true;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Panel1.Controls.Add(this.buttonAdd);
			this.splitContainer2.Panel1.Controls.Add(this.buttonRemove);
			this.splitContainer2.Panel2.Controls.Add(this.labelSelected);
			this.splitContainer2.Panel2.Controls.Add(this.selectedControlsList);
			this.splitContainer2.Size = new System.Drawing.Size(406, 311);
			this.splitContainer2.SplitterDistance = 60;
			this.splitContainer2.TabIndex = 0;
			this.labelSelected.AutoSize = true;
			this.labelSelected.Location = new System.Drawing.Point(3, 0);
			this.labelSelected.Name = "labelSelected";
			this.labelSelected.Size = new System.Drawing.Size(90, 13);
			this.labelSelected.TabIndex = 9;
			this.labelSelected.Text = "Selected Controls";
			base.AcceptButton = this.buttonOk;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.buttonCancel;
			base.ClientSize = new System.Drawing.Size(759, 366);
			base.Controls.Add(this.splitContainer1);
			base.Controls.Add(this.buttonCancel);
			base.Controls.Add(this.buttonOk);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			base.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(767, 392);
			base.Name = "ControlsListForm";
			base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select Controls";
			base.Load += new System.EventHandler(ControlsList_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			this.splitContainer2.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	protected virtual CWChildWindow[] FillSourceList(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		List<CWChildWindow> list = new List<CWChildWindow>();
		object instance = context.Instance;
		CWChildWindow cWChildWindow = null;
		foreach (Component component in context.Container.Components)
		{
			if (component is CWChildWindow && component != instance && component is CWChildWindow cWChildWindow2 && IsValidControl(cWChildWindow2))
			{
				list.Add(cWChildWindow2);
			}
		}
		return list.ToArray();
	}

	protected virtual bool IsValidControl(CWChildWindow itemToAdd)
	{
		return true;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			using ControlsListForm controlsListForm = new ControlsListForm();
			controlsListForm.SourceControls = FillSourceList(context, provider, value);
			ControlComp comparer = new ControlComp();
			Array.Sort(controlsListForm.SourceControls, comparer);
			if (value != null && value is CWChildWindow[] array)
			{
				List<CWChildWindow> list = new List<CWChildWindow>();
				CWChildWindow[] array2 = array;
				foreach (CWChildWindow cWChildWindow in array2)
				{
					if (IsValidControl(cWChildWindow))
					{
						list.Add(cWChildWindow);
					}
				}
				controlsListForm.SelectedControls = list.ToArray();
				Array.Sort(controlsListForm.SelectedControls, comparer);
			}
			if (windowsFormsEditorService.ShowDialog(controlsListForm) == DialogResult.OK)
			{
				return controlsListForm.SelectedControls;
			}
		}
		return base.EditValue(context, provider, value);
	}
}

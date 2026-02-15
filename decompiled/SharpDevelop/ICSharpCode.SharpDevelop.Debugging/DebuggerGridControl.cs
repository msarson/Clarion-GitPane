using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Widgets.TreeGrid;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.Debugging;

public class DebuggerGridControl : DynamicList
{
	private DynamicTreeRow row;

	private DynamicTreeRow.ChildForm frm;

	private bool isExpanded;

	public bool IsMouseOver
	{
		get
		{
			if (frm != null && !frm.IsDisposed)
			{
				return frm.ClientRectangle.Contains(frm.PointToClient(Control.MousePosition));
			}
			return false;
		}
	}

	public bool AllowClose => !isExpanded;

	public static void AddColumns(IList<DynamicListColumn> columns)
	{
		columns.Add(new DynamicListColumn());
		columns.Add(new DynamicListColumn());
		columns.Add(new DynamicListColumn());
		columns.Add(new DynamicListColumn());
		columns[0].BackgroundBrush = Brushes.White;
		columns[0].BackgroundBrushInactive = Brushes.White;
		columns[0].RowHighlightBrush = null;
		columns[0].AllowGrow = false;
		columns[1].AllowGrow = false;
		columns[1].Width = 18;
		columns[1].ColumnSeperatorColor = Color.Transparent;
		columns[1].ColumnSeperatorColorInactive = Color.Transparent;
		columns[2].AutoSize = true;
		columns[2].MinimumWidth = 75;
		columns[2].ColumnSeperatorColor = Color.White;
		columns[2].ColumnSeperatorColorInactive = Color.FromArgb(172, 168, 153);
		columns[3].AutoSize = true;
		columns[3].MinimumWidth = 75;
	}

	public DebuggerGridControl(DynamicTreeRow row)
	{
		this.row = row;
		BeginUpdate();
		AddColumns(base.Columns);
		base.Rows.Add(row);
		EventHandler<DynamicListEventArgs> value = delegate
		{
			isExpanded = true;
		};
		row.Expanded += value;
		row.Collapsed += delegate
		{
			isExpanded = false;
		};
		CreateControl();
		using (Graphics graphics = CreateGraphics())
		{
			base.Width = GetRequiredWidth(graphics);
		}
		base.Height = row.Height;
		EndUpdate();
	}

	public void ShowForm(TextArea textArea, TextLocation logicTextPos)
	{
		frm = new DynamicTreeRow.ChildForm();
		frm.AllowResizing = false;
		frm.Owner = textArea.FindForm();
		int num = (textArea.Document.GetVisibleLine(logicTextPos.Y) + 1) * textArea.TextView.FontHeight - textArea.VirtualTop.Y;
		Point p = new Point(0, num);
		p = textArea.PointToScreen(p);
		p.X = Control.MousePosition.X - 16;
		p.Y--;
		frm.StartPosition = FormStartPosition.Manual;
		frm.ShowInTaskbar = false;
		frm.Location = p;
		frm.ClientSize = new Size(base.Width + 2, row.Height + 2);
		Dock = DockStyle.Fill;
		frm.Controls.Add(this);
		frm.ShowWindowWithoutActivation = true;
		frm.Show();
		textArea.Click += OnTextAreaClick;
		textArea.KeyDown += OnTextAreaClick;
		frm.ClientSize = new Size(frm.ClientSize.Width, row.Height + 2);
	}

	private void OnTextAreaClick(object sender, EventArgs e)
	{
		((TextArea)sender).KeyDown -= OnTextAreaClick;
		((TextArea)sender).Click -= OnTextAreaClick;
		frm.Close();
	}
}

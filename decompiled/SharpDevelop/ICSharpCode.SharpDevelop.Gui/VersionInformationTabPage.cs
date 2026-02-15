using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class VersionInformationTabPage : UserControl
{
	private ColumnHeader columnHeader3;

	private ListView listView;

	private ColumnHeader columnHeader2;

	private Button button;

	private ColumnHeader columnHeader;

	public VersionInformationTabPage()
	{
		InitializeComponent();
		Dock = DockStyle.Fill;
		FillListView();
	}

	private void FillListView()
	{
		listView.BeginUpdate();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			AssemblyName name = assembly.GetName();
			ListViewItem listViewItem = new ListViewItem(name.Name);
			listViewItem.SubItems.Add(name.Version.ToString());
			try
			{
				listViewItem.SubItems.Add(assembly.Location);
			}
			catch (NotSupportedException)
			{
				listViewItem.SubItems.Add("dynamic");
			}
			listView.Items.Add(listViewItem);
		}
		listView.EndUpdate();
	}

	private void CopyButtonClick(object sender, EventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			AssemblyName name = assembly.GetName();
			stringBuilder.Append(name.Name);
			stringBuilder.Append(",");
			stringBuilder.Append(name.Version.ToString());
			stringBuilder.Append(",");
			try
			{
				stringBuilder.Append(assembly.Location);
			}
			catch (NotSupportedException)
			{
				stringBuilder.Append("dynamic");
			}
			stringBuilder.Append(Environment.NewLine);
		}
		ClipboardWrapper.SetText(stringBuilder.ToString());
	}

	private void InitializeComponent()
	{
		this.columnHeader = new System.Windows.Forms.ColumnHeader();
		this.button = new System.Windows.Forms.Button();
		this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
		this.listView = new System.Windows.Forms.ListView();
		this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
		base.SuspendLayout();
		this.columnHeader.Text = ICSharpCode.Core.ResourceService.GetString("Global.Name");
		this.columnHeader.Width = 130;
		this.button.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.button.Location = new System.Drawing.Point(8, 184);
		this.button.Name = "button";
		this.button.TabIndex = 1;
		this.button.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.About.VersionInfoTabName.CopyButton");
		this.button.Click += new System.EventHandler(CopyButtonClick);
		this.button.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.columnHeader2.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.About.VersionInfoTabName.VersionColumn");
		this.columnHeader2.Width = 100;
		this.listView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.columnHeader, this.columnHeader2, this.columnHeader3 });
		this.listView.FullRowSelect = true;
		this.listView.GridLines = true;
		this.listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
		this.listView.Location = new System.Drawing.Point(0, 0);
		this.listView.Name = "listView";
		this.listView.Size = new System.Drawing.Size(248, 176);
		this.listView.TabIndex = 0;
		this.listView.View = System.Windows.Forms.View.Details;
		this.columnHeader3.Text = ICSharpCode.Core.ResourceService.GetString("Global.Path");
		this.columnHeader3.Width = 150;
		base.Controls.Add(this.button);
		base.Controls.Add(this.listView);
		base.Name = "CreatedUserControl";
		base.Size = new System.Drawing.Size(248, 216);
		base.ResumeLayout(false);
	}
}

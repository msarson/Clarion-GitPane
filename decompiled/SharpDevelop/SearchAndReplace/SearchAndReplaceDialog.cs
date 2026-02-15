using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class SearchAndReplaceDialog : Form
{
	private const string SearchMenuAddInPath = "/SharpDevelop/Workbench/MainMenu/Search";

	public static string SearchPattern = string.Empty;

	public static string ReplacePattern = string.Empty;

	private Keys searchKeyboardShortcut;

	private Keys replaceKeyboardShortcut;

	private static SearchAndReplaceDialog Instance;

	private ToolStripButton searchButton;

	private ToolStripButton replaceButton;

	private ToolStrip toolStrip;

	private SearchAndReplacePanel searchAndReplacePanel;

	public static void ShowSingleInstance(SearchAndReplaceMode searchAndReplaceMode)
	{
		if (Instance == null)
		{
			Instance = new SearchAndReplaceDialog(searchAndReplaceMode);
			Instance.Show(WorkbenchSingleton.MainForm);
			return;
		}
		if (searchAndReplaceMode == SearchAndReplaceMode.Search)
		{
			Instance.searchButton.PerformClick();
		}
		else
		{
			Instance.replaceButton.PerformClick();
		}
		Instance.Focus();
	}

	public SearchAndReplaceDialog(SearchAndReplaceMode searchAndReplaceMode)
	{
		InitializeComponent();
		SuspendLayout();
		Font = FontService.GetFont(FontService.FontType.Dialogs);
		base.Owner = WorkbenchSingleton.MainForm;
		Text = StringParser.Parse("${res:Dialog.NewProject.SearchReplace.Title}");
		searchButton.Text = StringParser.Parse("${res:Dialog.NewProject.SearchReplace.FindDialogName}");
		searchButton.Image = IconService.GetBitmap("Icons.16x16.FindIcon");
		replaceButton.Text = StringParser.Parse("${res:Dialog.NewProject.SearchReplace.ReplaceDialogName}");
		replaceButton.Image = IconService.GetBitmap("Icons.16x16.ReplaceIcon");
		replaceButton.Checked = searchAndReplaceMode == SearchAndReplaceMode.Replace;
		searchButton.Checked = searchAndReplaceMode == SearchAndReplaceMode.Search;
		ResumeLayout(performLayout: true);
		SetSearchAndReplaceMode();
		RightToLeftConverter.ConvertRecursive(this);
		FormLocationHelper.Apply(this, "ICSharpCode.SharpDevelop.Gui.SearchAndReplaceDialog.Location", isResizable: false);
		FormPositionService.Instance.Apply(this, "SearchAndReplaceDialog");
		searchKeyboardShortcut = GetKeyboardShortcut("/SharpDevelop/Workbench/MainMenu/Search", "Find");
		replaceKeyboardShortcut = GetKeyboardShortcut("/SharpDevelop/Workbench/MainMenu/Search", "Replace");
	}

	private void InitializeComponent()
	{
		this.toolStrip = new System.Windows.Forms.ToolStrip();
		this.searchButton = new System.Windows.Forms.ToolStripButton();
		this.replaceButton = new System.Windows.Forms.ToolStripButton();
		this.searchAndReplacePanel = new SearchAndReplace.SearchAndReplacePanel();
		base.SuspendLayout();
		this.searchAndReplacePanel.AutoSize = true;
		this.searchAndReplacePanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.Controls.Add(this.searchAndReplacePanel);
		this.searchAndReplacePanel.SearchAndReplaceMode = SearchAndReplace.SearchAndReplaceMode.Search;
		this.toolStrip.Dock = System.Windows.Forms.DockStyle.Top;
		this.toolStrip.Stretch = true;
		this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.searchButton.Checked = true;
		this.searchButton.Click += new System.EventHandler(SearchButtonClick);
		this.toolStrip.Items.Add(this.searchButton);
		this.replaceButton.Checked = false;
		this.replaceButton.Click += new System.EventHandler(ReplaceButtonClick);
		this.toolStrip.Items.Add(this.replaceButton);
		base.Controls.Add(this.toolStrip);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.ShowInTaskbar = false;
		base.TopMost = false;
		base.KeyPreview = true;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoSize = false;
		base.ClientSize = new System.Drawing.Size(432, 380);
		base.Name = "SearchAndReplaceDialog";
		base.ResumeLayout(false);
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
		Instance = null;
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyData == Keys.Escape)
		{
			Close();
		}
		else if (searchKeyboardShortcut == e.KeyData && !searchButton.Checked)
		{
			EnableSearchMode(enable: true);
		}
		else if (replaceKeyboardShortcut == e.KeyData && !replaceButton.Checked)
		{
			EnableSearchMode(enable: false);
		}
	}

	private void SearchButtonClick(object sender, EventArgs e)
	{
		if (!searchButton.Checked)
		{
			EnableSearchMode(enable: true);
		}
	}

	private void ReplaceButtonClick(object sender, EventArgs e)
	{
		if (!replaceButton.Checked)
		{
			EnableSearchMode(enable: false);
		}
	}

	private void EnableSearchMode(bool enable)
	{
		searchButton.Checked = enable;
		replaceButton.Checked = !enable;
		SetSearchAndReplaceMode();
		Focus();
	}

	private void SetSearchAndReplaceMode()
	{
		AutoSize = false;
		searchAndReplacePanel.Dock = DockStyle.None;
		searchAndReplacePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		base.AutoScaleMode = AutoScaleMode.Font;
		searchAndReplacePanel.SearchAndReplaceMode = ((!searchButton.Checked) ? SearchAndReplaceMode.Replace : SearchAndReplaceMode.Search);
		searchAndReplacePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		Font = FontService.GetFont(FontService.FontType.Dialogs);
		PerformAutoScale();
		PerformLayout();
		searchAndReplacePanel.Dock = DockStyle.Top;
		base.ClientSize = new Size(base.ClientSize.Width, toolStrip.Height + 5 + searchAndReplacePanel.Height);
	}

	private Keys GetKeyboardShortcut(string path, string id)
	{
		AddInTreeNode treeNode = AddInTree.GetTreeNode(path);
		if (treeNode != null)
		{
			foreach (Codon codon in treeNode.Codons)
			{
				if (codon.Id == id)
				{
					return MenuCommand.ParseShortcut(codon.Properties["shortcut"]);
				}
			}
		}
		return Keys.None;
	}
}

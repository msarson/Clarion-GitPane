using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

[ProvideProperty("DoLocate", typeof(PropertyGrid))]
public abstract class ListLocatorBase : UserControl, IExtenderProvider
{
	public class SearchEventArgs : EventArgs
	{
		private string searchText = string.Empty;

		public string SearchText => searchText;

		public SearchEventArgs(string searchText)
		{
			this.searchText = searchText;
		}
	}

	private bool _SuportExapandContractButtons;

	private bool searchInString = true;

	private bool _ShowBeginWithButton = true;

	private static KeysConverter keyConverter = new KeysConverter();

	private string prevText = string.Empty;

	private IContainer components;

	private TextBox textBoxToSearch;

	private Button buttonSearch;

	private CheckBox beginWithCheckBox;

	private TableLayoutPanel tableLayoutPanel1;

	private Button buttonExpandAll;

	private Button buttonContractAll;

	public abstract bool HasListToLocate { get; }

	public abstract bool ListHasItems { get; }

	[Description("Show or not the ExpandAll ContractAll buttons")]
	[Category("Visual Behavior")]
	public bool SuportExapandContractButtons
	{
		get
		{
			return _SuportExapandContractButtons;
		}
		set
		{
			_SuportExapandContractButtons = value;
			buttonExpandAll.Visible = _SuportExapandContractButtons;
			buttonContractAll.Visible = _SuportExapandContractButtons;
			Refresh();
		}
	}

	[Category("Search Behavior")]
	[Description("Type of Search.\r\nIf set to true the locator will search inside the text, otherwise will search at the beginning.")]
	public bool InString
	{
		get
		{
			return searchInString;
		}
		set
		{
			searchInString = value;
		}
	}

	[Category("Appearance")]
	public bool IsTransparent
	{
		get
		{
			return BackColor == Color.Transparent;
		}
		set
		{
			if (value)
			{
				BackColor = Color.Transparent;
			}
			else
			{
				BackColor = SystemColors.Control;
			}
		}
	}

	[Category("Search Behavior")]
	[Description("Show the button to search matchs that Begin With the search term.\r\n")]
	public bool ShowBeginWithButton
	{
		get
		{
			return _ShowBeginWithButton;
		}
		set
		{
			if (_ShowBeginWithButton != value)
			{
				_ShowBeginWithButton = value;
				beginWithCheckBox.Visible = _ShowBeginWithButton;
			}
		}
	}

	public event EventHandler<SearchEventArgs> SearchClicked;

	public abstract bool Search(string text, bool fromTop);

	protected abstract bool CanLocateAtControl(object extendee);

	public void RefreshEnable()
	{
		if (ListHasItems)
		{
			textBoxToSearch.Text = string.Empty;
			textBoxToSearch.ReadOnly = false;
		}
		else
		{
			textBoxToSearch.Text = string.Empty;
			textBoxToSearch.ReadOnly = true;
		}
	}

	public ListLocatorBase()
	{
		InitializeComponent();
		buttonSearch.Image = new Bitmap(GetType().Assembly.GetManifestResourceStream("Resources.LocatorButtonImage.png"));
		beginWithCheckBox.Image = new Bitmap(GetType().Assembly.GetManifestResourceStream("Resources.LocatorBeginWith.png"));
		buttonContractAll.Image = new Bitmap(GetType().Assembly.GetManifestResourceStream("Resources.ContractAll.png"));
		buttonExpandAll.Image = new Bitmap(GetType().Assembly.GetManifestResourceStream("Resources.ExpandAll.png"));
		Font = FontService.GetFont(FontService.FontType.ListControls);
	}

	protected void KeyPressOnTree(object sender, KeyPressEventArgs e)
	{
		string text = textBoxToSearch.Text;
		if (e.KeyChar == '\b')
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (text.Length == 1)
			{
				textBoxToSearch.Text = string.Empty;
				return;
			}
			text = text.Substring(0, text.Length - 1);
		}
		else
		{
			text += e.KeyChar;
		}
		e.Handled = true;
		textBoxToSearch.Text = text;
		if (!Search(text, fromTop: false))
		{
			Search(text, fromTop: true);
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.Return)
		{
			RefreshSearchButton();
			OnSearchButtonClick(this, EventArgs.Empty);
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	public bool CanExtend(object extendee)
	{
		if (extendee == this || !HasListToLocate)
		{
			return false;
		}
		return CanLocateAtControl(extendee);
	}

	private void OnBeginWithCheckBoxCheckedChanged(object sender, EventArgs e)
	{
		if (beginWithCheckBox.Checked)
		{
			InString = false;
		}
		else
		{
			InString = true;
		}
	}

	private void OnSearchTextChanged(object sender, EventArgs e)
	{
		RefreshSearchButton();
	}

	private void RefreshSearchButton()
	{
		if (ListHasItems)
		{
			if (textBoxToSearch.Text != null && textBoxToSearch.Text != string.Empty)
			{
				buttonSearch.Enabled = true;
			}
			else
			{
				buttonSearch.Enabled = false;
			}
		}
	}

	private void OnSearchButtonClick(object sender, EventArgs e)
	{
		if (buttonSearch.Enabled)
		{
			bool flag = true;
			if (prevText.Equals(textBoxToSearch.Text, StringComparison.OrdinalIgnoreCase))
			{
				flag = false;
			}
			prevText = textBoxToSearch.Text;
			if (this.SearchClicked != null)
			{
				this.SearchClicked(null, new SearchEventArgs(textBoxToSearch.Text));
			}
			if (!flag && !Search(textBoxToSearch.Text, flag))
			{
				Search(textBoxToSearch.Text, fromTop: true);
			}
		}
	}

	private void OnTextBoxToSearch_Enter(object sender, EventArgs e)
	{
		RefreshEnable();
	}

	private void OnTextBoxToSearch_Leave(object sender, EventArgs e)
	{
	}

	private void buttonExpandAll_Click(object sender, EventArgs e)
	{
		ExpandAll();
	}

	protected virtual void ExpandAll()
	{
	}

	protected virtual void ContractAll()
	{
	}

	private void buttonContractAll_Click(object sender, EventArgs e)
	{
		ContractAll();
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
		this.textBoxToSearch = new System.Windows.Forms.TextBox();
		this.buttonSearch = new System.Windows.Forms.Button();
		this.beginWithCheckBox = new System.Windows.Forms.CheckBox();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.buttonExpandAll = new System.Windows.Forms.Button();
		this.buttonContractAll = new System.Windows.Forms.Button();
		this.tableLayoutPanel1.SuspendLayout();
		base.SuspendLayout();
		this.textBoxToSearch.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.textBoxToSearch.Location = new System.Drawing.Point(3, 6);
		this.textBoxToSearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.textBoxToSearch.Name = "textBoxToSearch";
		this.textBoxToSearch.Size = new System.Drawing.Size(120, 20);
		this.textBoxToSearch.TabIndex = 0;
		this.textBoxToSearch.TextChanged += new System.EventHandler(OnSearchTextChanged);
		this.textBoxToSearch.Enter += new System.EventHandler(OnTextBoxToSearch_Enter);
		this.textBoxToSearch.Leave += new System.EventHandler(OnTextBoxToSearch_Leave);
		this.buttonSearch.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.buttonSearch.BackColor = System.Drawing.SystemColors.Control;
		this.buttonSearch.Enabled = false;
		this.buttonSearch.FlatAppearance.BorderSize = 0;
		this.buttonSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonSearch.Location = new System.Drawing.Point(129, 4);
		this.buttonSearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.buttonSearch.MaximumSize = new System.Drawing.Size(24, 24);
		this.buttonSearch.MinimumSize = new System.Drawing.Size(24, 24);
		this.buttonSearch.Name = "buttonSearch";
		this.buttonSearch.Size = new System.Drawing.Size(24, 24);
		this.buttonSearch.TabIndex = 1;
		this.buttonSearch.UseVisualStyleBackColor = false;
		this.buttonSearch.Click += new System.EventHandler(OnSearchButtonClick);
		this.beginWithCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
		this.beginWithCheckBox.BackColor = System.Drawing.SystemColors.Control;
		this.beginWithCheckBox.FlatAppearance.BorderSize = 0;
		this.beginWithCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.beginWithCheckBox.Location = new System.Drawing.Point(159, 2);
		this.beginWithCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.beginWithCheckBox.MaximumSize = new System.Drawing.Size(24, 24);
		this.beginWithCheckBox.MinimumSize = new System.Drawing.Size(24, 24);
		this.beginWithCheckBox.Name = "beginWithCheckBox";
		this.beginWithCheckBox.Size = new System.Drawing.Size(24, 24);
		this.beginWithCheckBox.TabIndex = 2;
		this.beginWithCheckBox.UseVisualStyleBackColor = false;
		this.beginWithCheckBox.CheckedChanged += new System.EventHandler(OnBeginWithCheckBoxCheckedChanged);
		this.tableLayoutPanel1.ColumnCount = 5;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel1.Controls.Add(this.textBoxToSearch, 0, 0);
		this.tableLayoutPanel1.Controls.Add(this.beginWithCheckBox, 2, 0);
		this.tableLayoutPanel1.Controls.Add(this.buttonSearch, 1, 0);
		this.tableLayoutPanel1.Controls.Add(this.buttonContractAll, 4, 0);
		this.tableLayoutPanel1.Controls.Add(this.buttonExpandAll, 3, 0);
		this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 1;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(246, 32);
		this.tableLayoutPanel1.TabIndex = 3;
		this.buttonExpandAll.FlatAppearance.BorderSize = 0;
		this.buttonExpandAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonExpandAll.Location = new System.Drawing.Point(189, 2);
		this.buttonExpandAll.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.buttonExpandAll.Name = "buttonExpandAll";
		this.buttonExpandAll.Size = new System.Drawing.Size(24, 24);
		this.buttonExpandAll.TabIndex = 2;
		this.buttonExpandAll.UseVisualStyleBackColor = true;
		this.buttonExpandAll.Visible = false;
		this.buttonExpandAll.Click += new System.EventHandler(buttonExpandAll_Click);
		this.buttonContractAll.FlatAppearance.BorderSize = 0;
		this.buttonContractAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonContractAll.Location = new System.Drawing.Point(219, 2);
		this.buttonContractAll.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
		this.buttonContractAll.Name = "buttonContractAll";
		this.buttonContractAll.Size = new System.Drawing.Size(24, 24);
		this.buttonContractAll.TabIndex = 3;
		this.buttonContractAll.UseVisualStyleBackColor = true;
		this.buttonContractAll.Visible = false;
		this.buttonContractAll.Click += new System.EventHandler(buttonContractAll_Click);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
		base.Controls.Add(this.tableLayoutPanel1);
		base.Margin = new System.Windows.Forms.Padding(0);
		base.Name = "ListLocatorBase";
		base.Size = new System.Drawing.Size(246, 32);
		this.tableLayoutPanel1.ResumeLayout(false);
		this.tableLayoutPanel1.PerformLayout();
		base.ResumeLayout(false);
	}
}

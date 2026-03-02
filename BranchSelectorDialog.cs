using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitPane
{
    public class BranchSelectorDialog : Form
    {
        private ListView   _list;
        private TextBox    _searchBox;
        private CheckBox   _showRemote;
        private Button     _checkoutButton;
        private Button     _cancelButton;

        private readonly BranchInfo[] _allBranches;
        private readonly string       _currentBranch;

        public string SelectedBranch { get; private set; }

        public BranchSelectorDialog(BranchInfo[] branches, string currentBranch)
        {
            _allBranches   = branches;
            _currentBranch = currentBranch;
            BuildUI();
            PopulateList(string.Empty);
        }

        private void BuildUI()
        {
            Text            = "Branch Manager";
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize     = new Size(560, 380);
            Size            = new Size(680, 500);
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;

            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 5,
                ColumnCount = 1,
                Padding     = new Padding(12),
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // current branch label
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // search
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // list
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // show remote checkbox
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // buttons

            // Current branch header
            layout.Controls.Add(new Label
            {
                Text     = "Current branch:  " + (_currentBranch ?? "(unknown)"),
                AutoSize = true,
                Font     = new Font(Font, FontStyle.Bold),
                Margin   = new Padding(0, 0, 0, 8),
            }, 0, 0);

            // Search row
            var searchPanel = new Panel { Dock = DockStyle.Fill, Height = 28, Margin = new Padding(0, 0, 0, 6) };
            var searchLabel = new Label { Text = "Search:", AutoSize = true, Location = new Point(0, 5) };
            _searchBox = new TextBox { Location = new Point(52, 2), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            _searchBox.TextChanged += (s, e) => PopulateList(_searchBox.Text);
            searchPanel.Controls.Add(searchLabel);
            searchPanel.Controls.Add(_searchBox);
            searchPanel.Resize += (s, e) => _searchBox.Width = searchPanel.Width - 56;
            layout.Controls.Add(searchPanel, 0, 1);

            // Branch list
            _list = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                MultiSelect   = false,
                HideSelection = false,
                GridLines     = true,
                Font          = new Font("Consolas", 9F),
            };
            _list.Columns.Add("Branch",      280);
            _list.Columns.Add("Last commit", 120);
            _list.Columns.Add("Date",        90);
            _list.Columns.Add("Sync",        80);
            _list.DoubleClick          += (s, e) => { if (CanCheckout()) Accept(); };
            _list.SelectedIndexChanged += (s, e) => UpdateButtons();
            layout.Controls.Add(_list, 0, 2);

            // Show remote checkbox
            _showRemote = new CheckBox
            {
                Text     = "Show remote branches",
                AutoSize = true,
                Margin   = new Padding(0, 6, 0, 4),
            };
            _showRemote.CheckedChanged += (s, e) => PopulateList(_searchBox.Text);
            layout.Controls.Add(_showRemote, 0, 3);

            // Buttons
            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize      = true,
                Margin        = new Padding(0, 4, 0, 0),
            };
            _cancelButton   = new Button { Text = "Cancel",   Width = 80, DialogResult = DialogResult.Cancel };
            _checkoutButton = new Button { Text = "Checkout", Width = 80, Enabled = false };
            _checkoutButton.Click += (s, e) => Accept();
            buttons.Controls.Add(_cancelButton);
            buttons.Controls.Add(_checkoutButton);
            layout.Controls.Add(buttons, 0, 4);

            AcceptButton = _checkoutButton;
            CancelButton = _cancelButton;
            Controls.Add(layout);
        }

        private void PopulateList(string filter)
        {
            string prevSelected = SelectedItem();

            _list.BeginUpdate();
            _list.Items.Clear();

            string f = filter == null ? string.Empty : filter.ToLower();

            foreach (var branch in _allBranches)
            {
                if (branch.IsRemote && !_showRemote.Checked) continue;
                if (f.Length > 0 && !branch.Name.ToLower().Contains(f)) continue;

                string name = branch.IsCurrent ? branch.Name + "  \u2190 current" : branch.Name;

                string sync = string.Empty;
                if (!branch.IsRemote)
                {
                    if (branch.BehindCount == 0 && branch.AheadCount == 0)
                        sync = "✓";
                    else
                    {
                        if (branch.BehindCount > 0) sync += $"↓{branch.BehindCount} ";
                        if (branch.AheadCount  > 0) sync += $"↑{branch.AheadCount}";
                        sync = sync.Trim();
                    }
                }

                var item = new ListViewItem(name);
                item.SubItems.Add(branch.LastCommit);
                item.SubItems.Add(branch.ShortDate);
                item.SubItems.Add(sync);
                item.Tag = branch.Name;

                if (branch.IsCurrent)
                    item.Font = new Font(_list.Font, FontStyle.Bold);
                if (branch.IsRemote)
                    item.ForeColor = SystemColors.GrayText;

                _list.Items.Add(item);
            }

            // Restore selection
            string toSelect = prevSelected ?? _currentBranch;
            foreach (ListViewItem item in _list.Items)
            {
                if ((string)item.Tag == toSelect)
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    break;
                }
            }

            _list.EndUpdate();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            _checkoutButton.Enabled = CanCheckout();
        }

        private bool CanCheckout()
        {
            string sel = SelectedItem();
            return sel != null && sel != _currentBranch;
        }

        private string SelectedItem() =>
            _list.SelectedItems.Count > 0 ? (string)_list.SelectedItems[0].Tag : null;

        private void Accept()
        {
            SelectedBranch = SelectedItem();
            if (SelectedBranch != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }

    public class BranchInfo
    {
        public string Name { get; set; }
        public string LastCommit { get; set; }   // relative, e.g. "3 days ago"
        public string ShortDate  { get; set; }   // ISO short, e.g. "2026-02-28"
        public bool IsRemote { get; set; }
        public bool IsCurrent { get; set; }
        public int AheadCount { get; set; }
        public int BehindCount { get; set; }
    }

    public class StashEntry
    {
        public int    Index    { get; set; }
        public string Ref      { get; set; }
        public string Message  { get; set; }
        public string Relative { get; set; }
    }
}

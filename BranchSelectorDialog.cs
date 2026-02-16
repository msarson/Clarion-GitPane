using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitPane
{
    public class BranchSelectorDialog : Form
    {
        private ListBox branchListBox;
        private TextBox searchBox;
        private Button checkoutButton;
        private Button cancelButton;
        private Label infoLabel;
        private string selectedBranch;

        public string SelectedBranch => selectedBranch;

        public BranchSelectorDialog(BranchInfo[] branches, string currentBranch)
        {
            InitializeComponent(branches, currentBranch);
        }

        private void InitializeComponent(BranchInfo[] branches, string currentBranch)
        {
            Text = "Select Branch";
            Size = new Size(500, 450);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Search box
            var searchLabel = new Label();
            searchLabel.Text = "Search:";
            searchLabel.Location = new Point(10, 15);
            searchLabel.AutoSize = true;

            searchBox = new TextBox();
            searchBox.Location = new Point(60, 12);
            searchBox.Width = 410;
            searchBox.TextChanged += OnSearchChanged;

            // Info label
            infoLabel = new Label();
            infoLabel.Location = new Point(10, 40);
            infoLabel.AutoSize = true;
            infoLabel.ForeColor = Color.Gray;
            infoLabel.Text = $"Current: {currentBranch}";

            // Branch list
            branchListBox = new ListBox();
            branchListBox.Location = new Point(10, 65);
            branchListBox.Size = new Size(460, 300);
            branchListBox.Font = new Font("Consolas", 9F);
            branchListBox.DoubleClick += OnBranchDoubleClick;

            // Populate branches
            foreach (var branch in branches)
            {
                var displayText = branch.IsRemote 
                    ? $"{branch.Name,-40} (remote, {branch.LastCommit})"
                    : $"{branch.Name,-40} (local, {branch.LastCommit})";
                
                branchListBox.Items.Add(new BranchListItem(branch.Name, displayText, branch.IsRemote));
                
                if (branch.Name == currentBranch || 
                    (branch.IsRemote && branch.Name == "origin/" + currentBranch))
                {
                    branchListBox.SelectedIndex = branchListBox.Items.Count - 1;
                }
            }

            // Buttons
            checkoutButton = new Button();
            checkoutButton.Text = "Checkout";
            checkoutButton.Location = new Point(295, 375);
            checkoutButton.Width = 85;
            checkoutButton.Click += OnCheckoutClick;

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(385, 375);
            cancelButton.Width = 85;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Click += OnCancelClick;

            Controls.Add(searchLabel);
            Controls.Add(searchBox);
            Controls.Add(infoLabel);
            Controls.Add(branchListBox);
            Controls.Add(checkoutButton);
            Controls.Add(cancelButton);

            AcceptButton = checkoutButton;
            CancelButton = cancelButton;
        }

        private void OnSearchChanged(object sender, EventArgs e)
        {
            string filter = searchBox.Text.ToLower();
            for (int i = 0; i < branchListBox.Items.Count; i++)
            {
                var item = (BranchListItem)branchListBox.Items[i];
                if (item.BranchName.ToLower().Contains(filter))
                {
                    branchListBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void OnBranchDoubleClick(object sender, EventArgs e)
        {
            if (branchListBox.SelectedItem != null)
            {
                var item = (BranchListItem)branchListBox.SelectedItem;
                selectedBranch = item.BranchName;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void OnCheckoutClick(object sender, EventArgs e)
        {
            if (branchListBox.SelectedItem != null)
            {
                var item = (BranchListItem)branchListBox.SelectedItem;
                selectedBranch = item.BranchName;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a branch.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        private class BranchListItem
        {
            public string BranchName { get; }
            public string DisplayText { get; }
            public bool IsRemote { get; }

            public BranchListItem(string branchName, string displayText, bool isRemote)
            {
                BranchName = branchName;
                DisplayText = displayText;
                IsRemote = isRemote;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }
    }

    public class BranchInfo
    {
        public string Name { get; set; }
        public string LastCommit { get; set; }
        public bool IsRemote { get; set; }
        public bool IsCurrent { get; set; }
    }
}

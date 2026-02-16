using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitPane
{
    public class GitHistoryDialog : Form
    {
        private GitRepository gitRepo;
        private ListView commitsListView;
        private TextBox detailsTextBox;
        private SplitContainer splitContainer;

        public GitHistoryDialog(GitRepository repo)
        {
            gitRepo = repo;
            InitializeUI();
            LoadCommitHistory();
        }

        private void InitializeUI()
        {
            this.Text = "Git History";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(700, 400);

            // Split container for commits list and details
            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterDistance = 250;

            // Commits list view
            commitsListView = new ListView();
            commitsListView.Dock = DockStyle.Fill;
            commitsListView.View = View.Details;
            commitsListView.FullRowSelect = true;
            commitsListView.GridLines = true;
            commitsListView.Font = new Font("Segoe UI", 9F);
            commitsListView.SelectedIndexChanged += OnCommitSelected;

            // Add columns
            commitsListView.Columns.Add("Hash", 70);
            commitsListView.Columns.Add("Date", 150);
            commitsListView.Columns.Add("Author", 120);
            commitsListView.Columns.Add("Message", 500);

            // Details text box
            detailsTextBox = new TextBox();
            detailsTextBox.Dock = DockStyle.Fill;
            detailsTextBox.Multiline = true;
            detailsTextBox.ScrollBars = ScrollBars.Both;
            detailsTextBox.Font = new Font("Courier New", 9F);
            detailsTextBox.ReadOnly = true;
            detailsTextBox.BackColor = SystemColors.Window;
            detailsTextBox.WordWrap = false;

            splitContainer.Panel1.Controls.Add(commitsListView);
            splitContainer.Panel2.Controls.Add(detailsTextBox);

            this.Controls.Add(splitContainer);
        }

        private void LoadCommitHistory()
        {
            commitsListView.Items.Clear();
            
            var commits = gitRepo.GetCommitHistory(100);
            
            foreach (var commit in commits)
            {
                var item = new ListViewItem(commit.ShortHash);
                item.SubItems.Add(commit.Date);
                item.SubItems.Add(commit.Author);
                item.SubItems.Add(commit.Subject);
                item.Tag = commit.Hash; // Store full hash
                
                commitsListView.Items.Add(item);
            }
        }

        private void OnCommitSelected(object sender, EventArgs e)
        {
            if (commitsListView.SelectedItems.Count == 0)
            {
                detailsTextBox.Text = "";
                return;
            }

            var selectedItem = commitsListView.SelectedItems[0];
            string commitHash = selectedItem.Tag as string;

            if (string.IsNullOrEmpty(commitHash))
                return;

            // Show loading message
            detailsTextBox.Text = "Loading commit details...";
            detailsTextBox.Update();

            // Get commit details
            var details = gitRepo.GetCommitDetails(commitHash);
            if (details != null)
            {
                // Build display text
                var text = new System.Text.StringBuilder();
                text.AppendLine($"Commit: {details.Hash}");
                text.AppendLine($"Author: {details.Author}");
                text.AppendLine($"Date:   {details.Date}");
                text.AppendLine();
                text.AppendLine(details.Subject);
                
                if (!string.IsNullOrEmpty(details.Body))
                {
                    text.AppendLine();
                    text.AppendLine(details.Body);
                }
                
                if (!string.IsNullOrEmpty(details.Stats))
                {
                    text.AppendLine();
                    text.AppendLine(details.Stats);
                }
                
                if (!string.IsNullOrEmpty(details.Diff))
                {
                    text.AppendLine();
                    text.AppendLine("---");
                    text.AppendLine();
                    text.AppendLine(details.Diff);
                }

                detailsTextBox.Text = text.ToString();
                detailsTextBox.Select(0, 0);
            }
            else
            {
                detailsTextBox.Text = "Failed to load commit details.";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GitPane
{
    /// <summary>
    /// Shown after a merge that resulted in conflicts. Lists each conflicted file
    /// and allows the user to open it in an external diff tool, mark it as resolved,
    /// abort the merge, or complete the merge once all conflicts are resolved.
    /// </summary>
    public class MergeConflictDialog : Form
    {
        private readonly GitRepository _repo;
        private readonly string        _workingDir;
        private readonly string        _mergingBranch;

        private ListView     _listView;
        private Button       _openDiffButton;
        private Button       _resolvedButton;
        private Button       _abortButton;
        private Button       _completeButton;
        private Label        _statusLabel;

        // Detected external diff tool (null = Notepad fallback)
        private static string _diffToolPath;
        private static string _diffToolArgs;  // {0} = file path

        static MergeConflictDialog()
        {
            DetectDiffTool();
        }

        public MergeConflictDialog(GitRepository repo, string workingDir,
            string mergingBranch, string[] conflictedFiles)
        {
            _repo          = repo;
            _workingDir    = workingDir;
            _mergingBranch = mergingBranch;

            BuildUI(conflictedFiles);
        }

        private void BuildUI(string[] files)
        {
            Text            = "Merge Conflicts";
            Size            = new Size(620, 460);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;

            var infoLabel = new Label
            {
                Text      = $"Merging '{_mergingBranch}' produced conflicts in the following files.\n" +
                             "Resolve each file, mark it as resolved, then click Complete Merge.",
                Location  = new Point(10, 10),
                Size      = new Size(590, 40),
                ForeColor = Color.FromArgb(160, 80, 0),
            };

            _listView = new ListView
            {
                Location      = new Point(10, 55),
                Size          = new Size(590, 290),
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                MultiSelect   = false,
            };
            _listView.Columns.Add("File", 430);
            _listView.Columns.Add("Status", 140);
            _listView.SelectedIndexChanged += (s, e) => UpdateButtons();

            foreach (var f in files)
            {
                var item = new ListViewItem(f);
                item.SubItems.Add("⚠ Conflict");
                item.ForeColor = Color.DarkRed;
                item.Tag       = false; // resolved flag
                _listView.Items.Add(item);
            }

            string toolName = _diffToolPath != null
                ? Path.GetFileNameWithoutExtension(_diffToolPath)
                : "Notepad";

            _openDiffButton = new Button
            {
                Text     = $"Open in {toolName}",
                Location = new Point(10, 360),
                Width    = 130,
                Enabled  = false,
            };
            _openDiffButton.Click += OnOpenDiff;

            _resolvedButton = new Button
            {
                Text     = "Mark Resolved",
                Location = new Point(150, 360),
                Width    = 120,
                Enabled  = false,
            };
            _resolvedButton.Click += OnMarkResolved;

            _abortButton = new Button
            {
                Text     = "Abort Merge",
                Location = new Point(280, 360),
                Width    = 110,
            };
            _abortButton.Click += OnAbortMerge;

            _completeButton = new Button
            {
                Text     = "Complete Merge",
                Location = new Point(400, 360),
                Width    = 130,
                Enabled  = false,
            };
            _completeButton.Click += OnCompleteMerge;

            var cancelButton = new Button
            {
                Text         = "Close",
                Location     = new Point(540, 360),
                Width        = 60,
                DialogResult = DialogResult.Cancel,
            };

            _statusLabel = new Label
            {
                Location  = new Point(10, 395),
                Size      = new Size(590, 20),
                ForeColor = Color.Gray,
            };

            Controls.AddRange(new Control[]
            {
                infoLabel, _listView, _openDiffButton, _resolvedButton,
                _abortButton, _completeButton, cancelButton, _statusLabel
            });

            CancelButton = cancelButton;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool anySelected = _listView.SelectedItems.Count > 0;
            bool selectedResolved = anySelected &&
                (bool)_listView.SelectedItems[0].Tag;

            _openDiffButton.Enabled = anySelected && !selectedResolved;
            _resolvedButton.Enabled = anySelected && !selectedResolved;

            // Complete enabled only when every file is resolved
            bool allResolved = true;
            foreach (ListViewItem item in _listView.Items)
                if (!(bool)item.Tag) { allResolved = false; break; }
            _completeButton.Enabled = allResolved && _listView.Items.Count > 0;
        }

        private void OnOpenDiff(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0) return;
            string relativePath = _listView.SelectedItems[0].Text;
            string fullPath     = Path.Combine(_workingDir, relativePath);

            try
            {
                if (_diffToolPath != null)
                    Process.Start(_diffToolPath, string.Format(_diffToolArgs, $"\"{fullPath}\""));
                else
                    Process.Start("notepad.exe", $"\"{fullPath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open diff tool.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnMarkResolved(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0) return;
            var item = _listView.SelectedItems[0];
            string relativePath = item.Text;

            bool ok = _repo.StageFile(relativePath);
            if (ok)
            {
                item.SubItems[1].Text = "✓ Resolved";
                item.ForeColor        = Color.DarkGreen;
                item.Tag              = true;
                _statusLabel.Text     = $"Marked as resolved: {relativePath}";
            }
            else
            {
                _statusLabel.Text = $"git add failed for: {relativePath}";
            }
            UpdateButtons();
        }

        private void OnAbortMerge(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Abort the merge? All conflict markers will be removed and the branch will be restored to its pre-merge state.",
                "Abort Merge", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            var result = _repo.AbortMerge();
            if (result.ExitCode == 0)
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
            else
            {
                MessageBox.Show($"Failed to abort merge.\n\n{result.Error}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCompleteMerge(object sender, EventArgs e)
        {
            _completeButton.Enabled = false;
            _statusLabel.Text       = "Committing merge...";

            var result = _repo.CommitMerge();
            if (result.ExitCode == 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _completeButton.Enabled = true;
                _statusLabel.Text       = string.Empty;
                MessageBox.Show($"Failed to commit merge.\n\n{result.Error}\n\n{result.Output}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void DetectDiffTool()
        {
            // Probe in preference order: WinMerge, VS Code, Beyond Compare
            var candidates = new[]
            {
                new { Exe = "WinMergeU.exe",
                      Paths = new[] {
                          @"C:\Program Files\WinMerge\WinMergeU.exe",
                          @"C:\Program Files (x86)\WinMerge\WinMergeU.exe"
                      },
                      Args = "{0}" },
                new { Exe = "code.cmd",
                      Paths = new[] {
                          @"C:\Program Files\Microsoft VS Code\bin\code.cmd",
                          @"C:\Program Files (x86)\Microsoft VS Code\bin\code.cmd"
                      },
                      Args = "{0}" },
                new { Exe = "BComp.exe",
                      Paths = new[] {
                          @"C:\Program Files\Beyond Compare 4\BComp.exe",
                          @"C:\Program Files (x86)\Beyond Compare 4\BComp.exe"
                      },
                      Args = "{0}" },
            };

            foreach (var c in candidates)
            {
                // Try known install paths first
                foreach (var path in c.Paths)
                {
                    if (File.Exists(path))
                    {
                        _diffToolPath = path;
                        _diffToolArgs = c.Args;
                        return;
                    }
                }
                // Fall back to PATH lookup
                string found = FindOnPath(c.Exe);
                if (found != null)
                {
                    _diffToolPath = found;
                    _diffToolArgs = c.Args;
                    return;
                }
            }
            // _diffToolPath stays null → Notepad fallback
        }

        private static string FindOnPath(string exe)
        {
            try
            {
                var psi = new ProcessStartInfo("where", exe)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                };
                using (var p = Process.Start(psi))
                {
                    string line = p.StandardOutput.ReadLine();
                    p.WaitForExit(3000);
                    return (!string.IsNullOrEmpty(line) && File.Exists(line)) ? line : null;
                }
            }
            catch { return null; }
        }
    }
}

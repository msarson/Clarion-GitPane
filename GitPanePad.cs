using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace GitPane
{
    public class GitPanePad : AbstractPadContent
    {
        private Panel contentPanel;
        private Label statusLabel;

        public override Control Control => contentPanel;

        public GitPanePad()
        {
            InitializeUI();
            UpdateStatus();

            // Subscribe to solution events
            ProjectService.SolutionLoaded += OnSolutionChanged;
            ProjectService.SolutionClosed += OnSolutionClosed;
        }

        private void InitializeUI()
        {
            contentPanel = new Panel();
            contentPanel.BackColor = SystemColors.Window;
            contentPanel.Padding = new Padding(10);

            statusLabel = new Label();
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 10F);
            statusLabel.ForeColor = SystemColors.ControlText;
            statusLabel.Location = new Point(10, 10);

            contentPanel.Controls.Add(statusLabel);
        }

        private void UpdateStatus()
        {
            if (ProjectService.OpenSolution != null)
            {
                string solutionDir = ProjectService.OpenSolution.Directory;
                statusLabel.Text = $"Solution folder: {solutionDir}";
            }
            else
            {
                statusLabel.Text = "No solution opened";
            }
        }

        private void OnSolutionChanged(object sender, SolutionEventArgs e)
        {
            UpdateStatus();
        }

        private void OnSolutionClosed(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        public override void Dispose()
        {
            ProjectService.SolutionLoaded -= OnSolutionChanged;
            ProjectService.SolutionClosed -= OnSolutionClosed;
            contentPanel?.Dispose();
            base.Dispose();
        }
    }
}

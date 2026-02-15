using System;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

internal class CustomDialog : Form
{
	private Label label;

	private Panel panel;

	private int acceptButton;

	private int cancelButton;

	private int result = -1;

	public int Result => result;

	public CustomDialog(string caption, string message, int acceptButton, int cancelButton, string[] buttonLabels)
	{
		SuspendLayout();
		MyInitializeComponent();
		base.Icon = null;
		this.acceptButton = acceptButton;
		this.cancelButton = cancelButton;
		message = StringParser.Parse(message);
		Text = StringParser.Parse(caption);
		using (Graphics graphics = CreateGraphics())
		{
			Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
			Size clientSize = graphics.MeasureString(message, label.Font, workingArea.Width - 20).ToSize();
			Button[] array = new Button[buttonLabels.Length];
			int[] array2 = new int[buttonLabels.Length];
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				Button button = new Button
				{
					FlatStyle = FlatStyle.System,
					Tag = i
				};
				string text = (button.Text = StringParser.Parse(buttonLabels[i]));
				button.Click += ButtonClick;
				SizeF sizeF = graphics.MeasureString(text, button.Font);
				button.Width = Math.Max(button.Width, ((int)Math.Ceiling((double)sizeF.Width / 8.0) + 1) * 8);
				array2[i] = num;
				array[i] = button;
				num += button.Width + 4;
			}
			if (acceptButton >= 0)
			{
				base.AcceptButton = array[acceptButton];
			}
			if (cancelButton >= 0)
			{
				base.CancelButton = array[cancelButton];
			}
			num += 4;
			if (num > clientSize.Width)
			{
				clientSize.Width = num;
			}
			clientSize.Height += panel.Height + 6;
			base.ClientSize = clientSize;
			int num2 = (clientSize.Width - num) / 2;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Location = new Point(num2 + array2[j], 4);
			}
			panel.Controls.AddRange(array);
		}
		label.Text = message;
		RightToLeftConverter.ConvertRecursive(this);
		ResumeLayout(performLayout: false);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (cancelButton == -1 && e.KeyCode == Keys.Escape)
		{
			Close();
		}
	}

	private void ButtonClick(object sender, EventArgs e)
	{
		result = (int)((Control)sender).Tag;
		Close();
	}

	private void MyInitializeComponent()
	{
		panel = new Panel();
		label = new Label();
		panel.Dock = DockStyle.Bottom;
		panel.Location = new Point(4, 80);
		panel.Name = "panel";
		panel.Size = new Size(266, 32);
		panel.TabIndex = 0;
		label.Dock = DockStyle.Fill;
		label.FlatStyle = FlatStyle.System;
		label.Location = new Point(4, 4);
		label.Name = "label";
		label.Size = new Size(266, 76);
		label.TabIndex = 1;
		label.UseMnemonic = false;
		base.ClientSize = new Size(274, 112);
		base.Controls.Add(label);
		base.Controls.Add(panel);
		base.DockPadding.Left = 4;
		base.DockPadding.Right = 4;
		base.DockPadding.Top = 4;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "CustomDialog";
		base.KeyPreview = true;
		base.StartPosition = FormStartPosition.CenterParent;
		Text = "CustomDialog";
	}
}

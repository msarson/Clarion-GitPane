using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using SoftVelocity.Common.Controls;

namespace SoftVelocity.Common.Redirection;

public class LoadViaRedFormDialog : PositionedSharpDevelopForm
{
	private RedirectionFile redFile;

	private int traceHeight = -1;

	private int buttonsHeight = -1;

	private int originalHeight;

	private string title;

	private int lastFound = -1;

	private string lastText = "";

	private IContainer components;

	private Button copyButton;

	private Button cancelButton;

	private ListBox traceListBox;

	private Button okButton;

	private Label label1;

	private Button traceButton;

	private AutoCompleteTextBox fileNameText;

	protected override string DialogName => "LoadViaRedirectionFileDialog";

	public string File => fileNameText.Text;

	public LoadViaRedFormDialog()
	{
		InitializeComponent();
	}

	public LoadViaRedFormDialog(string defaultFileName, RedirectionFile red)
		: this()
	{
		redFile = red;
		if (!string.IsNullOrEmpty(defaultFileName))
		{
			RefreshFileNamesList(defaultFileName.Substring(0, 1), showDropList: false);
			fileNameText.Text = defaultFileName;
		}
	}

	private void OnLoad(object sender, EventArgs e)
	{
		((Control)(object)this).Text = ResourceService.GetString("Clarion.LoadViaRed.Title");
		label1.Text = ResourceService.GetString("Clarion.LoadViaRed.Text");
		okButton.Text = ResourceService.GetString("Global.OKButtonText");
		cancelButton.Text = ResourceService.GetString("Global.CancelButtonText");
		int height = ((Control)this).Height;
		((Control)this).Height = height;
		originalHeight = ((Control)this).Height;
		title = ((Control)(object)this).Text;
		traceButton.Enabled = false;
		copyButton.Enabled = false;
		copyButton.Visible = false;
		buttonsHeight = traceButton.Height;
		ButtonsRefresh();
	}

	private void ListShow(bool doShow, string[] listContent)
	{
		if (doShow)
		{
			if (!traceListBox.Visible)
			{
				traceListBox.Items.Clear();
				traceListBox.Items.AddRange(listContent);
				traceListBox.Visible = true;
				copyButton.Visible = true;
				traceHeight = ((Control)this).Height;
				if (((Control)this).Height < buttonsHeight * 6)
				{
					((Control)this).Height = buttonsHeight * 6;
				}
				traceListBox.Height = ((Control)this).Height - buttonsHeight * 4 - 30;
				((Control)(object)this).MinimumSize = default(Size);
				((Control)(object)this).Refresh();
			}
			else
			{
				traceListBox.Items.Clear();
				traceListBox.Items.AddRange(listContent);
			}
		}
		else if (traceListBox.Visible)
		{
			traceListBox.Items.Clear();
			traceListBox.Visible = false;
			copyButton.Visible = false;
			((Control)this).Height = originalHeight;
			((Control)(object)this).MinimumSize = default(Size);
		}
	}

	private void ButtonsRefresh()
	{
		bool enabled = !string.IsNullOrEmpty(fileNameText.Text);
		traceButton.Enabled = enabled;
		okButton.Enabled = enabled;
		copyButton.Enabled = enabled;
		ListShow(doShow: false, null);
	}

	private void OnTrace_Clicked(object sender, EventArgs e)
	{
		List<string> list = redFile.Trace(File, RedirectionFile.CurrentDirectory);
		ListShow(doShow: true, list.ToArray());
	}

	private void OnCopy_Clicked(object sender, EventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (object item in traceListBox.Items)
		{
			stringBuilder.AppendLine(item.ToString());
		}
		ClipboardWrapper.SetDataObject((object)stringBuilder.ToString());
	}

	private void OnClosing(object sender, FormClosingEventArgs e)
	{
		if (traceHeight != -1)
		{
			((Control)this).Height = traceHeight;
		}
	}

	private void OnFileName_TextChanged(object sender, EventArgs e)
	{
		ButtonsRefresh();
		RefreshFileNamesList(fileNameText.Text, showDropList: true);
	}

	private void RefreshFileNamesList(string textToSearch, bool showDropList)
	{
		if (string.IsNullOrEmpty(textToSearch) || textToSearch.Length != 1 || !(textToSearch != lastText))
		{
			return;
		}
		lastText = textToSearch;
		string text = null;
		text = (textToSearch.Contains(".") ? textToSearch.Trim() : (textToSearch.Trim() + "*.*"));
		lastFound = 0;
		try
		{
			List<string> list = redFile.OpenNames(text, RedirectionFile.CurrentDirectory);
			if (list.Count > 0)
			{
				List<string> list2 = new List<string>();
				foreach (string item in list)
				{
					list2.Add(Path.GetFileName(item));
				}
				if (list2.Count > 0)
				{
					fileNameText.Values = list2.ToArray();
				}
				lastFound = list2.Count;
				list2.Clear();
				list.Clear();
			}
		}
		catch
		{
		}
		if (lastFound > 0 && showDropList)
		{
			fileNameText.TextChanged -= OnFileName_TextChanged;
			fileNameText.Text = "";
			SendKeys.SendWait(lastText);
			fileNameText.TextChanged += OnFileName_TextChanged;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Form)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		copyButton = new Button();
		cancelButton = new Button();
		traceListBox = new ListBox();
		okButton = new Button();
		label1 = new Label();
		traceButton = new Button();
		fileNameText = new AutoCompleteTextBox();
		((Control)this).SuspendLayout();
		copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		copyButton.Location = new Point(86, 126);
		copyButton.Margin = new Padding(4);
		copyButton.Name = "copyButton";
		copyButton.Size = new Size(133, 50);
		copyButton.TabIndex = 34;
		copyButton.Text = "&Copy";
		copyButton.UseVisualStyleBackColor = true;
		copyButton.Click += OnCopy_Clicked;
		cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		cancelButton.DialogResult = DialogResult.Cancel;
		cancelButton.Location = new Point(507, 126);
		cancelButton.Margin = new Padding(4);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new Size(133, 50);
		cancelButton.TabIndex = 30;
		cancelButton.Text = "Cancel";
		cancelButton.UseCompatibleTextRendering = true;
		traceListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		traceListBox.FormattingEnabled = true;
		traceListBox.ItemHeight = 30;
		traceListBox.Location = new Point(19, 104);
		traceListBox.Margin = new Padding(4);
		traceListBox.Name = "traceListBox";
		traceListBox.Size = new Size(623, 4);
		traceListBox.TabIndex = 33;
		okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		okButton.DialogResult = DialogResult.OK;
		okButton.Location = new Point(366, 126);
		okButton.Margin = new Padding(4);
		okButton.Name = "okButton";
		okButton.Size = new Size(133, 50);
		okButton.TabIndex = 29;
		okButton.Text = "Ok";
		okButton.UseCompatibleTextRendering = true;
		label1.AutoSize = true;
		label1.Location = new Point(17, 21);
		label1.Margin = new Padding(4, 0, 4, 0);
		label1.Name = "label1";
		label1.Size = new Size(69, 30);
		label1.TabIndex = 31;
		label1.Text = "Label:";
		label1.TextAlign = ContentAlignment.MiddleLeft;
		traceButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		traceButton.Location = new Point(226, 126);
		traceButton.Margin = new Padding(4);
		traceButton.Name = "traceButton";
		traceButton.Size = new Size(133, 50);
		traceButton.TabIndex = 32;
		traceButton.Text = "&Trace";
		traceButton.UseVisualStyleBackColor = true;
		traceButton.Click += OnTrace_Clicked;
		fileNameText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		fileNameText.Location = new Point(17, 59);
		fileNameText.Margin = new Padding(4);
		fileNameText.Name = "fileNameText";
		fileNameText.Size = new Size(623, 37);
		fileNameText.TabIndex = 28;
		fileNameText.Values = null;
		fileNameText.TextChanged += OnFileName_TextChanged;
		((Form)this).AcceptButton = okButton;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(12f, 30f);
		((ContainerControl)this).AutoScaleMode = AutoScaleMode.Font;
		((Form)this).CancelButton = cancelButton;
		((Form)this).ClientSize = new Size(668, 200);
		((Control)this).Controls.Add(copyButton);
		((Control)this).Controls.Add(cancelButton);
		((Control)this).Controls.Add(traceListBox);
		((Control)this).Controls.Add(okButton);
		((Control)this).Controls.Add(label1);
		((Control)this).Controls.Add(traceButton);
		((Control)this).Controls.Add(fileNameText);
		((Control)(object)this).Font = new Font("Segoe UI", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
		((Form)this).Margin = new Padding(4);
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)(object)this).MinimumSize = new Size(690, 200);
		((Control)this).Name = "LoadViaRedDialog";
		((Form)this).ShowIcon = false;
		((Form)this).StartPosition = FormStartPosition.CenterScreen;
		((Control)(object)this).Text = "Load Via Redirection";
		((Form)this).FormClosing += OnClosing;
		((Form)this).Load += OnLoad;
		((Control)this).ResumeLayout(performLayout: false);
		((Control)this).PerformLayout();
	}
}

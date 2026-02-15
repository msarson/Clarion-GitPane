using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.ClarionNet.CommonProperties;

namespace SoftVelocity.Common;

public class ClaGeneralEditorOptionPanel : AbstractOptionPanel
{
	private Label labelLineofcodewidth;

	private NumericUpDown entryLineofcodewidth;

	private IContainer components;

	private CheckBox ShowBlockIndentDialogCheckBox;

	private CheckBox showRedDialogCheckBox;

	public ClaGeneralEditorOptionPanel()
	{
		InitializeComponent();
	}

	public override void LoadPanelContents()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		Properties val = PropertyService.Get<Properties>("ClarionEditor", new Properties());
		ShowBlockIndentDialogCheckBox.Checked = val.Get<bool>("ShowBlockIndentDialog", true);
		showRedDialogCheckBox.Checked = val.Get<bool>("DisplayRedDialog", true);
		int num = val.Get<int>("LineOfCodeWidth", TextSplitter.DefaultLettersPerLine);
		if (num < 80)
		{
			num = 80;
		}
		entryLineofcodewidth.Value = num;
	}

	public override bool StorePanelContents()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		Properties val = PropertyService.Get<Properties>("ClarionEditor", new Properties());
		val.Set<bool>("ShowBlockIndentDialog", ShowBlockIndentDialogCheckBox.Checked);
		val.Set<bool>("DisplayRedDialog", showRedDialogCheckBox.Checked);
		int num = (int)entryLineofcodewidth.Value;
		if (num < 80)
		{
			num = 80;
		}
		else if (num > 8000)
		{
			num = 8000;
		}
		val.Set<int>("LineOfCodeWidth", num);
		TextSplitter.LettersPerLine = num;
		PropertyService.Set<Properties>("ClarionEditor", val);
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((ContainerControl)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		ShowBlockIndentDialogCheckBox = new CheckBox();
		showRedDialogCheckBox = new CheckBox();
		labelLineofcodewidth = new Label();
		entryLineofcodewidth = new NumericUpDown();
		((ISupportInitialize)entryLineofcodewidth).BeginInit();
		((Control)this).SuspendLayout();
		ShowBlockIndentDialogCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		ShowBlockIndentDialogCheckBox.Location = new Point(17, 4);
		ShowBlockIndentDialogCheckBox.Margin = new Padding(4);
		ShowBlockIndentDialogCheckBox.Name = "ShowBlockIndentDialogCheckBox";
		ShowBlockIndentDialogCheckBox.Size = new Size(573, 30);
		ShowBlockIndentDialogCheckBox.TabIndex = 7;
		ShowBlockIndentDialogCheckBox.Text = "Show Block Indent dialog for indentation of selected lines";
		ShowBlockIndentDialogCheckBox.UseVisualStyleBackColor = true;
		showRedDialogCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		showRedDialogCheckBox.Location = new Point(17, 41);
		showRedDialogCheckBox.Margin = new Padding(4);
		showRedDialogCheckBox.Name = "showRedDialogCheckBox";
		showRedDialogCheckBox.Size = new Size(573, 30);
		showRedDialogCheckBox.TabIndex = 8;
		showRedDialogCheckBox.Text = "Display Entry Dialog when opening via redirection from the context menu";
		showRedDialogCheckBox.UseVisualStyleBackColor = true;
		labelLineofcodewidth.AutoSize = true;
		labelLineofcodewidth.Location = new Point(18, 85);
		labelLineofcodewidth.Name = "labelLineofcodewidth";
		labelLineofcodewidth.Size = new Size(126, 17);
		labelLineofcodewidth.TabIndex = 9;
		labelLineofcodewidth.Text = "Line of code width:";
		entryLineofcodewidth.DecimalPlaces = 0;
		entryLineofcodewidth.Location = new Point(21, 112);
		entryLineofcodewidth.Maximum = new decimal(new int[4] { 8000, 0, 0, 0 });
		entryLineofcodewidth.Minimum = new decimal(new int[4] { 80, 0, 0, 0 });
		entryLineofcodewidth.Name = "entryLineofcodewidth";
		entryLineofcodewidth.Size = new Size(71, 22);
		entryLineofcodewidth.TabIndex = 10;
		entryLineofcodewidth.Value = new decimal(new int[4] { 80, 0, 0, 0 });
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 16f);
		((ContainerControl)this).AutoScaleMode = AutoScaleMode.Font;
		((Control)this).Controls.Add(entryLineofcodewidth);
		((Control)this).Controls.Add(labelLineofcodewidth);
		((Control)this).Controls.Add(ShowBlockIndentDialogCheckBox);
		((Control)this).Controls.Add(showRedDialogCheckBox);
		((Control)this).Margin = new Padding(4);
		((Control)this).Name = "ClaGeneralEditorOptionPanel";
		((Control)this).Size = new Size(608, 386);
		((ISupportInitialize)entryLineofcodewidth).EndInit();
		((Control)this).ResumeLayout(performLayout: false);
		((Control)this).PerformLayout();
	}
}

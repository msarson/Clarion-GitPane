using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using VisualHint.SmartPropertyGrid;

namespace SoftVelocity.Common;

public class SmartFormatterOptionPanel : AbstractOptionPanel
{
	private SmartFormatterOptions options;

	private bool hasWinAddin;

	private bool hasNetAddin;

	private PropertyGridSV propertyGrid1;

	private IContainer components;

	public SmartFormatterOptionPanel()
	{
		InitializeComponent();
		((AbstractOptionPanel)this).LoadControlDictionary(((Control)this).Controls);
	}

	public override void LoadPanelContents()
	{
		options = new SmartFormatterOptions();
		options.Initialize();
		foreach (AddIn addIn in AddInTree.AddIns)
		{
			if (addIn.Name.Equals("ClarionWindowsBinding", StringComparison.InvariantCultureIgnoreCase))
			{
				hasWinAddin = true;
			}
			if (addIn.Name.Equals("ClarionNetBinding", StringComparison.InvariantCultureIgnoreCase))
			{
				hasNetAddin = true;
			}
		}
		propertyGrid1.SelectedObject = options;
		propertyGrid1.AdjustComments(ensureVisibleSelectedProperty: false);
		propertyGrid1.AdjustLabelColumn();
		SetEnableCommentsState(options.IndentComments);
		SetEnableFormatAfterEndState(options.FormatBlockAfterEnd);
	}

	public override bool StorePanelContents()
	{
		if (options != null)
		{
			options.SaveProperties();
			options.Dispose();
			options = null;
		}
		return true;
	}

	public override bool ReceiveDialogMessage(DialogMessage message)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((int)message == 1 && options != null)
		{
			options.Dispose();
			options = null;
		}
		return ((AbstractOptionPanel)this).ReceiveDialogMessage(message);
	}

	private void PropertyChanged(object sender, VisualHint.SmartPropertyGrid.PropertyChangedEventArgs e)
	{
		if (e.PropertyEnum.Property.Name.Equals("IndentComments"))
		{
			SetEnableCommentsState((bool)e.PropertyEnum.Property.Value.GetValue());
		}
		else if (e.PropertyEnum.Property.Name.Equals("FormatBlockAfterEnd"))
		{
			SetEnableFormatAfterEndState((bool)e.PropertyEnum.Property.Value.GetValue());
		}
	}

	private void SetEnableCommentsState(bool enable)
	{
		PropertyEnumerator propertyEnumerator = propertyGrid1.FindProperty(100);
		if (propertyEnumerator != null)
		{
			propertyGrid1.EnableProperty(propertyEnumerator, enable);
		}
	}

	private void SetEnableFormatAfterEndState(bool enable)
	{
		PropertyEnumerator propertyEnumerator = propertyGrid1.FindProperty(101);
		if (propertyEnumerator != null)
		{
			propertyGrid1.EnableProperty(propertyEnumerator, enable);
		}
	}

	private void PropertyPreFilterOut(object sender, PropertyPreFilterOutEventArgs e)
	{
		if (!hasWinAddin || !hasNetAddin)
		{
			if (!hasWinAddin && e.PropertyDescriptor.Attributes[typeof(ClaWinOnlyAttribute)] != null)
			{
				e.FilterOut = PropertyPreFilterOutEventArgs.FilterModes.FilterOut;
			}
			else if (!hasNetAddin && e.PropertyDescriptor.Attributes[typeof(ClaNetOnlyAttribute)] != null)
			{
				e.FilterOut = PropertyPreFilterOutEventArgs.FilterModes.FilterOut;
			}
		}
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
		propertyGrid1 = new PropertyGridSV();
		((Control)this).SuspendLayout();
		propertyGrid1.Dock = DockStyle.Fill;
		propertyGrid1.Location = new Point(0, 0);
		propertyGrid1.Name = "propertyGrid1";
		propertyGrid1.Size = new Size(456, 314);
		propertyGrid1.TabIndex = 0;
		propertyGrid1.CommentsHeight = 60;
		propertyGrid1.SupportTabPages = false;
		propertyGrid1.VerbsVisibility = false;
		propertyGrid1.PropertyChanged += PropertyChanged;
		propertyGrid1.PropertyPreFilterOut += PropertyPreFilterOut;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((Control)(object)this).AutoSize = false;
		((ContainerControl)this).AutoScaleMode = AutoScaleMode.Font;
		((Control)this).Controls.Add(propertyGrid1);
		((Control)this).Name = "SmartFormatterOptionPanel";
		((Control)this).Size = new Size(456, 314);
		((Control)this).ResumeLayout(performLayout: false);
	}
}

using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class AbstractWindowDesignerComboBoxCommand : AbstractComboBoxCommand
{
	protected ComboBox comboBox;

	public CommonClarionDesignerView View
	{
		get
		{
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null)
			{
				return null;
			}
			return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as CommonClarionDesignerView;
		}
	}

	private FormsDesignerViewContent FormDesigner => View;

	private void RefreshComboBox()
	{
		comboBox.Items.Clear();
	}

	public virtual bool RefreshText()
	{
		return true;
	}

	protected virtual bool FillInComboBox()
	{
		return true;
	}

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
		}
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		((AbstractCommand)this).OnOwnerChanged(e);
		ToolBarComboBox val = (ToolBarComboBox)((AbstractCommand)this).Owner;
		comboBox = ((ToolStripComboBox)(object)val).ComboBox;
		comboBox.DropDownStyle = ComboBoxStyle.DropDown;
		comboBox.KeyPress += OnKeyPress;
		RefreshComboBox();
		FillInComboBox();
	}
}

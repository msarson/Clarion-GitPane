using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class AbstractWindowDesignerTextBoxCommand : AbstractTextBoxCommand
{
	protected TextBox textBox;

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

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
		}
	}

	public virtual bool RefreshText()
	{
		return true;
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		((AbstractCommand)this).OnOwnerChanged(e);
		ToolBarTextBox val = (ToolBarTextBox)((AbstractCommand)this).Owner;
		textBox = ((ToolStripTextBox)(object)val).TextBox;
		textBox.KeyPress += OnKeyPress;
		textBox.LostFocus += textBox_LostFocus;
	}

	protected virtual void textBox_LostFocus(object sender, EventArgs e)
	{
	}
}

using System;
using System.Reflection;
using System.Windows.Forms;

namespace SoftVelocity.Common.CodeCompletion;

internal class ItemsListPopupMenu : ContextMenuStrip
{
	private readonly MethodInfo processArrowMethod;

	private readonly MethodInfo getSelectedItemMethod;

	private readonly FieldInfo lastMouseItemField;

	public ItemsListPopupMenu()
	{
		processArrowMethod = typeof(ToolStripDropDown).GetMethod("ProcessArrowKey", BindingFlags.Instance | BindingFlags.NonPublic);
		getSelectedItemMethod = typeof(ToolStrip).GetMethod("GetSelectedItem", BindingFlags.Instance | BindingFlags.NonPublic);
		lastMouseItemField = typeof(ToolStrip).GetField("lastMouseActiveItem", BindingFlags.Instance | BindingFlags.NonPublic);
		AutoSize = false;
		base.AutoClose = false;
	}

	public void ProcessEnterKey()
	{
		((ToolStripItem)getSelectedItemMethod.Invoke(this, null))?.PerformClick();
	}

	public void ProcessArrowKeys(Keys keyCode)
	{
		processArrowMethod.Invoke(this, new object[1] { keyCode });
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		ToolStripItem toolStripItem = (ToolStripItem)lastMouseItemField.GetValue(this);
		base.OnMouseLeave(e);
		toolStripItem?.Select();
	}

	protected override void OnMouseMove(MouseEventArgs mea)
	{
		ToolStripItem toolStripItem = (ToolStripItem)lastMouseItemField.GetValue(this);
		bool flag = toolStripItem?.Selected ?? false;
		base.OnMouseMove(mea);
		ToolStripItem toolStripItem2 = (ToolStripItem)lastMouseItemField.GetValue(this);
		if (toolStripItem == null && toolStripItem2 == null)
		{
			return;
		}
		if (toolStripItem != null && toolStripItem2 == null)
		{
			if (flag)
			{
				toolStripItem.Select();
			}
		}
		else if (toolStripItem != null && toolStripItem2 != toolStripItem && toolStripItem2.GetType().ToString().Equals("System.Windows.Forms.ToolStripScrollButton"))
		{
			toolStripItem.Select();
		}
	}
}

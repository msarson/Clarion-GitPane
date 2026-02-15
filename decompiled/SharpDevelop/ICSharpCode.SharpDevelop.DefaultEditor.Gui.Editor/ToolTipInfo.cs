using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class ToolTipInfo
{
	private object toolTipObject;

	public string ToolTipText => toolTipObject as string;

	public Control ToolTipControl => toolTipObject as Control;

	public ToolTipInfo(string toolTipText)
	{
		toolTipObject = toolTipText;
	}

	public ToolTipInfo(Control toolTipControl)
	{
		toolTipObject = toolTipControl;
	}
}

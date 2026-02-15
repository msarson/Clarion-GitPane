using System.Drawing;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class VisualError : TextMarker
{
	private Task task;

	public Task Task => task;

	public VisualError(int offset, int length, Task task)
		: base(offset, length, TextMarkerType.WaveLine, (task.TaskType == TaskType.Error) ? Color.Red : Color.Orange)
	{
		this.task = task;
		base.ToolTip = task.Description;
	}
}

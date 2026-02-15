using System.Windows.Forms;

namespace SoftVelocity.Common.Controls;

public class NoModalWaiter
{
	private Control parent;

	private WaitPanel pan;

	public NoModalWaiter(TabPage parent)
		: this(parent, AlphaBlendType.None)
	{
	}

	public NoModalWaiter(TabPage parent, AlphaBlendType transparent)
	{
		this.parent = parent;
		pan = new WaitPanel();
		pan.AlphaBlend = transparent;
	}

	public void Open()
	{
		parent.Controls.Add(pan);
		parent.Controls.SetChildIndex(pan, 0);
		pan.Visible = true;
	}

	public void Close()
	{
		if (pan != null)
		{
			pan.Visible = false;
			parent.Controls.Remove(pan);
		}
	}
}
